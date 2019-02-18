using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class ContactRepository : BaseRepository
    {
        // Key: Account No.,Contact No.
        const int TABLEID = 99009002;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public ContactRepository() : base()
        {
            sqlcolumns = "mt.[Account No_],mt.[Contact No_],mt.[Name],mt.[E-Mail],mt.[Phone No_],mt.[Mobile Phone No_],mt.[Blocked]," +
                         "mt.[First Name],mt.[Middle Name],mt.[Surname],mt.[Date of Birth],mt.[Gender],mt.[Marital Status],mt.[Home Page]," +
                         "mt.[Address],mt.[Address 2],mt.[City],mt.[Post Code],mt.[Country],mt.[County],mc.[Card No_],mlc.[Login ID],ma.[Club Code],ma.[Scheme Code]";

            sqlfrom = " FROM [" + navCompanyName + "Member Contact] mt" +
                      " INNER JOIN [" + navCompanyName + "Membership Card] mc ON mc.[Contact No_]=mt.[Contact No_] AND (mc.[Last Valid Date]>GETDATE() OR mc.[Last Valid Date]='1753-01-01')" +
                      " LEFT OUTER JOIN [" + navCompanyName + "Member Login Card] mlc ON mlc.[Card No_]=mc.[Card No_]" +
                      " LEFT OUTER JOIN [" + navCompanyName + "Member Account] ma ON ma.[No_]=mt.[Account No_]";
        }
        public List<ReplCustomer> ReplicateMembers(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Member Contact");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)";
                if (batchSize > 0)
                {
                    sql += sqlfrom + GetWhereStatement(true, keys, false);
                }
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, (batchSize > 0) ? keys : null, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplCustomer> list = new List<ReplCustomer>();

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
                                list.Add(ReaderToCustomer(reader, out lastKey));
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

                                list.Add(new ReplCustomer()
                                {
                                    AccountNumber = par[0],
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
                                    list.Add(ReaderToCustomer(reader, out string ts));
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

        public List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned)
        {
            if (maxNumberOfRowsReturned < 1)
                maxNumberOfRowsReturned = 1;

            List<MemberContact> list = new List<MemberContact>();
            string where = string.Empty;
            string order = string.Empty;

            switch (searchType)
            {
                case ContactSearchType.ContactNumber:
                    where = string.Format("mt.[Contact No_] LIKE '{0}%'", search);
                    order = "mt.[Contact No_]";
                    break;

                case ContactSearchType.Email:
                    where = string.Format("mt.[Search E-Mail] LIKE '{0}%' {1}", search, GetDbCICollation());
                    order = "mt.[Search E-Mail]";
                    break;

                case ContactSearchType.Name:
                    where = string.Format("mt.[Search Name] LIKE '{0}%' {1}", search, GetDbCICollation());
                    order = "mt.[Search Name]";
                    break;

                case ContactSearchType.PhoneNumber:
                    where = string.Format("(mt.[Phone No_] LIKE '{0}%' OR mt.[Mobile Phone No_] LIKE '{0}%')", search);
                    break;

                case ContactSearchType.CardId:
                    where = string.Format("mc.[Card No_] LIKE '{0}%'", search);
                    order = "mc.[Card No_]";
                    break;

                case ContactSearchType.UserName:
                    where = string.Format("mlc.[Login ID] LIKE '{0}%'", search);
                    order = "mlc.[Login ID]";
                    break;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT{0} {1}{2} WHERE {3}{4}",
                        (maxNumberOfRowsReturned > 0) ? string.Format(" TOP({0})", maxNumberOfRowsReturned) : string.Empty,
                        sqlcolumns, sqlfrom, where,
                        (string.IsNullOrWhiteSpace(order)) ? string.Empty : " ORDER BY " + order);

                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToContact(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }

            TraceIt(string.Format("Number of Contacts returned: {0} ", list.Count));
            return list;
        }

        public MemberContact ContactGet(ContactSearchType searchType, string searchValue)
        {
            MemberContact contact = null;
            string where = string.Empty;

            if (string.IsNullOrEmpty(searchValue))
                searchValue = string.Empty;

            switch (searchType)
            {
                case ContactSearchType.ContactNumber:
                    where = "mt.[Contact No_]=@id";
                    break;

                case ContactSearchType.Email:
                    where = "mt.[E-Mail]=@id " + GetDbCICollation();
                    break;

                case ContactSearchType.CardId:
                    where = "mc.[Card No_]=@id";
                    break;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom + " WHERE " + where;
                    command.Parameters.AddWithValue("@id", searchValue);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            contact = ReaderToContact(reader);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return contact;
        }

        public MemberContact ContactGetByUserName(string user)
        {
            MemberContact contact = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + ",ml.[Login ID],ml.[Password]" + sqlfrom +
                        " INNER JOIN [" + navCompanyName + "Member Login] ml ON ml.[Login ID]=mlc.[Login ID]" +
                        " WHERE ml.[Login ID]=@id " + GetDbCICollation();

                    command.Parameters.AddWithValue("@id", user);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            contact = ReaderToContact(reader);
                            contact.Password = SQLHelper.GetString(reader["Password"]);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return contact;
        }

        public Device DeviceGetById(string id)
        {
            Device dev = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[ID],mt.[Security Token],mt.[Friendly Name],mt.[Status],mt.[Reason Blocked],mt.[Date Blocked],mt.[Blocked By]" +
                                         " FROM [" + navCompanyName + "Member Device] mt WHERE mt.[ID]=@id";

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dev = new Device()
                            {
                                Id = SQLHelper.GetString(reader["ID"]),
                                DeviceFriendlyName = SQLHelper.GetString(reader["Friendly Name"]),
                                SecurityToken = SQLHelper.GetString(reader["Security Token"]),
                                Status = SQLHelper.GetInt32(reader["Status"]),
                                BlockedReason = SQLHelper.GetString(reader["Reason Blocked"]),
                                BlockedBy = SQLHelper.GetString(reader["Blocked By"]),
                                BlockedDate = SQLHelper.GetDateTime(reader["Date Blocked"])
                            };
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return dev;
        }

        public bool IsUserLinkedToDeviceId(string userName, string deviceId)
        {
            bool isUserLinkedToDevice = true;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Login ID] FROM [" + navCompanyName + "Member Login Device] WHERE [Login ID]=@Lid " +
                        GetDbCICollation() + " AND [Device ID]=@Did";
                    command.Parameters.AddWithValue("@Lid", userName);
                    command.Parameters.AddWithValue("@Did", deviceId);
                    TraceSqlCommand(command);
                    connection.Open();
                    string value = (string)command.ExecuteScalar();
                    if (value == null)
                        isUserLinkedToDevice = false;
                }
                connection.Close();
            }
            return isUserLinkedToDevice;
        }

        public Card CardGetByContactId(string contactId)
        {
            Card card = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    //CardStatus 3 = blocked
                    command.CommandText = "SELECT mt.[Card No_],mt.[Contact No_],mt.[Club Code],mt.[Status]," +
                                 "mt.[Reason Blocked],mt.[Date Blocked],mt.[Blocked by] " +
                                 "FROM [" + navCompanyName + "Membership Card] AS mt " +
                                 "WHERE mt.[Contact No_]=@id AND mt.[Status] != 3 AND (mt.[Last Valid Date]>GETDATE() OR mt.[Last Valid Date]='1753-01-01')";

                    command.Parameters.AddWithValue("@id", contactId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            card = ReaderToCard(reader);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return card;
        }

        public Card CardGetById(string id)
        {
            Card card = null;
            if (string.IsNullOrWhiteSpace(id))
                return null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Card No_],mt.[Contact No_],mt.[Club Code],mt.[Status]," +
                                 "mt.[Reason Blocked],mt.[Date Blocked],mt.[Blocked by] " +
                                 "FROM [" + navCompanyName + "Membership Card] AS mt WHERE mt.[Card No_]=@id";

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            card = ReaderToCard(reader);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return card;
        }

        public Account AccountGetById(string id)
        {
            Account account = null;
            if (string.IsNullOrWhiteSpace(id))
                return null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[No_],mt.[Scheme Code]," +
                                          "(SELECT SUM([Remaining Points]) FROM [" + navCompanyName + "Member Point Entry] " +
                                          "WHERE [Account No_]=@id AND [Open]=1) AS Sum1," +
                                          "(SELECT SUM([Points in Transaction]) FROM [" + navCompanyName + "Member Process Order Entry] " +
                                          "WHERE [Account No_]=@id AND [Date Processed]='1753-01-01') AS Sum2 " +
                                          "FROM [" + navCompanyName + "Member Account] mt WHERE mt.[No_]=@id";

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decimal sum1 = SQLHelper.GetDecimal(reader, "Sum1");
                            decimal sum2 = SQLHelper.GetDecimal(reader, "Sum2");

                            account = new Account()
                            {
                                Id = SQLHelper.GetString(reader["No_"]),
                                Scheme = SchemeGetById(SQLHelper.GetString(reader["Scheme Code"])),
                                PointBalance = (Int64)(sum1 + sum2)
                            };
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return account;
        }

        public List<Scheme> SchemeGetAll()
        {
            List<Scheme> list = new List<Scheme>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Code],mt.[Description],mt.[Club Code],mt.[Update Sequence]," +
                                          "up.[Min_ Point for Upgrade], mt.[Next Scheme Benefits], up.[Code] AS NextScheme " +
                                          "FROM [" + navCompanyName + "Member Scheme] mt " +
                                          "LEFT OUTER JOIN [" + navCompanyName + "Member Scheme] up " +
                                          "ON up.[Club Code]=mt.[Club Code] AND up.[Update Sequence]=mt.[Update Sequence]+1";

                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToScheme(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public Scheme SchemeGetById(string id)
        {
            Scheme scheme = null;
            if (string.IsNullOrWhiteSpace(id))
                return null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Code],mt.[Description],mt.[Club Code],mt.[Update Sequence]," +
                                          "up.[Min_ Point for Upgrade], mt.[Next Scheme Benefits], up.[Code] AS NextScheme " +
                                          "FROM [" + navCompanyName + "Member Scheme] mt " +
                                          "LEFT OUTER JOIN [" + navCompanyName + "Member Scheme] up " +
                                          "ON up.[Club Code]=mt.[Club Code] AND up.[Update Sequence]=mt.[Update Sequence]+1" +
                                          "WHERE mt.[Code]=@id";

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            scheme = ReaderToScheme(reader);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return scheme;
        }

        public Club ClubGetById(string id)
        {
            Club club = null;
            if (string.IsNullOrWhiteSpace(id))
                return null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Code],mt.[Description]" +
                                          "FROM [" + navCompanyName + "Member Club] mt WHERE mt.[Code]=@id";

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            club = new Club()
                            {
                                Id = SQLHelper.GetString(reader["Code"]),
                                Name = SQLHelper.GetString(reader["Description"])
                            };
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return club;
        }

        public List<Profile> ProfileGetAll()
        {
            List<Profile> list = new List<Profile>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Code],a.[Description],a.[Attribute Type],a.[Default Value],a.[Mandatory] " +
                                          "FROM [" + navCompanyName + "Member Attribute Setup] mt " +
                                          "INNER JOIN [" + navCompanyName + "Member Management Setup] mms ON mms.[Mobile Default Club Code]=mt.[Club Code] " +
                                          "INNER JOIN [" + navCompanyName + "Member Attribute] a ON a.[Code]=mt.[Code] " +
                                          "AND a.[Visible Type]=0 AND a.[Lookup Type]=0";

                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToProfile(reader, false));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public virtual List<Profile> ProfileGetByContactId(string id)
        {
            List<Profile> pro = new List<Profile>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Code],a.[Description],a.[Attribute Type],a.[Default Value],a.[Mandatory],v.[Attribute Value] " +
                                          "FROM [" + navCompanyName + "Member Attribute Setup] mt " +
                                          "INNER JOIN [" + navCompanyName + "Member Attribute] a ON a.[Code]=mt.[Code] " +
                                          "INNER JOIN [" + navCompanyName + "Member Attribute Value] v ON v.[Attribute Code]=mt.[Code] " +
                                          "AND a.[Visible Type]=0 AND a.[Lookup Type]=0 AND v.[Contact No_]=@id";

                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pro.Add(ReaderToProfile(reader, true));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return pro;
        }

        public List<Profile> ProfileSearch(string contactId, string search)
        {
            List<Profile> list = new List<Profile>();
            if (string.IsNullOrWhiteSpace(search))
                return list;

            char[] sep = new char[] { ' ' };
            string[] searchitems = search.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            string sqlwhere = " WHERE mc.[Contact No_]=@id";
            foreach (string si in searchitems)
            {
                sqlwhere += string.Format(" AND a.[Description] LIKE N'%{0}%' {1}", si, GetDbCICollation());
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT a.[Code],a.[Description],a.[Attribute Type],a.[Default Value],a.[Mandatory] " +
                                          "FROM [" + navCompanyName + "Member Contact] mc " +
                                          "INNER JOIN [" + navCompanyName + "Member Attribute Setup] ms ON ms.[Club Code]=mc.[Club Code] " +
                                          "INNER JOIN [" + navCompanyName + "Member Attribute] a ON a.[Code]=ms.[Code] " +
                                          "AND a.[Visible Type]=0 AND a.[Lookup Type]=0" + sqlwhere;
                    command.Parameters.AddWithValue("@id", contactId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToProfile(reader, false));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<string> GetCardsByContactId(string contactId)
        {
            List<string> list = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Card No_] FROM [" + navCompanyName + "Membership Card] WHERE [Contact No_]=@cid";
                    command.Parameters.AddWithValue("@cid", contactId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(SQLHelper.GetString(reader["Card No_"]));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private ReplCustomer ReaderToCustomer(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplCustomer()
            {
                Id = SQLHelper.GetString(reader["Contact No_"]),
                AccountNumber = SQLHelper.GetString(reader["Account No_"]),
                Name = SQLHelper.GetString(reader["Name"]),
                Street = SQLHelper.GetString(reader["Address"]),
                City = SQLHelper.GetString(reader["City"]),
                ZipCode = SQLHelper.GetString(reader["Post Code"]),
                Country = SQLHelper.GetString(reader["Country"]),
                County = SQLHelper.GetString(reader["County"]),
                URL = SQLHelper.GetString(reader["Home Page"]),
                Email = SQLHelper.GetString(reader["E-Mail"]),
                CellularPhone = SQLHelper.GetString(reader["Mobile Phone No_"]),
                PhoneLocal = SQLHelper.GetString(reader["Phone No_"]),
                Blocked = SQLHelper.GetInt32(reader["Blocked"]),
                UserName = SQLHelper.GetString(reader["Login ID"]),
                CardId = SQLHelper.GetString(reader["Card No_"]),
                ClubCode = SQLHelper.GetString(reader["Club Code"]),
                SchemeCode = SQLHelper.GetString(reader["Scheme Code"])
            };
        }

        private MemberContact ReaderToContact(SqlDataReader reader)
        {
            MemberContact cont = new MemberContact()
            {
                Id = SQLHelper.GetString(reader["Contact No_"]),
                Phone = SQLHelper.GetString(reader["Phone No_"]),
                MobilePhone = SQLHelper.GetString(reader["Mobile Phone No_"]),
                FirstName = SQLHelper.GetString(reader["First Name"]),
                MiddleName = SQLHelper.GetString(reader["Middle Name"]),
                LastName = SQLHelper.GetString(reader["Surname"]),
                Email = SQLHelper.GetString(reader["E-Mail"]),
                UserName = SQLHelper.GetString(reader["Login ID"]),
                BirthDay = SQLHelper.GetDateTime(reader["Date of Birth"]),
                Gender = (Gender)SQLHelper.GetInt32(reader["Gender"]),
                MaritalStatus = (MaritalStatus)SQLHelper.GetInt32(reader["Marital Status"])
            };

            cont.Card = CardGetById(SQLHelper.GetString(reader["Card No_"]));
            cont.Account = AccountGetById(SQLHelper.GetString(reader["Account No_"]));
            cont.Profiles = ProfileGetByContactId(cont.Id);
            cont.Transactions = new List<LoyTransaction>();

            cont.Addresses = new List<Address>();
            cont.Addresses.Add(new Address()
            {
                Address1 = SQLHelper.GetString(reader["Address"]),
                Address2 = SQLHelper.GetString(reader["Address 2"]),
                City = SQLHelper.GetString(reader["City"]),
                PostCode = SQLHelper.GetString(reader["Post Code"]),
                Country = SQLHelper.GetString(reader["Country"]),
                StateProvinceRegion = SQLHelper.GetString(reader["County"])
            });
            return cont;
        }

        private Card ReaderToCard(SqlDataReader reader)
        {
            return new Card()
            {
                Id = SQLHelper.GetString(reader["Card No_"]),
                ContactId = SQLHelper.GetString(reader["Contact No_"]),
                ClubId = SQLHelper.GetString(reader["Club Code"]),
                Status = (CardStatus)SQLHelper.GetInt32(reader["Status"]),
                BlockedReason = SQLHelper.GetString(reader["Reason Blocked"]),
                BlockedBy = SQLHelper.GetString(reader["Blocked By"]),
                DateBlocked = SQLHelper.GetDateTime(reader["Date Blocked"])
            };
        }

        private Scheme ReaderToScheme(SqlDataReader reader)
        {
            Scheme scheme = new Scheme()
            {
                Id = SQLHelper.GetString(reader["Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                Club = ClubGetById(SQLHelper.GetString(reader["Club Code"])),
                PointsNeeded = SQLHelper.GetInt64(reader["Min_ Point for Upgrade"]),
                Perks = SQLHelper.GetString(reader["Next Scheme Benefits"])
            };

            scheme.NextScheme = SchemeGetById(SQLHelper.GetString(reader["NextScheme"]));
            if (scheme.NextScheme == null)
                scheme.NextScheme = new Scheme();
            return scheme;
        }

        private Profile ReaderToProfile(SqlDataReader reader, bool contactValues)
        {
            Profile pro = new Profile()
            {
                Id = SQLHelper.GetString(reader["Code"]),
                Description = SQLHelper.GetString(reader["Description"]),
                DataType = (ProfileDataType)SQLHelper.GetInt32(reader["Attribute Type"]),
                DefaultValue = SQLHelper.GetString(reader["Default Value"]),
                Mandatory = SQLHelper.GetBool(reader["Mandatory"])
            };
            if (contactValues)
            {
                pro.ContactValue = (SQLHelper.GetString(reader["Attribute Value"]).Equals("Yes", StringComparison.InvariantCultureIgnoreCase)) ? true : false;
            }
            else
            {
                pro.ContactValue = false;
            }
            return pro;
        }
    }
}
 