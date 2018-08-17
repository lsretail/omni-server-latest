using System;
using System.Configuration;

namespace LSOmni.Common.Util
{
    public static class ConfigSetting
    {
        public static bool KeyExists(string key)
        {
            if (ConfigurationManager.AppSettings[key] != null)
                return true;
            else
                return false;
        }

        public static string GetString(string key, string defaultValue = "")
        {
            if (ConfigurationManager.AppSettings[key] != null)
                return ConfigurationManager.AppSettings[key].ToString();
            else
            {
                if (string.IsNullOrWhiteSpace(defaultValue))
                    throw new ArgumentException("AppSettings key: " + key + " doesn't exists in the current configurations", key);
                else
                    return defaultValue;
            }
        }

        public static void SetString(string key, string value)
        {
            ConfigurationManager.AppSettings[key] = value;
        }

        public static bool GetBoolean(string key)
        {
            string value = GetString(key);
            bool ret = false;
            if (bool.TryParse(value, out ret))
                return ret;
            else
                throw new ArgumentException("AppSettings key: " + key + " does not have boolean value: " + value, key);
        }

        public static void SetBoolean(string key, bool value)
        {
            SetString(key, value.ToString());
        }

        public static int GetInt(string key)
        {
            string value = GetString(key);
            int ret = 0;
            if (int.TryParse(value, out ret))
                return ret;
            else
                throw new ArgumentException("AppSettings key: " + key + " does not have integer value: " + value, key);
        }

        public static void SetInt(string key, int value)
        {
            SetString(key, value.ToString());
        }

        public static double GetDouble(string key)
        {
            string value = GetString(key);
            double ret = 0.0;
            if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out ret))
                return ret;
            else
                throw new ArgumentException("AppSettings key: " + key + " does not have double value: " + value, key);
        }

        public static decimal GetDecimal(string key)
        {
            string value = GetString(key);
            decimal ret = 0.0M;
            if (decimal.TryParse(value,System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture,out ret))
                return ret;
            else
                throw new ArgumentException("AppSettings key: " + key + " does not have double value: " + value, key);
        }
    }
}
