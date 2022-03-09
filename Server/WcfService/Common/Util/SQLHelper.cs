using System;
using System.Data.SqlClient;

namespace LSOmni.Common.Util
{
    public static class SQLHelper
    {
        /// <summary>
        /// Get Database String Value
        /// </summary>
        /// <param name="value">Database object</param>
        /// <returns>String Value</returns>
        static public string GetString(object value)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;
            return value.ToString();
        }

        /// <summary>
        /// Get Database String Value
        /// </summary>
        /// <param name="value">Database object</param>
        /// <returns>String Value</returns>
        static public string GetStringByte(object value, string encoding)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            if (encoding.Equals("utf-8", StringComparison.InvariantCultureIgnoreCase) || encoding.Equals("utf8", StringComparison.InvariantCultureIgnoreCase))
            {
                byte[] data = value as byte[];
                if (data.Length == 1 && data[0] == 0)
                    return string.Empty;

                if (data.Length > 1 && data[data.Length - 1] == 0)
                {
                    byte[] tmp = new byte[data.Length - 1];
                    Buffer.BlockCopy(data, 0, tmp, 0, data.Length - 1);
                    data = tmp;
                }
                return System.Text.Encoding.UTF8.GetString(data);
            }

            if (encoding.Equals("unicode", StringComparison.InvariantCultureIgnoreCase))
                return System.Text.Encoding.Unicode.GetString(value as byte[]);
            if (encoding.Equals("ascii", StringComparison.InvariantCultureIgnoreCase))
                return System.Text.Encoding.ASCII.GetString(value as byte[]);
            if (encoding.Equals("utf7", StringComparison.InvariantCultureIgnoreCase))
                return System.Text.Encoding.UTF7.GetString(value as byte[]);
            if (encoding.Equals("utf32", StringComparison.InvariantCultureIgnoreCase))
                return System.Text.Encoding.UTF32.GetString(value as byte[]);

            return System.Text.Encoding.Default.GetString(value as byte[]);
        }

        /// <summary>
        /// Get Database Integer Value
        /// </summary>
        /// <param name="value">Database object</param>
        /// <returns>Integer Value</returns>
        static public Int32 GetInt32(object value, bool invertValue = false)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            try
            {
                Int32 retvalue = Convert.ToInt32(value);
                if (invertValue)
                    return (retvalue * -1);
                return retvalue;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get Database Big Integer Value
        /// </summary>
        /// <param name="value">Database object</param>
        /// <returns>Big Integer Value</returns>
        public static Int64 GetInt64(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            try
            {
                return Convert.ToInt64(value);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get Database Decimal Value
        /// </summary>
        /// <param name="value">Database object</param>
        /// <returns>decimal Value</returns>
        public static decimal GetDecimal(object value, bool invertValue = false)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            try
            {
                decimal retvalue = Convert.ToDecimal(value);
                if (invertValue)
                    return (retvalue * -1);
                return retvalue;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get Database Decimal Value
        /// </summary>
        /// <param name="value">Database object</param>
        /// <returns>decimal Value</returns>
        public static decimal GetDecimal(SqlDataReader reader, string fieldname, bool invertValue = false)
        {
            try
            {
                if (reader[fieldname] == null || reader[fieldname] == DBNull.Value)
                    return 0;

                decimal retvalue = Convert.ToDecimal(reader[fieldname]);
                if (invertValue)
                    return (retvalue * -1);
                return retvalue;
            }
            catch (OverflowException ex)
            {
                LSLogger logger = new LSLogger();
                logger.Warn(ex, "Field: " + fieldname);
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get Database Double Value
        /// </summary>
        /// <param name="value">Database object</param>
        /// <returns>double Value</returns>
        public static double GetDouble(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            try
            {
                return Convert.ToDouble(value);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get Database Byte Value
        /// </summary>
        /// <param name="value">Database object</param>
        /// <returns>Byte Value</returns>
        public static byte GetByte(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            try
            {
                return Convert.ToByte(value);
            }
            catch
            {
                return 0;
            }
        }

        public static byte[] GetByteArray(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            try
            {
                return value as byte[];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get Database Boolean Value
        /// </summary>
        /// <param name="value">Database object</param>
        /// <returns>Boolean Value</returns>
        public static bool GetBool(object value)
        {
            if (value == null || value == DBNull.Value)
                return false;

            try
            {
                return Convert.ToBoolean(value);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get Database Guid Value
        /// </summary>
        /// <param name="value">Database object</param>
        /// <returns>Guid Value</returns>
        public static Guid GetGuid(object value)
        {
            if (value == null || value == DBNull.Value)
                return Guid.Empty;

            if (value is Guid)
                return (Guid)value;

            try
            {
                return Guid.Parse((string)value);
            }
            catch
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Get Database DateTime Value
        /// </summary>
        /// <param name="value">Database object</param>
        /// <returns>DateTime Value</returns>
        static public DateTime GetDateTime(object value)
        {
            if (value == null || value == DBNull.Value)
                return DateTime.MinValue;

            try
            {
                return Convert.ToDateTime(value);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Get Database DateTime Pointer Value
        /// </summary>
        /// <param name="value">Database object</param>
        /// <returns>DateTime Value</returns>
        static public DateTime? GetDateTimePtr(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            try
            {
                return Convert.ToDateTime(value);
            }
            catch
            {
                return null;
            }
        }

        static public string GetSQLNAVName(string name)
        {
            name = name.Replace('.', '_');
            name = name.Replace("'", "_");
            name = name.Replace('/', '_');
            name = name.Replace('"', '_');
            name = name.Replace("\\", "_");
            name = name.Replace('%', '_');
            name = name.Replace('[', '_');
            name = name.Replace(']', '_');
            return name;
        }

        //just for internal checks where using old sqlconnection
        static public string CheckForSQLInjection(string input, bool strict = false)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "";
            }
            string[] sqlCheckList = { "--",
                                ";--",
                                "/*",
                                "*/",
                                "@@",
                                 "xp_"
                            };

            string[] sqlCheckListStrict = { "--",
                            ";--",
                            ";",
                            "/*",
                            "*/",
                            "@@",
                            "@",
                            "char",
                            "nchar",
                            "varchar",
                            "nvarchar",
                            "alter",
                            "begin",
                            "cast",
                            "create",
                            "cursor",
                            "declare",
                            "delete",
                            "drop",
                            "end",
                            "exec",
                            "execute",
                            "fetch",
                            "insert",
                            "kill",
                            "select",
                            "sys",
                            "sysobjects",
                            "syscolumns",
                            "table",
                            "update",
                            "shutdown",
                            "xp_"
                        };

            if (strict)
            {
                sqlCheckList = sqlCheckListStrict;
            }

            string checkString = input.Replace("'", "''");
            for (int i = 0; i <= sqlCheckList.Length - 1; i++)
            {
                if ((checkString.IndexOf(sqlCheckList[i], StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    throw new UnauthorizedAccessException("CheckForSQLInjection found in: " + input);
                }
            }
            if (input.Contains("'"))
                input = input.Replace("'", "''");
            return input;
        }
    }
}
