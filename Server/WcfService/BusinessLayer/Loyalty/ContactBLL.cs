using System;
using System.Linq;
using System.Collections.Generic;
using NLog;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;

namespace LSOmni.BLL.Loyalty
{
    public class ContactBLL : BaseLoyBLL
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IContactRepository iContactRepository;

        public ContactBLL(int timeoutInSeconds) : this("", timeoutInSeconds)
        {
        }

        public ContactBLL(string securityToken, int timeoutInSeconds) : base(securityToken, timeoutInSeconds)
        {
            this.iContactRepository = GetDbRepository<IContactRepository>();
        }

        #region login and contacts

        public virtual decimal ContactGetPointBalance(string id)
        {
            MemberContact contact = ContactGetById(id);
            return contact.Account.PointBalance;
        }

        public virtual List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned)
        {
            return BOLoyConnection.ContactSearch(searchType, search, maxNumberOfRowsReturned);
        }

        public virtual MemberContact ContactGetById(string id, string alternateId = "")
        {
            if (string.IsNullOrWhiteSpace(id) == false && string.IsNullOrWhiteSpace(alternateId) == false)
                ValidateContact(id);

            MemberContact contact = BOLoyConnection.ContactGetById(id, 0);
            if (contact == null)
                throw new LSOmniServiceException(StatusCode.ContactIdNotFound, string.Format("Contact {0} {1} Not found.", id, alternateId));

            try
            {
                //update the Account with new pointbalance
                iContactRepository.UpdateAccountBalance(contact.Account.Id, Convert.ToDouble(contact.Account.PointBalance));
            }
            catch (LSOmniServiceException ex)
            {
                //dont want everything to fail because the points update
                logger.Error(ex.StatusCode.ToString() + " " + ex.Message);
            }

            OfferBLL offerBLL = new OfferBLL(SecurityToken, timeoutInSeconds);
            contact.PublishedOffers = offerBLL.PublishedOffersGetByCardId(contact.Card.Id, string.Empty);

            NotificationBLL notificationBLL = new NotificationBLL(SecurityToken, timeoutInSeconds);
            contact.Notifications = notificationBLL.NotificationsGetByContactId(contact.Id, 5000);

            //return the list
            OneListBLL oneListBLL = new OneListBLL(SecurityToken, timeoutInSeconds);
            contact.Basket = oneListBLL.OneListGetByContactId(contact.Id, ListType.Basket, true).FirstOrDefault();
            contact.WishList = oneListBLL.OneListGetByContactId(contact.Id, ListType.Wish, true).FirstOrDefault();

            contact.Profiles = ProfilesGetByContactId(contact.Id);
            contact.Environment = new OmniEnvironment();

            CurrencyBLL curBLL = new CurrencyBLL(timeoutInSeconds);
            contact.Environment.Currency = curBLL.CurrencyGetLocal();// CurrencyGetHelper.CurrencyGet();

            AppSettingsBLL appBll = new AppSettingsBLL();
            contact.Environment.PasswordPolicy = appBll.AppSettingsGetByKey(AppSettingsKey.Password_Policy);

            Device device = new Device();
            device.SecurityToken = SecurityToken;
            contact.LoggedOnToDevice = device;
            return contact;
        }

        public virtual MemberContact ContactGetByUserName(string userName)
        {
            return BOLoyConnection.ContactGetByUserName(userName);
        }

        public virtual MemberContact ContactGetByEMail(string email)
        {
            return BOLoyConnection.ContactGetByEMail(email);
        }

        public virtual MemberContact ContactGetByCardId(string cardId)
        {
            return iContactRepository.ContactGetByCardId(cardId);
        }

        public virtual List<Profile> ProfilesGetAll()
        {
            return BOLoyConnection.ProfileGetAll();
        }

        public virtual List<Profile> ProfilesGetByContactId(string contactId)
        {
            return BOLoyConnection.ProfileGetByContactId(contactId);
        }

        public virtual MemberContact ContactCreate(MemberContact contact)
        {
            //minor validation before going further 
            if (contact == null || string.IsNullOrWhiteSpace(contact.UserName) || string.IsNullOrWhiteSpace(contact.Password))
            {
                throw new LSOmniServiceException(StatusCode.UserNamePasswordInvalid, "User name or password are missing.");
            }

            if (Validation.IsValidUserName(contact.UserName) == false)
            {
                throw new LSOmniServiceException(StatusCode.UserNameInvalid, "Validation of user name failed.");
            }
            if (Validation.IsValidPassword(contact.Password) == false)
            {
                throw new LSOmniServiceException(StatusCode.PasswordInvalid, "Validation of password failed.");
            }
            if (Validation.IsValidEmail(contact.Email) == false)
            {
                throw new LSOmniServiceException(StatusCode.EmailInvalid, "Validation of email failed.");
            }

            //check if user exist before calling NAV
            if (BOLoyConnection.ContactGetByUserName(contact.UserName) != null)
            {
                throw new LSOmniServiceException(StatusCode.UserNameExists, "User name already exists: " + contact.UserName);
            }
            if (BOLoyConnection.ContactGetByEMail(contact.Email) != null)
            {
                throw new LSOmniServiceException(StatusCode.EmailExists, "Email already exists: " + contact.UserName);
            }

            contact.UserName = contact.UserName.Trim();
            contact.Password = contact.Password.Trim();

            //Web pages do not need to fill in a Card
            // the deviceId will be set to the UserName.
            if (contact.Card == null)
            {
                contact.Card = new Card();
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

            string conId = BOLoyConnection.ContactCreate(contact);
            Login(contact.UserName, contact.Password, contact.LoggedOnToDevice.Id); //this logs any existing user off the device in current user in
            base.SecurityCheck();

            try
            {
                EmailBLL eBLL = new EmailBLL();
                string name = (string.IsNullOrWhiteSpace(contact.FirstName)) ? contact.LastName : contact.FirstName;
                eBLL.EmailRegistration(contact.Email, name, contact.UserName);
            }
            catch (Exception x)
            {
                //ignor the error but write to logfile
                logger.Error(x, "Failed to save to LSOmni Email table");
            }
            return ContactGetById(conId, "");
        }

        public virtual double ContactAddCard(string contactId, string cardId)
        {
            base.ValidateContact(contactId);

            string accountNumber = iContactRepository.ContactGetAccountId(contactId);
            double pointBalance = BOLoyConnection.ContactAddCard(contactId, accountNumber, cardId);
            iContactRepository.UpdateAccountBalance(accountNumber, pointBalance);
            return pointBalance;
        }

        public virtual MemberContact ContactUpdate(MemberContact contact)
        {
            if (contact == null)
            {
                throw new LSOmniServiceException(StatusCode.ContactIdNotFound, "Contact object can not be empty");
            }
            if (string.IsNullOrWhiteSpace(contact.Id))
            {
                throw new LSOmniServiceException(StatusCode.ContactIdNotFound, "ContactId can not be empty");
            }
            if (string.IsNullOrWhiteSpace(contact.UserName))
            {
                throw new LSOmniServiceException(StatusCode.ParameterInvalid, "User name is missing.");
            }
            if (string.IsNullOrWhiteSpace(contact.Email))
            {
                throw new LSOmniServiceException(StatusCode.ParameterInvalid, "EMail is missing.");
            }

            if (Validation.IsValidEmail(contact.Email) == false)
            {
                throw new LSOmniServiceException(StatusCode.EmailInvalid, "Validation of email failed.");
            }

            base.ValidateContact(contact.Id);

            //get existing contact in db to compare the email and get accountId
            MemberContact ct = BOLoyConnection.ContactGetById(contact.Id, 0);
            if (ct.Email.Trim().ToLower() != contact.Email.Trim().ToLower())
            {
                //if the email has changed, check if the new one exists in db
                if (BOLoyConnection.ContactGetByEMail(contact.Email) != null)
                {
                    throw new LSOmniServiceException(StatusCode.EmailExists, string.Format("Email {0} already exists.", contact.Email));
                }
            }

            BOLoyConnection.ContactUpdate(contact, ct.Account.Id);
            string contactId = iContactRepository.ContactUpdate(contact);
            return BOLoyConnection.ContactGetById(contactId, 0);
        }

        public virtual string Login(string userName, string password, string deviceId = "", string ipAddress = "")
        {
            try
            {
                ContactRs contactRs = null;
                string msg = "";
                string cardId = "";

                //some validation
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                {
                    msg = "User name or password are missing.";
                    throw new LSOmniServiceException(StatusCode.UserNamePasswordInvalid, msg);
                }

                // when deviceId is missing, it defaults to the userName.
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    deviceId = GetDefaultDeviceId(userName.Trim());//logging in via web app
                }

                Device dev = BOLoyConnection.DeviceGetById(deviceId);
                if (dev != null && dev.Status == 3)
                {
                    msg = string.Format("Device has been blocked from usage: {0}", deviceId);
                    throw new LSOmniServiceException(StatusCode.DeviceIsBlocked, msg);
                }

                // found locally
                MemberContact contact = BOLoyConnection.ContactGetByUserName(userName);
                if(contact == null)
                    throw new LSOmniServiceException(StatusCode.UserNameNotFound, "userNameOrEmail not found: " + userName);

                String hpassword = Security.NAVHash(password);
                if (contact.Password.ToUpperInvariant() != hpassword.ToUpperInvariant())
                {
                    try
                    {
                        if (iContactRepository.DoesContactExist(userName) == false)
                            iContactRepository.ContactCreate(contact, deviceId);
                        else
                            iContactRepository.ChangePassword(userName, password, "");

                        contactRs = BOLoyConnection.Login(userName, password, ""); //TODO, deviceFriendlyName
                    }
                    catch (LSOmniServiceException ex)
                    {
                        this.LoginLog(userName, deviceId, ipAddress, true);
                        throw new LSOmniServiceException(ex.StatusCode, "Contact created, but " + ex.Message);  //returning text from 
                    }
                }

                if (dev == null)
                {
                    //call WEBService and create the device
                    iContactRepository.DeviceSave(deviceId, "", "", "", "", "");
                }

                if (BOLoyConnection.IsUserLinkedToDeviceId(userName, deviceId, out cardId))
                {
                    cardId = contact.Card.Id;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(cardId) && (contactRs == null))
                    {
                        throw new ApplicationException("cardId not found during login for user: " + userName);
                    }
                    if (string.IsNullOrWhiteSpace(cardId))
                    {
                        if (contactRs == null)
                            cardId = contactRs.CardId;
                    }

                    //CALL NAV web service but dont send in the cardId since that will try and link the card to user 
                    //and we only want to link the device to the user
                    BOLoyConnection.CreateDeviceAndLinkToUser(userName, deviceId, ""); //TODO, deviceFriendlyName
                }

                //finally do the login
                string securityToken = Security.CreateSecurityToken();
                base.securityToken = securityToken;
                string contactId = iContactRepository.Login(contact, deviceId, cardId, securityToken);
                this.LoginLog(userName, deviceId, ipAddress, false);
                return contactId;
            }
            catch (LSOmniServiceException oex)
            {
                if (oex.StatusCode == StatusCode.UserNameNotFound || oex.StatusCode == StatusCode.PasswordInvalid)
                    throw new LSOmniServiceException(StatusCode.AuthFailed, oex.Message, oex);

                throw;
            }
        }

        public virtual void Logout(string userName, string deviceId = "", string ipAddress = "")
        {
            //when deviceId is missing, it defaults to the userName.
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = GetDefaultDeviceId(userName.Trim());//logging in via web app
            }
            base.ValidateContactUserName(userName);
            iContactRepository.Logout(userName, deviceId);
            LoginLog(userName, deviceId, ipAddress, false, true);
        }

        public virtual void UserDelete(string userName)
        {
            iContactRepository.ContactDelete(userName);
        }

        public virtual void ChangePassword(string userName, string newPassword, string oldPassword)
        {
            base.ValidateContactUserName(userName);
            //minor validation before sending it to NAV web service
            if (Validation.IsValidPassword(newPassword) == false)
            {
                throw new LSOmniServiceException(StatusCode.PasswordInvalid, "Validation of new password failed.");
            }
            if (Validation.IsValidPassword(oldPassword) == false)
            {
                throw new LSOmniServiceException(StatusCode.PasswordOldInvalid, "Validation of old password failed.");
            }

            //CALL NAV web service - it validates the if old pwd is ok or not
            BOLoyConnection.ChangePassword(userName, newPassword, oldPassword); //

            //simply update our pwd in our db.  If changePassword fails should Data Director update it later..
            iContactRepository.ChangePassword(userName, newPassword, oldPassword);
            iContactRepository.DeleteSecurityTokens(base.ContactId, base.DeviceId); //remove securitytokens from all but this deviceId
        }

        public virtual void DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model)
        {
            base.ValidateContact(ContactId);
            iContactRepository.DeviceSave(deviceId, deviceFriendlyName, platform, osVersion, manufacturer, model);
        }

        //if anything fails in resetpwd, simply ask the user to go thru the forgotpassword again..
        //StatusCode.PasswordInvalid   ask user for better pwd
        //StatusCode.ParameterInvalid, ask user for correct username since it does not match resetcode
        //  all other errors should as the user to go thru the forgotPassword flow again
        public virtual void ResetPassword(string userNameOrEmail, string resetCode, string newPassword)
        {
            //minor validation before sending it to NAV web service
            if (Validation.IsValidPassword(newPassword) == false)
            {
                throw new LSOmniServiceException(StatusCode.PasswordInvalid, "Validation of new password failed.");
            }

            //get the username from resetCode, if resetCode is not specified
            MemberContact contact = BOLoyConnection.ContactGetByUserName(userNameOrEmail);
            if (contact == null)
                contact = BOLoyConnection.ContactGetByEMail(userNameOrEmail);

            if (contact == null)
                throw new LSOmniServiceException(StatusCode.UserNameNotFound, "userNameOrEmail not found: " + userNameOrEmail);

            AppSettingsBLL appSettings = new AppSettingsBLL();
            bool resetCodeIsEncrypted = appSettings.AppSettingsBoolGetByKey(AppSettingsKey.forgotpassword_code_encrypted, "en");
            try
            {
                if (resetCodeIsEncrypted)
                {
                    resetCode = StringCipher.Decrypt(resetCode, "System.ServiceHost");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "resetCode invalid, decryption failed: {0}", resetCode);
                throw new LSOmniServiceException(StatusCode.ResetPasswordCodeInvalid, "reset code could not be decrypted");
            }

            IResetPasswordRepository iResetPasswordRepository = GetDbRepository<IResetPasswordRepository>();
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
            BOLoyConnection.ResetPassword(userNameOrEmail, newPassword); //

            //simply update our pwd in our db.  If ChangePassword fails should Data Director update it later..
            iContactRepository.ChangePassword(userNameOrEmail, newPassword, "");

            //delete security token and resetpasswordDelete failure is 
            iContactRepository.DeleteSecurityTokens(contact.Id, ""); //remove securitytokens from all devices for this username
            iResetPasswordRepository.ResetPasswordDelete(resetCode);
        }

        public virtual void ForgotPasswordForDevice(string userNameOrEmail, string deviceId)
        {
            //email a reset code. 
            //there are different emails sent for those on devices vs web page
            //TODO, use deviceId and only allow reset code on that phone?

            //get the username from resetCode, if resetCode is not specified
            MemberContact contact = BOLoyConnection.ContactGetByUserName(userNameOrEmail);
            if (contact == null)
                contact = BOLoyConnection.ContactGetByEMail(userNameOrEmail);

            if (contact == null)
                throw new LSOmniServiceException(StatusCode.UserNameNotFound, "userNameOrEmail not found: " + userNameOrEmail);

            //create resetCode in db
            string resetCode = RandomString.GetString(10); //something easier than guid to write on phone

            IResetPasswordRepository iResetPasswordRepository = GetDbRepository<IResetPasswordRepository>();
            iResetPasswordRepository.ResetPasswordSave(resetCode, contact.Id, contact.Email);

            AppSettingsBLL appSettings = new AppSettingsBLL();

            string subject = appSettings.AppSettingsGetByKey(AppSettingsKey.forgotpassword_device_email_subject, "en");
            string body = appSettings.AppSettingsGetByKey(AppSettingsKey.forgotpassword_device_email_body, "en");
            body = body.Replace("[CRLF]", "\r\n");
            body = body.Replace("[RESETCODE]", resetCode);

            // send email to someone with userCode stored in db.
            EmailBLL emailBLL = new EmailBLL(timeoutInSeconds);
            emailBLL.Save(contact.Email, subject, body, EmailType.ResetEmail);
        }

        public virtual string ForgotPassword(string userNameOrEmail, string resetCode, string emailSubject, string emailBody)
        {
            EmailBLL emailBLL = new EmailBLL(timeoutInSeconds);
            AppSettingsBLL appSettings = new AppSettingsBLL();

            //get the contactId from either UserName of Email addresses
            MemberContact contact = BOLoyConnection.ContactGetByUserName(userNameOrEmail);
            if (contact == null)
                contact = BOLoyConnection.ContactGetByEMail(userNameOrEmail);

            if (contact == null)
                throw new LSOmniServiceException(StatusCode.UserNameNotFound, "userNameOrEmail not found: " + userNameOrEmail);

            //decrypt the resetCode
            if (string.IsNullOrEmpty(resetCode))
                resetCode = GuidHelper.NewGuidString();

            resetCode = resetCode.Replace("-", "");
            string encrResetCode = string.Empty;
            bool resetCodeIsEncrypted = appSettings.AppSettingsBoolGetByKey(AppSettingsKey.forgotpassword_code_encrypted, "en");
            try
            {
                if (resetCodeIsEncrypted)
                {
                    encrResetCode = StringCipher.Encrypt(resetCode, "System.ServiceHost");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "resetCode invalid, encryption failed: {0}", resetCode);
                throw new LSOmniServiceException(StatusCode.ResetPasswordCodeInvalid, "reset code could not be encrypted");
            }

            //create resetCode in db
            IResetPasswordRepository iResetPasswordRepository = GetDbRepository<IResetPasswordRepository>();
            iResetPasswordRepository.ResetPasswordSave(resetCode, contact.Id, contact.Email);

            bool sendEmail = appSettings.AppSettingsBoolGetByKey(AppSettingsKey.forgotpassword_omni_sendemail, "en");
            if (sendEmail == false)
                return (resetCodeIsEncrypted) ? encrResetCode : resetCode;

            string url = string.Empty;
            if (string.IsNullOrEmpty(emailSubject))
            {
                //email a reset link  or send code to phone
                emailSubject = appSettings.AppSettingsGetByKey(AppSettingsKey.forgotpassword_email_subject, "en");
                emailBody = appSettings.AppSettingsGetByKey(AppSettingsKey.forgotpassword_email_body, "en");
                url = appSettings.AppSettingsGetByKey(AppSettingsKey.forgotpassword_email_url, "en");
                if (url.EndsWith("?"))
                    url = url.Replace("?", ""); //just in case, remove the ?
                url += "?resetcode=" + resetCode;// + "&n=" + "jjohannsson%40hotmail.com";
                url = Uri.EscapeUriString(url);

                //"https://account.live.com/password/resetconfirm?otc=*Co3oREVxIgCNuj3uszpjZepoSmWTWAMMbaXQpm15YARADQPwgK2OHIoKbP!YcSye*xHD1AsxAIRZXKVarlC9jF8$&mn=jjohannsson%40hotmail.com&cxt=Default");
                emailBody = emailBody.Replace("[URL]", url);
                emailBody = emailBody.Replace("[CRLF]", "\r\n");
            }
            else
            {
                emailBody = emailBody.Replace("[RC]", resetCode);
            }

            // send email to someone with userCode stored in db.
            emailBLL.Save(contact.Email, emailSubject, emailBody, EmailType.ResetEmail);
            return string.Empty;
        }

        #endregion login and accounts

        #region private

        private void LoginLog(string userName, string deviceId, string ipAddress, bool failed, bool logout = false)
        {
            try
            {
                iContactRepository.LoginLog(userName, deviceId, ipAddress, failed, logout);
            }
            catch (Exception)
            {
                //ignore the error for now
            }
        }

        public virtual Account AccountGetById(string id)
        {
            base.ValidateAccount(id);
            return BOLoyConnection.AccountGetById(id);
        }

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
