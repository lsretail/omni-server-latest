using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.DataAccess.BOConnection.CentralExt.Dal
{
    public class AttributeRepository : BaseRepository
    {
        // Key: Code
        const int TABLEID = 10000784;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public AttributeRepository(BOConfiguration config, Version version) : base(config, version)
        {
            sqlcolumns = "mt.[Code],mt.[Description],mt.[Value Type],mt.[Default Value]";

            sqlfrom = " FROM [" + navCompanyName + "LSC Attribute$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public List<ReplAttribute> ReplicateEcommAttribute(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Attribute$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplAttribute> list = new List<ReplAttribute>();

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
                                list.Add(ReaderToEcommAttribute(reader, out lastKey));
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
                                list.Add(new ReplAttribute()
                                {
                                    Code = act.ParamValue,
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
                                    list.Add(ReaderToEcommAttribute(reader, out string ts));
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

        public List<ReplAttribute> ReplicateEcommAttributeTM(int batchSize, bool fullReplication, ref string lastKey, ref int recordsRemaining)
        {
            ProcessLastKey(lastKey, out string mainKey, out string delKey);
            List<JscKey> keys = GetPrimaryKeys("LSC Attribute$5ecfc871-5d82-43f1-9c54-59685e82318d");

            string sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            recordsRemaining = GetRecordCountTM(mainKey, sql, keys);

            List<JscActions> actions = LoadDeleteActions(fullReplication, TABLEID, "LSC Attribute$5ecfc871-5d82-43f1-9c54-59685e82318d", keys, batchSize, ref delKey);
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(true, keys, true);

            List<ReplAttribute> list = new List<ReplAttribute>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = sql;

                    JscActions actKey = new JscActions(mainKey);
                    SetWhereValues(command, actKey, keys, true, true);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int cnt = 0;
                        while (reader.Read())
                        {
                            list.Add(ReaderToEcommAttribute(reader, out mainKey));
                            cnt++;
                        }
                        reader.Close();
                        recordsRemaining -= cnt;
                    }

                    foreach (JscActions act in actions)
                    {
                        list.Add(new ReplAttribute()
                        {
                            Code = act.ParamValue,
                            IsDeleted = true
                        });
                    }
                    connection.Close();
                }
            }

            // just in case something goes too far
            if (recordsRemaining < 0)
                recordsRemaining = 0;

            lastKey = $"R={mainKey};D={delKey};";
            return list;
        }

        private ReplAttribute ReaderToEcommAttribute(SqlDataReader reader, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplAttribute()
            {
                Code = SQLHelper.GetString(reader["Code"]),
                DefaultValue = SQLHelper.GetString(reader["Default Value"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ValueType = SQLHelper.GetInt32(reader["Value Type"])
            };
        }
    }
}
