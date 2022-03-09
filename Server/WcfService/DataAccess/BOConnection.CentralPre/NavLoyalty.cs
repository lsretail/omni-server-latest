using System;
using System.Collections.Generic;

using LSOmni.DataAccess.BOConnection.CentralPre.Dal;
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

namespace LSOmni.DataAccess.BOConnection.CentralPre
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

        public virtual string Ping()
        {
            string ver = LSCentralWSBase.NavVersionToUse();
            if (ver.Contains("ERROR"))
                throw new ApplicationException(ver);

            return ver;
        }

        #region ScanPayGo

        public virtual ScanPayGoProfile ScanPayGoProfileGet(string profileId, string storeNo)
        {
            return LSCentralWSBase.ScanPayGoProfileGet(profileId, storeNo);
        }

        public virtual bool SecurityCheckProfile(string orderNo, string storeNo)
        {
            return LSCentralWSBase.SecurityCheckProfile(orderNo, storeNo);
        }

        public virtual string OpenGate(string qrCode, string storeNo, string devLocation, string memberAccount, bool exitWithoutShopping)
        {
            return LSCentralWSBase.OpenGate(qrCode, storeNo, devLocation, memberAccount, exitWithoutShopping);
        }

        #endregion

        #region Contact

        public virtual string ContactCreate(MemberContact contact)
        {
            return LSCentralWSBase.ContactCreate(contact);
        }

        public virtual void ContactUpdate(MemberContact contact, string accountId)
        {
            LSCentralWSBase.ContactUpdate(contact, accountId);
        }

        public virtual MemberContact ContactGetByCardId(string card, int numberOfTrans, bool includeDetails)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            MemberContact contact = rep.ContactGet(ContactSearchType.CardId, card);
        	if (contact != null && includeDetails)
            {
                long totalPoints = MemberCardGetPoints(card);
                contact.Account.PointBalance = (totalPoints == 0) ? contact.Account.PointBalance : totalPoints;

                contact.Profiles = ProfileGetByCardId(card);
                contact.PublishedOffers = PublishedOffersGet(card, string.Empty, string.Empty);
                contact.SalesEntries = SalesEntriesGetByCardId(card, string.Empty, DateTime.MinValue, false, numberOfTrans);
            }
            return contact;
        }

        public virtual MemberContact ContactGetByUserName(string user, bool includeDetails)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            MemberContact contact = rep.ContactGetByUserName(user);
            if (contact != null && includeDetails)
            {
                long totalPoints = MemberCardGetPoints(contact.Cards[0].Id);
                contact.Account.PointBalance = (totalPoints == 0) ? contact.Account.PointBalance : totalPoints;

                contact.Profiles = ProfileGetByCardId(contact.Cards[0].Id);
                contact.PublishedOffers = PublishedOffersGet(contact.Cards[0].Id, string.Empty, string.Empty);
            }
            return contact;
        }

        public virtual MemberContact ContactGet(ContactSearchType searchType, string searchValue)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            return rep.ContactGet(searchType, searchValue);
        }

        public virtual double ContactAddCard(string contactId, string accountId, string cardId)
        {
            return LSCentralWSBase.ContactAddCard(contactId, accountId, cardId);
        }

        public virtual MemberContact Login(string userName, string password, string deviceID, string deviceName, bool includeDetails)
        {
            return LSCentralWSBase.Logon(userName, password, deviceID, deviceName, includeDetails);
        }

        public virtual MemberContact SocialLogon(string authenticator, string authenticationId, string deviceID, string deviceName, bool includeDetails)
        {
            return LSCentralWSBase.SocialLogon(authenticator, authenticationId, deviceID, deviceName, includeDetails);
        }

        //Change the password in NAV
        public virtual void ChangePassword(string userName, string token, string newPassword, string oldPassword, ref bool oldmethod)
        {
            LSCentralWSBase.ChangePassword(userName, token, newPassword, oldPassword);
        }

        public virtual string ResetPassword(string userName, string email, string newPassword, ref bool oldmethod)
        {
            return LSCentralWSBase.ResetPassword(userName, email);
        }

        public virtual string SPGPassword(string email, string token, string newPwd)
        {
            return LSCentralWSBase.SPGPassword(email, token, newPwd);
        }

        public virtual void LoginChange(string oldUserName, string newUserName, string password)
        {
            LSCentralWSBase.LoginChange(oldUserName, newUserName, password);
        }

        public virtual List<Profile> ProfileGetByCardId(string id)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            return rep.ProfileGetByCardId(id);
        }

        public virtual List<Profile> ProfileGetAll()
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            return rep.ProfileGetAll();
        }

        public virtual List<Scheme> SchemeGetAll()
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            return rep.SchemeGetAll();
        }

        public virtual Scheme SchemeGetById(string schemeId)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            if (schemeId.Equals("Ping"))
            {
                rep.SchemeGetById(schemeId);
                return new Scheme("NAV");
            }
            return rep.SchemeGetById(schemeId);
        }

        #endregion

        #region Device

        public virtual Device DeviceGetById(string id)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            return rep.DeviceGetById(id);
        }

        public virtual bool IsUserLinkedToDeviceId(string userName, string deviceId)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            return rep.IsUserLinkedToDeviceId(userName, deviceId);
        }

        #endregion

        #region Search

        public virtual List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned, bool exact)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            return rep.ContactSearch(searchType, search, maxNumberOfRowsReturned, exact);
        }

        public virtual List<LoyItem> ItemsSearch(string search, string storeId, int maxNumberOfItems, bool includeDetails)
        {
            ItemRepository rep = new ItemRepository(config, LSCVersion);
            return rep.ItemLoySearch(search, storeId, maxNumberOfItems, includeDetails);
        }

        public virtual List<ItemCategory> ItemCategorySearch(string search)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository(config);
            return rep.ItemCategorySearch(search);
        }

        public virtual List<ProductGroup> ProductGroupSearch(string search)
        {
            ProductGroupRepository rep = new ProductGroupRepository(config);
            return rep.ProductGroupSearch(search);
        }

        public virtual List<Store> StoreLoySearch(string search)
        {
            StoreRepository rep = new StoreRepository(config, LSCVersion);
            return rep.StoreLoySearch(search);
        }

        public virtual List<Profile> ProfileSearch(string cardId, string search)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            return rep.ProfileSearch(cardId, search);
        }

        public virtual List<SalesEntry> SalesEntrySearch(string search, string cardId, int maxNumberOfTransactions)
        {
            SalesEntryRepository rep = new SalesEntryRepository(config);
            return rep.SalesEntrySearch(search, cardId, maxNumberOfTransactions);
        }

        #endregion

        #region Card

        public virtual Card CardGetById(string id)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            return rep.CardGetById(id);
        }

        public virtual long MemberCardGetPoints(string cardId)
        {
            return LSCentralWSBase.MemberCardGetPoints(cardId);
        }

        public virtual decimal GetPointRate()
        {
            CurrencyExchRateRepository rep = new CurrencyExchRateRepository(config);
            ReplCurrencyExchRate exchrate = rep.CurrencyExchRateGetById("LOY");
            if (exchrate == null)
                return 0;

            return exchrate.CurrencyFactor;
        }

        public virtual List<PointEntry> PointEntiesGet(string cardNo, DateTime dateFrom)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            return rep.PointEntiesGet(cardNo, dateFrom);
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo, string entryType)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            return rep.GetGiftCartBalance(cardNo, entryType);
        }

        #endregion

        #region Notification

        public virtual List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications)
        {
            return LSCentralWSBase.NotificationsGetByCardId(cardId);
        }

        #endregion

        #region Item

        public virtual LoyItem ItemGetById(string id, string storeId, string culture, bool includeDetails)
        {
            ItemRepository rep = new ItemRepository(config, LSCVersion);
            return rep.ItemLoyGetById(id, storeId, culture, includeDetails);
        }

        public virtual LoyItem ItemLoyGetByBarcode(string code, string storeId, string culture)
        {
            ItemRepository rep = new ItemRepository(config, LSCVersion);
            LoyItem item = rep.ItemLoyGetByBarcode(code, storeId, culture);
            if (item != null)
                return item;

            LoyItem bitem = LSCentralWSBase.ItemFindByBarcode(code, storeId, config.SettingsGetByKey(ConfigKey.ScanPayGo_Terminal));
            item = rep.ItemLoyGetById(bitem.Id, storeId, string.Empty, true);
            item.GrossWeight = bitem.GrossWeight;
            item.Price = bitem.Price;
            return item;
        }

        public virtual List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems)
        {
            List<LoyItem> tmplist = LSCentralWSBase.ItemsGetByPublishedOfferId(pubOfferId, numberOfItems);

            ItemRepository rep = new ItemRepository(config, LSCVersion);
            List<LoyItem> list = new List<LoyItem>();
            foreach (LoyItem item in tmplist)
            {
                list.Add(rep.ItemLoyGetById(item.Id, string.Empty, string.Empty, true));
            }
            return list;
        }

        public virtual List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, string storeId, bool includeDetails)
        {
            ItemRepository rep = new ItemRepository(config, LSCVersion);
            return rep.ItemsPage(pageSize, pageNumber, itemCategoryId, productGroupId, search, storeId, includeDetails);
        }

        public virtual UnitOfMeasure ItemUOMGetByIds(string itemid, string uomid)
        {
            ItemUOMRepository rep = new ItemUOMRepository(config);
            return rep.ItemUOMGetByIds(itemid, uomid);
        }

        public virtual List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items)
        {
            return LSCentralWSBase.ItemCustomerPricesGet(storeId, cardId, items);
        }

        #endregion

        #region ItemCategory and ProductGroup and Hierarchy

        public virtual List<ItemCategory> ItemCategoriesGet(string storeId, string culture)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository(config);
            return rep.ItemCategoriesGetAll(storeId, culture);
        }

        public virtual ItemCategory ItemCategoriesGetById(string id)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository(config);
            return rep.ItemCategoryGetById(id);
        }

        public virtual List<ProductGroup> ProductGroupGetByItemCategoryId(string itemcategoryId, string culture, bool includeChildren, bool includeItems)
        {
            ProductGroupRepository rep = new ProductGroupRepository(config);
            return rep.ProductGroupGetByItemCategoryId(itemcategoryId, culture, includeChildren, includeItems);
        }

        public virtual ProductGroup ProductGroupGetById(string id, string culture, bool includeItems, bool includeItemDetail)
        {
            ProductGroupRepository rep = new ProductGroupRepository(config);
            return rep.ProductGroupGetById(id, culture, includeItems, includeItemDetail);
        }

        public virtual List<Hierarchy> HierarchyGet(string storeId)
        {
            HierarchyRepository rep = new HierarchyRepository(config);
            return rep.HierarchyGetByStore(storeId);
        }

        public virtual MobileMenu MenuGet(string storeId, string salesType, Currency currency)
        {
            return LSCentralWSBase.MenuGet(storeId, salesType, currency);
        }

        #endregion

        #region Transaction

        public virtual string FormatAmount(decimal amount, string culture)
        {
            SalesEntryRepository rep = new SalesEntryRepository(config);
            return rep.FormatAmountToString(amount, culture);
        }

        #endregion

        #region Hospitality Order

        public virtual OrderHosp HospOrderCalculate(OneList list)
        {
            return LSCentralWSBase.HospOrderCalculate(list);
        }

        public virtual string HospOrderCreate(OrderHosp request)
        {
            return LSCentralWSBase.HospOrderCreate(request);
        }

        public virtual void HospOrderCancel(string storeId, string orderId)
        {
            LSCentralWSBase.HospOrderCancel(storeId, orderId);
        }

        public virtual OrderHospStatus HospOrderStatus(string storeId, string orderId)
        {
            return LSCentralWSBase.HospOrderKotStatus(storeId, orderId);
        }

        public virtual List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId)
        {
            InvStatusRepository rep = new InvStatusRepository(config, LSCVersion);
            return rep.CheckAvailability(request, storeId);
        }

        #endregion

        #region Basket

        public virtual Order BasketCalcToOrder(OneList list)
        {
            return LSCentralWSBase.BasketCalcToOrder(list);
        }

        #endregion

        #region Order

        public virtual OrderStatusResponse OrderStatusCheck(string orderId)
        {
            OrderRepository repo = new OrderRepository(config, LSCVersion);
            return repo.OrderStatusGet(orderId);
        }

        public virtual void OrderCancel(string orderId, string storeId, string userId, List<int> lineNo)
        {
            LSCentralWSBase.OrderCancel(orderId, storeId, userId, lineNo);
        }

        public virtual OrderAvailabilityResponse OrderAvailabilityCheck(OneList request, bool shippingOrder)
        {
            return LSCentralWSBase.OrderAvailabilityCheck(request, shippingOrder);
        }

        public virtual string OrderCreate(Order request, out string orderId)
        {
            if (request.OrderType == OrderType.ScanPayGoSuspend)
            {
                orderId = string.Empty;
                return LSCentralWSBase.ScanPayGoSuspend(request);
            }

            return LSCentralWSBase.OrderCreate(request, out orderId);
        }

        public virtual SalesEntry SalesEntryGet(string entryId, DocumentIdType type)
        {
            SalesEntry entry;
            if (type == DocumentIdType.Receipt)
            {
                SalesEntryRepository trepo = new SalesEntryRepository(config);
                entry = trepo.SalesEntryGetById(entryId);
            }
            else if (type == DocumentIdType.HospOrder)
            {
                SalesEntryRepository trepo = new SalesEntryRepository(config);
                entry = trepo.POSTransactionGetById(entryId);
                if (entry == null)
                    entry = trepo.SalesEntryGetById(entryId);
            }
            else
            {
                OrderRepository repo = new OrderRepository(config, LSCVersion);
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

        public virtual List<SalesEntry> SalesEntryGetReturnSales(string receiptNo)
        {
            SalesEntryRepository repo = new SalesEntryRepository(config);
            return repo.SalesEntryGetReturnSales(receiptNo);
        }

        public virtual List<SalesEntry> SalesEntriesGetByCardId(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNumberOfEntries)
        {
            SalesEntryRepository repo = new SalesEntryRepository(config);
            return repo.SalesEntriesByCardId(cardId, storeId, date, dateGreaterThan, maxNumberOfEntries);
        }

        #endregion

        #region Offer and Advertisement

        public virtual List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId)
        {
            return LSCentralWSBase.PublishedOffersGet(cardId, itemId, storeId);
        }

        public virtual List<Advertisement> AdvertisementsGetById(string id)
        {
            return LSCentralWSBase.AdvertisementsGetById(id);
        }

        #endregion

        #region Image

        public virtual ImageView ImageGetById(string imageId, bool includeBlob)
        {
            ImageRepository rep = new ImageRepository(config);
            return rep.ImageGetById(imageId, includeBlob);
        }

        public virtual List<ImageView> ImagesGetByKey(string tableName, string key1, string key2, string key3, int imgCount, bool includeBlob)
        {
            ImageRepository rep = new ImageRepository(config);
            return rep.ImageGetByKey(tableName, key1, key2, key3, imgCount, includeBlob);
        }

        #endregion

        #region Store

        private List<StoreHours> StoreHoursGetByStoreId(string storeId)
        {
            int offset = config.SettingsIntGetByKey(ConfigKey.Timezone_HoursOffset);
            return LSCentralWSBase.StoreHoursGetByStoreId(storeId, offset);
        }

        public virtual Store StoreGetById(string id, bool details)
        {
            StoreRepository rep = new StoreRepository(config, LSCVersion);
            Store store = rep.StoreLoyGetById(id, details);
            if (store != null)
                store.StoreHours = StoreHoursGetByStoreId(id);
            return store;
        }

        public virtual List<Store> StoresLoyGetByCoordinates(double latitude, double longitude, double maxDistance, Store.DistanceType units)
        {
            StoreRepository rep = new StoreRepository(config, LSCVersion);
            List<Store> stores = rep.StoresLoyGetByCoordinates(latitude, longitude, maxDistance, units);
            foreach (Store store in stores)
            {
                store.StoreHours = StoreHoursGetByStoreId(store.Id);
            }
            return stores;
        }

        public virtual List<Store> StoresGetAll(bool clickAndCollectOnly)
        {
            StoreRepository rep = new StoreRepository(config, LSCVersion);
            List<Store> stores = rep.StoreLoyGetAll(clickAndCollectOnly);
            foreach (Store store in stores)
            {
                store.StoreHours = StoreHoursGetByStoreId(store.Id);
                store.StoreServices = StoreServicesGetByStoreId(store.Id);
            }
            return stores;
        }

        public virtual List<StoreServices> StoreServicesGetByStoreId(string storeId)
        {
            return LSCentralWSBase.StoreServicesGetByStoreId(storeId);
        }

        public virtual List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1)
        {
            StoreRepository rep = new StoreRepository(config, LSCVersion);
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
            DataTranslationRepository rep = new DataTranslationRepository(config);
            return rep.ReplicateEcommDataTranslation(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDataTranslation> ReplEcommHtmlTranslation(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            DataTranslationRepository rep = new DataTranslationRepository(config);
            return rep.ReplicateEcommHtmlTranslation(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDataTranslationLangCode> ReplicateEcommDataTranslationLangCode(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            DataTranslationRepository rep = new DataTranslationRepository(config);
            return rep.ReplicateEcommDataTranslationLangCode();
        }

        public virtual List<ReplShippingAgent> ReplEcommShippingAgent(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ShippingRepository rep = new ShippingRepository(config, LSCVersion);
            return rep.ReplicateShipping(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCustomer> ReplEcommMember(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            return rep.ReplicateMembers(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCountryCode> ReplEcommCountryCode(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ShippingRepository rep = new ShippingRepository(config, LSCVersion);
            lastKey = string.Empty;
            maxKey = string.Empty;
            recordsRemaining = 0;
            return rep.ReplicateCountryCode();
        }

        public virtual List<ReplInvStatus> ReplEcommInventoryStatus(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            InvStatusRepository rep = new InvStatusRepository(config, LSCVersion);
            return rep.ReplicateInventoryStatus(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<LoyItem> ReplEcommFullItem(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemRepository rep = new ItemRepository(config, LSCVersion);
            return rep.ReplicateEcommFullItems(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        #endregion
    }
}
