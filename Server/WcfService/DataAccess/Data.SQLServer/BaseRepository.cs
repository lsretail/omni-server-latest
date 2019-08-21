using System;
using System.Text;
using System.Data.SqlClient;
using System.Data.Common;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.Dal
{
    public abstract class BaseRepository //: MembershipEntities
    {
        protected internal static DateTime MinDate = new DateTime(1970, 1, 1); //min date for json
        protected static string sqlConnectionString = null;
        protected static BOConfiguration config;

        private static readonly object myLock = new object();
        protected static LSLogger logger = new LSLogger();

        public BaseRepository(BOConfiguration configuration)
        {
            config = configuration;
            if (sqlConnectionString == null)
            {
                lock (myLock)
                {
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
                logger.Trace(config.LSKey.Key, "\r\n" + msg);
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
                    logger.Trace(config.LSKey.Key, "\r\n" + builder.ToString());
                }
                catch (Exception ex)
                {
                    logger.Error(config.LSKey.Key, "\r\n" + ex.Message);
                }
            }
        }

        protected string SerializeToXml(object value)
        {
            return Serialization.ToXml(value, true);
        }

        protected T DeserializeFromXml<T>(string xml)
        {
            return Serialization.DeserializeFromXml<T>(xml);
        }
    }
}
