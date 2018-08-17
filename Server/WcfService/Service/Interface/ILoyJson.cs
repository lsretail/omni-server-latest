using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;

namespace LSOmni.Service
{
    // Returns JSON 
    /// <summary>
    /// /LoyJson.svc
    /// </summary>
    [ServiceContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017/Json")]
    public interface ILoyJson
    {
        // COMMON  ////////////////////////////////////////////////////////////
        #region Helpers COMMON

        [OperationContract]
        [WebGet(UriTemplate = "/Ping", ResponseFormat = WebMessageFormat.Json)]
        string PingGet(); //REST http://localhost/LSOmniService/Json.svc/ping 

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string Ping();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        StatusCode PingStatus(); //exposing the StatusCode to client

        //both GET and POST of Version supported
        [OperationContract]
        [WebGet(UriTemplate = "/Version", ResponseFormat = WebMessageFormat.Json)]
        string VersionGet();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string Version();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OmniEnvironment Environment();

        #endregion Helpers   PING

        #region appsettings  COMMON

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string AppSettingsGetByKey(AppSettingsKey key, string languageCode);

        #endregion

        #region Images  COMMON

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ImageView ImageGetById(string id, ImageSize imageSize);

        [OperationContract]
        [WebGet(UriTemplate = "/ImageStreamGetById?id={id}&width={width}&height={height}", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Bare)]
        System.IO.Stream ImageStreamGetById(string id, int width, int height);
        
        #endregion Images

        #region PushNotification  COMMON

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool PushNotificationSave(PushNotificationRequest pushNotificationRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool PushNotificationDelete(string deviceId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityLogSave(ActivityLog activityLog);

        #endregion PushNotification

        #region Profile

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Profile> ProfilesGetAll();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Profile> ProfilesGetByContactId(string contactId);
        
        #endregion Profile

        #region account

        /// <summary>
        /// Get the account by account Id
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Account AccountGetById(string accountId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Scheme> SchemesGetAll();

        #endregion account

        #region contact

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact Login(string userName, string password, string deviceId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact LoginWeb(string userName, string password);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool Logout(string userName, string deviceId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactCreate(MemberContact contact);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactGetById(string contactId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactGetByAlternateId(string alternateId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactUpdate(MemberContact contact);
        
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ChangePassword(string userName, string newPassword, string oldPassword);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        double ContactAddCard(string contactId,string cardId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ResetPassword(string userName, string resetCode, string newPassword);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string ForgotPassword(string userNameOrEmail, string emailSubject, string emailBody);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ForgotPasswordForDevice(string userName);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        long ContactGetPointBalance(string contactId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        decimal GetPointRate();

        #endregion  contact

        #region coupon, offer and notifications

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<PublishedOffer> PublishedOffersGetByCardId(string cardId, string itemId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Notification> NotificationsGetByContactId(string contactId, int numberOfNotifications);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Notification NotificationGetById(string notificationId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool NotificationsUpdateStatus(string contactId, List<string> notificationIds, NotificationStatus notificationStatus);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        NotificationUnread NotificationCountGetUnread(string contactId, DateTime lastChecked);

        #endregion coupon, offer and notifications

        #region Item

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        LoyItem ItemGetById(string itemId, string storeId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        LoyItem ItemGetByBarcode(string barcode, string storeId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<LoyItem> ItemsSearch(string search, int maxNumberOfItems, bool includeDetails);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, bool includeDetails);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ItemCategory> ItemCategoriesGetAll();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ItemCategory ItemCategoriesGetById(string itemCategoryId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ProductGroup ProductGroupGetById(string productGroupId, bool includeDetails);

        #endregion Item

        #region One List  LOY

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<OneList> OneListGetByContactId(string contactId, ListType listType, bool includeLines);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListGetById(string oneListId, ListType listType, bool includeLines);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListSave(OneList oneList, bool calculate);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Order OneListCalculate(OneList oneList);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OneListDeleteById(string oneListId, ListType listType);

        #endregion One List 
 
        #region store location

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Store> StoresGetAll();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Store StoreGetById(string storeId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Store> StoresGetByCoordinates(double latitude, double longitude, double maxDistance, int maxNumberOfStores);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Store> StoresGetbyItemInStock(string itemId, string variantId, double latitude, double longitude, double maxDistance, int maxNumberOfStores);

        #endregion store location

        #region shopping transactions

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<LoyTransaction> TransactionsSearch(string contactId, string itemSearch, int maxNumberOfTransactions, bool includeLines);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        LoyTransaction TransactionGetByReceiptNo(string receiptNo);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<LoyTransaction> SalesEntriesGetByContactId(string contactId, int maxNumberOfTransactions);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        LoyTransaction SalesEntryGetById(string entryId);

        #endregion shopping transactions

        #region search

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        SearchRs Search(string contactId, string search, SearchType searchTypes);
        
        #endregion search

        #region Basket

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        BasketCalcResponse BasketCalc(BasketCalcRequest basketRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderStatusResponse OrderStatusCheck(string transactionId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string OrderCancel(string transactionId);

        #endregion

        #region menu & hierarchy

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MobileMenu MenusGetAll(string id, string lastVersion);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Hierarchy> HierarchyGet(string storeId);

        #endregion menu

        #region Ads

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Advertisement> AdvertisementsGetById(string id, string contactId);
        
        #endregion Ads

        #region OrderQueue

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderQueue OrderQueueSave(OrderQueue order);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderQueue OrderQueueGetById(string orderId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OrderQueueUpdateStatus(string orderId, OrderQueueStatus status);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<OrderQueue> OrderQueueSearch(OrderSearchRequest searchRequest);

        #endregion OrderQueue

        #region OrderMessage

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderMessage OrderMessageSave(OrderMessage orderMessage);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderMessage OrderMessageGetById(string id);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<OrderMessage> OrderMessageSearch(OrderMessageSearchRequest searchRequest);
        
        #endregion OrderMessage

        #region Click and Collect

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Order OrderCreate(Order request);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Order> OrderSearchClickCollect(OrderSearchRequest searchRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<OrderLineAvailability> OrderAvailabilityCheck(OrderAvailabilityRequest request);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderAvailabilityResponse OrderCheckAvailability(OneList request);

        #endregion Click and Collect

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string MyCustomFunction(string data);
    }
}  	 
