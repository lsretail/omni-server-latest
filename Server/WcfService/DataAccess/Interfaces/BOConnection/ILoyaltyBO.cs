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
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

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
        MemberContact ContactGetByCardId(string card, int numberOfTrans, bool includeDetails);
        MemberContact ContactGetByUserName(string user, bool includeDetails);
        MemberContact ContactGetByEMail(string email, bool includeDetails);
        List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned);

        void Login(string userName, string password, string cardId);
        string ChangePassword(string userName, string newPassword, string oldPassword);
        string ResetPassword(string userName, string newPassword);

        List<Profile> ProfileGetByCardId(string id);
        List<Profile> ProfileGetAll();
        List<Scheme> SchemeGetAll();
        Scheme SchemeGetById(string schemeId);

        #endregion

        #region Device

        Device DeviceGetById(string id);
        bool IsUserLinkedToDeviceId(string userName, string deviceId);
        void CreateDeviceAndLinkToUser(string userName, string deviceId, string deviceFriendlyName, string cardId = ""); //JIJ v1.1 it changed

        #endregion

        #region Card

        Card CardGetById(string id);
        long MemberCardGetPoints(string cardId);
        decimal GetPointRate();
        GiftCard GiftCardGetBalance(string cardNo, string entryType);

        #endregion

        #region Notification

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

        List<SalesEntry> SalesEntriesGetByCardId(string cardId, int maxNumberOfTransactions, string culture);
        SalesEntry SalesEntryGet(string entryId, DocumentIdType type, string tenderMapping);
        string FormatAmount(decimal amount, string culture);
        List<SalesEntry> SalesEntrySearch(string search, string cardId, int maxNumberOfTransactions, bool includeLines);

        #endregion

        #region Basket

        Order BasketCalcToOrder(OneList list);

        #endregion

        #region Offer and Advertisement

        List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId);
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
        Store StoreGetById(string id);
        List<Store> StoresGetAll(bool clickAndCollectOnly);
        List<Store> StoresLoyGetByCoordinates(double latitude, double longitude, double maxDistance, int maxNumberOfStores, Store.DistanceType units);

        #endregion

        #region Order

        OrderStatusResponse OrderStatusCheck(string transactionId);
        OrderAvailabilityResponse OrderAvailabilityCheck(OneList request);
        void OrderCancel(string transactionId);
        string OrderCreate(Order request, string tenderMapping, out string orderId);

        #endregion

        #region Search

        List<LoyItem> ItemsSearch(string search, string storeId, int maxNumberOfItems, bool includeDetails);
        List<ProductGroup> ProductGroupSearch(string search);
        List<ItemCategory> ItemCategorySearch(string search);
        List<Store> StoreLoySearch(string search);
        List<Profile> ProfileSearch(string contactId, string search);

        #endregion

        #region ItemCategory and ProductGroup and Hierarchy

        List<ItemCategory> ItemCategoriesGet(string storeId, string culture);
        ItemCategory ItemCategoriesGetById(string id);
        List<ProductGroup> ProductGroupGetByItemCategoryId(string itemcategoryId, string culture, bool includeChildren, bool includeItems);
        ProductGroup ProductGroupGetById(string id, string culture, bool includeItems, bool includeItemDetail);
        List<Hierarchy> HierarchyGet(string storeId);

        #endregion

        #region EComm Replication

        List<ReplImageLink> ReplEcommImageLinks(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplImage> ReplEcommImages(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplAttribute> ReplEcommAttribute(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplAttributeValue> ReplEcommAttributeValue(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplAttributeOptionValue> ReplEcommAttributeOptionValue(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplLoyVendorItemMapping> ReplEcommVendorItemMapping(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplDataTranslation> ReplEcommDataTranslation(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplShippingAgent> ReplEcommShippingAgent(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplCustomer> ReplEcommMember(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplCountryCode> ReplEcommCountryCode(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplInvStatus> ReplEcommInventoryStatus(string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<LoyItem> ReplEcommFullItem(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);

        #endregion
    }
}
