using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
{
    public class ItemRepository : BaseRepository
    {
        // Key: No.
        const int TABLEID = 27;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public ItemRepository(BOConfiguration config, Version navVersion) : base(config, navVersion)
        {
            sqlcolumns = "mt.[No_],mt.[Blocked],mt.[Description],mt2.[Keying in Price],mt2.[Keying in Quantity],mt2.[No Discount Allowed]," +
                         "mt2.[Scale Item],mt.[VAT Prod_ Posting Group],mt.[Base Unit of Measure],mt2.[Zero Price Valid],mt.[Sales Unit of Measure]," +
                         "mt.[Purch_ Unit of Measure],mt.[Vendor No_],mt.[Vendor Item No_],mt.[Unit Price],mt2.[Retail Product Code]," +
                         "mt.[Gross Weight],mt2.[Season Code],mt.[Item Category Code],mt2.[Item Family Code],mt.[Units per Parcel],mt.[Unit Volume],ih.[Html]," +
                         "(SELECT TOP(1) sl.[Block Sale on POS] FROM [" + navCompanyName + "Item Status Link$5ecfc871-5d82-43f1-9c54-59685e82318d] sl " +
                         "WHERE sl.[Item No_]=mt.[No_] AND sl.[Starting Date]<GETDATE() AND sl.[Block Sale on POS]=1) AS BlockOnPos, " +
                         "(SELECT TOP(1) sl.[Block Discount] FROM [" + navCompanyName + "Item Status Link$5ecfc871-5d82-43f1-9c54-59685e82318d] sl " +
                         "WHERE sl.[Item No_]=mt.[No_] AND sl.[Starting Date]<GETDATE() AND sl.[Block Discount]=1) AS BlockDiscount, " +
                         "(SELECT TOP(1) sl.[Block Manual Price Change] FROM [" + navCompanyName + "Item Status Link$5ecfc871-5d82-43f1-9c54-59685e82318d] sl " +
                         "WHERE sl.[Item No_]=mt.[No_] AND sl.[Starting Date]<GETDATE() AND sl.[Block Manual Price Change]=1) AS BlockPrice, " +
                         "(SELECT TOP(1) sl.[Block Negative Adjustment] FROM [" + navCompanyName + "Item Status Link$5ecfc871-5d82-43f1-9c54-59685e82318d] sl " +
                         "WHERE sl.[Item No_]=mt.[No_] AND sl.[Starting Date]<GETDATE() AND sl.[Block Negative Adjustment]=1) AS BlockNegAdj, " +
                         "(SELECT TOP(1) sl.[Block Positive Adjustment] FROM [" + navCompanyName + "Item Status Link$5ecfc871-5d82-43f1-9c54-59685e82318d] sl " +
                         "WHERE sl.[Item No_]=mt.[No_] AND sl.[Starting Date]<GETDATE() AND sl.[Block Positive Adjustment]=1) AS BlockPosAdj, " +
                         "(SELECT TOP(1) sl.[Block Purchase Return] FROM [" + navCompanyName + "Item Status Link$5ecfc871-5d82-43f1-9c54-59685e82318d] sl " +
                         "WHERE sl.[Item No_]=mt.[No_] AND sl.[Starting Date]<GETDATE() AND sl.[Block Purchase Return]=1) AS BlockPurRet";

            if (navVersion > new Version("16.2.0.0"))
            {
                sqlcolumns += ",(SELECT TOP(1) sl.[Blocked on eCommerce] FROM [" + navCompanyName + "Item Status Link$5ecfc871-5d82-43f1-9c54-59685e82318d] sl " +
                              "WHERE sl.[Item No_]=mt.[No_] AND sl.[Starting Date]<GETDATE() AND sl.[Blocked on eCommerce]=1) AS BlockEcom";
            }

            sqlfrom = " FROM [" + navCompanyName + "Item$437dbf0e-84ff-417a-965d-ed2bb9650972] mt " +
                      "INNER JOIN [" + navCompanyName + "Item$5ecfc871-5d82-43f1-9c54-59685e82318d] mt2 ON mt2.[No_]=mt.[No_] " +
                      "LEFT OUTER JOIN [" + navCompanyName + "Item HTML$5ecfc871-5d82-43f1-9c54-59685e82318d] ih ON mt.[No_]=ih.[Item No_]";
        }

        public List<ReplItem> ReplicateItems(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            string col = sqlcolumns + ",(SELECT TOP(1) id.[Status] FROM [" + navCompanyName + "Store Group Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] sg " +
                         "LEFT JOIN [" + navCompanyName + "Item Distribution$5ecfc871-5d82-43f1-9c54-59685e82318d] id ON id.[Code]=sg.[Store Group] " +
                         "WHERE sg.[Store Code]='" + storeId + "' AND id.[Item No_]=mt.[No_] ORDER BY sg.[Level] DESC) AS DistStatus";

            List<JscKey> keys = GetPrimaryKeys("Item$437dbf0e-84ff-417a-965d-ed2bb9650972");
            List<JscActions> actions = new List<JscActions>();
            string prevLastKey = lastKey;

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[No_]", storeId, false);
                recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);
            }
            else
            {
                string tmplastkey = lastKey;
                string mainlastkey = lastKey;
                recordsRemaining = 0;

                recordsRemaining = GetRecordCount(TABLEID, lastKey, string.Empty, keys, ref maxKey);
                actions = LoadActions(fullReplication, TABLEID, batchSize, ref mainlastkey, ref recordsRemaining);

                // get item HTML
                recordsRemaining += GetRecordCount(10001411, tmplastkey, string.Empty, keys, ref maxKey);
                List<JscActions> itemact = LoadActions(fullReplication, 10001411, batchSize, ref tmplastkey, ref recordsRemaining);
                if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                    mainlastkey = tmplastkey;

                // get item status
                recordsRemaining += GetRecordCount(10001404, tmplastkey, string.Empty, keys, ref maxKey);
                itemact.AddRange(LoadActions(fullReplication, 10001404, batchSize, ref tmplastkey, ref recordsRemaining));
                if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                    mainlastkey = tmplastkey;

                // get distribution changes 
                tmplastkey = lastKey;
                recordsRemaining += GetRecordCount(10000704, tmplastkey, string.Empty, keys, ref maxKey);
                itemact.AddRange(LoadActions(fullReplication, 10000704, batchSize, ref tmplastkey, ref recordsRemaining));
                if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                    mainlastkey = tmplastkey;

                lastKey = mainlastkey;

                foreach (JscActions act in itemact)
                {
                    string[] parvalues = act.ParamValue.Split(';');
                    JscActions newact;

                    if (act.TableId == 10000704)
                    {
                        newact = new JscActions()
                        {
                            id = act.id,
                            TableId = act.TableId,
                            Type = act.Type,
                            ParamValue = (parvalues.Length > 2) ? parvalues[2] : act.ParamValue
                        };
                    }
                    else
                    {
                        if (act.Type == DDStatementType.Delete)
                            continue;       // skip delete actions for extra tables

                        newact = new JscActions()
                        {
                            id = act.id,
                            TableId = act.TableId,
                            Type = act.Type,
                            ParamValue = (parvalues.Length == 1) ? act.ParamValue : parvalues[0]
                        };
                    }

                    JscActions findme = actions.Find(x => x.ParamValue.Equals(newact.ParamValue));
                    if (findme == null)
                    {
                        actions.Add(newact);
                    }
                }
            }

            List<ReplItem> list = new List<ReplItem>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + col + sqlfrom + GetWhereStatementWithStoreDist(fullReplication, keys, "mt.[No_]", storeId, true, false);

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
                                list.Add(ReaderToItem(reader, out lastKey));
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
                                list.Add(new ReplItem()
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
                                    list.Add(ReaderToItem(reader, out string ts));
                                }
                                reader.Close();
                            }
                            first = false;
                        }

                        if (actions.Count == 0)
                        {
                            lastKey = prevLastKey;
                            maxKey = prevLastKey;
                        }

                        if (string.IsNullOrEmpty(maxKey) || (Convert.ToInt32(lastKey) > Convert.ToInt32(maxKey)))
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

        public List<LoyItem> ReplicateEcommFullItems(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            string col = sqlcolumns + ",(SELECT TOP(1) id.[Status] FROM [" + navCompanyName + "Store Group Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] sg " +
                         "LEFT JOIN [" + navCompanyName + "Item Distribution$5ecfc871-5d82-43f1-9c54-59685e82318d] id ON id.[Code]=sg.[Store Group] " +
                         "WHERE sg.[Store Code]='" + storeId + "' AND id.[Item No_]=mt.[No_] ORDER BY sg.[Level] DESC) AS DistStatus";

            List<JscKey> keys = GetPrimaryKeys("Item$437dbf0e-84ff-417a-965d-ed2bb9650972");
            List<JscActions> actions = new List<JscActions>();
            string prevLastKey = lastKey;

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[No_]", storeId, false);
                recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);
            }
            else
            {
                string tmplastkey = lastKey;
                string mainlastkey = lastKey;
                recordsRemaining = 0;

                // get main item actions
                recordsRemaining = GetRecordCount(TABLEID, lastKey, string.Empty, keys, ref maxKey);
                actions = LoadActions(fullReplication, TABLEID, batchSize, ref mainlastkey, ref recordsRemaining);

                // Check for actions from Sales Price
                recordsRemaining += GetRecordCount(7002, tmplastkey, string.Empty, keys, ref maxKey);
                List<JscActions> itemact = LoadActions(fullReplication, 7002, batchSize, ref tmplastkey, ref recordsRemaining);
                if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                    mainlastkey = tmplastkey;

                // Check for actions from Item Variant
                tmplastkey = lastKey;
                recordsRemaining += GetRecordCount(5401, tmplastkey, string.Empty, keys, ref maxKey);
                itemact.AddRange(LoadActions(fullReplication, 5401, batchSize, ref tmplastkey, ref recordsRemaining));
                if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                    mainlastkey = tmplastkey;

                // Check for actions from Item Unit of Measure
                tmplastkey = lastKey;
                recordsRemaining += GetRecordCount(5404, tmplastkey, string.Empty, keys, ref maxKey);
                itemact.AddRange(LoadActions(fullReplication, 5404, batchSize, ref tmplastkey, ref recordsRemaining));
                if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                    mainlastkey = tmplastkey;

                // Check for actions from Item Variant Registration
                tmplastkey = lastKey;
                recordsRemaining += GetRecordCount(10001414, tmplastkey, string.Empty, keys, ref maxKey);
                itemact.AddRange(LoadActions(fullReplication, 10001414, batchSize, ref tmplastkey, ref recordsRemaining));
                if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                    mainlastkey = tmplastkey;

                // Check for actions from Item HTML
                tmplastkey = lastKey;
                recordsRemaining += GetRecordCount(10001411, tmplastkey, string.Empty, keys, ref maxKey);
                itemact.AddRange(LoadActions(fullReplication, 10001411, batchSize, ref tmplastkey, ref recordsRemaining));
                if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                    mainlastkey = tmplastkey;

                // get item status
                recordsRemaining += GetRecordCount(10001404, tmplastkey, string.Empty, keys, ref maxKey);
                itemact.AddRange(LoadActions(fullReplication, 10001404, batchSize, ref tmplastkey, ref recordsRemaining));
                if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                    mainlastkey = tmplastkey;

                // Check for actions from Distribution tables
                tmplastkey = lastKey;
                recordsRemaining += GetRecordCount(10000704, tmplastkey, string.Empty, keys, ref maxKey);
                itemact.AddRange(LoadActions(fullReplication, 10000704, batchSize, ref tmplastkey, ref recordsRemaining));

                lastKey = mainlastkey;

                // combine actions
                foreach (JscActions act in itemact)
                {
                    string[] parvalues = act.ParamValue.Split(';');
                    JscActions newact;

                    if (act.TableId == 10000704)
                    {
                        newact = new JscActions()
                        {
                            id = act.id,
                            TableId = act.TableId,
                            Type = act.Type,
                            ParamValue = (parvalues.Length > 2) ? parvalues[2] : act.ParamValue
                        };
                    }
                    else
                    {
                        if (act.Type == DDStatementType.Delete)
                            continue;       // skip delete actions for extra tables

                        newact = new JscActions()
                        {
                            id = act.id,
                            TableId = act.TableId,
                            Type = act.Type,
                            ParamValue = (parvalues.Length == 1) ? act.ParamValue : parvalues[0]
                        };
                    }

                    JscActions findme = actions.Find(x => x.ParamValue.Equals(newact.ParamValue));
                    if (findme == null)
                    {
                        actions.Add(newact);
                    }
                }
            }

            List<LoyItem> list = new List<LoyItem>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + col + sqlfrom + GetWhereStatementWithStoreDist(fullReplication, keys, "mt.[No_]", storeId, true, false);

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
                                LoyItem val = ReaderToLoyItem(reader, storeId , string.Empty, true, true, out lastKey);
                                list.Add(val);
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
                                list.Add(new LoyItem()
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
                                    list.Add(ReaderToLoyItem(reader, storeId, string.Empty, true, true, out string ts));
                                }
                                reader.Close();
                            }
                            first = false;
                        }

                        if (actions.Count == 0)
                        {
                            lastKey = prevLastKey;
                            maxKey = prevLastKey;
                        }

                        if (string.IsNullOrEmpty(maxKey) || (Convert.ToInt32(lastKey) > Convert.ToInt32(maxKey)))
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

        public List<LoyItem> ItemsGetByProductGroupId(string productGroupId, string culture, bool includeDetails = true)
        {
            List<LoyItem> list = new List<LoyItem>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom + " WHERE [Retail Product Code]=@id";
                    command.Parameters.AddWithValue("@id", productGroupId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyItem(reader, string.Empty, culture, includeDetails, false, out string ts));
                        }
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, string storeId, bool includeDetails = false)
        {
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize < 1)
                pageSize = 1;

            SQLHelper.CheckForSQLInjection(itemCategoryId);
            SQLHelper.CheckForSQLInjection(productGroupId);
            SQLHelper.CheckForSQLInjection(search);


            string sql =
            "WITH o AS (SELECT TOP(" + pageSize * pageNumber + ") mt.[No_],mt.[Description],mt.[Sales Unit of Measure]," +
            "mt2.[Retail Product Code],mt2.[Scale Item]," +
            "mt.[Blocked],mt.[Gross Weight],mt2.[Season Code],mt.[Item Category Code],mt2.[Item Family Code],mt.[Units per Parcel],mt.[Unit Volume],ih.[Html]," +
            " ROW_NUMBER() OVER(ORDER BY mt.[Description]) AS RowNumber, " +
            "(SELECT TOP(1) sl.[Block Sale on POS] FROM [" + navCompanyName + "Item Status Link$5ecfc871-5d82-43f1-9c54-59685e82318d] sl " +
            "WHERE sl.[Item No_]=mt.[No_] AND [Starting Date]<GETDATE() AND sl.[Block Sale on POS]=1) AS BlockOnPos, " +
            "(SELECT TOP(1) sl.[Block Discount] FROM [" + navCompanyName + "Item Status Link$5ecfc871-5d82-43f1-9c54-59685e82318d] sl " +
            "WHERE sl.[Item No_]=mt.[No_] AND sl.[Starting Date]<GETDATE() AND sl.[Block Discount]=1) AS BlockDiscount, " +
            "(SELECT TOP(1) sl.[Block Manual Price Change] FROM [" + navCompanyName + "Item Status Link$5ecfc871-5d82-43f1-9c54-59685e82318d] sl " +
            "WHERE sl.[Item No_]=mt.[No_] AND sl.[Starting Date]<GETDATE() AND sl.[Block Manual Price Change]=1) AS BlockPrice " +
            sqlfrom +
            " LEFT OUTER JOIN [" + navCompanyName + "Retail Product Group$5ecfc871-5d82-43f1-9c54-59685e82318d] pg ON pg.[Code]=mt2.[Retail Product Code]" +
            " WHERE (1=1)";

            if (string.IsNullOrWhiteSpace(itemCategoryId) == false)
                sql += " AND pg.[Item Category Code]='" + itemCategoryId + "'";
            if (string.IsNullOrWhiteSpace(productGroupId) == false)
                sql += " AND pg.[Code]='" + productGroupId + "'";
            if (string.IsNullOrWhiteSpace(search) == false)
                sql += " AND mt.[Description] LIKE '%" + search + "%'" + GetDbCICollation();

            sql += GetSQLStoreDist("mt.[No_]", storeId, true);
            sql += ") SELECT [No_],[Description],[Sales Unit of Measure],[Html],[RowNumber],[BlockOnPos],";
            sql += "[Retail Product Code],[Scale Item],";
            sql += "[Blocked],[Gross Weight],[Season Code],[Item Category Code],[Item Family Code],[Units per Parcel],";
            sql += "[Unit Volume],[BlockDiscount],[BlockPrice]" +
                  " FROM o WHERE RowNumber BETWEEN " + ((pageNumber - 1) * pageSize + 1) +
                  " AND " + (((pageNumber - 1) * pageSize) + pageSize) + " ORDER BY RowNumber";

            List<LoyItem> list = new List<LoyItem>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyItem(reader, string.Empty, string.Empty, includeDetails, false, out string ts));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public LoyItem ItemLoyGetById(string id, string storeId, string culture, bool includeDetails)
        {
            LoyItem item = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom +
                        " WHERE mt.[No_]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            item = ReaderToLoyItem(reader, storeId, culture, includeDetails, false, out string ts);
                        }
                    }
                    connection.Close();
                }
            }
            return item;
        }

        public LoyItem ItemLoyGetByBarcode(string code, string storeId, string culture)
        {
            BarcodeRepository brepo = new BarcodeRepository(config);
            Barcode bcode = brepo.BarcodeGetByCode(code);
            if (bcode == null)  // barcode not found
                return null;

            LoyItem item = ItemLoyGetById(bcode.ItemId, storeId, string.Empty, true);

            item.Prices.Clear();
            PriceRepository prepo = new PriceRepository(config, NavVersion);
            Price price = prepo.PriceGetByIds(bcode.ItemId, storeId, bcode.VariantId, bcode.UnitOfMeasureId, culture);
            if (price == null)
            {
                price = prepo.PriceGetByIds(bcode.ItemId, storeId, bcode.VariantId, string.Empty, culture);
                if (price == null)
                {
                    price = prepo.PriceGetByIds(bcode.ItemId, storeId, string.Empty, string.Empty, culture);
                }
            }
            if (price != null)
                item.Prices.Add(price);

            if (string.IsNullOrWhiteSpace(bcode.VariantId) == false)
            {
                item.VariantsRegistration.Clear();
                ItemVariantRegistrationRepository vreop = new ItemVariantRegistrationRepository(config, NavVersion);
                VariantRegistration variantReg = vreop.VariantRegGetById(bcode.VariantId, item.Id);
                if (variantReg != null)
                    item.VariantsRegistration.Add(variantReg);
            }

            if (string.IsNullOrWhiteSpace(bcode.UnitOfMeasureId) == false)
            {
                ItemUOMRepository ireop = new ItemUOMRepository(config);
                item.UnitOfMeasures.Clear();
                item.UnitOfMeasures.Add(new UnitOfMeasure()
                {
                    Id = bcode.UnitOfMeasureId,
                    ItemId = bcode.ItemId,
                    QtyPerUom = ireop.ItemUOMGetByIds(bcode.ItemId, bcode.UnitOfMeasureId).QtyPerUom
                });
            }
            return item;
        }

        public string ItemDetailsGetById(string itemId)
        {
            string detail = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Html] FROM [" + navCompanyName + "Item HTML$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [Item No_]=@id";
                    command.Parameters.AddWithValue("@id", itemId);
                    TraceSqlCommand(command);
                    connection.Open();
                    detail = command.ExecuteScalar() as string;
                    connection.Close();
                }
            }
            return detail;
        }

        public List<LoyItem> ItemLoySearch(string search, string storeId, int maxResult, bool includeDetails)
        {
            List<LoyItem> list = new List<LoyItem>();

            if (string.IsNullOrWhiteSpace(search))
                return list;

            SQLHelper.CheckForSQLInjection(search);

            char[] sep = new char[] { ' ' };
            string[] searchitems = search.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            string sqlwhere = string.Empty;
            foreach (string si in searchitems)
            {
                if (string.IsNullOrEmpty(sqlwhere))
                {
                    sqlwhere = string.Format(" WHERE mt.[Description] LIKE N'%{0}%' {1}", si, GetDbCICollation());
                }
                else
                {
                    sqlwhere += string.Format(" AND mt.[Description] LIKE N'%{0}%' {1}", si, GetDbCICollation());
                }
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetSQL(true, maxResult) + sqlcolumns + sqlfrom + sqlwhere +
                                            GetSQLStoreDist("mt.[No_]", storeId, true) + " ORDER BY mt.[Description]";
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyItem(reader, string.Empty, string.Empty, includeDetails, false, out string ts));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private ReplItem ReaderToItem(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            ReplItem item = new ReplItem()
            {
                Id = SQLHelper.GetString(reader["No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                Details = SQLHelper.GetStringByte(reader["Html"]),
                KeyingInPrice = SQLHelper.GetInt32(reader["Keying in Price"]),
                KeyingInQty = SQLHelper.GetInt32(reader["Keying in Quantity"]),
                NoDiscountAllowed = SQLHelper.GetInt32(reader["No Discount Allowed"]),
                ProductGroupId = SQLHelper.GetString(reader["Retail Product Code"]),
                ScaleItem = SQLHelper.GetInt32(reader["Scale Item"]),
                TaxItemGroupId = SQLHelper.GetString(reader["VAT Prod_ Posting Group"]),
                BaseUnitOfMeasure = SQLHelper.GetString(reader["Base Unit of Measure"]),
                ZeroPriceValId = SQLHelper.GetInt32(reader["Zero Price Valid"]),

                Blocked = SQLHelper.GetInt32(reader["Blocked"]),
                BlockedOnPos = SQLHelper.GetInt32(reader["BlockOnPos"]),
                BlockDiscount = SQLHelper.GetInt32(reader["BlockDiscount"]),
                BlockManualPriceChange = SQLHelper.GetInt32(reader["BlockPrice"]),
                BlockNegativeAdjustment = SQLHelper.GetInt32(reader["BlockNegAdj"]),
                BlockPositiveAdjustment = SQLHelper.GetInt32(reader["BlockPosAdj"]),
                BlockPurchaseReturn = SQLHelper.GetInt32(reader["BlockPurRet"]),
                BlockDistribution = SQLHelper.GetInt32(reader["DistStatus"]),

                UnitPrice = SQLHelper.GetDecimal(reader, "Unit Price"),
                PurchUnitOfMeasure = SQLHelper.GetString(reader["Purch_ Unit of Measure"]),
                SalseUnitOfMeasure = SQLHelper.GetString(reader["Sales Unit of Measure"]),
                VendorId = SQLHelper.GetString(reader["Vendor No_"]),
                VendorItemId = SQLHelper.GetString(reader["Vendor Item No_"]),

                SeasonCode = SQLHelper.GetString(reader["Season Code"]),
                ItemCategoryCode = SQLHelper.GetString(reader["Item Category Code"]),
                ItemFamilyCode = SQLHelper.GetString(reader["Item Family Code"]),

                GrossWeight = SQLHelper.GetDecimal(reader, "Gross Weight"),
                UnitsPerParcel = SQLHelper.GetDecimal(reader, "Units per Parcel"),
                UnitVolume = SQLHelper.GetDecimal(reader, "Unit Volume"),

                MustKeyInComment = 0
            };

            if (NavVersion > new Version("16.2.0.0"))
            {
                item.BlockedOnECom = SQLHelper.GetInt32(reader["BlockEcom"]);
            }

            return item;
        }

        private LoyItem ReaderToLoyItem(SqlDataReader reader, string storeId , string culture, bool incldetails, bool hastimestamp, out string timestamp)
        {
            LoyItem item = new LoyItem()
            {
                Id = SQLHelper.GetString(reader["No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                Details = SQLHelper.GetStringByte(reader["Html"]),

                ProductGroupId = SQLHelper.GetString(reader["Retail Product Code"]),
                SalesUomId = SQLHelper.GetString(reader["Sales Unit of Measure"]),
                ScaleItem = SQLHelper.GetBool(reader["Scale Item"]),
                Blocked = SQLHelper.GetBool(reader["Blocked"]),
                BlockDiscount = SQLHelper.GetBool(reader["BlockDiscount"]),
                BlockManualPriceChange = SQLHelper.GetBool(reader["BlockPrice"]),
                SeasonCode = SQLHelper.GetString(reader["Season Code"]),
                ItemCategoryCode = SQLHelper.GetString(reader["Item Category Code"]),
                ItemFamilyCode = SQLHelper.GetString(reader["Item Family Code"]),
                GrossWeight = SQLHelper.GetDecimal(reader, "Gross Weight"),
                UnitsPerParcel = SQLHelper.GetDecimal(reader, "Units per Parcel"),
                UnitVolume = SQLHelper.GetDecimal(reader, "Unit Volume"),
                QtyNotInDecimal = true
            };

            try
            {
                item.AllowedToSell = SQLHelper.GetInt32(reader["BlockOnPos"]) == 0;
            }
            catch
            {
                item.AllowedToSell = false;
            }

            ImageRepository imgrep = new ImageRepository(config, NavVersion);
            item.Images = imgrep.ImageGetByKey("Item", item.Id, string.Empty, string.Empty, 0, false);
            timestamp = (hastimestamp) ? ByteArrayToString(reader["timestamp"] as byte[]) : string.Empty;

            if (incldetails == false)
                return item;

            ItemLocationRepository locrep = new ItemLocationRepository(config);
            item.Locations = locrep.ItemLocationGetByItemId(item.Id, storeId);

            PriceRepository pricerep = new PriceRepository(config, NavVersion);
            item.Prices = pricerep.PricesGetByItemId(item.Id, storeId, culture);

            ItemUOMRepository uomrep = new ItemUOMRepository(config);
            item.UnitOfMeasures = uomrep.ItemUOMGetByItemId(item.Id);

            ItemVariantRegistrationRepository varrep = new ItemVariantRegistrationRepository(config, NavVersion);
            item.VariantsRegistration = varrep.VariantRegGetByItemId(item.Id);

            ExtendedVariantValuesRepository extvarrep = new ExtendedVariantValuesRepository(config, NavVersion);
            item.VariantsExt = extvarrep.VariantRegGetByItemId(item.Id);
            
            AttributeValueRepository attrrep = new AttributeValueRepository(config);
            item.ItemAttributes = attrrep.AttributesGetByItemId(item.Id);

            return item;
        }
    }
}
 