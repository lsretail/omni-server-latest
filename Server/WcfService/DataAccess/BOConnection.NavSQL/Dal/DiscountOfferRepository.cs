using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.DiscountEngine;
using LSRetail.Omni.DiscountEngine.Repositories;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class DiscountOfferRepository : BaseRepository
    {
        // Key: Store No., Priority No., Item No., Variant Code, Customer Disc. Group, Loyalty Scheme Code, From Date, To Date, Minimum Quantity
        const int TABLEID = 10012862;
        const int VTABLEID = 99001481;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;
        private string sqlVcolumns = string.Empty;
        private string sqlVfrom = string.Empty;

        public DiscountOfferRepository() : base()
        {
            sqlcolumns = "p.[Type],mt.[Store No_],mt.[Priority No_],mt.[Item No_],mt.[Variant Code],mt.[Customer Disc_ Group],mt.[Loyalty Scheme Code],mt.[From Date]," +
                         "mt.[To Date],mt.[Minimum Quantity],mt.[Discount _],mt.[Unit of Measure Code],mt.[Currency Code],mt.[Offer No_],mt.[Last Modify Date]";

            sqlfrom = " FROM [" + navCompanyName + "WI Discounts] mt" +
                      " INNER JOIN [" + navCompanyName + "Periodic Discount] p on p.No_ = mt.[Offer No_]";

            sqlVcolumns = "mt.[ID],mt.[Description],mt.[Starting Date],mt.[Ending Date],mt.[Starting Time],mt.[Ending Time],mt.[Time within Bounds],mt.[Monday Starting Time],mt.[Monday Ending Time] " +
                        ",mt.[Mon_ Time within Bounds],mt.[Tuesday Starting Time],mt.[Tuesday Ending Time],mt.[Tue_ Time within Bounds],mt.[Wednesday Starting Time],mt.[Wednesday Ending Time] " +
                        ",mt.[Wed_ Time within Bounds],mt.[Thursday Starting Time],mt.[Thursday Ending Time],mt.[Thu_ Time within Bounds],mt.[Friday Starting Time],mt.[Friday Ending Time] " +
                        ",mt.[Fri_ Time within Bounds],mt.[Saturday Starting Time],mt.[Saturday Ending Time],mt.[Sat_ Time within Bounds],mt.[Sunday Starting Time],mt.[Sunday Ending Time] " +
                        ",mt.[Sun_ Time within Bounds],mt.[Ending Time After Midnight],mt.[Mon_ End_ Time After Midnight],mt.[Tue_ End_ Time After Midnight],mt.[Wed_ End_ Time After Midnight] " +
                        ",mt.[Thu_ End_ Time After Midnight],mt.[Fri_ End_ Time After Midnight],mt.[Sat_ End_ Time After Midnight],mt.[Sun_ End_ Time After Midnight],mt.[No_ Series] ";

            sqlVfrom = " FROM [" + navCompanyName + "Validation Period] mt ";
        }

        public List<ReplDiscount> ReplicateDiscounts(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            // use actions for Periodic Discount table as we only have actions there
            List<JscKey> keys = new List<JscKey>();
            keys.Add(new JscKey()
            {
                FieldName = "Offer No_",
                FieldType = "nvarchar"
            });

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom;
                if (batchSize > 0)
                {
                    sql += GetWhereStatementWithStoreDist(true, keys, "mt.[Item No_]", storeId, false);
                }
            }

            // we use action count for peroidic discounts as WI Discounts does not have any actions
            recordsRemaining = GetRecordCount(99001453, lastKey, sql, (batchSize > 0) ? keys : null, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, 99001453, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplDiscount> list = new List<ReplDiscount>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatementWithStoreDist(fullReplication, keys, "mt.[Item No_]", storeId, true);

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
                                list.Add(ReaderToDiscount(reader, out lastKey));
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

                                list.Add(new ReplDiscount()
                                {
                                    OfferNo = par[0],
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
                                    list.Add(ReaderToDiscount(reader, out string ts));
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

        public List<ReplDiscountValidation> ReplicateDiscountValidations(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Validation Period");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlVfrom;
                if (batchSize > 0)
                {
                    sql += GetWhereStatement(true, keys, false);
                }
            }
            recordsRemaining = GetRecordCount(VTABLEID, lastKey, sql, (batchSize > 0) ? keys : null, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, VTABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplDiscountValidation> list = new List<ReplDiscountValidation>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlVcolumns + sqlVfrom + GetWhereStatement(fullReplication, keys, true);

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
                                list.Add(ReaderToDiscountValidation(reader, out lastKey));
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
                                list.Add(new ReplDiscountValidation()
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
                                    list.Add(ReaderToDiscountValidation(reader, out string ts));
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

        public List<ReplDiscount> DiscountGetByStore(string storeid, string itemid)
        {
            List<ReplDiscount> list = new List<ReplDiscount>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetSQL(false, 0) + sqlcolumns + sqlfrom + " WHERE mt.[Store No_]=@sid AND mt.[Item No_]=@iid";
                    command.Parameters.AddWithValue("@sid", storeid);
                    command.Parameters.AddWithValue("@iid", itemid);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToDiscount(reader, out string ts));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string loyaltySchemeCode)
        {
            DiscountEngine engine = new DiscountEngine(new NavRepository(navConnectionString));
            return engine.DiscountsGet(storeId, itemIds, loyaltySchemeCode);
        }
        
        private ReplDiscount ReaderToDiscount(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplDiscount()
            {
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                UnitOfMeasureId = SQLHelper.GetString(reader["Unit of Measure Code"]),
                PriorityNo = SQLHelper.GetInt32(reader["Priority No_"]),
                DiscountValue = SQLHelper.GetDecimal(reader["Discount _"]),
                CustomerDiscountGroup = SQLHelper.GetString(reader["Customer Disc_ Group"]),
                LoyaltySchemeCode = SQLHelper.GetString(reader["Loyalty Scheme Code"]),
                FromDate = SQLHelper.GetDateTime(reader["From Date"]),
                ToDate = SQLHelper.GetDateTime(reader["To Date"]),
                MinimumQuantity = SQLHelper.GetDecimal(reader["Minimum Quantity"]),
                CurrencyCode = SQLHelper.GetString(reader["Currency Code"]),
                OfferNo = SQLHelper.GetString(reader["Offer No_"]),
                ModifyDate = SQLHelper.GetDateTime(reader["Last Modify Date"]),
                Type = (PeriodicDiscType)(SQLHelper.GetInt32(reader["Type"]))
            };
        }

        private ReplDiscountValidation ReaderToDiscountValidation(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplDiscountValidation()
            {
                Id = SQLHelper.GetString(reader["ID"]),
                Description = SQLHelper.GetString(reader["Description"]),
                StartDate = SQLHelper.GetDateTime(reader["Starting Date"]),
                EndDate = SQLHelper.GetDateTime(reader["Ending Date"]),
                StartTime = SQLHelper.GetDateTime(reader["Starting Time"]),
                EndTime = SQLHelper.GetDateTime(reader["Ending Time"]),
                MondayStart = SQLHelper.GetDateTime(reader["Monday Starting Time"]),
                MondayEnd = SQLHelper.GetDateTime(reader["Monday Ending Time"]),
                TuesdayStart = SQLHelper.GetDateTime(reader["Tuesday Starting Time"]),
                TuesdayEnd = SQLHelper.GetDateTime(reader["Tuesday Ending Time"]),
                WednesdayStart = SQLHelper.GetDateTime(reader["Wednesday Starting Time"]),
                WednesdayEnd = SQLHelper.GetDateTime(reader["Wednesday Ending Time"]),
                ThursdayStart = SQLHelper.GetDateTime(reader["Thursday Starting Time"]),
                ThursdayEnd = SQLHelper.GetDateTime(reader["Thursday Ending Time"]),
                FridayStart = SQLHelper.GetDateTime(reader["Friday Starting Time"]),
                FridayEnd = SQLHelper.GetDateTime(reader["Friday Ending Time"]),
                SaturdayStart = SQLHelper.GetDateTime(reader["Saturday Starting Time"]),
                SaturdayEnd = SQLHelper.GetDateTime(reader["Saturday Ending Time"]),
                SundayStart = SQLHelper.GetDateTime(reader["Sunday Starting Time"]),
                SundayEnd = SQLHelper.GetDateTime(reader["Sunday Ending Time"]),
                TimeWithinBounds = SQLHelper.GetBool(reader["Time within Bounds"]),
                EndAfterMidnight = SQLHelper.GetBool(reader["Ending Time After Midnight"]),
                MondayWithinBounds = SQLHelper.GetBool(reader["Mon_ Time within Bounds"]),
                MondayEndAfterMidnight = SQLHelper.GetBool(reader["Mon_ End_ Time After Midnight"]),
                TuesdayWithinBounds = SQLHelper.GetBool(reader["Tue_ Time within Bounds"]),
                TuesdayEndAfterMidnight = SQLHelper.GetBool(reader["Tue_ End_ Time After Midnight"]),
                WednesdayWithinBounds = SQLHelper.GetBool(reader["Wed_ Time within Bounds"]),
                WednesdayEndAfterMidnight = SQLHelper.GetBool(reader["Wed_ End_ Time After Midnight"]),
                ThursdayWithinBounds = SQLHelper.GetBool(reader["Thu_ Time within Bounds"]),
                ThursdayEndAfterMidnight = SQLHelper.GetBool(reader["Thu_ End_ Time After Midnight"]),
                FridayWithinBounds = SQLHelper.GetBool(reader["Fri_ Time within Bounds"]),
                FridayEndAfterMidnight = SQLHelper.GetBool(reader["Fri_ End_ Time After Midnight"]),
                SaturdayWithinBounds = SQLHelper.GetBool(reader["Sat_ Time within Bounds"]),
                SaturdayEndAfterMidnight = SQLHelper.GetBool(reader["Sat_ End_ Time After Midnight"]),
                SundayWithinBounds = SQLHelper.GetBool(reader["Sun_ Time within Bounds"]),
                SundayEndAfterMidnight = SQLHelper.GetBool(reader["Sun_ End_ Time After Midnight"])
            };
        }
    }
}
 