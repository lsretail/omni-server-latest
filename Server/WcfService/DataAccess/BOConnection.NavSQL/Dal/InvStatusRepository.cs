using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class InvStatusRepository : BaseRepository
    {
        public InvStatusRepository(BOConfiguration config) : base(config)
        {
        }

        public virtual List<ReplInvStatus> ReplicateInventoryStatus(string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            string sqlcolumns = "mt.[timestamp],mt.[Item No_],mt.[Variant Code],mt.[Store No_],mt.[Net Inventory]";
            string sqlfrom = " FROM [" + navCompanyName + "Inventory Lookup Table] mt";

            SQLHelper.CheckForSQLInjection(storeId);
            string sqlwhere = (string.IsNullOrEmpty(storeId) ? string.Empty : string.Format(" AND mt.[Store No_]='{0}'", storeId));

            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Inventory Lookup Table");

            // get records remaining
            string sql = string.Empty;
            sql = "SELECT COUNT(*)";
            if (batchSize > 0)
            {
                sql += sqlfrom + GetWhereStatement(true, keys, sqlwhere, false);
            }
            recordsRemaining = GetRecordCount(99001608, lastKey, sql, (batchSize > 0) ? keys : null, ref maxKey);

            List<ReplInvStatus> list = new List<ReplInvStatus>();

            // get records
            sql = GetSQL(true, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(true, keys, sqlwhere, true);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = sql;

                    JscActions act = new JscActions(lastKey);
                    SetWhereValues(command, act, keys, true, true);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int cnt = 0;
                        while (reader.Read())
                        {
                            lastKey = ByteArrayToString(reader["timestamp"] as byte[]);

                            list.Add(new ReplInvStatus()
                            {
                                ItemId = SQLHelper.GetString(reader["Item No_"]),
                                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                                StoreId = SQLHelper.GetString(reader["Store No_"]),
                                Quantity = SQLHelper.GetDecimal(reader["Net Inventory"]),
                                IsDeleted = false
                            });
                            cnt++;
                        }
                        reader.Close();
                        recordsRemaining -= cnt;
                    }
                    if (recordsRemaining <= 0)
                        lastKey = maxKey;   // this should be the highest PreAction id;
                    connection.Close();
                }
            }

            // just in case something goes too far
            if (recordsRemaining < 0)
                recordsRemaining = 0;

            return list;
        }
    }
}
