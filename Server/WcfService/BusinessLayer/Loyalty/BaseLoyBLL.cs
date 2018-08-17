using System;

using NLog;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Dal;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;

namespace LSOmni.BLL.Loyalty
{
    public abstract class BaseLoyBLL : BaseBLL
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        protected int timeoutInSeconds = 0;

        private static object SecurityValidateTokenObj = null;
        private static bool SecurityValidateToken = false;
        //ALL  security related code is done in base class
        protected IValidationRepository iValidationRepository;

        protected string securityToken;
        private string contactId;
        private StatusCode securityTokenStatusCode;
        private string localCulture = null;

        public virtual string SecurityToken { get { return securityToken; } }
        public virtual string ContactId { get { return contactId; } }

        #region BOConnection

        private ILoyaltyBO iLoyBOConnection = null;
        private IAppBO iAppBOConnection = null;

        protected ILoyaltyBO BOLoyConnection
        {
            get
            {
                if (iLoyBOConnection == null)
                    iLoyBOConnection = GetBORepository<ILoyaltyBO>();
                iLoyBOConnection.TimeoutInSeconds = this.timeoutInSeconds;
                return iLoyBOConnection;
            }
        }

        protected IAppBO BOAppConnection
        {
            get
            {
                if (iAppBOConnection == null)
                    iAppBOConnection = GetBORepository<IAppBO>();
                return iAppBOConnection;
            }
        }

        #endregion

        private static object lockObject = new object();
        private static string LicenseKey = string.Empty;

        public BaseLoyBLL(int timeoutInSeconds)
            : this("", "", timeoutInSeconds)
        {
            //this constructor does NOT do any security checks. Used for Login() and ContactCreate()

        }

        public BaseLoyBLL(string securitytoken, int timeoutInSeconds)
            : this(securitytoken, "", timeoutInSeconds)
        {

        }

        public BaseLoyBLL(string securitytoken, string deviceId, int timeoutInSeconds)
        {
            this.timeoutInSeconds = timeoutInSeconds;
            if (SecurityValidateTokenObj == null)
            {
                SecurityValidateTokenObj = "x";
                try
                {
                    SecurityValidateToken = ConfigSetting.GetBoolean("Security.Validatetoken");
                }
                catch
                {
                    // find in database if not found in config file
                    IAppSettingsRepository iRepository = GetDbRepository<IAppSettingsRepository>();
                    SecurityValidateToken = iRepository.AppSettingsBoolGetByKey(AppSettingsKey.Security_Validatetoken);
                }
            }

            Init(securitytoken);
            base.DeviceId = deviceId; //keep this order

            if (string.IsNullOrWhiteSpace(securitytoken) == false)
            {
                SecurityCheck();
            }
        }

        private void Init(string securitytoken)
        {
            this.securityToken = securitytoken;
            base.DeviceId = string.Empty;
            this.contactId = string.Empty;
            this.securityTokenStatusCode = StatusCode.OK;
            this.iValidationRepository = GetDbRepository<IValidationRepository>();
        }

        protected void SecurityCheck()
        {
            //security_validatetoken  true/false  key can be added to bypass the securitytoken validation. Appsettings table
            SecurityTokenCheck();

            //always validate security token and if device is blocked etc.  Will get deviceId and contactId back
            string deviceId = "";
            securityTokenStatusCode = iValidationRepository.ValidateSecurityToken(securityToken, out deviceId, out contactId);
            base.DeviceId = deviceId;
            if (SecurityValidateToken)
            {
                // 
                if (securityTokenStatusCode != StatusCode.OK)
                {
                    string msg = string.Empty;
                    securityToken = ""; //dont want to send the token back in error message
                    if (securityTokenStatusCode == StatusCode.DeviceIsBlocked)
                        msg = string.Format("Device has been blocked from usage: {0}", base.DeviceId);
                    else if (securityTokenStatusCode == StatusCode.SecurityTokenInvalid)
                        msg = string.Format("Security token not valid: {0}", securityToken);
                    else if (securityTokenStatusCode == StatusCode.UserNotLoggedIn)
                        msg = string.Format("User is not logged in: {0}", securityToken);

                    throw new LSOmniServiceException(securityTokenStatusCode, msg);
                }
            }
        }

        #region validation

        //all data validations are in base class
        protected void ValidateContact(string contId)
        {
            if (string.IsNullOrEmpty(contId))
                return;

            contId = contId.Trim();
            if (SecurityValidateToken == false)
                return;

            if (iValidationRepository.ValidateContact(contId, this.ContactId) == false)
            {
                //throw an error if id passed in is not tied to security token  
                // security token retrived the ContactId so OK to use it

                string msg = "ContactId and SecurityToken do not match.";
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, msg);
            }
        }

        protected void ValidateCard(string cardId)
        {
            cardId = cardId.Trim();
            if (SecurityValidateToken == false)
                return;

            if (iValidationRepository.ValidateCard(cardId, this.ContactId) == false)
            {
                //throw an error if id passed in is not tied to security token  
                // security token retrived the ContactId so OK to use it

                string msg = "cardId and SecurityToken do not match.";
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, msg);
            }
        }

        protected void ValidateContactUserName(string userName)
        {
            if (SecurityValidateToken == false)
                return;

            if (iValidationRepository.ValidateContactUserName(userName, this.ContactId) == false)
            {
                //throw an error if id passed in is not tied to security token  
                // security token retrived the ContactId so OK to use it

                string msg = "UserName and SecurityToken do not match.";
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, msg);
            }
        }

        protected void ValidateAccount(string accountId)
        {
            if (SecurityValidateToken == false)
                return;

            if (iValidationRepository.ValidateAccount(accountId, this.ContactId) == false)
            {
                //throw an error if id passed in is not tied to security token  
                // security token retrived the ContactId so OK to use it

                string msg = "AccountId and SecurityToken do not match.";
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, msg);
            }
        }

        protected void ValidateOneList(string oneListId)
        {
            if (SecurityValidateToken == false)
                return;

            if (iValidationRepository.ValidateOneList(oneListId, this.ContactId) == false)
            {
                //throw an error if id passed in is not tied to security token  
                // security token retrived the ContactId so OK to use it

                string msg = "OneListId and SecurityToken do not match.";
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, msg);
            }
        }

        private void SecurityTokenCheck()
        {
            if (SecurityValidateToken == false)
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
                AppSettingsRepository appRep = new AppSettingsRepository();
                localCulture = appRep.AppSettingsGetByKey(AppSettingsKey.Currency_Culture, "en");
            }
            return localCulture;
        }
    }
}
