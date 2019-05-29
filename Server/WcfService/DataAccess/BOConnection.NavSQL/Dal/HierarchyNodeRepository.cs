using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Replication;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class HierarchyNodeRepository : BaseRepository
    {
        // Key: Hierarchy Code
        const int TABLENODEID = 10000921;
        const int TABLELINKID = 10000922;

        private string sqlcolumnsNode = string.Empty;
        private string sqlfromNode = string.Empty;

        private string sqlcolumnsLink = string.Empty;
        private string sqlfromLink = string.Empty;

        public HierarchyNodeRepository(Version navVersion) : base(navVersion)
        {
            sqlcolumnsNode = "mt.[Hierarchy Code],mt.[Node ID],mt.[Parent Node ID],mt.[Description],mt.[Children Order],mt.[Indentation],mt.[Presentation Order],mt.[Retail Image Code]";
            sqlfromNode = " FROM [" + navCompanyName + ((NavVersion >= new Version("11.01.00")) ? "Hierarchy Nodes] mt" : "Hierarchy Node] mt") +
                          " INNER JOIN [" + navCompanyName + "Hierarchy Date] hd ON hd.[Hierarchy Code]=mt.[Hierarchy Code] AND hd.[Start Date]<=GETDATE()";

            sqlcolumnsLink = "mt.[Hierarchy Code],mt.[Node ID],mt.[Type],mt.[No_],mt.[Description],il.[Image Id]";
            sqlfromLink = " FROM [" + navCompanyName + "Hierarchy Node Link] mt" +
                          " INNER JOIN [" + navCompanyName + "Hierarchy Date] hd ON hd.[Hierarchy Code]=mt.[Hierarchy Code] AND hd.[Start Date]<=GETDATE()" +
                          " LEFT OUTER JOIN [" + navCompanyName + "Retail Image Link] il ON il.KeyValue=mt.[No_] AND il.[Display Order]=0" +
                          " AND il.[TableName]=CASE WHEN mt.[Type]=0 THEN 'Item' ELSE 'Offer' END";
        }

        public List<ReplHierarchyNode> ReplicateHierarchyNode(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            SQLHelper.CheckForSQLInjection(storeId);
            List<JscKey> keys = GetPrimaryKeys("Hierarchy Nodes");
            string where = string.Format(" AND hd.[Store Code]='{0}'", storeId);

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)";
                if (batchSize > 0)
                {
                    sql += sqlfromNode + GetWhereStatement(true, keys, where, false);
                }
            }
            recordsRemaining = GetRecordCount(TABLENODEID, lastKey, sql, (batchSize > 0) ? keys : null, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLENODEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplHierarchyNode> list = new List<ReplHierarchyNode>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumnsNode + sqlfromNode + GetWhereStatement(fullReplication, keys, where, true);

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
                                list.Add(ReaderToHierarchyNode(reader, out lastKey));
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

                                list.Add(new ReplHierarchyNode()
                                {
                                    HierarchyCode = par[0],
                                    Id = par[1],
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
                                    list.Add(ReaderToHierarchyNode(reader, out string ts));
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

        public List<HierarchyNode> HierarchyNodeGet(string hCode, string storeId)
        {
            List<HierarchyNode> list = new List<HierarchyNode>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumnsNode + sqlfromNode + " AND hd.[Store Code]=@sid";
                    command.Parameters.AddWithValue("@sid", storeId);
                    if (string.IsNullOrEmpty(hCode) == false)
                    {
                        command.CommandText += " WHERE mt.[Hierarchy Code]=@id";
                        command.Parameters.AddWithValue("@id", hCode);
                    }
                    command.CommandText += " ORDER BY mt.[Presentation Order]";

                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToHierarchyNode(reader, storeId));
                        }
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<ReplHierarchyLeaf> ReplicateHierarchyLeaf(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            SQLHelper.CheckForSQLInjection(storeId);
            List<JscKey> keys = GetPrimaryKeys("Hierarchy Node Link");
            string where = string.Format(" AND hd.[Store Code]='{0}'", storeId);

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)";
                if (batchSize > 0)
                {
                    sql += sqlfromLink + GetWhereStatement(true, keys, where, false);
                }
            }
            recordsRemaining = GetRecordCount(TABLELINKID, lastKey, sql, (batchSize > 0) ? keys : null, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLELINKID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplHierarchyLeaf> list = new List<ReplHierarchyLeaf>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumnsLink + sqlfromLink + GetWhereStatement(fullReplication, keys, where, true);

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
                                list.Add(ReaderToHierarchyNodeLink(reader, out lastKey));
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
                                if (par.Length < 4 || par.Length != keys.Count)
                                    continue;

                                list.Add(new ReplHierarchyLeaf()
                                {
                                    HierarchyCode = par[0],
                                    NodeId = par[1],
                                    Type = (HierarchyLeafType)Convert.ToInt32(par[2]),
                                    Id = par[3],
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
                                    list.Add(ReaderToHierarchyNodeLink(reader, out string ts));
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

        public List<HierarchyLeaf> HierarchyNodeLinkGet(string hCode, string nCode, string storeId)
        {
            List<HierarchyLeaf> list = new List<HierarchyLeaf>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumnsLink + sqlfromLink +
                                          " WHERE mt.[Hierarchy Code]=@Hid AND mt.[Node ID]=@Nid AND hd.[Store Code]=@sid";

                    command.Parameters.AddWithValue("@Hid", hCode);
                    command.Parameters.AddWithValue("@Nid", nCode);
                    command.Parameters.AddWithValue("@sid", storeId);
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToHierarchyNodeLink(reader));
                        }
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private ReplHierarchyNode ReaderToHierarchyNode(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplHierarchyNode()
            {
                HierarchyCode = SQLHelper.GetString(reader["Hierarchy Code"]),
                Id = SQLHelper.GetString(reader["Node ID"]),
                ParentNode = SQLHelper.GetString(reader["Parent Node ID"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ChildrenOrder = SQLHelper.GetInt32(reader["Children Order"]),
                Indentation = SQLHelper.GetInt32(reader["Indentation"]),
                PresentationOrder = SQLHelper.GetInt32(reader["Presentation Order"]),
                ImageId = SQLHelper.GetString(reader["Retail Image Code"])
            };
        }

        private ReplHierarchyLeaf ReaderToHierarchyNodeLink(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplHierarchyLeaf()
            {
                HierarchyCode = SQLHelper.GetString(reader["Hierarchy Code"]),
                NodeId = SQLHelper.GetString(reader["Node ID"]),
                Id = SQLHelper.GetString(reader["No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                Type = (HierarchyLeafType)SQLHelper.GetInt32(reader["Type"]),
                ImageId = SQLHelper.GetString(reader["Image Id"])
            };
        }

        private HierarchyNode ReaderToHierarchyNode(SqlDataReader reader, string storeId)
        {
            HierarchyNode val = new HierarchyNode()
            {
                HierarchyCode = SQLHelper.GetString(reader["Hierarchy Code"]),
                ParentNode = SQLHelper.GetString(reader["Parent Node ID"]),
                Id = SQLHelper.GetString(reader["Node ID"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ImageId = SQLHelper.GetString(reader["Retail Image Code"]),
                Indentation = SQLHelper.GetInt32(reader["Indentation"]),
                ChildrenOrder = SQLHelper.GetInt32(reader["Children Order"]),
                PresentationOrder = SQLHelper.GetInt32(reader["Presentation Order"])
            };

            val.Leafs = HierarchyNodeLinkGet(val.HierarchyCode, val.Id, storeId);
            return val;
        }

        private HierarchyLeaf ReaderToHierarchyNodeLink(SqlDataReader reader)
        {
            return new HierarchyLeaf()
            {
                HierarchyCode = SQLHelper.GetString(reader["Hierarchy Code"]),
                ParentNode = SQLHelper.GetString(reader["Node ID"]),
                Id = SQLHelper.GetString(reader["No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                Type = (HierarchyLeafType)SQLHelper.GetInt32(reader["Type"]),
                ImageId = SQLHelper.GetString(reader["Image Id"])
            };
        }
    }
}
