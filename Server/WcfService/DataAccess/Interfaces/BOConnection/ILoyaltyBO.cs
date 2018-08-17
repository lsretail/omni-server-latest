using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.DataAccess.Interface.BOConnection
{
    //Interface to the back office, Nav, Ax, etc.
    public interface ILoyaltyBO
    {
        int TimeoutInSeconds { set; }
        string Ping(string ipAddress);

        #region Contact

        string ContactCreate(MemberContact contact);
        void ContactUpdate(MemberContact contact, string accountId);
        double ContactAddCard(string contactId, string accountId, string cardId);
        MemberContact ContactGetById(string id, int numberOfTrans);
        MemberContact ContactGetByCardId(string card, int numberOfTrans);
        MemberContact ContactGetByUserName(string user);
        MemberContact ContactGetByEMail(string email);
        List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned);

        ContactRs Login(string userName, string password, string cardId);   //added cardId
        string ChangePassword(string userName, string newPassword, string oldPassword);
        string ResetPassword(string userName, string newPassword);

        List<Profile> ProfileGetByContactId(string id);
        List<Profile> ProfileGetAll();
        Account AccountGetById(string accountId);
        List<Scheme> SchemeGetAll();
        Scheme SchemeGetById(string schemeId);

        #endregion

        #region Device

        Device DeviceGetById(string id);
        bool IsUserLinkedToDeviceId(string userName, string deviceId, out string cardId);
        void CreateDeviceAndLinkToUser(string userName, string deviceId, string deviceFriendlyName, string cardId = ""); //JIJ v1.1 it changed

        #endregion

        #region Card

        Card CardGetByContactId(string contactId);
        Card CardGetById(string id);
        long MemberCardGetPoints(string cardId);
        decimal GetPointRate();

        #endregion

        #region Notification

        List<Notification> NotificationsGetByContactId(string contactId, int numberOfNotifications);
        List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications);

        #endregion

        #region Item

        LoyItem ItemLoyGetByBarcode(string code, string storeId, string culture);
        LoyItem ItemGetById(string itemId, string storeId, string culture, bool includeDetails);
        List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems);
        List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, string storeId, bool includeDetails);
        UnitOfMeasure ItemUOMGetByIds(string itemid, string uomid);

        #endregion

        #region Transaction

        LoyTransaction TransactionNavGetByIdWithPrint(string storeId, string terminalId, string transactionId);
        LoyTransaction TransactionGetByReceiptNo(string receiptNo, bool includeLines);
        List<LoyTransaction> SalesEntriesGetByContactId(string contactId, int maxNumberOfTransactions, string culture);
        LoyTransaction SalesEntryGetById(string entryId);
        string FormatAmount(decimal amount, string culture);

        #endregion

        #region Basket

        BasketCalcResponse BasketCalc(BasketCalcRequest basketRequest, decimal shippingPrice = 0);
        Order BasketCalcToOrder(OneList list);

        #endregion

        #region Offer and Advertisement

        List<PublishedOffer> PublishedOffersGetByCardId(string cardId, string itemId);
        List<Advertisement> AdvertisementsGetById(string id);

        #endregion

        #region Menu

        MobileMenu MenusGet(string lastVersion, Currency currency);

        #endregion

        #region Image

        ImageView ImageBOGetById(string imageId);
        List<ImageView> ImageBOGetByKey(string tableName, string key1, string key2, string key3, int imgCount, bool includeBlob);

        #endregion

        #region Store

        List<StoreServices> StoreServicesGetByStoreId(string storeId);
        string GetWIStoreId();
        List<StoreHours> StoreHoursGetByStoreId(string storeId, int offset, int dayOfWeekOffset);
        Store StoreGetById(string id);
        List<Store> StoresGetAll(bool clickAndCollectOnly);
        List<Store> StoresLoyGetByCoordinates(double latitude, double longitude, double maxDistance, int maxNumberOfStores, Store.DistanceType units);

        #endregion

        #region Order

        OrderStatusResponse OrderStatusCheck(string transactionId);
        List<OrderLineAvailability> OrderAvailabilityCheck(OrderAvailabilityRequest request);
        OrderAvailabilityResponse OrderAvailabilityCheck(OneList request);
        void OrderCancel(string transactionId);
        void OrderCreate(Order request, string tenderMapping);
        Order OrderGetById(string id, bool includeLines, string tenderMapping);
        Order OrderGetByWebId(string id, bool includeLines, string tenderMapping);
        List<Order> OrderHistoryByContactId(string contactId, bool includeLines, bool includeTransactions, string tenderMapping);

        #endregion

        #region Search

        List<LoyItem> ItemsSearch(string search, string storeId, int maxNumberOfItems, bool includeDetails);
        List<ProductGroup> ProductGroupSearch(string search);
        List<ItemCategory> ItemCategorySearch(string search);
        List<Store> StoreLoySearch(string search);
        List<Profile> ProfileSearch(string contactId, string search);
        List<LoyTransaction> TransactionSearch(string search, string contactId, int maxNumberOfTransactions, string culture, bool includeLines);

        #endregion

        #region ItemCategory and ProductGroup and Hierarchy

        List<ItemCategory> ItemCategoriesGet(string storeId, string culture);
        ItemCategory ItemCategoriesGetById(string id);
        List<ProductGroup> ProductGroupGetByItemCategoryId(string itemcategoryId, string culture, bool includeChildren, bool includeItems);
        ProductGroup ProductGroupGetById(string id, string culture, bool includeChildren, bool includeItems);
        List<Hierarchy> HierarchyGet(string storeId);

        #endregion

        #region EComm Replication

        List<ReplImageLink> ReplEcommImageLinks(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplImage> ReplEcommImages(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplAttribute> ReplEcommAttribute(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplAttributeValue> ReplEcommAttributeValue(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplAttributeOptionValue> ReplEcommAttributeOptionValue(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplLoyVendorItemMapping> ReplEcommVendorItemMapping(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplDataTranslation> ReplEcommDataTranslation(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplShippingAgent> ReplEcommShippingAgent(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplCustomer> ReplEcommMember(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplCountryCode> ReplEcommCountryCode(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<LoyItem> ReplEcommFullItem(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);

        #endregion
    }
}
