using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class PriceRepository : BaseRepository
    {
        // Key: Item No., Sales Type, Sales Code, Starting Date, Currency Code, Variant Code, Unit of Measure Code, Minimum Quantity
        const int TABLEID = 7002;
        // Key: StoreId, ItemId, VariantId, UomId, CustomerDiscountGroup, LoyaltySchemeCode
        const int TABLEMOBILEID = 10012861;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;
        private string sqlMcolumns = string.Empty;
        private string sqlMfrom = string.Empty;

        public PriceRepository() : base()
        {
            sqlcolumns = "mt.[Item No_],mt.[Sales Type],mt.[Sales Code],mt.[Starting Date],mt.[Ending Date],mt.[Currency Code],mt.[Variant Code],mt.[Unit of Measure Code]," +
                         "mt.[Minimum Quantity],mt.[Currency Code],mt.[Unit Price],mt.[Price Includes VAT],mt.[Unit Price Including VAT],mt.[VAT Bus_ Posting Gr_ (Price)],spg.[Priority]";
            sqlfrom = " FROM [" + navCompanyName + "Sales Price] mt";

            sqlMcolumns = "mt.[Store No_],mt.[Item No_],mt.[Variant Code],mt.[Unit of Measure Code],mt.[Customer Disc_ Group],mt.[Loyalty Scheme Code],mt.[Currency Code],mt.[Unit Price],mt.[Offer No_],mt.[Last Modify Date],u.[Qty_ per Unit of Measure]";
            sqlMfrom = " FROM [" + navCompanyName + "WI Price] mt" +
                       " LEFT OUTER JOIN [" + navCompanyName + "Item Unit of Measure] u ON mt.[Item No_] = u.[Item No_] AND mt.[Unit of Measure Code] = u.[Code]";
        }

        public List<ReplPrice> ReplicatePrice(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            // get all prices for a item that has changed
            List<JscKey> keys = new List<JscKey>()
            {
                new JscKey()
                {
                    FieldName = "Item No_",
                    FieldType = "nvarchar"
                }
            };

            List<JscActions> actions = new List<JscActions>();

            SQLHelper.CheckForSQLInjection(storeId);

            // get records remaining
            string where = (string.IsNullOrEmpty(storeId)) ? string.Empty : string.Format(" AND mt.[Store No_]='{0}'", storeId);
            string sql = string.Empty;
            string tmplastkey = "0";
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlMfrom;
                if (batchSize > 0)
                {
                    sql += GetWhereStatement(true, keys, where, false);
                }

                recordsRemaining = GetRecordCount(TABLEMOBILEID, lastKey, sql, (batchSize > 0) ? keys : null, ref maxKey);

                // get maxkey value for item
                GetRecordCount(27, tmplastkey, sql, (batchSize > 0) ? keys : null, ref maxKey);
            }
            else
            {
                // Use actions from Item,Sales Price,Item Variant,Item Unit of Measure tables and as trigger for price update
                actions = LoadActions(fullReplication, 27, 0, ref lastKey, ref recordsRemaining);

                List<JscActions> priceact = LoadActions(fullReplication, TABLEID, batchSize, ref tmplastkey, ref recordsRemaining);
                priceact.AddRange(LoadActions(fullReplication, 5401, batchSize, ref tmplastkey, ref recordsRemaining));
                priceact.AddRange(LoadActions(fullReplication, 5404, batchSize, ref tmplastkey, ref recordsRemaining));
                foreach (JscActions act in priceact)
                {
                    JscActions newact = new JscActions()
                    {
                        id = act.id,
                        TableId = act.TableId,
                        Type = act.Type,
                        ParamValue = act.ParamValue.Substring(0, act.ParamValue.IndexOf(';'))
                    };

                    JscActions findme = actions.Find(x => x.ParamValue.Equals(newact.ParamValue));
                    if (findme == null)
                        actions.Add(newact);
                }
                recordsRemaining = 0;   // we will process all actions
            }

            List<ReplPrice> list = new List<ReplPrice>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlMcolumns + sqlMfrom + GetWhereStatement(fullReplication, keys, where, true);

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
                                list.Add(ReaderToWIPrice(reader, out lastKey));
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
                                string[] par = act.ParamValue.Split(';');
                                if (par.Length < 2 || par.Length != keys.Count)
                                    continue;

                                list.Add(new ReplPrice()
                                {
                                    ItemId = par[0],
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
                                    list.Add(ReaderToWIPrice(reader, out string ts));
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

        public List<ReplPrice> ReplicateAllPrice(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Sales Price");

            // get records remaining
            string sql = string.Empty;
            string where = " AND spg.[Store] = '" + storeId + "' AND mt.[Sales Code]=spg.[Price Group Code]";
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + ",[" + navCompanyName + "Store Price Group] spg";
                if (batchSize > 0)
                {
                    sql += GetWhereStatementWithStoreDist(true, keys, where, "mt.[Item No_]", storeId, false);
                }
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, (batchSize > 0) ? keys : null, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplPrice> list = new List<ReplPrice>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + ",spg.[Priority]" + 
                sqlfrom + ",[" + navCompanyName + "Store Price Group] spg" + GetWhereStatementWithStoreDist(fullReplication, keys, where, "mt.[Item No_]", storeId, true);

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
                                list.Add(ReaderToPrice(reader, storeId, out lastKey));
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
                                string[] par = act.ParamValue.Split(';');
                                if (par.Length < 8 || par.Length != keys.Count)
                                    continue;

                                list.Add(new ReplPrice()
                                {
                                    ItemId = par[0],
                                    SaleType = Convert.ToInt32(par[1]),
                                    SaleCode = par[2],
                                    StartingDate = GetDateTimeFromNav(par[3]),
                                    CurrencyCode = par[4],
                                    VariantId = par[5],
                                    UnitOfMeasure = par[6],
                                    MinimumQuantity = Convert.ToDecimal(par[7]),
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
                                    list.Add(ReaderToPrice(reader, storeId, out string ts));
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

        public List<ReplPrice> PriceGetByCodes(string storeId, int salesType, string itemId, string curId, string varId,
            string uomId, DateTime dateValid)
        {
            SQLHelper.CheckForSQLInjection(storeId);
            SQLHelper.CheckForSQLInjection(itemId);
            SQLHelper.CheckForSQLInjection(curId);
            SQLHelper.CheckForSQLInjection(varId);
            SQLHelper.CheckForSQLInjection(uomId);

            List<ReplPrice> list = new List<ReplPrice>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetSQL(false, 0) + sqlMcolumns + sqlMfrom;
                    command.CommandText += "WHERE mt.[Store No_]=@f0 AND mt.[Item No_]=@f1 ";
                    command.Parameters.AddWithValue("@f0", storeId);
                    command.Parameters.AddWithValue("@f1", itemId);
                    
                    //TODO: check for correct variant and currency

                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToWIPrice(reader, out string ts));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        
        public List<ReplPrice> ECOMMPriceGetByCodes(string storeId, int salesType, string itemId, string curId, string varId, string uomId, DateTime dateValid)
        {
            SQLHelper.CheckForSQLInjection(storeId);
            SQLHelper.CheckForSQLInjection(itemId);
            SQLHelper.CheckForSQLInjection(curId);
            SQLHelper.CheckForSQLInjection(varId);
            SQLHelper.CheckForSQLInjection(uomId);

            List<ReplPrice> list = new List<ReplPrice>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetSQL(false, 0) + sqlcolumns + sqlfrom +
                           string.Format(" RIGHT JOIN [{0}Store Price Group] spg ON spg.[Store]='{1}' AND spg.[Price Group Code]=mt.[Sales Code]", navCompanyName, storeId) +
                           string.Format(" INNER JOIN [{0}Customer Price Group] cpg ON cpg.[Code]=spg.[Price Group Code]", navCompanyName) +
                           string.Format(" WHERE mt.[Sales Type]={0} AND mt.[Item No_]='{1}'", salesType, itemId) +
                           string.Format(" AND (mt.[Currency Code]='' OR mt.[Currency Code]='{0}') AND (mt.[Variant Code]='' OR mt.[Variant Code]='{1}')", curId, varId) +
                           string.Format(" AND (mt.[Unit of Measure Code]='' OR mt.[Unit of Measure Code]='{0}')", uomId) +
                           string.Format(" AND mt.[Starting Date]<'{0}' AND (mt.[Ending Date]>='{0}' OR mt.[Ending Date]='{1}') ORDER BY spg.[Priority] DESC", GetSQLNAVDate(dateValid), GetSQLNAVDate(DateTime.MinValue));

                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToPrice(reader, string.Empty, out string ts));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<Price> PricesGetByItemId(string itemId, string storeId, string culture)
        {
            List<Price> list = new List<Price>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlMcolumns + sqlMfrom + " WHERE mt.[Item No_]=@id";
                    command.Parameters.AddWithValue("@id", itemId);

                    if (string.IsNullOrWhiteSpace(storeId) == false)
                    {
                        command.CommandText += " AND mt.[Store No_]=@Sid";
                        command.Parameters.AddWithValue("@Sid", storeId);
                    }

                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyPrice(reader, culture));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public Price PriceGetByIds(string itemId, string storeId, string variantId, string uomId, string culture)
        {
            Price price = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlMcolumns + sqlMfrom + " WHERE mt.[Item No_]=@Iid";
                    command.Parameters.AddWithValue("@Iid", itemId);

                    if (string.IsNullOrWhiteSpace(storeId) == false)
                    {
                        command.CommandText += " AND mt.[Store No_]=@Sid";
                        command.Parameters.AddWithValue("@Sid", storeId);
                    }

                    if (string.IsNullOrWhiteSpace(variantId) == false)
                    {
                        command.CommandText += " AND mt.[Variant Code]=@Vid";
                        command.Parameters.AddWithValue("@Vid", variantId);
                    }

                    if (string.IsNullOrWhiteSpace(uomId) == false)
                    {
                        command.CommandText += " AND mt.[Unit of Measure Code]=@Uid";
                        command.Parameters.AddWithValue("Uid", uomId);
                    }

                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            price = ReaderToLoyPrice(reader, culture);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return price;
        }

        private ReplPrice ReaderToPrice(SqlDataReader reader, string storeid, out string timestamp)
        {
            ReplPrice price = new ReplPrice()
            {
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                SaleType = SQLHelper.GetInt32(reader["Sales Type"]),
                SaleCode = SQLHelper.GetString(reader["Sales Code"]),
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                UnitOfMeasure = SQLHelper.GetString(reader["Unit of Measure Code"]),
                CurrencyCode = SQLHelper.GetString(reader["Currency Code"]),
                UnitPrice = SQLHelper.GetDecimal(reader["Unit Price"]),
                UnitPriceInclVat = SQLHelper.GetDecimal(reader["Unit Price Including VAT"]),
                PriceInclVat = SQLHelper.GetBool(reader["Price Includes VAT"]),
                MinimumQuantity = SQLHelper.GetDecimal(reader["Minimum Quantity"]),
                StartingDate = SQLHelper.GetDateTime(reader["Starting Date"]),
                EndingDate = SQLHelper.GetDateTime(reader["Ending Date"]),
                VATPostGroup = SQLHelper.GetString(reader["VAT Bus_ Posting Gr_ (Price)"]),
                Priority = SQLHelper.GetInt32(reader["Priority"])
            };

            if (string.IsNullOrWhiteSpace(storeid) == false)
                price.StoreId = storeid;

            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);
            return price;
        }

        private ReplPrice ReaderToWIPrice(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplPrice()
            {
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                UnitOfMeasure = SQLHelper.GetString(reader["Unit of Measure Code"]),
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                CustomerDiscountGroup = SQLHelper.GetString(reader["Customer Disc_ Group"]),
                LoyaltySchemeCode = SQLHelper.GetString(reader["Loyalty Scheme Code"]),
                CurrencyCode = SQLHelper.GetString(reader["Currency Code"]),
                UnitPrice = SQLHelper.GetDecimal(reader["Unit Price"]),
                ModifyDate = SQLHelper.GetDateTime(reader["Last Modify Date"]),
                QtyPerUnitOfMeasure = SQLHelper.GetDecimal(reader["Qty_ per Unit of Measure"]),
                PriceInclVat = true
            };
        }

        private Price ReaderToLoyPrice(SqlDataReader reader, string culture)
        {
            Price price = new Price()
            {
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                UomId = SQLHelper.GetString(reader["Unit of Measure Code"]),
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                Amt = SQLHelper.GetDecimal(reader["Unit Price"])
            };

            price.Amount = FormatAmount(price.Amt, culture);
            return price;
        }
    }
}