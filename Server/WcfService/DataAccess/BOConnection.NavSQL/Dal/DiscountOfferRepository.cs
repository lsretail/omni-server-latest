using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.DiscountEngine;
using LSRetail.Omni.DiscountEngine.Repositories;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class DiscountOfferRepository : BaseRepository
    {
        // Key: Store No., Priority No., Item No., Variant Code, Customer Disc. Group, Loyalty Scheme Code, From Date, To Date, Minimum Quantity
        const int DTABLEID = 10012862;
        const int MTABLEID = 10012863;
        const int VTABLEID = 99001481;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;
        private string sqlMMcolumns = string.Empty;
        private string sqlMMfrom = string.Empty;
        private string sqlVcolumns = string.Empty;
        private string sqlVfrom = string.Empty;

        public DiscountOfferRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Store No_],mt.[Priority No_],mt.[Item No_],mt.[Variant Code],mt.[Customer Disc_ Group],mt.[Loyalty Scheme Code],mt.[From Date]," +
                         "mt.[To Date],mt.[Minimum Quantity],mt.[Discount _],mt.[Unit of Measure Code],mt.[Offer No_],mt.[Last Modify Date]," +
                         "p.[Type],p.[Discount Type],p.[Description],p.[Pop-up Line 1],p.[Pop-up Line 2],p.[Pop-up Line 3],p.[Validation Period ID],p.[Discount Amount Value]";

            sqlfrom = " FROM [" + navCompanyName + "WI Discounts] mt" +
                      " JOIN [" + navCompanyName + "Periodic Discount] p ON p.[No_]=mt.[Offer No_]";

            sqlMMcolumns = "mt.[Store No_],mt.[Item No_],mt.[Variant Code],mt.[Customer Disc_ Group],mt.[Loyalty Scheme Code],mt.[From Date]," +
                           "mt.[To Date],mt.[Offer No_],mt.[Last Modify Date]," +
                           "p.[Type],p.[Priority],p.[Description],p.[Pop-up Line 1],p.[Pop-up Line 2],p.[Pop-up Line 3],p.[Validation Period ID]";

            sqlMMfrom = " FROM [" + navCompanyName + "WI Mix & Match Offer] mt" +
                      " JOIN [" + navCompanyName + "Periodic Discount] p ON p.[No_]=mt.[Offer No_]";

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
            if (string.IsNullOrWhiteSpace(maxKey))
            {
                maxKey = "0";
                if (int.TryParse(lastKey, out int maxi))
                {
                    maxKey = maxi.ToString();
                    lastKey = "0";
                }
            }

            // get all prices for a item that has changed
            List<JscKey> keys = new List<JscKey>()
            {
                new JscKey()
                {
                    FieldName = "Offer No_",
                    FieldType = "nvarchar"
                }
            };

            SQLHelper.CheckForSQLInjection(storeId);

            // get records remaining
            string where = (string.IsNullOrEmpty(storeId)) ? string.Empty : string.Format(" AND mt.[Store No_]='{0}'", storeId);

            string sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, where, false);
            recordsRemaining = GetRecordCount(DTABLEID, lastKey, sql, keys, ref maxKey);

            int rr = 0;
            List<JscActions> actions = LoadActions(fullReplication, 99001453, 0, ref maxKey, ref rr);

            // get records
            sql = GetSQL(true, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(true, keys, where, true);

            List<ReplDiscount> list = new List<ReplDiscount>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    if (fullReplication)
                    {
                        command.CommandText = "SELECT MAX([Entry No_]) FROM [" + navCompanyName + "Preaction] WHERE [Table No_]='99001453'";
                        TraceSqlCommand(command);
                        var ret = command.ExecuteScalar();
                        maxKey = (ret == DBNull.Value) ? "0" : ret.ToString();
                    }

                    command.CommandText = sql;

                    JscActions act = new JscActions(lastKey);
                    SetWhereValues(command, act, keys, true, true);
                    TraceSqlCommand(command);
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

                    // find deleted items
                    if (fullReplication == false)
                    {
                        foreach (JscActions a in actions)
                        {
                            if (a.Type == DDStatementType.Delete)
                            {
                                list.Add(new ReplDiscount(config.IsJson)
                                {
                                    OfferNo = a.ParamValue,
                                    IsDeleted = true
                                });
                                continue;
                            }

                            // check if this is action for disabled discount
                            using (SqlCommand command2 = connection.CreateCommand())
                            {
                                command2.CommandText = "SELECT [No_] FROM [" + navCompanyName + "Periodic Discount] WHERE [No_]=@0 AND [Status]=0";
                                command2.Parameters.AddWithValue("@0", a.ParamValue);
                                string no = (string)command2.ExecuteScalar();
                                if (no != null)
                                {
                                    list.Add(new ReplDiscount(config.IsJson)
                                    {
                                        OfferNo = no,
                                        IsDeleted = true
                                    });
                                }
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

        public List<ReplDiscount> ReplicateMixAndMatch(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";
            if (string.IsNullOrWhiteSpace(maxKey))
            {
                maxKey = "0";
                if (int.TryParse(lastKey, out int maxi))
                {
                    maxKey = maxi.ToString();
                    lastKey = "0";
                }
            }

            SQLHelper.CheckForSQLInjection(storeId);

            // use actions for 99001453-Periodic Discount table as we only have actions there
            List<JscKey> keys = new List<JscKey>()
            {
                new JscKey()
                {
                    FieldName = "Offer No_",
                    FieldType = "nvarchar"
                }
            };

            // get records remaining
            string where = (string.IsNullOrEmpty(storeId)) ? string.Empty : string.Format(" AND mt.[Store No_]='{0}'", storeId);

            string sql = "SELECT COUNT(*)" + sqlMMfrom + GetWhereStatement(true, keys, where, false);
            recordsRemaining = GetRecordCount(MTABLEID, lastKey, sql, keys, ref maxKey);

            int rr = 0;
            List<JscActions> actions = LoadActions(fullReplication, 99001453, 0, ref maxKey, ref rr);

            // get records
            sql = GetSQL(true, batchSize) + sqlMMcolumns + sqlMMfrom + GetWhereStatement(true, keys, where, true);

            List<ReplDiscount> list = new List<ReplDiscount>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    if (fullReplication)
                    {
                        command.CommandText = "SELECT MAX([Entry No_]) FROM [" + navCompanyName + "Preaction] WHERE [Table No_]='99001453'";
                        TraceSqlCommand(command);
                        var ret = command.ExecuteScalar();
                        maxKey = (ret == DBNull.Value) ? "0" : ret.ToString();
                    }

                    command.CommandText = sql;

                    JscActions act = new JscActions(lastKey);
                    SetWhereValues(command, act, keys, true, true);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int cnt = 0;
                        while (reader.Read())
                        {
                            list.Add(ReaderToMixMatch(reader, out lastKey));
                            cnt++;
                        }
                        reader.Close();
                        recordsRemaining -= cnt;
                    }

                    if (fullReplication == false)
                    {
                        foreach (JscActions a in actions)
                        {
                            if (a.Type == DDStatementType.Delete)
                            {
                                list.Add(new ReplDiscount(config.IsJson)
                                {
                                    OfferNo = a.ParamValue,
                                    IsDeleted = true
                                });
                                continue;
                            }

                            // check if this is action for disabled discount
                            using (SqlCommand command2 = connection.CreateCommand())
                            {
                                command2.CommandText = "SELECT [No_] FROM [" + navCompanyName + "Periodic Discount] WHERE [No_]=@0 AND [Status]=0";
                                command2.Parameters.AddWithValue("@0", a.ParamValue);
                                string no = (string)command2.ExecuteScalar();
                                if (no != null)
                                {
                                    list.Add(new ReplDiscount(config.IsJson)
                                    {
                                        OfferNo = no,
                                        IsDeleted = true
                                    });
                                }
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

        public List<ReplDiscountValidation> ReplicateDiscountValidations(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Validation Period");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlVfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(VTABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, VTABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplDiscountValidation> list = new List<ReplDiscountValidation>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlVcolumns + sqlVfrom + GetWhereStatement(fullReplication, keys, true);

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
                                list.Add(new ReplDiscountValidation(config.IsJson)
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
            DiscountEngine engine;
            if (NavVersion.Major > 14)
                engine = new DiscountEngine(new Nav15Repository(navConnectionString));
            else
                engine = new DiscountEngine(new NavRepository(navConnectionString));

            return engine.DiscountsGet(storeId, itemIds, loyaltySchemeCode);
        }

        private ReplDiscount ReaderToDiscount(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            ReplDiscount disc = new ReplDiscount(config.IsJson)
            {
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                UnitOfMeasureId = SQLHelper.GetString(reader["Unit of Measure Code"]),
                PriorityNo = SQLHelper.GetInt32(reader["Priority No_"]),
                DiscountValue = SQLHelper.GetDecimal(reader, "Discount _"),
                DiscountValueType = (DiscountValueType)SQLHelper.GetInt32(reader["Discount Type"]),
                CustomerDiscountGroup = SQLHelper.GetString(reader["Customer Disc_ Group"]),
                LoyaltySchemeCode = SQLHelper.GetString(reader["Loyalty Scheme Code"]),
                FromDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["From Date"]), config.IsJson),
                ToDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["To Date"]), config.IsJson),
                MinimumQuantity = SQLHelper.GetDecimal(reader, "Minimum Quantity"),
                OfferNo = SQLHelper.GetString(reader["Offer No_"]),
                ModifyDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Last Modify Date"]), config.IsJson),
                Type = (ReplDiscountType)SQLHelper.GetInt32(reader["Type"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ValidationPeriodId = SQLHelper.GetInt32(reader["Validation Period ID"])
            };

            string tx1 = SQLHelper.GetString(reader["Pop-up Line 1"]);
            string tx2 = SQLHelper.GetString(reader["Pop-up Line 2"]);
            string tx3 = SQLHelper.GetString(reader["Pop-up Line 3"]);
            disc.Details = tx1;
            if (string.IsNullOrEmpty(tx2) == false)
                disc.Details += "\r\n" + tx2;
            if (string.IsNullOrEmpty(tx3) == false)
                disc.Details += "\r\n" + tx3;

            decimal amt = SQLHelper.GetDecimal(reader, "Discount Amount Value");
            if (amt > 0 && disc.Type == ReplDiscountType.DiscOffer)
            {
                disc.DiscountValueType = DiscountValueType.Amount;
                disc.DiscountValue = amt;
            }
            return disc;
        }

        private ReplDiscount ReaderToMixMatch(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            ReplDiscount disc = new ReplDiscount(config.IsJson)
            {
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                PriorityNo = SQLHelper.GetInt32(reader["Priority"]),
                CustomerDiscountGroup = SQLHelper.GetString(reader["Customer Disc_ Group"]),
                LoyaltySchemeCode = SQLHelper.GetString(reader["Loyalty Scheme Code"]),
                FromDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["From Date"]), config.IsJson),
                ToDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["To Date"]), config.IsJson),
                OfferNo = SQLHelper.GetString(reader["Offer No_"]),
                ModifyDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Last Modify Date"]), config.IsJson),
                Type = (ReplDiscountType)(SQLHelper.GetInt32(reader["Type"])),
                Description = SQLHelper.GetString(reader["Description"]),
                ValidationPeriodId = SQLHelper.GetInt32(reader["Validation Period ID"])
            };

            string tx1 = SQLHelper.GetString(reader["Pop-up Line 1"]);
            string tx2 = SQLHelper.GetString(reader["Pop-up Line 2"]);
            string tx3 = SQLHelper.GetString(reader["Pop-up Line 3"]);
            disc.Details = tx1;
            if (string.IsNullOrEmpty(tx2) == false)
                disc.Details += "\r\n" + tx2;
            if (string.IsNullOrEmpty(tx3) == false)
                disc.Details += "\r\n" + tx3;

            return disc;
        }

        private ReplDiscountValidation ReaderToDiscountValidation(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplDiscountValidation(config.IsJson)
            {
                Id = SQLHelper.GetString(reader["ID"]),
                Description = SQLHelper.GetString(reader["Description"]),
                StartDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Starting Date"]), config.IsJson),
                EndDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Ending Date"]), config.IsJson),
                StartTime = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Starting Time"]), config.IsJson),
                EndTime = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Ending Time"]), config.IsJson),
                MondayStart = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Monday Starting Time"]), config.IsJson),
                MondayEnd = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Monday Ending Time"]), config.IsJson),
                TuesdayStart = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Tuesday Starting Time"]), config.IsJson),
                TuesdayEnd = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Tuesday Ending Time"]), config.IsJson),
                WednesdayStart = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Wednesday Starting Time"]), config.IsJson),
                WednesdayEnd = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Wednesday Ending Time"]), config.IsJson),
                ThursdayStart = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Thursday Starting Time"]), config.IsJson),
                ThursdayEnd = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Thursday Ending Time"]), config.IsJson),
                FridayStart = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Friday Starting Time"]), config.IsJson),
                FridayEnd = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Friday Ending Time"]), config.IsJson),
                SaturdayStart = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Saturday Starting Time"]), config.IsJson),
                SaturdayEnd = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Saturday Ending Time"]), config.IsJson),
                SundayStart = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Sunday Starting Time"]), config.IsJson),
                SundayEnd = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Sunday Ending Time"]), config.IsJson),
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
 