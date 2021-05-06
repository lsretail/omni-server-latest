﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class ContactRepository : BaseRepository
    {
        // Key: Account No.,Contact No.
        const int TABLEID = 99009002;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public ContactRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Account No_],mt.[Contact No_],mt.[Name],mt.[E-Mail],mt.[Phone No_],mt.[Mobile Phone No_],mt.[Blocked]," +
                         "mt.[First Name],mt.[Middle Name],mt.[Surname],mt.[Date of Birth],mt.[Gender],mt.[Marital Status],mt.[Home Page]," +
                         "mt.[Address],mt.[Address 2],mt.[City],mt.[Post Code],mt.[County],ma.[Club Code],ma.[Scheme Code],mt.[Country_Region Code]";

            sqlfrom = " FROM [" + navCompanyName + "LSC Member Contact$5ecfc871-5d82-43f1-9c54-59685e82318d] mt" +
                      " LEFT OUTER JOIN [" + navCompanyName + "LSC Member Account$5ecfc871-5d82-43f1-9c54-59685e82318d] ma ON ma.[No_]=mt.[Account No_]";
        }

        public List<ReplCustomer> ReplicateMembers(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Member Contact$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

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

        public List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned, bool exact)
        {
            if (maxNumberOfRowsReturned < 1)
                maxNumberOfRowsReturned = 0;

            List<MemberContact> list = new List<MemberContact>();
            string col = sqlcolumns;
            string from = sqlfrom;
            string where = string.Empty;
            string order = string.Empty;
            string like = (exact) ? "=" : " LIKE ";

            switch (searchType)
            {
                case ContactSearchType.ContactNumber:
                    where = string.Format("mt.[Contact No_]{0}@value", like);
                    order = "mt.[Contact No_]";
                    break;

                case ContactSearchType.Email:
                    where = string.Format("mt.[Search E-Mail]{0}@value {1}", like, GetDbCICollation());
                    order = "mt.[Search E-Mail]";
                    break;

                case ContactSearchType.Name:
                    where = string.Format("mt.[Search Name]{0}@value {1}", like, GetDbCICollation());
                    order = "mt.[Search Name]";
                    break;

                case ContactSearchType.PhoneNumber:
                    where = string.Format("(mt.[Phone No_]{0}@value OR mt.[Mobile Phone No_]{0}@value)", like);
                    break;

                case ContactSearchType.CardId:
                    col += ", mc.[Card No_] ";
                    from += " LEFT OUTER JOIN [" + navCompanyName + "LSC Membership Card$5ecfc871-5d82-43f1-9c54-59685e82318d] mc on mc.[Contact No_]=mt.[Contact No_] ";
                    where = "mc.[Card No_]=@value";
                    order = "mc.[Card No_]";
                    break;

                case ContactSearchType.UserName:
                    col += ", mlc.[Login ID]";
                    from += " LEFT OUTER JOIN [" + navCompanyName + "LSC Membership Card$5ecfc871-5d82-43f1-9c54-59685e82318d] mc on mc.[Contact No_]=mt.[Contact No_] " +
                            "LEFT OUTER JOIN[" + navCompanyName + "LSC Member Login Card$5ecfc871-5d82-43f1-9c54-59685e82318d] mlc on mc.[Card No_]=mlc.[Card No_]";
                    where = string.Format("mlc.[Login ID]{0}@value {1}", like, GetDbCICollation());
                    order = "mlc.[Login ID]";
                    break;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT{0} {1}{2} WHERE {3}{4}",
                        (maxNumberOfRowsReturned > 0) ? string.Format(" TOP({0})", maxNumberOfRowsReturned) : string.Empty,
                        col, from, where,
                        (string.IsNullOrWhiteSpace(order)) ? string.Empty : " ORDER BY " + order);

                    command.Parameters.AddWithValue("@value", ((exact) ? search : $"%{search}%"));
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
            string col = sqlcolumns;
            string from = sqlfrom;
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
                    col += ", mc.[Card No_] ";
                    from += " LEFT OUTER JOIN [" + navCompanyName + "LSC Membership Card$5ecfc871-5d82-43f1-9c54-59685e82318d] mc on mc.[Contact No_]=mt.[Contact No_] ";
                    where = "mc.[Card No_]=@id";
                    break;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + col + from + " WHERE " + where;
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
                    command.CommandText = 
                        "SELECT " + sqlcolumns + ",ml.[Login ID],ml.[Password]" + sqlfrom +
                        " LEFT OUTER JOIN["+ navCompanyName + "LSC Membership Card$5ecfc871-5d82-43f1-9c54-59685e82318d] mc on mc.[Contact No_]=mt.[Contact No_]" +
                        " LEFT OUTER JOIN["+ navCompanyName + "LSC Member Login Card$5ecfc871-5d82-43f1-9c54-59685e82318d] mlc on mc.[Card No_]=mlc.[Card No_]" +
                        " INNER JOIN [" + navCompanyName + "LSC Member Login$5ecfc871-5d82-43f1-9c54-59685e82318d] ml ON ml.[Login ID]=mlc.[Login ID]" +
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
                                         " FROM [" + navCompanyName + "LSC Member Device$5ecfc871-5d82-43f1-9c54-59685e82318d] mt WHERE mt.[ID]=@id";

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
                                BlockedDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Date Blocked"]), config.IsJson)
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
                    command.CommandText = "SELECT [Login ID] FROM [" + navCompanyName + "LSC Member Login Device$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [Login ID]=@Lid " +
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

        public List<Card> CardsGetByContactId(string contactId, out string username)
        {
            List<Card> list = new List<Card>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    //CardStatus 3 = blocked
                    command.CommandText = "SELECT mt.[Card No_],mt.[Contact No_],mt.[Club Code],mt.[Status]," +
                                 "mt.[Reason Blocked],mt.[Date Blocked],mt.[Blocked by], ml.[Login ID] " +
                                 "FROM [" + navCompanyName + "LSC Membership Card$5ecfc871-5d82-43f1-9c54-59685e82318d] AS mt " +
                                 "LEFT OUTER JOIN[" + navCompanyName + "LSC Member Login Card$5ecfc871-5d82-43f1-9c54-59685e82318d] ml on mt.[Card No_] = ml.[Card No_]" +
                                 "WHERE mt.[Contact No_]=@id AND mt.[Status] != 3 AND (mt.[Last Valid Date]>GETDATE() OR mt.[Last Valid Date]='1753-01-01')";

                    command.Parameters.AddWithValue("@id", contactId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToCard(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            username = list.FirstOrDefault(c => string.IsNullOrEmpty(c.LoginId) == false)?.LoginId ?? string.Empty;
            return list;
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
                                 "FROM [" + navCompanyName + "LSC Membership Card$5ecfc871-5d82-43f1-9c54-59685e82318d] AS mt WHERE mt.[Card No_]=@id";

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
                                          "(SELECT SUM([Remaining Points]) FROM [" + navCompanyName + "LSC Member Point Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                                          "WHERE [Account No_]=@id AND [Open]=1) AS Sum1," +
                                          "(SELECT SUM([Points in Transaction]) FROM [" + navCompanyName + "LSC Member Process Order Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                                          "WHERE [Account No_]=@id AND [Date Processed]='1753-01-01') AS Sum2 " +
                                          "FROM [" + navCompanyName + "LSC Member Account$5ecfc871-5d82-43f1-9c54-59685e82318d] mt WHERE mt.[No_]=@id";

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
                                          "FROM [" + navCompanyName + "LSC Member Scheme$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                                          "LEFT OUTER JOIN [" + navCompanyName + "LSC Member Scheme$5ecfc871-5d82-43f1-9c54-59685e82318d] up " +
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
                                          "FROM [" + navCompanyName + "LSC Member Scheme$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                                          "LEFT OUTER JOIN [" + navCompanyName + "LSC Member Scheme$5ecfc871-5d82-43f1-9c54-59685e82318d] up " +
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
                                          "FROM [" + navCompanyName + "LSC Member Club$5ecfc871-5d82-43f1-9c54-59685e82318d] mt WHERE mt.[Code]=@id";

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
                                          "FROM [" + navCompanyName + "LSC Member Attribute Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                                          "INNER JOIN [" + navCompanyName + "LSC Member Management Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] mms ON mms.[Mobile Default Club Code]=mt.[Club Code] " +
                                          "INNER JOIN [" + navCompanyName + "LSC Member Attribute$5ecfc871-5d82-43f1-9c54-59685e82318d] a ON a.[Code]=mt.[Code] " +
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

        public virtual List<Profile> ProfileGetByCardId(string id)
        {
            List<Profile> pro = new List<Profile>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Code],a.[Description],a.[Attribute Type],a.[Default Value],a.[Mandatory],v.[Attribute Value] " +
                                          "FROM [" + navCompanyName + "LSC Member Attribute Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                                          "INNER JOIN [" + navCompanyName + "LSC Membership Card$5ecfc871-5d82-43f1-9c54-59685e82318d] mc ON mc.[Card No_]=@id " +
                                          "INNER JOIN [" + navCompanyName + "LSC Member Attribute$5ecfc871-5d82-43f1-9c54-59685e82318d] a ON a.[Code]=mt.[Code] " +
                                          "INNER JOIN [" + navCompanyName + "LSC Member Attribute Value$5ecfc871-5d82-43f1-9c54-59685e82318d] v ON v.[Attribute Code]=mt.[Code] " +
                                          "AND a.[Visible Type]=0 AND a.[Lookup Type]=0 AND v.[Contact No_]=mc.[Contact No_]";

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

        public List<Profile> ProfileSearch(string cardId, string search)
        {
            List<Profile> list = new List<Profile>();
            if (string.IsNullOrWhiteSpace(search))
                return list;

            SQLHelper.CheckForSQLInjection(search);

            char[] sep = new char[] { ' ' };
            string[] searchitems = search.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            string sqlwhere = " WHERE c.[Card No_]=@id";
            foreach (string si in searchitems)
            {
                sqlwhere += string.Format(" AND a.[Description] LIKE N'%{0}%' {1}", si, GetDbCICollation());
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT a.[Code],a.[Description],a.[Attribute Type],a.[Default Value],a.[Mandatory] " +
                                          "FROM [" + navCompanyName + "LSC Member Contact$5ecfc871-5d82-43f1-9c54-59685e82318d] mc " +
                                          "INNER JOIN [" + navCompanyName + "LSC Member Attribute Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] ms ON ms.[Club Code]=mc.[Club Code] " +
                                          "INNER JOIN [" + navCompanyName + "LSC Member Attribute$5ecfc871-5d82-43f1-9c54-59685e82318d] a ON a.[Code]=ms.[Code] " +
                                          "INNER JOIN[" + navCompanyName + "LSC Membership Card$5ecfc871-5d82-43f1-9c54-59685e82318d] c on c.[Contact No_]=mc.[Contact No_]" +
                                          "AND a.[Visible Type]=0 AND a.[Lookup Type]=0" + sqlwhere;
                    command.Parameters.AddWithValue("@id", cardId);
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

        public GiftCard GetGiftCartBalance(string cardId, string type)
        {
            GiftCard card = new GiftCard(cardId);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Expiring Date] AS Exp, (SELECT SUM([Amount]) " +
                                          "FROM [" + navCompanyName + "LSC Voucher Entries$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                                          "WHERE [Voucher No_]=mt.[Entry Code] AND [Voucher Type]=mt.[Entry Type]) AS Amt " +
                                          "FROM [" + navCompanyName + "LSC POS Data Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                                          "WHERE mt.[Entry Type]=@type AND mt.[Entry Code]=@id";

                    command.Parameters.AddWithValue("@id", cardId);
                    command.Parameters.AddWithValue("@type", type);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            card.Balance = SQLHelper.GetDecimal(reader, "Amt");
                            card.ExpireDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Exp"]), config.IsJson);
                        }
                        else
                        {
                            connection.Close();
                            throw new LSOmniServiceException(StatusCode.GiftCardNotFound, "Gift Card not found");
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return card;
        }

        private ReplCustomer ReaderToCustomer(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            ReplCustomer cust = new ReplCustomer()
            {
                Id = SQLHelper.GetString(reader["Contact No_"]),
                AccountNumber = SQLHelper.GetString(reader["Account No_"]),
                Name = SQLHelper.GetString(reader["Name"]),
                FirstName = SQLHelper.GetString(reader["First Name"]),
                MiddleName = SQLHelper.GetString(reader["Middle Name"]),
                LastName = SQLHelper.GetString(reader["Surname"]),
                Street = SQLHelper.GetString(reader["Address"]),
                City = SQLHelper.GetString(reader["City"]),
                ZipCode = SQLHelper.GetString(reader["Post Code"]),
                Country = SQLHelper.GetString(reader["Country_Region Code"]),
                County = SQLHelper.GetString(reader["County"]),
                URL = SQLHelper.GetString(reader["Home Page"]),
                Email = SQLHelper.GetString(reader["E-Mail"]),
                CellularPhone = SQLHelper.GetString(reader["Mobile Phone No_"]),
                PhoneLocal = SQLHelper.GetString(reader["Phone No_"]),
                Blocked = SQLHelper.GetInt32(reader["Blocked"]),
                ClubCode = SQLHelper.GetString(reader["Club Code"]),
                SchemeCode = SQLHelper.GetString(reader["Scheme Code"])
            };
            cust.Cards = CardsGetByContactId(cust.Id, out string username);
            cust.UserName = username;
            return cust;
        }

        private MemberContact ReaderToContact(SqlDataReader reader)
        {
            MemberContact cont = new MemberContact()
            {
                Id = SQLHelper.GetString(reader["Contact No_"]),
                FirstName = SQLHelper.GetString(reader["First Name"]),
                MiddleName = SQLHelper.GetString(reader["Middle Name"]),
                LastName = SQLHelper.GetString(reader["Surname"]),
                Email = SQLHelper.GetString(reader["E-Mail"]),
                BirthDay = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Date of Birth"]), config.IsJson),
                Gender = (Gender)SQLHelper.GetInt32(reader["Gender"]),
                MaritalStatus = (MaritalStatus)SQLHelper.GetInt32(reader["Marital Status"])
            };

            cont.Cards = CardsGetByContactId(cont.Id, out string username);
            cont.UserName = username;
            cont.Account = AccountGetById(SQLHelper.GetString(reader["Account No_"]));
            if (cont.Cards.Count > 0)
                cont.Profiles = ProfileGetByCardId(cont.Cards[0].Id);

            cont.SalesEntries = new List<SalesEntry>();
            cont.OneLists = new List<OneList>();
            cont.Addresses = new List<Address>()
            {
                new Address()
                {
                    Address1 = SQLHelper.GetString(reader["Address"]),
                    Address2 = SQLHelper.GetString(reader["Address 2"]),
                    City = SQLHelper.GetString(reader["City"]),
                    PostCode = SQLHelper.GetString(reader["Post Code"]),
                    Country = SQLHelper.GetString(reader["Country_Region Code"]),
                    StateProvinceRegion = SQLHelper.GetString(reader["County"]),
                    PhoneNumber = SQLHelper.GetString(reader["Phone No_"]),
                    CellPhoneNumber = SQLHelper.GetString(reader["Mobile Phone No_"])
                }
            };
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
                DateBlocked = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Date Blocked"]), config.IsJson),
                LoginId = SQLHelper.GetString(reader["Login ID"])
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
 