using System;
using System.IO;
using System.Collections.Generic;
using System.ServiceModel;

using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
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
    //[XmlSerializerFormat]  
    // Returns good old SOAP 
    /// <summary>
    /// /LoyService.svc
    /// </summary>
    [ServiceContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017/Service")]
    [ServiceKnownType(typeof(StatusCode))]
    public interface ILoyService
    {
        #region Helpers

        [OperationContract]
        string Ping();

        [OperationContract]
        StatusCode PingStatus(); //exposing the StatusCode to client

        [OperationContract]
        string Version();

        [OperationContract]
        OmniEnvironment Environment();

        #endregion Helpers

        #region Images

        [OperationContract]
        ImageView ImageGetById(string id, ImageSize imageSize);

        [OperationContract]
        Stream ImageStreamGetById(string id, int width, int height);

        #endregion Images

        #region appsettings

        [OperationContract]
        string AppSettingsGetByKey(AppSettingsKey key, string languageCode);
        
        #endregion

        #region PushNotification

        [OperationContract]
        bool PushNotificationSave(PushNotificationRequest pushNotificationRequest);

        [OperationContract]
        bool PushNotificationDelete(string deviceId);

        [OperationContract]
        bool ActivityLogSave(ActivityLog activityLog);
        
        #endregion PushNotification

        #region Profile

        [OperationContract]
        List<Profile> ProfilesGetAll();

        [OperationContract]
        List<Profile> ProfilesGetByContactId(string contactId);
        
        #endregion Profile

        #region account

        [OperationContract]
        Account AccountGetById(string accountId);

        [OperationContract]
        List<Scheme> SchemesGetAll();
        
        #endregion account

        #region contact

        [OperationContract]
        MemberContact Login(string userName, string password, string deviceId);

        [OperationContract]
        MemberContact LoginWeb(string userName, string password);

        [OperationContract]
        bool Logout(string userName, string deviceId);

        [OperationContract]
        MemberContact ContactCreate(MemberContact contact);

        [OperationContract]
        bool ChangePassword(string userName, string newPassword, string oldPassword);

        [OperationContract]
        MemberContact ContactUpdate(MemberContact contact);

        [OperationContract]
        MemberContact ContactGetById(string contactId);

        [OperationContract]
        MemberContact ContactGetByAlternateId(string alternateId);

        [OperationContract]
        double ContactAddCard(string contactId, string cardId);

        [OperationContract]
        bool DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model);
        
        [OperationContract]
        bool ResetPassword(string userName, string resetCode, string newPassword);
        
        [OperationContract]
        string ForgotPassword(string userNameOrEmail, string emailSubject, string emailBody);

        [OperationContract]
        bool ForgotPasswordForDevice(string userName);

        [OperationContract]
        long ContactGetPointBalance(string contactId);

        [OperationContract]
        decimal GetPointRate();

        #endregion contact

        #region coupon, offer and notifications

        [OperationContract]
        List<PublishedOffer> PublishedOffersGetByCardId(string cardId, string itemId);

        [OperationContract]
        List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId);

        [OperationContract]
        List<Notification> NotificationsGetByContactId(string contactId, int numberOfNotifications);

        [OperationContract]
        Notification NotificationGetById(string notificationId);

        [OperationContract]
        bool NotificationsUpdateStatus(string contactId, List<string> notificationIds, NotificationStatus notificationStatus);

        [OperationContract]
        NotificationUnread NotificationCountGetUnread(string contactId, DateTime lastChecked);

        #endregion coupon, offer and notifications
 
        #region Item

        [OperationContract]
        LoyItem ItemGetById(string itemId, string storeId);

        [OperationContract]
        LoyItem ItemGetByBarcode(string barcode, string storeId);

        [OperationContract]
        List<LoyItem> ItemsSearch(string search, int maxNumberOfItems, bool includeDetails);

        [OperationContract]
        List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems);

        [OperationContract]
        List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, bool includeDetails);

        [OperationContract]
        List<ItemCategory> ItemCategoriesGetAll();

        [OperationContract]
        ItemCategory ItemCategoriesGetById(string itemCategoryId);

        [OperationContract]
        ProductGroup ProductGroupGetById(string productGroupId, bool includeDetails);

        #endregion Item

        #region One List

        [OperationContract]
        List<OneList> OneListGetByContactId(string contactId, ListType listType, bool includeLines);

        [OperationContract]
        List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines);

        [OperationContract]
        OneList OneListGetById(string oneListId, ListType listType, bool includeLines);

        [OperationContract]
        OneList OneListSave(OneList oneList, bool calculate);

        [OperationContract]
        Order OneListCalculate(OneList oneList);

        [OperationContract]
        bool OneListDeleteById(string oneListId, ListType listType);
        
        #endregion One List

        #region store location

        [OperationContract]
        List<Store> StoresGetAll();

        [OperationContract]
        Store StoreGetById(string storeId);

        [OperationContract]
        List<Store> StoresGetByCoordinates(double latitude, double longitude, double maxDistance, int maxNumberOfStores);

        [OperationContract]
        List<Store> StoresGetbyItemInStock(string itemId, string variantId, double latitude, double longitude, double maxDistance, int maxNumberOfStores);
        
        #endregion store location

        #region shopping transactions

        [OperationContract]
        List<LoyTransaction> TransactionsSearch(string contactId, string itemSearch, int maxNumberOfTransactions, bool includeLines);

        [OperationContract]
        LoyTransaction TransactionGetByReceiptNo(string receiptNo);

        [OperationContract]
        List<LoyTransaction> SalesEntriesGetByContactId(string contactId, int maxNumberOfTransactions);

        [OperationContract]
        LoyTransaction SalesEntryGetById(string entryId);

        #endregion shopping transactions

        #region search

        [OperationContract]
        SearchRs Search(string contactId, string search, SearchType searchTypes);
        
        #endregion search

        #region Basket

        [OperationContract]
        BasketCalcResponse BasketCalc(BasketCalcRequest basketRequest);

        [OperationContract]
        OrderStatusResponse OrderStatusCheck(string transactionId);

        [OperationContract]
        string OrderCancel(string transactionId);

        #endregion

        #region menu & hierarchy

        [OperationContract]
        MobileMenu MenusGetAll(string id, string lastVersion);

        [OperationContract]
        List<Hierarchy> HierarchyGet(string storeId);

        #endregion menu

        #region Ads

        [OperationContract]
        List<Advertisement> AdvertisementsGetById(string id, string contactId);
        
        #endregion Ads

        #region OrderQueue

        [OperationContract]
        OrderQueue OrderQueueSave(OrderQueue order);

        [OperationContract]
        OrderQueue OrderQueueGetById(string orderId);

        [OperationContract]
        bool OrderQueueUpdateStatus(string orderId, OrderQueueStatus status);

        [OperationContract]
        List<OrderQueue> OrderQueueSearch(OrderSearchRequest searchRequest);
       
        #endregion OrderQueue

        #region OrderMessage

        [OperationContract]
        OrderMessage OrderMessageSave(OrderMessage orderMessage);

        [OperationContract]
        OrderMessage OrderMessageGetById(string id);

        [OperationContract]
        List<OrderMessage> OrderMessageSearch(OrderMessageSearchRequest searchRequest);

        #endregion OrderMessage

        #region Click and Collect

        [OperationContract]
        Order OrderCreate(Order request);

        [OperationContract]
        List<Order> OrderSearchClickCollect(OrderSearchRequest searchRequest);

        [OperationContract]
        List<OrderLineAvailability> OrderAvailabilityCheck(OrderAvailabilityRequest request);

        [OperationContract]
        OrderAvailabilityResponse OrderCheckAvailability(OneList request);

        #endregion Click and Collect
    }
}
