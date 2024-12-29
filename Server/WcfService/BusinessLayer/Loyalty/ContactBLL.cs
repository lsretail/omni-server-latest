using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.BLL.Loyalty
{
    public class ContactBLL : BaseLoyBLL
    {
        private readonly IDeviceRepository iDeviceRepository;

        public ContactBLL(BOConfiguration config, int timeoutInSeconds) : base(config, timeoutInSeconds)
        {
            iDeviceRepository = GetDbRepository<IDeviceRepository>(config);
        }

        #region login and contacts
 
        public virtual void DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model)
        {
            //TODO: Implement?
        }

        public virtual decimal CardGetPointBalance(string cardId, Statistics stat)
        {
            SecurityCardCheck(cardId);
            return BOLoyConnection.MemberCardGetPoints(cardId, stat);
        }

        public virtual List<PointEntry> CardGetPointEntries(string cardNo, DateTime dateFrom, Statistics stat)
        {
            SecurityCardCheck(cardNo);
            return BOLoyConnection.PointEntriesGet(cardNo, dateFrom, stat);
        }

        public virtual List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned, Statistics stat)
        {
            List<MemberContact> list = BOLoyConnection.ContactSearch(searchType, search, maxNumberOfRowsReturned, stat);
            foreach (MemberContact contact in list)
            {
                if (contact.Cards.Count > 0)
                    SecurityCardCheck(contact.Cards.FirstOrDefault().Id);
            }
            return list;
        }

        public virtual MemberContact ContactGet(ContactSearchType searchType, string searchValue, Statistics stat)
        {
            MemberContact contact = BOLoyConnection.ContactGet(searchType, searchValue, stat);
            if (contact != null)
                SecurityCardCheck(contact.Cards.FirstOrDefault().Id);
            return contact;
        }

        public virtual MemberContact ContactGetByCardId(string cardId, int numberOfTrans, Statistics stat)
        {
            SecurityCardCheck(cardId);

            if (string.IsNullOrWhiteSpace(cardId))
                throw new LSOmniException(StatusCode.CardIdInvalid, "cardId can not be empty or null");

            MemberContact contact = BOLoyConnection.ContactGet(ContactSearchType.CardId, cardId, stat);
            if (contact == null)
                throw new LSOmniException(StatusCode.ContactIdNotFound, string.Format("Contact with card {0} Not found", cardId));

            if (contact.Blocked)
                return contact;

            contact.LoggedOnToDevice = new Device();
            contact.LoggedOnToDevice.SecurityToken = config.SecurityToken;

            if (contact.Account.PointBalance == 0)
                contact.Account.PointBalance = BOLoyConnection.MemberCardGetPoints(cardId, stat);

            contact.Profiles = BOLoyConnection.ProfileGetByCardId(cardId, stat);
            contact.PublishedOffers = BOLoyConnection.PublishedOffersGet(cardId, string.Empty, string.Empty, stat);
            contact.SalesEntries = BOLoyConnection.SalesEntriesGetByCardId(cardId, string.Empty, DateTime.MinValue, false, numberOfTrans, stat);

            PushNotificationBLL notificationBLL = new PushNotificationBLL(config, timeoutInSeconds);
            contact.Notifications = notificationBLL.NotificationsGetByCardId(cardId, 5000, stat);

            OneListBLL oneListBLL = new OneListBLL(config, timeoutInSeconds);
            contact.OneLists = oneListBLL.OneListGet(contact, true, stat);
            return contact;
        }

        public virtual List<Customer> CustomerSearch(CustomerSearchType searchType, string search, int maxNumberOfRowsReturned, Statistics stat)
        {
            return base.BOLoyConnection.CustomerSearch(searchType, search, maxNumberOfRowsReturned, stat);
        }

        public virtual List<Profile> ProfilesGetAll(Statistics stat)
        {
            return BOLoyConnection.ProfileGetAll(stat);
        }

        public virtual List<Profile> ProfilesGetByCardId(string cardId, Statistics stat)
        {
            SecurityCardCheck(cardId);
            return BOLoyConnection.ProfileGetByCardId(cardId, stat);
        }

        public virtual MemberContact ContactCreate(MemberContact contact, bool doLogin, Statistics stat)
        {
            //minor validation before going further 
            if (contact == null)
                throw new LSOmniException(StatusCode.ObjectMissing, "Null Object");

            if (string.IsNullOrEmpty(contact.UserName) == false)
            {
                if (Validation.IsValidUserName(contact.UserName) == false)
                    throw new LSOmniException(StatusCode.UserNameInvalid, "Validation of user name failed");
                contact.UserName = contact.UserName.Trim();
                if (contact.Password != null)
                    contact.Password = contact.Password.Trim();
            }

            if (string.IsNullOrEmpty(contact.Password) == false && Validation.IsValidPassword(contact.Password) == false)
                throw new LSOmniException(StatusCode.PasswordInvalid, "Validation of password failed");

            if (Validation.IsValidEmail(contact.Email) == false)
                throw new LSOmniException(StatusCode.EmailInvalid, "Validation of email failed");

            //check if user exist before calling NAV
            if (config.SettingsBoolGetByKey(ConfigKey.Allow_Dublicate_Email) == false && BOLoyConnection.ContactGet(ContactSearchType.Email, contact.Email, stat) != null)
                throw new LSOmniServiceException(StatusCode.EmailExists, "Email already exists: " + contact.UserName);

            if (string.IsNullOrEmpty(contact.AuthenticationId) == false)
            {
                contact.AuthenticationId = contact.AuthenticationId.Trim();
                contact.Authenticator = contact.Authenticator.Trim();
            }

            //Web pages do not need to fill in a Card
            // the deviceId will be set to the UserName.
            if (contact.Cards == null)
                contact.Cards = new List<Card>();

            if (contact.LoggedOnToDevice == null)
                contact.LoggedOnToDevice = new Device();
            if (string.IsNullOrWhiteSpace(contact.LoggedOnToDevice.Id))
                contact.LoggedOnToDevice.Id = GetDefaultDeviceId(contact.UserName);
            if (string.IsNullOrWhiteSpace(contact.LoggedOnToDevice.DeviceFriendlyName))
                contact.LoggedOnToDevice.DeviceFriendlyName = "Web application";

            if (contact.Profiles == null)
                contact.Profiles = new List<Profile>();

            MemberContact newContact = BOLoyConnection.ContactCreate(contact, stat);
            if (doLogin)
            {
                if (string.IsNullOrWhiteSpace(contact.Authenticator))
                    newContact = Login(contact.UserName, contact.Password, true, contact.LoggedOnToDevice.Id, stat);
                else
                    newContact = SocialLogon(contact.Authenticator, contact.AuthenticationId, contact.LoggedOnToDevice.Id, string.Empty, true, stat);
            }
            base.SecurityCheck();
            return newContact;
        }

        public virtual double ContactAddCard(string contactId, string cardId, string accountId, Statistics stat)
        {
            SecurityCardCheck(cardId);
            return BOLoyConnection.ContactAddCard(contactId, accountId, cardId, stat);
        }

        public virtual void ContactBlock(string accountId, string cardId, Statistics stat)
        {
            SecurityCardCheck(cardId);
            OneListBLL oBll = new OneListBLL(config, timeoutInSeconds);
            oBll.OneListDeleteByCardId(cardId, stat);

            BOLoyConnection.ConatctBlock(accountId, cardId, stat);
        }

        public virtual MemberContact ContactUpdate(MemberContact contact, bool getContact, Statistics stat)
        {
            if (contact == null)
                throw new LSOmniException(StatusCode.ContactIdNotFound, "Contact object can not be empty");

            if (string.IsNullOrWhiteSpace(contact.Id))
                throw new LSOmniException(StatusCode.ContactIdNotFound, "ContactId can not be empty");

            if (string.IsNullOrWhiteSpace(contact.UserName))
                throw new LSOmniException(StatusCode.UserNameInvalid, "User name is missing");
            if (string.IsNullOrWhiteSpace(contact.Email))
                throw new LSOmniException(StatusCode.EmailMissing, "Email is missing");
            if (Validation.IsValidEmail(contact.Email) == false)
                throw new LSOmniServiceException(StatusCode.EmailInvalid, "Validation of email failed");

            if (string.IsNullOrWhiteSpace(contact.Cards.FirstOrDefault()?.Id))
                throw new LSOmniServiceException(StatusCode.MemberCardNotFound, "Card ID is missing");

            SecurityCardCheck(contact.Cards.FirstOrDefault().Id);

            //get existing contact in db to compare the email and get accountId
            MemberContact ct = BOLoyConnection.ContactGet(ContactSearchType.CardId, contact.Cards.FirstOrDefault().Id, stat);
            if (ct == null)
                throw new LSOmniServiceException(StatusCode.ContactIdNotFound, "Contact not found with CardId: " + contact.Cards.FirstOrDefault().Id);

            if (config.SettingsBoolGetByKey(ConfigKey.Allow_Dublicate_Email) == false && (ct.Email.Trim().ToLower() != contact.Email.Trim().ToLower()))
            {
                //if the email has changed, check if the new one exists in db
                if (BOLoyConnection.ContactGet(ContactSearchType.Email, contact.Email, stat) != null)
                    throw new LSOmniServiceException(StatusCode.EmailExists, string.Format("Email {0} already exists", contact.Email));
            }

            BOLoyConnection.ContactUpdate(contact, ct.Account.Id, stat);
            if (getContact)
                return BOLoyConnection.ContactGet(ContactSearchType.CardId, ct.Cards.FirstOrDefault().Id, stat);

            return new MemberContact();
        }

        public virtual MemberContact Login(string userName, string password, bool includeDetails, string deviceId, Statistics stat)
        {
            //some validation
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                throw new LSOmniException(StatusCode.UserNamePasswordInvalid, "User name or password are missing");

            // when deviceId is missing, it defaults to the userName.
            if (string.IsNullOrWhiteSpace(deviceId))
                deviceId = GetDefaultDeviceId(userName.Trim());//logging in via web app

            Device dev = BOLoyConnection.DeviceGetById(deviceId, stat);
            if (dev != null && dev.Status == 3)
                throw new LSOmniServiceException(StatusCode.DeviceIsBlocked, string.Format("Device has been blocked from usage: {0}", deviceId));

            MemberContact contact = BOLoyConnection.Login(userName, password, deviceId, (dev == null) ? string.Empty : dev.DeviceFriendlyName, stat);
            if (contact == null)
            {
                contact = BOLoyConnection.ContactGet(ContactSearchType.UserName, userName, stat);
                if (contact == null)
                    throw new LSOmniServiceException(StatusCode.UserNameNotFound, "Cannot login user " + userName);
            }

            if (contact.Blocked)
                return contact;

            if (contact.Cards.Count == 0)
                throw new LSOmniServiceException(StatusCode.MemberCardNotFound, "cardId not found during login for user: " + userName);

            if (includeDetails)
            {
                contact.PublishedOffers = BOLoyConnection.PublishedOffersGet(contact.Cards.FirstOrDefault().Id, string.Empty, string.Empty, stat);

                PushNotificationBLL notificationBLL = new PushNotificationBLL(config, timeoutInSeconds);
                contact.Notifications = notificationBLL.NotificationsGetByCardId(contact.Cards.FirstOrDefault().Id, 5000, stat);

                OneListBLL oneListBLL = new OneListBLL(config, timeoutInSeconds);
                contact.OneLists = oneListBLL.OneListGet(contact, true, stat);
            }

            contact.Environment = new OmniEnvironment();
            CurrencyBLL curBLL = new CurrencyBLL(config, timeoutInSeconds);
            contact.Environment.Currency = curBLL.CurrencyGetLocal(stat);// CurrencyGetHelper.CurrencyGet();
            contact.Environment.PasswordPolicy = config.SettingsGetByKey(ConfigKey.Password_Policy);
            contact.Environment.Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

            string securityToken = Security.CreateSecurityToken();
            base.config.SecurityToken = securityToken;
            this.iDeviceRepository.DeviceSave(deviceId, contact.Cards.FirstOrDefault().Id, securityToken);

            if (dev == null)
            {
                dev = new Device();
            }
            dev.SecurityToken = securityToken;
            contact.LoggedOnToDevice = dev;

            return contact;
        }

        public virtual MemberContact SocialLogon(string authenticator, string authenticationId, string deviceId, string deviceName, bool includeDetails, Statistics stat)
        {
            //some validation
            if (string.IsNullOrWhiteSpace(authenticator) || string.IsNullOrWhiteSpace(authenticationId))
                throw new LSOmniException(StatusCode.UserNamePasswordInvalid, "User authenticator or authenticationId are missing");

            // when deviceId is missing, it defaults to the userName.
            if (string.IsNullOrWhiteSpace(deviceId))
                deviceId = GetDefaultDeviceId(authenticator.Trim());//logging in via web app

            Device dev = BOLoyConnection.DeviceGetById(deviceId, stat);
            if (dev != null && dev.Status == 3)
                throw new LSOmniServiceException(StatusCode.DeviceIsBlocked, string.Format("Device has been blocked from usage: {0}", deviceId));

            MemberContact contact = BOLoyConnection.SocialLogon(authenticator, authenticationId, deviceId, (dev == null) ? string.Empty : dev.DeviceFriendlyName, stat);
            if (contact == null)
                throw new LSOmniServiceException(StatusCode.UserNameNotFound, "Cannot login user " + authenticator);

            if (contact.Cards.Count == 0)
                throw new LSOmniServiceException(StatusCode.MemberCardNotFound, "cardId not found during login for user: " + authenticator);

            if (includeDetails)
            {
                contact.PublishedOffers = BOLoyConnection.PublishedOffersGet(contact.Cards.FirstOrDefault().Id, string.Empty, string.Empty, stat);

                PushNotificationBLL notificationBLL = new PushNotificationBLL(config, timeoutInSeconds);
                contact.Notifications = notificationBLL.NotificationsGetByCardId(contact.Cards.FirstOrDefault().Id, 5000, stat);

                OneListBLL oneListBLL = new OneListBLL(config, timeoutInSeconds);
                contact.OneLists = oneListBLL.OneListGet(contact, true, stat);
            }

            contact.Environment = new OmniEnvironment();
            CurrencyBLL curBLL = new CurrencyBLL(config, timeoutInSeconds);
            contact.Environment.Currency = curBLL.CurrencyGetLocal(stat);// CurrencyGetHelper.CurrencyGet();
            contact.Environment.PasswordPolicy = config.SettingsGetByKey(ConfigKey.Password_Policy);
            contact.Environment.Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

            string securityToken = Security.CreateSecurityToken();
            base.config.SecurityToken = securityToken;
            this.iDeviceRepository.DeviceSave(deviceId, contact.Cards.FirstOrDefault().Id, securityToken);

            if (dev == null)
                dev = new Device();

            dev.SecurityToken = securityToken;
            contact.LoggedOnToDevice = dev;
            return contact;
        }

        public virtual void ChangePassword(string userName, string newPassword, string oldPassword, Statistics stat)
        {
            //minor validation before sending it to NAV web service
            if (Validation.IsValidPassword(newPassword) == false)
                throw new LSOmniException(StatusCode.PasswordInvalid, "Validation of new password failed");
            if (Validation.IsValidPassword(oldPassword) == false)
                throw new LSOmniException(StatusCode.PasswordOldInvalid, "Validation of old password failed");

            //CALL NAV web service - it validates the if old pwd is ok or not
            bool oldMethod = false;
            BOLoyConnection.ChangePassword(userName, string.Empty, newPassword, oldPassword, ref oldMethod, stat);
        }

        //if anything fails in ResetPassword, simply ask the user to go through the ForgotPassword again..
        //StatusCode.PasswordInvalid   ask user for better pwd
        //StatusCode.ParameterInvalid, ask user for correct username since it does not match resetCode
        //  all other errors should as the user to go through the ForgotPassword flow again
        public virtual void ResetPassword(string userNameOrEmail, string resetCode, string newPassword, Statistics stat)
        {
            //minor validation before sending it to NAV web service
            if (Validation.IsValidPassword(newPassword) == false)
                throw new LSOmniException(StatusCode.PasswordInvalid, "Validation of new password failed");

            //get the username from resetCode, if resetCode is not specified
            MemberContact contact = BOLoyConnection.ContactGet(ContactSearchType.UserName, userNameOrEmail, stat);
            if (contact == null)
                contact = BOLoyConnection.ContactGet(ContactSearchType.Email, userNameOrEmail, stat);

            if (contact == null)
                throw new LSOmniServiceException(StatusCode.UserNameNotFound, "userNameOrEmail not found: " + userNameOrEmail);

            bool resetCodeIsEncrypted = config.SettingsBoolGetByKey(ConfigKey.forgotpassword_code_encrypted);
            try
            {
                if (resetCodeIsEncrypted)
                {
                    resetCode = StringCipher.Decrypt(resetCode, "System.ServiceHost");
                }
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex, "resetCode invalid, decryption failed: {0}", resetCode);
                throw new LSOmniServiceException(StatusCode.ResetPasswordCodeInvalid, "reset code could not be decrypted");
            }

            IResetPasswordRepository iResetPasswordRepository = GetDbRepository<IResetPasswordRepository>(config);
            if (iResetPasswordRepository.ResetPasswordExists(resetCode, contact.Id) == false)
                throw new LSOmniServiceException(StatusCode.ResetPasswordCodeNotFound, "Parameters userName and resetCode do not match those in system");

            DateTime dtCreated = iResetPasswordRepository.ResetPasswordGetDateById(resetCode);
            //if more than x hours were since reset pwd was generated, then throw an error
            int hoursBeforeExpire = 1;
            if (DateTime.Now > dtCreated.AddHours(hoursBeforeExpire))
                throw new LSOmniServiceException(StatusCode.ResetPasswordCodeExpired, "Reset code has expired for reset password");

            //CALL NAV web service 
            bool oldMethod = true;
            BOLoyConnection.ResetPassword(userNameOrEmail, string.Empty, newPassword, ref oldMethod, stat);
            iResetPasswordRepository.ResetPasswordDelete(resetCode);
        }

        public virtual string ForgotPassword(string userNameOrEmail, Statistics stat)
        {
            //get the contactId from either UserName of Email addresses
            MemberContact contact = BOLoyConnection.ContactGet(ContactSearchType.UserName, userNameOrEmail, stat);
            if (contact == null)
                contact = BOLoyConnection.ContactGet(ContactSearchType.Email, userNameOrEmail, stat);

            if (contact == null)
                throw new LSOmniServiceException(StatusCode.UserNameNotFound, "userNameOrEmail not found: " + userNameOrEmail);

            string resetCode = GuidHelper.NewGuidString();

            resetCode = resetCode.Replace("-", "");
            bool resetCodeIsEncrypted = config.SettingsBoolGetByKey(ConfigKey.forgotpassword_code_encrypted);
            try
            {
                if (resetCodeIsEncrypted)
                {
                    resetCode = StringCipher.Encrypt(resetCode, "System.ServiceHost");
                }
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex, "resetCode invalid, encryption failed: {0}", resetCode);
                throw new LSOmniServiceException(StatusCode.ResetPasswordCodeInvalid, "reset code could not be encrypted");
            }

            //create resetCode in db
            IResetPasswordRepository iResetPasswordRepository = GetDbRepository<IResetPasswordRepository>(config);
            iResetPasswordRepository.ResetPasswordSave(resetCode, contact.Id, contact.Email);

            return resetCode;
        }

        public virtual string PasswordReset(string userName, string email, Statistics stat)
        {
            bool oldMethod = false;
            string token = BOLoyConnection.ResetPassword(userName, email, string.Empty, ref oldMethod, stat);

            if (config.SettingsBoolGetByKey(ConfigKey.forgotpassword_code_encrypted))
            {
                try
                {
                    token = StringCipher.Encrypt(token, "System.ServiceHost");
                }
                catch (Exception ex)
                {
                    logger.Error(config.LSKey.Key, ex, "token {0} invalid, encryption failed", token);
                    throw new LSOmniServiceException(StatusCode.ResetPasswordCodeInvalid, "token could not be encrypted");
                }
            }

            if (oldMethod)
            {
                MemberContact contact = BOLoyConnection.ContactGet(ContactSearchType.UserName, userName, stat);
                if (contact == null)
                    contact = BOLoyConnection.ContactGet(ContactSearchType.Email, email, stat);

                if (contact == null)
                    throw new LSOmniServiceException(StatusCode.UserNameNotFound, string.Format("User {0} or Email {1} not found", userName, email));

                //create resetCode in db
                IResetPasswordRepository iResetPasswordRepository = GetDbRepository<IResetPasswordRepository>(config);
                iResetPasswordRepository.ResetPasswordSave(token, contact.Id, contact.Email);
            }

            return token;
        }

        public virtual void PasswordChange(string userName, string token, string newPassword, string oldPassword, Statistics stat)
        {
            //minor validation before sending it to NAV web service
            if (Validation.IsValidPassword(newPassword) == false)
                throw new LSOmniException(StatusCode.PasswordInvalid, "Validation of new password failed");

            if (string.IsNullOrEmpty(token) == false && config.SettingsBoolGetByKey(ConfigKey.forgotpassword_code_encrypted))
            {
                try
                {
                    token = StringCipher.Decrypt(token, "System.ServiceHost");
                }
                catch (Exception ex)
                {
                    logger.Error(config.LSKey.Key, ex, "token {0} invalid, decryption failed", token);
                    throw new LSOmniServiceException(StatusCode.ResetPasswordCodeInvalid, "token could not be decrypted");
                }
            }

            bool oldMethod = false;
            BOLoyConnection.ChangePassword(userName, token, newPassword, oldPassword, ref oldMethod, stat);
            if (oldMethod)
            {
                ResetPassword(userName, token, newPassword, stat);
            }
        }

        public virtual string SPGPassword(string email, string token, string newPassword, Statistics stat)
        {
            return BOLoyConnection.SPGPassword(email, token, newPassword, stat);
        }

        public virtual void LoginChange(string oldUserName, string newUserName, string password, Statistics stat)
        {
            BOLoyConnection.LoginChange(oldUserName, newUserName, password, stat);
        }

        #endregion login and accounts

        #region private

        public virtual List<Scheme> SchemesGetAll(Statistics stat)
        {
            return BOLoyConnection.SchemeGetAll(stat);
        }

        private string GetDefaultDeviceId(string userName)
        {
            return ("WEB-" + userName);
        }

        #endregion private
    }
}
