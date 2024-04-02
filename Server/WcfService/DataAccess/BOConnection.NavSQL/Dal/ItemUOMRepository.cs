using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class ItemUOMRepository : BaseRepository
    {
        // Key: Item No., Code
        const int TABLEID = 5404;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public ItemUOMRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Item No_],mt.[Code],mt.[Qty_ per Unit of Measure],mt.[POS Selection],mt.[Count as 1 on Receipt],mt.[Order],um.[Description]";

            sqlfrom = " FROM [" + navCompanyName + "Item Unit of Measure] mt JOIN [" + navCompanyName + "Unit of Measure] um ON um.[Code]=mt.[Code]";
        }

        public List<ReplItemUnitOfMeasure> ReplicateItemUOM(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Item Unit of Measure");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[Item No_]", storeId, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplItemUnitOfMeasure> list = new List<ReplItemUnitOfMeasure>();

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
                                list.Add(ReaderToItemUOM(reader, out lastKey));
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

                                list.Add(new ReplItemUnitOfMeasure()
                                {
                                    ItemId = par[0],
                                    Code = par[1],
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
                                    list.Add(ReaderToItemUOM(reader, out string ts));
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

        public UnitOfMeasure ItemUOMGetByIds(string itemid, string uomid)
        {
            UnitOfMeasure uom = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + ",it.[Base Unit of Measure]" + sqlfrom +
                                         " JOIN [" + navCompanyName + "Item] it ON it.[No_]=mt.[Item No_] WHERE mt.[Item No_]=@itemid AND mt.[Code]=@uomid";

                    command.Parameters.AddWithValue("@itemid", itemid);
                    command.Parameters.AddWithValue("@uomid", uomid);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            uom = ReaderToLoyUOM(reader);
                            string baseuom = SQLHelper.GetString(reader["Base Unit of Measure"]);
                            if (baseuom == uomid)
                                uom.QtyPerUom = 1;
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return uom;
        }

        public List<UnitOfMeasure> ItemUOMGetByItemId(string itemId)
        {
            List<UnitOfMeasure> list = new List<UnitOfMeasure>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetSQL(false, 0) + sqlcolumns + sqlfrom + string.Format(" WHERE mt.[Item No_]='{0}'", itemId);
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyUOM(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private ReplItemUnitOfMeasure ReaderToItemUOM(SqlDataReader reader, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplItemUnitOfMeasure()
            {
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                Code = SQLHelper.GetString(reader["Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ShortDescription = SQLHelper.GetString(reader["Code"]),
                QtyPrUOM = SQLHelper.GetDecimal(reader, "Qty_ per Unit of Measure"),
                Selection = SQLHelper.GetInt32(reader["POS Selection"]),
                CountAsOne = SQLHelper.GetBool(reader["Count as 1 on Receipt"]),
                Order = SQLHelper.GetInt32(reader["Order"])
            };
        }

        private UnitOfMeasure ReaderToLoyUOM(SqlDataReader reader)
        {
            return new UnitOfMeasure()
            {
                Id = SQLHelper.GetString(reader["Code"]),
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ShortDescription = SQLHelper.GetString(reader["Code"]),
                QtyPerUom = SQLHelper.GetDecimal(reader, "Qty_ per Unit of Measure"),
                Decimals = 0
            };
        }
    }
}
 