﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase;

namespace LSOmni.DataAccess.BOConnection.CentralExt.Dal
{
    public class CustomerRepository : BaseRepository
    {
        // Key: No.
        const int TABLEID = 18;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public CustomerRepository(BOConfiguration config, Version version) : base(config, version)
        {
            sqlcolumns = "mt.[No_],mt.[Name],mt.[Address],mt.[Address 2],mt2.[LSC House_Apartment No_$5ecfc871-5d82-43f1-9c54-59685e82318d],mt.[City],mt.[Post Code],mt.[County],mt.[Country_Region Code],mt.[Territory Code]," +
                         "mt.[E-Mail],mt.[Home Page],mt.[Phone No_],mt.[Mobile Phone No_],mt.[Currency Code],mt.[VAT Bus_ Posting Group],mt.[Blocked],mt.[Prices Including VAT]," +
                         "mt.[Customer Price Group],mt.[Customer Disc_ Group],mt.[Payment Terms Code],mt.[Location Code]";

            sqlfrom = " FROM [" + navCompanyName + "Customer$437dbf0e-84ff-417a-965d-ed2bb9650972] mt" +
                      " JOIN [" + navCompanyName + "Customer$437dbf0e-84ff-417a-965d-ed2bb9650972$ext] mt2 ON mt2.[No_]=mt.[No_]";
        }

