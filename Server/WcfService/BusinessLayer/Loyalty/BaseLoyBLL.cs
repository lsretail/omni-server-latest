using System;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.BLL.Loyalty
{
    public abstract class BaseLoyBLL : BaseBLL
    {
        private static LSLogger logger = new LSLogger();
        protected int timeoutInSeconds = 0;

        //ALL  security related code is done in base class
        protected IValidationRepository iValidationRepository;

        private string contactId;
        private StatusCode securityTokenStatusCode;
        private string localCulture = null;

        public virtual string SecurityToken { get { return config.SecurityToken; } }
        public virtual string ContactId { get { return contactId; } }

        #region BOConnection

        private ILoyaltyBO iLoyBOConnection = null;
        private IAppBO iAppBOConnection = null;

        protected ILoyaltyBO BOLoyConnection
        {
            get
            {
                if (iLoyBOConnection == null)
                    iLoyBOConnection = GetBORepository<ILoyaltyBO>(config.LSKey.Key, config.IsJson);
                iLoyBOConnection.TimeoutInSeconds = this.timeoutInSeconds;
                return iLoyBOConnection;
            }
        }

        protected IAppBO BOAppConnection
        {
            get
            {
                if (iAppBOConnection == null)
                    iAppBOConnection = GetBORepository<IAppBO>(config.LSKey.Key, config.IsJson);
                iAppBOConnection.TimeoutInSeconds = this.timeoutInSeconds;
                return iAppBOConnection;
            }
        }

        #endregion

        public BaseLoyBLL(BOConfiguration config, int timeoutInSeconds) : this(config, "", timeoutInSeconds)
        {

        }

        public BaseLoyBLL(BOConfiguration config, string deviceId, int timeoutInSeconds) : base(config)
        {
            this.timeoutInSeconds = timeoutInSeconds;
            Init(config.SecurityToken);
            base.DeviceId = deviceId; //keep this order

            if (config.SecurityCheck)
            {
                SecurityCheck();
            }
        }

        private void Init(string securitytoken)
        {
            this.config.SecurityToken = securitytoken;
            base.DeviceId = string.Empty;
            this.contactId = string.Empty;
            this.securityTokenStatusCode = StatusCode.OK;
            this.iValidationRepository = GetDbRepository<IValidationRepository>(config);
        }

        protected void SecurityCheck()
        {
            //security_validatetoken  true/false  key can be added to bypass the security token validation. Appsettings table
            SecurityTokenCheck();

            //always validate security token and if device is blocked etc.  Will get deviceId and contactId back
            string deviceId = "";
            securityTokenStatusCode = iValidationRepository.ValidateSecurityToken(config.SecurityToken, out deviceId, out this.contactId);
            base.DeviceId = deviceId;
            if (config.SecurityCheck)
            {
                // 
                if (securityTokenStatusCode != StatusCode.OK)
                {
                    string msg = string.Empty;
                    config.SecurityToken = ""; //dont want to send the token back in error message
                    if (securityTokenStatusCode == StatusCode.DeviceIsBlocked)
                        msg = string.Format("Device has been blocked from usage: {0}", base.DeviceId);
                    else if (securityTokenStatusCode == StatusCode.SecurityTokenInvalid)
                        msg = string.Format("Security token not valid: {0}", config.SecurityToken);
                    else if (securityTokenStatusCode == StatusCode.UserNotLoggedIn)
                        msg = string.Format("User is not logged in: {0}", config.SecurityToken);

                    throw new LSOmniServiceException(securityTokenStatusCode, msg);
                }
            }
        }

        protected string SendToEcom(string command, object obj)
        {
            string payloadJson = new JavaScriptSerializer().Serialize(obj);
            return SendToEcom(command, payloadJson);
        }

        protected string SendToEcom(string command, string data)
        {
            try
            {
                string ecomUrl = config.SettingsGetByKey(ConfigKey.EComUrl);

                if (string.IsNullOrEmpty(ecomUrl))
                {
                    logger.Info(config.LSKey.Key, "ECOM Message Error: Missing Ecom.Url in Appsettings");
                    return "ERROR: Missing Ecom.Url in Appsettings";
                }

                if (ecomUrl.ToUpper() == "DEMO")
                {
                    logger.Info(config.LSKey.Key, "ECOM Demo mode on, return OK");
                    return "{ \"success\": true, \"message\": \"Command posted successfully\" }";
                }

                Uri url = new Uri(ecomUrl + "/" + command);
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                logger.Info(config.LSKey.Key, "ECOM Sent to:{0} Message:{1}", url.LocalPath, data);
                using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                string ret = string.Empty;
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    ret = streamReader.ReadToEnd();
                }

                logger.Info(config.LSKey.Key, "ECOM Result:[{0}]", ret);
                return ret;
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex);
                return "ERROR:" + ex.Message;
            }
        }

        #region validation

        private void SecurityTokenCheck()
        {
            if (config.SecurityCheck == false)
                return;

            if (Security.IsValidSecurityToken(this.SecurityToken) == false)
            {
                string msg = string.Format("SecurityToken:{0} is invalid.", this.SecurityToken);
                throw new LSOmniServiceException(StatusCode.SecurityTokenInvalid, msg);
            }
        }

        #endregion validation

        protected string GetAppSettingCurrencyCulture()
        {
            if (string.IsNullOrEmpty(localCulture))
            {
                config.SettingsGetByKey(ConfigKey.Currency_Culture);
            }
            return localCulture;
        }
    }
}
