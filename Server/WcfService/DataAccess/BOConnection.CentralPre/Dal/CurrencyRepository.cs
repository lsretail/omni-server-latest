using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class CurrencyRepository : BaseRepository
    {
        // Key: Code
        const int TABLEID = 4;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public CurrencyRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Code],mt.[Description],mt.[Amount Rounding Precision],mt.[Invoice Rounding Type]," +
                         "mt.[Invoice Rounding Precision],mt2.[LSC POS Currency Symbol],mt2.[LSC Placement Of Curr_ Symbol]";

            sqlfrom = " FROM [" + navCompanyName + "Currency$437dbf0e-84ff-417a-965d-ed2bb9650972] mt" +
                      " JOIN [" + navCompanyName + "Currency$5ecfc871-5d82-43f1-9c54-59685e82318d] mt2 ON mt2.[Code]=mt.[Code]";
        }

        public List<ReplCurrency> ReplicateCurrency(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Currency$437dbf0e-84ff-417a-965d-ed2bb9650972");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplCurrency> list = new List<ReplCurrency>();

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
                                list.Add(ReaderToCurrency(reader, out lastKey));
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
                                list.Add(new ReplCurrency()
                                {
                                    CurrencyCode = act.ParamValue,
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
                                    list.Add(ReaderToCurrency(reader, out string ts));
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

        public ReplCurrency CurrencyGetByStoreId(string storeId)
        {
            StoreRepository storeRep = new StoreRepository(config, LSCVersion);
            ReplStore store = storeRep.StoreGetById(storeId);
            return CurrencyGetById(store.Currency);
        }

        public ReplCurrency CurrencyGetById(string id)
        {
            ReplCurrency currency = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetSQL(false, 0) + sqlcolumns + sqlfrom + " WHERE mt.[Code]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currency = ReaderToCurrency(reader, out string ts);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return currency;
        }

        private ReplCurrency ReaderToCurrency(SqlDataReader reader, out string timestamp)
        {
            ReplCurrency currency = new ReplCurrency()
            {
                CurrencyCode = SQLHelper.GetString(reader["Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                RoundOfSales = SQLHelper.GetDecimal(reader, "Amount Rounding Precision"),
                RoundOfTypeAmount = SQLHelper.GetInt32(reader["Invoice Rounding Type"]),
                RoundOfAmount = SQLHelper.GetDecimal(reader, "Invoice Rounding Precision"),
                Symbol = SQLHelper.GetString(reader["LSC POS Currency Symbol"])
            };

            int cursymplacement = SQLHelper.GetInt32(reader["LSC Placement Of Curr_ Symbol"]);
            if (cursymplacement == 0)
            {
                currency.CurrencyPrefix = string.Empty;
                currency.CurrencySuffix = currency.Symbol;
            }
            else
            {
                currency.CurrencyPrefix = currency.Symbol;
                currency.CurrencySuffix = string.Empty;
            }
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);
            return currency;
        }

        public Currency CurrencyLoyGetById(string id, string culture)
        {
            Currency currency = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetSQL(false, 0) + sqlcolumns + sqlfrom + " WHERE mt.[Code]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currency = ReaderToLoyCurrency(reader, culture);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return currency;
        }

        private Currency ReaderToLoyCurrency(SqlDataReader reader, string culture)
        {
            Currency cur = new Currency
            {
                Id = SQLHelper.GetString(reader["Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                RoundOffSales = SQLHelper.GetDecimal(reader, "Amount Rounding Precision"),
                RoundOffAmount = SQLHelper.GetDecimal(reader, "Invoice Rounding Precision"),
                Symbol = SQLHelper.GetString(reader["LSC POS Currency Symbol"]),
                Culture = culture,
                DecimalSeparator = CultureInfo.CurrentUICulture.NumberFormat.CurrencyDecimalSeparator,
                ThousandSeparator = CultureInfo.CurrentUICulture.NumberFormat.CurrencyGroupSeparator
            };

            cur.DecimalPlaces = CurrencyGetHelper.GetNumberOfDecimals(cur.RoundOffSales);

            int cursymplacement = SQLHelper.GetInt32(reader["LSC Placement Of Curr_ Symbol"]);
            if (cursymplacement == 0)
            {
                cur.Prefix = string.Empty;
                cur.Postfix = cur.Symbol;
            }
            else
            {
                cur.Prefix = cur.Symbol;
                cur.Postfix = string.Empty;
            }

            // to overwrite the culture on the server
            if (string.IsNullOrWhiteSpace(culture) == false)
            {
                CultureInfo ci = new CultureInfo(culture);//de-DE en-US
                cur.DecimalSeparator = ci.NumberFormat.CurrencyDecimalSeparator;
                cur.ThousandSeparator = ci.NumberFormat.CurrencyGroupSeparator;
            }

            int roundoftype = SQLHelper.GetInt32(reader["Invoice Rounding Type"]);
            switch (roundoftype)
            {
                case 0:
                    //  <!-- SaleRoundingMethod: RoundNearest, RoundDown,RoundUp -->
                    cur.SaleRoundingMethod = CurrencyRoundingMethod.RoundNearest;
                    break;
                case 1:
                    //  <!-- SaleRoundingMethod: RoundNearest, RoundDown,RoundUp -->
                    cur.SaleRoundingMethod = CurrencyRoundingMethod.RoundDown;
                    break;
                case 2:
                    //  <!-- SaleRoundingMethod: RoundNearest, RoundDown,RoundUp -->
                    cur.SaleRoundingMethod = CurrencyRoundingMethod.RoundUp;
                    break;
            }

            cur.AmountRoundingMethod = cur.SaleRoundingMethod; //nav doesn't have amount rounding..
            return cur;
        }
    }
}
 