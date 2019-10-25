using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class UnitOfMeasureRepository : BaseRepository
    {
        // Key: Code
        const int TABLEID = 204;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public UnitOfMeasureRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Code],mt.[Description]";

            sqlfrom = " FROM [" + navCompanyName + "Unit of Measure] mt";
        }

        public List<ReplUnitOfMeasure> ReplicateUnitOfMeasure(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Unit of Measure");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplUnitOfMeasure> list = new List<ReplUnitOfMeasure>();

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
                                list.Add(ReaderToUnitOfMeasure(reader, out lastKey));
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
                                list.Add(new ReplUnitOfMeasure()
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
                                    list.Add(ReaderToUnitOfMeasure(reader, out string ts));
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

        public UnitOfMeasure UnitOfMeasureGetById(string id)
        {
            UnitOfMeasure uom = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom + " WHERE mt.[Code]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            uom = ReaderToUnitOfMeasure(reader);
                        }
                    }
                    connection.Close();
                }
            }
            return uom;
        }

        private ReplUnitOfMeasure ReaderToUnitOfMeasure(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplUnitOfMeasure()
            {
                Id = SQLHelper.GetString(reader["Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ShortDescription = SQLHelper.GetString(reader["Description"])
            };
        }

        private UnitOfMeasure ReaderToUnitOfMeasure(SqlDataReader reader)
        {
            return new UnitOfMeasure()
            {
                Id = SQLHelper.GetString(reader["Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ShortDescription = SQLHelper.GetString(reader["Code"])
            };
        }
    }
}
 