﻿using System;
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

        public bool IsJson { get; set; }
        public string AppId { get; set; }

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
            var setting = Settings.FirstOrDefault(x => x.Key == key.ToString());
            return setting == null ? string.Empty : setting.Value;
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
            return SettingsGetByKey(key).ToLower() == "true";
        }

        public decimal SettingsDecimalGetByKey(ConfigKey key)
        {
            string val = SettingsGetByKey(key);
            if (string.IsNullOrEmpty(val))
                return 0;

            return Convert.ToDecimal(val);
        }

        public bool SettingsKeyExists(ConfigKey key)
        {
            return SettingsGetByKey(key) != null;
        }

        public void SettingsUpdateByKey(ConfigKey key, string value)
        {
            TenantSetting setting = Settings.FirstOrDefault(x => x.Key == key.ToString());
            if (setting == null)
                return;
            setting.Value = value;
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
