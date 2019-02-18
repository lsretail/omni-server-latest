using System.Collections.Generic;
using System.Data.SqlClient;

using LSRetail.Omni.Domain.DataModel.Pos.Replication;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class TerminalRepository : BaseRepository
    {
        // Key: No.
        const int TABLEID = 99001471;

        private string sqltext = string.Empty;
        private string sqlfrom = string.Empty;

        public TerminalRepository() : base()
        {
            // TODO: do NAV 10 version of the device licence ? 
            sqltext = "SELECT mt.[No_],mt.[Store No_],mt.[Terminal Type],mt.[Device Type],mt.[Description],mt.[Exit After Each Trans_]," +
                     "mt.[AutoLogoff After (Min_)],mt.[EFT Store No_],mt.[EFT POS Terminal No_],mt.[Hardware Profile]," +
                     "mt.[Interface Profile],mt.[Functionality Profile],mt.[Default Sales Type],mt.[Sales Type Filter]," +
                     "mt.[Inventory Main Menu],mt.[Show Numberpad],mt.[Device License Key],mt.[Item Filtering Method]";
            sqlfrom = " FROM [" + navCompanyName + "POS Terminal] mt";
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

        public string TerminalGetLicense(string terminalId, string appid, int navVersion)
        {
            string lic = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    if (navVersion < 10)
                    {
                        command.CommandText = "SELECT mt.[Device License Key]" + sqlfrom + " WHERE mt.[No_]=@termid";
                        command.Parameters.AddWithValue("@termid", terminalId);
                    }
                    else
                    {
                        command.CommandText = "SELECT mlr.[Device License Key]" + sqlfrom +
                                             " INNER JOIN [" + navCompanyName + "MobileLicenseRegistration] mlr ON mt.[No_]=mlr.[Terminal ID]" +
                                             " WHERE mt.[No_]=@termid AND mlr.[App ID]=@appid";
                        command.Parameters.AddWithValue("@termid", terminalId);
                        command.Parameters.AddWithValue("@appid", appid);
                    }

                    TraceSqlCommand(command);
                    connection.Open();
                    lic = command.ExecuteScalar() as string;
                    connection.Close();
                }
            }
            return lic;
        }

        private ReplTerminal ReaderToTerminal(SqlDataReader reader)
        {
            return new ReplTerminal()
            {
                Id = SQLHelper.GetString(reader["No_"]),
                Name = SQLHelper.GetString(reader["Description"]),
                EFTStoreId = SQLHelper.GetString(reader["EFT Store No_"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                SFTTerminalId = SQLHelper.GetString(reader["EFT POS Terminal No_"]),
                HardwareProfile = SQLHelper.GetString(reader["Hardware Profile"]),
                VisualProfile = SQLHelper.GetString(reader["Interface Profile"]),
                FunctionalityProfile = SQLHelper.GetString(reader["Functionality Profile"]),
                ExitAfterEachTransaction = SQLHelper.GetInt32(reader["Exit After Each Trans_"]),
                TerminalType = SQLHelper.GetInt32(reader["Terminal Type"]),
                DeviceType = SQLHelper.GetInt32(reader["Device Type"]),
                HospTypeFilter = SQLHelper.GetString(reader["Sales Type Filter"]),
                DefaultHospType = SQLHelper.GetString(reader["Default Sales Type"]),
                AutoLogOffAfterMin = SQLHelper.GetInt32(reader["AutoLogoff After (Min_)"])
            };
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
            return new Terminal()
            {
                Id = SQLHelper.GetString(reader["No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                InventoryMainMenuId = SQLHelper.GetString(reader["Inventory Main Menu"]),
                ShowNumPad = SQLHelper.GetBool(reader["Show Numberpad"]),
                LicenseKey = SQLHelper.GetString(reader["Device License Key"]),
                TerminalType = SQLHelper.GetInt32(reader["Terminal Type"]),
                DeviceType = SQLHelper.GetInt32(reader["Device Type"]),
                ItemFilterMethod = SQLHelper.GetInt32(reader["Item Filtering Method"]),
                AutoLogOffAfterMin = SQLHelper.GetInt32(reader["AutoLogoff After (Min_)"]),
                Store = new Store(SQLHelper.GetString(reader["Store No_"])),
                StoreInventory = GetStoreInventoryStatus()
            };
        }
    }
}
 