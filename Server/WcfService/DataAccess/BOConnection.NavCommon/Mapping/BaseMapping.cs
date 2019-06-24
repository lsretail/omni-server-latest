using System;

namespace LSOmni.DataAccess.BOConnection.NavCommon.Mapping
{
    public abstract class BaseMapping
    {
        protected string GetString(string value)
        {
            if (value == null)
                return string.Empty;
            return value;
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
            {
                return lineNumber;
            }
        }

        protected DateTime DateAndTimeFromNav(DateTime date, DateTime time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        }

        internal static DateTime GetSQLNAVDate(DateTime date)
        {
            if (date == DateTime.MinValue)
                return new DateTime(1753, 1, 1);      // this is NULL Date for NAV

            return date;
        }
    }
}
