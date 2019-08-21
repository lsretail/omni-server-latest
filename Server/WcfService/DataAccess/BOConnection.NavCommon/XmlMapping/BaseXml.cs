using System;
using System.Globalization;

namespace LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping
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

        protected int LineNumberToNav(int lineNumber)
        {
            //multiply with 1000 for nav, if not already done!
            return (lineNumber >= 1000 ? lineNumber : lineNumber * 10000);
        }

        protected int LineNumberFromNav(int lineNumber)
        {
            //div by 1000 for nav, if not already done!
            if ((lineNumber % 10000) == 0)
            {
                return (lineNumber >= 10000 ? lineNumber / 10000 : lineNumber);
            }
            else
                return lineNumber;
        }
    }
}
