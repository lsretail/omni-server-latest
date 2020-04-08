using System;
using System.IO;
using System.Collections.Generic;

using LSOmni.BLL;
using LSOmni.BLL.Loyalty;
using LSOmni.Common.Util;
using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
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
                logger.Debug(config.LSKey.Key, "ProfileGetAll()");

                ContactBLL profileBLL = new ContactBLL(config, clientTimeOutInSeconds); //no security token neede
                return profileBLL.ProfilesGetAll();
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed: ProfileGetAll()");
                return null; //never gets here
            }
        }

        public virtual List<Profile> ProfilesGetByCardId(string cardId)
        {
            try
            {
                logger.Debug(config.LSKey.Key, string.Format("ProfilesGetByContactId() - cardId:{0}", cardId));

                ContactBLL profileBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return profileBLL.ProfilesGetByCardId(cardId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed: ProfilesGetByContactId() cardId:{0}", cardId));
                return null; //never gets here
            }
        }

        #endregion Profile

        #region contact and account

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
                logger.Debug(config.LSKey.Key, "SchemeGetAll()");

                ContactBLL accountBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return accountBLL.SchemesGetAll();
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed: SchemeGetAll()");
                return null; //never gets here
            }
        }

        /// <summary>
        /// Get contact by contact Id
        /// </summary>
        /// <param name="cardId">Card Id</param>
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
        public virtual MemberContact ContactGetByCardId(string cardId)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "cardId:{0}", cardId);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                MemberContact contact = contactBLL.ContactGetByCardId(cardId, true);
                contact.Environment.Version = this.Version();
                ContactSetLocation(contact);
                return contact;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed: ContactGetById() cardId:{0}", cardId));
                return null; //never gets here
            }
        }

        public virtual List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "searchType:{0} searchValue:{1}", searchType, search);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return contactBLL.ContactSearch(searchType, search, maxNumberOfRowsReturned, false);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed: ContactSearch() searchType:{0} searchValue:{1}", searchType, search));
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
                if (contact.Cards == null)
                    contact.Cards = new List<Card>();

                logger.Debug(config.LSKey.Key, LogJson(contact));
                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);//not using securitytoken here, so no security checks
                MemberContact contactOut = contactBLL.ContactCreate(contact);
                contactOut.Environment.Version = this.Version();
                ContactSetLocation(contactOut);
                return contactOut;
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, LogJson(contact));
                HandleExceptions(ex, "Failed: ContactCreate().");
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
                if (contact.Cards == null)
                    contact.Cards = new List<Card>();

                logger.Debug(config.LSKey.Key, LogJson(contact));

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                MemberContact contactOut = contactBLL.ContactUpdate(contact);
                contactOut.Environment = new OmniEnvironment();
                contactOut.Environment.Version = this.Version();
                contactOut.LoggedOnToDevice = contact.LoggedOnToDevice;
                ContactSetLocation(contactOut);
                return contactOut;
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, LogJson(contact));
                HandleExceptions(ex, "Failed: ContactUpdate().");
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
                logger.Debug(config.LSKey.Key, "userName:{0} deviceId:{1} ", userName, deviceId);
                config.SecurityCheck = false;
                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds); //not using securitytoken here in login, so no security checks
                MemberContact contact = contactBLL.Login(userName, password, true, deviceId, clientIPAddress);
                contact.Environment.Version = this.Version();
                ContactSetLocation(contact);
                return contact;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed: Login() userName: {0}  deviceId:{1}", userName, deviceId));
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
                logger.Debug(config.LSKey.Key, "userName:{0} ", userName);
                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds); //not using securitytoken here, so no security checks
                return contactBLL.Login(userName, password, false, "", clientIPAddress);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed: Login() userName: {0} ", userName));
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
                logger.Debug(config.LSKey.Key, "userName: {0} deviceId: {1}", userName, deviceId);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                contactBLL.Logout(userName, deviceId, clientIPAddress);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed: Logout() userName: {0} deviceId: {1} ", userName, deviceId));
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
                logger.Debug(config.LSKey.Key, "userName:{0}  ", userName);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                contactBLL.ChangePassword(userName, newPassword, oldPassword);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed: ChangePassword() userName: {0} ", userName));
            }
            return true;
        }

        public virtual double ContactAddCard(string contactId, string cardId, string accountId)
        {
            double points = 0;
            try
            {
                logger.Debug(config.LSKey.Key, "contactid:{0}, cardid:{1}, accountid:{2}", contactId, cardId, accountId);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                points = contactBLL.ContactAddCard(contactId, cardId, accountId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, String.Format("contactid:{0}, cardid:{1}, accountid:{2}", contactId, cardId, accountId));
            }
            return points;
        }

        public virtual long CardGetPointBalance(string cardId)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "cardId:{0}", cardId);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return Convert.ToInt64(contactBLL.CardGetPointBalance(cardId));
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed: ContactGetPointBalance() cardId:{0}", cardId));
                return 0;  //never gets here
            }
        }

        public virtual decimal GetPointRate()
        {
            try
            {
                logger.Debug(config.LSKey.Key, "PointRate");

                CurrencyBLL curBLL = new CurrencyBLL(config, clientTimeOutInSeconds);
                return curBLL.GetPointRate();
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed: GetPointRate()"));
                return 0;  //never gets here
            }
        }

        public virtual bool DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "deviceid:{0} ", deviceId);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
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
                logger.Debug(config.LSKey.Key, "userName:{0}  resetCode:{1}", userName, resetCode);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                contactBLL.ResetPassword(userName, resetCode, newPassword);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, String.Format("userName:{0}  resetCode:{1}", userName, resetCode));
            }
            return true;
        }

        public virtual string ForgotPassword(string userNameOrEmail)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "userNameOrEmail:{0}", userNameOrEmail);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return contactBLL.ForgotPassword(userNameOrEmail);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, String.Format("userNameOrEmail:{0}", userNameOrEmail));
            }
            return string.Empty;
        }

        #endregion contact and account

        #region coupon, offer and notification

        public virtual List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "pubOfferId:{0}", pubOfferId.ToString());

                ItemBLL bll = new ItemBLL(config, clientTimeOutInSeconds);
                List<LoyItem> items = bll.ItemsGetByPublishedOfferId(pubOfferId, numberOfItems);
                foreach (LoyItem item in items)
                {
                    ItemSetLocation(item);
                }
                return items;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed: ItemsGetByPublishedOfferId() pubOfferId:{0} ", pubOfferId));
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
                logger.Debug(config.LSKey.Key, "itemId:{0}  cardId:{1} ", itemId, cardId);
                OfferBLL bll = new OfferBLL(config, clientTimeOutInSeconds);
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
                logger.Debug(config.LSKey.Key, "notificationId:{0}  ", notificationId);

                NotificationBLL notificationBLL = new NotificationBLL(config, clientTimeOutInSeconds);
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
        /// <param name="cardId">Card Id</param>
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
        public virtual List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "cardId:{0} numberOfNotifications:{1}", cardId, numberOfNotifications);

                NotificationBLL notificationBLL = new NotificationBLL(config, clientTimeOutInSeconds);
                List<Notification> notificationList = notificationBLL.NotificationsGetByCardId(cardId, numberOfNotifications);
                foreach (Notification notification in notificationList)
                {
                    NotificationSetLocation(notification);
                }
                return notificationList;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("cardId:{0} numberOfNotifications:{1}", cardId, numberOfNotifications));
                return null; //never gets here
            }
        }

        /// <summary>
        /// Update the notification status
        /// </summary>
        public virtual bool NotificationsUpdateStatus(string cardId, List<string> notificationIds, NotificationStatus notificationStatus)
        {
            try
            {
                //TODO: add cardId functionality
                NotificationBLL notificationBLL = new NotificationBLL(config, clientTimeOutInSeconds);
                notificationBLL.NotificationsUpdateStatus(notificationIds, notificationStatus);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Empty);
            }
            return true;
        }

        public virtual List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string loyaltySchemeCode)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "Store: {0}, ItemIds: {1}, LoyaltySchemeCode: {2}", storeId, string.Join(", ", itemIds), loyaltySchemeCode);
                OfferBLL bll = new OfferBLL(config, clientTimeOutInSeconds);
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
                logger.Debug(config.LSKey.Key, "itemId:{0}", itemId);

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
                LoyItem item = itemBLL.ItemGetById(itemId, storeId);
                ItemSetLocation(item);
                return item;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed: ItemGetById() itemId:{0}", itemId));
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
                logger.Debug(config.LSKey.Key, "barcode:{0}", barcode);

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
                LoyItem item = itemBLL.ItemGetByBarcode(barcode, storeId);
                ItemSetLocation(item);
                return item;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed: ItemGetByBarcode() barcode:{0}", barcode));
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
                logger.Debug(config.LSKey.Key, "search:{0} maxNumberOfItems:{1} includeDetails:{2}", search, maxNumberOfItems, includeDetails);

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
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
                logger.Debug(config.LSKey.Key, "storeId: {0} itemId: {1} variantId: {2} arrivingInStockInDays: {3}", storeId, itemId, variantId, arrivingInStockInDays);
                ItemBLL bll = new ItemBLL(config, clientTimeOutInSeconds);
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
                logger.Debug(config.LSKey.Key, "storeId: {0} item cnt: {1}", storeId, items.Count);
                ItemBLL bll = new ItemBLL(config, clientTimeOutInSeconds);
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
                logger.Debug(config.LSKey.Key, "pageSize:{0} pageNumber:{1} itemCategoryId:{2} productGroupId:{3} search:{4} includeDetails:{5} ",
                    pageSize, pageNumber, itemCategoryId, productGroupId, search, includeDetails);

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
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
                logger.Debug(config.LSKey.Key, "ItemCategoriesGetAll");

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
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
                logger.Debug(config.LSKey.Key, "itemCategoryId:{0}", itemCategoryId);

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
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
                logger.Debug(config.LSKey.Key, "productGroupId:{0} includeDetails:{1} ", productGroupId, includeDetails);

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
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

        public virtual List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "cardId:{0} , includeLines: {1}, listType: {2}", cardId, includeLines, listType.ToString());

                OneListBLL listBLL = new OneListBLL(config, clientTimeOutInSeconds);
                List<OneList> lists = listBLL.OneListGetByCardId(cardId, listType, includeLines);
                OneListSetLocation(lists);
                return lists;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("cardId:{0}   includeLines: {1} , listType: {2}", cardId, includeLines, listType.ToString()));
                return null; //never gets here
            }
        }

        public virtual OneList OneListGetById(string oneListId, bool includeLines)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "oneListId:{0} includeLines:{1}", oneListId, includeLines);

                OneListBLL listBLL = new OneListBLL(config, clientTimeOutInSeconds);
                OneList list = listBLL.OneListGetById(oneListId, includeLines, false);
                OneListSetLocation(list);
                return list;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("oneListId:{0}", oneListId));
                return null; //never gets here
            }
        }

        public virtual OneList OneListSave(OneList oneList, bool calculate)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(oneList));
                OneListBLL listBLL = new OneListBLL(config, clientTimeOutInSeconds);
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
                logger.Debug(config.LSKey.Key, LogJson(oneList));
                OneListBLL listBLL = new OneListBLL(config, clientTimeOutInSeconds);
                return listBLL.OneListCalculate(oneList);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "");
                return null; //never gets here
            }
        }

        public virtual bool OneListDeleteById(string oneListId)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "oneListId:{0}", oneListId);
                OneListBLL listBLL = new OneListBLL(config, clientTimeOutInSeconds);
                listBLL.OneListDeleteById(oneListId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("oneListId:{0}", oneListId));
            }
            return true;
        }

        public virtual OneList OneListItemModify(string onelistId, OneListItem item, bool remove, bool calculate)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "OneListItem.Id:{0} OneList.Id", item.Id, item.OneListId);
                OneListBLL listBLL = new OneListBLL(config, clientTimeOutInSeconds);
                return listBLL.OneListItemModify(onelistId, item, remove, calculate);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("OneListItem.Id:{0} OneList.Id", item.Id, item.OneListId));
                return null; //never gets here
            }
        }

        public virtual bool OneListLinking(string oneListId, string cardId, string email, LinkStatus status)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "oneListId:{0} cardId:{1} email:{2} status:{3}", oneListId, cardId, email, status);
                OneListBLL listBLL = new OneListBLL(config, clientTimeOutInSeconds);
                listBLL.OneListLinking(oneListId, cardId, email, status);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("oneListId:{0}", oneListId));
            }
            return true;
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
                logger.Debug(config.LSKey.Key, "StoresGetAll");

                StoreBLL storeBLL = new StoreBLL(config, clientTimeOutInSeconds);
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
                logger.Debug(config.LSKey.Key, "storeId:{0}  ", storeId);

                StoreBLL storeBLL = new StoreBLL(config, clientTimeOutInSeconds);
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
                logger.Debug(config.LSKey.Key, "latitude:{0} longitude:{1} maxDistance:{2} maxNumberOfStores:{3}",
                    latitude, longitude, maxDistance, maxNumberOfStores);

                StoreBLL storeBLL = new StoreBLL(config, clientTimeOutInSeconds);
                maxNumberOfStores = (maxNumberOfStores > maxNumberReturned ? maxNumberReturned : maxNumberOfStores); //max 1000 should be the limit!
                List<Store> storeList = storeBLL.StoresGetByCoordinates(latitude, longitude, maxDistance, maxNumberOfStores);
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
                logger.Debug(config.LSKey.Key, "itemId:{0}  variantId:{1} latitude:{2} longitude:{3} maxDistance:{4} maxNumberOfStores:{5} ",
                    itemId, variantId, latitude, longitude, maxDistance, maxNumberOfStores);

                StoreBLL storeBLL = new StoreBLL(config, clientTimeOutInSeconds);
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
        /// Get Sales history by card Id
        /// </summary>
        /// <param name="cardId">Card Id</param>
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
        public virtual List<SalesEntry> SalesEntriesGetByCardId(string cardId, int maxNumberOfTransactions)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "cardId:{0} maxNumberOfTransactions:{1}", cardId, maxNumberOfTransactions);
                TransactionBLL transactionBLL = new TransactionBLL(config, clientTimeOutInSeconds);
                maxNumberOfTransactions = (maxNumberOfTransactions > maxNumberReturned ? maxNumberReturned : maxNumberOfTransactions); //max 1000 should be the limit!
                return transactionBLL.SalesEntriesGetByCardId(cardId, maxNumberOfTransactions);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("cardId:{0} maxNumberOfTransactions:{1} ", cardId, maxNumberOfTransactions));
                return null; //never gets here
            }
        }

        public virtual SalesEntry SalesEntryGet(string id, DocumentIdType type)
        {
            try
            {
                TransactionBLL bll = new TransactionBLL(config, clientTimeOutInSeconds);
                return bll.SalesEntryGet(id, type);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "SalesEntryGet");
                return null; //never gets here
            }
        }

        /// <summary>
        /// Search for text based on searchType
        /// </summary>
        /// <param name="cardId">card Id</param>
        /// <param name="search">search string</param>
        /// <param name="searchTypes">enum: All, IOtem, ProductGroup, ItemCategory, OneList, Transaction, Store, Profile, Notification, Offer, Coupon</param>
        /// <returns>SearchRs</returns>
        public virtual SearchRs Search(string cardId, string search, SearchType searchTypes)
        {
            try
            {
                if (cardId == null)
                    cardId = string.Empty;

                logger.Debug(config.LSKey.Key, "cardId:{0} search:{1} searchType:{2} ", cardId, search, searchTypes.ToString());

                SearchBLL searchBLL = new SearchBLL(config, clientTimeOutInSeconds);
                SearchRs searchRs = searchBLL.Search(cardId, search, maxNumberReturned, searchTypes);
                SearchSetLocation(searchRs);
                return searchRs;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("cardId:{0} search:{1} searchTypes:{2}", cardId, search, searchTypes.ToString()));
                return null; //never gets here
            }
        }

        #endregion transactions

        #region Basket 

        public virtual OrderStatusResponse OrderStatusCheck(string orderId)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "orderId:{0}", orderId);
                OrderBLL bll = new OrderBLL(config, clientTimeOutInSeconds);
                return bll.OrderStatusCheck(orderId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("orderId:{0} ", orderId));
                return null; //never gets here
            }
        }

        public virtual string OrderCancel(string orderId, string storeId, string userId)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "orderId:{0}", orderId);
                OrderBLL bll = new OrderBLL(config, clientTimeOutInSeconds);
                bll.OrderCancel(orderId, storeId, userId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("orderId:{0} ", orderId));
            }
            return string.Empty;
        }

        #endregion Basket 

        #region Hierarchy, Menu & Image

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
                logger.Debug(config.LSKey.Key, "id: {0} width: {1}  height: {1} ", id, width, height);
                ImageSize imgSize = new ImageSize(width, height);
                ImageBLL bll = new ImageBLL(config);
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

        public virtual List<Hierarchy> HierarchyGet(string storeId)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "storeId: {0}", storeId);
                ItemBLL bll = new ItemBLL(config, clientTimeOutInSeconds);
                return bll.HierarchyGet(storeId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("storeId: {0}", storeId));
                return null; //never gets here
            }
        }

        public virtual MobileMenu MenuGet(string storeId, string salesType, bool loadDetails, ImageSize imageSize)
        {
            try
            {
                logger.Debug("id: {0} storeId:{1}", salesType, storeId);
                MenuBLL bll = new MenuBLL(config, clientTimeOutInSeconds);
                MobileMenu mobileMenu = bll.MenuGet(storeId, salesType, loadDetails, imageSize);
                MenuSetLocation(mobileMenu);
                return mobileMenu;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("id:{0}  storeId:{1}", salesType, storeId));
                return null; //never gets here
            }
        }

        #endregion

        #region Ads

        //id = "LOY",  or  "HOSPLOY"  etc.
        public virtual List<Advertisement> AdvertisementsGetById(string id, string contactId)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "id: {0} contactId: {1}", id, contactId);

                AdvertisementBLL bll = new AdvertisementBLL(config, clientTimeOutInSeconds);
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

        #region OrderMessage

        public virtual void OrderMessageStatusUpdate(OrderMessage orderMessage)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(orderMessage));
                OrderMessageBLL bll = new OrderMessageBLL(config, this.deviceId, clientTimeOutInSeconds);
                bll.OrderMessageStatusUpdate(orderMessage);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("OrderId:{0}, Line count:{1}", orderMessage.OrderId, orderMessage.Lines.Count));
            }
        }

        public virtual void OrderMessageSave(string orderId, int status, string subject, string message)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "OrderId:{0}, Status:{1}, Subject:{2}, Message:{3}", orderId, status, subject, message);
                OrderMessageBLL bll = new OrderMessageBLL(config, this.deviceId, clientTimeOutInSeconds);
                bll.OrderMessageSave(orderId, status, subject, message);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("OrderId:{0}, Status:{1}, Subject:{2}", orderId, status, subject));
            }
        }

        public virtual string OrderMessageRequestPayment(string orderId, int status, decimal amount, string token, string authcode, string reference)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "id:{0} status:{1} amount:{2} token:{3}", orderId, status, amount, token);
                OrderMessageBLL bll = new OrderMessageBLL(config, clientTimeOutInSeconds);
                return bll.OrderMessageRequestPayment(orderId, status, amount, token, authcode, reference);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("id:{0} status:{1} amount:{2}", orderId, status, amount));
                return null; //never gets here
            }
        }

        #endregion OrderMessage

        #region Click and Collect

        public virtual OrderAvailabilityResponse OrderCheckAvailability(OneList request)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(request));
                OrderBLL bll = new OrderBLL(config, clientTimeOutInSeconds);
                return bll.OrderAvailabilityCheck(request);
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, LogJson(request));
                HandleExceptions(ex, string.Format("id: {0} ", request.Id));
                return null; //never gets here
            }
        }

        public virtual SalesEntry OrderCreate(Order request)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(request));
                OrderBLL bll = new OrderBLL(config, clientTimeOutInSeconds);
                return bll.OrderCreate(request);
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, LogJson(request));
                HandleExceptions(ex, string.Format("id: {0} ", request.Id));
                return null; //never gets here
            }
        }

        #endregion Click and Collect

        #region LS Recommends

        public virtual bool RecommendedActive()
        {
            try
            {
                return LSRecommendsBLLStatic.IsLSRecommendsAvail();
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex);
                HandleExceptions(ex, string.Empty);
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
                logger.Debug(config.LSKey.Key, "userId:{0} items:{1} maxNumberOfItems:{2}", userId, itms, maxNumberOfItems);

                if (LSRecommendsBLLStatic.IsLSRecommendsAvail() == false)
                {
                    throw new NotImplementedException("LS Recommends is not implemented yet. Missing LS Recommends.dll");
                }

                LSRecommendsBLL bll = new LSRecommendsBLL(config, clientTimeOutInSeconds);
                return bll.RecommendedItemsGetByUserId(userId, items, maxNumberOfItems);
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, "userId: {0}", userId);
                HandleExceptions(ex, string.Format("userId: {0} ", userId));
                return null; //never gets here
            }
        }

        public virtual List<RecommendedItem> RecommendedItemsGet(string userId, string storeId, string items)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "userId:{0} storeId:{1} items:{2}", userId, storeId, items);
                if (LSRecommendsBLLStatic.IsLSRecommendsAvail() == false)
                {
                    throw new NotImplementedException("LS Recommends is not implemented yet. Missing LS Recommends.dll");
                }

                LSRecommendsBLL bll = new LSRecommendsBLL(config, clientTimeOutInSeconds);
                return bll.RecommendedItemsGet(userId, storeId, items);
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, "userId:{0}", userId);
                HandleExceptions(ex, string.Format("userId:{0} storeId:{1}", userId, storeId));
                return null; //never gets here
            }
        }

        public void LSRecommendSetting(string endPointUrl, string accountConnection, string azureAccountKey, string azureName, int numberOfRecommendedItems, bool calculateStock, string wsURI, string wsUserName, string wsPassword, string wsDomain, string storeNo, string location, int minStock)
        {
            try
            {
                logger.Debug(config.LSKey.Key, "userId:{0} ", wsUserName);
                if (LSRecommendsBLLStatic.IsLSRecommendsAvail() == false)
                {
                    throw new NotImplementedException("LS Recommends is not implemented yet. Missing LS Recommends.dll");
                }

                LSRecommendsBLL bll = new LSRecommendsBLL(config);
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
