using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;

namespace LSOmni.DataAccess.BOConnection.CentralExt.Dal
{
    public class BarcodeGS1Repository : BaseRepository
    {
        // Key: Entry No_
        const int TABLEID = 10000936;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public BarcodeGS1Repository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Type],mt.[Identifier],mt.[Section Type],mt.[Section Size],mt.[Identifier Size]," +
                         "mt.[Section Mapping],mt.[Mapping Starting Char],mt.[Preferred Sequence]," +
                         "mt.[Decimals],mt.[Value Type],mt.[Value],mt.[Value (Dec)],mt.[Value (Date)]";

            sqlfrom = " FROM [" + navCompanyName + "LSC GS1DataBar Barcode Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public List<ReplGS1BarcodeSetup> ReplicateBarcodeGS1Setup(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC GS1DataBar Barcode Setup$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplGS1BarcodeSetup> list = new List<ReplGS1BarcodeSetup>();

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
                                list.Add(ReaderToBarcodeSetup(reader, out lastKey));
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

                                list.Add(new ReplGS1BarcodeSetup()
                                {
                                    Type = Convert.ToInt32(par[0]),
                                    Identifier = par[1],
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
                                    list.Add(ReaderToBarcodeSetup(reader, out string ts));
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

        private ReplGS1BarcodeSetup ReaderToBarcodeSetup(SqlDataReader reader, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplGS1BarcodeSetup()
            {
                Type = SQLHelper.GetInt32(reader["Type"]),
                Identifier = SQLHelper.GetString(reader["Identifier"]),
                SectionType = SQLHelper.GetInt32(reader["Section Type"]),
                SectionSize = SQLHelper.GetInt32(reader["Section Size"]),
                IdentifierSize = SQLHelper.GetInt32(reader["Identifier Size"]),
                SectionMapping = SQLHelper.GetInt32(reader["Section Mapping"]),
                MappingStartingChar = SQLHelper.GetInt32(reader["Mapping Starting Char"]),
                PreferredSequence = SQLHelper.GetInt32(reader["Preferred Sequence"]),
                Decimals = SQLHelper.GetDecimal(reader, "Decimals"),
                ValueType = SQLHelper.GetInt32(reader["Value Type"]),
                Value = SQLHelper.GetString(reader["Value"]),
                ValueDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Value (Date)"]), config.IsJson),
                ValueDec = SQLHelper.GetDecimal(reader, "Value (Dec)")
            };
        }
    }
}
