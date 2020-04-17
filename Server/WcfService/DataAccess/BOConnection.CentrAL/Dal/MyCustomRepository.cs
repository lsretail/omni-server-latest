using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
{
    public class MyCustomRepository : BaseRepository
    {
        public MyCustomRepository(BOConfiguration config, Version navVersion) : base(config, navVersion)
        {
        }

        public string GetMyData(string data)
        {
            string returndata = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "SELECT [Name] FROM [" + navCompanyName + "Store$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [No_]=@no";
                    command.Parameters.AddWithValue("@no", data);

                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            returndata = SQLHelper.GetString(reader["Name"]);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return returndata;
        }
    }
}
