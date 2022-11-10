using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class HierarchyRepository : BaseRepository
    {
        // Key: Hierarchy Code
        const int TABLEID = 10000920;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public HierarchyRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Hierarchy Code],mt.[Description],mt.[Type],hd.[Start Date],hd.[Priority],hd.[Sales Type Filter],hd.[Validation Schedule ID]";

            sqlfrom = " FROM [" + navCompanyName + "LSC Hierarchy$5ecfc871-5d82-43f1-9c54-59685e82318d] mt" +
                      " JOIN [" + navCompanyName + "LSC Hierar_ Date$5ecfc871-5d82-43f1-9c54-59685e82318d] hd ON hd.[Hierarchy Code]=mt.[Hierarchy Code]";
        }

        public List<ReplHierarchy> ReplicateHierarchy(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Hierarchy$5ecfc871-5d82-43f1-9c54-59685e82318d");

            SQLHelper.CheckForSQLInjection(storeId);
            string where = string.Format(" AND hd.[Store Code]='{0}'", storeId);

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, where, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplHierarchy> list = new List<ReplHierarchy>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(fullReplication, keys, where, true);

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
                                list.Add(ReaderToHierarchy(reader, out lastKey));
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
                                list.Add(new ReplHierarchy()
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
                                    list.Add(ReaderToHierarchy(reader, out string ts));
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

        public List<Hierarchy> HierarchyGetByStore(string storeId, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            List<Hierarchy> list = new List<Hierarchy>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom + " AND hd.[Store Code]=@sid";
                    command.Parameters.AddWithValue("@sid", storeId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToHierarchy(reader));
                        }
                    }
                    connection.Close();
                }
            }

            foreach (Hierarchy root in list)
            {
                HierarchyNodeRepository rep = new HierarchyNodeRepository(config);
                List<HierarchyNode> nodes = rep.HierarchyNodeGet(root.Id, storeId, stat);

                root.Nodes = nodes.FindAll(x => x.HierarchyCode == root.Id && string.IsNullOrEmpty(x.ParentNode));
                for (int i = 0; i < root.Nodes.Count; i++)
                {
                    HierarchyNode node = root.Nodes[i];
                    root.RecursiveBuilder(ref node, nodes);
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        private List<HierarchyAttribute> HierarchyAttributeGet(string code)
        {
            List<HierarchyAttribute> list = new List<HierarchyAttribute>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Attribute Code],at.[Description] FROM [" + navCompanyName + "LSC Hierar_ Attribute$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                                          "JOIN [" + navCompanyName + "LSC Attribute$5ecfc871-5d82-43f1-9c54-59685e82318d] at ON at.[Code]=mt.[Attribute Code] WHERE mt.[Hierarchy Code]=@id";
                    command.Parameters.AddWithValue("@id", code);

                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToHierarchyAttribute(reader));
                        }
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private ReplHierarchy ReaderToHierarchy(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);
            return new ReplHierarchy()
            {
                Id = SQLHelper.GetString(reader["Hierarchy Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                Type = (HierarchyType)SQLHelper.GetInt32(reader["Type"]),
                StartDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Start Date"]), config.IsJson),
                Priority = SQLHelper.GetInt32(reader["Priority"]),
                SalesType = SQLHelper.GetString(reader["Sales Type Filter"]),
                ValidationScheduleId = SQLHelper.GetString(reader["Validation Schedule ID"])
            };
        }

        private Hierarchy ReaderToHierarchy(SqlDataReader reader)
        {
            Hierarchy val = new Hierarchy()
            {
                Id = SQLHelper.GetString(reader["Hierarchy Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                Type = (HierarchyType)SQLHelper.GetInt32(reader["Type"]),
                StartDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Start Date"]), config.IsJson),
                Priority = SQLHelper.GetInt32(reader["Priority"]),
                SalesType = SQLHelper.GetString(reader["Sales Type Filter"]),
                ValidationScheduleId = SQLHelper.GetString(reader["Validation Schedule ID"])
            };

            val.Attributes = HierarchyAttributeGet(val.Id);
            return val;
        }

        private HierarchyAttribute ReaderToHierarchyAttribute(SqlDataReader reader)
        {
            return new HierarchyAttribute()
            {
                Id = SQLHelper.GetString(reader["Attribute Code"]),
                Description = SQLHelper.GetString(reader["Description"])
            };
        }
    }
}
