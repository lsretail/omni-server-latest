using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;

namespace LSOmni.DataAccess.BOConnection.CentralExt.Dal
{
    public class StaffRepository : BaseRepository
    {
        // Key: ID
        const int TABLEID = 99001461;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public StaffRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[ID],mt.[First Name],mt.[Last Name],mt.[Name on Receipt],mt.[Store No_],mt.[Password]," +
                         "mt.[Change Password],mt.[Blocked],mt.[Date to Be Blocked],mt.[Inventory Main Menu],mt.[Inventory Active]";

            sqlfrom = " FROM [" + navCompanyName + "LSC Staff$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public List<ReplStaff> ReplicateStaff(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            SQLHelper.CheckForSQLInjection(storeId);
            List<JscKey> keys = GetPrimaryKeys("LSC Staff$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // this will replicate all records and ignores the batchSize value
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom;
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, null, ref maxKey);

            sql = GetSQL(fullReplication, 0) + sqlcolumns + sqlfrom +
                        GetWhereStatement(fullReplication, keys, " AND (mt.[Store No_] = '" + storeId + "' OR mt.[Store No_] = '')", false) +
                        " UNION " +
                        GetSQL(fullReplication, 0) + sqlcolumns +
                        " FROM [" + navCompanyName + "LSC STAFF Store Link$5ecfc871-5d82-43f1-9c54-59685e82318d] sl" +
                        " LEFT JOIN [" + navCompanyName + "LSC Staff$5ecfc871-5d82-43f1-9c54-59685e82318d] mt ON sl.[Staff ID]=mt.[ID]" +
                        GetWhereStatement(fullReplication, keys, " AND sl.[Store No_]='" + storeId + "'", false);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, 0, ref lastKey, ref recordsRemaining);
            List<ReplStaff> list = new List<ReplStaff>();

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
                                list.Add(ReaderToStaff(reader, out lastKey));
                                cnt++;
                            }
                            reader.Close();
                        }
                        // as we replicate everything always in full replication
                        lastKey = maxKey;
                        recordsRemaining = 0;
                    }
                    else
                    {
                        bool first = true;
                        foreach (JscActions act in actions)
                        {
                            if (act.Type == DDStatementType.Delete)
                            {
                                list.Add(new ReplStaff()
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
                                    list.Add(ReaderToStaff(reader, out string ts));
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


        private ReplStaff ReaderToStaff(SqlDataReader reader, out string timestamp)
        {
            ReplStaff staff = new ReplStaff()
            {
                Id = SQLHelper.GetString(reader["ID"]),
                StoreID = SQLHelper.GetString(reader["Store No_"]),
                NameOnReceipt = SQLHelper.GetString(reader["Name on Receipt"]),
                Password = SQLHelper.GetString(reader["Password"]),
                FirstName = SQLHelper.GetString(reader["First Name"]),
                LastName = SQLHelper.GetString(reader["Last Name"]),
                BlockingDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Date to Be Blocked"]), config.IsJson),
                ChangePassword = SQLHelper.GetInt32(reader["Change Password"]),
                Blocked = SQLHelper.GetInt32(reader["Blocked"]),
                InventoryActive = SQLHelper.GetBool(reader["Inventory Active"]),
                InventoryMainMenu = SQLHelper.GetString(reader["Inventory Main Menu"])
            };

            staff.Name = staff.FirstName + " " + staff.LastName;
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);
            return staff;
        }
    }
}