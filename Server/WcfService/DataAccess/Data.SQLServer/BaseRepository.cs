using System;
using System.Text;
using System.Data.SqlClient;
using System.Data.Common;

using NLog;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.DataAccess.Dal
{
    public abstract class BaseRepository //: MembershipEntities
    {
        public int CacheImageDurationInMinutes { get { return CacheSettings.Instance.CacheImageDurationInMinutes; } }
        public int CacheMenuDurationInMinutes { get { return CacheSettings.Instance.CacheMenuDurationInMinutes; } }
        public bool CacheImage { get { return CacheSettings.Instance.CacheImage; } }
        public bool CacheMenu { get { return CacheSettings.Instance.CacheMenu; } }

        protected internal static DateTime MinDate = new DateTime(1970, 1, 1); //min date for json
        protected static string sqlConnectionString = null;

        private static readonly Object myLock = new Object();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        public BaseRepository()
        {
            if (sqlConnectionString == null)
            {
                lock (myLock)
                {
                    //first chech the old one
                    if (ConfigSetting.KeyExists("LSOmniSQLConnectionString"))
                        sqlConnectionString = ConfigSetting.GetString("LSOmniSQLConnectionString");
                    else
                        sqlConnectionString = ConfigSetting.GetString("SQLConnectionString.LSOmni");

                    DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
                    builder.ConnectionString = sqlConnectionString;

                    //string server = builder["Data Source"] as string;
                    if (builder.ContainsKey("Password"))
                    {
                        string tmpPwd = builder["Password"] as string;
                        tmpPwd = tmpPwd.Trim();
                        //
                        if (DecryptConfigValue.IsEncryptedPwd(tmpPwd))
                        {
                            //decrypte the pwd
                            builder["Password"] = DecryptConfigValue.DecryptString(tmpPwd);
                        }
                    }
                    sqlConnectionString = builder.ConnectionString;
                }
            }
        }

        internal delegate string GetValueDelegate();

        //simply check if a record exists
        // select count(*)  from xx where y=9. 
        //using params args to avoid sql injection
        protected bool DoesRecordExist(string tableName, string whereClause, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(whereClause))
                whereClause = "";

            whereClause = SQLHelper.CheckForSQLInjection(whereClause);
            tableName = SQLHelper.CheckForSQLInjection(tableName);

            int cnt = 0;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT COUNT(*) FROM {0}", tableName);
                    if (string.IsNullOrWhiteSpace(whereClause) == false)
                    {
                        command.CommandText += " WHERE " + whereClause;
                        foreach (object arg in args)
                        {
                            string prname = string.Format("@{0}", cnt++);
                            command.Parameters.AddWithValue(prname, arg);
                        }
                    }
                    TraceSqlCommand(command);
                    cnt = (int)command.ExecuteScalar();
                }
                connection.Close();
            }
            return (cnt > 0);
        }

        protected string NullToString(object objectIn, int maxLength = -1)
        {
            if (objectIn == null)
                return "";

            //need to pass in object since dynamic is used
            if (objectIn.GetType() != typeof(string))
                return "";

            string stringIn = objectIn.ToString();

            string stringOut = string.IsNullOrWhiteSpace(stringIn) ? "" : stringIn.Trim();
            //truncate the string length
            if (maxLength > 0)
            {
                if (string.IsNullOrWhiteSpace(stringOut) == false && stringOut.Length > maxLength)
                    stringOut = stringOut.Substring(0, maxLength);
            }
            return stringOut;
        }

        protected T CastTo<T>(object value) where T : struct
        {
            if (typeof(T) == typeof(DateTime) && ((DateTime)value).Year < 1970)
            {
                value = MinDate; //1970 is a safe json year
            }
            return value != DBNull.Value ? (T)value : default(T);
        }

        protected void TraceIt(string msg)
        {
            if (logger.IsTraceEnabled)
                logger.Trace("\r\n" + msg);
        }

        protected void TraceSqlCommand(System.Data.IDbCommand command)
        {
            if (logger.IsTraceEnabled)
            {
                try
                {
                    StringBuilder builder = new StringBuilder();
                    if (command.CommandType == System.Data.CommandType.StoredProcedure)
                        builder.AppendLine("Stored procedure: " + command.CommandText);
                    else
                        builder.AppendLine("Sql command: " + command.CommandText);

                    if (command.Parameters.Count > 0)
                        builder.AppendLine("With the following parameters.");

                    foreach (System.Data.IDataParameter param in command.Parameters)
                    {
                        builder.AppendFormat(
                            "     Paramater {0}: {1}",
                            param.ParameterName,
                            (param.Value == null ?
                            "NULL" : param.Value.ToString())).AppendLine();
                    }
                    logger.Trace("\r\n" + builder.ToString());
                }
                catch (Exception ex)
                {
                    logger.Error("\r\n" + ex.Message);
                }
            }
        }

        // CACHE part
        protected CacheState Validate(string sql, int durationInMinutes)
        {
            if (durationInMinutes <= 0)
                durationInMinutes = 0; // 

            DateTime currDate = DateTime.MinValue;
            DateTime lastDate = DateTime.MinValue;
            DateTime createdDate = DateTime.MinValue;

            CacheState state = CacheState.NotExist;//in case nothing got returned default this to notexist
            try
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        TraceSqlCommand(command);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currDate = DateTime.Now;
                                lastDate = SQLHelper.GetDateTime(reader["LastModifiedDate"]);
                                createdDate = SQLHelper.GetDateTime(reader["CreatedDate"]);
                            }
                            reader.Close();

                            if (currDate == DateTime.MinValue)
                                state = CacheState.NotExist; //id does not exist   9 < 20     false
                            else if ((currDate - lastDate).TotalMinutes < durationInMinutes)
                                state = CacheState.Exists; //id found and has not expired
                            else
                                state = CacheState.ExistsButExpired; //id found but has expired
                        }
                    }
                    connection.Close();
                }
                return state;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Validate failed..");
                throw;
            }
        }

        protected string SerializeToXml(object value)
        {
            return Serialization.SerializeToXml(value);
        }

        protected T DeserializeFromXml<T>(string xml)
        {
            return Serialization.DeserializeFromXml<T>(xml);
        }
    }
}
