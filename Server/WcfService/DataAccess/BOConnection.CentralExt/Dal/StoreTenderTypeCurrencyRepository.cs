using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Pos.Replication;

namespace LSOmni.DataAccess.BOConnection.CentralExt.Dal
{
    public class StoreTenderTypeCurrencyRepository : BaseRepository
    {
        // Key : Store No., Tender Type Code, Currency Code
        const int TABLEID = 99001636;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public StoreTenderTypeCurrencyRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Store No_],mt.[Tender Type Code],mt.[Currency Code],mt.[Description]";

            sqlfrom = " FROM [" + navCompanyName + "LSC Tender Type Currency Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public List<ReplStoreTenderTypeCurrency> ReplicateStoreTenderTypeCurrency(string storeId, int batchSize, bool fullReplication,  ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Tender Type Currency Setup$5ecfc871-5d82-43f1-9c54-59685e82318d");

            SQLHelper.CheckForSQLInjection(storeId);
            string where = " AND mt.[Store No_]='" + storeId + "'";

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, where, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplStoreTenderTypeCurrency> list = new List<ReplStoreTenderTypeCurrency>();

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
                                list.Add(ReaderToStoreTenderTypeCurrency(reader, out lastKey));
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
                                if (par.Length < 3 || par.Length != keys.Count)
                                    continue;

                                list.Add(new ReplStoreTenderTypeCurrency()
                                {
                                    StoreID = par[0],
                                    TenderTypeId = par[1],
                                    CurrencyCode = par[2],
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
                                    list.Add(ReaderToStoreTenderTypeCurrency(reader, out string ts));
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

        private ReplStoreTenderTypeCurrency ReaderToStoreTenderTypeCurrency(SqlDataReader reader, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplStoreTenderTypeCurrency()
            {
                StoreID = SQLHelper.GetString(reader["Store No_"]),
                TenderTypeId = SQLHelper.GetString(reader["Tender Type Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                CurrencyCode = SQLHelper.GetString(reader["Currency Code"])
            };
        }
    }
}
 