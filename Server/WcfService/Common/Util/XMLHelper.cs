using System;
using System.Reflection;
using System.Xml.Linq;

namespace LSOmni.Common.Util
{
    public static class XMLHelper
    {
        /// <summary>
        /// Get String value from XElement, creates and returns default value if XElement is empty or not found
        /// </summary>
        /// <param name="el">XElement object</param>
        /// <param name="value">XElement name to look for</param>
        /// <param name="defvalue">Default value to return if XElement is not found</param>
        /// <returns>String Value from XElement</returns>
        static public string GetXMLValue(XElement el, string value, string defvalue)
        {
            if (el == null)
                return defvalue;

            try
            {
                return el.Element(value).Value;
            }
            catch
            {
                if (defvalue == null)
                    throw new Exception(value + " node missing in string");
                else
                {
                    el.Add(new XElement(value, defvalue));
                    return defvalue;
                }
            }
        }

        /// <summary>
        /// Get String value from XElement, creates and returns default value if XElement is empty or not found
        /// </summary>
        /// <param name="el">XElement object</param>
        /// <param name="value">XElement name to look for</param>
        /// <returns>String Value from XElement</returns>
        static public string GetXMLValue(XElement el, string value)
        {
            return GetXMLValue(el, value, null);
        }

        /// <summary>
        /// Get Int32 value from XElement, returns default value if XElement is empty or not found
        /// </summary>
        /// <param name="el">XElement object</param>
        /// <param name="value">XElement name to look for</param>
        /// <param name="defvalue">Default value to return if XElement is not found</param>
        /// <returns>Int32 Value from XElement</returns>
        static public Int32 GetXMLInt32(XElement el, string value, Int32 defvalue)
        {
            string tmp = GetXMLValue(el, value, defvalue.ToString());
            if (string.IsNullOrEmpty(tmp))
                return defvalue;
            return Convert.ToInt32(tmp);
        }

        /// <summary>
        /// Get Int32 value from XElement, returns default value if XElement is empty or not found
        /// </summary>
        /// <param name="el">XElement object</param>
        /// <param name="value">XElement name to look for</param>
        /// <returns>Int32 Value from XElement</returns>
        static public Int32 GetXMLInt32(XElement el, string value)
        {
            string tmp = GetXMLValue(el, value);
            if (string.IsNullOrEmpty(tmp))
                return 0;
            return Convert.ToInt32(tmp);
        }

        /// <summary>
        /// Get Int64 value from XElement, returns default value if XElement is empty or not found
        /// </summary>
        /// <param name="el">XElement object</param>
        /// <param name="value">XElement name to look for</param>
        /// <param name="defvalue">Default value to return if XElement is not found</param>
        /// <returns>Int64 Value from XElement</returns>
        static public Int64 GetXMLInt64(XElement el, string value, Int64 defvalue)
        {
            string tmp = GetXMLValue(el, value, defvalue.ToString());
            if (String.IsNullOrEmpty(tmp))
                return defvalue;
            return Convert.ToInt64(tmp);
        }

        /// <summary>
        /// Get Int64 value from XElement, returns default value if XElement is empty or not found
        /// </summary>
        /// <param name="el">XElement object</param>
        /// <param name="value">XElement name to look for</param>
        /// <returns>Int64 Value from XElement</returns>
        static public Int64 GetXMLInt64(XElement el, string value)
        {
            string tmp = GetXMLValue(el, value);
            if (String.IsNullOrEmpty(tmp))
                return 0;
            return Convert.ToInt64(tmp);
        }

        /// <summary>
        /// Get Double value from XElement, returns default value if XElement is empty or not found
        /// </summary>
        /// <param name="el">XElement object</param>
        /// <param name="value">XElement name to look for</param>
        /// <param name="defvalue">Default value to return if XElement is not found</param>
        /// <returns>Double Value from XElement</returns>
        static public Double GetXMLDouble(XElement el, string value, Double defvalue)
        {
            string tmp = GetXMLValue(el, value, defvalue.ToString());
            if (String.IsNullOrEmpty(tmp))
                return defvalue;

            return ConvertTo.SafeDouble(tmp);
        }

        /// <summary>
        /// Get Double value from XElement, returns default value if XElement is empty or not found
        /// </summary>
        /// <param name="el">XElement object</param>
        /// <param name="value">XElement name to look for</param>
        /// <returns>Double Value from XElement</returns>
        static public Double GetXMLDouble(XElement el, string value)
        {
            string tmp = GetXMLValue(el, value);
            if (String.IsNullOrEmpty(tmp))
                return 0.0;

            return ConvertTo.SafeDouble(tmp);
        }

        /// <summary>
        /// Get Boolean value from XElement, returns default value if XElement is empty or not found
        /// </summary>
        /// <param name="el">XElement object</param>
        /// <param name="value">XElement name to look for</param>
        /// <param name="defvalue">Default value to return if XElement is not found</param>
        /// <returns>Boolean Value from XElement</returns>
        static public bool GetXMLBool(XElement el, string value, bool defvalue)
        {
            string tmp = GetXMLValue(el, value, defvalue.ToString());
            if (String.IsNullOrEmpty(tmp))
                return defvalue;
            return Convert.ToBoolean(tmp);
        }

