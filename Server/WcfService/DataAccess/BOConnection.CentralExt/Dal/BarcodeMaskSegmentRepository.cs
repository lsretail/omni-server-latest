using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;

namespace LSOmni.DataAccess.BOConnection.CentralExt.Dal
{
    public class BarcodeMaskSegmentRepository : BaseRepository 
    {
        // Key: Mask Entry No.,Segment No
        const int TABLEID = 99001480;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public BarcodeMaskSegmentRepository(BOConfiguration config, Version version) : base(config, version)
        {
            sqlcolumns = "mt.[Mask Entry No_],mt.[Segment No],mt.[Length],mt.[Type],mt.[Decimals],mt.[Char],ROW_NUMBER() OVER (ORDER BY [Mask Entry No_],[Segment No]) AS Id";

            sqlfrom = " FROM [" + navCompanyName + "LSC Barcode Mask Segment$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public List<ReplBarcodeMaskSegment> ReplicateBarcodeMaskSegments(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Barcode Mask Segment$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplBarcodeMaskSegment> list = new List<ReplBarcodeMaskSegment>();

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
                                list.Add(ReaderToBarcodeMaskSegment(reader, out lastKey));
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

                                list.Add(new ReplBarcodeMaskSegment()
                                {
                                    MaskId = Convert.ToInt32(par[0]),
                                    Number = Convert.ToInt32(par[1]),
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
                                    list.Add(ReaderToBarcodeMaskSegment(reader, out string ts));
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

        public List<ReplBarcodeMaskSegment> ReplicateBarcodeMaskSegmentsTM(int batchSize, bool fullReplication, ref string lastKey, ref int recordsRemaining)
        {
            ProcessLastKey(lastKey, out string mainKey, out string delKey);
            List<JscKey> keys = GetPrimaryKeys("LSC Barcode Mask Segment$5ecfc871-5d82-43f1-9c54-59685e82318d");

            string sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            recordsRemaining = GetRecordCountTM(mainKey, sql, keys);

            List<JscActions> actions = LoadDeleteActions(fullReplication, TABLEID, "LSC Barcode Mask Segment$5ecfc871-5d82-43f1-9c54-59685e82318d", keys, batchSize, ref delKey);
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(true, keys, true);

            List<ReplBarcodeMaskSegment> list = new List<ReplBarcodeMaskSegment>();
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
                            list.Add(ReaderToBarcodeMaskSegment(reader, out mainKey));
                            cnt++;
                        }
                        reader.Close();
                        recordsRemaining -= cnt;
                    }

                    foreach (JscActions act in actions)
                    {
                        string[] par = act.ParamValue.Split(';');
                        if (par.Length < 2 || par.Length != keys.Count)
                            continue;

                        list.Add(new ReplBarcodeMaskSegment()
                        {
                            MaskId = Convert.ToInt32(par[0]),
                            Number = Convert.ToInt32(par[1]),
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

        private ReplBarcodeMaskSegment ReaderToBarcodeMaskSegment(SqlDataReader reader, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplBarcodeMaskSegment()
            {
                Id = SQLHelper.GetInt32(reader["Id"]),
                Number = SQLHelper.GetInt32(reader["Segment No"]),
                MaskId = SQLHelper.GetInt32(reader["Mask Entry No_"]),
                Length = SQLHelper.GetInt32(reader["Length"]),
                SegmentType = SQLHelper.GetInt32(reader["Type"]),
                Decimals = SQLHelper.GetInt32(reader["Decimals"]),
                Char = SQLHelper.GetString(reader["Char"])
            };
        }
    }
}
 