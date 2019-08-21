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
            BOLoyConnection = GetBORepository<ILoyaltyBO>(config.LSKey.Key);
        }
        public ConfigBLL() : base(null)
        {
            this.iConfigRepository = GetDbRepository<IConfigRepository>(config);
        }

        public virtual bool ConfigKeyExists(ConfigKey key, string lsKey)
        {
            return this.iConfigRepository.ConfigKeyExists(lsKey, key);
        }

        public virtual void ConfigSetByKey(string lsKey, ConfigKey key, string value, string valueType)
        {
            this.iConfigRepository.ConfigSetByKey(lsKey, key, value, valueType);
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

        public virtual string PingWs(string ipAddress)
        {
            //Nav returns the version number, Ax returns "AX"
            return BOLoyConnection.Ping(ipAddress);
        }

        public virtual string PingNavDb()
        {
            BOLoyConnection.TimeoutInSeconds = 4;
            Scheme ret = BOLoyConnection.SchemeGetById("Ping");
            return ret.Id;
        }

        #endregion ping
    }
}
