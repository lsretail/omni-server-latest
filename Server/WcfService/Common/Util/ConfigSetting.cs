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

        public static string GetEncrCode()
        {
            string pwd = ConfigSetting.GetString("Security.EncrCode");
            return DecryptConfigValue.DecryptString(pwd, string.Empty);
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
            if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out ret))
                return ret;
            else
                throw new ArgumentException("AppSettings key: " + key + " does not have double value: " + value, key);
        }

        public static string TenderTypeMapping(string tenderMapping, string tenderType, bool toOmni)
        {
            try
            {
                int tenderTypeId = -1;
                if (string.IsNullOrWhiteSpace(tenderMapping))
                {
                    return tenderType;
                }

                // first one is Commerce Service for LS Central TenderType, 2nd one is the NAV id
                //tenderMapping: "1=1,2=2,3=3,4=4,6=6,7=7,8=8,9=9,10=10,11=11,15=15,19=19"
                string[] commaMapping = tenderMapping.Split(',');
                foreach (string s in commaMapping)
                {
                    string[] eqMapping = s.Split('=');
                    if (toOmni)
                    {
                        if (tenderType == eqMapping[1].Trim())
                        {
                            tenderTypeId = Convert.ToInt32(eqMapping[0].Trim());
                            break;
                        }
                    }
                    else
                    {
                        if (tenderType == eqMapping[0].Trim())
                        {
                            tenderTypeId = Convert.ToInt32(eqMapping[1].Trim());
                            break;
                        }
                    }
                }

                if (tenderTypeId == -1)
                    return tenderType;

                return tenderTypeId.ToString();
            }
            catch
            {
                return tenderType;
            }
        }
    }
}