        /// <summary>
        /// Get Boolean value from XElement, returns default value if XElement is empty or not found
        /// </summary>
        /// <param name="el">XElement object</param>
        /// <param name="value">XElement name to look for</param>
        /// <returns>Boolean Value from XElement</returns>
        static public bool GetXMLBool(XElement el, string value)
        {
            string tmp = GetXMLValue(el, value);
            if (String.IsNullOrEmpty(tmp))
                return false;
            return Convert.ToBoolean(tmp);
        }

        /// <summary>
        /// Get Guid value from XElement, returns default value if XElement is empty or not found
        /// </summary>
        /// <param name="el">XElement object</param>
        /// <param name="value">XElement name to look for</param>
        /// <returns>Guid Value from XElement</returns>
        static public Guid GetXMLGuid(XElement el, string value)
        {
            string tmp = GetXMLValue(el, value);
            if (String.IsNullOrEmpty(tmp))
                return Guid.Empty;
            return Guid.Parse(tmp);
        }

        /// <summary>
        /// Get DateTime value from XElement, returns default value if XElement is empty or not found
        /// </summary>
        /// <param name="el">XElement object</param>
        /// <param name="value">XElement name to look for</param>
        /// <param name="defvalue">Default value to return if XElement is not found</param>
        /// <returns>DateTime Value from XElement</returns>
        static public DateTime GetXMLDateTime(XElement el, string value, DateTime defvalue)
        {
            string tmp = GetXMLValue(el, value, defvalue.ToString());
            if (String.IsNullOrEmpty(tmp))
                return defvalue;
            return DateTime.Parse(tmp);
        }

        /// <summary>
        /// Get DateTime value from XElement, returns default value if XElement is empty or not found
        /// </summary>
        /// <param name="el">XElement object</param>
        /// <param name="value">XElement name to look for</param>
        /// <returns>DateTime Value from XElement</returns>
        static public DateTime GetXMLDateTime(XElement el, string value)
        {
            string tmp = GetXMLValue(el, value);
            if (String.IsNullOrEmpty(tmp))
                return DateTime.MinValue;
            return DateTime.Parse(tmp);
        }

        /// <summary>
        /// Convert any possible string-Value of a given enumeration
        /// type to its internal representation.
        /// </summary>
        /// <param name="t">Enum object to convert value to</param>
        /// <param name="Value">Enum Value String</param>
        /// <returns>Enum Value</returns>
        static public object StringToEnum(Type t, string Value)
        {
            return StringToEnum(t, Value, true);
        }

        /// <summary>
        /// Convert any possible string-Value of a given enumeration
        /// type to its internal representation.
        /// </summary>
        /// <param name="t">Enum object to convert value to</param>
        /// <param name="Value">Enum Value String</param>
        /// <param name="usedefault">if false the function returns NULL if value not found in Enum otherwise it returns first Enum value</param>
        /// <returns>Enum Value</returns>
        static public object StringToEnum(Type t, string Value, bool usedefault)
        {
            if (Value == null)
                return null;

            FieldInfo defaultfieldinfo = null;
            foreach (FieldInfo fi in t.GetFields())
            {
                if (fi.Name.Equals(Value, StringComparison.InvariantCultureIgnoreCase))
                    return fi.GetValue(null);   // We use null because enumeration values are static

                // save first value in enum as default if nothing is found
                if (usedefault && defaultfieldinfo == null && fi.FieldType == t)
                    defaultfieldinfo = fi;
            }

            if (defaultfieldinfo == null)
                return null;

            return defaultfieldinfo.GetValue(null);
        }

        static public string GetString(string value)
        {
            if (value == null)
                return string.Empty;
            return value;
        }

        static public int GetWebBoolInt(string value)
        {
            bool b = ConvertTo.SafeBoolean(value);
            return (b) ? 1 : 0;
        }

        static public DateTime GetWebDateTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DateTime.MinValue;
            return DateTime.Parse(value);
        }

        static public string ToNAVDate(DateTime dt)
        {
            if (dt == null || dt == DateTime.MinValue)
                return string.Empty;
            return string.Format("{0}{1}{2}", dt.Day.ToString("D2"), dt.Month.ToString("D2"), dt.Year);
        }

        static public DateTime GetSQLNAVDate(DateTime date)
        {
            if (date == DateTime.MinValue)
                return new DateTime(1753, 1, 1, 0, 0, 0);      // this is NULL Date for NAV
            return date;
        }

        static public DateTime GetSQLNAVTime(DateTime date)
        {
            if (date == DateTime.MinValue)
                return new DateTime(1754, 1, 1, 0, 0, 0);      // this is NULL Time for NAV
            return date;
        }

        /// <summary>
        /// Get SQL version of NAV table/field name  ."\/'
        /// </summary>
        /// <param name="name">Dynamic NAV SQL name</param>
        /// <returns>SQL name</returns>
        public static string GetSQLNAVName(string name)
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

        public static int LineNumberToNav(int lineNumber)
        {
            //multiply with 1000 for nav, if not already done!
            return (lineNumber >= 1000 ? lineNumber : lineNumber * 10000);
        }
    }
}
