using System;
using System.IO;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;
using LSRetail.Omni.DiscountEngine.DataModels;

namespace LSOmni.Service
{
    // Returns json
    [ServiceContract(Namespace = "http://lsretail.com/LSOmniService/EComm/2017/Json")]
    [ServiceKnownType(typeof(StatusCode))]
    public interface IeCommerceJson
    {
        // COMMON ////////////////////////////////////////////////////////////

        #region Helpers   COMMON

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OmniEnvironment Environment();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string Ping();

        [OperationContract]
        [WebGet(UriTemplate = "/Ping", ResponseFormat = WebMessageFormat.Json)]
        string PingGet(); //REST http://localhost/LSOmniService/Json.svc/ping

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        StatusCode PingStatus(); //exposing the StatusCode to client

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string Version();

        //both GET and POST of Version supported
        [OperationContract]
        [WebGet(UriTemplate = "/Version", ResponseFormat = WebMessageFormat.Json)]
        string VersionGet();

        #endregion

        #region Discount and Offers

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<PublishedOffer> PublishedOffersGetByCardId(string cardId, string itemId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems);

        /// <summary>
        /// Get discounts for items. Send in empty string for loyaltyschemecode if getting anonymously.
        /// </summary>
        /// <param name="storeId">Store Id</param>
        /// <param name="itemiIds">List of item ids to check for discounts</param>
        /// <param name="loyaltySchemeCode">[OPTIONAL] Loyalty scheme code for a user</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemiIds, string loyaltySchemeCode);

        #endregion

        #region Notification

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Notification> NotificationsGetByContactId(string contactId, int numberOfNotifications);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool NotificationsUpdateStatus(string contactId, List<string> notificationIds, NotificationStatus notificationStatus);

        #endregion

        #region One List

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OneListDeleteById(string oneListId, ListType listType);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<OneList> OneListGetByContactId(string contactId, ListType listType, bool includeLines);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListGetById(string id, ListType listType, bool includeLines);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListSave(OneList oneList, bool calculate);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Order OneListCalculate(OneList oneList);

        #endregion

        #region Order

        /// <summary>
        /// Check the quantity of an item available in a certain store, Use with LS Nav 10.0 and earlier
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<OrderLineAvailability> OrderAvailabilityCheck(OrderAvailabilityRequest request);

        /// <summary>
        /// Check the quantity of an item available in a certain store, Use with LS Nav 11.0 and later
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderAvailabilityResponse OrderCheckAvailability(OneList request);

        /// <summary>
        /// Create Basket or Click And Collect Order
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Order OrderCreate(Order request);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Order> OrderSearchClickCollect(OrderSearchRequest searchRequest);

        /// <summary>
        /// Check status of an Order
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderStatusResponse OrderStatusCheck(string transactionId);

        /// <summary>
        /// Cancel Order
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string OrderCancel(string transactionId);

        /// <summary>
        /// Send Payment 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OrderConfirmPayRequest(string orderId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Order OrderGetById(string id, bool includeLines);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Order OrderGetByWebId(string id, bool includeLines);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Order> OrderHistoryByContactId(string contactId, bool includeLines, bool includeTransactions);

        #endregion

