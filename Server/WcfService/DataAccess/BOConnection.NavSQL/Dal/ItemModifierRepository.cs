using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class ItemModifierRepository : BaseRepository
    {
        // Key: Code
        const int TABLEID = 99001483;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public ItemModifierRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "tsi.[Value],mt.[Code],mt.[Subcode],mt.[Description],mt.[Variant Code],mt.[Unit of Measure]," +
                     "mt.[Trigger Function],mt.[Trigger Code],mt.[Price Type],mt.[Price Handling],mt.[Amount _Percent]," +
                     "ic.[Max_ Selection],ic.[Min_ Selection],mt.[Time Modifier Minutes]," +
                     "tsi.[Usage Category],tsi.[Usage Sub-Category],ic.[Explanatory Header Text],ic.[Prompt]";

            sqlfrom = " FROM [" + navCompanyName + "Table Specific Infocode] tsi " +
                      "INNER JOIN [" + navCompanyName + "Information Subcode] mt ON mt.[Code]=tsi.[Infocode Code]";
        }

        public List<ReplItemModifier> ReplicateItemModifier(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Information Subcode");
            string prevlastkey = lastKey;

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*) " + sqlfrom + GetWhereStatementWithStoreDist(true, keys, "tsi.[Value]", storeId, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplItemModifier> list = new List<ReplItemModifier>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom +
                " LEFT OUTER JOIN [" + navCompanyName + "Infocode] ic ON ic.[Code]=mt.[Code]" +
                GetWhereStatementWithStoreDist(fullReplication, keys, "tsi.[Value]", storeId, true);

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
                                ReplItemModifier rec = ReaderToModifier(reader, out lastKey);
                                list.Add(rec);
                                if (rec.TriggerFunction == ItemTriggerFunction.Infocode)
                                {
                                    list.AddRange(GetSubCodes(rec.TriggerCode, rec.Id));
                                }
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

                                list.Add(new ReplItemModifier()
                                {
                                    Code = par[0],
                                    SubCode = par[1],
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
                                    ReplItemModifier rec = ReaderToModifier(reader, out string ts);
                                    list.Add(rec);
                                    if (rec.TriggerFunction == ItemTriggerFunction.Infocode)
                                    {
                                        list.AddRange(GetSubCodes(rec.TriggerCode, rec.Id));
                                    }
                                }
                                reader.Close();
                            }
                            first = false;
                        }

                        if (actions.Count == 0)
                            lastKey = prevlastkey;

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

        public List<ItemModifier> ModifierGetByItemId(string id)
        {
            List<ItemModifier> list = new List<ItemModifier>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom +
                        " LEFT OUTER JOIN [" + navCompanyName + "Infocode] ic ON ic.[Code]=mt.[Code]" +
                        " WHERE tsi.[Value]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ItemModifier rec = ReaderToModifier(reader);
                            list.Add(rec);
                            if (rec.TriggerFunction == ItemTriggerFunction.Infocode)
                            {
                                list.AddRange(GetSubCodes2(rec.TriggerCode, id));
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private List<ReplItemModifier> GetSubCodes(string code, string value)
        {
            List<JscKey> keys = GetPrimaryKeys("Information Subcode");
            List<ReplItemModifier> list = new List<ReplItemModifier>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "SELECT mt.[timestamp],'' AS [Value],mt.[Code],mt.[Subcode],mt.[Description],mt.[Variant Code]," +
                                 "mt.[Unit of Measure],mt.[Trigger Function],mt.[Trigger Code],mt.[Price Type],mt.[Price Handling],mt.[Amount _Percent]," +
                                 "mt.[Max_ Selection],mt.[Min_ Selection],mt.[Time Modifier Minutes],mt.[Usage Category],'' AS [Usage Sub-Category],'' AS [Explanatory Header Text],'' AS [Prompt] " +
                                 "FROM [" + navCompanyName + "Information Subcode] mt " +
                                 "WHERE mt.[Code]=@id";

                    command.Parameters.AddWithValue("@id", code);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ReplItemModifier rec = ReaderToModifier(reader, out string lastKey);
                            rec.Id = value;
                            list.Add(rec);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private List<ItemModifier> GetSubCodes2(string code, string value)
        {
            List<JscKey> keys = GetPrimaryKeys("LSC Information Subcode$5ecfc871-5d82-43f1-9c54-59685e82318d");
            List<ItemModifier> list = new List<ItemModifier>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "SELECT mt.[Code],mt.[Subcode],mt.[Description],mt.[Variant Code]," +
                                 "mt.[Unit of Measure],mt.[Trigger Function],mt.[Trigger Code],mt.[Price Type],mt.[Price Handling],mt.[Amount _Percent]," +
                                 "mt.[Max_ Selection],mt.[Min_ Selection],mt.[Time Modifier Minutes],mt.[Usage Category]," +
                                 "'' AS [Usage Sub-Category],'' AS [Explanatory Header Text],'' AS [Prompt],0 AS [GrMinSel],0 AS [GrMaxSel] " +
                                 "FROM [" + navCompanyName + "Information Subcode] mt " +
                                 "WHERE mt.[Code]=@id";

                    command.Parameters.AddWithValue("@id", code);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToModifier(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private ReplItemModifier ReaderToModifier(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplItemModifier()
            {
                Id = SQLHelper.GetString(reader["Value"]),
                Code = SQLHelper.GetString(reader["Code"]),
                SubCode = SQLHelper.GetString(reader["Subcode"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ExplanatoryHeaderText = SQLHelper.GetString(reader["Explanatory Header Text"]),
                Prompt = SQLHelper.GetString(reader["Prompt"]),
                UnitOfMeasure = SQLHelper.GetString(reader["Unit of Measure"]),
                VariantCode = SQLHelper.GetString(reader["Variant Code"]),
                TriggerCode = SQLHelper.GetString(reader["Trigger Code"]),
                TriggerFunction = (ItemTriggerFunction)SQLHelper.GetInt32(reader["Trigger Function"]),
                UsageCategory = (ItemUsageCategory)SQLHelper.GetInt32(reader["Usage Category"]),
                Type = (ItemModifierType)SQLHelper.GetInt32(reader["Usage Sub-Category"]),
                PriceType = (ItemModifierPriceType)SQLHelper.GetInt32(reader["Price Type"]),
                AlwaysCharge = (ItemModifierPriceHandling)SQLHelper.GetInt32(reader["Price Handling"]),
                AmountPercent = SQLHelper.GetDecimal(reader, "Amount _Percent"),
                MinSelection = SQLHelper.GetInt32(reader["Min_ Selection"]),
                MaxSelection = SQLHelper.GetInt32(reader["Max_ Selection"]),
                TimeModifierMinutes = SQLHelper.GetDecimal(reader, "Time Modifier Minutes")
            };
        }

        private ItemModifier ReaderToModifier(SqlDataReader reader)
        {
            return new ItemModifier()
            {
                Code = SQLHelper.GetString(reader["Code"]),
                SubCode = SQLHelper.GetString(reader["Subcode"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ExplanatoryHeaderText = SQLHelper.GetString(reader["Explanatory Header Text"]),
                Prompt = SQLHelper.GetString(reader["Prompt"]),
                UnitOfMeasure = SQLHelper.GetString(reader["Unit of Measure"]),
                VariantCode = SQLHelper.GetString(reader["Variant Code"]),
                TriggerCode = SQLHelper.GetString(reader["Trigger Code"]),
                TriggerFunction = (ItemTriggerFunction)SQLHelper.GetInt32(reader["Trigger Function"]),
                UsageCategory = (ItemUsageCategory)SQLHelper.GetInt32(reader["Usage Category"]),
                Type = (ItemModifierType)SQLHelper.GetInt32(reader["Usage Sub-Category"]),
                PriceType = (ItemModifierPriceType)SQLHelper.GetInt32(reader["Price Type"]),
                AlwaysCharge = (ItemModifierPriceHandling)SQLHelper.GetInt32(reader["Price Handling"]),
                AmountPercent = SQLHelper.GetDecimal(reader, "Amount _Percent"),
                MinSelection = SQLHelper.GetInt32(reader["Min_ Selection"]),
                MaxSelection = SQLHelper.GetInt32(reader["Max_ Selection"]),
                TimeModifierMinutes = SQLHelper.GetDecimal(reader, "Time Modifier Minutes")
            };
        }
    }
}
