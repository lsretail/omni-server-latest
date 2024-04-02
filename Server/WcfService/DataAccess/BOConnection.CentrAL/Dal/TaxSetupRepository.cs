using System.Collections.Generic;
using System.Data.SqlClient;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
{
    public class TaxSetupRepository : BaseRepository
    {
        // Key: VAT Bus. Posting Group, VAT Prod. Posting Group
        const int TABLEID = 325;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public TaxSetupRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[VAT Bus_ Posting Group],mt.[VAT Prod_ Posting Group],mt.[VAT _]";

            sqlfrom = " FROM [" + navCompanyName + "VAT Posting Setup$437dbf0e-84ff-417a-965d-ed2bb9650972] mt";
        }

        public List<ReplTaxSetup> ReplicateTaxSetup(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("VAT Posting Setup$437dbf0e-84ff-417a-965d-ed2bb9650972");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplTaxSetup> list = new List<ReplTaxSetup>();

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
                                list.Add(ReaderToTaxGroup(reader, out lastKey));
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

                                list.Add(new ReplTaxSetup()
                                {
                                    BusinessTaxGroup = par[0],
                                    ProductTaxGroup = par[1],
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
                                    list.Add(ReaderToTaxGroup(reader, out string ts));
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

        public decimal VATGet(string busCode, string vatCode)
        {
            decimal vat = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[VAT _]" + sqlfrom + " WHERE [VAT Bus_ Posting Group]=@busCode AND [VAT Prod_ Posting Group]=@vatCode";
                    command.Parameters.AddWithValue("@busCode", busCode);
                    command.Parameters.AddWithValue("@vatCode", vatCode);
                    TraceSqlCommand(command);
                    connection.Open();
                    vat = SQLHelper.GetDecimal(command.ExecuteScalar());
                    connection.Close();
                }
            }
            return vat;
        }

        private ReplTaxSetup ReaderToTaxGroup(SqlDataReader reader, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplTaxSetup()
            {
                BusinessTaxGroup = SQLHelper.GetString(reader["VAT Bus_ Posting Group"]),
                ProductTaxGroup = SQLHelper.GetString(reader["VAT Prod_ Posting Group"]),
                TaxPercent = SQLHelper.GetDecimal(reader, "VAT _")
            };
        }
    }
}
 