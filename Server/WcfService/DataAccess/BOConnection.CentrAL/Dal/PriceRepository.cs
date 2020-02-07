using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
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

        public PriceRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Item No_],mt.[Sales Type],mt.[Sales Code],mt.[Starting Date],mt.[Ending Date],mt.[Currency Code]," +
                         "mt.[Variant Code],mt.[Unit of Measure Code],mt.[Minimum Quantity],mt.[Currency Code],mt.[Unit Price]," +
                         "mt.[Price Includes VAT],mt2.[Unit Price Including VAT],mt.[VAT Bus_ Posting Gr_ (Price)],spg.[Priority]";
            sqlfrom = " FROM [" + navCompanyName + "Sales Price$437dbf0e-84ff-417a-965d-ed2bb9650972] mt " +
                      "INNER JOIN [" + navCompanyName + "Sales Price$5ecfc871-5d82-43f1-9c54-59685e82318d] mt2 " +
                      "ON mt2.[Item No_]=mt.[Item No_] AND mt2.[Sales Type]=mt.[Sales Type] AND mt2.[Sales Code]=mt.[Sales Code] " +
                      "AND mt2.[Starting Date]=mt.[Starting Date] AND mt2.[Currency Code]=mt.[Currency Code] " +
                      "AND mt2.[Variant Code]=mt.[Variant Code] AND mt2.[Unit of Measure Code]=mt.[Unit of Measure Code] " +
                      "AND mt2.[Minimum Quantity]=mt.[Minimum Quantity]";

            sqlMcolumns = "mt.[Store No_],mt.[Item No_],mt.[Variant Code],mt.[Unit of Measure Code],mt.[Customer Disc_ Group]," +
                          "mt.[Loyalty Scheme Code],mt.[Currency Code],mt.[Unit Price],mt.[Offer No_],mt.[Last Modify Date]," +
                          "u.[Qty_ per Unit of Measure]";
            sqlMfrom = " FROM [" + navCompanyName + "WI Price$5ecfc871-5d82-43f1-9c54-59685e82318d] mt" +
                       " LEFT OUTER JOIN [" + navCompanyName + "Item Unit of Measure$437dbf0e-84ff-417a-965d-ed2bb9650972] u " +
                       "ON mt.[Item No_] = u.[Item No_] AND mt.[Unit of Measure Code] = u.[Code]";
        }

        public List<ReplPrice> ReplicatePrice(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";
            if (string.IsNullOrWhiteSpace(maxKey))
                maxKey = "0";

            // get all prices for a item that has changed
            List<JscKey> keys = new List<JscKey>()
            {
                new JscKey()
                {
                    FieldName = "Item No_",
                    FieldType = "nvarchar"
                }
            };

            SQLHelper.CheckForSQLInjection(storeId);

            // get records remaining
            string where = (string.IsNullOrEmpty(storeId)) ? string.Empty : string.Format(" AND mt.[Store No_]='{0}'", storeId);

            string sql = "SELECT COUNT(*)" + sqlMfrom + GetWhereStatement(true, keys, where, false);
            recordsRemaining = GetRecordCount(TABLEMOBILEID, lastKey, sql, keys, ref maxKey);

            int rr = 0;
            string tmplastkey = maxKey;
            List<JscActions> actions = LoadActions(fullReplication, 27, 0, ref maxKey, ref rr);
            actions.AddRange(LoadActions(fullReplication, 10000704, 0, ref tmplastkey, ref rr));
            if (Convert.ToInt64(tmplastkey) > Convert.ToInt64(maxKey))
                maxKey = tmplastkey;

            // get records
            sql = GetSQL(true, batchSize) + sqlMcolumns + sqlMfrom + GetWhereStatement(true, keys, where, true);

            List<ReplPrice> list = new List<ReplPrice>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = sql;

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

                    // find deleted items
                    if (fullReplication == false)
                    {
                        foreach (JscActions a in actions)
                        {
                            if (a.Type == DDStatementType.Delete)
                            {
                                string[] values = a.ParamValue.Split(new char[] { ';' });
                                if (values.Length > 2)
                                    a.ParamValue = values[2];
                                
                                list.Add(new ReplPrice()
                                {
                                    ItemId = a.ParamValue,
                                    IsDeleted = true
                                });
                            }
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

        public List<ReplPrice> ReplicateAllPrice(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Sales Price$437dbf0e-84ff-417a-965d-ed2bb9650972");

            // get records remaining
            string sql = string.Empty;
            string where = " AND spg.[Store] = '" + storeId + "' AND mt.[Sales Code]=spg.[Price Group Code]";
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + ",[" + navCompanyName + "Store Price Group$5ecfc871-5d82-43f1-9c54-59685e82318d] spg" + 
                    GetWhereStatementWithStoreDist(true, keys, where, "mt.[Item No_]", storeId, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplPrice> list = new List<ReplPrice>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + ",spg.[Priority]" + 
                sqlfrom + ",[" + navCompanyName + "Store Price Group$5ecfc871-5d82-43f1-9c54-59685e82318d] spg" + GetWhereStatementWithStoreDist(fullReplication, keys, where, "mt.[Item No_]", storeId, true);

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
                           string.Format(" RIGHT JOIN [{0}Store Price Group$5ecfc871-5d82-43f1-9c54-59685e82318d] spg ON spg.[Store]='{1}' AND spg.[Price Group Code]=mt.[Sales Code]", navCompanyName, storeId) +
                           string.Format(" INNER JOIN [{0}Customer Price Group$5ecfc871-5d82-43f1-9c54-59685e82318d] cpg ON cpg.[Code]=spg.[Price Group Code]", navCompanyName) +
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
                UnitPrice = SQLHelper.GetDecimal(reader, "Unit Price"),
                UnitPriceInclVat = SQLHelper.GetDecimal(reader, "Unit Price Including VAT"),
                PriceInclVat = SQLHelper.GetBool(reader["Price Includes VAT"]),
                MinimumQuantity = SQLHelper.GetDecimal(reader, "Minimum Quantity"),
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
                UnitPrice = SQLHelper.GetDecimal(reader, "Unit Price"),
                UnitPriceInclVat = SQLHelper.GetDecimal(reader, "Unit Price"),
                PriceInclVat = true,
                ModifyDate = SQLHelper.GetDateTime(reader["Last Modify Date"]),
                QtyPerUnitOfMeasure = SQLHelper.GetDecimal(reader, "Qty_ per Unit of Measure")
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
                Amt = SQLHelper.GetDecimal(reader, "Unit Price")
            };

            price.Amount = FormatAmount(price.Amt, culture);
            return price;
        }
    }
}