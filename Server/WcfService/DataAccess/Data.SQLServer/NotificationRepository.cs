using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.Dal
{
    public class NotificationRepository : BaseRepository, INotificationRepository
    {
        private string notificationSql =
            "SELECT n.[Id],n.[Type],n.[TypeCode],n.[PrimaryText],n.[SecondaryText],n.[DisplayFrequency],n.[ValidFrom],n.[ValidTo]," +
            "n.[Created],n.[LastModifiedDate],n.[CreatedBy],n.[DateLastModified],n.[QRText],n.[NotificationType]," +
            "n.[Status] " +
            "FROM [Notification] AS n ";

        public NotificationRepository(BOConfiguration config) : base(config)
        {
        }

        //string sql = ((System.Data.Objects.ObjectQuery)query).ToTraceString();
        public Notification NotificationGetById(string id)
        {
            SQLHelper.CheckForSQLInjection(id);
            string where = string.Format("WHERE n.[Id]='{0}'", id);

            List<Notification> list = NotificationsGetList(where);
            return (list.Count > 0 ? list[0] : null); // return null if nothing found!
        }

        public List<Notification> NotificationsGetByContactId(string contactId, int numberOfNotifications)
        {
            return NotificationsGetList("WHERE [TypeCode]='" + contactId + "'");
        }

        private List<Notification> NotificationsGetByStoredProcs(string sql, string contactId)
        {
            List<Notification> list = new List<Notification>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToNotification(reader, contactId));
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return list;
        }

        public void NotificationsUpdateStatus(List<string> notificationIds, NotificationStatus notificationStatus)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlTransaction trans = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (string noteId in notificationIds)
                        {
                            //id+contactid is PK
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "IF EXISTS (SELECT * FROM [Notification] WHERE [Id]=@id) " +
                                                      "UPDATE [Notification] SET [Status]=@f1 WHERE [Id]=@id";

                                command.Parameters.AddWithValue("@id", noteId);
                                command.Parameters.AddWithValue("@f1", (int)notificationStatus);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public List<Notification> NotificationSearch(string contactId, string search, int maxNumberOfLists)
        {
            if (string.IsNullOrWhiteSpace(search))
                search = "";
            if (search.Contains("'"))
                search = search.Replace("'", "''");

            //OfferSearch must return same columns as OfferView
            string sql = string.Format("EXEC NotificationSearch @contactId='{0}', @search='{1}', @top='{2}'", contactId, search, maxNumberOfLists);
            return NotificationsGetByStoredProcs(sql, contactId);
        }

        public void Save(string contactId, List<Notification> notifications)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlTransaction trans = connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.Transaction = trans;
                            command.CommandText = "DELETE FROM [Notification] WHERE [TypeCode]=@id AND [NotificationType]=0";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@id", contactId);
                            TraceSqlCommand(command);
                            command.ExecuteNonQuery();

                            command.CommandText = "IF NOT EXISTS (SELECT * FROM [Notification] WHERE [Id]=@f0 AND [TypeCode]=@f2)" +
                                "INSERT INTO [Notification] ([Id],[Type],[TypeCode],[PrimaryText],[SecondaryText],[DisplayFrequency],[ValidFrom]," +
                                "[ValidTo],[Created],[CreatedBy],[LastModifiedDate],[DateLastModified],[QRText],[NotificationType],[Status]) " +
                                "VALUES (@f0,@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8,@f9,@f10,@f11,@f12,@f13,@f14)";

                            command.Parameters.Clear();
                            command.Parameters.Add("@f0", SqlDbType.NVarChar);
                            command.Parameters.Add("@f1", SqlDbType.Int);
                            command.Parameters.Add("@f2", SqlDbType.NVarChar);
                            command.Parameters.Add("@f3", SqlDbType.NVarChar);
                            command.Parameters.Add("@f4", SqlDbType.NVarChar);
                            command.Parameters.Add("@f5", SqlDbType.Int);
                            command.Parameters.Add("@f6", SqlDbType.DateTime);
                            command.Parameters.Add("@f7", SqlDbType.DateTime);
                            command.Parameters.Add("@f8", SqlDbType.DateTime);
                            command.Parameters.Add("@f9", SqlDbType.NVarChar);
                            command.Parameters.Add("@f10", SqlDbType.DateTime);
                            command.Parameters.Add("@f11", SqlDbType.DateTime);
                            command.Parameters.Add("@f12", SqlDbType.NVarChar);
                            command.Parameters.Add("@f13", SqlDbType.Int);
                            command.Parameters.Add("@f14", SqlDbType.Int);

                            foreach (Notification notification in notifications)
                            {
                                command.Parameters["@f0"].Value = notification.Id;
                                command.Parameters["@f1"].Value = 1; //   0=Account,1=Contact,2=Club,3=Scheme   
                                command.Parameters["@f2"].Value = contactId;
                                command.Parameters["@f3"].Value = notification.Description;
                                command.Parameters["@f4"].Value = NullToString(notification.Details);
                                command.Parameters["@f5"].Value = 0;
                                command.Parameters["@f6"].Value = notification.Created; //using this for now
                                command.Parameters["@f7"].Value = (DateTime)notification.ExpiryDate;
                                command.Parameters["@f8"].Value = DateTime.Now;
                                command.Parameters["@f9"].Value = string.Empty;
                                command.Parameters["@f10"].Value = DateTime.Now;
                                command.Parameters["@f11"].Value = DateTime.Now;
                                command.Parameters["@f12"].Value = NullToString(notification.QRText);
                                command.Parameters["@f13"].Value = (int)notification.NotificationType;
                                command.Parameters["@f14"].Value = (int) notification.Status;

                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
                connection.Close();
            }
        }

        public void OrderMessageNotificationSave(string notificationId, long orderMessageId, string contactId, string description, string details, string qrText)
        {
            //for notification table, Type should be = 1 (means contact) and TypeCode = contactId
            int typeOfContact = 1;
            string typeCodeContact = contactId;
            using (SqlConnection db = new SqlConnection(sqlConnectionString))
            {
                db.Open();
                using (SqlTransaction trans = db.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand command = db.CreateCommand())
                        {
                            command.Transaction = trans;

                            command.CommandText = "INSERT INTO [Notification] ([Id],[Type],[TypeCode],[PrimaryText],[SecondaryText],[DisplayFrequency]," +
                                                  "[ValidFrom],[ValidTo],[Created],[CreatedBy],[LastModifiedDate],[DateLastModified],[QRText],[NotificationType],[Status]" +
                                                  ") VALUES (@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8,@f9,@f10,@f11,@f12,@f13,@f14,@f15)";

                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@f1", notificationId);
                            command.Parameters.AddWithValue("@f2", typeOfContact);
                            command.Parameters.AddWithValue("@f3", typeCodeContact);
                            command.Parameters.AddWithValue("@f4", NullToString(description));
                            command.Parameters.AddWithValue("@f5", NullToString(details));
                            command.Parameters.AddWithValue("@f6", 0);
                            command.Parameters.AddWithValue("@f7", DateTime.Now.AddMonths(-1));
                            command.Parameters.AddWithValue("@f8", DateTime.Now.AddDays(2));
                            command.Parameters.AddWithValue("@f9", DateTime.Now);
                            command.Parameters.AddWithValue("@f10", string.Empty);
                            command.Parameters.AddWithValue("@f11", DateTime.Now);
                            command.Parameters.AddWithValue("@f12", DateTime.Now);
                            command.Parameters.AddWithValue("@f13", NullToString(qrText));
                            command.Parameters.AddWithValue("@f14", 1); //0 = BO, 1 = OrderMessage 
                            command.Parameters.AddWithValue("@f15", 0); //Status 0 = New
                            TraceSqlCommand(command);
                            command.ExecuteNonQuery();

                            trans.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(config.LSKey.Key, ex);
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        db.Close();
                    }
                }
            }
        }

        private List<Notification> NotificationsGetList(string where)
        {
            List<Notification> list = new List<Notification>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = notificationSql + where;
                    TraceSqlCommand(command);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToNotification(reader, ""));
                        }
                    }
                }
                connection.Close();
            }
            return list;
        }

        private Notification ReaderToNotification(SqlDataReader reader, string contactId)
        {
            ImageRepository imageRepository = new ImageRepository(config);

            DateTime created = SQLHelper.GetDateTime(reader["Created"]);
            Notification notification = new Notification()
            {
                Id = SQLHelper.GetString(reader["Id"]),
                Description = SQLHelper.GetString(reader["PrimaryText"]),
                Details = SQLHelper.GetString(reader["SecondaryText"]),
                Created = (created == null ? DateTime.Now : created),
                ContactId = contactId,
                DateLastModified = SQLHelper.GetDateTime(reader["DateLastModified"]),
                QRText = SQLHelper.GetString(reader["QRText"]),
                NotificationType = (NotificationType)SQLHelper.GetInt32(reader["NotificationType"]),
                ExpiryDate = SQLHelper.GetDateTime(reader["ValidTo"]),
                Status = (NotificationStatus)SQLHelper.GetInt32(reader["Status"])
            };

            if (notification.NotificationType == NotificationType.OrderMessage)
                notification.NotificationTextType = NotificationTextType.Html;
            else
                notification.NotificationTextType = NotificationTextType.Plain;


            if (notification.ExpiryDate != null && notification.ExpiryDate.Value.Hour == 0
                && notification.ExpiryDate.Value.Minute == 0 && notification.ExpiryDate.Value.Second == 0)
            {
                notification.ExpiryDate = notification.ExpiryDate.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            notification.Images = imageRepository.NotificationImagesById(notification.Id);

            return notification;
        }
    }
}
 
