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
        private static LSLogger logger = new LSLogger();
        private IDeviceRepository iDeviceRepository;

        public ContactBLL(BOConfiguration config, int timeoutInSeconds) : base(config, timeoutInSeconds)
        {
            iDeviceRepository = GetDbRepository<IDeviceRepository>(config);
        }

        #region login and contacts
        public virtual void DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model)
        {
            //TODO: Implement?
        }

        public virtual decimal CardGetPointBalance(string cardId)
        {
            return BOLoyConnection.MemberCardGetPoints(cardId);
        }

        public virtual List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned, bool exact)
        {
            return BOLoyConnection.ContactSearch(searchType, search, maxNumberOfRowsReturned, exact);
        }

        public virtual MemberContact ContactGet(ContactSearchType searchType, string searchValue)
        {
            return BOLoyConnection.ContactGet(searchType, searchValue);
        }

        public virtual MemberContact ContactGetByCardId(string cardId, bool includeDetails)
        {
            if(string.IsNullOrWhiteSpace(cardId))
                throw new LSOmniException(StatusCode.CardIdInvalid, "cardId can not be empty or null");

            MemberContact contact = BOLoyConnection.ContactGetByCardId(cardId, 0, includeDetails);
            if (contact == null)
                throw new LSOmniException(StatusCode.ContactIdNotFound, string.Format("Contact with card {0} Not found.", cardId));

            contact.LoggedOnToDevice = new Device();
            contact.LoggedOnToDevice.SecurityToken = config.SecurityToken;

            if (includeDetails)
            {
                NotificationBLL notificationBLL = new NotificationBLL(config, timeoutInSeconds);
                contact.Notifications = notificationBLL.NotificationsGetByCardId(cardId, 5000);

                OneListBLL oneListBLL = new OneListBLL(config, timeoutInSeconds);
                contact.OneLists = oneListBLL.OneListGet(contact, true);
            }
            return contact;
        }

        public virtual List<Profile> ProfilesGetAll()
        {
            return BOLoyConnection.ProfileGetAll();
        }

        public virtual List<Profile> ProfilesGetByCardId(string cardId)
        {
            return BOLoyConnection.ProfileGetByCardId(cardId);
        }

        public virtual MemberContact ContactCreate(MemberContact contact)
        {
            //minor validation before going further 
            if (contact == null || string.IsNullOrWhiteSpace(contact.UserName) || string.IsNullOrWhiteSpace(contact.Password))
            {
                throw new LSOmniException(StatusCode.UserNamePasswordInvalid, "User name or password are missing.");
            }

            if (Validation.IsValidUserName(contact.UserName) == false)
            {
                throw new LSOmniException(StatusCode.UserNameInvalid, "Validation of user name failed.");
            }
            if (Validation.IsValidPassword(contact.Password) == false)
            {
                throw new LSOmniException(StatusCode.PasswordInvalid, "Validation of password failed.");
            }
            if (Validation.IsValidEmail(contact.Email) == false)
            {
                throw new LSOmniException(StatusCode.EmailInvalid, "Validation of email failed.");
            }

            //check if user exist before calling NAV
            if (BOLoyConnection.ContactGetByUserName(contact.UserName, false) != null)
            {
                throw new LSOmniServiceException(StatusCode.UserNameExists, "User name already exists: " + contact.UserName);
            }
            if (config.SettingsBoolGetByKey(ConfigKey.Allow_Dublicate_Email) == false && BOLoyConnection.ContactGet(ContactSearchType.Email, contact.Email) != null)
            {
                throw new LSOmniServiceException(StatusCode.EmailExists, "Email already exists: " + contact.UserName);
            }

            contact.UserName = contact.UserName.Trim();
            contact.Password = contact.Password.Trim();

            //Web pages do not need to fill in a Card
            // the deviceId will be set to the UserName.
            if (contact.Cards == null)
            {
                contact.Cards = new List<Card>();
            }
            if (contact.LoggedOnToDevice == null)
            {
                contact.LoggedOnToDevice = new Device();
            }
            if (string.IsNullOrWhiteSpace(contact.LoggedOnToDevice.Id))
            {
                contact.LoggedOnToDevice.Id = GetDefaultDeviceId(contact.UserName);
                if (string.IsNullOrWhiteSpace(contact.LoggedOnToDevice.DeviceFriendlyName))
                    contact.LoggedOnToDevice.DeviceFriendlyName = "Web application";
            }

            BOLoyConnection.ContactCreate(contact);
            MemberContact newcontact = Login(contact.UserName, contact.Password, true, contact.LoggedOnToDevice.Id); //this logs any existing user off the device in current user in
            base.SecurityCheck();
            return newcontact;
        }

        public virtual double ContactAddCard(string contactId, string cardId, string accountId)
        {
            return BOLoyConnection.ContactAddCard(contactId, accountId, cardId);
        }

        public virtual MemberContact ContactUpdate(MemberContact contact)
        {
            if (contact == null)
            {
                throw new LSOmniException(StatusCode.ContactIdNotFound, "Contact object can not be empty");
            }
            if (string.IsNullOrWhiteSpace(contact.Id))
            {
                throw new LSOmniException(StatusCode.ContactIdNotFound, "ContactId can not be empty");
            }
            if (string.IsNullOrWhiteSpace(contact.UserName))
            {
                throw new LSOmniException(StatusCode.UserNameInvalid, "User name is missing.");
            }
            if (string.IsNullOrWhiteSpace(contact.Email))
            {
                throw new LSOmniException(StatusCode.EmailMissing, "Email is missing.");
            }

            if (Validation.IsValidEmail(contact.Email) == false)
            {
                throw new LSOmniServiceException(StatusCode.EmailInvalid, "Validation of email failed.");
            }
            if (string.IsNullOrWhiteSpace(contact.Cards.FirstOrDefault()?.Id))
            {
                throw new LSOmniServiceException(StatusCode.MemberCardNotFound, "Card ID is missing.");
            }

            //get existing contact in db to compare the email and get accountId
            MemberContact ct = BOLoyConnection.ContactGetByCardId(contact.Cards.FirstOrDefault().Id, 0, false);
            if (ct == null)
                throw new LSOmniServiceException(StatusCode.ContactIdNotFound, "Contact not found with CardId: " + contact.Cards.FirstOrDefault().Id);

            if (config.SettingsBoolGetByKey(ConfigKey.Allow_Dublicate_Email) == false && (ct.Email.Trim().ToLower() != contact.Email.Trim().ToLower()))
            {
                //if the email has changed, check if the new one exists in db
                if (BOLoyConnection.ContactGet(ContactSearchType.Email, contact.Email) != null)
                {
                    throw new LSOmniServiceException(StatusCode.EmailExists, string.Format("Email {0} already exists.", contact.Email));
                }
            }

            BOLoyConnection.ContactUpdate(contact, ct.Account.Id);
            return BOLoyConnection.ContactGetByCardId(ct.Cards.FirstOrDefault().Id, 0, false);
        }

        public virtual MemberContact Login(string userName, string password, bool includeDetails, string deviceId)
        {
            //some validation
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                throw new LSOmniException(StatusCode.UserNamePasswordInvalid, "User name or password are missing");
            }

            // when deviceId is missing, it defaults to the userName.
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = GetDefaultDeviceId(userName.Trim());//logging in via web app
            }

            Device dev = BOLoyConnection.DeviceGetById(deviceId);
            if (dev != null && dev.Status == 3)
            {
                throw new LSOmniServiceException(StatusCode.DeviceIsBlocked, string.Format("Device has been blocked from usage: {0}", deviceId));
            }

            MemberContact contact = BOLoyConnection.Login(userName, password, deviceId, (dev == null) ? string.Empty : dev.DeviceFriendlyName, includeDetails);
            if (contact == null)
            {
                contact = BOLoyConnection.ContactSearch(ContactSearchType.UserName, userName, 0, true).FirstOrDefault();
                if (contact == null)
                    throw new LSOmniServiceException(StatusCode.UserNameNotFound, "Cannot login user " + userName);
            }

            if (contact.Cards.Count == 0)
            {
                throw new LSOmniServiceException(StatusCode.MemberCardNotFound, "cardId not found during login for user: " + userName);
            }

            if (includeDetails)
            {
                NotificationBLL notificationBLL = new NotificationBLL(config, timeoutInSeconds);
                contact.Notifications = notificationBLL.NotificationsGetByCardId(contact.Cards[0].Id, 5000);

                OneListBLL oneListBLL = new OneListBLL(config, timeoutInSeconds);
                contact.OneLists = oneListBLL.OneListGet(contact, true);
            }

            contact.Environment = new OmniEnvironment();
            CurrencyBLL curBLL = new CurrencyBLL(config, timeoutInSeconds);
            contact.Environment.Currency = curBLL.CurrencyGetLocal();// CurrencyGetHelper.CurrencyGet();
            contact.Environment.PasswordPolicy = config.SettingsGetByKey(ConfigKey.Password_Policy);
            contact.Environment.Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

            string securityToken = Security.CreateSecurityToken();
            base.config.SecurityToken = securityToken;
            this.iDeviceRepository.DeviceSave(deviceId, contact.Id, securityToken);

            Device device = new Device();
            device.SecurityToken = securityToken;
            contact.LoggedOnToDevice = device;

            return contact;
        }

        public virtual void Logout(string userName, string deviceId = "", string ipAddress = "")
        {
            //when deviceId is missing, it defaults to the userName.
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = GetDefaultDeviceId(userName.Trim());//logging in via web app
            }
            iDeviceRepository.Logout(userName, deviceId);
        }

        public virtual void ChangePassword(string userName, string newPassword, string oldPassword)
        {
            //minor validation before sending it to NAV web service
            if (Validation.IsValidPassword(newPassword) == false)
            {
                throw new LSOmniException(StatusCode.PasswordInvalid, "Validation of new password failed.");
            }
            if (Validation.IsValidPassword(oldPassword) == false)
            {
                throw new LSOmniException(StatusCode.PasswordOldInvalid, "Validation of old password failed.");
            }

            //CALL NAV web service - it validates the if old pwd is ok or not
            BOLoyConnection.ChangePassword(userName, string.Empty, newPassword, oldPassword); //

        }

        //if anything fails in resetpwd, simply ask the user to go through the forgotpassword again..
        //StatusCode.PasswordInvalid   ask user for better pwd
        //StatusCode.ParameterInvalid, ask user for correct username since it does not match resetcode
        //  all other errors should as the user to go through the forgotPassword flow again
        public virtual void ResetPassword(string userNameOrEmail, string resetCode, string newPassword)
        {
            //minor validation before sending it to NAV web service
            if (Validation.IsValidPassword(newPassword) == false)
            {
                throw new LSOmniException(StatusCode.PasswordInvalid, "Validation of new password failed.");
            }

            //get the username from resetCode, if resetCode is not specified
            MemberContact contact = BOLoyConnection.ContactGetByUserName(userNameOrEmail, false);
            if (contact == null)
                contact = BOLoyConnection.ContactGet(ContactSearchType.Email, userNameOrEmail);

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
            {
                throw new LSOmniServiceException(StatusCode.ResetPasswordCodeNotFound, "Parameters userName and resetCode do not match those in system.");
            }

            DateTime dtCreated = iResetPasswordRepository.ResetPasswordGetDateById(resetCode);
            //if more than x hours were since reset pwd was generated, then throw an error
            int hoursBeforeExpire = 1;
            if (DateTime.Now > dtCreated.AddHours(hoursBeforeExpire))
            {
                throw new LSOmniServiceException(StatusCode.ResetPasswordCodeExpired, "Reset code has expired for reset password");
            }

            //CALL NAV web service 
            BOLoyConnection.ResetPassword(userNameOrEmail, string.Empty, newPassword); //

            //delete security token and resetpasswordDelete failure is 
            iResetPasswordRepository.ResetPasswordDelete(resetCode);
        }

        public virtual string ForgotPassword(string userNameOrEmail)
        {
            //get the contactId from either UserName of Email addresses
            MemberContact contact = BOLoyConnection.ContactGetByUserName(userNameOrEmail, false);
            if (contact == null)
                contact = BOLoyConnection.ContactGet(ContactSearchType.Email, userNameOrEmail);

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

        public virtual string PasswordReset(string userName, string email)
        {
            string token = BOLoyConnection.ResetPassword(userName, email, string.Empty);

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
            return token;
        }

        public virtual void PasswordChange(string userName, string token, string newPassword, string oldPassword)
        {
            //minor validation before sending it to NAV web service
            if (Validation.IsValidPassword(newPassword) == false)
            {
                throw new LSOmniException(StatusCode.PasswordInvalid, "Validation of new password failed.");
            }

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

            BOLoyConnection.ChangePassword(userName, token, newPassword, oldPassword);
        }

        public virtual void LoginChange(string oldUserName, string newUserName, string password)
        {
            BOLoyConnection.LoginChange(oldUserName, newUserName, password);
        }

        #endregion login and accounts

        #region private

        public virtual List<Scheme> SchemesGetAll()
        {
            return BOLoyConnection.SchemeGetAll();
        }

        private string GetDefaultDeviceId(string userName)
        {
            return ("WEB-" + userName);
        }

        #endregion private
    }
}
