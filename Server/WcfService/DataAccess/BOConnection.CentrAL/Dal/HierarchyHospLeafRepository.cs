using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Replication;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
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
                             "mt.[Unit of Measure],mt.[Min_ Selection],mt.[Max_ Selection],mt.[Modifier Added Amount],mt.[Deal Mod_ Size Gr_ Index]," +
                             "(SELECT TOP(1) il.[Image Id] FROM [" + navCompanyName + "Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d] il " +
                             "WHERE il.[KeyValue]=mt.[No_] AND il.[TableName]='Item' " +
                             "ORDER BY il.[Display Order]) AS ImageId";
            sqlfromDeal = " FROM [" + navCompanyName + "Hierarchy Node Link$5ecfc871-5d82-43f1-9c54-59685e82318d] nl" +
                          " JOIN [" + navCompanyName + "Hierarchy Date$5ecfc871-5d82-43f1-9c54-59685e82318d] hd ON hd.[Hierarchy Code]=nl.[Hierarchy Code] AND hd.[Start Date]<=GETDATE()" +
                          " JOIN [" + navCompanyName + "Offer Line$5ecfc871-5d82-43f1-9c54-59685e82318d] mt ON mt.[Offer No_]=nl.[No_]";

            sqlcolumnsDealLine = "nl.[Hierarchy Code],nl.[Node ID],mt.[Offer No_],mt.[Deal Modifier Code],mt.[Offer Line No_]," +
                          "mt.[Deal Modifier Line No_],mt.[Item No_],mt.[Description],mt.[Variant Code],mt.[Unit of Measure]," +
                          "mt.[Min_ Selection],mt.[Max_ Item Selection],mt.[Added Amount]," +
                          "(SELECT TOP(1) il.[Image Id] FROM [" + navCompanyName + "Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d] il " +
                          "WHERE il.[KeyValue]=mt.[Item No_] AND il.[TableName]='Item' " +
                          "ORDER BY il.[Display Order]) AS ImageId";
            sqlfromDealLine = " FROM [" + navCompanyName + "Hierarchy Node Link$5ecfc871-5d82-43f1-9c54-59685e82318d] nl" +
                          " JOIN [" + navCompanyName + "Hierarchy Date$5ecfc871-5d82-43f1-9c54-59685e82318d] hd ON hd.[Hierarchy Code]=nl.[Hierarchy Code] AND hd.[Start Date]<=GETDATE()" +
                          " JOIN [" + navCompanyName + "Deal Modifier Item$5ecfc871-5d82-43f1-9c54-59685e82318d] mt ON mt.[Offer No_]=nl.[No_]";
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
                List<JscActions> nodeact = LoadActions(fullReplication, TABLEDEALID, batchSize, ref mainlastkey, ref recordsRemaining);

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
                List<JscActions> nodeact = LoadActions(fullReplication, TABLEDEALLINEID, batchSize, ref mainlastkey, ref recordsRemaining);

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

        public List<HierarchyNode> HierarchyDealGet(string hCode, string nCode, string offerId)
        {
            List<HierarchyNode> list = new List<HierarchyNode>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Offer No_],mt.[Line No_],mt.[No_],mt.[Description],mt.[Variant Code],mt.[Type],mt.[Unit of Measure],mt.[Min_ Selection],mt.[Max_ Selection],mt.[Modifier Added Amount],mt.[Deal Mod_ Size Gr_ Index]," +
                                          "(SELECT TOP(1) il.[Image Id] FROM [" + navCompanyName + "Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d] il " +
                                          "WHERE il.[KeyValue]=mt.[No_] AND il.[TableName]='Item' " +
                                          "ORDER BY il.[Display Order]) AS ImageId " +
                                          "FROM [" + navCompanyName + "Offer Line$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                                          "WHERE mt.[Offer No_]=@id AND mt.[Type]=5";
                    command.Parameters.AddWithValue("@id", offerId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToHierarchyNode(reader, hCode, nCode));
                        }
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<HierarchyLeaf> HierarchyDealLeafGet(string hCode, string nCode, string offerId)
        {
            List<HierarchyLeaf> list = new List<HierarchyLeaf>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Offer No_],mt.[Line No_],mt.[No_],mt.[Description],mt.[Variant Code],mt.[Type],mt.[Unit of Measure],mt.[Min_ Selection],mt.[Max_ Selection],mt.[Modifier Added Amount],mt.[Deal Mod_ Size Gr_ Index]," +
                                          "(SELECT TOP(1) il.[Image Id] FROM [" + navCompanyName + "Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d] il " +
                                          "WHERE il.[KeyValue]=mt.[No_] AND il.[TableName]='Item' " +
                                          "ORDER BY il.[Display Order]) AS ImageId " +
                                          "FROM [" + navCompanyName + "Offer Line$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                                          "WHERE mt.[Offer No_]=@id AND mt.[Type]=0";
                    command.Parameters.AddWithValue("@id", offerId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToHierarchyLeaf(reader, hCode, nCode));
                        }
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<HierarchyLeaf> HierarchyDealLineGet(string hCode, string nCode, string orderNo, string dealNo)
        {
            List<HierarchyLeaf> list = new List<HierarchyLeaf>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " +
                            "mt.[Offer No_],mt.[Deal Modifier Code],mt.[Offer Line No_]," +
                            "mt.[Deal Modifier Line No_],mt.[Item No_],mt.[Description],mt.[Variant Code],mt.[Unit of Measure]," +
                            "mt.[Min_ Selection],mt.[Max_ Item Selection],mt.[Added Amount]," +
                            "(SELECT TOP(1) il.[Image Id] FROM [" + navCompanyName + "Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d] il " +
                            "WHERE il.[KeyValue]=mt.[Item No_] AND il.[TableName]='Item' " +
                            "ORDER BY il.[Display Order]) AS ImageId " +
                            "FROM [" + navCompanyName + "Deal Modifier Item$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                            "WHERE mt.[Offer No_]=@ono AND mt.[Deal Modifier Code]=@dno";

                    command.Parameters.AddWithValue("@ono", orderNo);
                    command.Parameters.AddWithValue("@dno", dealNo);
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToHierarchyNodeLink(reader, hCode, nCode));
                        }
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private HierarchyNode ReaderToHierarchyNode(SqlDataReader reader, string hCode, string nCode)
        {
            HierarchyNode node = new HierarchyNode()
            {
                HierarchyCode = hCode,
                ParentNode = nCode,
                Id = SQLHelper.GetString(reader["Offer No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                No = SQLHelper.GetString(reader["No_"]),
                Type = (HierarchyDealType)SQLHelper.GetInt32(reader["Type"]),
                VariantCode = SQLHelper.GetString(reader["Variant Code"]),
                UnitOfMeasure = SQLHelper.GetString(reader["Unit of Measure"]),
                MinSelection = SQLHelper.GetInt32(reader["Min_ Selection"]),
                MaxSelection = SQLHelper.GetInt32(reader["Max_ Selection"]),
                AddedAmount = SQLHelper.GetDecimal(reader, "Modifier Added Amount"),
                DealModSizeGroupIndex = SQLHelper.GetInt32(reader["Deal Mod_ Size Gr_ Index"]),
                LineNo = SQLHelper.GetInt32(reader["Line No_"]),
                ImageId = SQLHelper.GetString(reader["ImageId"])
            };
            node.Leafs = HierarchyDealLineGet(hCode, nCode, node.Id, node.No);
            return node;
        }

        private HierarchyLeaf ReaderToHierarchyLeaf(SqlDataReader reader, string hCode, string nCode)
        {
            HierarchyLeaf leaf = new HierarchyLeaf()
            {
                HierarchyCode = hCode,
                ParentNode = nCode,
                Id = SQLHelper.GetString(reader["Offer No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ItemNo = SQLHelper.GetString(reader["No_"]),
                Type = HierarchyLeafType.Item,
                VariantCode = SQLHelper.GetString(reader["Variant Code"]),
                ItemUOM = SQLHelper.GetString(reader["Unit of Measure"]),
                MinSelection = SQLHelper.GetInt32(reader["Min_ Selection"]),
                MaxSelection = SQLHelper.GetInt32(reader["Max_ Selection"]),
                AddedAmount = SQLHelper.GetDecimal(reader, "Modifier Added Amount"),
                LineNo = SQLHelper.GetInt32(reader["Line No_"]),
                ImageId = SQLHelper.GetString(reader["ImageId"])
            };

            ItemModifierRepository mrep = new ItemModifierRepository(config);
            leaf.Modifiers = mrep.ModifierGetByItemId(leaf.ItemNo);

            ItemRecipeRepository rrep = new ItemRecipeRepository(config);
            leaf.Recipies = rrep.RecipeGetByItemId(leaf.ItemNo);
            return leaf;
        }

        private HierarchyLeaf ReaderToHierarchyNodeLink(SqlDataReader reader, string hCode, string nCode)
        {
            return new HierarchyLeaf()
            {
                HierarchyCode = hCode,
                ParentNode = nCode,
                Id = SQLHelper.GetString(reader["Offer No_"]),
                DealLineCode = SQLHelper.GetString(reader["Deal Modifier Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                DealLineNo = SQLHelper.GetInt32(reader["Offer Line No_"]),
                LineNo = SQLHelper.GetInt32(reader["Deal Modifier Line No_"]),
                ItemNo = SQLHelper.GetString(reader["Item No_"]),
                VariantCode = SQLHelper.GetString(reader["Variant Code"]),
                ItemUOM = SQLHelper.GetString(reader["Unit of Measure"]),
                MinSelection = SQLHelper.GetInt32(reader["Min_ Selection"]),
                MaxSelection = SQLHelper.GetInt32(reader["Max_ Item Selection"]),
                AddedAmount = SQLHelper.GetDecimal(reader, "Added Amount"),
                ImageId = SQLHelper.GetString(reader["ImageId"])
            };
        }

        private ReplHierarchyHospDeal ReaderToHierarchyDeal(SqlDataReader reader, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

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
                ImageId = SQLHelper.GetString(reader["ImageId"])
            };
        }

        private ReplHierarchyHospDealLine ReaderToHierarchyDealLine(SqlDataReader reader, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

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
                ImageId = SQLHelper.GetString(reader["ImageId"])
            };
        }
    }
}
