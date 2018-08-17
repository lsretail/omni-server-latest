using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.Dal
{
    public class ContactRepository : BaseRepository, IContactRepository
    {
        static object statusLock = new object();

        private string contactViewSql =
            "SELECT DISTINCT c.[Id],c.[AlternateId],c.[AccountId],c.[FirstName],c.[MiddleName],c.[LastName],c.[Email]," +
            "c.[CreateDate],c.[ContactStatus],c.[BlockedReason],c.[BlockedDate],c.[BlockedBy],u.[UserId] AS UserName,u.[Password] " +
            "FROM [Contact] AS c " +
            "INNER JOIN [Card] AS cd ON c.[Id]=cd.[ContactId] " +
            "INNER JOIN [UserCard] AS uc ON cd.[Id]=uc.[CardId] " +
            "INNER JOIN [User] AS u ON uc.[UserId]=u.[UserId] ";

        public ContactRepository()
            : base()
        {

        }

        public string Login(MemberContact contact, string deviceId, string cardId, string securityToken)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                throw new Exception(string.Format("User {0} is not linked to device {1}", contact.UserName, deviceId));
            }

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlTransaction trans = connection.BeginTransaction())
                {
                    try
                    {
                        string codevalue = string.Empty;
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.Transaction = trans;
                            if (IsValidLogin(contact.UserName) == false)
                            {
                                // user exists in CRM system, but not in our local db, so create it
                                ContactCreate(contact, deviceId);
                            }

                            command.CommandText = "SELECT * FROM [UserDevice] WHERE [DeviceId]=@dId AND [UserId]=@uId";
                            command.Parameters.AddWithValue("@dId", deviceId);
                            command.Parameters.AddWithValue("@uId", contact.UserName);
                            codevalue = (string)command.ExecuteScalar();
                        }

                        if (string.IsNullOrEmpty(codevalue))
                        {
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "INSERT INTO [UserDevice] (DeviceId, UserId) VALUES (@dId, @uId)";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("dId", deviceId);
                                command.Parameters.AddWithValue("uId", contact.UserName);
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "IF NOT EXISTS (SELECT * FROM [UserCard] WHERE [CardId]=@cId AND [UserId]=@uId) " +
                                                      "INSERT INTO [UserCard] ([CardId],[UserId]) VALUES (@cId,@uId)";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@cId", cardId);
                                command.Parameters.AddWithValue("@uId", contact.UserName);
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "IF NOT EXISTS (SELECT * FROM [Card] WHERE [Id]=@cId) " +
                                                      "INSERT INTO [Card] ([Id],[ContactId],[CardStatus],[ClubId]) VALUES (@cId,@cont,2,'')";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@cId", cardId);
                                command.Parameters.AddWithValue("@cont", contact.Id);
                                command.ExecuteNonQuery();
                            }
                        }

                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.Transaction = trans;
                            command.CommandText = "IF EXISTS (SELECT * FROM [DeviceSecurity] WHERE [DeviceId]=@dId AND [ContactId]=@cId) " +
                                              "UPDATE [DeviceSecurity] SET [SecurityToken]=@tok WHERE [DeviceId]=@dId AND [ContactId]=@cId " +
                                              "ELSE " +
                                              "INSERT INTO [DeviceSecurity] ([SecurityToken],[DeviceId],[ContactId]) VALUES (@tok,@dId,@cId)";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@dId", deviceId);
                            command.Parameters.AddWithValue("@cId", contact.Id);
                            command.Parameters.AddWithValue("@tok", securityToken);
                            command.ExecuteNonQuery();
                        }

                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.Transaction = trans;
                            command.CommandText = "UPDATE [User] SET [LastAccessed]=GETDATE() WHERE [UserId]=@uId";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("uId", contact.UserName);
                            command.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return contact.Id;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public void Logout(string userName, string deviceId)
        {
            MemberContact contact = this.ContactGetByUserName(userName);
            RemoveSecurityToken(deviceId, contact.Id);
        }

        public void LoginLog(string userName, string deviceId, string ipAddress, bool failed, bool logout)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                //lock(statuslock) removed
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO [LoginLog] ([UserName],[LoginType],[Failed],[IPAddress],[DeviceId],[CreateDate])" +
                        " VALUES (@f1,@f2,@f3,@f4,@f5,@f6)";

                    command.Parameters.AddWithValue("@f1", userName);
                    command.Parameters.AddWithValue("@f2", logout ? "O" : "I");
                    command.Parameters.AddWithValue("@f3", failed);
                    command.Parameters.AddWithValue("@f4", ipAddress);
                    command.Parameters.AddWithValue("@f5", deviceId);
                    command.Parameters.AddWithValue("@f6", DateTime.Now);
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public bool DoesDeviceIdExist(string deviceId)
        {
            //return true if the id exists
            return base.DoesRecordExist("[Device]", "[Id]=@0", deviceId);
        }

        public MemberContact ContactGetById(string id, string alternateId)
        {
            string sqlId = id.Trim();
            string sql = contactViewSql + " WHERE c.[Id]=@id ";

            if (string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(alternateId))
            {
                sqlId = alternateId.Trim();
                sql = contactViewSql + " WHERE c.[AlternateId]=@id ";
            }

            MemberContact contact = null;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;

                    command.Parameters.AddWithValue("@id", sqlId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            contact = new MemberContact()
                            {
                                Id = SQLHelper.GetString(reader["Id"]),
                                AlternateId = SQLHelper.GetString(reader["AlternateId"]),
                                Email = SQLHelper.GetString(reader["Email"]),
                                FirstName = SQLHelper.GetString(reader["FirstName"]), 
                                MiddleName = SQLHelper.GetString(reader["MiddleName"]), 
                                LastName = SQLHelper.GetString(reader["LastName"]),
                                UserName = SQLHelper.GetString(reader["UserName"]),
                                Account = new Account(SQLHelper.GetString(reader["AccountId"])),
                                Addresses = new List<Address>()
                            };
                        }
                        reader.Close();
                    }
                }   
                connection.Close();
            }
            return contact;
        }

        public MemberContact ContactGetByCardId(string cardId)
        {
            if (string.IsNullOrWhiteSpace(cardId))
                return null;

            MemberContact contact = null;
            string contactId = "";
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [ContactId] FROM [Card] WHERE [Id]=@id";
                    command.Parameters.AddWithValue("@id", cardId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            contactId = SQLHelper.GetString(reader["ContactId"]);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            if (!string.IsNullOrWhiteSpace(contactId))
                contact = this.ContactGetById(contactId, "");
            return contact;
        }

        public MemberContact ContactGetByUserName(string userName)
        {
            MemberContact contact = null;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = contactViewSql + " WHERE u.[UserId]=@userName";
                    command.Parameters.AddWithValue("@userName", userName);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            contact = new MemberContact()
                            {
                                Id = SQLHelper.GetString(reader["Id"]),
                                AlternateId = SQLHelper.GetString(reader["AlternateId"]),
                                Email = SQLHelper.GetString(reader["Email"]),
                                FirstName = SQLHelper.GetString(reader["FirstName"]),
                                MiddleName = SQLHelper.GetString(reader["MiddleName"]),
                                LastName = SQLHelper.GetString(reader["LastName"]),
                                UserName = SQLHelper.GetString(reader["UserName"]),
                                Account = new Account(SQLHelper.GetString(reader["AccountId"])),
                                Addresses = new List<Address>()
                            };
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return contact;
        }

        //minimal to create a user in case it was first done in eCommerce and not yet replicate to loyalyt
        public void ContactCreate(MemberContact contact, string deviceId)
        {
            //should not happen but check if contactId exists
            if (DoesContactExist(contact.UserName))
            {
                throw new LSOmniServiceException(StatusCode.UserNameExists, string.Format("userName {0} already exists.", contact.UserName));
            }

            contact.UserName = NullToString(contact.UserName, 50);
            contact.Email = NullToString(contact.Email, 80);
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlTransaction trans = connection.BeginTransaction())
                {
                    lock (statusLock)
                    {
                        try
                        {
                            // Create a new contact
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "IF NOT EXISTS (SELECT * FROM [Contact] WHERE [Id]=@f0) " +
                                                      "INSERT INTO [Contact] ([Id],[AccountId],[Email],[FirstName],[LastName]," +
                                                      "[MiddleName],[CreateDate],[BlockedDate],[ContactStatus]) " +
                                                      "VALUES (@f0,@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8)";

                                command.Parameters.AddWithValue("@f0", contact.Id);
                                command.Parameters.AddWithValue("@f1", contact.Account.Id);
                                command.Parameters.AddWithValue("@f2", NullToString(contact.Email, 80));
                                command.Parameters.AddWithValue("@f3", NullToString(contact.FirstName, 30));
                                command.Parameters.AddWithValue("@f4", NullToString(contact.LastName, 30));
                                command.Parameters.AddWithValue("@f5", NullToString(contact.MiddleName, 30));
                                command.Parameters.AddWithValue("@f6", DateTime.Now);
                                command.Parameters.AddWithValue("@f7", DBNull.Value);
                                command.Parameters.AddWithValue("@f8", DBNull.Value);
                                command.ExecuteNonQuery();
                            }

                            //only add account if it does not exist
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "IF NOT EXISTS (SELECT * FROM [Account] WHERE [Id]=@f2) " +
                                                      "INSERT INTO [Account] ([Balance],[Id],[SchemeId]) VALUES (@f1,@f2,@f3)";

                                command.Parameters.AddWithValue("@f1", contact.Account.PointBalance);
                                command.Parameters.AddWithValue("@f2", contact.Account.Id);
                                command.Parameters.AddWithValue("@f3", contact.Account.Scheme.Id);
                                command.ExecuteNonQuery();
                            }

                            //and tie to a temp scheme and club
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "IF NOT EXISTS (SELECT * FROM [Scheme] WHERE [Id]=@f1) " +
                                                      "INSERT INTO [Scheme] ([Id],[Description],[ClubId],[UpdateSequence]," +
                                                      "[MinPointsToUpgrade],[NextSchemeBenefits]) " +
                                                      "VALUES (@f1,@f2,@f3,@f4,@f5,@f6)";

                                command.Parameters.AddWithValue("@f1", contact.Account.Scheme.Id);
                                command.Parameters.AddWithValue("@f2", NullToString(contact.Account.Scheme.Description, 30));
                                command.Parameters.AddWithValue("@f3", contact.Account.Scheme.Club.Id);
                                command.Parameters.AddWithValue("@f4", -999);
                                command.Parameters.AddWithValue("@f5", contact.Account.Scheme.PointsNeeded);
                                command.Parameters.AddWithValue("@f6", (contact.Account.Scheme.NextScheme == null) ? string.Empty : contact.Account.Scheme.NextScheme.Perks);
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "IF NOT EXISTS (SELECT * FROM [Club] WHERE [Id]=@f1) " +
                                                      "INSERT INTO [Club] ([Id],[Description]) VALUES (@f1,@f2)";

                                command.Parameters.AddWithValue("@f1", contact.Account.Scheme.Club.Id);
                                command.Parameters.AddWithValue("@f2", NullToString(contact.Account.Scheme.Club.Name, 30));
                                command.ExecuteNonQuery();
                            }

                            foreach (Profile profile in contact.Profiles)
                            {
                                using (SqlCommand command = connection.CreateCommand())
                                {
                                    command.Transaction = trans;
                                    command.CommandText = "IF NOT EXISTS (SELECT * FROM [ContactProfile] WHERE [AccountId]=@f1 AND [ContactId]=@f2 AND [ProfileId]=@f3) " +
                                                          "INSERT INTO [ContactProfile] ([ClubId],[AccountId],[ContactId],[ProfileId]," +
                                                          "[Value],[OmniValue],[ReplicationCounter],[Status],[ActivationDate]) " +
                                                          "VALUES (@f0,@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8)";

                                    command.Parameters.AddWithValue("@f0", contact.Account.Scheme.Club.Id);
                                    command.Parameters.AddWithValue("@f1", contact.Account.Id);
                                    command.Parameters.AddWithValue("@f2", contact.Id);
                                    command.Parameters.AddWithValue("@f3", profile.Id);
                                    command.Parameters.AddWithValue("@f4", (profile.ContactValue) ? "Yes" : "No"); //NAV wants this Yes/No
                                    command.Parameters.AddWithValue("@f5", string.Empty);
                                    command.Parameters.AddWithValue("@f6", 0);
                                    command.Parameters.AddWithValue("@f7", 0); // 0=active, 1=closed
                                    command.Parameters.AddWithValue("@f8", DateTime.Now);
                                    command.ExecuteNonQuery();
                                }
                            }

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "INSERT INTO [User] ([UserId],[Password],[Blocked]) VALUES (@f0,@f1,@f2)";
                                command.Parameters.AddWithValue("@f0", contact.UserName);
                                command.Parameters.AddWithValue("@f1", Security.NAVHash(contact.Password));
                                command.Parameters.AddWithValue("@f2", 0);
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "IF NOT EXISTS (SELECT * FROM [Card] WHERE [Id]=@f0) " +
                                                      "INSERT INTO [Card] ([Id],[ContactId],[ClubId],[CardStatus],[BlockedReason],[BlockedDate],[BlockedBy]) " +
                                                      "VALUES (@f0, @f1, @f2, @f3, @f4, @f5, @f6)";

                                command.Parameters.AddWithValue("@f0", contact.Card.Id);
                                command.Parameters.AddWithValue("@f1", contact.Id);
                                command.Parameters.AddWithValue("@f2", contact.Account.Scheme.Club.Id);
                                command.Parameters.AddWithValue("@f3", 2); //2=Active
                                command.Parameters.AddWithValue("@f4", DBNull.Value);
                                command.Parameters.AddWithValue("@f5", DBNull.Value);
                                command.Parameters.AddWithValue("@f6", DBNull.Value);
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "INSERT INTO [UserDevice] ([DeviceId],[UserId]) VALUES (@f0,@f1)";
                                command.Parameters.AddWithValue("@f0", deviceId);
                                command.Parameters.AddWithValue("@f1", contact.UserName);
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "INSERT INTO  [UserCard] ([CardId],[UserId]) VALUES (@f0,@f1)";
                                command.Parameters.AddWithValue("@f0", contact.Card.Id);
                                command.Parameters.AddWithValue("@f1", contact.UserName);
                                command.ExecuteNonQuery();
                            }

                            ////NAV 70 - for nav 70 only, still using DeviceUser table
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = string.Format(@"IF EXISTS(SELECT * FROM sys.columns WHERE [name]=N'CardId' AND [object_id]=OBJECT_ID(N'[DeviceUser]')) EXEC('UPDATE [DeviceUser] SET [CardId]=''{0}'' WHERE [DeviceId]=''{1}'' AND [UserId]=''{2}'' ')",
                                    contact.Card.Id, deviceId, contact.UserName);
                                command.ExecuteNonQuery();
                            }

                            //if device already exists then use that Id
                            if (DoesDeviceIdExist(deviceId) == false)
                            {
                                using (SqlCommand command = connection.CreateCommand())
                                {
                                    command.Transaction = trans;
                                    command.CommandText =
                                        "INSERT INTO [Device] ([Id],[DeviceFriendlyName],[DeviceStatus],[BlockedReason],[BlockedDate],[BlockedBy]," +
                                        "[Platform],[OsVersion],[Manufacturer],[Model]) " +
                                        "VALUES (@f0, @f1, @f2, @f3, @f4, @f5, @f6, @f7, @f8, @f9)";

                                    command.Parameters.AddWithValue("@f0", deviceId);
                                    command.Parameters.AddWithValue("@f2", 2);
                                    if (contact.LoggedOnToDevice != null)
                                    {
                                        command.Parameters.AddWithValue("@f1", NullToString(contact.LoggedOnToDevice.DeviceFriendlyName, 50));
                                        command.Parameters.AddWithValue("@f3", DBNull.Value);
                                        command.Parameters.AddWithValue("@f4", DBNull.Value);
                                        command.Parameters.AddWithValue("@f5", DBNull.Value);
                                        command.Parameters.AddWithValue("@f6", NullToString(contact.LoggedOnToDevice.Platform, 50));
                                        command.Parameters.AddWithValue("@f7", NullToString(contact.LoggedOnToDevice.OsVersion, 50));
                                        command.Parameters.AddWithValue("@f8", NullToString(contact.LoggedOnToDevice.Manufacturer, 50));
                                        command.Parameters.AddWithValue("@f9", NullToString(contact.LoggedOnToDevice.Model, 50));
                                    }
                                    else
                                    {
                                        command.Parameters.AddWithValue("@f1", string.Empty);
                                        command.Parameters.AddWithValue("@f3", DBNull.Value);
                                        command.Parameters.AddWithValue("@f4", DBNull.Value);
                                        command.Parameters.AddWithValue("@f5", DBNull.Value);
                                        command.Parameters.AddWithValue("@f6", string.Empty);
                                        command.Parameters.AddWithValue("@f7", string.Empty);
                                        command.Parameters.AddWithValue("@f8", string.Empty);
                                        command.Parameters.AddWithValue("@f9", string.Empty);
                                    }
                                    command.ExecuteNonQuery();
                                }
                            }

                            // Mark the transaction as complete.
                            trans.Commit();
                        }
                        catch (Exception)
                        {
                            trans.Rollback();
                            throw;
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
            }
        }

        public void ContactDelete(string userName)
        {
            MemberContact contact = ContactGetByUserName(userName);
            if (contact == null)
            {
                return;
            }

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlTransaction trans = connection.BeginTransaction())
                {
                    lock (statusLock)
                    {
                        try
                        {
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "DELETE FROM [Contact] WHERE [Id]=@f1";
                                command.Parameters.AddWithValue("@f1", contact.Id);
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "DELETE FROM [Account] WHERE [Id]=@f1";
                                command.Parameters.AddWithValue("@f1", contact.Account.Id);
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "DELETE FROM [ContactProfile] WHERE [ContactId]=@f1";
                                command.Parameters.AddWithValue("@f1", contact.Id);
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "DELETE FROM [Card] WHERE [Id] IN (SELECT [CardId] FROM [UserCard] WHERE [UserId]=@f1)";
                                command.Parameters.AddWithValue("@f1", contact.UserName);
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "DELETE FROM [Device] WHERE [Id] in (SELECT [DeviceId] FROM [UserDevice] WHERE [UserId]=@f1)";
                                command.Parameters.AddWithValue("@f1", contact.UserName);
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "DELETE FROM [User] WHERE [UserId]=@f1";
                                command.Parameters.AddWithValue("@f1", contact.UserName);
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "DELETE FROM [UserCard] WHERE [UserId]=@f1";
                                command.Parameters.AddWithValue("@f1", contact.UserName);
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "DELETE FROM [UserDevice] WHERE [UserId]=@f1";
                                command.Parameters.AddWithValue("@f1", contact.UserName);
                                command.ExecuteNonQuery();
                            }

                            trans.Commit();
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
                connection.Close();
            }
        }

        public string ContactUpdate(MemberContact contact)
        {
            if (DoesContactIdExist(contact.Id) == false)
            {
                throw new LSOmniServiceException(StatusCode.ContactIdNotFound, string.Format("ContactId {0} Not found.", contact.Id));
            }

            AccountRepository dalAccountRepository = new AccountRepository();
            string acctId = "", email = "";

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [AccountId],[Email] FROM [Contact] WHERE [Id]=@f0";
                    command.Parameters.AddWithValue("@f0", contact.Id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            acctId = SQLHelper.GetString(reader["AccountId"]);
                            email = SQLHelper.GetString(reader["Email"]);
                        }
                        else
                        {
                            reader.Close();
                            connection.Close();
                            throw new Exception("Contact not found: " + contact.Id);
                        }
                        reader.Close();
                    }
                }

                contact.Account = dalAccountRepository.AccountGetById(acctId);
                if (contact.Account == null)
                {
                    connection.Close();
                    throw new Exception("Account not found: " + acctId);
                }

                if (email.Trim().ToLower() != contact.Email.Trim().ToLower())
                {
                    //if the email has changed, check if the new one exists in db
                    if (DoesEmailExist(contact.Email))
                    {
                        connection.Close();
                        throw new LSOmniServiceException(StatusCode.EmailExists, string.Format("Email {0} already exists.", contact.Email));
                    }
                }

                using (SqlTransaction trans = connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.Transaction = trans;
                            command.CommandText = "UPDATE [Contact] SET " +
                                "[AlternateId]=@f1,[FirstName]=@f2,[MiddleName]=@f3,[LastName]=@f4,[Email]=@f5 " +
                                "WHERE [Id]=@id";

                            //force the alternate key to be null if no value given
                            if (string.IsNullOrWhiteSpace(contact.AlternateId))
                            {
                                command.Parameters.AddWithValue("@f1", DBNull.Value);
                            }
                            else
                            {
                                command.Parameters.AddWithValue("@f1", contact.AlternateId);
                            }
                            command.Parameters.AddWithValue("@f2", contact.FirstName);
                            command.Parameters.AddWithValue("@f3", contact.MiddleName);
                            command.Parameters.AddWithValue("@f4", contact.LastName);
                            command.Parameters.AddWithValue("@f5", contact.Email);
                            command.Parameters.AddWithValue("@id", contact.Id);
                            command.ExecuteNonQuery();
                        }

                        if (contact.Profiles != null && contact.Profiles.Count >= 0)
                        {
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                command.CommandText = "DELETE FROM [ContactProfile] WHERE [ContactId] = @f0";

                                command.Parameters.AddWithValue("@f0", contact.Id);
                                command.ExecuteNonQuery();
                            }

                            foreach (Profile profile in contact.Profiles)
                            {
                                if (contact.Account.Scheme == null)
                                    throw new ApplicationException("Account Scheme is empty for contactId: " + contact.Id);
                                if (contact.Account.Scheme.Club == null)
                                    throw new ApplicationException("Account Scheme Club is empty for contactId: " + contact.Id);

                                using (SqlCommand command = connection.CreateCommand())
                                {
                                    command.Transaction = trans;
                                    command.CommandText = "INSERT INTO [ContactProfile] ([ClubId],[AccountId],[ContactId],[ProfileId]," +
                                                          "[Value],[OmniValue],[ReplicationCounter],[Status],[ActivationDate]) " +
                                                          "VALUES (@f0,@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8)";

                                    command.Parameters.AddWithValue("@f0", contact.Account.Scheme.Club.Id);
                                    command.Parameters.AddWithValue("@f1", contact.Account.Id);
                                    command.Parameters.AddWithValue("@f2", contact.Id);
                                    command.Parameters.AddWithValue("@f3", profile.Id);
                                    command.Parameters.AddWithValue("@f4", (profile.ContactValue) ? "Yes" : "No"); //NAV wants this Yes/No
                                    command.Parameters.AddWithValue("@f5", string.Empty);
                                    command.Parameters.AddWithValue("@f6", 0);
                                    command.Parameters.AddWithValue("@f7", 0); // 0=active, 1=closed
                                    command.Parameters.AddWithValue("@f8", DateTime.Now);
                                    command.ExecuteNonQuery();
                                }
                            }
                        }

                        // Mark the transaction as complete.
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
                return contact.Id;
            }
        }

        public string DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                lock (statusLock)
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = 
                            "IF EXISTS( SELECT * FROM [Device] WHERE [Id]=@f0 ) " +
                            "UPDATE [Device] SET [Id]=@f0,[DeviceFriendlyName]=@f1,[Platform]=@f6,[OsVersion]=@f7,[Manufacturer]=@f8,[Model]=@f9 WHERE [Id]=@f0 " +
                            "ELSE " +
                            "INSERT INTO [Device] ([Id],[DeviceFriendlyName],[DeviceStatus],[BlockedReason]," +
                            "[BlockedDate],[BlockedBy],[Platform],[OsVersion],[Manufacturer],[Model]) " +
                            "VALUES (@f0,@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8,@f9)";

                        command.Parameters.AddWithValue("@f0", deviceId);
                        command.Parameters.AddWithValue("@f1", NullToString(deviceFriendlyName, 50));
                        command.Parameters.AddWithValue("@f2", 2);
                        command.Parameters.AddWithValue("@f3", DBNull.Value);
                        command.Parameters.AddWithValue("@f4", DBNull.Value);
                        command.Parameters.AddWithValue("@f5", DBNull.Value);
                        command.Parameters.AddWithValue("@f6", NullToString(platform, 50));
                        command.Parameters.AddWithValue("@f7", NullToString(osVersion, 50));
                        command.Parameters.AddWithValue("@f8", NullToString(manufacturer, 50));
                        command.Parameters.AddWithValue("@f9", NullToString(model, 50));
                        TraceSqlCommand(command);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
            return deviceId;
        }

        public void ChangePassword(string userName, string newPassword, string oldPassword)
        {
            if (base.DoesRecordExist("[User]", "[UserId]=@0", userName) == false)
                throw new LSOmniServiceException(StatusCode.ContactIdNotFound, string.Format("UserName {0} Not found.", userName));

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE [User] SET [Password]=@f0 WHERE [UserId]=@id";
                    command.Parameters.AddWithValue("@f0", Security.NAVHash(newPassword));
                    command.Parameters.AddWithValue("@id", userName);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void UpdateAccountBalance(string accountNumber, double balance)
        {
            if (base.DoesRecordExist("[Account]", "[Id]=@0", accountNumber) == false)
                throw new LSOmniServiceException(StatusCode.AccountNotFound, string.Format("Account {0} Not found.", accountNumber));

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE [Account] SET [Balance]=@f0 WHERE [Id]=@id";
                    command.Parameters.AddWithValue("@f0", (decimal) balance);
                    command.Parameters.AddWithValue("@id", accountNumber);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public bool DoesContactExist(string userName)
        {
            //return true if the username exists
            return base.DoesRecordExist("[User]", "[UserId]=@0", userName);
        }

        public bool DoesEmailExist(string email)
        {
            //return true if the username exists
            return base.DoesRecordExist("[Contact]", "[Email]=@0", email);
        }

        public string ContactGetAccountId(string contactId)
        {
            string ret = "";
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [AccountId] FROM [Contact] WHERE [Id]=@id";
                    command.Parameters.AddWithValue("@id", contactId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ret = SQLHelper.GetString(reader["AccountId"]);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return ret;
        }

        public void DeleteSecurityTokens(string contactId, string skipThisDeviceId = "")
        {
            List<string> deviceIdsToUpdate = new List<string>();

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT d.[Id] AS DeviceId " +
                                          "FROM [Contact] AS c " +
                                          "INNER JOIN [Card] AS cd ON c.[Id]=cd.[ContactId] " +
                                          "INNER JOIN [UserCard] AS uc ON cd.[Id]=uc.[CardId] " +
                                          "INNER JOIN [User] AS u ON uc.[UserId]=u.[UserId] " +
                                          "INNER JOIN [UserDevice] AS ud ON ud.[UserId]=uc.[UserId] " +
                                          "INNER JOIN [Device] AS d ON ud.[DeviceId]=d.[Id] " +
                                          "WHERE c.[Id]=@id AND d.[Id] != @f0";

                    command.Parameters.AddWithValue("@id", contactId);
                    command.Parameters.AddWithValue("@f0", skipThisDeviceId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            deviceIdsToUpdate.Add(SQLHelper.GetString(reader["DeviceId"]));
                        }
                    }
                }
                connection.Close();
            }
            //delete all security tokens except for this device

            //now update the securitytoken to blank
            foreach (string deviceId in deviceIdsToUpdate)
            {
                RemoveSecurityToken(deviceId, contactId);
            }
        }

        protected internal string ContactIdGetByCardId(string cardId)
        {
            string contactId = "";
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [ContactId] FROM [Card] WHERE [Id]=@id";
                    command.Parameters.AddWithValue("@id", cardId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            contactId = SQLHelper.GetString(reader["ContactId"]);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return contactId;
        }

        #region private

        private bool IsValidLogin(string userName)
        {
            //password = Security.NAVHash(password);

            bool ret = false;
            //now check if this contact has a device linked to it.  ContactStatus is null when not blocked
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = contactViewSql + " WHERE u.[UserId]=@username AND c.[ContactStatus] IS NULL";
                    command.Parameters.AddWithValue("@username", userName);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ret = true;
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return ret;
        }

        private bool DoesContactIdExist(string id)
        {
            //return true if the id exists
            return base.DoesRecordExist("[Contact]", "[Id]=@0", id);
        }

        private void RemoveSecurityToken(string deviceId, string contactId)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM [DeviceSecurity] WHERE [DeviceId]=@f0 AND [ContactId]=@f1";
                    command.Parameters.AddWithValue("@f0", deviceId);
                    command.Parameters.AddWithValue("@f1", contactId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        #endregion private
    }
}

