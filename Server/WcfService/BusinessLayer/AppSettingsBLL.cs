using System;

using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.BLL
{
    public class AppSettingsBLL : BaseBLL
    {
        #region BOConnection

        //BOConnection used here to ping the nav service and db
        private ILoyaltyBO iLoyBOConnection = null;

        protected ILoyaltyBO BOLoyConnection
        {
            get
            {
                if (iLoyBOConnection == null)
                    iLoyBOConnection = GetBORepository<ILoyaltyBO>();
                return iLoyBOConnection;
            }
        }

        #endregion BOConnection

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private IAppSettingsRepository iAppSettingsRepository;

        public AppSettingsBLL() : base()
        {
            this.iAppSettingsRepository = GetDbRepository<IAppSettingsRepository>(); 
        }

        public virtual string AppSettingsGetByKey(AppSettingsKey key, string languageCode="en")
        {
            return this.iAppSettingsRepository.AppSettingsGetByKey(key, languageCode);
        }

        public virtual bool AppSettingsBoolGetByKey(AppSettingsKey key, string languageCode = "en")
        {
            return this.iAppSettingsRepository.AppSettingsBoolGetByKey(key, languageCode);
        }

        public virtual int AppSettingsIntGetByKey(AppSettingsKey key, string languageCode = "en")
        {
            return this.iAppSettingsRepository.AppSettingsIntGetByKey(key, languageCode);
        }

        public virtual decimal AppSettingsDecimalGetByKey(AppSettingsKey key, string languageCode = "en")
        {
            return this.iAppSettingsRepository.AppSettingsDecimalGetByKey(key, languageCode);
        }

        public virtual bool AppSettingsKeyExists(AppSettingsKey key, string languageCode = "en")
        {
            return this.iAppSettingsRepository.AppSettingsKeyExists(key, languageCode);
        }
		
        public virtual void AppSettingsSetByKey(AppSettingsKey key, string value, string languageCode = "en")
        {
            this.iAppSettingsRepository.AppSettingsSetByKey(key, value, "string", "New String Value", languageCode);
        }

        public virtual void AppSettingsSetByKey(AppSettingsKey key, string value, string valuetype, string comment, string languageCode = "en")
        {
            this.iAppSettingsRepository.AppSettingsSetByKey(key, value, valuetype, comment, languageCode);
        }

        public virtual bool SkipPrinting()
        {
            //<!-- demo.print.enabled = false; true then print receipt in NAV will not be called  -->
            bool skipPrinting = false;
            try
            {
                //really shouldn't bother since MPOS shouldn't try and connect to Omni database - but OK to keep it here.
                skipPrinting = this.iAppSettingsRepository.AppSettingsBoolGetByKey(AppSettingsKey.Demo_Print_Enabled,
                    "en");

            }
            catch (Exception ex)
            {
                //ignor error here.. throw ex;
                logger.Warn("SkipPrinting: " + ex.Message);
            }
            return skipPrinting;
        }

        public virtual bool DbIsConnected(bool throwEx = false)
        {
            //simply test if we can connect to LSOmniDB 
            try
            {
                this.iAppSettingsRepository.AppSettingsGetByKey(AppSettingsKey.Security_Validatetoken, "en");
                return true;
            }
            catch (Exception ex)
            {
                logger.Debug("DbConnection() failed. " + ex.Message);
                if (throwEx)
                    throw;
                else
                    return false;
            }
        }

        public virtual void DbCleanUp(int daysLog, int daysQueue, int daysNotify, int daysUser, int daysOneList)
        {
            try
            {
                this.iAppSettingsRepository.DbCleanUp(daysLog, daysQueue, daysNotify, daysUser, daysOneList);
            }
            catch (Exception ex)
            {
                logger.Log(NLog.LogLevel.Info, ex, "Relax only the StartDbCleanUpThread failed. ");
                throw;
            }
        }

        #region ping

        public virtual void PingOmniDb()
        {
            this.iAppSettingsRepository.Ping();
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

 