        public List<ReplCustomer> ReplicateCustomer(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Customer$437dbf0e-84ff-417a-965d-ed2bb9650972");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplCustomer> list = new List<ReplCustomer>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(fullReplication, keys, true);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = sql;

                    if (fullReplication)
                    {
                        JscActions act = new JscActions(lastKey);
                        SetWhereValues(command, act, keys, true, true);
                        TraceSqlCommand(command);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            int cnt = 0;
                            while (reader.Read())
                            {
                                list.Add(ReaderToCustomer(reader, out lastKey));
                                cnt++;
                            }
                            reader.Close();
                            recordsRemaining -= cnt;
                        }
                        if (recordsRemaining <= 0)
                            lastKey = maxKey;   // this should be the highest PreAction id;
                    }
                    else
                    {
                        bool first = true;
                        foreach (JscActions act in actions)
                        {
                            if (act.Type == DDStatementType.Delete)
                            {
                                list.Add(new ReplCustomer()
                                {
                                    AccountNumber = act.ParamValue,
                                    IsDeleted = true
                                });
                                continue;
                            }

                            if (SetWhereValues(command, act, keys, first) == false)
                                continue;

                            TraceSqlCommand(command);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    list.Add(ReaderToCustomer(reader, out string ts));
                                }
                                reader.Close();
                            }
                            first = false;
                        }
                        if (string.IsNullOrEmpty(maxKey))
                            maxKey = lastKey;
                    }
                    connection.Close();
                }
            }

            // just in case something goes too far
            if (recordsRemaining < 0)
                recordsRemaining = 0;

            return list;
        }

        public List<ReplCustomer> ReplicateCustomerTM(int batchSize, bool fullReplication, ref string lastKey, ref int recordsRemaining)
        {
            ProcessLastKey(lastKey, out string mainKey, out string delKey);
            List<JscKey> keys = GetPrimaryKeys("Customer$437dbf0e-84ff-417a-965d-ed2bb9650972");

            // get records remaining
            string sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            recordsRemaining = GetRecordCountTM(mainKey, sql, keys);

            List<JscActions> actions = LoadDeleteActions(fullReplication, TABLEID, "Customer$437dbf0e-84ff-417a-965d-ed2bb9650972", keys, batchSize, ref delKey);
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(true, keys, true);

            List<ReplCustomer> list = new List<ReplCustomer>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = sql;

                    JscActions actKey = new JscActions(mainKey);
                    SetWhereValues(command, actKey, keys, true, true);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int cnt = 0;
                        while (reader.Read())
                        {
                            list.Add(ReaderToCustomer(reader, out mainKey));
                            cnt++;
                        }
                        reader.Close();
                        recordsRemaining -= cnt;
                    }

                    foreach (JscActions act in actions)
                    {
                        list.Add(new ReplCustomer()
                        {
                            AccountNumber = act.ParamValue,
                            IsDeleted = true
                        });
                    }
                    connection.Close();
                }
            }

            // just in case something goes too far
            if (recordsRemaining < 0)
                recordsRemaining = 0;

            lastKey = $"R={mainKey};D={delKey};";
            return list;
        }

        private ReplCustomer ReaderToCustomer(SqlDataReader reader, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplCustomer()
            {
                AccountNumber = SQLHelper.GetString(reader["No_"]),
                Name = SQLHelper.GetString(reader["Name"]),
                Street = SQLHelper.GetString(reader["Address"]),
                City = SQLHelper.GetString(reader["City"]),
                ZipCode = SQLHelper.GetString(reader["Post Code"]),
                Country = SQLHelper.GetString(reader["Country_Region Code"]),
                County = SQLHelper.GetString(reader["County"]),
                State = SQLHelper.GetString(reader["Territory Code"]),
                URL = SQLHelper.GetString(reader["Home Page"]),
                Email = SQLHelper.GetString(reader["E-Mail"]),
                CellularPhone = SQLHelper.GetString(reader["Mobile Phone No_"]),
                PhoneLocal = SQLHelper.GetString(reader["Phone No_"]),
                Currency = SQLHelper.GetString(reader["Currency Code"]),
                TaxGroup = SQLHelper.GetString(reader["VAT Bus_ Posting Group"]),
                PriceGroup = SQLHelper.GetString(reader["Customer Price Group"]),
                DiscountGroup = SQLHelper.GetString(reader["Customer Disc_ Group"]),
                PaymentTerms = SQLHelper.GetString(reader["Payment Terms Code"]),
                ShippingLocation = SQLHelper.GetString(reader["Location Code"]),
                Blocked = SQLHelper.GetInt32(reader["Blocked"]),
                IncludeTax = SQLHelper.GetInt32(reader["Prices Including VAT"])
            };
        }

        private Customer ReaderToPosCustomer(SqlDataReader reader)
        {
            return new Customer()
            {
                Id = SQLHelper.GetString(reader["No_"]),
                Name = SQLHelper.GetString(reader["Name"]),
                Url = SQLHelper.GetString(reader["Home Page"]),
                Email = SQLHelper.GetString(reader["E-Mail"]),
                TaxGroup = SQLHelper.GetString(reader["VAT Bus_ Posting Group"]),
                PriceGroup = SQLHelper.GetString(reader["Customer Price Group"]),
                DiscountGroup = SQLHelper.GetString(reader["Customer Disc_ Group"]),
                PaymentTerms = SQLHelper.GetString(reader["Payment Terms Code"]),
                ShippingLocation = SQLHelper.GetString(reader["Location Code"]),
                IsBlocked = SQLHelper.GetBool(reader["Blocked"]),
                InclTax = SQLHelper.GetInt32(reader["Prices Including VAT"]),

                Currency = new UnknownCurrency(SQLHelper.GetString(reader["Currency Code"])),

                Address = new Address()
                {
                    Address1 = SQLHelper.GetString(reader["Address"]),
                    Address2 = SQLHelper.GetString(reader["Address 2"]),
                    HouseNo = SQLHelper.GetString(reader["LSC House_Apartment No_$5ecfc871-5d82-43f1-9c54-59685e82318d"]),
                    City = SQLHelper.GetString(reader["City"]),
                    PostCode = SQLHelper.GetString(reader["Post Code"]),
                    Country = SQLHelper.GetString(reader["Country_Region Code"]),
                    County = SQLHelper.GetString(reader["County"]),
                    StateProvinceRegion = SQLHelper.GetString(reader["Territory Code"]),
                    CellPhoneNumber = SQLHelper.GetString(reader["Mobile Phone No_"]),
                    PhoneNumber = SQLHelper.GetString(reader["Phone No_"])
                }
            };
        }

        public List<Customer> CustomerSearch(CustomerSearchType searchType, string search, int maxNumberOfRowsReturned)
        {
            if (maxNumberOfRowsReturned < 1)
                maxNumberOfRowsReturned = 1;

            List<Customer> list = new List<Customer>();
            string where = string.Empty;
            string order = string.Empty;

            switch (searchType)
            {
                case CustomerSearchType.CustomerId:
                    where = string.Format("mt.[No_]='{0}'", search);
                    order = "mt.[No_]";
                    break;

                case CustomerSearchType.Email:
                    where = string.Format("mt.[E-Mail] LIKE '{0}%' {1}", search, GetDbCICollation());
                    order = "mt.[E-Mail]";
                    break;

                case CustomerSearchType.Name:
                    where = string.Format("mt.[Search Name] LIKE '{0}%' {1}", search, GetDbCICollation());
                    order = "mt.[Search Name]";
                    break;

                case CustomerSearchType.PhoneNumber:
                    where = string.Format("(mt.[Phone No_] LIKE '{0}%' OR mt.[Mobile Phone No_] LIKE '{0}%')", search);
                    order = "mt.[Phone No_]";
                    break;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT TOP({0}) {1}{2} WHERE {3} ORDER BY {4}", maxNumberOfRowsReturned, sqlcolumns, sqlfrom, where, order);
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToPosCustomer(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }

            TraceIt(string.Format("Number of Customers returned: {0} ", list.Count));
            return list;
        }
    }
}
 