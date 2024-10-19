using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;

namespace LSOmni.DataAccess.BOConnection.CentralExt.Dal
{
    public class InvStatusRepository : BaseRepository
    {
        public InvStatusRepository(BOConfiguration config, Version navVersion) : base(config, navVersion)
        {
        }

        public virtual List<ReplInvStatus> ReplicateInventoryStatus(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            string sqlcolumns = "mt.[Item No_],mt.[Variant Code],mt.[Store No_],mt.[Net Inventory],mt.[Replication Counter]";
            if (LSCVersion >= new Version("18.4"))
                sqlcolumns += ",mt.[Sourcing Location Inventory]";

            string sqlfrom = " FROM [" + navCompanyName + "LSC Inventory Lookup Table$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";

            SQLHelper.CheckForSQLInjection(storeId);
            string sqlwhere = " WHERE " + ((string.IsNullOrEmpty(storeId)) ? string.Empty : "mt.[Store No_]='" + storeId + "' AND ");
            if (fullReplication)
                sqlwhere += "mt.[timestamp]>@cnt";
            else
                sqlwhere += "mt.[Replication Counter]>@cnt";

            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<ReplInvStatus> list = new List<ReplInvStatus>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "SELECT COUNT(*),Max(mt.[Replication Counter])" + sqlfrom + sqlwhere;
                    if (fullReplication)
                    {
                        SqlParameter par = new SqlParameter("@cnt", SqlDbType.Timestamp);
                        if (lastKey == "0")
                            par.Value = new byte[] { 0 };
                        else
                            par.Value = StringToByteArray(lastKey);
                        command.Parameters.Add(par);
                    }
                    else
                        command.Parameters.AddWithValue("@cnt", lastKey);

                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            recordsRemaining = SQLHelper.GetInt32(reader[0]);
                            if (reader[1] != DBNull.Value)
                                maxKey = SQLHelper.GetString(reader[1]);
                        }
                        reader.Close();
                    }

                    // get data
                    command.CommandText = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + sqlwhere;
                    if (fullReplication)
                        command.CommandText += " ORDER BY mt.[timestamp]";
                    else
                        command.CommandText += " ORDER BY mt.[Replication Counter]";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int cnt = 0;
                        while (reader.Read())
                        {
                            list.Add(ReaderToStatus(reader, fullReplication, ref lastKey));
                            cnt++;

                            if (batchSize > 0 && cnt >= batchSize)
                                break;
                        }
                        reader.Close();
                        recordsRemaining -= cnt;
                    }
                    connection.Close();
                }
            }

            if (fullReplication && recordsRemaining <= 0)
                lastKey = maxKey;

            // just in case something goes too far
            if (recordsRemaining < 0)
                recordsRemaining = 0;

            return list;
        }

        public virtual List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId)
        {
            List<HospAvailabilityResponse> list = new List<HospAvailabilityResponse>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    string sql = "SELECT mt.[Store No_],mt.[Type],mt.[No_],mt.[Available Qty_],mt.[Unit of Measure] " +
                                 "FROM [" + navCompanyName + "LSC Current Availability$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";

                    connection.Open();
                    if (request == null || request.Count == 0)
                    {
                        command.CommandText = sql;
                        if (string.IsNullOrEmpty(storeId) == false)
                        {
                            command.CommandText += " WHERE mt.[Store No_]=@sid";
                            command.Parameters.AddWithValue("@sid", storeId);
                        }

                        TraceSqlCommand(command);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(ReaderToHospResponse(reader));
                            }
                            reader.Close();
                        }
                    }
                    else
                    {
                        foreach (HospAvailabilityRequest req in request)
                        {
                            command.Parameters.Clear();

                            command.CommandText = sql + " WHERE mt.[No_]=@no";
                            command.Parameters.AddWithValue("@no", req.ItemId);

                            if (string.IsNullOrEmpty(req.UnitOfMeasure) == false)
                            {
                                command.CommandText += " AND mt.[Unit of Measure]=@uom";
                                command.Parameters.AddWithValue("@uom", req.UnitOfMeasure);
                            }

                            if (string.IsNullOrEmpty(storeId) == false)
                            {
                                command.CommandText += " AND mt.[Store No_]=@sid";
                                command.Parameters.AddWithValue("@sid", storeId);
                            }

                            TraceSqlCommand(command);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    list.Add(ReaderToHospResponse(reader));
                                }
                                reader.Close();
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public virtual ScanPayGoSecurityLog SecurityCheckLog(string orderNo, Statistics stat)
        {
            ScanPayGoSecurityLog log = new ScanPayGoSecurityLog();
            logger.StatisticStartSub(false, ref stat, out int index);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Source],[Check] FROM [" + navCompanyName + "LSC SPG Security Check Log$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                                          "WHERE [Customer Order ID]=@id ORDER BY [Entry No_] DESC";

                    connection.Open();
                    command.Parameters.AddWithValue("@id", orderNo);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            log.Source = SQLHelper.GetBool(reader["Source"]);
                            log.Check = SQLHelper.GetBool(reader["Check"]);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return log;
        }

        private ReplInvStatus ReaderToStatus(SqlDataReader reader, bool fullRepl, ref string lastKey)
        {
            if (fullRepl)
                lastKey = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);
            else
                lastKey = SQLHelper.GetString(reader["Replication Counter"]);

            ReplInvStatus rec = new ReplInvStatus()
            {
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                Quantity = SQLHelper.GetDecimal(reader, "Net Inventory"),
                IsDeleted = false
            };
            if (LSCVersion >= new Version("18.4") && LSCVersion < new Version("24.1"))
            {
                rec.Quantity += SQLHelper.GetDecimal(reader, "Sourcing Location Inventory");
            }
            return rec;
        }

        private HospAvailabilityResponse ReaderToHospResponse(SqlDataReader reader)
        {
            return new HospAvailabilityResponse()
            {
                Number = SQLHelper.GetString(reader["No_"]),
                UnitOfMeasure = SQLHelper.GetString(reader["Unit of Measure"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                IsDeal = SQLHelper.GetBool(reader["Type"]),
                Quantity = SQLHelper.GetDecimal(reader, "Available Qty_")
            };
        }
    }
}
