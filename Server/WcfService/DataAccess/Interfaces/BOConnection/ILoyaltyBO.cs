using System;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Checkout;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;

namespace LSOmni.DataAccess.Interface.BOConnection
{
    //Interface to the back office, Nav, Ax, etc.
    public interface ILoyaltyBO
    {
        int TimeoutInSeconds { set; }

        string Ping(out string centralVersion);

        #region ScanPayGo

        ScanPayGoProfile ScanPayGoProfileGet(string profileId, string storeNo, Statistics stat);
        bool SecurityCheckProfile(string orderNo, string storeNo, Statistics stat);
        bool SecurityCheckLogResponse(string orderNo, string validationError, bool validationSuccessful, Statistics stat);
        string OpenGate(string qrCode, string storeNo, string devLocation, string memberAccount, bool exitWithoutShopping, bool isEntering, Statistics stat);
        OrderCheck ScanPayGoOrderCheck(string documentId, Statistics stat);
        bool TokenEntrySet(ClientToken token, bool deleteToken, Statistics stat);
        List<ClientToken> TokenEntryGet(string accountNo, bool hotelToken, Statistics stat);

        #endregion

        #region Contact

        string ContactCreate(MemberContact contact, Statistics stat);
        void ContactUpdate(MemberContact contact, string accountId, Statistics stat);
        double ContactAddCard(string contactId, string accountId, string cardId, Statistics stat);
        MemberContact ContactGet(ContactSearchType searchType, string searchValue, Statistics stat);
        List<Customer> CustomerSearch(CustomerSearchType searchType, string search, int maxNumberOfRowsReturned, Statistics stat);
        void ConatctBlock(string accountId, string cardId, Statistics stat);

        MemberContact Login(string userName, string password, string deviceID, string deviceName, Statistics stat);
        MemberContact SocialLogon(string authenticator, string authenticationId, string deviceID, string deviceName, Statistics stat);
        void ChangePassword(string userName, string token, string newPassword, string oldPassword, ref bool oldmethod, Statistics stat);
        string ResetPassword(string userName, string email, string newPassword, ref bool oldmethod, Statistics stat);
        string SPGPassword(string email, string token, string newPwd, Statistics stat);
        void LoginChange(string oldUserName, string newUserName, string password, Statistics stat);

        List<Profile> ProfileGetByCardId(string id, Statistics stat);
        List<Profile> ProfileGetAll(Statistics stat);
        List<Scheme> SchemeGetAll(Statistics stat);
        Scheme SchemeGetById(string schemeId, Statistics stat);

        #endregion

        #region Device

        Device DeviceGetById(string id, Statistics stat);
        bool IsUserLinkedToDeviceId(string userName, string deviceId, Statistics stat);

        #endregion

        #region Card

        Card CardGetById(string id, Statistics stat);
        long MemberCardGetPoints(string cardId, Statistics stat);
        decimal GetPointRate(Statistics stat);
        GiftCard GiftCardGetBalance(string cardNo, string entryType, Statistics stat);
        List<GiftCardEntry> GiftCardGetHistory(string cardNo, string entryType, Statistics stat);
        List<PointEntry> PointEntiesGet(string cardNo, DateTime dateFrom, Statistics stat);

        #endregion

        #region Notification

        List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications, Statistics stat);

        #endregion

        #region Item

