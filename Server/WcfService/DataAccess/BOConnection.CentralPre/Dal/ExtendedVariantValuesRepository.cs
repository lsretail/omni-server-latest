using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class ExtendedVariantValuesRepository : BaseRepository
    {
        // Key: Framework Code, Item No., Code, Value, Extension
        const int TABLEID = 10001413;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public ExtendedVariantValuesRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Framework Code],mt.[Item No_],mt.[Dimension],mt.[Code],mt.[Value],mt.[Logical Order]";

            sqlfrom = " FROM [" + navCompanyName + "LSC Extd_ Variant Values$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public List<ReplExtendedVariantValue> ReplicateExtendedVariantValues(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Extd_ Variant Values$5ecfc871-5d82-43f1-9c54-59685e82318d");

            string whereaddon = (string.IsNullOrWhiteSpace(storeId)) ? " AND mt.[Framework Code]<>'' AND mt.[Item No_]<>''" : string.Empty;

            // get records remaining
            string sql = string.Empty;
            string col = sqlcolumns;

            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatementWithStoreDist(true, keys, whereaddon, "mt.[Item No_]", storeId, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<JscActions> actions2 = LoadActions(fullReplication, 10001412, batchSize, ref lastKey, ref recordsRemaining);

            List<ReplExtendedVariantValue> list = new List<ReplExtendedVariantValue>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandText = "SELECT 1 FROM sys.columns WHERE [Name]='Logical Order' AND Object_ID=Object_ID('[" + navCompanyName + "LSC Extd_ Variant Dimensions$5ecfc871-5d82-43f1-9c54-59685e82318d]')";
                    bool hascol = SQLHelper.GetBool(command.ExecuteScalar());

                    if (hascol)
                    {
                        col += ",(SELECT vd.[Logical Order] FROM [" + navCompanyName + "LSC Extd_ Variant Dimensions$5ecfc871-5d82-43f1-9c54-59685e82318d] vd " +
                                      "WHERE vd.[Framework Code]=mt.[Framework Code] AND vd.[Dimension No_]=mt.[Dimension] AND vd.[Item]='') AS DOrder";
                    }
                    command.CommandText = GetSQL(fullReplication, batchSize) + col + sqlfrom + GetWhereStatementWithStoreDist(fullReplication, keys, whereaddon, "mt.[Item No_]", storeId, true);

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
                                list.Add(ReaderToExtendedVariantValue(reader, hascol, out lastKey));
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
                                if (par.Length < 4 || par.Length != keys.Count)
                                    continue;

                                list.Add(new ReplExtendedVariantValue()
                                {
                                    FrameworkCode = par[0],
                                    ItemId = par[1],
                                    Code = par[2],
                                    Value = par[3],
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
                                    list.Add(ReaderToExtendedVariantValue(reader, hascol, out string ts));
                                }
                                reader.Close();
                            }
                            first = false;
                        }

                        command.CommandText = command.CommandText.Replace("AND mt.[Value]=@3 ", "");
                        command.CommandText = command.CommandText.Replace("@4", "@3");

                        keys.RemoveAt(3);
                        foreach (JscActions act in actions2)
                        {
                            if (act.Type == DDStatementType.Delete)
                            {
                                continue;
                            }

                            if (SetWhereValues(command, act, keys, first) == false)
                                continue;

                            TraceSqlCommand(command);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    list.Add(ReaderToExtendedVariantValue(reader, hascol, out string ts));
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

        public List<VariantExt> VariantRegGetByItemId(string itemId, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            List<VariantExt> list = new List<VariantExt>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandText = "SELECT 1 FROM sys.columns WHERE [Name]='Logical Order' AND Object_ID=Object_ID('[" + navCompanyName + "LSC Extd_ Variant Dimensions$5ecfc871-5d82-43f1-9c54-59685e82318d]')";
                    bool hascol = SQLHelper.GetBool(command.ExecuteScalar());

                    command.CommandText = "SELECT DISTINCT mt.[Code],mt.[Dimension]";
                    if (hascol)
                    {
                        command.CommandText += ",(SELECT vd.[Logical Order] FROM [" + navCompanyName + "LSC Extd_ Variant Dimensions$5ecfc871-5d82-43f1-9c54-59685e82318d] vd " +
                                      "WHERE vd.[Framework Code]=mt.[Framework Code] AND vd.[Dimension No_]=mt.[Dimension] AND vd.[Item]='') AS DOrder";
                    }
                    command.CommandText += sqlfrom + " WHERE mt.[Item No_]='" + itemId + "'";
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToVariantExt(reader, itemId, hascol));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<ReplExtendedVariantValue> VariantRegGetValuesByVarCode(string itemid, string varcode)
        {
            List<ReplExtendedVariantValue> list = new List<ReplExtendedVariantValue>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetSQL(false, 0) + sqlcolumns + sqlfrom +
                        string.Format(" WHERE mt.[Item No_]='{0}' AND mt.[Code]='{1}' ORDER BY mt.[Logical Order]", itemid, varcode);
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToExtendedVariantValue(reader, false, out string ts));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private ReplExtendedVariantValue ReaderToExtendedVariantValue(SqlDataReader reader, bool dorder, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            ReplExtendedVariantValue extvar = new ReplExtendedVariantValue()
            {
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                Dimensions = SQLHelper.GetString(reader["Dimension"]),
                Code = SQLHelper.GetString(reader["Code"]),
                Value = SQLHelper.GetString(reader["Value"]),
                FrameworkCode = SQLHelper.GetString(reader["Framework Code"]),
                LogicalOrder = SQLHelper.GetInt32(reader["Logical Order"])
            };

            if (dorder)
                extvar.DimensionLogicalOrder = SQLHelper.GetInt32(reader["DOrder"]);

            return extvar;
        }

        private VariantExt ReaderToVariantExt(SqlDataReader reader, string itemid, bool dorder)
        {
            VariantExt extvar = new VariantExt()
            {
                ItemId = itemid,
                Dimension = SQLHelper.GetString(reader["Dimension"]),
                Code = SQLHelper.GetString(reader["Code"])
            };

            if (dorder)
                extvar.DisplayOrder = SQLHelper.GetInt32(reader["DOrder"]);

            foreach (ReplExtendedVariantValue var in VariantRegGetValuesByVarCode(extvar.ItemId, extvar.Code))
            {
                extvar.Values.Add(new DimValue()
                {
                    Value = var.Value,
                    DisplayOrder = var.LogicalOrder
                });
            }
            return extvar;
        }
    }
}
