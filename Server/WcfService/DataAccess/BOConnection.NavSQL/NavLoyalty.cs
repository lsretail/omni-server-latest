using System;
using System.Collections.Generic;

using LSOmni.DataAccess.BOConnection.NavSQL.Dal;
using LSOmni.DataAccess.Interface.BOConnection;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Checkout;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment;

namespace LSOmni.DataAccess.BOConnection.NavSQL
{
    //Navision back office connection
    public class NavLoyalty : NavBase, ILoyaltyBO
    {
        public int TimeoutInSeconds
        {
            set { base.TimeOutInSeconds = value; }
        }

        public NavLoyalty(BOConfiguration config) : base(config)
        {
        }

        public virtual string Ping(out string centralVersion)
        {
            string ver = NavWSBase.NavVersionToUse(true, true, out centralVersion);
            if (ver.Contains("ERROR"))
                throw new ApplicationException(ver);

            return ver;
        }

        #region ScanPayGo

        public virtual ScanPayGoProfile ScanPayGoProfileGet(string profileId, string storeNo, Statistics stat)
        {
            throw new NotImplementedException();
        }

        public virtual bool SecurityCheckProfile(string orderNo, string storeNo, Statistics stat)
        {
            throw new NotImplementedException();
        }

        public virtual string OpenGate(string qrCode, string storeNo, string devLocation, string memberAccount, bool exitWithoutShopping, bool isEntering, Statistics stat)
        {
            throw new NotImplementedException();
        }

        public virtual OrderCheck ScanPayGoOrderCheck(string documentId, Statistics stat)
        {
            throw new NotImplementedException();
        }

        public virtual bool TokenEntrySet(ClientToken token, Statistics stat)
        {
            throw new NotImplementedException();
        }

        public virtual ClientTokenResult TokenEntryGet(string cardNo, Statistics stat)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Contact

        public virtual string ContactCreate(MemberContact contact, Statistics stat)
        {
            return NavWSBase.ContactCreate(contact);
        }

        public virtual void ContactUpdate(MemberContact contact, string accountId, Statistics stat)
        {
            NavWSBase.ContactUpdate(contact, accountId);
        }

        public virtual MemberContact ContactGet(ContactSearchType searchType, string searchValue, Statistics stat)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.ContactGet(searchType, searchValue);
        }

        public virtual List<Customer> CustomerSearch(CustomerSearchType searchType, string search, int maxNumberOfRowsReturned, Statistics stat)
        {
            CustomerRepository rep = new CustomerRepository(config);
            return rep.CustomerSearch(searchType, search, maxNumberOfRowsReturned);
        }

        public virtual double ContactAddCard(string contactId, string accountId, string cardId, Statistics stat)
        {
            return NavWSBase.ContactAddCard(contactId, accountId, cardId);
        }

        public virtual void ConatctBlock(string accountId, string cardId, Statistics stat)
        {
            throw new NotSupportedException();
        }

        public virtual MemberContact Login(string userName, string password, string deviceID, string deviceName, Statistics stat)
        {
            return NavWSBase.Logon(userName, password, deviceID, deviceName);
        }

        public virtual MemberContact SocialLogon(string authenticator, string authenticationId, string deviceID, string deviceName, Statistics stat)
        {
            throw new NotImplementedException();
        }

        //Change the password in NAV
        public virtual void ChangePassword(string userName, string token, string newPassword, string oldPassword, ref bool oldmethod, Statistics stat)
        {
            NavWSBase.ChangePassword(userName, token, newPassword, oldPassword, ref oldmethod);
        }

        public virtual string ResetPassword(string userName, string email, string newPassword, ref bool oldmethod, Statistics stat)
        {
            return NavWSBase.ResetPassword(userName, email, newPassword, ref oldmethod);
        }

        public virtual string SPGPassword(string email, string token, string newPwd, Statistics stat)
        {
            throw new NotImplementedException();
        }

        public virtual void LoginChange(string oldUserName, string newUserName, string password, Statistics stat)
        {
            NavWSBase.LoginChange(oldUserName, newUserName, password);
        }

