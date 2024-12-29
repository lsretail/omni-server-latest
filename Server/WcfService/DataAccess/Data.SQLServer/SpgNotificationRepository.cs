using System;
using System.Data.SqlClient;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.Dal
{
    public class SpgNotificationRepository : BaseRepository, ISpgNotificationRepository
    {
        public SpgNotificationRepository(BOConfiguration config) : base(config)
        {
        }

        public void RegisterNotification(string cardId, string token, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "IF EXISTS (SELECT * FROM [SpgNotification] WHERE [CardId]=@f1) " +
                        "UPDATE [SpgNotification] SET [Token]=@f2 WHERE [CardId]=@f1 " +
                        "ELSE " +
                        "INSERT INTO [SpgNotification] ([CardId],[Token]) VALUES (@f1,@f2)";

                    command.Parameters.AddWithValue("@f1", cardId);
                    command.Parameters.AddWithValue("@f2", token);
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

            logger.StatisticEndSub(ref stat, index);
        }

        public void Delete(string cardId, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "IF EXISTS (SELECT * FROM [SpgNotification] WHERE [CardId]=@f1) " +
                                          "DELETE FROM [SpgNotification] WHERE [CardId]=@f1";

                    command.Parameters.AddWithValue("@f1", cardId);
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

            logger.StatisticEndSub(ref stat, index);
        }

        public string GetToken(string cardId, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);

            string token = null;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Token] FROM [SpgNotification] WHERE [CardId]=@f1";

                    command.Parameters.AddWithValue("@f1", cardId);
                    TraceSqlCommand(command);
                    connection.Open();
                    token = (string)command.ExecuteScalar();
                }
                connection.Close();
            }

            logger.StatisticEndSub(ref stat, index);
            return token;
        }

        public void AddLogEntry(string cardId, string message, bool hasError, string errorMsg, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO [SpgNotificationLog] ([CardId],[Message],[SentAt],[Success],[ErrorMsg]) VALUES (@f1,@f2,@f3,@f4,@f5)";

                    command.Parameters.AddWithValue("@f1", cardId);
                    command.Parameters.AddWithValue("@f2", message);
                    command.Parameters.AddWithValue("@f3", DateTime.Now);
                    command.Parameters.AddWithValue("@f4", hasError);
                    command.Parameters.AddWithValue("@f5", errorMsg);
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

            logger.StatisticEndSub(ref stat, index);
        }
    }
}
 

 
