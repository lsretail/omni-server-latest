using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Pos.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class TerminalRepository : BaseRepository
    {
        // Key: No.
        const int TABLEID = 99001471;

        private string sqltext = string.Empty;
        private string sqlfrom = string.Empty;

        public TerminalRepository(BOConfiguration config) : base(config)
        {
            sqltext = "SELECT mt.[No_],mt.[Store No_],mt.[Terminal Type],mt.[Device Type],mt.[Description],mt.[Exit After Each Trans_]," +
                     "mt.[AutoLogoff After (Min_)],mt.[EFT Store No_],mt.[EFT POS Terminal No_],mt.[Hardware Profile],mt.[ASN Quantity Method]," +
                     "mt.[Interface Profile],mt.[Functionality Profile],mt.[Default Sales Type],mt.[Sales Type Filter]," +
                     "mt.[Inventory Main Menu],mt.[Show Numberpad],mt.[Device License Key],mt.[Item Filtering Method]";

            sqlfrom = " FROM [" + navCompanyName + "LSC POS Terminal$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public List<ReplTerminal> ReplicateTerminals(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<ReplTerminal> list = new List<ReplTerminal>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sqltext + sqlfrom + " WHERE mt.[Store No_]=@id";
                    command.Parameters.AddWithValue("@id", storeId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToTerminal(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }

            // we always replicate everything here
            maxKey = string.Empty;
            lastKey = string.Empty;
            recordsRemaining = 0;

            return list;
        }

        public ReplTerminal TerminalGetById(string id)
        {
            ReplTerminal posTerminal = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sqltext + sqlfrom + " WHERE mt.[No_]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            posTerminal = ReaderToTerminal(reader);
                        }
                    }
                    connection.Close();
                }
            }
            return posTerminal;
        }

        public string TerminalGetLicense(string terminalId, string appid)
        {
            string lic = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mlr.[Device License Key]" +
                                         " FROM [" + navCompanyName + "LSC MobileLicenseRegistration$5ecfc871-5d82-43f1-9c54-59685e82318d] mlr" +
                                         " WHERE mlr.[Terminal ID]=@termid AND mlr.[App ID]=@appid";
                    command.Parameters.AddWithValue("@termid", terminalId);
                    command.Parameters.AddWithValue("@appid", appid);

                    TraceSqlCommand(command);
                    connection.Open();
                    lic = command.ExecuteScalar() as string;
                    connection.Close();
                }
            }
            return lic;
        }

        private void GetFeatureFlags(FeatureFlags flags, string storeId, string terminalId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Feature Flag],mt.[Value] FROM [" + navCompanyName + "LSC Feature Flags Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                                          "WHERE (mt.[POS Terminal]='' OR mt.[POS Terminal]=@tId) AND (mt.[Store No_]='' OR mt.[Store No_]=@sId)";
                    command.Parameters.AddWithValue("@tId", terminalId);
                    command.Parameters.AddWithValue("@sId", storeId);
                    TraceSqlCommand(command);
                    connection.Open();
                    try
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                flags.AddFlag(SQLHelper.GetString(reader["Feature Flag"]), SQLHelper.GetString(reader["Value"]));
                            }
                        }
                    }
                    catch (Exception)
                    {
                        logger.Warn(config.LSKey.Key, "No Feature Flags Loaded");
                    }
                    connection.Close();
                }
            }
        }

        private ReplTerminal ReaderToTerminal(SqlDataReader reader)
        {
            ReplTerminal term = new ReplTerminal()
            {
                Id = SQLHelper.GetString(reader["No_"]),
                Name = SQLHelper.GetString(reader["Description"]),
                EFTStoreId = SQLHelper.GetString(reader["EFT Store No_"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                SFTTerminalId = SQLHelper.GetString(reader["EFT POS Terminal No_"]),
                HardwareProfile = SQLHelper.GetString(reader["Hardware Profile"]),
                VisualProfile = SQLHelper.GetString(reader["Interface Profile"]),
                FunctionalityProfile = SQLHelper.GetString(reader["Functionality Profile"]),
                TerminalType = SQLHelper.GetInt32(reader["Terminal Type"]),
                DeviceType = SQLHelper.GetInt32(reader["Device Type"]),
                HospTypeFilter = SQLHelper.GetString(reader["Sales Type Filter"]),
                DefaultHospType = SQLHelper.GetString(reader["Default Sales Type"]),
                ASNQuantityMethod = (AsnQuantityMethod)SQLHelper.GetInt32(reader["ASN Quantity Method"])
            };

            GetFeatureFlags(term.Features, term.StoreId, term.Id);
            term.Features.AddFlag(FeatureFlagName.ExitAfterEachTransaction, SQLHelper.GetString(reader["Exit After Each Trans_"]));
            term.Features.AddFlag(FeatureFlagName.AutoLogOffAfterMin, SQLHelper.GetString(reader["AutoLogoff After (Min_)"]));
            term.Features.AddFlag(FeatureFlagName.ShowNumberPad, SQLHelper.GetString(reader["Show Numberpad"]));
            return term;
        }

        public Terminal TerminalBaseGetById(string id)
        {
            Terminal posTerminal = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sqltext + sqlfrom + " WHERE mt.[No_]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            posTerminal = ReaderToTerminalBase(reader);
                        }
                    }
                    connection.Close();
                }
            }
            return posTerminal;
        }

        private Terminal ReaderToTerminalBase(SqlDataReader reader)
        {
            Terminal term = new Terminal()
            {
                Id = SQLHelper.GetString(reader["No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                InventoryMainMenuId = SQLHelper.GetString(reader["Inventory Main Menu"]),
                LicenseKey = SQLHelper.GetString(reader["Device License Key"]),
                TerminalType = SQLHelper.GetInt32(reader["Terminal Type"]),
                DeviceType = SQLHelper.GetInt32(reader["Device Type"]),
                ItemFilterMethod = SQLHelper.GetInt32(reader["Item Filtering Method"]),
                Store = new Store(SQLHelper.GetString(reader["Store No_"])),
                AsnQuantityMethod = (AsnQuantityMethod)SQLHelper.GetInt32(reader["ASN Quantity Method"]),
                StoreInventory = true
            };

            GetFeatureFlags(term.Features, term.Store.Id, term.Id);
            term.Features.AddFlag(FeatureFlagName.ExitAfterEachTransaction, SQLHelper.GetString(reader["Exit After Each Trans_"]));
            term.Features.AddFlag(FeatureFlagName.AutoLogOffAfterMin, SQLHelper.GetString(reader["AutoLogoff After (Min_)"]));
            term.Features.AddFlag(FeatureFlagName.ShowNumberPad, SQLHelper.GetString(reader["Show Numberpad"]));
            return term;
        }
    }
}
 