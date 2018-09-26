using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using NLog;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;

namespace LSOmni.DataAccess.Dal
{
    public class AppSettingsRepository : BaseRepository, IAppSettingsRepository
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static object lockDictionary = new Object();

        public AppSettingsRepository()
        {
        }

        public void Ping()
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM AppSettings";
                    command.CommandTimeout = 3;
                    connection.Open();
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public int AppSettingsIntGetByKey(AppSettingsKey key, string languageCode = "en")
        {
            string value = AppSettingsGetByKey(key, languageCode);
            return Convert.ToInt32(value);
        }

        public bool AppSettingsBoolGetByKey(AppSettingsKey key, string languageCode = "en")
        {
            string value = AppSettingsGetByKey(key, languageCode);
            return Convert.ToBoolean(value);
        }

        public decimal AppSettingsDecimalGetByKey(AppSettingsKey key, string languageCode = "en")
        {
            string value = AppSettingsGetByKey(key, languageCode);
            return Convert.ToDecimal(value);
        }

        public bool AppSettingsKeyExists(AppSettingsKey key, string languageCode = "en")
        {
            try
            {
                AppSettingsGetByKey(key, languageCode);
                return true;
            }
            catch (LSOmniServiceException ex)
            {
                if (ex.StatusCode == StatusCode.AppSettingsNotFound)
                    return false;
                else
                    return true;
            }
        }

        public string AppSettingsGetByKey(AppSettingsKey key, string languageCode = "en")
        {
            //defaults to english if key not found for other languages
            languageCode = languageCode.ToLower().Trim();

            //check if it comes in as en-US or fr-FR  strip the region part
            //Using  TwoLetterISOLanguageName   en fr de is etc
            if (languageCode.Contains("-"))
            {
                string[] tmp = languageCode.Split('-');
                if (tmp.Length > 1)
                {
                    languageCode = tmp[0]; //get the en
                }
            }

            //do cache it all
            string ret = AppSettingGetKey(key.ToString(), languageCode);
            if (ret != null)
                return ret;

            //en-US not just en 
            if (languageCode.StartsWith("en") == false)
            {
                //not found for this language, default to english
                languageCode = "en";
                ret = AppSettingGetKey(key.ToString(), languageCode);
                if (ret != null)
                    return ret;
            }
            throw new LSOmniServiceException(StatusCode.AppSettingsNotFound,
                string.Format("Key {0} - languageCode: {1} not found.", key, languageCode));
        }

        public void AppSettingsSetByKey(AppSettingsKey key, string value, string valuetype, string comment, string languageCode = "en")
        {
            //defaults to english if key not found for other languages
            languageCode = languageCode.ToLower().Trim();

            //check if it comes in as en-US or fr-FR  strip the region part
            //Using  TwoLetterISOLanguageName   en fr de is etc
            if (languageCode.Contains("-"))
            {
                string[] tmp = languageCode.Split('-');
                if (tmp.Length > 1)
                {
                    languageCode = tmp[0]; //get the en
                }
            }

            if (AppSettingGetKey(key.ToString(), languageCode) != null)
            {
                // update key value
                AppSettingUpdateKey(key.ToString(), languageCode, value);
                return;
            }

            //en-US not just en 
            if (languageCode.StartsWith("en") == false)
            {
                //not found for this language, default to english
                languageCode = "en";
                if (AppSettingGetKey(key.ToString(), languageCode) != null)
                {
                    AppSettingUpdateKey(key.ToString(), languageCode, value);
                    return;
                }
            }

            // create new entry
            AppSettingSaveKey(key.ToString(), languageCode, value, valuetype, comment);
        }

