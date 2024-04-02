using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class ShippingRepository : BaseRepository
    {
        public ShippingRepository(BOConfiguration config) : base(config)
        {
        }

        public List<ReplShippingAgent> ReplicateShipping(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<ReplShippingAgent> list = new List<ReplShippingAgent>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Code],mt.[Name],mt.[Internet Address],mt.[Account No_]" +
                                          " FROM [" + navCompanyName + "Shipping Agent] mt";
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ReplShippingAgent agent = ReaderToShippingAgent(reader);
                            list.Add(agent);

                            using (SqlCommand command2 = connection.CreateCommand())
                            {
                                command2.CommandText = "SELECT mt.[Code],mt.[Description],mt.[Shipping Time]" +
                                                      " FROM [" + navCompanyName + "Shipping Agent Services] mt" +
                                                      " WHERE [Shipping Agent Code]=@code";

                                command2.Parameters.AddWithValue("@code", agent.Id);
                                using (SqlDataReader reader2 = command2.ExecuteReader())
                                {
                                    while (reader2.Read())
                                    {
                                        agent.Services.Add(ReaderToShippingAgentService(reader2));
                                    }
                                }
                            }
                        }
                    }
                }
                connection.Close();
            }

            // we always replicate everything here
            maxKey = string.Empty;
            lastKey = string.Empty;
            recordsRemaining = 0;

            return list;
        }

        public List<ReplCountryCode> ReplicateCountryCode()
        {
            List<ReplCountryCode> list = new List<ReplCountryCode>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Code],[Name] FROM [" + navCompanyName + "Country_Region]";
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ReplCountryCode()
                            {
                                Code = SQLHelper.GetString(reader["Code"]),
                                Name = SQLHelper.GetString(reader["Name"])
                            });
                        }
                    }
                }
                connection.Close();
            }
            return list;
        }

        private ReplShippingAgent ReaderToShippingAgent(SqlDataReader reader)
        {
            return new ReplShippingAgent(SQLHelper.GetString(reader["Code"]))
            {
                Name = SQLHelper.GetString(reader["Name"]),
                InternetAddress = SQLHelper.GetString(reader["Internet Address"]),
                AccountNo = SQLHelper.GetString(reader["Account No_"])
            };
        }

        private ShippingAgentService ReaderToShippingAgentService(SqlDataReader reader)
        {
            return new ShippingAgentService(SQLHelper.GetString(reader["Code"]))
            {
                Description = SQLHelper.GetString(reader["Description"]),
                ShippingTime = SQLHelper.GetDateFormula(reader["Shipping Time"])
            };
        }
    }
}