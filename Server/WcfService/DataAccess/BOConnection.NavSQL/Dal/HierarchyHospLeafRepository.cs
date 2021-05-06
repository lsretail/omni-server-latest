using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Replication;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class HierarchyHospLeafRepository : BaseRepository
    {
        // Key: Hierarchy Code
        const int TABLEDEALID = 99001503;
        const int TABLEDEALLINEID = 99001651;

        private string sqlcolumnsDeal = string.Empty;
        private string sqlfromDeal = string.Empty;

        private string sqlcolumnsDealLine = string.Empty;
        private string sqlfromDealLine = string.Empty;

        public HierarchyHospLeafRepository(BOConfiguration config, Version navVersion) : base(config, navVersion)
        {
            sqlcolumnsDeal = "nl.[Hierarchy Code],nl.[Node ID],mt.[Offer No_],mt.[Line No_],mt.[No_],mt.[Description],mt.[Variant Code],mt.[Type]," +
                             "mt.[Unit of Measure],mt.[Min_ Selection],mt.[Max_ Selection],mt.[Modifier Added Amount],mt.[Deal Mod_ Size Gr_ Index],il.[Image Id]";
            sqlfromDeal = " FROM [" + navCompanyName + "Hierarchy Node Link] nl" +
                          " INNER JOIN [" + navCompanyName + "Hierarchy Date] hd ON hd.[Hierarchy Code]=nl.[Hierarchy Code] AND hd.[Start Date]<=GETDATE()" +
                          " INNER JOIN [" + navCompanyName + "Offer Line] mt ON mt.[Offer No_]=nl.[No_]" +
                          " LEFT OUTER JOIN [" + navCompanyName + "Retail Image Link] il ON il.KeyValue=mt.[No_] AND il.[Display Order]=0 AND il.[TableName]='Item'";

            sqlcolumnsDealLine = "nl.[Hierarchy Code],nl.[Node ID],mt.[Offer No_],mt.[Deal Modifier Code],mt.[Offer Line No_]," +
                          "mt.[Deal Modifier Line No_],mt.[Item No_],mt.[Description],mt.[Variant Code],mt.[Unit of Measure]," +
                          "mt.[Min_ Selection],mt.[Max_ Item Selection],mt.[Added Amount],il.[Image Id]";
            sqlfromDealLine = " FROM [" + navCompanyName + "Hierarchy Node Link] nl" +
                          " INNER JOIN [" + navCompanyName + "Hierarchy Date] hd ON hd.[Hierarchy Code]=nl.[Hierarchy Code] AND hd.[Start Date]<=GETDATE()" +
                          " INNER JOIN [" + navCompanyName + "Deal Modifier Item] mt ON mt.[Offer No_]=nl.[No_]" +
                          " LEFT OUTER JOIN [" + navCompanyName + "Retail Image Link] il ON il.KeyValue=mt.[Item No_] AND il.[Display Order]=0 AND il.[TableName]='Item'";
        }

        public List<ReplHierarchyHospDeal> ReplicateHierarchyHospDeal(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            SQLHelper.CheckForSQLInjection(storeId);
            List<JscActions> actions = new List<JscActions>();
            List<JscKey> keys = new List<JscKey>();
            keys.Add(new JscKey()
            {
                FieldName = "Offer No_",
                FieldType = "nvarchar"
            });
            string where = string.Format(" AND nl.[Type]=1 AND hd.[Store Code]='{0}'", storeId);

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfromDeal + GetWhereStatement(true, keys, where, false);
                recordsRemaining = GetRecordCount(TABLEDEALID, lastKey, sql, keys, ref maxKey);
            }
            else
            {
                string tmplastkey = lastKey;
                string mainlastkey = lastKey;
                recordsRemaining = 0;

                recordsRemaining = GetRecordCount(TABLEDEALID, lastKey, sql, keys, ref maxKey);
                List<JscActions> nodeact = LoadActions(fullReplication, TABLEDEALID, batchSize, ref lastKey, ref recordsRemaining);

                recordsRemaining += GetRecordCount(10000922, tmplastkey, string.Empty, keys, ref maxKey);
                nodeact.AddRange(LoadActions(fullReplication, 10000922, batchSize, ref tmplastkey, ref recordsRemaining));
                if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                    mainlastkey = tmplastkey;

                lastKey = mainlastkey;

                foreach (JscActions act in nodeact)
                {
                    string[] parvalues = act.ParamValue.Split(';');
                    JscActions newact;

                    if (act.Type == DDStatementType.Delete)
                    {
                        if (act.TableId != TABLEDEALID)
                            continue;       // skip delete actions for extra tables

                        actions.Add(act);
                        continue;
                    }

                    newact = new JscActions()
                    {
                        id = act.id,
                        TableId = act.TableId,
                        Type = act.Type,
                        ParamValue = (act.TableId == TABLEDEALID) ? parvalues[0] : parvalues[3]
                    };

                    JscActions findme = actions.Find(x => x.ParamValue.Equals(newact.ParamValue));
                    if (findme == null)
                    {
                        actions.Add(newact);
                    }
                }
            }

            List<ReplHierarchyHospDeal> list = new List<ReplHierarchyHospDeal>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumnsDeal + sqlfromDeal + GetWhereStatement(fullReplication, keys, where, true);

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
                                list.Add(ReaderToHierarchyDeal(reader, out lastKey));
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
                                if (par.Length < 2)
                                    continue;

                                list.Add(new ReplHierarchyHospDeal()
                                {
                                    DealNo = par[0],
                                    LineNo = Convert.ToInt32(par[1]),
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
                                    list.Add(ReaderToHierarchyDeal(reader, out string ts));
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

        public List<ReplHierarchyHospDealLine> ReplicateHierarchyHospDealLine(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            SQLHelper.CheckForSQLInjection(storeId);
            List<JscActions> actions = new List<JscActions>();
            List<JscKey> keys = new List<JscKey>();
            keys.Add(new JscKey()
            {
                FieldName = "Offer No_",
                FieldType = "nvarchar"
            });
            string where = string.Format(" AND nl.[Type]=1 AND hd.[Store Code]='{0}'", storeId);

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfromDealLine + GetWhereStatement(true, keys, where, false);
                recordsRemaining = GetRecordCount(TABLEDEALLINEID, lastKey, sql, keys, ref maxKey);
            }
            else
            {
                string tmplastkey = lastKey;
                string mainlastkey = lastKey;
                recordsRemaining = 0;

                recordsRemaining = GetRecordCount(TABLEDEALLINEID, lastKey, sql, keys, ref maxKey);
                List<JscActions> nodeact = LoadActions(fullReplication, TABLEDEALLINEID, batchSize, ref lastKey, ref recordsRemaining);

                recordsRemaining += GetRecordCount(10000922, tmplastkey, string.Empty, keys, ref maxKey);
                nodeact.AddRange(LoadActions(fullReplication, 10000922, batchSize, ref tmplastkey, ref recordsRemaining));
                if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                    mainlastkey = tmplastkey;

                lastKey = mainlastkey;

                foreach (JscActions act in nodeact)
                {
                    string[] parvalues = act.ParamValue.Split(';');
                    JscActions newact;

                    if (act.Type == DDStatementType.Delete)
                    {
                        if (act.TableId != TABLEDEALLINEID)
                            continue;       // skip delete actions for extra tables

                        actions.Add(act);
                        continue;
                    }

                    newact = new JscActions()
                    {
                        id = act.id,
                        TableId = act.TableId,
                        Type = act.Type,
                        ParamValue = (act.TableId == TABLEDEALLINEID) ? parvalues[0] : parvalues[3]
                    };

                    JscActions findme = actions.Find(x => x.ParamValue.Equals(newact.ParamValue));
                    if (findme == null)
                    {
                        actions.Add(newact);
                    }
                }

                if (actions.Count == 0)
                    recordsRemaining = 0;
            }

            List<ReplHierarchyHospDealLine> list = new List<ReplHierarchyHospDealLine>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumnsDealLine + sqlfromDealLine + GetWhereStatement(fullReplication, keys, where, true);

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
                                list.Add(ReaderToHierarchyDealLine(reader, out lastKey));
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
                                if (par.Length < 3)
                                    continue;

                                list.Add(new ReplHierarchyHospDealLine()
                                {
                                    DealNo = par[0],
                                    DealLineNo = Convert.ToInt32(par[1]),
                                    LineNo = Convert.ToInt32(par[2]),
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
                                    list.Add(ReaderToHierarchyDealLine(reader, out string ts));
                                }
                                reader.Close();
                            }
                            first = false;
                        }
                        if (string.IsNullOrEmpty(maxKey) || recordsRemaining <= 0)
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

        private ReplHierarchyHospDeal ReaderToHierarchyDeal(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplHierarchyHospDeal()
            {
                HierarchyCode = SQLHelper.GetString(reader["Hierarchy Code"]),
                ParentNode = SQLHelper.GetString(reader["Node ID"]),
                DealNo = SQLHelper.GetString(reader["Offer No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                No = SQLHelper.GetString(reader["No_"]),
                Type = (HierarchyDealType)SQLHelper.GetInt32(reader["Type"]),
                LineNo = SQLHelper.GetInt32(reader["Line No_"]),
                VariantCode = SQLHelper.GetString(reader["Variant Code"]),
                UnitOfMeasure = SQLHelper.GetString(reader["Unit of Measure"]),
                MinSelection = SQLHelper.GetInt32(reader["Min_ Selection"]),
                MaxSelection = SQLHelper.GetInt32(reader["Max_ Selection"]),
                AddedAmount = SQLHelper.GetDecimal(reader, "Modifier Added Amount"),
                DealModSizeGroupIndex = SQLHelper.GetInt32(reader["Deal Mod_ Size Gr_ Index"]),
                ImageId = SQLHelper.GetString(reader["Image Id"])
            };
        }

        private ReplHierarchyHospDealLine ReaderToHierarchyDealLine(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplHierarchyHospDealLine()
            {
                HierarchyCode = SQLHelper.GetString(reader["Hierarchy Code"]),
                ParentNode = SQLHelper.GetString(reader["Node ID"]),
                DealNo = SQLHelper.GetString(reader["Offer No_"]),
                DealLineCode = SQLHelper.GetString(reader["Deal Modifier Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                DealLineNo = SQLHelper.GetInt32(reader["Offer Line No_"]),
                LineNo = SQLHelper.GetInt32(reader["Deal Modifier Line No_"]),
                ItemNo = SQLHelper.GetString(reader["Item No_"]),
                VariantCode = SQLHelper.GetString(reader["Variant Code"]),
                UnitOfMeasure = SQLHelper.GetString(reader["Unit of Measure"]),
                MinSelection = SQLHelper.GetInt32(reader["Min_ Selection"]),
                MaxSelection = SQLHelper.GetInt32(reader["Max_ Item Selection"]),
                AddedAmount = SQLHelper.GetDecimal(reader, "Added Amount"),
                ImageId = SQLHelper.GetString(reader["Image Id"])
            };
        }
    }
}