        private string AppSettingGetKey(string key, string code)
        {
            string ret = string.Empty;
            lock (lockDictionary)
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT [Value] FROM [AppSettings] WHERE [Key]=@key AND [LanguageCode]=@code";
                        command.Parameters.AddWithValue("@key", key);
                        command.Parameters.AddWithValue("@code", code);
                        TraceSqlCommand(command);
                        connection.Open();
                        ret = (string)command.ExecuteScalar();
                    }
                    connection.Close();
                }
                return ret;
            }
        }

        private void AppSettingSaveKey(string key, string code, string value, string datatype, string comment)
        {
            lock (lockDictionary)
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO [AppSettings] ([Key],[LanguageCode],[Value],[Comment],[DataType]" +
                                              ") VALUES (@f0,@f1,@f2,@f3,@f4)";

                        command.Parameters.AddWithValue("@f0", key);
                        command.Parameters.AddWithValue("@f1", code);
                        command.Parameters.AddWithValue("@f2", value);
                        command.Parameters.AddWithValue("@f3", comment);
                        command.Parameters.AddWithValue("@f4", datatype);
                        TraceSqlCommand(command);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
        }

        private void AppSettingUpdateKey(string key, string code, string value)
        {
            lock (lockDictionary)
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "UPDATE [AppSettings] SET [Value]=@value WHERE [LanguageCode]=@code AND [Key]=@key";
                        command.Parameters.AddWithValue("@key", key);
                        command.Parameters.AddWithValue("@code", code);
                        command.Parameters.AddWithValue("@value", value);
                        TraceSqlCommand(command);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
        }

        public void DbCleanUp(int daysLog, int daysQueue, int daysNotify, int daysUser, int daysOneList)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    //don't want to stop if one stored proc fails
                    try
                    {
                        if (daysQueue > 0)
                        {
                            command.CommandText = "DELETE FROM [OrderQueue] WHERE [DateCreated]<@f1";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@f1", DateTime.Now.AddDays(daysQueue * -1));
                            TraceSqlCommand(command);
                            int cnt = command.ExecuteNonQuery();
                            logger.Log(LogLevel.Info, "OrderQueue Cleanup, removed {0} records", cnt);
                        }

                        command.CommandText = "UPDATE [OrderQueue] SET [OrderStatus]=3,[StatusChange]=@f1 WHERE [DateCreated]<@f2 AND [OrderStatus]<>3";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@f1", string.Format("StatusChange to 3"));
                        command.Parameters.AddWithValue("@f2", DateTime.Now.AddMinutes(-(60 * 24 * 2)));
                        TraceSqlCommand(command);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, ex, "OrderQueue Cleanup failed.");
                    }

                    if (daysLog > 0)
                    {
                        try
                        {
                            command.CommandText = "DELETE FROM [LoginLog] WHERE [CreateDate]<@f1";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@f1", DateTime.Now.AddDays(daysLog * -1));
                            TraceSqlCommand(command);
                            int cnt = command.ExecuteNonQuery();
                            logger.Log(LogLevel.Info, "LoginLog Cleanup, removed {0} records", cnt);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(LogLevel.Error, ex, "LoginLog Cleanup failed");
                        }

                        try
                        {
                            command.CommandText = "DELETE FROM [TaskLog] WHERE [ModifyTime]<@f1";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@f1", DateTime.Now.AddDays(daysLog * -1));
                            TraceSqlCommand(command);
                            int cnt = command.ExecuteNonQuery();
                            logger.Log(LogLevel.Info, "TaskLog Cleanup, removed {0} records", cnt);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(LogLevel.Error, ex, "TaskLog Cleanup failed");
                        }

                        try
                        {
                            command.CommandText = "DELETE FROM [TaskLogLine] WHERE [ModifyTime]<@f1";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@f1", DateTime.Now.AddDays(daysLog * -1));
                            TraceSqlCommand(command);
                            int cnt = command.ExecuteNonQuery();
                            logger.Log(LogLevel.Info, "TaskLogLine Cleanup, removed {0} records", cnt);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(LogLevel.Error, ex, "TaskLog Cleanup failed");
                        }
                    }

                    if (daysNotify > 0)
                    {
                        try
                        {
                            command.CommandText = "DELETE FROM [PushNotification] WHERE [DateSent]<@f1";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@f1", DateTime.Now.AddDays(daysNotify * -1));
                            TraceSqlCommand(command);
                            int cnt = command.ExecuteNonQuery();
                            logger.Log(LogLevel.Info, "Notification Cleanup, removed {0} records", cnt);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(LogLevel.Error, ex, "Notification Cleanup failed");
                        }
                    }

                    if (daysUser > 0)
                    {
                        try
                        {
                            List<string> users = new List<string>();
                            command.CommandText = "SELECT [UserId] FROM [User] WHERE [LastAccessed]<@f1";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@f1", DateTime.Now.AddDays(daysUser * -1));
                            TraceSqlCommand(command);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    users.Add(SQLHelper.GetString(reader["UserId"]));
                                }
                                reader.Close();
                            }

                            ContactRepository rep = new ContactRepository();
                            foreach (string user in users)
                            {
                                rep.ContactDelete(user);
                            }
                            logger.Log(LogLevel.Info, "UserData CleanUp, removed {0} records", users.Count);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(LogLevel.Error, ex, "UserData Cleanup failed");
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
                                if (reader.Read())
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

                            OneListRepository rep = new OneListRepository();
                            foreach (OneList list in lists)
                            {
                                rep.OneListDeleteById(list.Id, list.ListType);
                            }
                            logger.Log(LogLevel.Info, "OneList CleanUp, removed {0} records", lists.Count);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(LogLevel.Error, ex, "OneList Cleanup failed");
                        }
                    }
                }
                connection.Close();
            }
        }
    }
}
        
 
         