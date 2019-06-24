using System;
using System.Data.SqlClient;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.Dal
{
    public class PayRequestRepository : BaseRepository, IPayRequestRepository
    {
        public PayRequestRepository(BOConfiguration config) : base(config)
        {
        }

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
                        command.Parameters.AddWithValue("@f0", id.ToString());
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
                logger.Error(config.LSKey.Key, ex, "NewRequest for Order: " + orderId);
                throw;
            }
        }
    }
}
