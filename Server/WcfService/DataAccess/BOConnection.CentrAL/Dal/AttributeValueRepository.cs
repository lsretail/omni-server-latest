using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
{
    public class AttributeValueRepository : BaseRepository
    {
        // Key: Link Type,Link Field 1,Link Field 2,Link Field 3,Attribute Code,Sequence
        const int TABLEID = 10000786;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public AttributeValueRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Link Type],mt.[Link Field 1],mt.[Link Field 2],mt.[Link Field 3],mt.[Attribute Code],mt.[Attribute Value],mt.[Numeric Value],mt.[Sequence]";

            sqlfrom = " FROM [" + navCompanyName + "Attribute Value$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public List<ReplAttributeValue> ReplicateEcommAttributeValue(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Attribute Value$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplAttributeValue> list = new List<ReplAttributeValue>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(fullReplication, keys, true);

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
                                list.Add(ReaderToEcommAttributeValue(reader, out lastKey));
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
                                if (par.Length < 6 || par.Length != keys.Count)
                                    continue;

                                list.Add(new ReplAttributeValue()
                                {
                                    LinkType = Convert.ToInt32(par[0]),
                                    LinkField1 = par[1],
                                    LinkField2 = par[2],
                                    LinkField3 = par[3],
                                    Code = par[4],
                                    Sequence = Convert.ToInt32(par[5]),
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
                                    list.Add(ReaderToEcommAttributeValue(reader, out string ts));
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

        /// <summary>
        /// Get Attributes by type and value
        /// </summary>
        /// <param name="value">Link Field 1</param>
        /// <param name="type">Link Type</param>
        /// <returns></returns>
        public List<RetailAttribute> AttributesGet(string value, AttributeLinkType type)
        {
            List<RetailAttribute> list = new List<RetailAttribute>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandText = "SELECT " + sqlcolumns + ",a.[Description],a.[Value Type],a.[Default Value]" + sqlfrom +
                        " LEFT JOIN [" + navCompanyName + "Attribute$5ecfc871-5d82-43f1-9c54-59685e82318d] a ON a.[Code]=mt.[Attribute Code]" + " WHERE mt.[Link Field 1]=@id AND mt.[Link Type]=@type" ;
                    command.Parameters.AddWithValue("@id", value);
                    command.Parameters.AddWithValue("@type", (int)type);

                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToRetailAttribute(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private RetailAttribute ReaderToRetailAttribute(SqlDataReader reader)
        {
            RetailAttribute attr = new RetailAttribute()
            {
                Code = SQLHelper.GetString(reader["Attribute Code"]),
                Value = SQLHelper.GetString(reader["Attribute Value"]),
                NumericValue = SQLHelper.GetInt32(reader["Numeric Value"]),
                LinkType = (AttributeLinkType)SQLHelper.GetInt32(reader["Link Type"]),
                LinkField1 = SQLHelper.GetString(reader["Link Field 1"]),
                LinkField2 = SQLHelper.GetString(reader["Link Field 2"]),
                LinkField3 = SQLHelper.GetString(reader["Link Field 3"]),
                Sequence = SQLHelper.GetInt32(reader["Sequence"]),
                DefaultValue = SQLHelper.GetString(reader["Default Value"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ValueType = (AttributeValueType)SQLHelper.GetInt32(reader["Value Type"])
            };

            AttributeOptionValueRepository rep = new AttributeOptionValueRepository(config);
            attr.OptionValues = rep.GetOptionValues(attr.Code);
            return attr;
        }

        private ReplAttributeValue ReaderToEcommAttributeValue(SqlDataReader reader, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplAttributeValue()
            {
                Code = SQLHelper.GetString(reader["Attribute Code"]),
                Value = SQLHelper.GetString(reader["Attribute Value"]),
                NumbericValue = SQLHelper.GetInt32(reader["Numeric Value"]),
                LinkType = SQLHelper.GetInt32(reader["Link Type"]),
                LinkField1 = SQLHelper.GetString(reader["Link Field 1"]),
                LinkField2 = SQLHelper.GetString(reader["Link Field 2"]),
                LinkField3 = SQLHelper.GetString(reader["Link Field 3"]),
                Sequence = SQLHelper.GetInt32(reader["Sequence"])
            };
        }
    }
}
