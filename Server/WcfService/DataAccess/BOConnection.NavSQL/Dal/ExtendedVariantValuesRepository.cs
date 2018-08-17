using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class ExtendedVariantValuesRepository : BaseRepository
    {
        // Key: Framework Code, Item No., Code, Value, Extension
        const int TABLEID = 10001413;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public ExtendedVariantValuesRepository() : base()
        {
            sqlcolumns = "mt.[Framework Code],mt.[Item No_],mt.[Dimension],mt.[Code],mt.[Value],mt.[Logical Order]";

            sqlfrom = " FROM [" + navCompanyName + "Extended Variant Values] mt";
        }

        public List<ReplExtendedVariantValue> ReplicateExtendedVariantValues(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Extended Variant Values");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)";
                if (batchSize > 0)
                {
                    sql += sqlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[Item No_]", storeId, false);
                }
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, (batchSize > 0) ? keys : null, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplExtendedVariantValue> list = new List<ReplExtendedVariantValue>();

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
                                list.Add(ReaderToExtendedVariantValue(reader, out lastKey));
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

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    list.Add(ReaderToExtendedVariantValue(reader, out string ts));
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

        public List<VariantExt> VariantRegGetByItemId(string itemId)
        {
            List<VariantExt> list = new List<VariantExt>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT DISTINCT mt.[Code],mt.[Dimension]" + sqlfrom + " WHERE mt.[Item No_]='" + itemId + "'";
                    TraceIt(command.CommandText);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToVariantExt(reader, itemId));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
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
                    TraceIt(command.CommandText);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToExtendedVariantValue(reader, out string ts));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private ReplExtendedVariantValue ReaderToExtendedVariantValue(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplExtendedVariantValue()
            {
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                Dimensions = SQLHelper.GetString(reader["Dimension"]),
                Code = SQLHelper.GetString(reader["Code"]),
                Value = SQLHelper.GetString(reader["Value"]),
                FrameworkCode = SQLHelper.GetString(reader["Framework Code"]),
                LogicalOrder = SQLHelper.GetInt32(reader["Logical Order"])
            };
        }

        private VariantExt ReaderToVariantExt(SqlDataReader reader, string itemid)
        {
            VariantExt extvar = new VariantExt()
            {
                ItemId = itemid,
                Dimension = SQLHelper.GetString(reader["Dimension"]),
                Code = SQLHelper.GetString(reader["Code"])
            };

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