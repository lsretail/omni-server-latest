﻿using System;
using System.Data;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.DataAccess.BOConnection.CentralExt.Dal
{

    public abstract class BaseRepository 
    {
        protected static string connectionString = null;
        protected static string navConnectionString = "";
        protected static string navOrgCompanyName = null;
        protected static string navCompanyName = null;
        protected static string databaseName = "";
        protected static string dbCollation = null;

        protected static Currency CurrencyLocalUnit = null;
        protected static CultureInfo CultInfo = CultureInfo.CurrentUICulture;
        protected static LSLogger logger = new LSLogger();
        protected static BOConfiguration config = null;
        protected static int useTMReplication = 0;

        private static readonly Object myLock = new Object();

        public static Version LSCVersion = new Version("26.0");

        public BaseRepository(BOConfiguration config, Version navVersion) : this(config)
        {
            if (navVersion != null)
                LSCVersion = navVersion;
        }

        public BaseRepository(BOConfiguration configuration)
        {
            config = configuration;

            if (connectionString == null)
            {
                lock (myLock)
                {
                    connectionString = config.SettingsGetByKey(ConfigKey.BOSql);
                    if (DecryptConfigValue.IsEncryptedPwd(connectionString))
                    {
                        connectionString = DecryptConfigValue.DecryptString(connectionString, config.SettingsGetByKey(ConfigKey.EncrCode));
                    }

                    navConnectionString = connectionString;

                    System.Data.Common.DbConnectionStringBuilder builder = new System.Data.Common.DbConnectionStringBuilder();
                    builder.ConnectionString = connectionString;

                    //string server = builder["Data Source"] as string;
                    if (builder.ContainsKey("Password"))
                    {
                        string tmpPwd = builder["Password"] as string;
                        tmpPwd = tmpPwd.Trim();
                        //
                        if (DecryptConfigValue.IsEncryptedPwd(tmpPwd))
                        {
                            //decrypt the pwd
                            builder["Password"] = DecryptConfigValue.DecryptString(tmpPwd, config.SettingsGetByKey(ConfigKey.EncrCode));
                        }
                    }

                    navCompanyName = "";
                    if (builder.ContainsKey("NAVCompanyName"))
                    {
                        navOrgCompanyName = builder["NAVCompanyName"] as string; //get the 
                        navOrgCompanyName = navOrgCompanyName.Trim();

                        navCompanyName = SQLHelper.GetSQLNAVName(navOrgCompanyName);

                        //NAV company name must end with a $
                        if (navCompanyName.EndsWith("$") == false)
                            navCompanyName += "$";

                        SQLHelper.CheckForSQLInjection(navCompanyName, 1);
                    }

                    if (builder.ContainsKey("Initial Catalog"))
                    {
                        databaseName = builder["Initial Catalog"] as string; //get the 
                        databaseName = databaseName.Trim();
                    }
                    
                    builder.Remove("NAVCompanyName");
                    connectionString = builder.ConnectionString;
                }
            }
        }

        internal static string FormatAmount(decimal amount, string culture, bool absValue = true)
        {
            if (absValue)
                amount = Math.Abs(amount); //no negative values returned

            if (CurrencyLocalUnit == null)
            {
                string curcode = string.Empty;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT [LCY Code] FROM [" + navCompanyName + "General Ledger Setup$437dbf0e-84ff-417a-965d-ed2bb9650972]";
                        connection.Open();
                        curcode = command.ExecuteScalar() as string;
                        connection.Close();
                    }
                }

                CurrencyRepository currRep = new CurrencyRepository(config, LSCVersion);
                CurrencyLocalUnit = currRep.CurrencyLoyGetById(curcode, culture);
                if (CurrencyLocalUnit == null)
                    CurrencyLocalUnit = new Currency();

                //using culture to get the correct  group and decimal char. , or .
                if (string.IsNullOrWhiteSpace(CurrencyLocalUnit.Culture) == false)
                {
                    CultInfo = new CultureInfo(CurrencyLocalUnit.Culture);  // de-DE en-US
                }
            }

            if (string.IsNullOrEmpty(CurrencyLocalUnit.Id))
            {
                return amount.ToString("N02");
            }
            else
            {
                // cannot use string.Format since we must follow what BackOffice tells us about decimal places
                // BO also determines if we want the symbol ($) as pre of post
                string amt = amount.ToString("N0" + CurrencyLocalUnit.DecimalPlaces.ToString(), CultInfo);      // N03 has 3 decimal pts

                // add the prefix and postfix to the string  e.g. $
                return string.Format("{0}{1} {2}", CurrencyLocalUnit.Prefix, amt, CurrencyLocalUnit.Postfix).Trim();
            }
        }

        protected string GetDbCICollation()
        {
            try
            {
                if (dbCollation == null)
                {
                    string sql = "SELECT DATABASEPROPERTYEX('" + databaseName + "', 'Collation') As SQLCollation;";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = sql;
                            TraceSqlCommand(command);
                            connection.Open();
                            dbCollation = "COLLATE " + command.ExecuteScalar().ToString().Replace("_CS_", "_CI_"); // make it case sensitive;
                            connection.Close();
                        }
                    }
                }
                return dbCollation;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void TraceIt(string msg)
        {
            //logger only in baseRepository
            if (logger.IsTraceEnabled)
                logger.Trace((config == null) ? "Unknown" : config.LSKey.Key, "\r\n" + msg);
        }

        protected void TraceSqlCommand(IDbCommand command)
        {
            if (logger.IsTraceEnabled == false)
                return;
            try
            {
                StringBuilder builder = new StringBuilder();
                if (command.CommandType == CommandType.StoredProcedure)
                    builder.AppendLine("Stored procedure: " + command.CommandText);
                else
                    builder.AppendLine("Sql command: " + command.CommandText);

                if (command.Parameters.Count > 0)
                    builder.AppendLine("With the following parameters.");

                string value;
                foreach (IDataParameter param in command.Parameters)
                {
                    if (param.Value == null)
                    {
                        value = "NULL";
                    }
                    else
                    {
                        switch (param.DbType)
                        {
                            case DbType.Binary:
                                value = BitConverter.ToString((byte[])param.Value);
                                break;
                            default:
                                value = param.Value.ToString();
                                break;

                        }
                    }
                    builder.AppendLine(string.Format(" > Parameter {0}: {1}", param.ParameterName, value));
                }
                logger.Trace((config == null) ? "Unknown" : config.LSKey.Key, "\r\n" + builder.ToString());
            }
            catch (Exception ex)
            {
                logger.Error((config == null) ? "Unknown" : config.LSKey.Key, "\r\n" + ex.Message);
            }
        }

        public int GetRecordCount(int tableid, string lastkey, string fullreplsql, List<JscKey> keys, ref string maxkey)
        {
            int cnt = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    if (string.IsNullOrWhiteSpace(fullreplsql))
                    {
                        command.CommandText = "SELECT COUNT(*)" + GetSQLAction(tableid, lastkey, false);
                        TraceSqlCommand(command);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cnt++;
                            }
                            reader.Close();
                        }
                    }
                    else
                    {
                        command.CommandText = fullreplsql;
                        JscActions act = new JscActions(lastkey);
                        SetWhereValues(command, act, keys, true, true);
                        cnt = (int)command.ExecuteScalar();
                    }
                }

                // Get highest PreAction counter for table as we will start normal replication from that point after FullReplication
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT MAX([Entry No_]) FROM [{0}LSC Preaction$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [Table No_]='{1}'",
                        navCompanyName, tableid);
                    TraceSqlCommand(command);
                    var ret = command.ExecuteScalar();
                    string mkey = (ret == DBNull.Value) ? "0" : ret.ToString();
                    if ((string.IsNullOrWhiteSpace(maxkey) == false) && maxkey != "0")
                    {
                        // check previous maxkey if less or grater then this mkey
                        if (Convert.ToInt32(mkey) > Convert.ToInt32(maxkey))
                            maxkey = mkey;
                    }
                    else
                    {
                        maxkey = mkey;
                    }
                    connection.Close();
                }
            }
            return cnt;
        }

        public int GetRecordCountTM(string lastKey, string replSQL, List<JscKey> keys)
        {
            int cnt = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = replSQL;
                    JscActions act = new JscActions(lastKey);
                    SetWhereValues(command, act, keys, true, true);
                    cnt = (int)command.ExecuteScalar();
                }
            }
            return cnt;
        }

        public string GetSQL(bool fullreplication, int batchsize, bool gettimestamp = true, bool distinct = false)
        {
            string sql;
            if (fullreplication && batchsize > 0)
            {
                sql = string.Format("SELECT {0}TOP({1}) ", (distinct) ? "DISTINCT " : string.Empty, batchsize);
            }
            else
            {
                sql = "SELECT ";
                if (distinct)
                    sql += "DISTINCT ";
            }

            if (gettimestamp)
                sql += "mt.[timestamp],";

            return sql;
        }

        private string GetSQLAction(int tableid, string lastkey, bool orderby)
        {
            string sql = string.Format(" FROM [{0}{1}] p1 WHERE p1.[Table No_]={2} AND p1.[Entry No_]>{3} {4}",
                    navCompanyName,
                    UseTimeStampReplication() ? "LSC Commerce Action$5ecfc871-5d82-43f1-9c54-59685e82318d" : "LSC Preaction$5ecfc871-5d82-43f1-9c54-59685e82318d", 
                    tableid, lastkey,
                    "GROUP BY p1.[Table No_],p1.[Key]");

            if (orderby)
                sql += " ORDER BY [EntryNo]";

            return sql;
        }

        public List<JscActions> LoadActions(bool fullreplication, int tableid, int batchsize, ref string lastkey, ref int recremaining)
        {
            List<JscActions> actions = new List<JscActions>();
            if (fullreplication)
                return actions;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetSQL(true, batchsize, false) +
                            "p1.[Table No_],p1.[Key],MAX(p1.[Entry No_]) AS [EntryNo],(SELECT TOP 1 p2.[Action] " +
                            "FROM [" + navCompanyName + 
                            (UseTimeStampReplication() ? "LSC Commerce Action$5ecfc871-5d82-43f1-9c54-59685e82318d] p2 " : "LSC Preaction$5ecfc871-5d82-43f1-9c54-59685e82318d] p2 ") +
                            "WHERE p2.[Table No_]=p1.[Table No_] AND p2.[Key]=p1.[Key] " +
                            "ORDER BY p2.[Entry No_] DESC) AS [Action]" +
                            GetSQLAction(tableid, lastkey, true);

                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            JscActions act = new JscActions()
                            {
                                id = SQLHelper.GetInt64(reader[2]),
                                Type = (DDStatementType)SQLHelper.GetInt32(reader[3]),
                                TableId = SQLHelper.GetInt32(reader[0]),
                                ParamValue = SQLHelper.GetString(reader[1])
                            };

                            if (String.IsNullOrEmpty(act.ParamValue))
                                continue;

                            lastkey = act.id.ToString();
                            recremaining--;
                            actions.Add(act);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return actions;
        }

        public List<JscActions> LoadDeleteActions(bool fullReplication, int tableId, string tableName, List<JscKey> keys, int batchSize, ref string delKey)
        {
            List<JscActions> actions = new List<JscActions>();
            if (fullReplication)
                return actions;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetSQL(true, batchSize, false) +
                                        "[Table No_],[Key],[Entry No_] " +
                                        "FROM [" + navCompanyName + "LSC Delete Preaction$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                                        "WHERE [Table No_]=@tid AND [Entry No_]>@eid ORDER BY [Entry No_]";
                    command.Parameters.AddWithValue("@tid", tableId);
                    command.Parameters.AddWithValue("@eid", delKey);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        using (SqlCommand command2 = connection.CreateCommand())
                        {
                            command2.CommandText = string.Format("SELECT COUNT(1) FROM [{0}{1}] mt{2}",
                                navCompanyName, tableName, GetWhereStatement(false, keys, false));

                            while (reader.Read())
                            {
                                JscActions act = new JscActions()
                                {
                                    id = SQLHelper.GetInt64(reader[2]),
                                    Type = DDStatementType.Delete,
                                    TableId = SQLHelper.GetInt32(reader[0]),
                                    ParamValue = SQLHelper.GetString(reader[1])
                                };

                                if (String.IsNullOrEmpty(act.ParamValue))
                                    continue;

                                SetWhereValues(command2, act, keys, true);
                                int rows = command2.ExecuteNonQuery();
                                if (rows > 0)
                                    continue;

                                delKey = act.id.ToString();
                                actions.Add(act);
                            }
                            reader.Close();
                        }
                    }
                }
                connection.Close();
            }
            return actions;
        }

        public List<JscKey> GetPrimaryKeys(string table)
        {
            List<JscKey> keys = new List<JscKey>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT C.COLUMN_NAME, C.DATA_TYPE" +
                                          " FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE K" +
                                          " LEFT JOIN INFORMATION_SCHEMA.COLUMNS C ON K.COLUMN_NAME = C.COLUMN_NAME AND K.TABLE_NAME = C.TABLE_NAME" +
                                          " WHERE OBJECTPROPERTY(OBJECT_ID(K.CONSTRAINT_SCHEMA + '.' + K.CONSTRAINT_NAME), 'IsPrimaryKey') = 1" +
                                          " AND K.TABLE_NAME = '" + navCompanyName + table + "' AND K.TABLE_SCHEMA = 'dbo'" +
                                          " ORDER BY K.ORDINAL_POSITION";

                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            keys.Add(new JscKey()
                            {
                                FieldName = (string)reader[0],
                                FieldType = (string)reader[1]
                            });
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return keys;
        }

        public void ProcessLastKey(string lastKey, out string mainKey, out string delKey, out string actKey)
        {
            ProcessLastKey(lastKey, out mainKey, out delKey);
            actKey = GetKeyValue(lastKey, 'A');
        }

        public void ProcessLastKey(string lastKey, out string mainKey, out string delKey)
        {
            if (string.IsNullOrEmpty(lastKey))
                lastKey = "0";

            if (lastKey.StartsWith("R="))
            {
                mainKey = GetKeyValue(lastKey, 'R');
                delKey = GetKeyValue(lastKey, 'D');
            }
            else
            {
                mainKey = (string.IsNullOrEmpty(lastKey)) ? "0" : lastKey;
                delKey = "0";
            }
        }

        private string GetKeyValue(string lastKey, char keyId)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                return("0");

            int start = lastKey.IndexOf($"{keyId}=");
            if (start == -1)
                return("0");

            start += 2;
            // Key for timestamp replication
            int end = lastKey.IndexOf(";", start);
            if (end == -1)
                return("0");

            string key = lastKey.Substring(start, end - start);
            return key;
        }

        public string GetWhereStatementWithStoreDist(bool fullreplication, List<JscKey> keys, string whereaddon, string itemcolumnname, string storeid, bool includeorder)
        {
            return GetWhereStatement(fullreplication, keys, whereaddon + GetSQLStoreDist(itemcolumnname, storeid, fullreplication), includeorder);
        }

        public string GetWhereStatementWithStoreDist(bool fullreplication, List<JscKey> keys, string itemcolumnname, string storeid, bool includeorder, bool usestatus = true)
        {
            return GetWhereStatement(fullreplication, keys, GetSQLStoreDist(itemcolumnname, storeid, fullreplication, usestatus), includeorder);
        }

        public string GetSQLStoreDist(string itemcolumnname, string storeid, bool fullreplication, bool usestatus = true)
        {
            if (string.IsNullOrWhiteSpace(storeid))
                return string.Empty;

            SQLHelper.CheckForSQLInjection(storeid);

            // for full replication get only active, otherwise get all
            if (fullreplication)
            {
                return " AND " + itemcolumnname + " IN (SELECT id.[Item No_] FROM [" + navCompanyName + "LSC Store Group Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] sg" +
                   " LEFT JOIN [" + navCompanyName + "LSC Item Distribution$5ecfc871-5d82-43f1-9c54-59685e82318d] id ON id.[Code]=sg.[Store Group] OR id.[Code]='" + storeid + "'" +
                   " WHERE " + ((usestatus) ? "(id.[Status]=0 OR id.[Status]=2) AND " : string.Empty) + "sg.[Store Code]='" + storeid + "')";
            }

            return " AND " + itemcolumnname + " IN (SELECT id.[Item No_] FROM [" + navCompanyName + "LSC Store Group Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] sg" +
                   " LEFT JOIN [" + navCompanyName + "LSC Item Distribution$5ecfc871-5d82-43f1-9c54-59685e82318d] id ON id.[Code]=sg.[Store Group] OR id.[Code]='" + storeid + "'" +
                   " WHERE sg.[Store Code]='" + storeid + "')";
        }

        public string GetWhereStatement(bool fullreplication, List<JscKey> keys, bool includeorder)
        {
            return GetWhereStatement(fullreplication, keys, string.Empty, includeorder);
        }

        public string GetWhereStatement(bool fullreplication, List<JscKey> keys, string whereaddon, bool includeorder)
        {
            string where = string.Empty;
            string order = string.Empty;
            bool first = true;
            int index = 0;

            if (fullreplication)
            {
                where = " WHERE mt.[timestamp]>@0";
                order = " ORDER BY mt.[timestamp]";
            }
            else
            {
                foreach (JscKey key in keys)
                {
                    if (first)
                    {
                        where += string.Format(" WHERE mt.[{0}]=@{1}", key.FieldName, index++);
                        order += string.Format(" ORDER BY mt.[{0}]", key.FieldName);
                        first = false;
                        continue;
                    }
                    where += string.Format(" AND mt.[{0}]=@{1}", key.FieldName, index++);
                    order += string.Format(",mt.[{0}]", key.FieldName);
                }
            }

            if (string.IsNullOrWhiteSpace(where) == false)
                where += whereaddon;

            if (includeorder)
                where += order;
            return where;
        }

        public object GetSQLDataFromKey(string fieldtype, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (fieldtype == "datetime")
                    return ConvertTo.GetDateTimeFromNav(value);
                return string.Empty;
            }

            switch (fieldtype)
            {
                case "datetime":
                    return ConvertTo.GetDateTimeFromNav(value);

                case "int":
                    return Convert.ToInt32(value);
                case "smallint":
                case "tinyint":
                    return Convert.ToInt16(value);
                case "bigint":
                    return Convert.ToInt64(value);

                case "float":
                    return Convert.ToDouble(value);
                case "smallmoney":
                case "money":
                case "decimal":
                    return Convert.ToDecimal(value);
                case "real":
                    return Convert.ToSingle(value);

                default:
                    return value;
            }
        }

        public bool SetWhereValues(SqlCommand sqlcmd, JscActions actions, List<JscKey> keys, bool first, bool fullrepl = false)
        {
            if (string.IsNullOrWhiteSpace(actions.ParamValue))
                return false;

            if (first)
                sqlcmd.Parameters.Clear();

            if (fullrepl)
            {
                SqlParameter par = new SqlParameter("@0", SqlDbType.Timestamp);
                if (actions.ParamValue == "0")
                    par.Value = new byte[] { 0 };
                else
                    par.Value = StringToByteArray(actions.ParamValue);

                sqlcmd.Parameters.Add(par);
                return true;
            }

            string[] data;
            if (actions.ParamValue == "0")
            {
                // add number of values for the primary key to start from
                actions.ParamValue = "";
                for (int i = 1; i < keys.Count; i++)
                {
                    actions.ParamValue += ";";
                }
            }

            data = actions.ParamValue.Split(';');
            if (data.Length != keys.Count)
                return false;     // mismatch in action key values and the actual table key

            for (int i = 0; i < data.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(data[i]))
                    data[i] = "";

                string fieldname = "@" + i.ToString();
                if (first)
                {
                    SqlParameter par = new SqlParameter(fieldname, (SqlDbType)EnumHelper.StringToEnum(typeof(SqlDbType), keys[i].FieldType));
                    par.Value = GetSQLDataFromKey(keys[i].FieldType, data[i]);
                    sqlcmd.Parameters.Add(par);
                }
                else
                {
                    sqlcmd.Parameters[i].Value = GetSQLDataFromKey(keys[i].FieldType, data[i]);
                }
            }
            return true;
        }

        public bool UseTimeStampReplication()
        {
            if (LSCVersion < new Version("25.0"))
                return false;

            // 0=not set, 1=use, 2=not use
            if (useTMReplication > 0)
                return (useTMReplication == 1);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1 FROM sys.columns WHERE [Name]='Use Preaction Timestamp' AND Object_ID=Object_ID('[" + navCompanyName + "LSC Distribution Location$5ecfc871-5d82-43f1-9c54-59685e82318d]')";
                    bool hascol = SQLHelper.GetBool(command.ExecuteScalar());
                    if (hascol == false)
                        return false;
                        
                    command.CommandText = "SELECT dl.[Use Preaction Timestamp] " +
                        "FROM [" + navCompanyName + "LSC Retail Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] rs " +
                        "JOIN [" + navCompanyName + "LSC Distribution Location$5ecfc871-5d82-43f1-9c54-59685e82318d] dl " +
                        "ON dl.[Code]=rs.[Distribution Location]";
                    try
                    {
                        bool useTM = ConvertTo.SafeBoolean(command.ExecuteScalar().ToString());
                        useTMReplication = (useTM) ? 1 : 2;
                    }
                    catch
                    {
                        logger.Error(config.LSKey.Key, "[Use Preaction Timestamp] not found in Dist.Location table");
                    }
                }
                connection.Close();
            }
            return useTMReplication == 1;
        }

        public static byte[] StringToByteArray(String hex)
        {
            try
            {
                int NumberChars = hex.Length;
                byte[] bytes = new byte[NumberChars / 2];
                for (int i = 0; i < NumberChars; i += 2)
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                return bytes;
            }
            catch
            {
                throw new LSOmniServiceException(StatusCode.Error, "Invalid LastKey for Full Replication");
            }
        }
    }
}
