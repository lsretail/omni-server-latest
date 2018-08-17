using System;
using System.Data.SqlClient;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class NavWSRepository : BaseRepository
    {
        private string sqlfrom = string.Empty;

        public NavWSRepository() : base()
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
                    ret = (string)command.ExecuteScalar();
                }
                connection.Close();
            }
            return ret;
        }
    }
}