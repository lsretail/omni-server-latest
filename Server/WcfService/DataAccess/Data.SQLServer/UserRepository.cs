using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Portal;

namespace LSOmni.DataAccess.Dal
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(BOConfiguration config) : base(config)
        {
        }

        public bool IsAdmin(string username)
        {
            bool ret = false;

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Admin] FROM [Users] WHERE UPPER([Username])=@usr";
                    command.Parameters.AddWithValue("@usr", username.ToUpper());
                    TraceSqlCommand(command);
                    connection.Open();
                    ret = (bool)command.ExecuteScalar();
                }
                connection.Close();
            }
            return ret;
        }

        public bool HasAccess(string username, string lskey)
        {
            bool ret = false;

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Username], [LSKey] FROM [UserKeys] WHERE UPPER([Username])=@usr";
                    command.Parameters.AddWithValue("@usr", username.ToUpper());
                    TraceSqlCommand(command);
                    connection.Open();
                    ret = command.ExecuteScalar() == null ? false : true;
                }
                connection.Close();
            }
            return ret;
        }

        public string GetPassword(string username)
        {
            string ret = string.Empty;

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Password] FROM [Users] WHERE UPPER([Username])=@usr";
                    command.Parameters.AddWithValue("@usr", username.ToUpper());
                    TraceSqlCommand(command);
                    connection.Open();
                    ret = (string)command.ExecuteScalar();
                }
                connection.Close();
            }
            return ret;
        }

        public void ChangePassword(string username, string newPassword)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE [Users] SET [Password]=@newPass WHERE UPPER([Username])=@usr";
                    command.Parameters.AddWithValue("@newPass", newPassword);
                    command.Parameters.AddWithValue("@usr", username.ToUpper());
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void CreatePortalUser(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandText = "SELECT UPPER([Username]) FROM [Users] WHERE [Username]=@usr ";
                    command.Parameters.AddWithValue("@usr", username.ToUpper());
                    TraceSqlCommand(command);
                    if (command.ExecuteScalar() != null)
                    {
                        connection.Close();
                        throw new LSOmniServiceException(StatusCode.UserNameExists, "A user with that username already exists");
                    }

                    command.CommandText = "INSERT INTO [Users] ([Username],[Password]) VALUES (@usr, @pwd)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@usr", username);
                    command.Parameters.AddWithValue("@pwd", password);
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void SavePortalUser(PortalUser user)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    
                    command.CommandText = "UPDATE [Users] SET [Active]=@act WHERE  UPPER([Username])=@usr ";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@usr", user.UserName.ToUpper());
                    command.Parameters.AddWithValue("@act", user.Active);
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM [UserKeys] WHERE UPPER([Username])=@usr";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@usr", user.UserName.ToUpper());
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();

                    foreach (LSKey key in user.LSKeys)
                    {
                        command.CommandText = "INSERT INTO [UserKeys] ([Username],[LSKey]) VALUES (@usr, @key)";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@usr", user.UserName);
                        command.Parameters.AddWithValue("@key", key.Key);
                        TraceSqlCommand(command);
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
        }

        public List<LSKey> GetKeys(string username)
        {
            List<LSKey> list = new List<LSKey>();

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT k.[LSKey],k.[Description],k.[Active] " +
                        "FROM [UserKeys] u " +
                        "INNER JOIN [LSKeys] k on u.[LSKey] = k.[LSKey] " +
                        " WHERE UPPER(u.[Username])=@usr ";
                    command.Parameters.AddWithValue("@usr", username.ToUpper());
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LSKey key = new LSKey();
                            key.Key = SQLHelper.GetString(reader["LSKey"]);
                            key.Description = SQLHelper.GetString(reader["Description"]);
                            key.Active = SQLHelper.GetBool(reader["Active"]);
                            list.Add(key);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return list;
        }

        public List<LSKey> GetAllKeys(string username, bool isAdmin)
        {
            List<LSKey> list = new List<LSKey>();

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT k.[LSKey],k.[Description],k.[Active] " +
                        "FROM [LSKeys] k ";
                    if(isAdmin == false)
                    {
                        command.CommandText += "INNER JOIN [UserKeys] uk on uk.[LSKey] = k.[LSKey] " +
                            "WHERE UPPER(uk.[Username])=@usr ";
                        command.Parameters.AddWithValue("@usr", username.ToUpper());
                    }
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LSKey key = new LSKey();
                            key.Key = SQLHelper.GetString(reader["LSKey"]);
                            key.Description = SQLHelper.GetString(reader["Description"]);
                            key.Active = SQLHelper.GetBool(reader["Active"]);
                            list.Add(key);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return list;
        }

        public List<PortalUser> GetPortalUsers()
        {
            List<PortalUser> list = new List<PortalUser>();

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Username], [Active] FROM [Users] WHERE [Admin]=0";
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PortalUser user = new PortalUser();
                            user.UserName = SQLHelper.GetString(reader["Username"]);
                            user.Active = SQLHelper.GetBool(reader["Active"]);
                            list.Add(user);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            foreach (PortalUser user in list)
            {
                user.LSKeys = GetKeys(user.UserName);
            }
            return list;
        }

        public PortalUser Login(string username)
        {
            PortalUser user = new PortalUser();

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "IF EXISTS(SELECT[Username] FROM [UserLogin] WHERE UPPER([Username])= UPPER(@usr)) " +
                            "BEGIN UPDATE [UserLogin] SET [DateModified]=GETDATE(), [Token]=UPPER(@tok) WHERE UPPER([Username])=UPPER(@usr) END " +
                            "ELSE " +
                            "BEGIN INSERT INTO [UserLogin] ([Username], [Token], [DateModified]) " +
                            "VALUES (@usr, @tok, GETDATE()) END";
                    command.Parameters.AddWithValue("@usr", username);
                    command.Parameters.AddWithValue("@tok", GuidHelper.NewGuidString());
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();


                    command.CommandText = "SELECT u.[Username], u.[Active], u.[Admin], ul.[Token] " +
                        "FROM [Users] u " +
                        "INNER JOIN [UserLogin] ul on ul.[Username] = u.[Username] " +
                        "WHERE UPPER(u.[Username])=UPPER(@usr)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@usr", username);
                    TraceSqlCommand(command);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user.UserName = SQLHelper.GetString(reader["Username"]);
                            user.Active = SQLHelper.GetBool(reader["Active"]);
                            user.Admin = SQLHelper.GetBool(reader["Admin"]);
                            user.Token = SQLHelper.GetString(reader["Token"]);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            user.LSKeys = GetKeys(user.UserName);
            return user;
        }

        public bool UserIsNotActive(string username)
        {
            bool ret = false;

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Active] FROM [Users] WHERE UPPER([Username])=@usr";
                    command.Parameters.AddWithValue("@usr", username.ToUpper());
                    TraceSqlCommand(command);
                    connection.Open();
                    ret = (bool)command.ExecuteScalar();
                }
                connection.Close();
            }
            return ret == false;
        }

        public void ResetPassword(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandText = "UPDATE [Users] SET [Password]=@pwd WHERE UPPER([Username])=@usr ";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@usr", username.ToUpper());
                    command.Parameters.AddWithValue("@pwd", password);
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void DeleteUser(string username)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM [Users] WHERE UPPER([Username])=@usr";
                    command.Parameters.AddWithValue("@usr", username.ToUpper());
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM [UserKeys] WHERE UPPER([Username])=@usr";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@usr", username.ToUpper());
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void ToggleUser(string username, bool toggle)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandText = "UPDATE [Users] SET [Active]=@toggle WHERE UPPER([Username])=@usr ";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@usr", username.ToUpper());
                    command.Parameters.AddWithValue("@toggle", toggle);
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public DateTime GetTokenDate(string token)
        {
            DateTime modified = new DateTime();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandText = "SELECT [DateModified] FROM [UserLogin] WHERE UPPER([Token])=UPPER(@tok)";
                    command.Parameters.AddWithValue("@tok", token);
                    TraceSqlCommand(command);
                    var dt = command.ExecuteScalar();
                    if (dt != null)
                        modified = (DateTime)dt;

                    command.CommandText = "IF EXISTS (SELECT [DateModified] FROM [UserLogin] WHERE UPPER([Token])=UPPER(@tok))" +
                        "BEGIN UPDATE [UserLogin] SET [DateModified]=GETDATE() WHERE UPPER([Token])=UPPER(@tok) END";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@tok", token);
                    TraceSqlCommand(command);
                   command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return modified;
        }

        public void Logout(string token)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandText = "IF EXISTS(SELECT [Token] FROM [UserLogin] WHERE UPPER([Token]) = UPPER(@tok)) " +
                            "BEGIN UPDATE [UserLogin] SET [DateModified]=GETDATE(), [Token]='' WHERE UPPER([Token])=UPPER(@tok) END ";
                    command.Parameters.AddWithValue("@tok", token);
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public string GetUsernameByToken(string token)
        {
            string user = "";
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandText = "SELECT [Username] FROM [UserLogin] WHERE UPPER([Token])=UPPER(@tok)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@tok", token);
                    TraceSqlCommand(command);
                    user = command.ExecuteScalar().ToString();
                }
                connection.Close();
            }
            return user;
        }
    }
}