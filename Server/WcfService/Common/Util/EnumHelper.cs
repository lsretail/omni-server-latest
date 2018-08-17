using System;
using System.Reflection;

namespace LSOmni.Common.Util
{
    public static class EnumHelper
    {
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

        static public string GetEnumSQLInValue(object en)
        {
            string[] st = en.ToString().Split(',');
            string stt = string.Empty;
            foreach (string s in st)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;

                stt += string.Format("'{0}',", s.Trim());
            }
            return stt.TrimEnd(',');
        }
    }
}
