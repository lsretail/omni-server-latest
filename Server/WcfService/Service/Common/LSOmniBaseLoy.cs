using System;
using System.IO;
using System.Collections.Generic;

using LSOmni.BLL;
using LSOmni.BLL.Loyalty;
using LSOmni.Common.Util;
using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;

namespace LSOmni.Service
{
    /// <summary>
    /// Base class for JSON, SOAP client and XML 
    /// </summary>
    public partial class LSOmniBase
    {
        #region Profile

        /// <summary>
        /// Get all user profiles
        /// </summary>
        /// <returns>List of profiles</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// </list>        
        /// </exception>       
        public virtual List<Profile> ProfilesGetAll()
        {
            try
            {
                logger.Debug("ProfileGetAll()");

                ContactBLL profileBLL = new ContactBLL(clientTimeOutInSeconds); //no security token neede
                return profileBLL.ProfilesGetAll();
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to ProfileGetAll()");
                return null; //never gets here
            }
        }

        public virtual List<Profile> ProfilesGetByContactId(string contactId)
        {
            try
            {
                logger.Debug(string.Format("ProfilesGetByContactId() - contactId:{0}", contactId));

                ContactBLL profileBLL = new ContactBLL(securityToken, clientTimeOutInSeconds);
                return profileBLL.ProfilesGetByContactId(contactId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to ProfilesGetByContactId() contactId:{0}", contactId));
                return null; //never gets here
            }
        }

        #endregion Profile

        #region contact and account

        /// <summary>
        /// Get account by account Id
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>Account</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>
        /// </list>        
        /// </exception>          
        public virtual Account AccountGetById(string accountId)
        {
            try
            {
                logger.Debug("accountId:{0}", accountId);

                ContactBLL accountBLL = new ContactBLL(securityToken, clientTimeOutInSeconds);
                return accountBLL.AccountGetById(accountId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to AccountGetById() accountId:{0}", accountId));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get all schemes in system
        /// </summary>
        /// <returns>List of schemes</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// </list>        
        /// </exception> 
        public virtual List<Scheme> SchemesGetAll()
        {
            try
            {
                logger.Debug("SchemeGetAll()");

                ContactBLL accountBLL = new ContactBLL(clientTimeOutInSeconds);
                return accountBLL.SchemesGetAll();
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to SchemeGetAll()");
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get contact by contact Id
        /// </summary>
        /// <param name="contactId">contact Id</param>
        /// <returns>Contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>
        /// </list>        
        /// </exception> 
        public virtual MemberContact ContactGetById(string contactId)
        {
            try
            {
                logger.Debug("contactId:{0}", contactId);

                ContactBLL contactBLL = new ContactBLL(securityToken, clientTimeOutInSeconds);
                MemberContact contact = contactBLL.ContactGetById(contactId);
                contact.Environment.Version = this.Version();
                ContactSetLocation(contact);
                return contact;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to ContactGetById() contactId:{0}", contactId));
                return null; //never gets here
            }
        }

        public virtual List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned)
        {
            try
            {
                logger.Debug("searchType:{0} searchValue:{1}", searchType, search);

                ContactBLL contactBLL = new ContactBLL(securityToken, clientTimeOutInSeconds);
                return contactBLL.ContactSearch(searchType, search, maxNumberOfRowsReturned);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to ContactSearch() searchType:{0} searchValue:{1}", searchType, search));
                return null; //never gets here
            }
        }

        public virtual MemberContact ContactGetByAlternateId(string alternateId)
        {
            try
            {
                logger.Debug("alternateId:{0}", alternateId);

                ContactBLL contactBLL = new ContactBLL(securityToken, clientTimeOutInSeconds);
                MemberContact contact = contactBLL.ContactGetById("", alternateId);
                contact.Environment.Version = this.Version();
                ContactSetLocation(contact);
                return contact;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to ContactGetByAlternateId() alternateId:{0}", alternateId));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Create a new contact
        /// </summary>
        /// <param name="contact">contact</param>
        /// <returns>Contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNamePasswordInvalid</description>
        /// </item>	         
        /// <item>
        /// <description>StatusCode.PasswordInvalid</description>
        /// </item>	   
        /// <item>
        /// <description>StatusCode.EmailInvalid</description>
        /// </item>	
        /// <item>
        /// <description>StatusCode.UserNameInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNameExists</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.MissingLastName</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.MissingFirstName</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AccountNotFound</description>
        /// </item>
        /// </list>        
        /// </exception>   
        public virtual MemberContact ContactCreate(MemberContact contact)
        {
            try
            {
                logger.Debug(LogJson(contact));
                ContactBLL contactBLL = new ContactBLL(clientTimeOutInSeconds);//not using securitytoken here, so no security checks
                MemberContact contactOut = contactBLL.ContactCreate(contact);
                contactOut.Environment.Version = this.Version();
                ContactSetLocation(contactOut);
                return contactOut;
            }
            catch (Exception ex)
            {
                logger.Error(LogJson(contact));
                HandleExceptions(ex, "Failed to ContactCreate().");
                return null; //never gets here
            }
        }

        /// <summary>
        /// Update a contact
        /// </summary>
        /// <returns>Contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>         
        /// <item>
        /// <description>StatusCode.ParameterInvalid</description>
        /// </item>	 
        /// <item>
        /// <description>StatusCode.EmailInvalid</description>
        /// </item>	          
        /// <item>
        /// <description>StatusCode.ContactIdNotFound</description>
        /// </item>	
        /// </list>        
        /// </exception> 
        public virtual MemberContact ContactUpdate(MemberContact contact)
        {
            try
            {
                logger.Debug(LogJson(contact));

                ContactBLL contactBLL = new ContactBLL(securityToken, clientTimeOutInSeconds);
                MemberContact contactOut = contactBLL.ContactUpdate(contact);
                contactOut.Environment = new OmniEnvironment();
                contactOut.Environment.Version = this.Version();
                contactOut.LoggedOnToDevice = contact.LoggedOnToDevice;
                ContactSetLocation(contactOut);
                return contactOut;
            }
            catch (Exception ex)
            {
                logger.Error(LogJson(contact));
                HandleExceptions(ex, "Failed to ContactUpdate().");
                return null; //never gets here
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="password">password</param>
        /// <param name="deviceId">device Id. Should be empty for non device user (web apps)</param>
        /// <returns>Contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNamePasswordInvalid</description>
        /// </item>	         
        /// <item>
        /// <description>StatusCode.AuthFailed</description>
        /// </item>	 
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	
        /// <item>
        /// <description>StatusCode.InvalidPassword</description>
        /// </item>	
        /// <item>
        /// <description>StatusCode.LoginIdNotFound</description>
        /// </item>	         
        /// </list>                           
        /// </exception> 
        public virtual MemberContact Login(string userName, string password, string deviceId)
        {
            try
            {
                logger.Debug("userName:{0} deviceId:{1} ", userName, deviceId);

                //some validation
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                {
                    throw new LSOmniServiceException(StatusCode.UserNamePasswordInvalid, "User name or password are missing.");
                }

                ContactBLL contactBLL = new ContactBLL(clientTimeOutInSeconds); //not using securitytoken here in login, so no security checks
                string contactId = contactBLL.Login(userName, password, deviceId, clientIPAddress);

                ContactBLL contactWithSecurityTokenBLL = new ContactBLL(contactBLL.SecurityToken, clientTimeOutInSeconds);
                MemberContact contact = contactWithSecurityTokenBLL.ContactGetById(contactId);
                contact.Environment.Version = this.Version();
                ContactSetLocation(contact);
                return contact;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to Login() userName: {0}  deviceId:{1}", userName, deviceId));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Login user from web page.
        /// Returns contact but only Contact and Card object have data  
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="password">password</param>
        /// <returns>Contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNamePasswordInvalid</description>
        /// </item>	         
        /// <item>
        /// <description>StatusCode.AuthFailed</description>
        /// </item>	 
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	
        /// <item>
        /// <description>StatusCode.InvalidPassword</description>
        /// </item>	
        /// <item>
        /// <description>StatusCode.LoginIdNotFound</description>
        /// </item>	         
        /// </list>                           
        /// </exception> 
        public virtual MemberContact LoginWeb(string userName, string password)
        {
            //security token is in card
            try
            {
                logger.Debug("userName:{0} ", userName);

                //some validation
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                {
                    throw new LSOmniServiceException(StatusCode.UserNamePasswordInvalid, "User name or password are missing.");
                }

                ContactBLL contactBLL = new ContactBLL(clientTimeOutInSeconds); //not using securitytoken here, so no security checks
                string contactId = contactBLL.Login(userName, password, "", clientIPAddress);
                ContactBLL contactWithSecurityTokenBLL = new ContactBLL(contactBLL.SecurityToken, clientTimeOutInSeconds); //not using securitytoken here in login, so no security checks
                MemberContact contact = contactWithSecurityTokenBLL.ContactGetById(contactId);
                contact.Notifications = new List<Notification>();
                contact.Profiles = new List<Profile>();
                contact.PublishedOffers = new List<PublishedOffer>();
                return contact;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to Login() userName: {0} ", userName));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Log out
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="deviceId">device Id. Should be empty for non device user (web apps)</param>
        /// <returns>true/false</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>	 
        /// <item>
        /// <description>StatusCode.ContactIdNotFound</description>
        /// </item>	         
        /// </list>                           
        /// </exception> 
        public virtual bool Logout(string userName, string deviceId)
        {
            try
            {
                logger.Debug("userName: {0} deviceId: {1}", userName, deviceId);

                ContactBLL contactBLL = new ContactBLL(securityToken, clientTimeOutInSeconds);
                contactBLL.Logout(userName, deviceId, clientIPAddress);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to Logout() userName: {0} deviceId: {1} ", userName, deviceId));
            }
            return false;
        }
        
        public virtual bool UserDelete(string userName)
        {
            try
            {
                logger.Debug("userName: {0}", userName);
                ContactBLL contactBLL = new ContactBLL(securityToken, clientTimeOutInSeconds);
                contactBLL.UserDelete(userName);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to delete user " + userName);
            }
            return false;
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="newPassword">new password</param>
        /// <param name="oldPassword">old password</param>
        /// <returns>true/false</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	  
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>	 
        /// <item>
        /// <description>StatusCode.ContactIdNotFound</description>
        /// </item>	    
        /// <item>
        /// <description>StatusCode.PasswordInvalid</description>
        /// </item>	    
        /// <item>
        /// <description>StatusCode.PasswordOldInvalid</description>
        /// </item>	  
        /// </list>                           
        /// </exception>    
        public virtual bool ChangePassword(string userName, string newPassword, string oldPassword)
        {
            try
            {
                logger.Debug("userName:{0}  ", userName);

                ContactBLL contactBLL = new ContactBLL(securityToken, clientTimeOutInSeconds);
                contactBLL.ChangePassword(userName, newPassword, oldPassword);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to changePassword() userName: {0} ", userName));
            }
            return true;
        }

        public virtual double ContactAddCard(string contactId, string cardId)
        {
            double points = 0;
            try
            {
                logger.Debug("contactid:{0}  , cardid:{1}", contactId, cardId);

                ContactBLL contactBLL = new ContactBLL(securityToken, clientTimeOutInSeconds);
                points = contactBLL.ContactAddCard(contactId, cardId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, String.Format("contactid:{0} , cardid:{1}", contactId, cardId));
            }
            return points;
        }

        public virtual long ContactGetPointBalance(string contactId)
        {
            try
            {
                logger.Debug("contactId:{0}", contactId);

                ContactBLL contactBLL = new ContactBLL(securityToken, clientTimeOutInSeconds);
                return Convert.ToInt64(contactBLL.ContactGetPointBalance(contactId));
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to ContactGetPointBalance() contactId:{0}", contactId));
                return 0;  //never gets here
            }
        }

        public virtual decimal GetPointRate()
        {
            try
            {
                logger.Debug("PointRate");

                CurrencyBLL curBLL = new CurrencyBLL(securityToken, clientTimeOutInSeconds);
                return curBLL.GetPointRate();
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to GetPointRate()"));
                return 0;  //never gets here
            }
        }

        public virtual bool DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model)
        {
            try
            {
                logger.Debug("deviceid:{0} ", deviceId);

                ContactBLL contactBLL = new ContactBLL(securityToken, clientTimeOutInSeconds);
                contactBLL.DeviceSave(deviceId, deviceFriendlyName, platform, osVersion, manufacturer, model);
            }
            catch (Exception ex)
            {

                HandleExceptions(ex, String.Format("deviceid:{0} ", deviceId));

            }
            return true;
        }

        public virtual bool ResetPassword(string userName, string resetCode, string newPassword)
        {
            try
            {
                logger.Debug("userName:{0}  resetCode:{1}", userName, resetCode);

                ContactBLL contactBLL = new ContactBLL(clientTimeOutInSeconds);
                contactBLL.ResetPassword(userName, resetCode, newPassword);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, String.Format("userName:{0}  resetCode:{1}", userName, resetCode));
            }
            return true;
        }

        public virtual string ForgotPassword(string userNameOrEmail, string emailSubject, string emailBody)
        {
            try
            {
                logger.Debug("userNameOrEmail:{0} emailSubject:{1}", userNameOrEmail, emailSubject);

                ContactBLL contactBLL = new ContactBLL(clientTimeOutInSeconds);
                return contactBLL.ForgotPassword(userNameOrEmail, string.Empty, emailSubject, emailBody);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, String.Format("userNameOrEmail:{0}", userNameOrEmail));
            }
            return string.Empty;
        }

        public virtual bool ForgotPasswordForDevice(string userNameOrEmail)
        {
            try
            {
                logger.Debug("userNameOrEmail:{0}  deviceId:{1}", userNameOrEmail, this.deviceId);

                ContactBLL contactBLL = new ContactBLL(clientTimeOutInSeconds);
                contactBLL.ForgotPasswordForDevice(userNameOrEmail, this.deviceId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, String.Format("userNameOrEmail:{0} deviceId:{1}", userNameOrEmail, this.deviceId));
            }
            return true;
        }

        // offer2.Image = @"R0lGODlhUAAPAKIAAAsLav///88PD9WqsYmApmZmZtZfYmdakyH5BAQUAP8ALAAAAABQAA8AAAPb
        //WLrc/jDKSVe4OOvNu/9gqARDSRBHegyGMahqO4R0bQcjIQ8E4BMCQc930JluyGRmdAAcdiigMLVr
        //ApTYWy5FKM1IQe+Mp+L4rphz+qIOBAUYeCY4p2tGrJZeH9y79mZsawFoaIRxF3JyiYxuHiMGb5KT
        //kpFvZj4ZbYeCiXaOiKBwnxh4fnt9e3ktgZyHhrChinONs3cFAShFF2JhvCZlG5uchYNun5eedRxM
        //AF15XEFRXgZWWdciuM8GCmdSQ84lLQfY5R14wDB5Lyon4ubwS7jx9NcV9/j5+g4JADs=";

        #endregion contact and account

        #region coupon, offer and notification

        public virtual List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems)
        {
            try
            {
                logger.Debug("pubOfferId:{0}", pubOfferId.ToString());

                ItemBLL bll = new ItemBLL(securityToken, clientTimeOutInSeconds);
                List<LoyItem> items = bll.ItemsGetByPublishedOfferId(pubOfferId, numberOfItems);
                foreach (LoyItem item in items)
                {
                    ItemSetLocation(item);
                }
                return items;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to ItemsGetByPublishedOfferId() pubOfferId:{0} ", pubOfferId));
                return null; //never gets here
            }
        }

        public virtual List<PublishedOffer> PublishedOffersGetByCardId(string cardId, string itemId)
        {
            if (cardId == null)
                cardId = string.Empty;
            if (itemId == null)
                itemId = string.Empty;

            try
            {
                logger.Debug("itemId:{0}  cardId:{1} ", itemId, cardId);
                OfferBLL bll = new OfferBLL(securityToken, clientTimeOutInSeconds);
                List<PublishedOffer> list = bll.PublishedOffersGet(cardId, itemId, string.Empty);
                foreach (PublishedOffer it in list)
                {
                    foreach (ImageView iv in it.Images)
                    {
                        iv.Location = GetImageStreamUrl(iv);
                    }
                    foreach (OfferDetails od in it.OfferDetails)
                    {
                        od.Image.Location = GetImageStreamUrl(od.Image);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("itemId:{0}  cardId:{1} ", itemId, cardId));
                return null; //never gets here
            }
        }

        public virtual List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId)
        {
            if (cardId == null)
                cardId = string.Empty;
            if (itemId == null)
                itemId = string.Empty;
            if (storeId == null)
                storeId = string.Empty;

            try
            {
                logger.Debug("itemId:{0} cardId:{1} storeId:{2}", itemId, cardId, storeId);
                OfferBLL bll = new OfferBLL(securityToken, clientTimeOutInSeconds);
                List<PublishedOffer> list = bll.PublishedOffersGet(cardId, itemId, storeId);
                foreach (PublishedOffer it in list)
                {
                    foreach (ImageView iv in it.Images)
                    {
                        iv.Location = GetImageStreamUrl(iv);
                    }
                    foreach (OfferDetails od in it.OfferDetails)
                    {
                        od.Image.Location = GetImageStreamUrl(od.Image);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("itemId:{0} cardId:{1} storeId:{2}", itemId, cardId, storeId));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get notification by notification Id
        /// </summary>
        /// <param name="notificationId">notification Id</param>
        /// <returns>Notification</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// </list>        
        /// </exception>
        public virtual Notification NotificationGetById(string notificationId)
        {
            try
            {
                logger.Debug("notificationId:{0}  ", notificationId);

                NotificationBLL notificationBLL = new NotificationBLL(clientTimeOutInSeconds);
                Notification notification = notificationBLL.NotificationGetById(notificationId);
                NotificationSetLocation(notification);
                return notification;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("notificationId:{0} ", notificationId));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get notification by contact Id
        /// </summary>
        /// <param name="contactId">contact Id</param>
        /// <param name="numberOfNotifications">numberOfNotifications</param>
        /// <returns>List of notifications</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>      
        /// </list>        
        /// </exception>
        public virtual List<Notification> NotificationsGetByContactId(string contactId, int numberOfNotifications)
        {
            try
            {
                logger.Debug("contactId:{0} numberOfNotifications:{1}", contactId, numberOfNotifications);

                NotificationBLL notificationBLL = new NotificationBLL(securityToken, clientTimeOutInSeconds);
                List<Notification> notificationList = notificationBLL.NotificationsGetByContactId(contactId, numberOfNotifications);
                foreach (Notification notification in notificationList)
                {
                    NotificationSetLocation(notification);
                }
                return notificationList;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("contactId:{0} numberOfNotifications:{1}", contactId, numberOfNotifications));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Update the notification status
        /// </summary>
        public virtual bool NotificationsUpdateStatus(string contactId, List<string> notificationIds, NotificationStatus notificationStatus)
        {
            try
            {
                logger.Debug("contactId:{0} ", contactId);

                NotificationBLL notificationBLL = new NotificationBLL(securityToken, clientTimeOutInSeconds);
                notificationBLL.NotificationsUpdateStatus(contactId, notificationIds, notificationStatus);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("contactId:{0} ", contactId));
            }
            return true;
        }

        /// <summary>
        /// Get the number of unread notifications since last checked date
        /// </summary>
        /// <param name="contactId">contact id</param>
        /// <param name="lastChecked">datetime of last notification</param>
        /// <returns></returns>
        public virtual NotificationUnread NotificationCountGetUnread(string contactId, DateTime lastChecked)
        {
            try
            {
                logger.Debug("contactId:{0}   lastChecked:{1} - NO LONGER SUPPORTED, use Pushnotification", contactId, lastChecked.ToString());
                return new NotificationUnread
                {
                    Created = DateTime.Now,
                    Count = 0
                };
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("contactId:{0} lastChecked:{1}", contactId, lastChecked.ToString()));
                return null;
            }

        }

        public virtual List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string loyaltySchemeCode)
        {
            try
            {
                logger.Debug("Store: {0}, ItemIds: {1}, LoyaltySchemeCode: {2}", storeId, string.Join(", ", itemIds), loyaltySchemeCode);
                OfferBLL bll = new OfferBLL(clientTimeOutInSeconds);
                return bll.DiscountsGet(storeId, itemIds, loyaltySchemeCode);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Store: {0}, ItemIds: {1}, LoyaltySchemeCode: {2}", storeId, string.Join(", ", itemIds), loyaltySchemeCode));
                return null; //never gets here
            }
        }

        #endregion coupon, offer and notification

        #region Item, Itemcategory, productgroup

        /// <summary>
        /// Get item by Id
        /// </summary>
        /// <param name="itemId">item id</param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>		
        /// </list>        
        /// </exception>
        public virtual LoyItem ItemGetById(string itemId, string storeId)
        {
            try
            {
                logger.Debug("itemId:{0}", itemId);

                ItemBLL itemBLL = new ItemBLL(clientTimeOutInSeconds);
                LoyItem item = itemBLL.ItemGetById(itemId, storeId);
                ItemSetLocation(item);
                return item;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to ItemGetById() itemId:{0}", itemId));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get item by barcode
        /// </summary>
        /// <param name="barcode">barcode</param>
        /// <param name="storeId"></param>
        /// <returns>Item</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// </list>        
        /// </exception>
        public virtual LoyItem ItemGetByBarcode(string barcode, string storeId)
        {
            try
            {
                logger.Debug("barcode:{0}", barcode);

                ItemBLL itemBLL = new ItemBLL(clientTimeOutInSeconds);
                LoyItem item = itemBLL.ItemGetByBarcode(barcode, storeId);
                ItemSetLocation(item);
                return item;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to ItemGetByBarcode() barcode:{0}", barcode));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get item by search string
        /// </summary>
        /// <param name="search">search string</param>
        /// <param name="maxNumberOfItems">max number of items returned</param>
        /// <param name="includeDetails">includeDetails</param>
        /// <returns>List of items</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// </list>        
        /// </exception>
        public virtual List<LoyItem> ItemsSearch(string search, int maxNumberOfItems, bool includeDetails)
        {
            try
            {
                logger.Debug("search:{0} maxNumberOfItems:{1} includeDetails:{2}", search, maxNumberOfItems, includeDetails);

                ItemBLL itemBLL = new ItemBLL(clientTimeOutInSeconds);
                maxNumberOfItems = (maxNumberOfItems > maxNumberReturned ? maxNumberReturned : maxNumberOfItems); //max 1000 should be the limit!
                List<LoyItem> itemList = itemBLL.ItemsSearch(search, maxNumberOfItems, includeDetails);
                foreach (LoyItem it in itemList)
                {
                    ItemSetLocation(it);
                }
                return itemList;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("search:{0} maxNumberOfItems:{1} includeDetails:{2}",
                    search, maxNumberOfItems, includeDetails));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get the inventory levels for an item in stores close to storeId.  
        /// </summary>
        /// <param name="storeId">id of store</param>
        /// <param name="itemId">id of item</param>
        /// <param name="variantId">Id of variant</param>
        /// <param name="arrivingInStockInDays">Include items arriving in inventory in next X days</param>
        /// <returns></returns>
        public virtual List<InventoryResponse> ItemsInStockGet(string storeId, string itemId, string variantId, int arrivingInStockInDays)
        {
            try
            {
                logger.Debug("storeId: {0} itemId: {1} variantId: {2} arrivingInStockInDays: {3}", storeId, itemId, variantId, arrivingInStockInDays);
                ItemBLL bll = new ItemBLL(clientTimeOutInSeconds);
                return bll.ItemsInStockGet(storeId, itemId, variantId, arrivingInStockInDays);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("storeId: {0} itemId: {1} variantId: {2} arrivingInStockInDays: {3}",
                    storeId, itemId, variantId, arrivingInStockInDays));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get the inventory levels for list of items in store
        /// </summary>
        /// <param name="storeId">id of store</param>
        /// <param name="items">list of items</param>
        /// <returns></returns>
        public virtual List<InventoryResponse> ItemsInStoreGet(List<InventoryRequest> items, string storeId)
        {
            try
            {
                logger.Debug("storeId: {0} item cnt: {1}", storeId, items.Count);
                ItemBLL bll = new ItemBLL(clientTimeOutInSeconds);
                return bll.ItemsInStoreGet(items, storeId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("storeId: {0} item cnt: {1}", storeId, items.Count));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Returns one page of items at a time
        /// </summary>
        /// <param name="pageSize">Number of items you want returned</param>
        /// <param name="pageNumber">Start on page number 1, then increment</param>
        /// <param name="itemCategoryId">ID of item category</param>
        /// <param name="productGroupId">ID of product group</param>
        /// <param name="search">item description</param>
        /// <param name="includeDetails">include Details </param>
        /// <returns></returns>
        public virtual List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, bool includeDetails)
        {
            try
            {
                logger.Debug("pageSize:{0} pageNumber:{1} itemCategoryId:{2} productGroupId:{3} search:{4} includeDetails:{5} ",
                    pageSize, pageNumber, itemCategoryId, productGroupId, search, includeDetails);

                ItemBLL itemBLL = new ItemBLL(clientTimeOutInSeconds);
                List<LoyItem> itemList = itemBLL.ItemsPage(pageSize, pageNumber, itemCategoryId, productGroupId, search, includeDetails);
                foreach (LoyItem it in itemList)
                {
                    ItemSetLocation(it);
                }
                return itemList;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("pageSize:{0} pageNumber:{1} itemCategoryId:{2} productGroupId:{3} search:{4} includeDetails:{5}", pageSize, pageNumber, itemCategoryId, productGroupId, search, includeDetails));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get all item categories
        /// </summary>
        /// <returns>List of item categories</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// </list>        
        /// </exception>
        public virtual List<ItemCategory> ItemCategoriesGetAll()
        {
            try
            {
                logger.Debug("ItemCategoriesGetAll");

                ItemBLL itemBLL = new ItemBLL(clientTimeOutInSeconds);
                List<ItemCategory> categories = itemBLL.ItemCategoriesGetAll();
                foreach (ItemCategory ic in categories)
                {
                    ItemCategorySetLocation(ic);
                }
                return categories;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ItemCategoriesGetAll");
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get item category by Id
        /// </summary>
        /// <param name="itemCategoryId">item category Id</param>
        /// <returns>Item category</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// </list>        
        /// </exception>
        public virtual ItemCategory ItemCategoriesGetById(string itemCategoryId)
        {
            try
            {
                logger.Debug("itemCategoryId:{0}", itemCategoryId);

                ItemBLL itemBLL = new ItemBLL(clientTimeOutInSeconds);
                ItemCategory categories = itemBLL.ItemCategoriesGetById(itemCategoryId);
                ItemCategorySetLocation(categories);
                return categories;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("itemCategoryId:{0}", itemCategoryId));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get product group by Id
        /// </summary>
        /// <param name="productGroupId">product group Id</param>
        /// <param name="includeDetails">include Details</param>
        /// <returns>Product group</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// </list>        
        /// </exception>
        public virtual ProductGroup ProductGroupGetById(string productGroupId, bool includeDetails)
        {
            try
            {
                logger.Debug("productGroupId:{0} includeDetails:{1} ", productGroupId, includeDetails);

                ItemBLL itemBLL = new ItemBLL(clientTimeOutInSeconds);
                ProductGroup pg = itemBLL.ProductGroupGetById(productGroupId, includeDetails);
                ProductGroupSetLocation(pg);
                return pg;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("productGroupId:{0}  includeDetails:{1}", productGroupId, includeDetails));
                return null; //never gets here
            }
        }

        #endregion Item

        #region One list

        public virtual List<OneList> OneListGetByContactId(string contactId, ListType listType, bool includeLines)
        {
            try
            {
                logger.Debug("contactId:{0} , includeLines: {1} , listType: {2}", contactId, includeLines, listType.ToString());

                OneListBLL listBLL = new OneListBLL(securityToken, clientTimeOutInSeconds);
                List<OneList> list = listBLL.OneListGetByContactId(contactId, listType, includeLines);
                foreach (OneList l in list)
                {
                    this.OneListSetLocation(l);
                }
                return list;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("contactId:{0}   includeLines: {1} , listType: {2}", contactId, includeLines, listType.ToString()));
                return null; //never gets here
            }
        }

        public virtual List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines)
        {
            try
            {
                logger.Debug("cardId:{0} , includeLines: {1}, listType: {2}", cardId, includeLines, listType.ToString());

                OneListBLL listBLL = new OneListBLL(securityToken, clientTimeOutInSeconds);
                List<OneList> list = listBLL.OneListGetByCardId(cardId, listType, includeLines);
                foreach (OneList l in list)
                {
                    this.OneListSetLocation(l);
                }
                return list;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("cardId:{0}   includeLines: {1} , listType: {2}", cardId, includeLines, listType.ToString()));
                return null; //never gets here
            }
        }

        public virtual OneList OneListGetById(string oneListId, ListType listType, bool includeLines)
        {
            try
            {
                logger.Debug("oneListId:{0} , includeLines: {1}, listType: {2}", oneListId, includeLines, listType.ToString());

                OneListBLL listBLL = new OneListBLL(securityToken, clientTimeOutInSeconds);
                OneList list = listBLL.OneListGetById(oneListId, listType, includeLines, false);
                OneListSetLocation(list);
                return list;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("oneListId:{0}   includeLines: {1}, listType: {2}", oneListId, includeLines, listType.ToString()));
                return null; //never gets here
            }
        }

        public virtual OneList OneListSave(OneList oneList, bool calculate)
        {
            try
            {
                logger.Debug(LogJson(oneList));
                OneListBLL listBLL = new OneListBLL(securityToken, clientTimeOutInSeconds);
                OneList list = listBLL.OneListSave(oneList, calculate, false);
                OneListSetLocation(list);
                return list;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "");
                return null; //never gets here
            }
        }

        public virtual Order OneListCalculate(OneList oneList)
        {
            try
            {
                logger.Debug(LogJson(oneList));
                OneListBLL listBLL = new OneListBLL(securityToken, clientTimeOutInSeconds);
                return listBLL.OneListCalculate(oneList);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "");
                return null; //never gets here
            }
        }

        public virtual bool OneListDeleteById(string oneListId, ListType listType)
        {
            try
            {
                logger.Debug("oneListId:{0} , listType: {1}", oneListId, listType.ToString());
                OneListBLL listBLL = new OneListBLL(securityToken, clientTimeOutInSeconds);
                listBLL.OneListDeleteById(oneListId, listType);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("oneListId:{0} , listType: {1} ", oneListId, listType.ToString()));
                return false; //never gets here
            }
        }

        #endregion one list

        #region stores

        /// <summary>
        /// Get all stores
        /// </summary>
        /// <returns>List of stores</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// </list>        
        /// </exception>
        public virtual List<Store> StoresGetAll()
        {
            try
            {
                logger.Debug("StoresGetAll");

                StoreBLL storeBLL = new StoreBLL(clientTimeOutInSeconds);
                List<Store> storeList = storeBLL.StoresGetAll(false);
                foreach (Store st in storeList)
                {
                    StoreSetLocation(st);
                }
                return storeList;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("StoresGetAll"));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get store by store Id
        /// </summary>
        /// <param name="storeId">store Id</param>
        /// <returns>Store</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// </list>        
        /// </exception>
        public virtual Store StoreGetById(string storeId)
        {
            try
            {
                logger.Debug("storeId:{0}  ", storeId);

                StoreBLL storeBLL = new StoreBLL(clientTimeOutInSeconds);
                Store store = storeBLL.StoreGetById(storeId);
                StoreSetLocation(store);
                return store;

            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("storeId:{0}  ", storeId));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get stores by coordinates, latitude and longitude
        /// </summary>
        /// <param name="latitude">latitude</param>
        /// <param name="longitude">longitude</param>
        /// <param name="maxDistance">max distance of stores from latitude and longitude in kilometers</param>
        /// <param name="maxNumberOfStores">max number of stores returned</param>
        /// <returns>List of stores within max distance of coords</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// </list>        
        /// </exception>
        public virtual List<Store> StoresGetByCoordinates(double latitude, double longitude, double maxDistance, int maxNumberOfStores)
        {
            try
            {
                logger.Debug("latitude:{0} longitude:{1} maxDistance:{2} maxNumberOfStores:{3}",
                    latitude, longitude, maxDistance, maxNumberOfStores);

                StoreBLL storeBLL = new StoreBLL(clientTimeOutInSeconds);
                maxNumberOfStores = (maxNumberOfStores > maxNumberReturned ? maxNumberReturned : maxNumberOfStores); //max 1000 should be the limit!
                List<Store> storeList = storeBLL.StoresGetByCoordinates(latitude, longitude, maxDistance);
                foreach (Store st in storeList)
                {
                    StoreSetLocation(st);
                }
                return storeList;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("latitude:{0} longitude:{1} maxDistance:{2} maxNumberOfStores:{3}", latitude, longitude, maxDistance, maxNumberOfStores));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get stores that have items in stock
        /// </summary>
        /// <param name="itemId">item Id</param>
        /// <param name="variantId">variant Id</param>
        /// <param name="latitude">latitude</param>
        /// <param name="longitude">longitude</param>
        /// <param name="maxDistance">max distance of stores from latitude and longitude in kilometers</param>
        /// <param name="maxNumberOfStores">max number of stores returned</param>
        /// <returns>List of stores that have items in stock</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>		
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// </list>        
        /// </exception>
        public virtual List<Store> StoresGetbyItemInStock(string itemId, string variantId, double latitude, double longitude, double maxDistance, int maxNumberOfStores)
        {
            try
            {
                logger.Debug("itemId:{0}  variantId:{1} latitude:{2} longitude:{3} maxDistance:{4} maxNumberOfStores:{5} ",
                    itemId, variantId, latitude, longitude, maxDistance, maxNumberOfStores);

                StoreBLL storeBLL = new StoreBLL(clientTimeOutInSeconds);
                List<Store> storeList = storeBLL.StoresGetbyItemInStock(itemId, variantId, latitude, longitude, maxDistance, maxNumberOfStores);
                foreach (Store st in storeList)
                {
                    StoreSetLocation(st);
                }
                return storeList;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("itemId:{0}  variantId:{1} latitude:{2} longitude:{3} maxDistance:{4} maxNumberOfStores:{5}",
                            itemId, variantId, latitude, longitude, maxDistance, maxNumberOfStores));
                return null; //never gets here
            }
        }

        #endregion store location

        #region transactions

        /// <summary>
        /// Get transactions by contact Id
        /// </summary>
        /// <param name="contactId">contact Id</param>
        /// <param name="maxNumberOfTransactions">max number of transactions returned</param>
        /// <returns>List of most recent Transactions for a contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>      
        /// </list>        
        /// </exception>
        public virtual List<LoyTransaction> SalesEntriesGetByContactId(string contactId, int maxNumberOfTransactions)
        {
            try
            {
                logger.Debug("contactId:{0} maxNumberOfTransactions:{1}", contactId, maxNumberOfTransactions);
                TransactionBLL transactionBLL = new TransactionBLL(securityToken, clientTimeOutInSeconds);
                maxNumberOfTransactions = (maxNumberOfTransactions > maxNumberReturned ? maxNumberReturned : maxNumberOfTransactions); //max 1000 should be the limit!
                return transactionBLL.SalesEntriesGetByContactId(contactId, maxNumberOfTransactions);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("contactId:{0} maxNumberOfTransactions:{1} ", contactId, maxNumberOfTransactions));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get transactions that include the items specified in itemSearch criteria
        /// </summary>
        /// <param name="contactId">contact Id</param>
        /// <param name="itemSearch">item description to search for</param>
        /// <param name="maxNumberOfTransactions">max number of transactions returned</param>
        /// <param name="includeLines">Include list of SaleLines and TenderLines, or not</param>
        /// <returns>List of Transactions matching the item search criteria</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>  
        /// </item>	 
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>  
        /// </item>	
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>	 
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>      
        /// </list>        
        /// </exception>
        public virtual List<LoyTransaction> TransactionsSearch(string contactId, string itemSearch, int maxNumberOfTransactions, bool includeLines)
        {
            try
            {
                logger.Debug("contactId:{0} itemSearch:{1} maxNumberOfTransactions:{2}  includeLines:{3}",
                    contactId, itemSearch, maxNumberOfTransactions, includeLines);

                TransactionBLL transactionBLL = new TransactionBLL(securityToken, clientTimeOutInSeconds);
                maxNumberOfTransactions = (maxNumberOfTransactions > maxNumberReturned ? maxNumberReturned : maxNumberOfTransactions); //max 1000 should be the limit!
                return transactionBLL.TransactionsSearch(contactId, itemSearch, maxNumberOfTransactions, includeLines);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("contactId:{0} itemSearch:{1} maxNumberOfTransactions:{2}  includeLines:{3}",
                    contactId, itemSearch, maxNumberOfTransactions, includeLines));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get transaction by Receipt Number
        /// </summary>
        public virtual LoyTransaction TransactionGetByReceiptNo(string receiptNo)
        {
            try
            {
                logger.Debug("receipt:{0}", receiptNo);

                TransactionBLL transactionBLL = new TransactionBLL(securityToken, clientTimeOutInSeconds);
                return transactionBLL.TransactionGetByReceiptNoWithPrint(receiptNo);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("receipt:{0}", receiptNo));
                return null; //never gets here
            }
        }

        public virtual LoyTransaction SalesEntryGetById(string entryId)
        {
            try
            {
                logger.Debug("id:{0}", entryId);
                TransactionBLL transactionBLL = new TransactionBLL(securityToken, clientTimeOutInSeconds);
                return transactionBLL.SalesEntryGetById(entryId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("id:{0}", entryId));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Search for text based on searchType
        /// </summary>
        /// <param name="contactId">contact Id</param>
        /// <param name="search">search string</param>
        /// <param name="searchTypes">enum: All, IOtem, ProductGroup, ItemCategory, OneList, Transaction, Store, Profile, Notification, Offer, Coupon</param>
        /// <returns>SearchRs</returns>
        public virtual SearchRs Search(string contactId, string search, SearchType searchTypes)
        {
            try
            {
                logger.Debug("contactId:{0} search:{1} searchType:{2} ", contactId, search, searchTypes.ToString());

                SearchBLL searchBLL = new SearchBLL(securityToken, clientTimeOutInSeconds);
                SearchRs searchRs = searchBLL.Search(contactId, search, maxNumberReturned, searchTypes);
                SearchSetLocation(searchRs);
                return searchRs;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("contactId:{0} search:{1} searchTypes:{2}", contactId, search, searchTypes.ToString()));
                return null; //never gets here
            }
        }

        #endregion transactions

        #region Basket 

        public virtual BasketCalcResponse BasketCalc(BasketCalcRequest basketRequest)
        {
            try
            {
                logger.Debug(LogJson(basketRequest));
                BasketBLL bll = new BasketBLL(clientTimeOutInSeconds);
                return bll.BasketCalc(basketRequest);
            }
            catch (Exception ex)
            {
                string msg = "Failed to BasketCalc() ";
                if (basketRequest != null)
                {
                    msg += basketRequest.ToString();
                }
                HandleExceptions(ex, msg);
                return null; //never gets here
            }
        }

        public virtual OrderStatusResponse OrderStatusCheck(string transactionId)
        {
            try
            {
                logger.Debug("transactionId:{0}", transactionId);
                BasketBLL bll = new BasketBLL(clientTimeOutInSeconds);
                return bll.OrderStatusCheck(transactionId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("transactionId:{0} ", transactionId));
                return null; //never gets here
            }
        }

        public virtual string OrderCancel(string transactionId)
        {
            try
            {
                logger.Debug("transactionId:{0}", transactionId);
                BasketBLL bll = new BasketBLL(clientTimeOutInSeconds);
                bll.OrderCancel(transactionId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("transactionId:{0} ", transactionId));
            }
            return string.Empty;
        }

        #endregion Basket 

        #region menu & hierarchy & Image

        /// <summary>
        /// To Get images via URL
        /// </summary>
        /// <param name="id"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public virtual Stream ImageStreamGetById(string id, int width, int height)
        {
            try
            {
                logger.Debug("id: {0} width: {1}  height: {1} ", id, width, height);
                ImageSize imgSize = new ImageSize(width, height);
                ImageBLL bll = new ImageBLL();
                ImageView imgView = bll.ImageSizeGetById(id, imgSize);
                if (imgView == null)
                    return null;
                return ImageConverter.StreamFromBase64(imgView.Image);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("id: {0} width: {1}  height: {1} ", id, width, height));
                return null; //never gets here
            }
        }

        public virtual MobileMenu MenusGetAll(string id, string lastVersion)
        {
            try
            {
                logger.Debug("id: {0} lastVersion: {1}   ", id, lastVersion);
                MenuBLL bll = new MenuBLL(clientTimeOutInSeconds);
                MobileMenu mobileMenu = bll.MenusGetAll(id, lastVersion);
                MenuSetLocation(mobileMenu);
                return mobileMenu;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("id: {0} lastVersion: {1}   ", id, lastVersion));
                return null; //never gets here
            }
        }

        public virtual List<Hierarchy> HierarchyGet(string storeId)
        {
            try
            {
                logger.Debug("storeId: {0}", storeId);
                MenuBLL bll = new MenuBLL(clientTimeOutInSeconds);
                return bll.HierarchyGet(storeId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("storeId: {0}", storeId));
                return null; //never gets here
            }
        }

        #endregion menu

        #region Ads

        //id = "LOY",  or  "HOSPLOY"  etc.
        public virtual List<Advertisement> AdvertisementsGetById(string id, string contactId)
        {
            try
            {
                logger.Debug("id: {0} contactId: {1}", id, contactId);

                AdvertisementBLL bll = new AdvertisementBLL(clientTimeOutInSeconds);
                List<Advertisement> ads = bll.AdvertisementsGetById(id, contactId);
                foreach (Advertisement ad in ads)
                {
                    AdvertisementSetLocation(ad);
                }
                return ads;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("id: {0} : {1} ", id, contactId));
                return null; //never gets here
            }
        }

        #endregion Ads

        #region orderqueue

        public virtual OrderQueue OrderQueueSave(OrderQueue order)
        {
            try
            {
                logger.Debug(LogJson(order));
                OrderQueueBLL bll = new OrderQueueBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.Save(order);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("orderId: {0} orderXml: {1} ", order.Id, order.OrderXml));
                return null; //never gets here
            }
        }

        public virtual OrderQueue OrderQueueGetById(string orderId)
        {
            try
            {
                logger.Debug("orderId: {0}   ", orderId);

                OrderQueueBLL bll = new OrderQueueBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.OrderGetById(orderId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("guid: {0}   ", orderId));
                return null; //never gets here
            }
        }

        public virtual bool OrderQueueUpdateStatus(string orderId, OrderQueueStatus status)
        {
            try
            {
                logger.Debug("orderId: {0} status: {1}  ", orderId, status.ToString());

                OrderQueueBLL bll = new OrderQueueBLL(this.deviceId, clientTimeOutInSeconds);
                bll.UpdateStatus(orderId, status);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("orderId: {0} status: {1} ", orderId, status.ToString()));
                return false; //never gets here
            }
        }

        //This does not need to be on the phones
        public virtual List<OrderQueue> OrderQueueSearch(OrderSearchRequest searchRequest)
        {
            try
            {
                logger.Debug(LogJson(searchRequest));
                OrderQueueBLL bll = new OrderQueueBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.OrderSearch(searchRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("searchRequest: {0}", searchRequest.ToString()));
                return null; //never gets here
            }
        }

        #endregion orderqueue

        #region OrderMessage

        public virtual OrderMessage OrderMessageSave(OrderMessage orderMessage)
        {
            try
            {
                logger.Debug(LogJson(orderMessage));
                OrderMessageBLL bll = new OrderMessageBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.OrderMessageSave(orderMessage);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("orderId: {0} Description: {1}  ", orderMessage.Id, orderMessage.Description));
                return null; //never gets here
            }
        }

        public virtual OrderMessage OrderMessageGetById(string id)
        {
            try
            {
                logger.Debug("id: {0} ", id);
                OrderMessageBLL bll = new OrderMessageBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.OrderMessageGetById(id);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("id: {0} ", id));
                return null; //never gets here
            }
        }

        public virtual List<OrderMessage> OrderMessageSearch(OrderMessageSearchRequest searchRequest)
        {
            try
            {
                logger.Debug(LogJson(searchRequest));
                OrderMessageBLL bll = new OrderMessageBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.OrderMessageSearch(searchRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("searchRequest: {0}  ", searchRequest.ToString()));
                return null; //never gets here
            }
        }

        public virtual string OrderMessageRequestPayment(string orderId, OrderMessagePayStatus status, decimal amount, string token)
        {
            try
            {
                logger.Debug("id:{0} status:{1} amount:{2} token:{3}", orderId, status, amount, token);
                OrderMessageBLL bll = new OrderMessageBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.OrderMessageRequestPayment(orderId, status, amount, token);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("id:{0} status:{1} amount:{2}", orderId, status, amount));
                return null; //never gets here
            }
        }

        #endregion OrderMessage

        #region Click and Collect

        public virtual List<OrderLineAvailability> OrderAvailabilityCheck(OrderAvailabilityRequest request)
        {
            try
            {
                logger.Debug(LogJson(request));
                OrderBLL bll = new OrderBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.OrderAvailabilityCheck(request);
            }
            catch (Exception ex)
            {
                logger.Error(LogJson(request));
                HandleExceptions(ex, string.Format("id: {0} ", request.Id));
                return null; //never gets here
            }
        }

        public virtual OrderAvailabilityResponse OrderCheckAvailability(OneList request)
        {
            try
            {
                logger.Debug(LogJson(request));
                OrderBLL bll = new OrderBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.OrderAvailabilityCheck(request);
            }
            catch (Exception ex)
            {
                logger.Error(LogJson(request));
                HandleExceptions(ex, string.Format("id: {0} ", request.Id));
                return null; //never gets here
            }
        }

        public virtual Order OrderCreate(Order request)
        {
            try
            {
                logger.Debug(LogJson(request));
                OrderBLL bll = new OrderBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.OrderCreate(request);
            }
            catch (Exception ex)
            {
                logger.Error(LogJson(request));
                HandleExceptions(ex, string.Format("id: {0} ", request.Id));
                return null; //never gets here
            }
        }

        public virtual List<Order> OrderSearchClickCollect(OrderSearchRequest searchRequest)
        {
            try
            {
                logger.Debug(LogJson(searchRequest));
                OrderQueueBLL bll = new OrderQueueBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.OrderSearchClickCollect(searchRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("searchRequest: {0}", searchRequest.ToString()));
                return null; //never gets here
            }
        }

        public virtual Order OrderGetById(string id, bool includeLines)
        {
            try
            {
                OrderBLL bll = new OrderBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.OrderGetById(id, includeLines);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "OrderGetById");
                return null; //never gets here
            }
        }

        public virtual Order OrderGetByWebId(string id, bool includeLines)
        {
            try
            {
                OrderBLL bll = new OrderBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.OrderGetByWebId(id, includeLines);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "OrderGetByWebId");
                return null; //never gets here
            }
        }

        public virtual Order OrderGetByReceiptId(string id, bool includeLines)
        {
            try
            {
                OrderBLL bll = new OrderBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.OrderGetByReceiptId(id, includeLines);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "OrderGetByReceiptId");
                return null; //never gets here
            }
        }

        public virtual List<Order> OrderHistoryByContactId(string contactId, bool includeLines, bool includeTransactions)
        {
            try
            {
                OrderBLL bll = new OrderBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.OrderHistoryByContactId(contactId, includeLines, includeTransactions);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "GetOrderHistory");
                return null; //never gets here
            }
        }

        #endregion Click and Collect

        #region LS Recommends

        public virtual bool RecommendedActive()
        {
            try
            {
                if (LSRecommendsBLLStatic.IsLSRecommendsAvail())
                {
                    // check configuration
                    LSRecommendsBLL bll = new LSRecommendsBLL(clientTimeOutInSeconds);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false; //never gets here
            }
        }

        public virtual List<RecommendedItem> RecommendedItemsGetByUserId(string userId, List<LoyItem> items, int maxNumberOfItems)
        {
            try
            {
                string itms = "";
                foreach (LoyItem i in items)
                {
                    itms += " " + i.Id;
                }
                logger.Debug("userId:{0} items:{1} maxNumberOfItems:{2}", userId, itms, maxNumberOfItems);

                if (LSRecommendsBLLStatic.IsLSRecommendsAvail() == false)
                {
                    throw new NotImplementedException("LS Recommends is not implemented yet. Missing LS Recommends.dll");
                }

                LSRecommendsBLL bll = new LSRecommendsBLL(clientTimeOutInSeconds);
                return bll.RecommendedItemsGetByUserId(userId, items, maxNumberOfItems);
            }
            catch (Exception ex)
            {
                logger.Error("userId: {0}", userId);
                HandleExceptions(ex, string.Format("userId: {0} ", userId));
                return null; //never gets here
            }
        }

        public virtual List<RecommendedItem> RecommendedItemsGet(string userId, string storeId, string items)
        {
            try
            {
                logger.Debug("userId:{0} storeId:{1} items:{2}", userId, storeId, items);
                if (LSRecommendsBLLStatic.IsLSRecommendsAvail() == false)
                {
                    throw new NotImplementedException("LS Recommends is not implemented yet. Missing LS Recommends.dll");
                }

                LSRecommendsBLL bll = new LSRecommendsBLL(clientTimeOutInSeconds);
                return bll.RecommendedItemsGet(userId, storeId, items);
            }
            catch (Exception ex)
            {
                logger.Error("userId:{0}", userId);
                HandleExceptions(ex, string.Format("userId:{0} storeId:{1}", userId, storeId));
                return null; //never gets here
            }
        }

        public void LSRecommendSetting(string endPointUrl, string accountConnection, string azureAccountKey, string azureName, int numberOfRecommendedItems, bool calculateStock, string wsURI, string wsUserName, string wsPassword, string wsDomain, string storeNo, string location, int minStock)
        {
            try
            {
                logger.Debug("userId:{0} ", wsUserName);
                if (LSRecommendsBLLStatic.IsLSRecommendsAvail() == false)
                {
                    throw new NotImplementedException("LS Recommends is not implemented yet. Missing LS Recommends.dll");
                }

                LSRecommendsBLL bll = new LSRecommendsBLL();
                bll.LSRecommendSetting(endPointUrl, accountConnection, azureAccountKey, azureName, numberOfRecommendedItems, calculateStock, wsURI, wsUserName, wsPassword, wsDomain, storeNo, location, minStock);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "LSRecommend Setting Error");
            }
        }

        #endregion LS Recommends
    }
}  	 
