using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;

namespace LSOmni.DataAccess.Dal
{
    public class ConfigRepository : BaseRepository, IConfigRepository
    {
        private static object lockDictionary = new Object();

        public ConfigRepository(BOConfiguration config) : base(config)
        {
        }

        public void PingOmniDB()
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM [TenantConfig]";
                    command.CommandTimeout = 3;
                    connection.Open();
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public bool ConfigExists(string lskey)
        {
            bool ret = false;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT [LSKey] FROM [LSKeys] WHERE [LSKey]=@key";
                    command.Parameters.AddWithValue("@key", lskey);
                    TraceSqlCommand(command);
                    connection.Open();
                    ret = command.ExecuteScalar() != null;
                }
                connection.Close();
            }
            return ret;
        }

        public void ToggleLSKey(string lskey, bool toggle)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "UPDATE [LSKeys] SET [Active]=@act WHERE [LSKey]=@key";
                    command.Parameters.AddWithValue("@act", toggle);
                    command.Parameters.AddWithValue("@key", lskey);
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public bool ConfigKeyExists(string lsKey, ConfigKey key)
        {
            if (string.IsNullOrEmpty(lsKey))
                return false;

            bool ret = false;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [" + key + "] FROM [TenantConfig] WHERE [LSKey]=@id";
                    command.Parameters.AddWithValue("@id", lsKey);
                    TraceSqlCommand(command);
                    connection.Open();
                    ret = command.ExecuteScalar() != null;
                }
                connection.Close();
            }
            return ret;
        }

        public BOConfiguration ConfigGet(string lsKey)
        {
            SQLHelper.CheckForSQLInjection(lsKey);
            BOConfiguration config = null;
            List<TenantSetting> list = new List<TenantSetting>();

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT t1.[Key],t1.[DataType],t1.[Comment], t1.[Advanced],t1.[Value] AS DefaultValue," +
                                          "(SELECT t2.[Value] FROM [TenantConfig] t2 WHERE t2.[Key]=t1.[Key] AND t2.[LSKey]=@id) AS CustomValue " +
                                          "FROM [TenantConfig] t1 where t1.[LSKey]=''";

                    command.Parameters.AddWithValue("@id", lsKey);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string key = SQLHelper.GetString(reader["Key"]);
                            string defaultValue = SQLHelper.GetString(reader["DefaultValue"]);
                            string value = SQLHelper.GetString(reader["CustomValue"]);
                            string comment = SQLHelper.GetString(reader["Comment"]);
                            string dataType = SQLHelper.GetString(reader["DataType"]);
                            bool advanced = SQLHelper.GetBool(reader["Advanced"]);
                            bool isDefault = false;

                            if (string.IsNullOrEmpty(value))
                            {
                                value = defaultValue;
                                isDefault = true;
                            }

                            if (DecryptConfigValue.IsEncryptedPwd(value))
                                value = DecryptConfigValue.DecryptString(value);

                            list.Add(new TenantSetting(key, value, comment, dataType, advanced, isDefault));
                        }
                        reader.Close();
                    }
                }

                // add values that are not found in database
                foreach (ConfigKey key in Enum.GetValues(typeof(ConfigKey)))
                {
                    TenantSetting set = list.Find(x => x.Key == key.ToString());
                    if (set == null)
                    {
                        list.Add(new TenantSetting(key.ToString(), string.Empty, string.Empty, string.Empty, false, true));
                    }
                }

                config = new BOConfiguration(lsKey);
                config.Settings = list;
                config.LSKey.Description = GetDescription(lsKey);
                config.LSKey.Active = ConfigIsActive(lsKey);

                connection.Close();
            }
            config.LSKey.Description = GetDescription(lsKey);
            return config;
        }

        public List<BOConfiguration> ConfigGetAll()
        {
            List<LSKey> keys = new List<LSKey>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [LSKey] FROM [LSKeys]";
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            keys.Add(new LSKey(SQLHelper.GetString(reader["LSKey"])));
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return ConfigGetByKeys(keys);
        }

        public List<BOConfiguration> ConfigGetByKeys(List<LSKey> lsKeys)
        {
            List<BOConfiguration> list = new List<BOConfiguration>();
            foreach (LSKey key in lsKeys)
            {
                BOConfiguration config = ConfigGet(key.Key);
                list.Add(config);
            }
            return list;
        }

        public void SaveConfig(BOConfiguration config)
        {
            lock (lockDictionary)
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    using (SqlTransaction trans = connection.BeginTransaction("TenantConfig"))
                    {
                        try
                        {
                            Delete(config.LSKey.Key, connection, trans);
                            if (config.Settings == null)
                                config.Settings = new List<TenantSetting>();
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                foreach (TenantSetting settings in config.Settings)
                                {
                                    command.CommandText = "IF NOT EXISTS (SELECT [LSKey],[Key] FROM [TenantConfig] " +
                                        "WHERE [LSKey]=@lskey AND [Key]=@key) " +
                                        "AND (SELECT [Value] FROM [TenantConfig] WHERE [Key]=@key AND [LSKey]='') != @value " +
                                        "INSERT INTO [TenantConfig] ([LSKey],[Key],[Value],[DataType], [Advanced]) " +
                                        "VALUES (@lskey,@key,@value,(SELECT [DataType] FROM [TenantConfig] WHERE [Key]=@key AND [LSKey]=''), " +
                                        "(SELECT [Advanced] FROM [TenantConfig] WHERE [Key]=@key AND [LSKey]='')) " +
                                        "ELSE " +
                                        "UPDATE [TenantConfig] SET [Value]=@value WHERE [LSKey]=@lskey AND [Key]=@Key ";

                                    command.Parameters.Clear();
                                    command.Parameters.AddWithValue("@lskey", config.LSKey.Key);
                                    command.Parameters.AddWithValue("@key", settings.Key);
                                    if(settings.Key == ConfigKey.BOPassword.ToString() || settings.Key == ConfigKey.BOSql.ToString())
                                        command.Parameters.AddWithValue("@value", DecryptConfigValue.EncryptString(settings.Value));
                                    else
                                        command.Parameters.AddWithValue("@value", settings.Value);
                                    TraceSqlCommand(command);
                                    command.ExecuteNonQuery();
                                }
                                command.CommandText = "IF NOT EXISTS (SELECT [LSKey] FROM [LSKeys] " +
                                        "WHERE [LSKey]=@lskey) " +
                                        "INSERT INTO [LSKeys] ([LSKey],[Description],[Active]) VALUES (@lskey,@desc,@active)" +
                                        "ELSE " +
                                        "UPDATE [LSKeys] SET [Description]=@desc WHERE [LSKey]=@lskey";

                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@lskey", config.LSKey.Key);
                                command.Parameters.AddWithValue("@desc", config.LSKey.Description);
                                command.Parameters.AddWithValue("@active", config.LSKey.Active);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();
                                trans.Commit();
                            }
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

        public void ConfigSetByKey(string lsKey, ConfigKey key, string value, string valueType, bool advanced, string comment)
        {
            lock (lockDictionary)
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "IF NOT EXISTS (SELECT [LSKey],[Key] FROM [TenantConfig] WHERE [LSKey]=@lskey AND [Key]=@key) " +
                            "INSERT INTO [TenantConfig] ([LSKey],[Key],[Value],[DataType],[Comment],[Advanced]) " +
                            "VALUES (@lskey,@key,@value,@type,@comm,@adv) " +
                            "ELSE " +
                            "UPDATE [TenantConfig] SET [Value]=@value WHERE [LSKey]=@lskey AND [Key]=@key";
                        command.Parameters.AddWithValue("@lskey", lsKey);
                        command.Parameters.AddWithValue("@key", key.ToString());
                        command.Parameters.AddWithValue("@value", value);
                        command.Parameters.AddWithValue("@type", valueType);
                        command.Parameters.AddWithValue("@comm", comment);
                        command.Parameters.AddWithValue("@adv", advanced);
                        TraceSqlCommand(command);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
        }

        public bool ConfigIsActive(string lskey)
        {
            if (string.IsNullOrEmpty(lskey))
                return true;

            bool ret = true;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Active] FROM [LSKeys] WHERE [LSKey]=@key";
                    command.Parameters.AddWithValue("@key", lskey);
                    TraceSqlCommand(command);
                    connection.Open();
                    ret = (bool)command.ExecuteScalar();
                }
                connection.Close();
            }
            return ret;
        }

        private string GetDescription(string lskey)
        {
            if (string.IsNullOrEmpty(lskey))
                return "DEFAULT";

            string desc = string.Empty;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Description] FROM [LSKeys] WHERE [LSKey]=@lskey ";
                    command.Parameters.AddWithValue("@lskey", lskey);
                    TraceSqlCommand(command);
                    connection.Open();
                    desc = command.ExecuteScalar().ToString();
                }
                connection.Close();
            }
            return desc;
        }

        public void ResetDefaults(string lskey)
        {
            //Don't want to delete the DEFAULT config
            if (string.IsNullOrEmpty(lskey))
                return;

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM [TenantConfig] WHERE [LSKey]=@lskey ";
                    command.Parameters.AddWithValue("@lskey", lskey);
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void Delete(string lskey)
        {
            //Don't want to delete the DEFAULT config
            if (string.IsNullOrEmpty(lskey))
                return;

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM [TenantConfig] WHERE [LSKey]=@lskey ";
                    command.Parameters.AddWithValue("@lskey", lskey);
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM [LSKeys] WHERE [LSKey]=@lskey ";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@lskey", lskey);
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM [UserKeys] WHERE [LSKey]=@lskey ";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@lskey", lskey);
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void Delete(string lskey, SqlConnection db, SqlTransaction trans)
        {
            //Don't want to delete the DEFAULT config
            if (string.IsNullOrEmpty(lskey))
                return;

            using (SqlCommand command = db.CreateCommand())
            {
                command.Transaction = trans;
                command.CommandText = "DELETE FROM [TenantConfig] WHERE [LSKey]=@lskey ";
                command.Parameters.AddWithValue("@lskey", lskey);
                TraceSqlCommand(command);
                command.ExecuteNonQuery();
            }
        }

        public void DbCleanUp(int daysLog, int daysNotify, int daysOneList)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    if (daysLog > 0)
                    {
                        try
                        {
                            command.CommandText = "DELETE FROM [TaskLog] WHERE [ModifyTime]<@f1";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@f1", DateTime.Now.AddDays(daysLog * -1));
                            TraceSqlCommand(command);
                            int cnt = command.ExecuteNonQuery();
                            logger.Info(config.LSKey.Key, "TaskLog Cleanup, removed {0} records", cnt);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(config.LSKey.Key, ex, "TaskLog Cleanup failed");
                        }

                        try
                        {
                            command.CommandText = "DELETE FROM [TaskLogLine] WHERE [ModifyTime]<@f1";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@f1", DateTime.Now.AddDays(daysLog * -1));
                            TraceSqlCommand(command);
                            int cnt = command.ExecuteNonQuery();
                            logger.Info(config.LSKey.Key, "TaskLogLine Cleanup, removed {0} records", cnt);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(config.LSKey.Key, ex, "TaskLog Cleanup failed");
                        }
                    }

                    if (daysNotify > 0)
                    {
                        try
                        {
                            command.CommandText = "DELETE FROM [Notification] WHERE [LastModifiedDate]<@f1";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@f1", DateTime.Now.AddDays(daysNotify * -1));
                            TraceSqlCommand(command);
                            int cnt = command.ExecuteNonQuery();
                            logger.Info(config.LSKey.Key, "Notification Cleanup, removed {0} records", cnt);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(config.LSKey.Key, ex, "Notification Cleanup failed");
                        }
                    }

                    if (daysOneList > 0)
                    {
                        try
                        {
                            List<OneList> lists = new List<OneList>();
                            command.CommandText = "SELECT [Id],[ListType] FROM [OneList] WHERE [LastAccessed]<@f1";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@f1", DateTime.Now.AddDays(daysOneList * -1));
                            TraceSqlCommand(command);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    OneList list = new OneList()
                                    {
                                        Id = SQLHelper.GetString(reader["Id"]),
                                        ListType = (ListType)SQLHelper.GetInt32(reader["ListType"])
                                    };
                                    lists.Add(list);
                                }
                                reader.Close();
                            }

                            OneListRepository rep = new OneListRepository(config);
                            foreach (OneList list in lists)
                            {
                                rep.OneListDeleteById(list.Id);
                            }
                            logger.Info(config.LSKey.Key, "OneList CleanUp, removed {0} records", lists.Count);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(config.LSKey.Key, ex, "OneList Cleanup failed");
                        }
                    }
                }
                connection.Close();
            }
        }
    }
}
