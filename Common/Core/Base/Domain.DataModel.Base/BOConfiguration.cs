using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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
        public List<TenantSetting> Settings { get; set; }

        public BOConfiguration() : this(string.Empty)
        {
        }

        public BOConfiguration(string key)
        {
            LSKey = new LSKey(key);
            Settings = new List<TenantSetting>();
            SecurityCheck = false;
            SecurityToken = string.Empty;
        }

        public string SettingsGetByKey(ConfigKey key)
        {
            TenantSetting setting = Settings.FirstOrDefault(x => x.Key == key.ToString());
            return setting == null ? "" : setting.Value;
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
