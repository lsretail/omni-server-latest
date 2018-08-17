using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class StoreRepository : BaseRepository
    {
        // Key: Store No.
        const int TABLEID = 10012866;

        private string sqlcolumns = string.Empty;
        private string sqlcolumnsinv = string.Empty;
        private string sqlfrom = string.Empty;
        private string sqlfrominv = string.Empty;

        public StoreRepository(Version navVersion) : base(navVersion)
        {
            if (navVersion.Major > 10)
            {
                sqlcolumns = "mt.[No_],mt.[Name],mt.[Address],mt.[Address 2],mt.[Post Code],mt.[City],mt.[County],mt.[Country Code],mt.[Latitude],mt.[Longitude]," +
                         "mt.[Phone No_],mt.[Currency Code],mt.[Functionality Profile],mt.[Store VAT Bus_ Post_ Gr_],mt.[Click and Collect]," +
                         "(SELECT gs.[LCY Code] FROM [" + navCompanyName + "General Ledger Setup] gs) AS LCYCode";

                sqlfrom = " FROM [" + navCompanyName + "Store] mt";
            }
            else
            {
                sqlcolumns = "st.[No_],st.[Name],st.[Address],st.[Address 2],st.[Post Code],st.[City],st.[County],st.[Country Code],st.[Latitude],st.[Longitude]," +
                         "st.[Phone No_],st.[Currency Code],st.[Functionality Profile],st.[Store VAT Bus_ Post_ Gr_],st.[Click and Collect]," +
                         "(SELECT gs.[LCY Code] FROM [" + navCompanyName + "General Ledger Setup] gs) AS LCYCode";

                sqlfrom = " FROM [" + navCompanyName + "WI Store] mt INNER JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store No_]";
            }

            sqlcolumnsinv = "mt.[Store],st.[Name],st.[Address],st.[Post Code],st.[City],st.[County],st.[Country Code],st.[Latitude],st.[Longitude]," +
                            "st.[Phone No_],st.[Currency Code],st.[Functionality Profile],st.[Store VAT Bus_ Post_ Gr_],st.[Click and Collect]," +
                            "(SELECT gs.[LCY Code] FROM [" + navCompanyName + "General Ledger Setup] gs) AS LCYCode";

            sqlfrominv = " FROM [" + navCompanyName + "Inventory Terminal-Store] mt INNER JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store]";
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
                sql = "SELECT COUNT(*)";
                if (batchSize > 0)
                {
                    sql += sqlfrom + GetWhereStatement(true, keys, where, false);
                }
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, (batchSize > 0) ? keys : null, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplStore> list = new List<ReplStore>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(fullReplication, keys, where, true);

            TraceIt(sql);
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

            TraceIt(sql);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = sql;
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
                logger.Info("My store not found in setup, try to load data for " + storeId);

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
                            store = ReaderToLoyStore(reader, includeDetails, -1, -1);
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
                            stores.Add(ReaderToLoyStore(reader, true, -1, -1));
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

            if (search.Contains("'"))
                search = search.Replace("'", "''");

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
                            list.Add(ReaderToLoyStore(reader, false, -1, -1));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<Store> StoresLoyGetByCoordinates(double latitude, double longitude, double maxDistance, int maxNumberOfStores, Store.DistanceType units)
        {
            List<Store> storecheck = StoreLoyGetAll(true);

            Store.Position startpos = new Store.Position()
            {
                Latitude = latitude,
                Longitude = longitude
            };

            List<Store> stores = new List<Store>();
            foreach (Store store in storecheck)
            {
                Store.Position endpos = new Store.Position()
                {
                    Latitude = store.Latitude,
                    Longitude = store.Longitude
                };

                if (store.CalculateDistance(startpos, endpos, units) <= maxDistance)
                    stores.Add(store);
            }
            return stores;
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

        public string GetWIStoreId()
        {
            string storeid = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT [Web Store Code] FROM [{0}WI Setup]", navCompanyName);
                    TraceSqlCommand(command);
                    connection.Open();
                    storeid = command.ExecuteScalar() as string;
                    if (string.IsNullOrEmpty(storeid))
                    {
                        command.CommandText = string.Format("SELECT [Local Store No_] FROM [{0}Retail Setup]", navCompanyName);
                        TraceSqlCommand(command);
                        storeid = command.ExecuteScalar() as string;
                    }
                    connection.Close();
                }
            }
            return storeid;
        }

        public List<StoreHours> StoreHoursGetByStoreId(string storeId, int offset, int dayOfWeekOffset)
        {
            //NAV can store datetime in UTC in db but UI shows correct. But DD replicates UTC so I need to adjust
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

                            storehr.DayOfWeek = dayofweek - dayOfWeekOffset; //NAV starts with Sunday as 1 but .Net Sunday=0
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
                Latitute = SQLHelper.GetDecimal(reader["Latitude"]),
                Longitude = SQLHelper.GetDecimal(reader["Longitude"]),
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

        private Store ReaderToLoyStore(SqlDataReader reader, bool includeDetails, double latitude, double longitude)
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

            if (latitude > 0 && longitude > 0)
            {
                Store.Position startpos = new Store.Position();
                startpos.Latitude = latitude;
                startpos.Longitude = longitude;

                Store.Position endpos = new Store.Position();
                endpos.Latitude = store.Latitude;
                endpos.Longitude = store.Longitude;

                store.Distance = store.CalculateDistance(startpos, endpos, Store.DistanceType.Kilometers);
            }

            string cur = SQLHelper.GetString(reader["Currency Code"]);
            if (string.IsNullOrWhiteSpace(cur))
                store.Currency = new Currency(SQLHelper.GetString(reader["LCYCode"]));
            else
                store.Currency = new Currency(cur);

            store.StoreHours = StoreHoursGetByStoreId(store.Id, 0, 1);

            ImageRepository imgrepo = new ImageRepository();
            store.Images = imgrepo.ImageGetByKey("Store", store.Id, string.Empty, string.Empty, 0, includeDetails);
            return store;
        }
    }
} 