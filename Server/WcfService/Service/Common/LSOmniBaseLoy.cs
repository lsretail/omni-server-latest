using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
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
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Checkout;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using Newtonsoft.Json;
using RestSharp;

namespace LSOmni.Service
{
    /// <summary>
    /// Base class for JSON, SOAP client and XML 
    /// </summary>
    public partial class LSOmniBase
    {
        #region Profile

        public virtual List<Profile> ProfilesGetAll()
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "ProfileGetAll()");

                ContactBLL profileBLL = new ContactBLL(config, clientTimeOutInSeconds); //no security token needed
                return profileBLL.ProfilesGetAll(stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ProfileGetAll()");
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<Profile> ProfilesGetByCardId(string cardId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, string.Format("ProfilesGetByContactId() - cardId:{0}", cardId));

                ContactBLL profileBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return profileBLL.ProfilesGetByCardId(cardId, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ProfilesGetByContactId() cardId:{0}", cardId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        #endregion Profile

        #region contact and account

        public virtual List<Scheme> SchemesGetAll()
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "SchemeGetAll()");

                ContactBLL accountBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return accountBLL.SchemesGetAll(stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "SchemeGetAll()");
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual MemberContact ContactGetByCardId(string cardId, int numberOfTransReturned)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"cardId:{cardId}");

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                MemberContact contact = contactBLL.ContactGetByCardId(cardId, numberOfTransReturned, stat);
                contact.Environment.Version = this.Version();
                ContactSetLocation(contact);
                if (config.IsJson && logger.IsDebugEnabled)
                    Serialization.TestJsonSerialize(typeof(MemberContact), contact);
                return contact;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ContactGetById() cardId:{0}", cardId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "searchType:{0} searchValue:{1} maxRow:{2}", searchType, search, maxNumberOfRowsReturned);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return contactBLL.ContactSearch(searchType, search, maxNumberOfRowsReturned, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ContactSearch() searchType:{0} searchValue:{1}", searchType, search);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual MemberContact ContactGet(ContactSearchType searchType, string search)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "searchType:{0} searchValue:{1}", searchType, search);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return contactBLL.ContactGet(searchType, search, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ContactGet() searchType:{0} searchValue:{1}", searchType, search);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual MemberContact ContactCreate(MemberContact contact, bool doLogin)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, LogJson(contact));

                if (contact.Cards == null)
                    contact.Cards = new List<Card>();

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);//not using security token here, so no security checks
                MemberContact contactOut = contactBLL.ContactCreate(contact, doLogin, stat);
                contactOut.Environment.Version = this.Version();
                ContactSetLocation(contactOut);
                return contactOut;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ContactCreate()");
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual MemberContact ContactUpdate(MemberContact contact, bool getContact)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, LogJson(contact));

                if (contact.Cards == null)
                    contact.Cards = new List<Card>();

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                MemberContact contactOut = contactBLL.ContactUpdate(contact, getContact, stat);
                contactOut.Environment = new OmniEnvironment();
                contactOut.Environment.Version = this.Version();
                contactOut.LoggedOnToDevice = contact.LoggedOnToDevice;
                ContactSetLocation(contactOut);
                return contactOut;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ContactUpdate()");
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<Customer> CustomerSearch(CustomerSearchType searchType, string search, int maxNumberOfRowsReturned)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "searchType:{0} search:{1} maxNumberOfRowsReturned:{2}", searchType, search, maxNumberOfRowsReturned);

                //if not passed in then default to 0
                if (maxNumberOfRowsReturned < 1)
                    maxNumberOfRowsReturned = 100; //for backward compatibility

                ContactBLL bll = new ContactBLL(config, clientTimeOutInSeconds);
                return bll.CustomerSearch(searchType, search, maxNumberOfRowsReturned, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "searchType:{0} search:{1} maxNumberOfRowsReturned:{2}", searchType, search, maxNumberOfRowsReturned);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual MemberContact Login(string userName, string password, string deviceId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "userName:{0} deviceId:{1}", userName, deviceId);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds); //not using security token here in login, so no security checks
                MemberContact contact = contactBLL.Login(userName, password, true, deviceId, stat);
                contact.Environment.Version = this.Version();
                ContactSetLocation(contact);
                if (config.IsJson && logger.IsDebugEnabled)
                    Serialization.TestJsonSerialize(typeof(MemberContact), contact);
                return contact;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Login() userName:{0}  deviceId:{1}", userName, deviceId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual MemberContact SocialLogon(string authenticator, string authenticationId, string deviceId, string deviceName, bool includeDetails)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "authenticator:{0} deviceId:{1}", authenticator, deviceId);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                MemberContact contact = contactBLL.SocialLogon(authenticator, authenticationId, deviceId, deviceName, includeDetails, stat);
                contact.Environment.Version = this.Version();
                ContactSetLocation(contact);
                if (config.IsJson && logger.IsDebugEnabled)
                    Serialization.TestJsonSerialize(typeof(MemberContact), contact);
                return contact;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Login() authenticator:{0}  deviceId:{1}", authenticator, deviceId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual MemberContact LoginWeb(string userName, string password)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "userName:{0}", userName);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds); //not using security token here, so no security checks
                return contactBLL.Login(userName, password, false, string.Empty, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Login() userName:{0}", userName);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool ChangePassword(string userName, string newPassword, string oldPassword)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "userName:{0}", userName);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                contactBLL.ChangePassword(userName, newPassword, oldPassword, stat);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ChangePassword() userName:{0}", userName);
                return false;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual double ContactAddCard(string contactId, string cardId, string accountId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "contactId:{0}, cardId:{1}, accountId:{2}", contactId, cardId, accountId);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return contactBLL.ContactAddCard(contactId, cardId, accountId, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "contactId:{0}, cardId:{1}, accountId:{2}", contactId, cardId, accountId);
                return 0;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool ConatctBlock(string accountId, string cardId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "accountId:{0} cardId:{1}", accountId, cardId);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                contactBLL.ConatctBlock(accountId, cardId, stat);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ConatctBlock() accountId:{0} cardId:{1}", accountId, cardId);
                return false;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual long CardGetPointBalance(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
                return 0;

            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "cardId:{0}", cardId);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return Convert.ToInt64(contactBLL.CardGetPointBalance(cardId, stat));
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ContactGetPointBalance() cardId:{0}", cardId);
                return 0;  //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<PointEntry> CardGetPointEnties(string cardId, DateTime dateFrom)
        {
            if (string.IsNullOrEmpty(cardId))
                throw new LSOmniServiceException(StatusCode.CardIdInvalid, "Card No missing");
            if (dateFrom == null)
                dateFrom = DateTime.MinValue;

            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "cardId:{0} dateFrom:{1}", cardId, dateFrom);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return contactBLL.CardGetPointEnties(cardId, dateFrom, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "CardGetPointEnties() cardId:{0}", cardId);
                return null;  //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual decimal GetPointRate()
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "PointRate");

                CurrencyBLL curBLL = new CurrencyBLL(config, clientTimeOutInSeconds);
                return curBLL.GetPointRate(stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "GetPointRate()");
                return 0;  //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo, string entryType)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"cardNo:{cardNo} entryType:{entryType}");
                CurrencyBLL bll = new CurrencyBLL(config, clientTimeOutInSeconds);
                return bll.GiftCardGetBalance(cardNo, entryType, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "CardNo:{0}", cardNo);
                return new GiftCard(string.Empty);
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<GiftCardEntry> GiftCardGetHistory(string cardNo, string entryType)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"cardNo:{cardNo} entryType:{entryType}");
                CurrencyBLL bll = new CurrencyBLL(config, clientTimeOutInSeconds);
                return bll.GiftCardGetHistory(cardNo, entryType, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "CardNo:{0}", cardNo);
                return null;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "deviceId:{0}", deviceId);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                contactBLL.DeviceSave(deviceId, deviceFriendlyName, platform, osVersion, manufacturer, model);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "deviceId:{0}", deviceId);
                return false;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool ResetPassword(string userName, string resetCode, string newPassword)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "userName:{0} resetCode:{1}", userName, resetCode);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                contactBLL.ResetPassword(userName, resetCode, newPassword, stat);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "userName:{0} resetCode:{1}", userName, resetCode);
                return false;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual string ForgotPassword(string userNameOrEmail)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "userNameOrEmail:{0}", userNameOrEmail);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return contactBLL.ForgotPassword(userNameOrEmail, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "userNameOrEmail:{0}", userNameOrEmail);
                return string.Empty;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual string PasswordReset(string userName, string email)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "userName:{0} email:{1}", userName, email);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return contactBLL.PasswordReset(userName, email, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "userName:{0} email:{1}", userName, email);
                return string.Empty;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool PasswordChange(string userName, string token, string newPassword, string oldPassword)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "userName:{0} token:{1}", userName, token);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                contactBLL.PasswordChange(userName, token, newPassword, oldPassword, stat);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "userName:{0} token:{1}", userName, token);
                return false;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual string SPGPassword(string email, string token, string newPassword)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"email:{email} token:{token}");

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                return contactBLL.SPGPassword(email, token, newPassword, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "email:{0}", email);
                return string.Empty;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool LoginChange(string oldUserName, string newUserName, string password)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "oldUserName:{0} newUserName:{1}", oldUserName, newUserName);

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                contactBLL.LoginChange(oldUserName, newUserName, password, stat);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "oldUserName:{0} newUserName:{1}", oldUserName, newUserName);
                return false;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        #endregion contact and account

        #region coupon, offer and notification

        public virtual List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "pubOfferId:{0}", pubOfferId.ToString());

                ItemBLL bll = new ItemBLL(config, clientTimeOutInSeconds);
                List<LoyItem> items = bll.ItemsGetByPublishedOfferId(pubOfferId, numberOfItems, stat);
                foreach (LoyItem item in items)
                {
                    ItemSetLocation(item);
                }
                return items;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ItemsGetByPublishedOfferId() pubOfferId:{0}", pubOfferId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<PublishedOffer> PublishedOffersGetByCardId(string cardId, string itemId)
        {
            if (cardId == null)
                cardId = string.Empty;
            if (itemId == null)
                itemId = string.Empty;

            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "itemId:{0} cardId:{1}", itemId, cardId);

                OfferBLL bll = new OfferBLL(config, clientTimeOutInSeconds);
                List<PublishedOffer> list = bll.PublishedOffersGet(cardId, itemId, string.Empty, stat);
                foreach (PublishedOffer it in list)
                {
                    foreach (ImageView iv in it.Images)
                    {
                        iv.StreamURL = GetImageStreamUrl(iv);
                    }
                    foreach (OfferDetails od in it.OfferDetails)
                    {
                        od.Image.StreamURL = GetImageStreamUrl(od.Image);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "itemId:{0} cardId:{1}", itemId, cardId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual Notification NotificationGetById(string notificationId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "notificationId:{0}", notificationId);

                NotificationBLL notificationBLL = new NotificationBLL(config, clientTimeOutInSeconds);
                Notification notification = notificationBLL.NotificationGetById(notificationId, stat);
                NotificationSetLocation(notification);
                return notification;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "notificationId:{0}", notificationId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "cardId:{0} numberOfNotifications:{1}", cardId, numberOfNotifications);

                NotificationBLL notificationBLL = new NotificationBLL(config, clientTimeOutInSeconds);
                List<Notification> notificationList = notificationBLL.NotificationsGetByCardId(cardId, numberOfNotifications, stat);
                foreach (Notification notification in notificationList)
                {
                    NotificationSetLocation(notification);
                }
                return notificationList;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "cardId:{0} numberOfNotifications:{1}", cardId, numberOfNotifications);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        /// <summary>
        /// Update the notification status
        /// </summary>
        public virtual bool NotificationsUpdateStatus(string cardId, List<string> notificationIds, NotificationStatus notificationStatus)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"cardId:{cardId}");

                NotificationBLL notificationBLL = new NotificationBLL(config, clientTimeOutInSeconds);
                notificationBLL.NotificationsUpdateStatus(notificationIds, notificationStatus, stat);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Empty);
                return false;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string loyaltySchemeCode)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "Store:{0} ItemIds:{1} LoyaltySchemeCode:{2}", storeId, string.Join(", ", itemIds), loyaltySchemeCode);

                OfferBLL bll = new OfferBLL(config, clientTimeOutInSeconds);
                return bll.DiscountsGet(storeId, itemIds, loyaltySchemeCode, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Store:{0}, ItemIds:{1}, LoyaltySchemeCode:{2}", storeId, string.Join(", ", itemIds), loyaltySchemeCode);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        #endregion coupon, offer and notification

        #region Item, ItemCategory, ProductGroup

        public virtual LoyItem ItemGetById(string itemId, string storeId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "itemId:{0}", itemId);

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
                LoyItem item = itemBLL.ItemGetById(itemId, storeId, stat);
                ItemSetLocation(item);
                return item;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ItemGetById() itemId:{0}", itemId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual LoyItem ItemGetByBarcode(string barcode, string storeId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "barcode:{0}", barcode);

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
                LoyItem item = itemBLL.ItemGetByBarcode(barcode, storeId, stat);
                ItemSetLocation(item);
                return item;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ItemGetByBarcode() barcode:{0}", barcode);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<LoyItem> ItemsSearch(string search, int maxNumberOfItems, bool includeDetails)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "search:{0} maxNumberOfItems:{1} includeDetails:{2}", search, maxNumberOfItems, includeDetails);

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
                maxNumberOfItems = (maxNumberOfItems > maxNumberReturned ? maxNumberReturned : maxNumberOfItems); //max 1000 should be the limit!
                List<LoyItem> itemList = itemBLL.ItemsSearch(search, maxNumberOfItems, includeDetails, stat);
                foreach (LoyItem it in itemList)
                {
                    ItemSetLocation(it);
                }
                return itemList;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "search:{0} maxNumberOfItems:{1} includeDetails:{2}", search, maxNumberOfItems, includeDetails);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<InventoryResponse> ItemsInStockGet(string storeId, string itemId, string variantId, int arrivingInStockInDays)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "storeId:{0} itemId:{1} variantId:{2} arrivingInStockInDays:{3}", storeId, itemId, variantId, arrivingInStockInDays);

                ItemBLL bll = new ItemBLL(config, clientTimeOutInSeconds);
                return bll.ItemsInStockGet(storeId, itemId, variantId, arrivingInStockInDays, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "storeId:{0} itemId:{1} variantId:{2} arrivingInStockInDays:{3}", storeId, itemId, variantId, arrivingInStockInDays);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<InventoryResponse> ItemsInStoreGet(List<InventoryRequest> items, string storeId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "storeId:{0} itemCnt:{1}", storeId, items.Count);

                ItemBLL bll = new ItemBLL(config, clientTimeOutInSeconds);
                return bll.ItemsInStoreGet(items, storeId, string.Empty, false, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "storeId:{0} itemCnt:{1}", storeId, items.Count);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<InventoryResponse> ItemsInStoreGetEx(List<InventoryRequest> items, string storeId, string locationId, bool useSourcingLocation)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "storeId:{0} itemCnt:{1}", storeId, items.Count);

                ItemBLL bll = new ItemBLL(config, clientTimeOutInSeconds);
                return bll.ItemsInStoreGet(items, storeId, locationId, useSourcingLocation, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "storeId:{0} itemCnt:{1}", storeId, items.Count);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "storeId:{0} itemCnt:{1}", storeId, request.Count);

                ItemBLL bll = new ItemBLL(config, clientTimeOutInSeconds);
                return bll.CheckAvailability(request, storeId, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "storeId:{0} itemCnt:{1}", storeId, request.Count);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<LoyItem> ItemsPage(string storeId, int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, bool includeDetails)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "pageSize:{0} pageNumber:{1} itemCategoryId:{2} productGroupId:{3} search:{4} includeDetails:{5}",
                    pageSize, pageNumber, itemCategoryId, productGroupId, search, includeDetails);

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
                List<LoyItem> itemList = itemBLL.ItemsPage(storeId, pageSize, pageNumber, itemCategoryId, productGroupId, search, includeDetails, stat);
                foreach (LoyItem it in itemList)
                {
                    ItemSetLocation(it);
                }
                return itemList;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "pageSize:{0} pageNumber:{1} itemCategoryId:{2} productGroupId:{3} search:{4} includeDetails:{5}", pageSize, pageNumber, itemCategoryId, productGroupId, search, includeDetails);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "storeId:{0} cardId:{1} itemCnt:{2}", storeId, cardId, items.Count);

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
                return itemBLL.ItemCustomerPricesGet(storeId, cardId, items, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "storeId:{0} cardId:{1}", storeId, cardId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<ItemCategory> ItemCategoriesGetAll()
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "ItemCategoriesGetAll");

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
                List<ItemCategory> categories = itemBLL.ItemCategoriesGetAll(stat);
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
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual ItemCategory ItemCategoriesGetById(string itemCategoryId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "itemCategoryId:{0}", itemCategoryId);

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
                ItemCategory categories = itemBLL.ItemCategoriesGetById(itemCategoryId, stat);
                ItemCategorySetLocation(categories);
                return categories;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "itemCategoryId:{0}", itemCategoryId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual ProductGroup ProductGroupGetById(string productGroupId, bool includeDetails)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "productGroupId:{0} includeDetails:{1}", productGroupId, includeDetails);

                ItemBLL itemBLL = new ItemBLL(config, clientTimeOutInSeconds);
                ProductGroup pg = itemBLL.ProductGroupGetById(productGroupId, includeDetails, stat);
                ProductGroupSetLocation(pg);
                return pg;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "productGroupId:{0} includeDetails:{1}", productGroupId, includeDetails);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        #endregion Item

        #region One list

        public virtual List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "cardId:{0} includeLines:{1} listType:{2}", cardId, includeLines, listType.ToString());

                OneListBLL listBLL = new OneListBLL(config, clientTimeOutInSeconds);
                List<OneList> lists = listBLL.OneListGetByCardId(cardId, listType, includeLines, stat);
                OneListSetLocation(lists);
                return lists;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "cardId:{0} includeLines:{1} listType:{2}", cardId, includeLines, listType);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual OneList OneListGetById(string oneListId, bool includeLines)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "oneListId:{0} includeLines:{1}", oneListId, includeLines);

                OneListBLL listBLL = new OneListBLL(config, clientTimeOutInSeconds);
                OneList list = listBLL.OneListGetById(oneListId, includeLines, false, stat);
                OneListSetLocation(list);
                return list;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "oneListId:{0}", oneListId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual OneList OneListSave(OneList oneList, bool calculate)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, LogJson(oneList));

                OneListBLL listBLL = new OneListBLL(config, clientTimeOutInSeconds);
                OneList list = listBLL.OneListSave(oneList, calculate, stat);
                OneListSetLocation(list);
                return list;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Empty);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual Order OneListCalculate(OneList oneList)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, LogJson(oneList));

                OneListBLL listBLL = new OneListBLL(config, clientTimeOutInSeconds);
                return listBLL.OneListCalculate(oneList, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Empty);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual OrderHosp OneListHospCalculate(OneList oneList)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, LogJson(oneList));

                OneListBLL hostBLL = new OneListBLL(config, clientTimeOutInSeconds);
                return hostBLL.OneListHospCalculate(oneList, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "id:{0}", oneList.Id);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool OneListDeleteById(string oneListId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "oneListId:{0}", oneListId);

                OneListBLL listBLL = new OneListBLL(config, clientTimeOutInSeconds);
                listBLL.OneListDeleteById(oneListId, stat);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "oneListId:{0}", oneListId);
                return false;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual OneList OneListItemModify(string onelistId, OneListItem item, bool remove, bool calculate)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "OneListItem.Id:{0} OneList.Id", item.Id, item.OneListId);

                OneListBLL listBLL = new OneListBLL(config, clientTimeOutInSeconds);
                return listBLL.OneListItemModify(onelistId, item, remove, calculate, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "OneListItem.Id:{0} OneListId:{1}", item.Id, item.OneListId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool OneListLinking(string oneListId, string cardId, string email, string phone, LinkStatus status)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "oneListId:{0} cardId:{1} email:{2} phone:{3} status:{4}",
                        oneListId, cardId, email, phone, status);

                OneListBLL listBLL = new OneListBLL(config, clientTimeOutInSeconds);
                listBLL.OneListLinking(oneListId, cardId, email, phone, status, stat);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "oneListId:{0}", oneListId);
                return false;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        #endregion one list

        #region stores

        public virtual List<Store> StoresGetAll()
        {
            logger.Debug(config.LSKey.Key, "StoresGetAll");
            return StoresGet(StoreGetType.All, false, false);
        }

        public virtual List<Store> StoresGet(StoreGetType storeType, bool includeDetails, bool includeImages)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "StoresGet Type:{0} Det:{1} Img:{2}", storeType, includeDetails, includeImages);

                StoreBLL storeBLL = new StoreBLL(config, clientTimeOutInSeconds);
                List<Store> storeList = storeBLL.StoresGetAll(storeType, includeDetails, includeImages, stat);
                if (includeDetails)
                {
                    foreach (Store st in storeList)
                    {
                        StoreSetLocation(st);
                    }
                }
                return storeList;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "StoresGetAll");
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual Store StoreGetById(string storeId, bool includeImages)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "storeId:{0}", storeId);

                StoreBLL storeBLL = new StoreBLL(config, clientTimeOutInSeconds);
                Store store = storeBLL.StoreGetById(storeId, includeImages, stat);
                StoreSetLocation(store);
                return store;

            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "storeId:{0}", storeId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<Store> StoresGetByCoordinates(double latitude, double longitude, double maxDistance)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "latitude:{0} longitude:{1} maxDistance:{2}",
                    latitude, longitude, maxDistance);

                StoreBLL storeBLL = new StoreBLL(config, clientTimeOutInSeconds);
                List<Store> storeList = storeBLL.StoresGetByCoordinates(latitude, longitude, maxDistance, stat);
                foreach (Store st in storeList)
                {
                    StoreSetLocation(st);
                }
                return storeList;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "latitude:{0} longitude:{1} maxDistance:{2}", latitude, longitude, maxDistance);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<Store> StoresGetbyItemInStock(string itemId, string variantId, double latitude, double longitude, double maxDistance)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "itemId:{0}  variantId:{1} latitude:{2} longitude:{3} maxDistance:{4}",
                    itemId, variantId, latitude, longitude, maxDistance);

                StoreBLL storeBLL = new StoreBLL(config, clientTimeOutInSeconds);
                List<Store> storeList = storeBLL.StoresGetbyItemInStock(itemId, variantId, latitude, longitude, maxDistance, stat);
                foreach (Store st in storeList)
                {
                    StoreSetLocation(st);
                }
                return storeList;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "itemId:{0}  variantId:{1} latitude:{2} longitude:{3} maxDistance:{4}",
                            itemId, variantId, latitude, longitude, maxDistance);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }


        public virtual List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"storeId:{storeId} storeGroupCode:{storeGroupCode} itemCategory:{itemCategory} productGroup:{productGroup} itemId:{itemId} variantCode:{variantCode} variantDim1:{variantDim1}");

                StoreBLL storeBLL = new StoreBLL(config, clientTimeOutInSeconds);
                return storeBLL.ReturnPolicyGet(storeId, storeGroupCode, itemCategory, productGroup, itemId, variantCode, variantDim1, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "storeId:{0} storeGroup:{1} itemCat:{2} prodGroup:{2}", storeId, storeGroupCode, itemCategory, productGroup);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        #endregion store location

        #region transactions

        public virtual List<SalesEntry> SalesEntriesGetByCardId(string cardId, int maxNumberOfEntries)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "cardId:{0} maxNumberOfTransactions:{1}", cardId, maxNumberOfEntries);

                TransactionBLL transactionBLL = new TransactionBLL(config, clientTimeOutInSeconds);
                maxNumberOfEntries = (maxNumberOfEntries > maxNumberReturned ? maxNumberReturned : maxNumberOfEntries); //max 1000 should be the limit!
                List<SalesEntry> data = transactionBLL.SalesEntriesGetByCardId(cardId, string.Empty, DateTime.MinValue, false, maxNumberOfEntries, stat);
                return data;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "cardId:{0} maxNumberOfTransactions:{1}", cardId, maxNumberOfEntries);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<SalesEntry> SalesEntriesGetByCardIdEx(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNumberOfEntries)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "cardId:{0} maxNumberOfTransactions:{1}", cardId, maxNumberOfEntries);

                TransactionBLL transactionBLL = new TransactionBLL(config, clientTimeOutInSeconds);
                maxNumberOfEntries = (maxNumberOfEntries > maxNumberReturned ? maxNumberReturned : maxNumberOfEntries); //max 1000 should be the limit!
                List<SalesEntry> data = transactionBLL.SalesEntriesGetByCardId(cardId, storeId, date, dateGreaterThan, maxNumberOfEntries, stat);
                return data;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "cardId:{0} maxNumberOfTransactions:{1}", cardId, maxNumberOfEntries);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual SalesEntry SalesEntryGet(string id, DocumentIdType type)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                TransactionBLL bll = new TransactionBLL(config, clientTimeOutInSeconds);
                return bll.SalesEntryGet(id, type, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "SalesEntryGet");
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<SalesEntry> SalesEntryGetReturnSales(string receiptNo)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"receiptNo:{receiptNo}");

                TransactionBLL bll = new TransactionBLL(config, clientTimeOutInSeconds);
                return bll.SalesEntryGetReturnSales(receiptNo, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "SalesEntryGetReturnSales");
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<SalesEntry> SalesEntryGetSalesByOrderId(string orderId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"orderId:{orderId}");

                TransactionBLL bll = new TransactionBLL(config, clientTimeOutInSeconds);
                return bll.SalesEntryGetSalesByOrderId(orderId, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "SalesEntryGetSalesByOrderId");
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual SearchRs Search(string cardId, string search, SearchType searchTypes)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "cardId:{0} search:{1} searchType:{2}", cardId, search, searchTypes);

                if (cardId == null)
                    cardId = string.Empty;

                SearchBLL searchBLL = new SearchBLL(config, clientTimeOutInSeconds);
                SearchRs searchRs = searchBLL.Search(cardId, search, maxNumberReturned, searchTypes, stat);
                SearchSetLocation(searchRs);
                return searchRs;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "cardId:{0} search:{1} searchTypes:{2}", cardId, search, searchTypes);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        #endregion transactions

        #region Order 

        public virtual OrderStatusResponse OrderStatusCheck(string orderId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "orderId:{0}", orderId);

                OrderBLL bll = new OrderBLL(config, clientTimeOutInSeconds);
                return bll.OrderStatusCheck(orderId, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "orderId:{0}", orderId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool OrderCancel(string orderId, string storeId, string userId, List<int> lineNo)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "orderId:{0}", orderId);

                OrderBLL bll = new OrderBLL(config, clientTimeOutInSeconds);
                bll.OrderCancel(orderId, storeId, userId, lineNo, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "orderId:{0}", orderId);
                return false;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
            return true;
        }

        public virtual OrderAvailabilityResponse OrderCheckAvailability(OneList request, bool shippingOrder)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, LogJson(request));

                OrderBLL bll = new OrderBLL(config, clientTimeOutInSeconds);
                return bll.OrderAvailabilityCheck(request, shippingOrder, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "id:{0}", request.Id);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual SalesEntry OrderCreate(Order request)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, LogJson(request));

                OrderBLL bll = new OrderBLL(config, clientTimeOutInSeconds);
                SalesEntry data = bll.OrderCreate(request, stat);
                if (config.IsJson && logger.IsDebugEnabled)
                    Serialization.TestJsonSerialize(typeof(SalesEntry), data);
                return data;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "id:{0}", request.Id);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual SalesEntry OrderHospCreate(OrderHosp request)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, LogJson(request));

                OrderBLL hostBLL = new OrderBLL(config, clientTimeOutInSeconds);
                SalesEntry data = hostBLL.OrderHospCreate(request, stat);
                if (config.IsJson && logger.IsDebugEnabled)
                    Serialization.TestJsonSerialize(typeof(SalesEntry), data);
                return data;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "id:{0}", request.Id);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool HospOrderCancel(string storeId, string orderId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "storeId:{0} orderId:{1}", storeId, orderId);

                OrderBLL hostBLL = new OrderBLL(config, clientTimeOutInSeconds);
                hostBLL.HospOrderCancel(storeId, orderId, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "storeId:{0} orderId:{1}", storeId, orderId);
                return false;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
            return true;
        }

        public virtual OrderHospStatus HospOrderStatus(string storeId, string orderId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "storeId:{0} orderId:{1}", storeId, orderId);

                OrderBLL hostBLL = new OrderBLL(config, clientTimeOutInSeconds);
                return hostBLL.HospOrderStatus(storeId, orderId, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "storeId:{0} orderId:{1}", storeId, orderId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
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
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "id:{0} width:{1} height:{1}", id, width, height);

                ImageSize imgSize = new ImageSize(width, height);
                ImageBLL bll = new ImageBLL(config);
                ImageView imgView = bll.ImageSizeGetById(id, imgSize, stat);
                if (imgView == null)
                    return null;
                return ImageConverter.StreamFromBase64(imgView.Image);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "id:{0} width:{1} height:{1}", id, width, height);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<Hierarchy> HierarchyGet(string storeId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "storeId:{0}", storeId);

                ItemBLL bll = new ItemBLL(config, clientTimeOutInSeconds);
                return bll.HierarchyGet(storeId, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "storeId:{0}", storeId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual MobileMenu MenuGet(string storeId, string salesType, bool loadDetails, ImageSize imageSize)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "id:{0} storeId:{1}", salesType, storeId);

                MenuBLL bll = new MenuBLL(config, clientTimeOutInSeconds);
                MobileMenu mobileMenu = bll.MenuGet(storeId, salesType, loadDetails, imageSize, stat);
                MenuSetLocation(mobileMenu);
                return mobileMenu;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "id:{0} storeId:{1}", salesType, storeId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        #endregion

        #region Ads

        //id = "LOY",  or  "HOSPLOY"  etc.
        public virtual List<Advertisement> AdvertisementsGetById(string id, string contactId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "id:{0} contactId:{1}", id, contactId);

                AdvertisementBLL bll = new AdvertisementBLL(config, clientTimeOutInSeconds);
                List<Advertisement> ads = bll.AdvertisementsGetById(id, contactId, stat);
                foreach (Advertisement ad in ads)
                {
                    AdvertisementSetLocation(ad);
                }
                return ads;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "id:{0} : {1}", id, contactId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        #endregion Ads

        #region OrderMessage

        public virtual bool OrderMessageStatusUpdate(OrderMessageStatus orderMessage)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, LogJson(orderMessage));

                OrderMessageBLL bll = new OrderMessageBLL(config, this.deviceId, clientTimeOutInSeconds);
                bll.OrderMessageStatusUpdate(orderMessage, stat);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "OrderId:{0}, Line count:{1}", orderMessage.OrderId, orderMessage.Lines.Count);
                return false;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool OrderMessageSave(string orderId, int status, string subject, string message)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "OrderId:{0} Status:{1} Subject:{2} Message:{3}", orderId, status, subject, message);

                OrderMessageBLL bll = new OrderMessageBLL(config, this.deviceId, clientTimeOutInSeconds);
                bll.OrderMessageSave(orderId, status, subject, message, stat);
                return true;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "OrderId:{0} Status:{1} Subject:{2}", orderId, status, subject);
                return false;
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual string OrderMessageRequestPayment(string orderId, int status, decimal amount, string token, string authcode, string reference)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "id:{0} status:{1} amount:{2} token:{3}", orderId, status, amount, token);

                OrderMessageBLL bll = new OrderMessageBLL(config, clientTimeOutInSeconds);
                return bll.OrderMessageRequestPayment(orderId, status, amount, token, authcode, reference);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "id:{0} status:{1} amount:{2}", orderId, status, amount);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool OrderMessageRequestPaymentEx(string orderId, int status, decimal amount, string curCode, string token, string authcode, string reference, ref string message)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "id:{0} status:{1} amount:{2} cur:{3} token:{4}", orderId, status, amount, curCode, token);

                OrderMessageBLL bll = new OrderMessageBLL(config, clientTimeOutInSeconds);
                return bll.OrderMessageRequestPaymentEx(orderId, status, amount, curCode, token, authcode, reference, ref message);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "id:{0} status:{1} amount:{2}", orderId, status, amount);
                return false; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool OrderMessagePayment(OrderMessagePayment orderPayment, ref string message)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, LogJson(orderPayment));

                OrderMessageBLL bll = new OrderMessageBLL(config, clientTimeOutInSeconds);
                return bll.OrderMessagePayment(orderPayment, ref message);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "OrderMessagePayment");
                return false; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual OrderMessageShippingResult OrderMessageShipping(OrderMessageShipping orderShipping)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "Request - " + LogJson(orderShipping));

                OrderMessageBLL bll = new OrderMessageBLL(config, clientTimeOutInSeconds);
                return bll.OrderMessageShipping(orderShipping);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "OrderMessageShipping");
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        #endregion OrderMessage

        #region LS Recommends

        public virtual bool RecommendedActive()
        {
            // not supported anymore
            return false;
        }

        public virtual List<RecommendedItem> RecommendedItemsGet(List<string> items)
        {
            return null;
        }

        public bool LSRecommendSetting(string lsKey, string batchNo, string modelReaderURL, string authenticationURL, string clientId, string clientSecret, string userName, string password, int numberOfDownloadedItems, int numberOfDisplayedItems, bool filterByInventory, decimal minInvStock)
        {
            return false;
        }

        #endregion LS Recommends

        #region ScanPayGo

        public virtual ClientToken PaymentClientTokenGet(string customerId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"customerId:{customerId}");

                ScanPayGoBLL bll = new ScanPayGoBLL(config, clientTimeOutInSeconds);
                return bll.PaymentClientTokenGet(customerId);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "customerId:{0}", customerId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual ScanPayGoProfile ScanPayGoProfileGet(string profileId, string storeNo)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"profileId:{profileId} storeNo:{storeNo}");

                ScanPayGoBLL bll = new ScanPayGoBLL(config, clientTimeOutInSeconds);
                return bll.ScanPayGoProfileGet(profileId, storeNo, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "profileId:{0}", profileId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool SecurityCheckProfile(string orderNo, string storeNo)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"orderNo:{orderNo} storeNo:{storeNo}");

                ScanPayGoBLL bll = new ScanPayGoBLL(config, clientTimeOutInSeconds);
                return bll.SecurityCheckProfile(orderNo, storeNo, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "orderNo:{0} storeNo:{1}", orderNo, storeNo);
                return false; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public bool SecurityCheckLogResponse(string orderNo, string validationError, bool validationSuccessful)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"orderNo:{orderNo} validationError:{validationError}");

                ScanPayGoBLL bll = new ScanPayGoBLL(config, clientTimeOutInSeconds);
                return bll.SecurityCheckLogResponse(orderNo, validationError, validationSuccessful, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "orderNo:{0} validationError:{1}", orderNo, validationError);
                return false; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual ScanPayGoSecurityLog SecurityCheckLog(string orderNo)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"orderNo:{orderNo}");

                ScanPayGoBLL bll = new ScanPayGoBLL(config, clientTimeOutInSeconds);
                return bll.SecurityCheckLog(orderNo, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "orderNo:{0}", orderNo);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual string OpenGate(string qrCode, string storeNo, string devLocation, string memberAccount, bool exitWithoutShopping, bool isEntering)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"qrCode:{qrCode} storeNo:{storeNo}");

                ScanPayGoBLL bll = new ScanPayGoBLL(config, clientTimeOutInSeconds);
                return bll.OpenGate(qrCode, storeNo, devLocation, memberAccount, exitWithoutShopping, isEntering, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "qrCode:{0} storeNo:{1}", qrCode, storeNo);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual OrderCheck ScanPayGoOrderCheck(string documentId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"documentId:{documentId}");

                ScanPayGoBLL bll = new ScanPayGoBLL(config, clientTimeOutInSeconds);
                return bll.ScanPayGoOrderCheck(documentId, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "documentId:{0}", documentId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual bool TokenEntrySet(ClientToken token, bool deleteToken)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"token:{token}");

                ScanPayGoBLL bll = new ScanPayGoBLL(config, clientTimeOutInSeconds);
                return bll.TokenEntrySet(token, deleteToken, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "TokenEntrySet");
                return false; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        public virtual List<ClientToken> TokenEntryGet(string accountNo, bool hotelToken)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"accountNo:{accountNo}");

                ScanPayGoBLL bll = new ScanPayGoBLL(config, clientTimeOutInSeconds);
                return bll.TokenEntryGet(accountNo, hotelToken, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "accountNo:{0}", accountNo);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        #endregion

        #region PassCreator

        public virtual string CreateWalletPass(string cardId)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            string authorizationKey = config.SettingsGetByKey(ConfigKey.PassCreator_AuthorizationKey);
            string templateId = config.SettingsGetByKey(ConfigKey.PassCreator_TemplateUid);
            
            try
            {
                logger.Debug(config.LSKey.Key, $"CreateWalletPass cardId:{cardId}");

                string passUrl = string.Empty;
                var contact = ContactGetByCardId(cardId, 0);

                //verify contact exists and get extra data
                if (contact != null)
                {
                    //get existing pass
                    var retrievePassClient = new RestClient($"https://app.passcreator.com/api/pass/{cardId}");

                    var retrievePassRequest = new RestRequest(string.Empty, Method.GET);
                    retrievePassRequest.AddOrUpdateHeader("Authorization", authorizationKey);
                    retrievePassRequest.AddOrUpdateHeader("Content-Type", "application/json");

                    var retrievePassResponse = retrievePassClient.Get(retrievePassRequest);

                    if (retrievePassResponse.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(retrievePassResponse.Content))
                    {
                        var retrievePassDynamicObject = JsonConvert.DeserializeObject<dynamic>(retrievePassResponse.Content);
                        return retrievePassDynamicObject.linkToPassPage;
                    }

                    //create pass
                    var createPassClient = new RestClient($"https://app.passcreator.com/api/pass?passtemplate={templateId}&zapierStyle=true");

                    var request = new RestRequest(Method.POST);
                    request.AddOrUpdateHeader("Authorization", authorizationKey);
                    request.AddOrUpdateHeader("Content-Type", "application/json");
                    request.RequestFormat = DataFormat.Json;
                    request.AddBody(new { userProvidedId = cardId, barcodeValue = cardId });

                    var createPassResponse = createPassClient.Post(request); 
                    
                    var createPassDynamicResponse = JsonConvert.DeserializeObject<dynamic>(createPassResponse.Content);
                    return createPassDynamicResponse.linkToPassPage;
                }
                
                return passUrl;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, $"CreateWalletPass cardId:{cardId}");
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        #endregion
    }
}  	 
