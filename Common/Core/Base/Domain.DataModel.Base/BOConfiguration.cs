using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class BOConfiguration : IDisposable
    {
        [DataMember]
        public LSKey LSKey { get; set; }
        [DataMember]
        public string SecurityToken { get; set; }
        [DataMember]
        public bool SecurityCheck { get; set; }
        [DataMember]
        public Dictionary<string, string> Settings { get; set; }

        public BOConfiguration() : this(string.Empty)
        {
        }

        public BOConfiguration(string key)
        {
            LSKey = new LSKey(key);
            Settings = new Dictionary<string, string>();
            SecurityCheck = false;
            SecurityToken = string.Empty;
        }

        public string SettingsGetByKey(ConfigKey key)
        {
            Settings.TryGetValue(key.ToString(), out string value);
            return value;
            
        }
        public int SettingsIntGetByKey(ConfigKey key)
        {
            string val = SettingsGetByKey(key);
            if (string.IsNullOrEmpty(val))
                return 0;

            return Convert.ToInt32(val);
        }
        public bool SettingsBoolGetByKey(ConfigKey key)
        {
            return SettingsGetByKey(key) == "true";
        }
        public decimal SettingsDecimalGetByKey(ConfigKey key)
        {
            return Convert.ToDecimal(SettingsGetByKey(key));
        }
        public bool SettingsKeyExists(ConfigKey key)
        {
            return SettingsGetByKey(key) != null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}
