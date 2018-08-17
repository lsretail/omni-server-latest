using System.Data.SqlClient;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.Dal
{
    public class DisclaimerRepository : BaseRepository, IDisclaimerRepository
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public DisclaimerRepository() : base()
        {
        }

        public Disclaimer DisclaimerGetById(string code)
        {
            Disclaimer dis = null;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Personalized],[Disclaimer],[DateCreated] FROM [Disclaimer] WHERE [Code]=@id";
                    command.Parameters.AddWithValue("@id", code);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dis.DateCreated = SQLHelper.GetDateTime(reader["DateCreated"]);
                            dis.Description = SQLHelper.GetString(reader["Disclaimer"]);
                            dis.Personalized = SQLHelper.GetBool(reader["Personalized"]);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return dis;
        }
    }
}
        
 
         