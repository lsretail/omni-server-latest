using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.DataAccess.Dal
{
    public class PushNotificationRepository : BaseRepository, IPushNotificationRepository
    {
        private const int maxRetryCounter = 3;
        private const int dateCreatedTimeBuffer = 48;

        public PushNotificationRepository(BOConfiguration config) : base(config)
        {
        }

        public void Save(PushNotificationRequest request)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "IF EXISTS (SELECT * FROM [DeviceSecurity] WHERE [DeviceId]=@f1) " +
                        "UPDATE [DeviceSecurity] SET [FcmToken]=@f2 WHERE [DeviceId]=@f1";
                    
                    command.Parameters.AddWithValue("@f1", request.DeviceId);
                    if (request.Status == PushStatus.Enabled && string.IsNullOrEmpty(request.Id) == false)
                    {
                        command.Parameters.AddWithValue("@f2", request.Id);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@f2", DBNull.Value);
                    }
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void SavePushNotification(string contactId, string notificationId)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "IF NOT EXISTS (SELECT * FROM [PushNotification] WHERE [ContactId]=@f1 AND [NotificationId]=@f2) " +
                        "INSERT INTO [PushNotification] ([ContactId],[NotificationId]) VALUES (@f1,@f2)";

                    command.Parameters.AddWithValue("@f1", contactId);
                    command.Parameters.AddWithValue("@f2", notificationId);
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void Delete(string deviceId)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "IF EXISTS (SELECT * FROM [DeviceSecurity] WHERE [DeviceId]=@f1) " +
                                          "UPDATE [DeviceSecurity] SET [FcmToken]=@f2 WHERE [DeviceId]=@f1";

                    command.Parameters.AddWithValue("@f1", deviceId);
                    command.Parameters.AddWithValue("@f2", DBNull.Value);
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void UpdateToSent(string contactId, string notificationId)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "UPDATE [PushNotification] SET [DateSent]=@f0 WHERE [ContactId]=@f1 AND [NotificationId]=@f2";

                    command.Parameters.AddWithValue("@f0", DateTime.Now);
                    command.Parameters.AddWithValue("@f1", contactId);
                    command.Parameters.AddWithValue("@f2", notificationId);
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void UpdateCounter(string contactId, string notificationId)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "UPDATE [PushNotification] SET [RetryCounter]=[RetryCounter]+1 WHERE [ContactId]=@f0 AND [NotificationId]=@f1";

                    command.Parameters.AddWithValue("@f0", contactId);
                    command.Parameters.AddWithValue("@f1", notificationId);
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public List<PushNotification> PushOutNotificationGetNext(DateTime dateCreated, int numberOfNotifications = 100)
        {
            dateCreated = dateCreated.AddHours(-dateCreatedTimeBuffer); //dateCreated is only used for index and performance

            List<PushNotification> list = new List<PushNotification>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    //status=0 are NEW, 
                    command.CommandText = 
                        "SELECT TOP(@f0) p.[ContactId],p.[NotificationId],p.[DateCreated],n.[PrimaryText],n.[SecondaryText],n.[Status], n.[DateLastModified] " +
                        "FROM [PushNotification] p " +
                        "INNER JOIN [Notification] n on p.[NotificationId]=n.[Id] " +
                        "WHERE [DateCreated]>@f1 AND [DateSent] IS NULL AND n.[Status]=0 " +
                        "AND [RetryCounter]<@f2 ORDER BY [DateCreated] ASC ";

                    command.Parameters.AddWithValue("@f0", numberOfNotifications);
                    command.Parameters.AddWithValue("@f1", dateCreated);
                    command.Parameters.AddWithValue("@f2", maxRetryCounter);
                    TraceSqlCommand(command);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PushNotification push = new PushNotification()
                            {
                                NotificationId = SQLHelper.GetString(reader["NotificationId"]),
                                CreatedDate = SQLHelper.GetDateTime(reader["DateCreated"]),
                                Title = SQLHelper.GetString(reader["PrimaryText"]),
                                Body = SQLHelper.GetString(reader["SecondaryText"]),
                                Status = (PushStatus)SQLHelper.GetInt32(reader["Status"]),
                                LastModifiedDate = SQLHelper.GetDateTime(reader["DateLastModified"]),
                                ContactId = SQLHelper.GetString(reader["ContactId"]),
                                Platform = PushPlatform.Unknown
                            };
                            push.DeviceIds = GetFcmTokens(push.ContactId);
                            list.Add(push);
                        }
                    }
                }
                connection.Close();
            }
            
            return list;
        }

        private List<string> GetFcmTokens(string contactId)
        {
            List<string> list = new List<string>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    //status=0 are NEW, 
                    command.CommandText =
                        "SELECT [FcmToken] FROM [DeviceSecurity] WHERE [ContactId]=@f0 AND [FcmToken] is not null";

                    command.Parameters.AddWithValue("@f0", contactId);
                    TraceSqlCommand(command);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(SQLHelper.GetString(reader["FcmToken"]));
                        }
                    }
                }
                connection.Close();
            }
            return list;
        }

        public bool DoesNewPushOutNotificationExist(DateTime dateLastChecked)
        {
            dateLastChecked = dateLastChecked.AddHours(-dateCreatedTimeBuffer); //dateCreated is only used for index and performance
            int cnt = 0;

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM [PushNotification] WHERE [DateCreated] > @f0 AND [DateSent] IS null";
                    command.Parameters.AddWithValue("@f0", dateLastChecked);
                    TraceSqlCommand(command);
                    connection.Open();
                    cnt = (int)command.ExecuteScalar();
                }
                connection.Close();
            }
            return (cnt > 0);
        }

    }
}
 

 
