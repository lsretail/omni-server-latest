using System;
using System.Globalization;
using System.Text;

namespace LSOmni.Common.Util
{
    public static class ConvertTo
    {
        public static decimal RoundIt(string val, decimal unit, CurrencyGetHelper.RoundingMethod roundMethod)
        {
            decimal d = Convert.ToDecimal(val, CultureInfo.InvariantCulture);
            return CurrencyGetHelper.RoundToUnit(d, unit, roundMethod);
        }

        public static decimal RoundIt(decimal val, decimal unit, CurrencyGetHelper.RoundingMethod roundMethod)
        {
            return CurrencyGetHelper.RoundToUnit(val, unit, roundMethod);
        }

        public static decimal SafeDecimal(string val, bool absValue = false)
        {
            if (string.IsNullOrWhiteSpace(val))
                return 0M;
            val = SafeDecimalString(val, absValue);
            return Convert.ToDecimal(val, CultureInfo.InvariantCulture);
        }

        public static int SafeInt(string val)
        {
            if (string.IsNullOrEmpty(val))
                return 0;

            if (int.TryParse(val, out int retVal))
                return retVal;
            else
            {
                if (decimal.TryParse(val, out decimal retdec))
                    return decimal.ToInt32(SafeDecimal(val));
                return 0;
            }
        }

        public static long SafeLong(string val)
        {
            if (string.IsNullOrEmpty(val))
                return 0L;

            if (long.TryParse(val, out long retVal))
                return retVal;
            else
                return 0L;
        }

        public static string SafeDecimalString(string val, bool absValue = false)
        {
            if (string.IsNullOrWhiteSpace(val))
                return "0";
            //for those that don't send decimal strings with a perion ("30.01") 
            //we need to check what the comma and period really mean in the decimal string

            val = val.Trim();
            //if , is a thousand separator then remove the comma
            if (val.Contains(",") && val.Contains(".") && (val.IndexOf(",") < val.IndexOf(".")))
                val = val.Replace(",", "");
            //if . is a thousand separator then remove the period and make the comma a period
            else if (val.Contains(",") && val.Contains(".") && (val.IndexOf(",") > val.IndexOf(".")))
            {
                val = val.Replace(".", "");
                val = val.Replace(",", ".");
            }
            else if (val.Contains(",") && !val.Contains("."))
                val = val.Replace(",", ".");

            //Math.Abs()
            if (absValue && val.StartsWith("-"))
                val = val.Replace("-", "");
            return val;
        }

        public static double SafeDouble(string val, bool absValue = false)
        {
            val = SafeDecimalString(val, absValue);
            return Convert.ToDouble(val, CultureInfo.InvariantCulture);
        }

        public static string SafeStringDecimal(decimal val, bool absValue = false)
        {
            if (absValue)
                val = Math.Abs(val);
            return Convert.ToString(val, CultureInfo.InvariantCulture);
        }

        public static bool SafeBoolean(string value)
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

        public static string SafeStringDateTime(DateTime dt)
        {
            //json friendly date, all dates sent to client do not have timezone info in them
            return dt.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static DateTime SafeDateTime(DateTime dt)
        {
            //json friendly date, all dates sent to client do not have timezone info in them
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc); //specify it as Utc for json to understand
        }

        public static DateTime NavJoinDateAndTime(DateTime date, DateTime time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
        }

        public static DateTime NavGetDate(DateTime date, bool returnMinDate)
        {
            if (date == DateTime.MinValue)
                return (returnMinDate) ? DateTime.MinValue : new DateTime(1753, 1, 1);

            return new DateTime(date.Year, date.Month, date.Day);
        }

        public static DateTime NavGetTime(DateTime date, bool returnMinDate)
        {
            if (date == DateTime.MinValue && returnMinDate) 
                return DateTime.MinValue;

            return new DateTime(1754, 1, 1, date.Hour, date.Minute, date.Second, date.Millisecond);
        }

        public static DateTime SafeJsonDate(DateTime date, bool json)
        {
            if (((date.Year == 1754 || date.Year == 1753) && date.Month == 1 && date.Day == 1) || (date == DateTime.MinValue))
            {
                if (json)
                    return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); // 1970 is a safe json year

                return DateTime.MinValue;   // SOAP date
            }
            return date;
        }

        public static DateTime SafeJsonTime(DateTime date, bool json)
        {
            if ((date.Year == 1753 && date.Month == 1 && date.Day == 1 && date.Hour == 0 && date.Minute == 0 && date.Second == 0) || (date == DateTime.MinValue))
            {
                if (json)
                    return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); // 1970 is a safe json year

                return DateTime.MinValue;   // SOAP date
            }
            if (json)
                return new DateTime(1970, 1, 1, date.Hour, date.Minute, date.Second);

            return new DateTime(1900, 1, 1, date.Hour, date.Minute, date.Second);
        }

        public static DateTime SafeDateTime(string value)
        {
            try
            {
                //stip out zulu time 2015-05-19T10:41:55Z since I send without zulu
                value = value.Replace("T", " ").Replace("Z", ""); //take out all timezone info
                //json friendly date
                DateTime dt = DateTime.Parse(value);
                return SafeDateTime(dt);
            }
            catch
            {
                return new DateTime(1970, 1, 1, 0, 0, 0); //1970 is a safe json year
            }
        }

        public static DateTime SafeTime(string value, bool useToday = false)
        {
            //json friendly date
            //values is hour only 11:30:00
            try
            {
                value = value.Replace("T", " ").Replace("Z", ""); //take out all timezone info
                string date = "";
                if (useToday)
                    date = string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), value); //using the current day
                else
                    date = string.Format("1970-01-01 {0}", value); //1970 is a safe json year
                DateTime dt = DateTime.Parse(date);
                return SafeDateTime(dt);
            }
            catch
            {
                return new DateTime(1970, 1, 1, 0, 0, 0); //1970 is a safe json year
            }
        }

        public static string SafeString(string[] value)
        {
            if (value == null || value.Length == 0)
                return string.Empty;
            return string.Concat(value);
        }

        public static string Base64Encode(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);

            if (base64EncodedBytes.Length == 1 && base64EncodedBytes[0] == 0)
                return string.Empty;

            if (base64EncodedBytes.Length > 1 && base64EncodedBytes[base64EncodedBytes.Length - 1] == 0)
            {
                byte[] tmp = new byte[base64EncodedBytes.Length - 1];
                Buffer.BlockCopy(base64EncodedBytes, 0, tmp, 0, base64EncodedBytes.Length - 1);
                base64EncodedBytes = tmp;
            }

            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}