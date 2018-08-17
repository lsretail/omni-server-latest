using System;
using System.Data.SqlClient;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using NLog;

namespace LSOmni.DataAccess.Dal
{
    public class PayRequestRepository : BaseRepository, IPayRequestRepository
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Guid NewRequest(string orderId)
        {
            Guid id = Guid.NewGuid();

            try
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO [PayRequests] ([Id],[OrderId],[RegTime]) VALUES (@f0, @f1, @f2)";
                        command.Parameters.AddWithValue("@f0", id);
                        command.Parameters.AddWithValue("@f1", orderId);
                        command.Parameters.AddWithValue("@f2", DateTime.Now);
                        TraceSqlCommand(command);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                return id;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "NewRequest for Order: " + orderId);
                throw;
            }
        }

        public bool CheckRequest(string orderId)
        {
            try
            {
                int rows = 0;
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM [PayRequests] WHERE [OrderId]=@id";
                        command.Parameters.AddWithValue("@id", orderId);
                        TraceSqlCommand(command);
                        connection.Open();
                        rows = (int)command.ExecuteScalar();
                    }
                    connection.Close();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Guid: " + orderId);
                throw;
            }
        }
    }
}
