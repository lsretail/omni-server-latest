using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Hagar;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class HagarItemWebExtRepository : BaseRepository
    {
        // Key: Code
        const int TABLEID = 50203;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public HagarItemWebExtRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Item No_],mt.[Variant Code],mt.[Data Type],mt.[Information]";

            sqlfrom = " FROM [" + navCompanyName + "NVL Item Web Extended Info$968ac057-ecff-4336-8c68-b4b64e4b20f7] mt";
        }

        public List<ReplItemWebExtended> ReplicateItemWebExtended(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("NVL Item Web Extended Info$968ac057-ecff-4336-8c68-b4b64e4b20f7");
            string prevlastkey = lastKey;

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[Item No_]", storeId, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplItemWebExtended> list = new List<ReplItemWebExtended>();

            // get records
            sql = GetSQL(fullReplication, batchSize, true, true) + sqlcolumns + sqlfrom + GetWhereStatementWithStoreDist(fullReplication, keys, "mt.[Item No_]", storeId, true);

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
                                list.Add(ReaderToWebExt(reader, out lastKey));
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
                                if (par.Length < 3 || par.Length != keys.Count)
                                    continue;

                                list.Add(new ReplItemWebExtended()
                                {
                                    ItemId = par[0],
                                    VariantId = par[1],
                                    DataType = (WebExtDataType)Convert.ToInt32(par[2]),
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
                                    list.Add(ReaderToWebExt(reader, out string ts));
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

        private ReplItemWebExtended ReaderToWebExt(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplItemWebExtended()
            {
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                DataType = (WebExtDataType)SQLHelper.GetInt32(reader["Data Type"]),
                Information = SQLHelper.GetString(reader["Information"])
            };
        }
    }
}