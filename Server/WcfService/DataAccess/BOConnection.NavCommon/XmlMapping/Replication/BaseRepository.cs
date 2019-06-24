using System;
using LSOmni.Common.Util;

namespace LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Replication
{
    public abstract class BaseRepository
    {
        protected static LSLogger logger = new LSLogger();

        internal static int GetWebBoolInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;
            if (value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                return 1;
            if (value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                return 0;
            return Convert.ToInt32(value);
        }

        internal static bool GetWebBool(string value)
        {
            return (GetWebBoolInt(value) == 1);
        }

        internal static DateTime GetWebDateTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new DateTime(1970, 1, 1); //1970 is a safe json year
            return DateTime.Parse(value);
        }

        internal static int GetWebInt(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;
            return Convert.ToInt32(value);
        }

        internal static decimal GetWebDecimal(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;
            return Convert.ToDecimal(value);
        }
    }
}
