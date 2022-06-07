using System;
using System.Globalization;

namespace LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping
{
    public abstract class BaseXml
    {
        protected string wsEncoding;

        public BaseXml()
        {
            wsEncoding = "utf-8"; //default to utf-8
        }

        //needed for NAV 6.4 where 0  is false
        protected bool ToBool(string value)
        {
            try
            {
                value = value.ToLower();
                if (value == "1" || value == "true" || value == "yes")
                    return true;
                else if (value == "0" || value == "false" || value == "no")
                    return false;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        protected string ToNAVDate(DateTime dt)
        {
            if (dt == null || dt == DateTime.MinValue)
                return string.Empty;
            return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        protected string ToNAVTime(DateTime dt)
        {
            if (dt == null || dt == DateTime.MinValue)
                return string.Empty;
            return dt.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}
