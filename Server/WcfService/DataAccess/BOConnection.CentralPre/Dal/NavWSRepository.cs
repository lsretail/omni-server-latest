using System.Data.SqlClient;

using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class NavWSRepository : BaseRepository
    {
        private string sqlfrom = string.Empty;

        public NavWSRepository(BOConfiguration config) : base(config)
        {
            sqlfrom = " FROM [" + navCompanyName + "WS Request] mt";
        }

        public bool DoesWebServiceExist(string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId))
                return false;

            int cnt = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) " + sqlfrom + " WHERE mt.[Request ID]=@id";
                    command.Parameters.AddWithValue("@id", requestId);
                    TraceSqlCommand(command);
                    cnt = (int)command.ExecuteScalar();
                }
                connection.Close();
            }
            return (cnt > 0);
        }

        public bool DoesWSTableExist()
        {
            int cnt = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + navCompanyName + "WS Request'";
                    TraceSqlCommand(command);
                    cnt = (int)command.ExecuteScalar();
                }
                connection.Close();
            }
            return (cnt > 0);
        }

        public string GetURLByRequestId(string requestId)
        {
            string ret = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Definition Url] FROM [" + navCompanyName + "Web Request] mt WHERE mt.[Request Id]=@id";
                    command.Parameters.AddWithValue("@id", requestId);
                    TraceSqlCommand(command);
                    ret = (string)command.ExecuteScalar();
                }
                connection.Close();
            }
            return ret;
        }
    }
}