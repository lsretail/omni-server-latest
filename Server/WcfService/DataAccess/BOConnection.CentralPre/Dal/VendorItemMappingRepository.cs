using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class VendorItemMappingRepository : BaseRepository
    {
        // Key : NavProductId, NavManufacturerId
        const int TABLEID = 27;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public VendorItemMappingRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[No_],mt.[Vendor No_],mt.[Vendor Item No_]";

            // we have vendor information in the Item table but there is also Item Vendor table in NAV, should we use that??
            sqlfrom = " FROM [" + navCompanyName + "Item$437dbf0e-84ff-417a-965d-ed2bb9650972] mt";
        }

        public List<ReplLoyVendorItemMapping> ReplicateEcommVendorItemMapping(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Item$437dbf0e-84ff-417a-965d-ed2bb9650972");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatementWithStoreDist(true, keys, " AND mt.[Vendor No_]<>''", "mt.[No_]", storeId, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplLoyVendorItemMapping> list = new List<ReplLoyVendorItemMapping>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatementWithStoreDist(fullReplication, keys, " AND mt.[Vendor No_]<>''", "mt.[No_]", storeId, true);

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
                                list.Add(ReaderToVendorItemMapping(reader, out lastKey));
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

                                list.Add(new ReplLoyVendorItemMapping()
                                {
                                    NavProductId = par[0],
                                    NavManufacturerId = par[1],
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
                                    list.Add(ReaderToVendorItemMapping(reader, out string ts));
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

        private ReplLoyVendorItemMapping ReaderToVendorItemMapping(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplLoyVendorItemMapping()
            {
                NavManufacturerId = SQLHelper.GetString(reader["Vendor No_"]),
                NavProductId = SQLHelper.GetString(reader["No_"]),
                NavManufacturerItemId = SQLHelper.GetString(reader["Vendor Item No_"]),

                // fixed values
                Deleted = false,
                DisplayOrder = 1,
                IsFeaturedProduct = true
            };
        }
    }
}
