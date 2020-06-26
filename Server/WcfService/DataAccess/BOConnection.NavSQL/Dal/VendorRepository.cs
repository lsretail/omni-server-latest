using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class VendorRepository : BaseRepository
    {
        // Key : No.
        const int TABLEID = 23;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public VendorRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[No_],mt.[Name],mt.[Blocked],mt.[Last Date Modified]";

            sqlfrom = " FROM [" + navCompanyName + "Vendor] mt";
        }

        public List<ReplVendor> ReplicateVendor(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Vendor");
            string from = sqlfrom;

            if (string.IsNullOrEmpty(storeId) == false)
                from += " LEFT JOIN [" + navCompanyName + "Item] it ON it.[Vendor No_]=mt.[No_]";

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                if (string.IsNullOrEmpty(storeId))
                    sql = "SELECT COUNT(*)" + from + GetWhereStatement(true, keys, false);
                else
                    sql = "SELECT COUNT(DISTINCT mt.[No_])" + from + GetWhereStatementWithStoreDist(true, keys, "it.[No_]", storeId, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplVendor> list = new List<ReplVendor>();

            // get records
            if (string.IsNullOrEmpty(storeId))
                sql = GetSQL(fullReplication, batchSize) + sqlcolumns + from + GetWhereStatement(fullReplication, keys, true);
            else
                sql = GetSQL(fullReplication, batchSize, true, true) + sqlcolumns + from + GetWhereStatementWithStoreDist(fullReplication, keys, "it.[No_]", storeId, true);

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
                                list.Add(ReaderToVendor(reader, out lastKey));
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
                                list.Add(new ReplVendor()
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
                                    list.Add(ReaderToVendor(reader, out string ts));
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

        private ReplVendor ReaderToVendor(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplVendor()
            {
                Id = SQLHelper.GetString(reader["No_"]),
                Name = SQLHelper.GetString(reader["Name"]),
                Blocked = SQLHelper.GetBool(reader["Blocked"]),
                UpdatedOnUtc = SQLHelper.GetDateTime(reader["Last Date Modified"]),

                // fixed values
                AllowCustomersToSelectPageSize = false,
                DisplayOrder = 1,
                ManufacturerTemplateId = 1,
                PageSize = 4,
                PictureId = 0,
                Published = true
            };
        }
    }
}
