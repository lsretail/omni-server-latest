using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class StoreRepository : BaseRepository
    {
        // Key: Store No.
        const int TABLEID = 99001470;

        private string sqlcolumns = string.Empty;
        private string sqlcolumnsinv = string.Empty;
        private string sqlfrom = string.Empty;
        private string sqlfrominv = string.Empty;

        public StoreRepository(BOConfiguration config, Version version) : base(config, version)
        {
            sqlcolumns = "mt.[No_],mt.[Name],mt.[Address],mt.[Address 2],mt.[Post Code],mt.[City],mt.[County],mt.[Country Code],mt.[Latitude],mt.[Longitude]," +
                        "mt.[Phone No_],mt.[Currency Code],mt.[Functionality Profile],mt.[Store VAT Bus_ Post_ Gr_],mt.[Click and Collect]," +
                        "mt.[Loyalty],mt.[Web Store],mt.[Web Store POS Terminal],mt.[Web Store Staff ID],tr.[Sales Type Filter]," +
                        "(SELECT gs.[LCY Code] FROM [" + navCompanyName + "General Ledger Setup$437dbf0e-84ff-417a-965d-ed2bb9650972] gs) AS LCYCode";

            if (LSCVersion >= new Version("18.4"))
            {
                sqlcolumns += ",mt.[Calc Inv for Sourcing Location]";
            }

            sqlfrom = " FROM [" + navCompanyName + "LSC Store$5ecfc871-5d82-43f1-9c54-59685e82318d] mt" +
                      " LEFT JOIN [" + navCompanyName + "LSC POS Terminal$5ecfc871-5d82-43f1-9c54-59685e82318d] tr ON tr.[No_]=mt.[Web Store POS Terminal]";

            sqlcolumnsinv = "mt.[Store],st.[Name],st.[Address],st.[Post Code],st.[City],st.[County],st.[Country Code],st.[Latitude],st.[Longitude]," +
                            "st.[Phone No_],st.[Currency Code],st.[Functionality Profile],st.[Store VAT Bus_ Post_ Gr_],st.[Click and Collect],'' AS [Sales Type Filter]," +
                            "(SELECT gs.[LCY Code] FROM [" + navCompanyName + "General Ledger Setup$437dbf0e-84ff-417a-965d-ed2bb9650972] gs) AS LCYCode";

            sqlfrominv = " FROM [" + navCompanyName + "LSC Inventory Terminal-Store$5ecfc871-5d82-43f1-9c54-59685e82318d] mt JOIN [" + navCompanyName + "LSC Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=mt.[Store]";
        }

        public List<ReplStore> ReplicateStores(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Store$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // get records remaining
            string sql = string.Empty;
            string where = " AND ([Loyalty]=1 OR [Mobile]=1)";

            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, where, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplStore> list = new List<ReplStore>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(fullReplication, keys, where, true);

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
                                list.Add(ReaderToStore(reader, false, out lastKey));
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
                                list.Add(new ReplStore()
                                {
                                    Id = act.ParamValue,
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
                                    list.Add(ReaderToStore(reader, false, out string ts));
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

        public List<ReplStore> ReplicateInvStores(string storeId, string terminalId)
        {
            List<JscKey> keys = GetPrimaryKeys("LSC Inventory Terminal-Store$5ecfc871-5d82-43f1-9c54-59685e82318d");
            List<ReplStore> list = new List<ReplStore>();

            // get records
            string sql = GetSQL(true, 0) + sqlcolumnsinv + sqlfrominv + string.Format(" WHERE mt.[Terminal No_]='{0}'", terminalId);
            bool foundmystore = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = sql;
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ReplStore store = ReaderToStore(reader, true, out string lastKey);
                            list.Add(store);
                            if (store.Id.Equals(storeId))
                                foundmystore = true;
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }

            if (foundmystore == false)
            {
                logger.Warn(config.LSKey.Key, "My store not found in setup, try to load data for " + storeId);

                ReplStore st = StoreGetById(storeId);
                if (st != null)
                    list.Insert(0, st);
            }

            return list;
        }

        public ReplStore StoreGetById(string storeId)
        {
            ReplStore store = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetSQL(false, 0) + sqlcolumns + sqlfrom + " WHERE mt.[No_]=@id";
                    command.Parameters.AddWithValue("@id", storeId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            store = ReaderToStore(reader, false, out string ts);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return store;
        }

        public Store StoreLoyGetById(string storeId, bool inclDetails)
        {
            Store store = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {

                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom +
                                          " WHERE mt.[No_]=@id";
                    command.Parameters.AddWithValue("@id", storeId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            store = ReaderToLoyStore(reader, inclDetails);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return store;
        }

        public List<Store> StoreLoyGetAll(StoreGetType storeType, bool inclDetails)
        {
            List<Store> stores = new List<Store>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    string type = string.Empty;
                    switch (storeType)
                    {
                        case StoreGetType.ClickAndCollect:
                            type = " WHERE mt.[Click and Collect]=1";
                            break;
                        case StoreGetType.WebStore:
                            type = " WHERE mt.[Web Store]=1";
                            break;
                    }

                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom + type +
                                          " ORDER BY mt.[Name]";
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stores.Add(ReaderToLoyStore(reader, inclDetails));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return stores;
        }

        public List<Store> StoreLoySearch(string search)
        {
            List<Store> list = new List<Store>();
            if (string.IsNullOrWhiteSpace(search))
                return list;

            SQLHelper.CheckForSQLInjection(search);

            char[] sep = new char[] { ' ' };
            string[] searchitems = search.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            string sqlwhere = string.Empty;
            string sqlwhere2 = string.Empty;
            foreach (string si in searchitems)
            {
                if (string.IsNullOrEmpty(sqlwhere))
                {
                    sqlwhere = string.Format(" WHERE (mt.[Name] LIKE N'%{0}%' {1}", si, GetDbCICollation());
                    sqlwhere2 = string.Format(" (mt.[Address] LIKE N'%{0}%' {1}", si, GetDbCICollation());
                }
                else
                {
                    sqlwhere += string.Format(" OR mt.[Name] LIKE N'%{0}%' {1}", si, GetDbCICollation());
                    sqlwhere2 += string.Format(" OR mt.[Address] LIKE N'%{0}%' {1}", si, GetDbCICollation());
                }
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom +
                                          sqlwhere + ((searchitems.Length == 1) ? ") OR" : ") AND") + sqlwhere2 + ") ORDER BY mt.[Name]";
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyStore(reader, false));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public ImageView StoreImageGetById(string storeId)
        {
            ImageView view = new ImageView();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT [Picture] FROM [{0}LSC Store$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [No_]=@id", navCompanyName);
                    command.Parameters.AddWithValue("@id", storeId);
                    TraceSqlCommand(command);

                    connection.Open();
                    view.Id = storeId;
                    view.ImgBytes = command.ExecuteScalar() as byte[];
                    connection.Close();
                }
            }
            return view;
        }

        private List<SalesType> GetSalesTypes(string saleTypeFilter)
        {
            List<SalesType> list = new List<SalesType>();
            string[] st = saleTypeFilter.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "SELECT [Description] FROM [" + navCompanyName + "LSC Sales Type$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [Code]=@id";
                    command.Parameters.Add("@id", SqlDbType.NVarChar);
                    TraceSqlCommand(command);

                    foreach (string s in st)
                    {
                        command.Parameters["@id"].Value = s;
                        string desc = command.ExecuteScalar() as string;
                        if (desc != null)
                        {
                            list.Add(new SalesType()
                            {
                                Code = s,
                                Description = desc
                            });
                        }
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private List<SourcingLocation> GetSourcingLocation(string storeId)
        {
            List<SourcingLocation> list = new List<SourcingLocation>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Sourcing Location],[Priority],[Will Ship Orders],[Orders can be Collected],[Description] " +
                        "FROM [" + navCompanyName + "LSC CO Sourcing Locations$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [Delivery Store]=@id";
                    command.Parameters.AddWithValue("@id", storeId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToSrcLoc(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1)
        {
            List<ReturnPolicy> list = new List<ReturnPolicy>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    string where = string.Empty;
                    if (string.IsNullOrEmpty(storeId) == false)
                    {
                        where += " AND [Store No_]=@p1";
                        command.Parameters.AddWithValue("@p1", storeId);
                    }
                    if (string.IsNullOrEmpty(storeGroupCode) == false)
                    {
                        where += " AND [Store Group Code]=@p2";
                        command.Parameters.AddWithValue("@p2", storeGroupCode);
                    }
                    if (string.IsNullOrEmpty(itemCategory) == false)
                    {
                        where += " AND [Item Category Code]=@p3";
                        command.Parameters.AddWithValue("@p3", itemCategory);
                    }
                    if (string.IsNullOrEmpty(productGroup) == false)
                    {
                        where += " AND [Retail Product Code]=@p4";
                        command.Parameters.AddWithValue("@p4", productGroup);
                    }
                    if (string.IsNullOrEmpty(itemId) == false)
                    {
                        where += " AND [Item No_]=@p5";
                        command.Parameters.AddWithValue("@p5", itemId);
                    }
                    if (string.IsNullOrEmpty(variantCode) == false)
                    {
                        where += " AND [Variant Code]=@p6";
                        command.Parameters.AddWithValue("@p6", variantCode);
                    }
                    if (string.IsNullOrEmpty(variantDim1) == false)
                    {
                        where += " AND [Variant Dimension 1 Code]=@p7";
                        command.Parameters.AddWithValue("@p7", variantDim1);
                    }

                    if (string.IsNullOrEmpty(where) == false)
                    {
                        where = " WHERE" + where.Substring(4, where.Length - 4);
                    }

                    command.CommandText = "SELECT [Store No_],[Store Group Code],[Item Category Code],[Retail Product Code]," +
                                          "[Item No_],[Variant Code],[Variant Dimension 1 Code],[Refund not Allowed]," +
                                          "[Refund Period Length],[Manager Privileges],[Message 1],[Message 2],[Return Policy HTML] " +
                                          "FROM [" + navCompanyName + "LSC Return Policy$5ecfc871-5d82-43f1-9c54-59685e82318d]" + where;
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToPolicy(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private string GetStoreGroups(string storeId)
        {
            string codes = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Store Group] FROM [" + navCompanyName + "LSC Store Group Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [Store Code]=@id";
                    command.Parameters.AddWithValue("@id", storeId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            codes += SQLHelper.GetString(reader["Store Group"]) + ";";
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return codes;
        }

        private string GetPriceGroups(string storeId)
        {
            string codes = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Price Group Code] FROM [" + navCompanyName + "LSC Store Price Group$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [Store]=@id";
                    command.Parameters.AddWithValue("@id", storeId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            codes += SQLHelper.GetString(reader["Price Group Code"]) + ";";
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return codes;
        }

        private ReturnPolicy ReaderToPolicy(SqlDataReader reader)
        {
            return new ReturnPolicy()
            {
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                StoreGroup = SQLHelper.GetString(reader["Store Group Code"]),
                ItemCategory = SQLHelper.GetString(reader["Item Category Code"]),
                ProductGroup = SQLHelper.GetString(reader["Retail Product Code"]),
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                VariantCode = SQLHelper.GetString(reader["Variant Code"]),
                VariantDimension1 = SQLHelper.GetString(reader["Variant Dimension 1 Code"]),
                RefundNotAllowed = SQLHelper.GetBool(reader["Refund not Allowed"]),
                ManagerPrivileges = SQLHelper.GetBool(reader["Manager Privileges"]),
                RefundPeriodLength = SQLHelper.GetString(reader["Refund Period Length"]),
                Message1 = SQLHelper.GetString(reader["Message 1"]),
                Message2 = SQLHelper.GetString(reader["Message 2"]),
                ReturnPolicyHTML = SQLHelper.GetString(reader["Return Policy HTML"])
            };
        }

        private SourcingLocation ReaderToSrcLoc(SqlDataReader reader)
        {
            return new SourcingLocation()
            {
                Id = SQLHelper.GetString(reader["Sourcing Location"]),
                Priority = SQLHelper.GetInt32(reader["Priority"]),
                CanShip = SQLHelper.GetBool(reader["Will Ship Orders"]),
                CanCollect = SQLHelper.GetBool(reader["Orders can be Collected"]),
                Description = SQLHelper.GetString(reader["Description"])
            };
        }

        private ReplStore ReaderToStore(SqlDataReader reader, bool invmode, out string timestamp)
        {
            ReplStore store = new ReplStore()
            {
                Id = (invmode) ? SQLHelper.GetString(reader["Store"]) : SQLHelper.GetString(reader["No_"]),
                Name = SQLHelper.GetString(reader["Name"]),
                Street = SQLHelper.GetString(reader["Address"]),
                ZipCode = SQLHelper.GetString(reader["Post Code"]),
                City = SQLHelper.GetString(reader["City"]),
                County = SQLHelper.GetString(reader["County"]),
                Country = SQLHelper.GetString(reader["Country Code"]),
                Phone = SQLHelper.GetString(reader["Phone No_"]),
                FunctionalityProfile = SQLHelper.GetString(reader["Functionality Profile"]),
                TaxGroup = SQLHelper.GetString(reader["Store VAT Bus_ Post_ Gr_"]),
                Latitute = SQLHelper.GetDecimal(reader, "Latitude"),
                Longitude = SQLHelper.GetDecimal(reader, "Longitude"),
                ClickAndCollect = SQLHelper.GetBool(reader["Click and Collect"]),
                HospSalesTypes = SQLHelper.GetString(reader["Sales Type Filter"]),

                State = string.Empty,
                CultureName = string.Empty,
                DefaultCustomerAccount = string.Empty,
                UserDefaultCustomerAccount = 0,

                Currency = SQLHelper.GetString(reader["Currency Code"])
            };

            store.StoreGroupCodes = GetStoreGroups(store.Id);
            store.PriceGroupCodes = GetPriceGroups(store.Id);
            if (string.IsNullOrWhiteSpace(store.Currency))
                store.Currency = SQLHelper.GetString(reader["LCYCode"]);

            if (LSCVersion >= new Version("18.4") && invmode == false)
            {
                store.UseSourcingLocation = SQLHelper.GetBool(reader["Calc Inv for Sourcing Location"]);
            }

            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);
            return store;
        }

        private Store ReaderToLoyStore(SqlDataReader reader, bool includeDetails)
        {
            Store store = new Store()
            {
                Id = SQLHelper.GetString(reader["No_"]),
                Description = SQLHelper.GetString(reader["Name"]),
                Phone = SQLHelper.GetString(reader["Phone No_"]),
                Latitude = SQLHelper.GetDouble(reader["Latitude"]),
                Longitude = SQLHelper.GetDouble(reader["Longitude"]),
                IsClickAndCollect = SQLHelper.GetBool(reader["Click and Collect"]),
                IsLoyalty = SQLHelper.GetBool(reader["Loyalty"]),
                IsWebStore = SQLHelper.GetBool(reader["Web Store"]),
                WebOmniTerminal = SQLHelper.GetString(reader["Web Store POS Terminal"]),
                WebOmniStaff = SQLHelper.GetString(reader["Web Store Staff ID"]),
                HospSalesTypes = GetSalesTypes(SQLHelper.GetString(reader["Sales Type Filter"])),

                Address = new Address()
                {
                    Address1 = SQLHelper.GetString(reader["Address"]),
                    Address2 = SQLHelper.GetString(reader["Address 2"]),
                    PostCode = SQLHelper.GetString(reader["Post Code"]),
                    City = SQLHelper.GetString(reader["City"]),
                    StateProvinceRegion = SQLHelper.GetString(reader["County"]),
                    Country = SQLHelper.GetString(reader["Country Code"]),
                    Type = AddressType.Store
                }
            };

            store.StoreGroupCodes = GetStoreGroups(store.Id);
            store.PriceGroupCodes = GetPriceGroups(store.Id);

            string cur = SQLHelper.GetString(reader["Currency Code"]);
            if (string.IsNullOrWhiteSpace(cur))
                store.Currency = new Currency(SQLHelper.GetString(reader["LCYCode"]));
            else
                store.Currency = new Currency(cur);

            if (LSCVersion >= new Version("18.4"))
            {
                store.UseSourcingLocation = SQLHelper.GetBool(reader["Calc Inv for Sourcing Location"]);
            }

            if (includeDetails)
            {
                store.SourcingLocations = GetSourcingLocation(SQLHelper.GetString(reader["No_"]));
            }
            return store;
        }
    }
} 