        public virtual List<Profile> ProfileGetByCardId(string id, Statistics stat)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.ProfileGetByCardId(id);
        }

        public virtual List<Profile> ProfileGetAll(Statistics stat)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.ProfileGetAll();
        }

        public virtual List<Scheme> SchemeGetAll(Statistics stat)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.SchemeGetAll();
        }

        public virtual Scheme SchemeGetById(string schemeId, Statistics stat)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            if (schemeId.Equals("Ping"))
            {
                rep.SchemeGetById(schemeId);
                return new Scheme("NAV");
            }
            return rep.SchemeGetById(schemeId);
        }

        #endregion

        #region Device

        public virtual Device DeviceGetById(string id, Statistics stat)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.DeviceGetById(id);
        }

        public virtual bool IsUserLinkedToDeviceId(string userName, string deviceId, Statistics stat)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.IsUserLinkedToDeviceId(userName, deviceId);
        }

        #endregion

        #region Search

        public virtual List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned, Statistics stat)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.ContactSearch(searchType, search, maxNumberOfRowsReturned);
        }

        public virtual List<LoyItem> ItemsSearch(string search, string storeId, int maxNumberOfItems, bool includeDetails, Statistics stat)
        {
            ItemRepository rep = new ItemRepository(config, NAVVersion);
            return rep.ItemLoySearch(search, storeId, maxNumberOfItems, includeDetails);
        }

        public virtual List<ItemCategory> ItemCategorySearch(string search, Statistics stat)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository(config, NAVVersion);
            return rep.ItemCategorySearch(search);
        }

        public virtual List<ProductGroup> ProductGroupSearch(string search, Statistics stat)
        {
            ProductGroupRepository rep = new ProductGroupRepository(config, NAVVersion);
            return rep.ProductGroupSearch(search);
        }

        public virtual List<Store> StoreLoySearch(string search, Statistics stat)
        {
            StoreRepository rep = new StoreRepository(config, NAVVersion);
            return rep.StoreLoySearch(search);
        }

        public virtual List<Profile> ProfileSearch(string cardId, string search, Statistics stat)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.ProfileSearch(cardId, search);
        }

        public virtual List<SalesEntry> SalesEntrySearch(string search, string cardId, int maxNumberOfTransactions, Statistics stat)
        {
            SalesEntryRepository rep = new SalesEntryRepository(config, NAVVersion);
            return rep.SalesEntrySearch(search, cardId, maxNumberOfTransactions);
        }

        #endregion

        #region Card

        public virtual Card CardGetById(string id, Statistics stat)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.CardGetById(id);
        }

        public virtual long MemberCardGetPoints(string cardId, Statistics stat)
        {
            return NavWSBase.MemberCardGetPoints(cardId);
        }

        public virtual decimal GetPointRate(Statistics stat)
        {
            CurrencyExchRateRepository rep = new CurrencyExchRateRepository(config);
            ReplCurrencyExchRate exchrate = rep.CurrencyExchRateGetById("LOY");
            if (exchrate == null)
                return 0;

            return exchrate.CurrencyFactor;
        }

        public virtual List<PointEntry> PointEntiesGet(string cardNo, DateTime dateFrom, Statistics stat)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.PointEntiesGet(cardNo, dateFrom);
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo, string entryType, Statistics stat)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.GetGiftCartBalance(cardNo, entryType);

            //NAV WS does not work as for now
            //return NavWSBase.GiftCardGetBalance(cardNo, entryType);
        }

        #endregion

        #region Notification

        public virtual List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications, Statistics stat)
        {
            return NavWSBase.NotificationsGetByCardId(cardId);
        }

        #endregion

        #region Item

        public virtual LoyItem ItemGetById(string id, string storeId, string culture, bool includeDetails, Statistics stat)
        {
            ItemRepository rep = new ItemRepository(config, NAVVersion);
            return rep.ItemLoyGetById(id, storeId, culture, includeDetails);
        }

        public virtual LoyItem ItemLoyGetByBarcode(string code, string storeId, string culture, Statistics stat)
        {
            ItemRepository rep = new ItemRepository(config, NAVVersion);
            return rep.ItemLoyGetByBarcode(code, storeId, culture);
        }

        public virtual List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems, Statistics stat)
        {
            List<LoyItem> tmplist = NavWSBase.ItemsGetByPublishedOfferId(pubOfferId, numberOfItems);

            ItemRepository rep = new ItemRepository(config, NAVVersion);
            List<LoyItem> list = new List<LoyItem>();
            foreach (LoyItem item in tmplist)
            {
                list.Add(rep.ItemLoyGetById(item.Id, string.Empty, string.Empty, true));
            }
            return list;
        }

        public virtual List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, string storeId, bool includeDetails, Statistics stat)
        {
            ItemRepository rep = new ItemRepository(config, NAVVersion);
            return rep.ItemsPage(pageSize, pageNumber, itemCategoryId, productGroupId, search, storeId, includeDetails);
        }

        public virtual UnitOfMeasure ItemUOMGetByIds(string itemid, string uomid, Statistics stat)
        {
            ItemUOMRepository rep = new ItemUOMRepository(config);
            return rep.ItemUOMGetByIds(itemid, uomid);
        }

        public virtual List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items, Statistics stat)
        {
            return NavWSBase.ItemCustomerPricesGet(storeId, cardId, items);
        }

        #endregion

        #region ItemCategory and ProductGroup and Hierarchy

        public virtual List<ItemCategory> ItemCategoriesGet(string storeId, string culture, Statistics stat)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository(config, NAVVersion);
            return rep.ItemCategoriesGetAll(storeId, culture);
        }

        public virtual ItemCategory ItemCategoriesGetById(string id, Statistics stat)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository(config, NAVVersion);
            return rep.ItemCategoryGetById(id);
        }

        public virtual List<ProductGroup> ProductGroupGetByItemCategoryId(string itemcategoryId, string culture, bool includeChildren, bool includeItems, Statistics stat)
        {
            ProductGroupRepository rep = new ProductGroupRepository(config, NAVVersion);
            return rep.ProductGroupGetByItemCategoryId(itemcategoryId, culture, includeChildren, includeItems);
        }

        public virtual ProductGroup ProductGroupGetById(string id, string culture, bool includeItems, bool includeItemDetail, Statistics stat)
        {
            ProductGroupRepository rep = new ProductGroupRepository(config, NAVVersion);
            return rep.ProductGroupGetById(id, culture, includeItems, includeItemDetail);
        }

        public virtual List<Hierarchy> HierarchyGet(string storeId, Statistics stat)
        {
            if (NAVVersion.Major < 10)
                throw new NotImplementedException();

            HierarchyRepository rep = new HierarchyRepository(config, NAVVersion);
            return rep.HierarchyGetByStore(storeId);
        }

        public virtual MobileMenu MenuGet(string storeId, string salesType, Currency currency, Statistics stat)
        {
            return NavWSBase.MenuGet(storeId, salesType, currency);
        }

        #endregion

        #region Transaction

        public virtual string FormatAmount(decimal amount, string culture)
        {
            SalesEntryRepository rep = new SalesEntryRepository(config, NAVVersion);
            return rep.FormatAmountToString(amount, culture);
        }

        #endregion

        #region Hospitality Order

        public virtual OrderHosp HospOrderCalculate(OneList list, Statistics stat)
        {
            return NavWSBase.HospOrderCalculate(list);
        }

        public virtual string HospOrderCreate(OrderHosp request, Statistics stat)
        {
            return NavWSBase.HospOrderCreate(request);
        }

        public virtual void HospOrderCancel(string storeId, string orderId, Statistics stat)
        {
            NavWSBase.HospOrderCancel(storeId, orderId);
        }

        public virtual OrderHospStatus HospOrderStatus(string storeId, string orderId, Statistics stat)
        {
            return NavWSBase.HospOrderStatus(storeId, orderId);
        }

        public virtual List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId, Statistics stat)
        {
            InvStatusRepository rep = new InvStatusRepository(config, NAVVersion);
            return rep.CheckAvailability(request, storeId);
        }

        #endregion

        #region Basket

        public virtual Order BasketCalcToOrder(OneList list, Statistics stat)
        {
            return NavWSBase.BasketCalcToOrder(list);
        }

        #endregion

        #region Order

        public virtual OrderStatusResponse OrderStatusCheck(string orderId, Statistics stat)
        {
            if (NAVVersion < new Version("13.5"))
                throw new NotImplementedException();

            return NavWSBase.OrderStatusCheck(orderId);
        }

        public virtual void OrderCancel(string orderId, string storeId, string userId, List<int> lineNo, Statistics stat)
        {
            if (NAVVersion < new Version("13.5"))
                return;

            NavWSBase.OrderCancel(orderId, storeId, userId);
        }

        public virtual OrderAvailabilityResponse OrderAvailabilityCheck(OneList request, bool shippingOrder, Statistics stat)
        {
            return NavWSBase.OrderAvailabilityCheck(request, shippingOrder);
        }

        public virtual string OrderCreate(Order request, out string orderId, Statistics stat)
        {
            if (request.OrderType == OrderType.ScanPayGoSuspend)
            {
                orderId = string.Empty;
                return NavWSBase.ScanPayGoSuspend(request);
            }

            return NavWSBase.OrderCreate(request, out orderId);
        }

        public virtual SalesEntry SalesEntryGet(string entryId, DocumentIdType type, Statistics stat)
        {
            SalesEntry entry;
            if (type == DocumentIdType.Receipt)
            {
                SalesEntryRepository trepo = new SalesEntryRepository(config, NAVVersion);
                entry = trepo.SalesEntryGetById(entryId);
            }
            else
            {
                OrderRepository repo = new OrderRepository(config, NAVVersion);
                entry = repo.OrderGetById(entryId, true, (type == DocumentIdType.External));
            }
            if (entry == null)
                return null;

            if (entry.Payments != null)
            {
                foreach (SalesEntryPayment line in entry.Payments)
                {
                    line.TenderType = ConfigSetting.TenderTypeMapping(config.SettingsGetByKey(ConfigKey.TenderType_Mapping), line.TenderType, true); //map tender type between LSOmni and NAV
                }
            }
            return entry;
        }

        public virtual List<SalesEntryId> SalesEntryGetReturnSales(string receiptNo, Statistics stat)
        {
            throw new NotImplementedException();
        }

        public virtual List<SalesEntryId> SalesEntryGetSalesByOrderId(string orderId, Statistics stat)
        {
            throw new NotImplementedException();
        }


        public virtual List<SalesEntry> SalesEntriesGetByCardId(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNumberOfEntries, Statistics stat)
        {
            SalesEntryRepository repo = new SalesEntryRepository(config, NAVVersion);
            return repo.SalesEntriesByCardId(cardId, storeId, date, dateGreaterThan, maxNumberOfEntries);
        }

        #endregion

        #region Offer and Advertisement

        public virtual List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId, Statistics stat)
        {
            return NavWSBase.PublishedOffersGet(cardId, itemId, storeId);
        }

        public virtual List<Advertisement> AdvertisementsGetById(string id, Statistics stat)
        {
            return NavWSBase.AdvertisementsGetById(id);
        }

        #endregion

        #region Image

        public virtual ImageView ImageGetById(string imageId, bool includeBlob, Statistics stat)
        {
            ImageRepository rep = new ImageRepository(config);
            return rep.ImageGetById(imageId, includeBlob);
        }

        public virtual List<ImageView> ImagesGetByKey(string tableName, string key1, string key2, string key3, int imgCount, bool includeBlob, Statistics stat)
        {
            ImageRepository rep = new ImageRepository(config);
            return rep.ImageGetByKey(tableName, key1, key2, key3, imgCount, includeBlob);
        }

        #endregion

        #region Store

        private List<StoreHours> StoreHoursGetByStoreId(string storeId)
        {
            int offset = config.SettingsIntGetByKey(ConfigKey.Timezone_HoursOffset);

            if (NAVVersion < new Version("13.01"))
            {
                StoreRepository rep = new StoreRepository(config, NAVVersion);
                return rep.StoreHoursGetByStoreId(storeId, offset);
            }
            return NavWSBase.StoreHoursGetByStoreId(storeId, offset);
        }

        public virtual Store StoreGetById(string id, bool details, Statistics stat)
        {
            StoreRepository rep = new StoreRepository(config, NAVVersion);
            Store store = rep.StoreLoyGetById(id, details);
            if (store != null)
                store.StoreHours = StoreHoursGetByStoreId(id);
            return store;
        }

        public virtual List<Store> StoresGetAll(bool clickAndCollectOnly, Statistics stat)
        {
            StoreRepository rep = new StoreRepository(config, NAVVersion);
            List<Store> stores = rep.StoreLoyGetAll(clickAndCollectOnly);
            foreach (Store store in stores)
            {
                store.StoreHours = StoreHoursGetByStoreId(store.Id);
                store.StoreServices = StoreServicesGetByStoreId(store.Id, stat);
            }
            return stores;
        }

        public virtual List<StoreServices> StoreServicesGetByStoreId(string storeId, Statistics stat)
        {
            return NavWSBase.StoreServicesGetByStoreId(storeId);
        }

        public virtual List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1, Statistics stat)
        {
            StoreRepository rep = new StoreRepository(config, NAVVersion);
            return rep.ReturnPolicyGet(storeId, storeGroupCode, itemCategory, productGroup, itemId, variantCode, variantDim1);
        }

        #endregion

        #region EComm Replication

        public virtual List<ReplImageLink> ReplEcommImageLinks(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ImageRepository rep = new ImageRepository(config);
            return rep.ReplEcommImageLink(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplImage> ReplEcommImages(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ImageRepository rep = new ImageRepository(config);
            return rep.ReplEcommImage(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplAttribute> ReplEcommAttribute(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            AttributeRepository rep = new AttributeRepository(config);
            return rep.ReplicateEcommAttribute(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplAttributeValue> ReplEcommAttributeValue(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            AttributeValueRepository rep = new AttributeValueRepository(config);
            return rep.ReplicateEcommAttributeValue(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplAttributeOptionValue> ReplEcommAttributeOptionValue(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            AttributeOptionValueRepository rep = new AttributeOptionValueRepository(config);
            return rep.ReplicateEcommAttributeOptionValue(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplLoyVendorItemMapping> ReplEcommVendorItemMapping(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            VendorItemMappingRepository rep = new VendorItemMappingRepository(config);
            return rep.ReplicateEcommVendorItemMapping(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDataTranslation> ReplEcommDataTranslation(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion.Major < 10)
                throw new NotImplementedException();

            DataTranslationRepository rep = new DataTranslationRepository(config);
            return rep.ReplicateEcommDataTranslation(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDataTranslation> ReplEcommHtmlTranslation(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            throw new NotImplementedException();
        }

        public virtual List<ReplDataTranslationLangCode> ReplicateEcommDataTranslationLangCode(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion.Major < 10)
                throw new NotImplementedException();

            DataTranslationRepository rep = new DataTranslationRepository(config);
            return rep.ReplicateEcommDataTranslationLangCode();
        }

        public virtual List<ReplShippingAgent> ReplEcommShippingAgent(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ShippingRepository rep = new ShippingRepository(config);
            return rep.ReplicateShipping(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCustomer> ReplEcommMember(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.ReplicateMembers(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCountryCode> ReplEcommCountryCode(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ShippingRepository rep = new ShippingRepository(config);
            lastKey = string.Empty;
            maxKey = string.Empty;
            recordsRemaining = 0;
            return rep.ReplicateCountryCode();
        }

        public virtual List<ReplInvStatus> ReplEcommInventoryStatus(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            InvStatusRepository rep = new InvStatusRepository(config, NAVVersion);
            return rep.ReplicateInventoryStatus(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<LoyItem> ReplEcommFullItem(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemRepository rep = new ItemRepository(config, NAVVersion);
            return rep.ReplicateEcommFullItems(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        #endregion
    }
}
