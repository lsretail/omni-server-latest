using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class BarcodeMaskRepository : BaseRepository 
    {
        // Key: Entry No_
        const int TABLEID = 99001459;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public BarcodeMaskRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Entry No_],mt.[Mask],mt.[Description],mt.[Type],mt.[Prefix],mt.[Symbology],mt.[Number Series]";

            sqlfrom = " FROM [" + navCompanyName + "LSC Barcode Mask$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public List<ReplBarcodeMask> ReplicateBarcodeMasks(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Barcode Mask$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplBarcodeMask> list = new List<ReplBarcodeMask>();

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
                                list.Add(ReaderToBarcodeMask(reader, out lastKey));
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
                                list.Add(new ReplBarcodeMask()
                                {
                                    Id = Convert.ToInt32(act.ParamValue),
                                    Del = true
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
                                    list.Add(ReaderToBarcodeMask(reader, out string ts));
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

        private ReplBarcodeMask ReaderToBarcodeMask(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplBarcodeMask()
            {
                Id = SQLHelper.GetInt32(reader["Entry No_"]),
                Mask = SQLHelper.GetString(reader["Mask"]),
                Description = SQLHelper.GetString(reader["Description"]),
                MaskType = SQLHelper.GetInt32(reader["Type"]),
                Prefix = SQLHelper.GetString(reader["Prefix"]),
                Symbology = SQLHelper.GetInt32(reader["Symbology"]),
                NumberSeries = SQLHelper.GetString(reader["Number Series"])
            };
        }
    }
}
 