using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.DataAccess.BOConnection.CentralExt.Dal
{
    public class DataTranslationRepository : BaseRepository
    {
        // Key : Translation ID, Key, Language Code
        const int TABLEID = 10000971;
        const int TABLEHTMLID = 10001410;
        const int TABLEDEALHTMLID = 10000825;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        private string sqlHtmlcolumns = string.Empty;
        private string sqlHtmlfrom = string.Empty;

        private string sqlDealHtmlcolumns = string.Empty;
        private string sqlDealHtmlfrom = string.Empty;

        public DataTranslationRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Key],mt.[Language Code],mt.[Translation],mt.[Translation ID]";
            sqlfrom = " FROM [" + navCompanyName + "LSC Data Translation$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";

            sqlHtmlcolumns = "mt.[Item No_],mt.[Language],mt.[Html],mt.[URL]";
            sqlHtmlfrom = " FROM [" + navCompanyName + "LSC Item HTML ML$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";

            sqlDealHtmlcolumns = "mt.[Offer No_],mt.[Language],mt.[Html],mt.[URL]";
            sqlDealHtmlfrom = " FROM [" + navCompanyName + "LSC Offer HTML ML$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public List<ReplDataTranslation> ReplicateEcommDataTranslation(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Data Translation$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplDataTranslation> list = new List<ReplDataTranslation>();

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
                                list.Add(ReaderToDataTranslation(reader, out lastKey));
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

                                list.Add(new ReplDataTranslation()
                                {
                                    TranslationId = par[0],
                                    Key = par[1],
                                    LanguageCode = par[2],
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
                                    list.Add(ReaderToDataTranslation(reader, out string ts));
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

        public List<ReplDataTranslation> ReplicateEcommItemHtmlTranslation(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Item HTML ML$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlHtmlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[Item No_]", storeId, false);
            }
            recordsRemaining = GetRecordCount(TABLEHTMLID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEHTMLID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplDataTranslation> list = new List<ReplDataTranslation>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlHtmlcolumns + sqlHtmlfrom + GetWhereStatementWithStoreDist(fullReplication, keys, "mt.[Item No_]", storeId, true);

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
                                list.Add(ReaderToHtmlTranslation(reader, out lastKey));
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

                                list.Add(new ReplDataTranslation()
                                {
                                    TranslationId = par[0],
                                    LanguageCode = par[1],
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
                                    list.Add(ReaderToHtmlTranslation(reader, out string ts));
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

        public List<ReplDataTranslation> ReplicateEcommDealHtmlTranslation(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Offer HTML ML$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlDealHtmlfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(TABLEDEALHTMLID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEDEALHTMLID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplDataTranslation> list = new List<ReplDataTranslation>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlDealHtmlcolumns + sqlDealHtmlfrom + GetWhereStatement(fullReplication, keys, true);

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
                                list.Add(ReaderToDealHtmlTranslation(reader, out lastKey));
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

                                list.Add(new ReplDataTranslation()
                                {
                                    TranslationId = par[0],
                                    LanguageCode = par[1],
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
                                    list.Add(ReaderToDealHtmlTranslation(reader, out string ts));
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

        public List<ReplDataTranslationLangCode> ReplicateEcommDataTranslationLangCode()
        {
            List<ReplDataTranslationLangCode> list = new List<ReplDataTranslationLangCode>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "SELECT [Language Code] FROM [" + navCompanyName + "LSC Data Translation Language$5ecfc871-5d82-43f1-9c54-59685e82318d]";

                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ReplDataTranslationLangCode()
                            {
                                Code = SQLHelper.GetString(reader["Language Code"])
                            });
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private ReplDataTranslation ReaderToDataTranslation(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplDataTranslation()
            {
                Key = SQLHelper.GetString(reader["Key"]),
                LanguageCode = SQLHelper.GetString(reader["Language Code"]),
                Text = SQLHelper.GetString(reader["Translation"]),
                TranslationId = SQLHelper.GetString(reader["Translation ID"])
            };
        }

        private ReplDataTranslation ReaderToHtmlTranslation(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplDataTranslation()
            {
                Key = SQLHelper.GetString(reader["Item No_"]),
                LanguageCode = SQLHelper.GetString(reader["Language"]),
                Text = SQLHelper.GetStringByte(reader["Html"], config.SettingsGetByKey(ConfigKey.BOEncode)),
                TranslationId = "T0010001410-F0000000020",
                URL = SQLHelper.GetString(reader["URL"])
            };
        }

        private ReplDataTranslation ReaderToDealHtmlTranslation(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplDataTranslation()
            {
                Key = SQLHelper.GetString(reader["Offer No_"]),
                LanguageCode = SQLHelper.GetString(reader["Language"]),
                Text = SQLHelper.GetStringByte(reader["Html"], config.SettingsGetByKey(ConfigKey.BOEncode)),
                TranslationId = "T0010000825-F0000000020",
                URL = SQLHelper.GetString(reader["URL"])
            };
        }
    }
}
