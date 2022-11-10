using System;

using LSOmni.DataAccess.Interface.Repository;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSOmni.Common.Util;

namespace LSOmni.BLL
{
    public class ConfigBLL : BaseBLL
    {
        private IConfigRepository iConfigRepository;
        private ILoyaltyBO BOLoyConnection = null;

        public ConfigBLL(BOConfiguration config) : base(config)
        {
            this.iConfigRepository = GetDbRepository<IConfigRepository>(config);

            if (config != null)
                BOLoyConnection = GetBORepository<ILoyaltyBO>(config?.LSKey.Key, config.IsJson);
        }

        public ConfigBLL() : base(null)
        {
            this.iConfigRepository = GetDbRepository<IConfigRepository>(config);
        }

        public virtual bool ConfigKeyExists(ConfigKey key, string lsKey)
        {
            return this.iConfigRepository.ConfigKeyExists(lsKey, key);
        }

        public virtual void ConfigSetByKey(string lsKey, ConfigKey key, string value, string valueType, bool advanced, string comment)
        {
            this.iConfigRepository.ConfigSetByKey(lsKey, key, value, valueType, advanced, comment);
        }

        public virtual BOConfiguration ConfigGet(string key)
        {
            //check if key is active
            if (iConfigRepository.ConfigIsActive(key) == false)
            {
                throw new LSOmniServiceException(StatusCode.LSKeyInvalid, "LSKey is not active");
            }

            return iConfigRepository.ConfigGet(key);
        }

        public virtual void DbCleanup(int daysLog, int daysNotify, int daysOneList)
        {
            this.iConfigRepository.DbCleanUp(daysLog, daysNotify, daysOneList);
        }

        #region ping

        public virtual void PingOmniDB()
        {
            iConfigRepository.PingOmniDB();
        }

        public virtual string PingWs(out string centralVersion)
        {
            // Nav returns the version number
            return BOLoyConnection.Ping(out centralVersion);
        }

        public virtual string PingNavDb()
        {
            BOLoyConnection.TimeoutInSeconds = 4;
            Scheme ret = BOLoyConnection.SchemeGetById("Ping", new Statistics());
            return ret.Id;
        }

        #endregion ping
    }
}
