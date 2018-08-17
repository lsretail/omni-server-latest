using System;
using System.Globalization;
using System.Text;

namespace LSOmni.Common.Util
{
    public static class ConvertTo
    {
        public static Decimal RoundIt(string val, decimal unit, CurrencyGetHelper.RoundingMethod roundMethod)
        {
            decimal d = Convert.ToDecimal(val, CultureInfo.InvariantCulture);
            return CurrencyGetHelper.RoundToUnit(d, unit, roundMethod);
        }

        public static Decimal RoundIt(decimal val, decimal unit, CurrencyGetHelper.RoundingMethod roundMethod)
        {
            return CurrencyGetHelper.RoundToUnit(val, unit, roundMethod);
        }

        public static Decimal SafeDecimal(string val, bool absValue = false)
        {
            if (string.IsNullOrWhiteSpace(val))
                return 0M;
            val = SafeDecimalString(val, absValue);
            //d = Math.Round(d,decimals);
            return Convert.ToDecimal(val, CultureInfo.InvariantCulture);
        }

        public static int SafeInt(string val)
        {
            if (string.IsNullOrEmpty(val))
                return 0;

            int retVal;
            if (Int32.TryParse(val, out retVal))
                return retVal;
            else
                return 0;
        }

        public static long SafeLong(string val)
        {
            if (string.IsNullOrEmpty(val))
                return 0L;

            long retVal;
            if (Int64.TryParse(val, out retVal))
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

        public static Double SafeDouble(string val, bool absValue = false)
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

        public static string Base64Encode(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}