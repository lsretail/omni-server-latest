using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class StoreRepository : BaseRepository
    {
        // Key: Store No.
        const int TABLEID = 99001470;

        private string sqlcolumns = string.Empty;
        private string sqlcolumnsinv = string.Empty;
        private string sqlfrom = string.Empty;
        private string sqlfrominv = string.Empty;

        public StoreRepository(BOConfiguration config, Version navVersion) : base(config, navVersion)
        {
            if (navVersion.Major > 10)
            {
                sqlcolumns = "mt.[No_],mt.[Name],mt.[Address],mt.[Address 2],mt.[Post Code],mt.[City],mt.[County],mt.[Country Code],mt.[Latitude],mt.[Longitude]," +
                         "mt.[Phone No_],mt.[Currency Code],mt.[Functionality Profile],mt.[Store VAT Bus_ Post_ Gr_],mt.[Click and Collect]," +
                         "mt.[Loyalty],mt.[Web Store],mt.[Web Store POS Terminal],mt.[Web Store Staff ID]," +
                         "(SELECT gs.[LCY Code] FROM [" + navCompanyName + "General Ledger Setup] gs) AS LCYCode";

                sqlfrom = " FROM [" + navCompanyName + "Store] mt";
            }
            else
            {
                sqlcolumns = "st.[No_],st.[Name],st.[Address],st.[Address 2],st.[Post Code],st.[City],st.[County],st.[Country Code],st.[Latitude],st.[Longitude]," +
                         "st.[Phone No_],st.[Currency Code],st.[Functionality Profile],st.[Store VAT Bus_ Post_ Gr_],st.[Click and Collect]," +
                         "(SELECT gs.[LCY Code] FROM [" + navCompanyName + "General Ledger Setup] gs) AS LCYCode";

                sqlfrom = " FROM [" + navCompanyName + "WI Store] mt JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store No_]";
            }

            sqlcolumnsinv = "mt.[Store],st.[Name],st.[Address],st.[Post Code],st.[City],st.[County],st.[Country Code],st.[Latitude],st.[Longitude]," +
                            "st.[Phone No_],st.[Currency Code],st.[Functionality Profile],st.[Store VAT Bus_ Post_ Gr_],st.[Click and Collect]," +
                            "(SELECT gs.[LCY Code] FROM [" + navCompanyName + "General Ledger Setup] gs) AS LCYCode";

            sqlfrominv = " FROM [" + navCompanyName + "Inventory Terminal-Store] mt JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store]";
        }

        public List<ReplStore> ReplicateStores(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys((NavVersion.Major > 10) ? "Store" : "WI Store");

            // get records remaining
            string sql = string.Empty;
            string where = (NavVersion.Major > 10) ? " AND ([Loyalty]=1 OR [Mobile]=1)" : string.Empty;

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
            List<JscKey> keys = GetPrimaryKeys("Inventory Terminal-Store");
            List<ReplStore> list = new List<ReplStore>();

            // get records
            string sql = GetSQL(true, 0) + sqlcolumnsinv + sqlfrominv + string.Format(" WHERE mt.[Terminal No_]='{0}'", terminalId);
            bool foundmystore = false;
            string lastKey;

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
                            ReplStore store = ReaderToStore(reader, true, out lastKey);
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
                    command.CommandText = GetSQL(false, 0) + sqlcolumns + sqlfrom + ((NavVersion.Major > 10) ? " WHERE mt.[No_]=@id" : " WHERE st.[No_]=@id");
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

        public Store StoreLoyGetById(string storeId, bool includeDetails)
        {
            Store store = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {

                    command.CommandText = "SELECT " + sqlcolumns + " FROM [" + navCompanyName + ((NavVersion.Major > 10) ? "Store] mt WHERE mt.[No_]=@id" : "Store] st WHERE st.[No_]=@id");
                    command.Parameters.AddWithValue("@id", storeId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            store = ReaderToLoyStore(reader, includeDetails);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return store;
        }

        public List<Store> StoreLoyGetAll(bool clickAndCollectOnly)
        {
            List<Store> stores = new List<Store>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    if (NavVersion.Major > 10)
                    {
                        string where = (clickAndCollectOnly) ? "WHERE mt.[Click and Collect]=1" : string.Empty;
                        command.CommandText = "SELECT " + sqlcolumns + " FROM [" + navCompanyName + "Store] mt " + where + " ORDER BY mt.[Name]";
                    }
                    else
                    {
                        string where = (clickAndCollectOnly) ? "WHERE st.[Click and Collect]=1" : string.Empty;
                        command.CommandText = "SELECT " + sqlcolumns + " FROM [" + navCompanyName + "Store] st " + where + " ORDER BY st.[Name]";
                    }
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stores.Add(ReaderToLoyStore(reader, true));
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
                if (NavVersion.Major > 10)
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
                else
                {
                    if (string.IsNullOrEmpty(sqlwhere))
                    {
                        sqlwhere = string.Format(" WHERE (st.[Name] LIKE N'%{0}%' {1}", si, GetDbCICollation());
                        sqlwhere2 = string.Format(" (st.[Address] LIKE N'%{0}%' {1}", si, GetDbCICollation());
                    }
                    else
                    {
                        sqlwhere += string.Format(" OR st.[Name] LIKE N'%{0}%' {1}", si, GetDbCICollation());
                        sqlwhere2 += string.Format(" OR st.[Address] LIKE N'%{0}%' {1}", si, GetDbCICollation());
                    }
                }
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom + sqlwhere +
                        ((searchitems.Length == 1) ? ") OR" : ") AND") + sqlwhere2 + ((NavVersion.Major > 10) ? ") ORDER BY mt.[Name]" : ") ORDER BY st.[Name]");
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
                    command.CommandText = string.Format("SELECT [Picture] FROM [{0}Store] WHERE [No_]=@id", navCompanyName);
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

        public List<StoreHours> StoreHoursGetByStoreId(string storeId, int offset)
        {
            List<StoreHours> storeHourList = new List<StoreHours>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [DayOfWeek],[NameOfDay],[OpenFrom],[OpenTo] " +
                                          "FROM [" + navCompanyName + "Mobile Store Opening Hours] " +
                                          "WHERE [StoreId]=@id " +
                                          "ORDER BY [DayOfWeek]";
                    command.Parameters.AddWithValue("@id", storeId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StoreHours storehr = new StoreHours();
                            storehr.OpenFrom = SQLHelper.GetDateTime(reader["OpenFrom"]);
                            storehr.OpenTo = SQLHelper.GetDateTime(reader["OpenTo"]);

                            //something is wrong, don't take store hours that have not value
                            if (storehr.OpenFrom == DateTime.MinValue || storehr.OpenTo == DateTime.MinValue)
                                continue;

                            int dayofweek = SQLHelper.GetInt32(reader["DayOfWeek"]);
                            storehr.NameOfDay = SQLHelper.GetString(reader["NameOfDay"]);
                            storehr.StoreId = storeId;
                            storehr.Description = storehr.NameOfDay;

                            //NAV can store datetime in UTC in db but UI shows correct. But DD replicates UTC so I need to adjust
                            storehr.DayOfWeek = (dayofweek == 7) ? 0 : dayofweek;
                            storehr.OpenFrom = ConvertTo.SafeDateTime(storehr.OpenFrom.AddHours(offset));
                            storehr.OpenTo = ConvertTo.SafeDateTime(storehr.OpenTo.AddHours(offset));
                            storeHourList.Add(storehr);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return storeHourList;
        }

        public List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1)
        {
            List<ReturnPolicy> list = new List<ReturnPolicy>();
            string prcode = (NavVersion.Major < 14) ? "Product Group Code" : "Retail Product Code";

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
                        where += " AND [" + prcode + "]=@p4";
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

                    command.CommandText = "SELECT [Store No_],[Store Group Code],[Item Category Code],[" + prcode + "]," +
                                          "[Item No_],[Variant Code],[Variant Dimension 1 Code],[Refund not Allowed]," +
                                          "[Refund Period Length],[Manager Privileges],[Message 1],[Message 2] " +
                                          "FROM [" + navCompanyName + "Return Policy]" + where;
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
                Message2 = SQLHelper.GetString(reader["Message 2"])
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

                State = string.Empty,
                CultureName = string.Empty,
                DefaultCustomerAccount = string.Empty,
                UserDefaultCustomerAccount = 0,

                Currency = SQLHelper.GetString(reader["Currency Code"])
            };

            if (string.IsNullOrWhiteSpace(store.Currency))
                store.Currency = SQLHelper.GetString(reader["LCYCode"]);

            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);
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

            string cur = SQLHelper.GetString(reader["Currency Code"]);
            if (string.IsNullOrWhiteSpace(cur))
                store.Currency = new Currency(SQLHelper.GetString(reader["LCYCode"]));
            else
                store.Currency = new Currency(cur);

            if (NavVersion.Major > 10)
            {
                store.IsLoyalty = SQLHelper.GetBool(reader["Loyalty"]);
                store.IsWebStore = SQLHelper.GetBool(reader["Web Store"]);
                store.WebOmniTerminal = SQLHelper.GetString(reader["Web Store POS Terminal"]);
                store.WebOmniStaff = SQLHelper.GetString(reader["Web Store Staff ID"]);
            }

            ImageRepository imgrepo = new ImageRepository(config);
            store.Images = imgrepo.ImageGetByKey("Store", store.Id, string.Empty, string.Empty, 0, includeDetails);
            return store;
        }
    }
} 