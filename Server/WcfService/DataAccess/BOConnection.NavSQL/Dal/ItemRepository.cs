using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class ItemRepository : BaseRepository
    {
        // Key: No.
        const int TABLEID = 27;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public ItemRepository() : base()
        {
            sqlcolumns = "mt.[No_],mt.[Blocked],mt.[Description],mt.[Keying in Price],mt.[Keying in Quantity],mt.[No Discount Allowed]," +
                         "mt.[Product Group Code],mt.[Scale Item],mt.[VAT Prod_ Posting Group],mt.[Base Unit of Measure],mt.[Zero Price Valid]," +
                         "mt.[Sales Unit of Measure],mt.[Purch_ Unit of Measure],mt.[Vendor No_],mt.[Vendor Item No_],mt.[Unit Price]," +
                         "mt.[Gross Weight],mt.[Season Code],mt.[Item Category Code],mt.[Item Family Code],mt.[Unit Cost],mt.[Units per Parcel],mt.[Unit Volume],ih.[Html]," +
                         "(SELECT TOP(1) sl.[Block Sale on POS] FROM [" + navCompanyName + "Item Status Link] sl " +
                          "WHERE sl.[Item No_]=mt.[No_] AND [Starting Date]<GETDATE() AND sl.[Block Sale on POS]=1) AS BlockOnPos";

            sqlfrom = " FROM [" + navCompanyName + "Item] mt" +
                      " LEFT OUTER JOIN [" + navCompanyName + "Item HTML] ih ON mt.[No_]=ih.[Item No_]";
        }

        public List<ReplItem> ReplicateItems(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Item");
            string prevLastKey = lastKey;

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)";
                if (batchSize > 0)
                {
                    sql += sqlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[No_]", storeId, false);
                }
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, (batchSize > 0) ? keys : null, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplItem> list = new List<ReplItem>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom +
                GetWhereStatementWithStoreDist(fullReplication, keys, "mt.[No_]", storeId, true);

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
                            lastKey = prevLastKey;

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

        public List<LoyItem> ReplicateEcommFullItems(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Item");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)";
                if (batchSize > 0)
                {
                    sql += sqlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[No_]", storeId, false);
                }
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, (batchSize > 0) ? keys : null, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<LoyItem> list = new List<LoyItem>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatementWithStoreDist(fullReplication, keys, "mt.[No_]", storeId, true);

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
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom + " WHERE [Product Group Code]=@id";
                    command.Parameters.AddWithValue("@id", productGroupId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
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

            string sql =
            "WITH o AS (SELECT TOP(" + pageSize * pageNumber + ") mt.[No_],mt.[Description],mt.[Product Group Code],mt.[Sales Unit of Measure],ih.[Html]," +
            " ROW_NUMBER() OVER(ORDER BY mt.[Description]) AS RowNumber," +
            " (SELECT sl.[Block Sale on POS] FROM[" + navCompanyName + "Item Status Link] sl " +
            "WHERE sl.[Item No_]=mt.[No_] AND [Starting Date]<GETDATE() AND sl.[Block Sale on POS]=1) AS BlockOnPos" +
            sqlfrom +
            " LEFT OUTER JOIN [" + navCompanyName + "Product Group] pg ON pg.[Code]=mt.[Product Group Code]" +
            " WHERE (1=1)";

            if (string.IsNullOrWhiteSpace(itemCategoryId) == false)
                sql += " AND pg.[Item Category Code]='" + itemCategoryId + "'";
            if (string.IsNullOrWhiteSpace(productGroupId) == false)
                sql += " AND pg.[Code]='" + productGroupId + "'";
            if (string.IsNullOrWhiteSpace(search) == false)
                sql += " AND mt.[Description] LIKE '%" + search + "%'" + GetDbCICollation();

            sql += GetSQLStoreDist("mt.[No_]", storeId);
            sql += ") SELECT [No_],[Description],[Product Group Code],[Sales Unit of Measure],[Html],RowNumber,BlockOnPos" +
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
            BarcodeRepository brepo = new BarcodeRepository();
            Barcode bcode = brepo.BarcodeGetByCode(code);
            if (bcode == null)  // barcode not found
                throw new LSOmniServiceException(StatusCode.ItemNotFound, "Cannot find Item with Barcode:" + code);

            LoyItem item = ItemLoyGetById(bcode.ItemId, storeId, string.Empty, true);

            item.Prices.Clear();
            PriceRepository prepo = new PriceRepository();
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
                ItemVariantRegistrationRepository vreop = new ItemVariantRegistrationRepository();
                VariantRegistration variantReg = vreop.VariantRegGetById(bcode.VariantId, item.Id);
                if (variantReg != null)
                    item.VariantsRegistration.Add(variantReg);
            }

            if (string.IsNullOrWhiteSpace(bcode.UnitOfMeasureId) == false)
            {
                ItemUOMRepository ireop = new ItemUOMRepository();
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

        public List<LoyItem> ItemLoySearch(string search, string storeId, int maxResult, bool includeDetails)
        {
            List<LoyItem> list = new List<LoyItem>();

            if (string.IsNullOrWhiteSpace(search))
                return list;

            if (search.Contains("'"))
                search = search.Replace("'", "''");

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
                                            GetSQLStoreDist("mt.[No_]", storeId) + " ORDER BY mt.[Description]";
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

        public string ItemDetailsGetById(string itemId)
        {
            string detail = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Html] FROM [" + navCompanyName + "Item HTML] WHERE [Item No_]=@id";
                    command.Parameters.AddWithValue("@id", itemId);
                    TraceSqlCommand(command);
                    connection.Open();
                    detail = command.ExecuteScalar() as string;
                    connection.Close();
                }
            }
            return detail;
        }

        private ReplItem ReaderToItem(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplItem()
            {
                Id = SQLHelper.GetString(reader["No_"]),
                BlockedOnPos = SQLHelper.GetInt32(reader["BlockOnPos"]),
                Description = SQLHelper.GetString(reader["Description"]),
                Details = SQLHelper.GetStringByte(reader["Html"]),
                KeyingInPrice = SQLHelper.GetInt32(reader["Keying in Price"]),
                KeyingInQty = SQLHelper.GetInt32(reader["Keying in Quantity"]),
                NoDiscountAllowed = SQLHelper.GetInt32(reader["No Discount Allowed"]),
                ProductGroupId = SQLHelper.GetString(reader["Product Group Code"]),
                ScaleItem = SQLHelper.GetInt32(reader["Scale Item"]),
                TaxItemGroupId = SQLHelper.GetString(reader["VAT Prod_ Posting Group"]),
                BaseUnitOfMeasure = SQLHelper.GetString(reader["Base Unit of Measure"]),
                ZeroPriceValId = SQLHelper.GetInt32(reader["Zero Price Valid"]),

                UnitPrice = SQLHelper.GetDecimal(reader["Unit Price"]),
                PurchUnitOfMeasure = SQLHelper.GetString(reader["Purch_ Unit of Measure"]),
                SalseUnitOfMeasure = SQLHelper.GetString(reader["Sales Unit of Measure"]),
                VendorId = SQLHelper.GetString(reader["Vendor No_"]),
                VendorItemId = SQLHelper.GetString(reader["Vendor Item No_"]),

                SeasonCode = SQLHelper.GetString(reader["Season Code"]),
                ItemCategoryCode = SQLHelper.GetString(reader["Item Category Code"]),
                ItemFamilyCode = SQLHelper.GetString(reader["Item Family Code"]),

                GrossWeight = SQLHelper.GetDecimal(reader["Gross Weight"]),
                UnitCost = SQLHelper.GetDecimal(reader["Unit Cost"]),
                UnitsPerParcel = SQLHelper.GetDecimal(reader["Units per Parcel"]),
                UnitVolume = SQLHelper.GetDecimal(reader["Unit Volume"]),

                MustKeyInComment = 0
            };
        }

        private LoyItem ReaderToLoyItem(SqlDataReader reader, string storeId , string culture, bool incldetails, bool hastimestamp, out string timestamp)
        {
            LoyItem item = new LoyItem()
            {
                Id = SQLHelper.GetString(reader["No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                Details = SQLHelper.GetStringByte(reader["Html"]),
                ProductGroupId = SQLHelper.GetString(reader["Product Group Code"]),
                SalesUomId = SQLHelper.GetString(reader["Sales Unit of Measure"]),
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

            ImageRepository imgrep = new ImageRepository();
            item.Images = imgrep.ImageGetByKey("Item", item.Id, string.Empty, string.Empty, 0, false);
            timestamp = (hastimestamp) ? ByteArrayToString(reader["timestamp"] as byte[]) : string.Empty;

            if (incldetails == false)
                return item;

            PriceRepository pricerep = new PriceRepository();
            item.Prices = pricerep.PricesGetByItemId(item.Id, storeId, culture);

            ItemUOMRepository uomrep = new ItemUOMRepository();
            item.UnitOfMeasures = uomrep.ItemUOMGetByItemId(item.Id);

            ItemVariantRegistrationRepository varrep = new ItemVariantRegistrationRepository();
            item.VariantsRegistration = varrep.VariantRegGetByItemId(item.Id);

            ExtendedVariantValuesRepository extvarrep = new ExtendedVariantValuesRepository();
            item.VariantsExt = extvarrep.VariantRegGetByItemId(item.Id);
            
            AttributeValueRepository attrrep = new AttributeValueRepository();
            item.ItemAttributes = attrrep.AttributesGetByItemId(item.Id);

            return item;
        }
    }
}
 