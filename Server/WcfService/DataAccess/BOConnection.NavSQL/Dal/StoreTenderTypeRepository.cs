using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Replication;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class StoreTenderTypeRepository : BaseRepository
    {
        // Key : Store No., Code
        const int TABLEID = 99001462;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        //FUNCTION = 0  AND  FOREIGNCURRENCY = 0  ER CASH
        //FUNCTION = 0  AND  FOREIGNCURRENCY = 1  ER CURRENCY
        //FUNCTION = 1 er card

        public StoreTenderTypeRepository() : base()
        {
            sqlcolumns = "mt.[Store No_],mt.[Code],mt.[Description],mt.[Function],mt.[Valid on Mobile POS]," +
                         "mt.[Counting Required],mt.[Change Tend_ Code],mt.[Above Min_ Change Tender Type]," +
                         "mt.[Min_ Change],mt.[Rounding],mt.[Rounding To],mt.[Overtender Allowed],mt.[Overtender Max_ Amt_]," +
                         "mt.[Undertender Allowed],mt.[Drawer Opens],mt.[Return_Minus Allowed],mt.[Foreign Currency]";

            sqlfrom = " FROM [" + navCompanyName + "Tender Type] mt";
        }

        public List<ReplStoreTenderType> ReplicateStoreTenderType(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Tender Type");

            SQLHelper.CheckForSQLInjection(storeId);
            string where = " AND mt.[Store No_]='" + storeId + "'";

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)";
                if (batchSize > 0)
                {
                    sql += sqlfrom + GetWhereStatement(true, keys, where, false);
                }
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, (batchSize > 0) ? keys : null, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplStoreTenderType> list = new List<ReplStoreTenderType>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(fullReplication, keys, where, true);

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
                                list.Add(ReaderToStoreTenderType(reader, out lastKey));
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

                                list.Add(new ReplStoreTenderType()
                                {
                                    StoreID = par[0],
                                    TenderTypeId = par[1],
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
                                    list.Add(ReaderToStoreTenderType(reader, out string ts));
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

        private ReplStoreTenderType ReaderToStoreTenderType(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplStoreTenderType()
            {
                StoreID = SQLHelper.GetString(reader["Store No_"]),
                TenderTypeId = SQLHelper.GetString(reader["Code"]),
                Name = SQLHelper.GetString(reader["Description"]),
                TenderFunction = SQLHelper.GetInt32(reader["Function"]),
                ChangeTenderId = SQLHelper.GetString(reader["Change Tend_ Code"]),
                AboveMinimumTenderId = SQLHelper.GetString(reader["Above Min_ Change Tender Type"]),
                MinimumChangeAmount = SQLHelper.GetDecimal(reader["Min_ Change"]),
                Rounding = SQLHelper.GetDecimal(reader["Rounding To"]),
                RoundingMethode = SQLHelper.GetInt32(reader["Rounding"]),
                ValidOnMobilePOS = SQLHelper.GetInt32(reader["Valid on Mobile POS"]),
                ReturnAllowed = SQLHelper.GetInt32(reader["Return_Minus Allowed"]),
                ForeignCurrency = SQLHelper.GetInt32(reader["Foreign Currency"]),
                MaximumOverTenderAmount = SQLHelper.GetDecimal(reader["Overtender Max_ Amt_"]),
                CountingRequired = SQLHelper.GetInt32(reader["Counting Required"]),
                AllowOverTender = SQLHelper.GetInt32(reader["Overtender Allowed"]),
                AllowUnderTender = SQLHelper.GetInt32(reader["Undertender Allowed"]),
                OpenDrawer = SQLHelper.GetInt32(reader["Drawer Opens"])
            };
        }
    }
}
 