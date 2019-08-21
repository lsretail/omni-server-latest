using LSRetail.Omni.Domain.DataModel.Base;
using System.Collections.Generic;

namespace LSOmni.DataAccess.Interface.Repository
{
    public interface IConfigRepository
    {
        void ConfigSetByKey(string lsKey, ConfigKey key, string value, string valueType);
        bool ConfigKeyExists(string lsKey, ConfigKey key);
        void PingOmniDB();
        BOConfiguration ConfigGet(string lsKey);
        List<BOConfiguration> ConfigGetAll();
        List<BOConfiguration> ConfigGetByKeys(List<LSKey> lsKeys);
        void SaveConfig(BOConfiguration config);
        bool ConfigIsActive(string lskey);
        bool ConfigExists(string lskey);
        void ToggleLSKey(string lskey, bool toggle);
        void DbCleanUp(int daysLog, int daysNotify, int daysOneList);
        void Delete(string lskey);
        void ResetDefaults(string lskey);
    }
}
