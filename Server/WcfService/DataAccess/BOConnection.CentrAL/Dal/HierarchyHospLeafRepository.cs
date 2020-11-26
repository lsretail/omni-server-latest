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
        const int TABLERECIPEID = 90;
        const int TABLEDEALID = 99001503;
        const int TABLEDEALLINEID = 99001651;

        private string sqlcolumnsRecipe = string.Empty;
        private string sqlfromRecipe = string.Empty;

        private string sqlcolumnsDeal = string.Empty;
        private string sqlfromDeal = string.Empty;

        private string sqlcolumnsDealLine = string.Empty;
        private string sqlfromDealLine = string.Empty;

        public HierarchyHospLeafRepository(BOConfiguration config, Version navVersion) : base(config, navVersion)
        {
            sqlcolumnsRecipe = "nl.[Hierarchy Code],nl.[Node ID],nl.[No_],mt2.[Line No_],mt.[Item No_],mt2.[Description]," +
                                "mt2.[Unit of Measure Code],mt2.[Quantity per],mt.Exclusion,mt.[Price on Exclusion],il.[Image Id]";
            sqlfromRecipe = " FROM [" + navCompanyName + "Hierarchy Node Link$5ecfc871-5d82-43f1-9c54-59685e82318d] nl" +
                             " INNER JOIN [" + navCompanyName + "Hierarchy Date$5ecfc871-5d82-43f1-9c54-59685e82318d] hd ON hd.[Hierarchy Code]=nl.[Hierarchy Code] AND hd.[Start Date]<=GETDATE()" +
                             " INNER JOIN [" + navCompanyName + "BOM Component$437dbf0e-84ff-417a-965d-ed2bb9650972] mt2 ON mt2.[Parent Item No_]=nl.[No_]" +
                             " INNER JOIN [" + navCompanyName + "BOM Component$5ecfc871-5d82-43f1-9c54-59685e82318d] mt ON mt.[Parent Item No_]=nl.[No_] AND mt2.[Line No_]=mt.[Line No_]" +
                             " LEFT OUTER JOIN [" + navCompanyName + "Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d] il ON il.KeyValue=mt.[Item No_] AND il.[Display Order]=0 AND il.[TableName]='Item'";

            sqlcolumnsDeal = "nl.[Hierarchy Code],nl.[Node ID],mt.[Offer No_],mt.[Line No_],mt.[No_],mt.[Description],mt.[Variant Code],mt.[Type]," +
                             "mt.[Unit of Measure],mt.[Min_ Selection],mt.[Max_ Selection],mt.[Modifier Added Amount],mt.[Deal Mod_ Size Gr_ Index],il.[Image Id]";
            sqlfromDeal = " FROM [" + navCompanyName + "Hierarchy Node Link$5ecfc871-5d82-43f1-9c54-59685e82318d] nl" +
                          " INNER JOIN [" + navCompanyName + "Hierarchy Date$5ecfc871-5d82-43f1-9c54-59685e82318d] hd ON hd.[Hierarchy Code]=nl.[Hierarchy Code] AND hd.[Start Date]<=GETDATE()" +
                          " INNER JOIN [" + navCompanyName + "Offer Line$5ecfc871-5d82-43f1-9c54-59685e82318d] mt ON mt.[Offer No_]=nl.[No_]" +
                          " LEFT OUTER JOIN [" + navCompanyName + "Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d] il ON il.KeyValue=mt.[No_] AND il.[Display Order]=0 AND il.[TableName]='Item'";

            sqlcolumnsDealLine = "nl.[Hierarchy Code],nl.[Node ID],mt.[Offer No_],mt.[Deal Modifier Code],mt.[Offer Line No_]," +
                          "mt.[Deal Modifier Line No_],mt.[Item No_],mt.[Description],mt.[Variant Code],mt.[Unit of Measure]," +
                          "mt.[Min_ Selection],mt.[Max_ Item Selection],mt.[Added Amount],il.[Image Id]";
            sqlfromDealLine = " FROM [" + navCompanyName + "Hierarchy Node Link$5ecfc871-5d82-43f1-9c54-59685e82318d] nl" +
                          " INNER JOIN [" + navCompanyName + "Hierarchy Date$5ecfc871-5d82-43f1-9c54-59685e82318d] hd ON hd.[Hierarchy Code]=nl.[Hierarchy Code] AND hd.[Start Date]<=GETDATE()" +
                          " INNER JOIN [" + navCompanyName + "Deal Modifier Item$5ecfc871-5d82-43f1-9c54-59685e82318d] mt ON mt.[Offer No_]=nl.[No_]" +
                          " LEFT OUTER JOIN [" + navCompanyName + "Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d] il ON il.KeyValue=mt.[Item No_] AND il.[Display Order]=0 AND il.[TableName]='Item'";
        }

        public List<ReplHierarchyHospDeal> ReplicateHierarchyHospDeal(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            SQLHelper.CheckForSQLInjection(storeId);
            List<JscKey> keys = GetPrimaryKeys("Offer Line$5ecfc871-5d82-43f1-9c54-59685e82318d");
            string where = string.Format(" AND nl.[Type]=1 AND hd.[Store Code]='{0}'", storeId);

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfromDeal + GetWhereStatement(true, keys, where, false);
            }
            recordsRemaining = GetRecordCount(TABLEDEALID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEDEALID, batchSize, ref lastKey, ref recordsRemaining);
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
                                if (par.Length < 2 || par.Length != keys.Count)
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
            List<JscKey> keys = GetPrimaryKeys("Deal Modifier Item$5ecfc871-5d82-43f1-9c54-59685e82318d");
            string where = string.Format(" AND nl.[Type]=1 AND hd.[Store Code]='{0}'", storeId);

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfromDealLine + GetWhereStatement(true, keys, where, false);
            }
            recordsRemaining = GetRecordCount(TABLEDEALLINEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEDEALLINEID, batchSize, ref lastKey, ref recordsRemaining);
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
                                if (par.Length < 3 || par.Length != keys.Count)
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

        public List<ReplHierarchyHospRecipe> ReplicateHierarchyHospRecipe(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            SQLHelper.CheckForSQLInjection(storeId);
            List<JscKey> keys = GetPrimaryKeys("BOM Component$5ecfc871-5d82-43f1-9c54-59685e82318d");
            string where = string.Format(" AND nl.[Type]=0 AND hd.[Store Code]='{0}'", storeId);

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfromRecipe + GetWhereStatement(true, keys, where, false);
            }
            recordsRemaining = GetRecordCount(TABLERECIPEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLERECIPEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplHierarchyHospRecipe> list = new List<ReplHierarchyHospRecipe>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumnsRecipe + sqlfromRecipe + GetWhereStatement(fullReplication, keys, where, true);

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
                                list.Add(ReaderToHierarchyRecipe(reader, out lastKey));
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

                                list.Add(new ReplHierarchyHospRecipe()
                                {
                                    RecipeNo = par[0],
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
                                    list.Add(ReaderToHierarchyRecipe(reader, out string ts));
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
                AddedAmount = SQLHelper.GetDecimal(reader["Modifier Added Amount"]),
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
                AddedAmount = SQLHelper.GetDecimal(reader["Added Amount"]),
                ImageId = SQLHelper.GetString(reader["Image Id"])
            };
        }

        private ReplHierarchyHospRecipe ReaderToHierarchyRecipe(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplHierarchyHospRecipe()
            {
                HierarchyCode = SQLHelper.GetString(reader["Hierarchy Code"]),
                ParentNode = SQLHelper.GetString(reader["Node ID"]),
                RecipeNo = SQLHelper.GetString(reader["No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                LineNo = SQLHelper.GetInt32(reader["Line No_"]),
                ItemNo = SQLHelper.GetString(reader["Item No_"]),
                UnitOfMeasure = SQLHelper.GetString(reader["Unit of Measure Code"]),
                Exclusion = SQLHelper.GetBool(reader["Exclusion"]),
                ExclusionPrice = SQLHelper.GetDecimal(reader["Price on Exclusion"]),
                QuantityPer = SQLHelper.GetDecimal(reader["Quantity per"]),
                ImageId = SQLHelper.GetString(reader["Image Id"])
            };
        }
    }
}