        #region Contact

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
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ChangePassword(string userName, string newPassword, string oldPassword);

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
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactCreate(MemberContact contact);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactGetByAlternateId(string alternateId);

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
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactGetById(string contactId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        long ContactGetPointBalance(string contactId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        decimal GetPointRate();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned);

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
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactUpdate(MemberContact contact);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string ForgotPassword(string userNameOrEmail, string emailSubject, string emailBody);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ResetPassword(string userName, string resetCode, string newPassword);

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
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact Login(string userName, string password, string deviceId);

        /// <summary>
        /// Login user from web page.
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="password">password</param>
        /// <returns>Returns contact but only Contact and Card object have data</returns>
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
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact LoginWeb(string userName, string password);

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
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool Logout(string userName, string deviceId);

        /// <summary>
        /// Deletes all information and login for User in Local Omni Database (does not affect NAV)
        /// </summary>
        /// <param name="userName">User Login</param>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool UserDelete(string userName);

        #endregion

        #region Transaction

        /// <summary>
        /// Get transaction by Receipt Number
        /// </summary>
        /// <param name="receiptNo"></param>
        /// <returns></returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description> >
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        LoyTransaction TransactionGetByReceiptNo(string receiptNo);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        LoyTransaction SalesEntryGetById(string entryId);

        /// <summary>
        /// Get All Sales Entries (Transactions and Invoices) by contact Id
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
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<LoyTransaction> SalesEntriesGetByContactId(string contactId, int maxNumberOfTransactions);
        
        #endregion

        #region Item

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<InventoryResponse> ItemsInStockGet(string storeId, string itemId, string variantId, int arrivingInStockInDays);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        LoyItem ItemGetById(string itemId, string storeId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Hierarchy> HierarchyGet(string storeId);

        #endregion

        #region Profile

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Profile> ProfilesGetAll();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Profile> ProfilesGetByContactId(string contactId);

        #endregion

        #region Basket  LOY

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        BasketCalcResponse BasketCalc(BasketCalcRequest basketRequest);

        #endregion

        #region Images  COMMON

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ImageView ImageGetById(string id, ImageSize imageSize);

        [OperationContract]
        [WebGet(UriTemplate = "/ImageStreamGetById?id={id}&width={width}&height={height}", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Bare)]
        Stream ImageStreamGetById(string id, int width, int height);

        #endregion

        #region Appsettings Common

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string AppSettingsGetByKey(AppSettingsKey key, string languageCode);

        #endregion

        #region Account

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
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Account AccountGetById(string accountId);

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
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Scheme> SchemesGetAll();

        #endregion

        #region Store Location

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
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Store StoreGetById(string storeId);

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
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Store> StoresGetAll();
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
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Store> StoresGetByCoordinates(double latitude, double longitude, double maxDistance, int maxNumberOfStores);

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
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Store> StoresGetbyItemInStock(string itemId, string variantId, double latitude, double longitude, double maxDistance, int maxNumberOfStores);

        #endregion

        #region Replication

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplBarcodeResponse ReplEcommBarcodes(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCurrencyResponse ReplEcommCurrency(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCurrencyExchRateResponse ReplEcommCurrencyRate(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplExtendedVariantValuesResponse ReplEcommExtendedVariants(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplImageLinkResponse ReplEcommImageLinks(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplImageResponse ReplEcommImages(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemCategoryResponse ReplEcommItemCategories(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemResponse ReplEcommItems(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemUnitOfMeasureResponse ReplEcommItemUnitOfMeasures(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemVariantRegistrationResponse ReplEcommItemVariantRegistrations(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplPriceResponse ReplEcommPrices(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplPriceResponse ReplEcommBasePrices(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplProductGroupResponse ReplEcommProductGroups(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplStoreResponse ReplEcommStores(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplUnitOfMeasureResponse ReplEcommUnitOfMeasures(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplVendorResponse ReplEcommVendor(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplLoyVendorItemMappingResponse ReplEcommVendorItemMapping(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplAttributeResponse ReplEcommAttribute(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplAttributeValueResponse ReplEcommAttributeValue(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplAttributeOptionValueResponse ReplEcommAttributeOptionValue(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDataTranslationResponse ReplEcommDataTranslation(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplHierarchyResponse ReplEcommHierarchy(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplHierarchyNodeResponse ReplEcommHierarchyNode(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplHierarchyLeafResponse ReplEcommHierarchyLeaf(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplFullItemResponse ReplEcommFullItem(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDiscountResponse ReplEcommDiscounts(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDiscountValidationResponse ReplEcommDiscountValidations(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplShippingAgentResponse ReplEcommShippingAgent(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCustomerResponse ReplEcommMember(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCountryCodeResponse ReplEcommCountryCode(ReplRequest replRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplStoreTenderTypeResponse ReplEcommStoreTenderTypes(ReplRequest replRequest);
        #endregion

        #region LS Recommends

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool RecommendedActive();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<RecommendedItem> RecommendedItemsGetByUserId(string userId, List<LoyItem> items, int maxNumberOfItems);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<RecommendedItem> RecommendedItemsGet(string userId, string storeId, string items);

        #endregion 

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string MyCustomFunction(string data);

    }
}