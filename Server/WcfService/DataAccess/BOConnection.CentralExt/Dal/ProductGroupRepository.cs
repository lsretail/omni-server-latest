using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;

namespace LSOmni.DataAccess.BOConnection.CentralExt.Dal
{
    public class ProductGroupRepository : BaseRepository
    {
        // Key: Item Category Code, Code
        const int TABLEID = 10000705;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public ProductGroupRepository(BOConfiguration config, Version version) : base(config, version)
        {
            sqlcolumns = "mt.[Code],mt.[Description],mt.[Item Category Code]";

            sqlfrom = " FROM [" + navCompanyName + "LSC Retail Product Group$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public List<ReplProductGroup> ReplicateProductGroups(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Retail Product Group$5ecfc871-5d82-43f1-9c54-59685e82318d");
            string prevLastKey = lastKey;
            string sqlfrom2 = sqlfrom + " LEFT JOIN [" + navCompanyName + "Item$437dbf0e-84ff-417a-965d-ed2bb9650972$ext] it ON it.[LSC Retail Product Code$5ecfc871-5d82-43f1-9c54-59685e82318d]=mt.[Code]";

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(DISTINCT mt.[Code])" + sqlfrom2 + GetWhereStatementWithStoreDist(true, keys, "it.[No_]", storeId, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplProductGroup> list = new List<ReplProductGroup>();

            // get records
            sql = GetSQL(fullReplication, batchSize, true, true) + sqlcolumns + sqlfrom2 + GetWhereStatementWithStoreDist(fullReplication, keys, "it.[No_]", storeId, true);

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
                                list.Add(ReaderToProductGroups(reader, out lastKey));
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

                                list.Add(new ReplProductGroup()
                                {
                                    ItemCategoryID = par[0],
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
                                    list.Add(ReaderToProductGroups(reader, out string ts));
                                }
                                reader.Close();
                            }
                            first = false;
                        }

                        if (actions.Count == 0)
                            lastKey = prevLastKey;

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

        public List<ProductGroup> ProductGroupGetByItemCategoryId(string itemcategoryId, string culture, bool includeChildren, bool includeItems, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            List<ProductGroup> list = new List<ProductGroup>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom + " WHERE mt.[Item Category Code]=@id ORDER BY mt.[Description]";
                    command.Parameters.AddWithValue("@id", itemcategoryId);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyProductGroups(reader, culture, includeChildren, includeItems, stat));
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<ProductGroup> ProductGroupSearch(string search, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            List<ProductGroup> list = new List<ProductGroup>();
            if (string.IsNullOrWhiteSpace(search))
                return list;

            SQLHelper.CheckForSQLInjection(search);

            char[] sep = new char[] { ' ' };
            string[] searchitems = search.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            string sqlwhere = string.Empty;
            foreach (string si in searchitems)
            {
                if (string.IsNullOrEmpty(sqlwhere) == false)
                {
                    sqlwhere += " AND";
                }

                sqlwhere += string.Format(" mt.[Description] LIKE N'%{0}%' {1}", si, GetDbCICollation());
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom + " WHERE" + sqlwhere;

                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyProductGroups(reader, string.Empty, false, false, stat));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public ProductGroup ProductGroupGetById(string id, string culture, bool includeItems, bool includeItemDetail, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ProductGroup prgroup = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom + " WHERE mt.[Code]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            prgroup = ReaderToLoyProductGroups(reader, culture, includeItems, includeItemDetail, stat);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return prgroup;
        }

        private ReplProductGroup ReaderToProductGroups(SqlDataReader reader, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplProductGroup()
            {
                Id = SQLHelper.GetString(reader["Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ItemCategoryID = SQLHelper.GetString(reader["Item Category Code"])
            };
        }

        private ProductGroup ReaderToLoyProductGroups(SqlDataReader reader, string culture, bool includeItems, bool includeItemDetail, Statistics stat)
        {
            ImageRepository imgrepo = new ImageRepository(config, LSCVersion);

            ProductGroup prgr = new ProductGroup()
            {
                Id = SQLHelper.GetString(reader["Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ItemCategoryId = SQLHelper.GetString(reader["Item Category Code"])
            };

            prgr.Images = imgrepo.ImageGetByKey("LSC Retail Product Group", prgr.ItemCategoryId, prgr.Id, string.Empty, 1, false);

            if (includeItems)
            {
                ItemRepository itrep = new ItemRepository(config, LSCVersion);
                prgr.Items = itrep.ItemsGetByProductGroupId(prgr.Id, culture, includeItemDetail, stat);
                ImageRepository imrep = new ImageRepository(config, LSCVersion);
                prgr.Images = imrep.ImageGetByKey("LSC Retail Product Group", prgr.ItemCategoryId, prgr.Id, string.Empty, 0, false);
            }
            else
            {
                prgr.Items = new List<LoyItem>();
            }
            return prgr;
        }
    }
} 