        LoyItem ItemLoyGetByBarcode(string code, string storeId, string culture, Statistics stat);
        LoyItem ItemGetById(string itemId, string storeId, string culture, bool includeDetails, Statistics stat);
        List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems, Statistics stat);
        List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, string storeId, bool includeDetails, Statistics stat);
        UnitOfMeasure ItemUOMGetByIds(string itemid, string uomid, Statistics stat);
        List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items, Statistics stat);

        #endregion

        #region Transaction

        List<SalesEntry> SalesEntriesGetByCardId(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNumberOfEntries, Statistics stat);
        SalesEntry SalesEntryGet(string entryId, DocumentIdType type, Statistics stat);
        List<SalesEntryId> SalesEntryGetReturnSales(string receiptNo, Statistics stat);
        List<SalesEntryId> SalesEntryGetSalesByOrderId(string orderId, Statistics stat);
        string FormatAmount(decimal amount, string culture);
        List<SalesEntry> SalesEntrySearch(string search, string cardId, int maxNumberOfTransactions, Statistics stat);

        #endregion

        #region Offer and Advertisement

        List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId, Statistics stat);
        List<Advertisement> AdvertisementsGetById(string id, Statistics stat);

        #endregion

        #region Image

        ImageView ImageGetById(string imageId, bool includeBlob, Statistics stat);
        List<ImageView> ImagesGetByKey(string tableName, string key1, string key2, string key3, int imgCount, bool includeBlob, Statistics stat);

        #endregion

        #region Store

        List<StoreServices> StoreServicesGetByStoreId(string storeId, Statistics stat);
        Store StoreGetById(string id, bool details, Statistics stat);
        List<Store> StoresGetAll(bool clickAndCollectOnly, Statistics stat);
        List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1, Statistics stat);

        #endregion

        #region Hospitality Order

        OrderHosp HospOrderCalculate(OneList list, Statistics stat);
        string HospOrderCreate(OrderHosp request, Statistics stat);
        void HospOrderCancel(string storeId, string orderId, Statistics stat);
        OrderHospStatus HospOrderStatus(string storeId, string orderId, Statistics stat);
        List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId, Statistics stat);

        #endregion

        #region Order

        OrderStatusResponse OrderStatusCheck(string orderId, Statistics stat);
        OrderAvailabilityResponse OrderAvailabilityCheck(OneList request, bool shippingOrder, Statistics stat);
        void OrderCancel(string orderId, string storeId, string userId, List<int> lineNo, Statistics stat);
        string OrderCreate(Order request, out string orderId, Statistics stat);
        Order BasketCalcToOrder(OneList list, Statistics stat);

        #endregion

        #region Search

        List<LoyItem> ItemsSearch(string search, string storeId, int maxNumberOfItems, bool includeDetails, Statistics stat);
        List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned, Statistics stat);
        List<ProductGroup> ProductGroupSearch(string search, Statistics stat);
        List<ItemCategory> ItemCategorySearch(string search, Statistics stat);
        List<Store> StoreLoySearch(string search, Statistics stat);
        List<Profile> ProfileSearch(string cardId, string search, Statistics stat);

        #endregion

        #region ItemCategory and ProductGroup and Hierarchy

        List<ItemCategory> ItemCategoriesGet(string storeId, string culture, Statistics stat);
        ItemCategory ItemCategoriesGetById(string id, Statistics stat);
        List<ProductGroup> ProductGroupGetByItemCategoryId(string itemcategoryId, string culture, bool includeChildren, bool includeItems, Statistics stat);
        ProductGroup ProductGroupGetById(string id, string culture, bool includeItems, bool includeItemDetail, Statistics stat);
        List<Hierarchy> HierarchyGet(string storeId, Statistics stat);
        MobileMenu MenuGet(string storeId, string salesType, Currency currency, Statistics stat);

        #endregion

        #region EComm Replication

        List<ReplImageLink> ReplEcommImageLinks(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplImage> ReplEcommImages(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplAttribute> ReplEcommAttribute(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplAttributeValue> ReplEcommAttributeValue(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplAttributeOptionValue> ReplEcommAttributeOptionValue(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplLoyVendorItemMapping> ReplEcommVendorItemMapping(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplDataTranslation> ReplEcommDataTranslation(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplDataTranslation> ReplEcommHtmlTranslation(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplDataTranslationLangCode> ReplicateEcommDataTranslationLangCode(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplShippingAgent> ReplEcommShippingAgent(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplCustomer> ReplEcommMember(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplCountryCode> ReplEcommCountryCode(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplInvStatus> ReplEcommInventoryStatus(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<LoyItem> ReplEcommFullItem(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);

        #endregion
    }
}
