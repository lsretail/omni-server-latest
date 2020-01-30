using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
{
    public class NotificationRepository : BaseRepository
    {
        public NotificationRepository(BOConfiguration config) : base(config)
        {
        }

        public List<Notification> NotificationsGetByContactId(string contactId, int numberOfNotifications)
        {
            List<Notification> list = new List<Notification>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + 
                         "mt.[No_],mt.[Status],mt.[Type],mt.[Code],mt.[Contact No_],mt.[Primary Text],mt.[Secondary Text],mt.[When Display],mt.[Valid From Date],mt.[Valid To Date],mt.[Created],mt.[Created by]," +
                         "nl.[Id],nl.[ContactId],nl.[DateDisplayed],nl.[DeviceId],nl.[DateClosed],nl.[ReplicationCounter],nl.[NotificationStatus] " +
                         "FROM [" + navCompanyName + "Member Notification] mt " +
                         "LEFT OUTER JOIN [" + navCompanyName + "Member Notification Log] nl ON nl.[Id]=mt.[No_] AND nl.[ContactId]=@id " +
                         "WHERE mt.[Status]=1 AND mt.[No_] IN (" +
                         "SELECT mt.[No_] FROM [" + navCompanyName + "Member Notification] mt " +
                         "INNER JOIN [" + navCompanyName + "Member Contact] c ON c.[Contact No_]=mt.[Code] " +
                         "WHERE mt.[Type]=1 AND c.[Contact No_]=@id " +
                         "UNION " +
                         "SELECT mt.[No_] FROM [" + navCompanyName + "Member Notification] mt " +
                         "INNER JOIN [" + navCompanyName + "Member Contact] c ON c.[Account No_]=mt.[Code] " +
                         "WHERE mt.[Type]=0 AND c.[Contact No_]=@id " +
                         "UNION " +
                         "SELECT mt.[No_] FROM [" + navCompanyName + "Member Notification] mt " +
                         "INNER JOIN [" + navCompanyName + "Member Scheme] s on s.[Club Code]=mt.[Code] " +
                         "INNER JOIN [" + navCompanyName + "Member Account] a on a.[Scheme Code]=s.[Code] " +
                         "INNER JOIN [" + navCompanyName + "Member Contact] c ON c.[Account No_]=a.[No_] " +
                         "WHERE mt.[Type]=2 AND c.[Contact No_]=@id " +
                         "UNION " +
                         "SELECT mt.[No_] FROM [" + navCompanyName + "Member Notification] mt " +
                         "INNER JOIN [" + navCompanyName + "Member Account] a on a.[Scheme Code]=mt.[Code] " +
                         "INNER JOIN [" + navCompanyName + "Member Contact] c ON c.[Account No_]=a.[No_] " +
                         "WHERE mt.[Type]=3 AND c.[Contact No_]=@id) " +
                         "ORDER BY mt.[Created] DESC";

                    command.Parameters.AddWithValue("@id", contactId);
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToNotification(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private Notification ReaderToNotification(SqlDataReader reader)
        {
            Notification noti = new Notification()
            {
                ContactId = SQLHelper.GetString(reader["ContactId"]),
                Created = SQLHelper.GetDateTime(reader["Valid From Date"]),
                Description = SQLHelper.GetString(reader["Primary Text"]),
                Details = SQLHelper.GetString(reader["Secondary Text"]),
                ExpiryDate = SQLHelper.GetDateTime(reader["Valid To Date"]),
                Id = SQLHelper.GetString(reader["No_"]),
                NotificationType = (NotificationType)SQLHelper.GetInt32(reader["Type"]),
                Status = (NotificationStatus)SQLHelper.GetInt32(reader["Status"])
            };

            ImageRepository imgrep = new ImageRepository(config);
            noti.Images = imgrep.ImageGetByKey("Member Notification", noti.Id, string.Empty, string.Empty, 0, false);
            return noti;
        }
    }
}
