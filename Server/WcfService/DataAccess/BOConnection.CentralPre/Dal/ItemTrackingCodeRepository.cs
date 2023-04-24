using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class ItemTrackingCodeRepository : BaseRepository
    {
        // Key: Code
        const int TABLEID = 6502;

        private readonly string sqlcolumns = string.Empty;
        private readonly string sqlfrom = string.Empty;

        public ItemTrackingCodeRepository(BOConfiguration config, Version version) : base(config, version)
        {
            sqlcolumns = "mt.[Code],mt.[Description],mt.[Warranty Date Formula],mt.[Man_ Warranty Date Entry Reqd_]," +
                         "mt.[Man_ Expir_ Date Entry Reqd_],mt.[Strict Expiration Posting],mt.[Use Expiration Dates]," +
                         "mt.[SN Specific Tracking],mt.[SN Info_ Inbound Must Exist],mt.[SN Info_ Outbound Must Exist]," +
                         "mt.[SN Warehouse Tracking],mt.[SN Purchase Inbound Tracking],mt.[SN Purchase Outbound Tracking]," +
                         "mt.[SN Sales Inbound Tracking],mt.[SN Sales Outbound Tracking],mt.[SN Pos_ Adjmt_ Inb_ Tracking]," +
                         "mt.[SN Pos_ Adjmt_ Outb_ Tracking],mt.[SN Neg_ Adjmt_ Inb_ Tracking],mt.[SN Neg_ Adjmt_ Outb_ Tracking]," +
                         "mt.[SN Transfer Tracking],mt.[SN Manuf_ Inbound Tracking],mt.[SN Manuf_ Outbound Tracking]," +
                         "mt.[SN Assembly Inbound Tracking],mt.[SN Assembly Outbound Tracking]," +
                         "mt.[Lot Specific Tracking],mt.[Lot Info_ Inbound Must Exist],mt.[Lot Info_ Outbound Must Exist]," +
                         "mt.[Lot Warehouse Tracking],mt.[Lot Purchase Inbound Tracking],mt.[Lot Purchase Outbound Tracking]," +
                         "mt.[Lot Sales Inbound Tracking],mt.[Lot Sales Outbound Tracking],mt.[Lot Pos_ Adjmt_ Inb_ Tracking]," +
                         "mt.[Lot Pos_ Adjmt_ Outb_ Tracking],mt.[Lot Neg_ Adjmt_ Inb_ Tracking],mt.[Lot Neg_ Adjmt_ Outb_ Tracking]," +
                         "mt.[Lot Transfer Tracking],mt.[Lot Manuf_ Inbound Tracking],mt.[Lot Manuf_ Outbound Tracking]," +
                         "mt.[Lot Assembly Inbound Tracking],mt.[Lot Assembly Outbound Tracking]";

            if (LSCVersion >= new Version("18.0"))
                sqlcolumns += ",mt.[Create SN Info on Posting]";

            sqlfrom = " FROM [" + navCompanyName + "Item Tracking Code$437dbf0e-84ff-417a-965d-ed2bb9650972] mt";
        }

        public List<ReplItemTrackingCode> ReplicateItemTrackingCode(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Item Tracking Code$437dbf0e-84ff-417a-965d-ed2bb9650972");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(fullReplication, keys, string.Empty, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplItemTrackingCode> list = new List<ReplItemTrackingCode>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(fullReplication, keys, string.Empty, false);

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
                                list.Add(ReaderToCode(reader, out lastKey));
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
                                list.Add(new ReplItemTrackingCode()
                                {
                                    Code = act.ParamValue,
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
                                    list.Add(ReaderToCode(reader, out string ts));
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

        private ReplItemTrackingCode ReaderToCode(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            ReplItemTrackingCode code = new ReplItemTrackingCode()
            {
                Code = SQLHelper.GetString(reader["Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                WarrantyDateFormula = SQLHelper.GetString(reader["Warranty Date Formula"]),
                ManWarrantyDateEntryReqired = SQLHelper.GetBool(reader["Man_ Warranty Date Entry Reqd_"]),
                ManExpirationDateEntryReqired = SQLHelper.GetBool(reader["Man_ Expir_ Date Entry Reqd_"]),
                StrictExpirationPosting = SQLHelper.GetBool(reader["Strict Expiration Posting"]),
                UseExpirationDates = SQLHelper.GetBool(reader["Use Expiration Dates"]),
                SNSpecificTracking = SQLHelper.GetBool(reader["SN Specific Tracking"]),
                SNInfoInboundMustExist = SQLHelper.GetBool(reader["SN Info_ Inbound Must Exist"]),
                SNInfoOutboundMustExist = SQLHelper.GetBool(reader["SN Info_ Outbound Must Exist"]),
                SNWarehouseTracking = SQLHelper.GetBool(reader["SN Warehouse Tracking"]),
                SNPurchaseInboundTracking = SQLHelper.GetBool(reader["SN Purchase Inbound Tracking"]),
                SNPurchaseOutboundTracking = SQLHelper.GetBool(reader["SN Purchase Outbound Tracking"]),
                SNSalesInboundTracking = SQLHelper.GetBool(reader["SN Sales Inbound Tracking"]),
                SNSalesOutboundTracking = SQLHelper.GetBool(reader["SN Sales Outbound Tracking"]),
                SNPosAdjmtInboundTracking = SQLHelper.GetBool(reader["SN Pos_ Adjmt_ Inb_ Tracking"]),
                SNPosAdjmtOutboundTracking = SQLHelper.GetBool(reader["SN Pos_ Adjmt_ Outb_ Tracking"]),
                SNNegAdjmtInboundTracking = SQLHelper.GetBool(reader["SN Neg_ Adjmt_ Inb_ Tracking"]),
                SNNegAdjmtOutboundTracking = SQLHelper.GetBool(reader["SN Neg_ Adjmt_ Outb_ Tracking"]),
                SNTransferTracking = SQLHelper.GetBool(reader["SN Transfer Tracking"]),
                SNManufInboundTracking = SQLHelper.GetBool(reader["SN Manuf_ Inbound Tracking"]),
                SNManufOutboundTracking = SQLHelper.GetBool(reader["SN Manuf_ Outbound Tracking"]),
                SNAssemblyInboundTracking = SQLHelper.GetBool(reader["SN Assembly Inbound Tracking"]),
                SNAssemblyOutboundTracking = SQLHelper.GetBool(reader["SN Assembly Outbound Tracking"]),
                LotSpecificTracking = SQLHelper.GetBool(reader["Lot Specific Tracking"]),
                LotInfoInboundMustExist = SQLHelper.GetBool(reader["Lot Info_ Inbound Must Exist"]),
                LotInfoOutboundMustExist = SQLHelper.GetBool(reader["Lot Info_ Outbound Must Exist"]),
                LotWarehouseTracking = SQLHelper.GetBool(reader["Lot Warehouse Tracking"]),
                LotPurchaseInboundTracking = SQLHelper.GetBool(reader["Lot Purchase Inbound Tracking"]),
                LotPurchaseOutboundTracking = SQLHelper.GetBool(reader["Lot Purchase Outbound Tracking"]),
                LotSalesInboundTracking = SQLHelper.GetBool(reader["Lot Sales Inbound Tracking"]),
                LotSalesOutboundTracking = SQLHelper.GetBool(reader["Lot Sales Outbound Tracking"]),
                LotPosAdjmtInbboundTracking = SQLHelper.GetBool(reader["Lot Pos_ Adjmt_ Inb_ Tracking"]),
                LotPosAdjmtOutboundTracking = SQLHelper.GetBool(reader["Lot Pos_ Adjmt_ Outb_ Tracking"]),
                LotNegAdjmtInboundTracking = SQLHelper.GetBool(reader["Lot Neg_ Adjmt_ Inb_ Tracking"]),
                LotNegAdjmtOutboundTracking = SQLHelper.GetBool(reader["Lot Neg_ Adjmt_ Outb_ Tracking"]),
                LotTransferTracking = SQLHelper.GetBool(reader["Lot Transfer Tracking"]),
                LotManufacturingInboundTracking = SQLHelper.GetBool(reader["Lot Manuf_ Inbound Tracking"]),
                LotManufacturingOutboundTracking = SQLHelper.GetBool(reader["Lot Manuf_ Outbound Tracking"]),
                LotAssemblyInboundTracking = SQLHelper.GetBool(reader["Lot Assembly Inbound Tracking"]),
                LotAssemblyOutboundTracking = SQLHelper.GetBool(reader["Lot Assembly Outbound Tracking"])
            };

            if (LSCVersion >= new Version("18.0"))
                code.CreateSNInfoOnPosting = SQLHelper.GetBool(reader["Create SN Info on Posting"]);

            return code;
        }
    }
}
