using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
{
    public class ItemVariantRegistrationRepository : BaseRepository
    {
        // Key: Item No.,Variant Dimension 1,Variant Dimension 2,Variant Dimension 3,Variant Dimension 4,Variant Dimension 5,Variant Dimension 6
        const int TABLEID = 10001414;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public ItemVariantRegistrationRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Item No_],mt.[Framework Code],mt.[Variant],mt.[Variant Dimension 1],mt.[Variant Dimension 2]," +
                         "mt.[Variant Dimension 3],mt.[Variant Dimension 4],mt.[Variant Dimension 5],mt.[Variant Dimension 6]";

            sqlfrom = " FROM [" + navCompanyName + "Item Variant Registration$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public List<ReplItemVariantRegistration> ReplicateItemVariantRegistration(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Item Variant Registration$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[Item No_]", storeId, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplItemVariantRegistration> list = new List<ReplItemVariantRegistration>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatementWithStoreDist(fullReplication, keys, "mt.[Item No_]", storeId, true);

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
                                list.Add(ReaderToItemVariantRegistration(reader, out lastKey));
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
                                if (par.Length < 7 || par.Length != keys.Count)
                                    continue;

                                list.Add(new ReplItemVariantRegistration()
                                {
                                    ItemId = par[0],
                                    VariantDimension1 = par[1],
                                    VariantDimension2 = par[2],
                                    VariantDimension3 = par[3],
                                    VariantDimension4 = par[4],
                                    VariantDimension5 = par[5],
                                    VariantDimension6 = par[6],
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
                                    list.Add(ReaderToItemVariantRegistration(reader, out string ts));
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

        public List<VariantRegistration> VariantRegGetByItemId(string itemId)
        {
            List<VariantRegistration> list = new List<VariantRegistration>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetSQL(false, 0) + sqlcolumns + sqlfrom + " WHERE [Item No_]='" + itemId + "'";
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyVariantReg(reader, itemId));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public VariantRegistration VariantRegGetById(string id, string itemId)
        {
            VariantRegistration varReg = new VariantRegistration();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetSQL(false, 0) + sqlcolumns + sqlfrom + " WHERE [Item No_]='" + itemId + "' AND [Variant]='" + id + "'";
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            varReg = ReaderToLoyVariantReg(reader, itemId);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return varReg;
        }

        private ReplItemVariantRegistration ReaderToItemVariantRegistration(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplItemVariantRegistration()
            {
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                VariantId = SQLHelper.GetString(reader["Variant"]),
                FrameworkCode = SQLHelper.GetString(reader["Framework Code"]),
                VariantDimension1 = SQLHelper.GetString(reader["Variant Dimension 1"]),
                VariantDimension2 = SQLHelper.GetString(reader["Variant Dimension 2"]),
                VariantDimension3 = SQLHelper.GetString(reader["Variant Dimension 3"]),
                VariantDimension4 = SQLHelper.GetString(reader["Variant Dimension 4"]),
                VariantDimension5 = SQLHelper.GetString(reader["Variant Dimension 5"]),
                VariantDimension6 = SQLHelper.GetString(reader["Variant Dimension 6"])
            };
        }

        private VariantRegistration ReaderToLoyVariantReg(SqlDataReader reader, string itemid)
        {
            VariantRegistration var = new VariantRegistration()
            {
                ItemId = itemid,
                Id = SQLHelper.GetString(reader["Variant"]),
                FrameworkCode = SQLHelper.GetString(reader["Framework Code"]),
                Dimension1 = SQLHelper.GetString(reader["Variant Dimension 1"]),
                Dimension2 = SQLHelper.GetString(reader["Variant Dimension 2"]),
                Dimension3 = SQLHelper.GetString(reader["Variant Dimension 3"]),
                Dimension4 = SQLHelper.GetString(reader["Variant Dimension 4"]),
                Dimension5 = SQLHelper.GetString(reader["Variant Dimension 5"]),
                Dimension6 = SQLHelper.GetString(reader["Variant Dimension 6"])
            };

            ImageRepository imgrepo = new ImageRepository(config);
            var.Images = imgrepo.ImageGetByKey("Item Variant", itemid, var.Id, string.Empty, 0, false);
            return var;
        }
    }
}
 