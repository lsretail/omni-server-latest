using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class ShippingRepository : BaseRepository
    {
        public ShippingRepository(BOConfiguration config, Version version) : base(config, version)
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
                                          " FROM [" + navCompanyName + "Shipping Agent$437dbf0e-84ff-417a-965d-ed2bb9650972] mt";
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
                                                      " FROM [" + navCompanyName + "Shipping Agent Services$437dbf0e-84ff-417a-965d-ed2bb9650972] mt" +
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
                    if (LSCVersion >= new Version("19.0"))
                    {
                        command.CommandText = "SELECT mt.[Code],mt.[Name],mt2.[LSC Web Store Customer No_],cu.[VAT Bus_ Posting Group] " +
                                              "FROM [" + navCompanyName + "Country_Region$437dbf0e-84ff-417a-965d-ed2bb9650972] mt " +
                                              "INNER JOIN [" + navCompanyName + "Country_Region$5ecfc871-5d82-43f1-9c54-59685e82318d] mt2 ON mt2.[Code]=mt.[Code] " +
                                              "LEFT OUTER JOIN [" + navCompanyName + "Customer$437dbf0e-84ff-417a-965d-ed2bb9650972] cu ON cu.[No_]=mt2.[LSC Web Store Customer No_]";
                    }
                    else
                    {
                        command.CommandText = "SELECT [Code],[Name] FROM [" + navCompanyName + "Country_Region$437dbf0e-84ff-417a-965d-ed2bb9650972]";
                    }
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToCountryCode(reader));
                        }
                    }
                }
                connection.Close();
            }
            return list;
        }

        private List<TaxItemGroup> GetTaxItemGroups(string code)
        {
            List<TaxItemGroup> list = new List<TaxItemGroup>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[VAT Prod_ Posting Group],mt.[VAT _] " +
                                          "FROM [" + navCompanyName + "VAT Posting Setup$437dbf0e-84ff-417a-965d-ed2bb9650972] mt " +
                                          "WHERE mt.[VAT Bus_ Posting Group]=@code";
                    command.Parameters.AddWithValue("@code", code);
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToTaxItemGroup(reader));
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
                ShippingTime = GetNAVDateFormula(SQLHelper.GetString(reader["Shipping Time"]))
            };
        }

        private ReplCountryCode ReaderToCountryCode(SqlDataReader reader)
        {
            ReplCountryCode code = new ReplCountryCode()
            {
                Code = SQLHelper.GetString(reader["Code"]),
                Name = SQLHelper.GetString(reader["Name"])
            };

            if (LSCVersion >= new Version("19.0"))
            {
                code.CustomerNo = SQLHelper.GetString(reader["LSC Web Store Customer No_"]);
                code.TaxPostGroup = SQLHelper.GetString(reader["VAT Bus_ Posting Group"]);

                code.TaxItemGroups = GetTaxItemGroups(code.TaxPostGroup);
            }
            return code;
        }

        private TaxItemGroup ReaderToTaxItemGroup(SqlDataReader reader)
        {
            return new TaxItemGroup()
            {
                Code = SQLHelper.GetString(reader["VAT Prod_ Posting Group"]),
                TaxPercent = SQLHelper.GetDecimal(reader["VAT _"])
            };
        }
    }
}