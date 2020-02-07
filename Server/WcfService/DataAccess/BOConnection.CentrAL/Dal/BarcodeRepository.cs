using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
{
    public class BarcodeRepository : BaseRepository 
    {
        // Key: Barcode No.
        const int TABLEID = 99001451;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public BarcodeRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Item No_],mt.[Barcode No_],mt.[Description],mt.[Variant Code],mt.[Unit of Measure Code]";

            sqlfrom = " FROM [" + navCompanyName + "Barcodes$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public List<ReplBarcode> ReplicateBarcodes(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Barcodes$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[Item No_]", storeId, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplBarcode> list = new List<ReplBarcode>();

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
                                list.Add(ReaderToReplBarcode(reader, out lastKey));
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
                                list.Add(new ReplBarcode()
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
                                    list.Add(ReaderToReplBarcode(reader, out string ts));
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

        public Barcode BarcodeGetByCode(string code)
        {
            Barcode bcode = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom + " WHERE mt.[Barcode No_]=@id";
                    command.Parameters.AddWithValue("@id", code);
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bcode = ReaderToBarcode(reader);
                        }
                    }
                    connection.Close();
                }
            }
            return bcode;
        }

        private ReplBarcode ReaderToReplBarcode(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplBarcode()
            {
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                Id = SQLHelper.GetString(reader["Barcode No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                UnitOfMeasure = SQLHelper.GetString(reader["Unit of Measure Code"])
            };
        }

        private Barcode ReaderToBarcode(SqlDataReader reader)
        {
            return new Barcode()
            {
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                Id = SQLHelper.GetString(reader["Barcode No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                UnitOfMeasureId = SQLHelper.GetString(reader["Unit of Measure Code"])
            };
        }
    }
}
 