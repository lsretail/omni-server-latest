using System;
using System.Collections.Generic;

using LSOmni.DataAccess.BOConnection.NavSQL.Dal;
using LSOmni.DataAccess.Interface.BOConnection;

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

        public virtual string Ping(string ipAddress)
        {
            string ver = NavWSBase.NavVersionToUse(true);
            if (ver.Contains("ERROR"))
                throw new ApplicationException(ver);

            return ver;
        }

        #region Contact

        public virtual string ContactCreate(MemberContact contact)
        {
            return NavWSBase.ContactCreate(contact);
        }

        public virtual void ContactUpdate(MemberContact contact, string accountId)
        {
            NavWSBase.ContactUpdate(contact, accountId);
        }

        public virtual MemberContact ContactGetByCardId(string card, int numberOfTrans, bool includeDetails)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            MemberContact contact = rep.ContactGet(ContactSearchType.CardId, card);
        	if (contact != null && includeDetails)
            {
                long totalPoints = MemberCardGetPoints(card);
                contact.Account.PointBalance = (totalPoints == 0) ? contact.Account.PointBalance : totalPoints;

                contact.Profiles = ProfileGetByCardId(card);
                contact.PublishedOffers = PublishedOffersGet(card, string.Empty, string.Empty);
                contact.SalesEntries = SalesEntriesGetByCardId(card, numberOfTrans, string.Empty);
            }
            return contact;
        }

        public virtual MemberContact ContactGetByUserName(string user, bool includeDetails)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
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

        public virtual MemberContact ContactGetByEMail(string email, bool includeDetails)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.ContactGet(ContactSearchType.Email, email);
        }

        public virtual double ContactAddCard(string contactId, string accountId, string cardId)
        {
            return NavWSBase.ContactAddCard(contactId, accountId, cardId);
        }

        public void Login(string userName, string password, string cardId)
        {
            NavWSBase.Logon(userName, password, cardId);
        }

        //Change the password in NAV
        public virtual string ChangePassword(string userName, string newPassword, string oldPassword)
        {
            return NavWSBase.ChangePassword(userName, newPassword, oldPassword);
        }

        public virtual string ResetPassword(string userName, string newPassword)
        {
            return NavWSBase.ResetPassword(userName, newPassword);
        }

        public virtual List<Profile> ProfileGetByCardId(string id)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.ProfileGetByCardId(id);
        }

        public virtual List<Profile> ProfileGetAll()
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.ProfileGetAll();
        }

        public virtual List<Scheme> SchemeGetAll()
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.SchemeGetAll();
        }

        public virtual Scheme SchemeGetById(string schemeId)
        {
            var rep = new ContactRepository(config, NAVVersion);
            if (schemeId.Equals("Ping"))
            {
                rep.SchemeGetById(schemeId);
                return new Scheme("NAV");
            }
            return rep.SchemeGetById(schemeId);
        }

        #endregion

        #region Device

        public virtual void CreateDeviceAndLinkToUser(string userName, string deviceId, string deviceFriendlyName, string cardId = "")
        {
            NavWSBase.CreateDeviceAndLinkToUser(userName, deviceId, deviceFriendlyName, cardId);
        }

        private string GetDefaultDeviceId(string userName)
        {
            return ("WEB-" + userName);
        }

        public virtual Device DeviceGetById(string id)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.DeviceGetById(id);
        }

        public virtual bool IsUserLinkedToDeviceId(string userName, string deviceId)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.IsUserLinkedToDeviceId(userName, deviceId);
        }

        #endregion

        #region Search

        public virtual List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned, bool exact)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.ContactSearch(searchType, search, maxNumberOfRowsReturned, exact);
        }

        public virtual List<LoyItem> ItemsSearch(string search, string storeId, int maxNumberOfItems, bool includeDetails)
        {
            ItemRepository rep = new ItemRepository(config, NAVVersion);
            return rep.ItemLoySearch(search, storeId, maxNumberOfItems, includeDetails);
        }

        public virtual List<ItemCategory> ItemCategorySearch(string search)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository(config, NAVVersion);
            return rep.ItemCategorySearch(search);
        }

        public virtual List<ProductGroup> ProductGroupSearch(string search)
        {
            ProductGroupRepository rep = new ProductGroupRepository(config, NAVVersion);
            return rep.ProductGroupSearch(search);
        }

        public virtual List<Store> StoreLoySearch(string search)
        {
            StoreRepository rep = new StoreRepository(config, NAVVersion);
            return rep.StoreLoySearch(search);
        }

        public virtual List<Profile> ProfileSearch(string cardId, string search)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.ProfileSearch(cardId, search);
        }

        public virtual List<SalesEntry> SalesEntrySearch(string search, string cardId, int maxNumberOfTransactions, bool includeLines)
        {
            SalesEntryRepository rep = new SalesEntryRepository(config, NAVVersion);
            return rep.SalesEntrySearch(search, cardId, maxNumberOfTransactions, includeLines);
        }

        #endregion

        #region Card

        public virtual Card CardGetById(string id)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.CardGetById(id);
        }

        public virtual long MemberCardGetPoints(string cardId)
        {
            return NavWSBase.MemberCardGetPoints(cardId);
        }

        public virtual decimal GetPointRate()
        {
            CurrencyExchRateRepository rep = new CurrencyExchRateRepository(config);
            ReplCurrencyExchRate exchrate = rep.CurrencyExchRateGetById("LOY");
            if (exchrate == null)
                return 0;

            return exchrate.CurrencyFactor;
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo, string entryType)
        {
            return NavWSBase.GiftCardGetBalance(cardNo, entryType);
        }

        #endregion

        #region Notification

        public virtual List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications)
        {
            return NavWSBase.NotificationsGetByCardId(cardId, numberOfNotifications);
        }

        #endregion

        #region Item

        public virtual LoyItem ItemGetById(string id, string storeId, string culture, bool includeDetails)
        {
            ItemRepository rep = new ItemRepository(config, NAVVersion);
            return rep.ItemLoyGetById(id, storeId, culture, includeDetails);
        }

        public virtual LoyItem ItemLoyGetByBarcode(string code, string storeId, string culture)
        {
            ItemRepository rep = new ItemRepository(config, NAVVersion);
            return rep.ItemLoyGetByBarcode(code, storeId, culture);
        }

        public virtual List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems)
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

        public virtual List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, string storeId, bool includeDetails)
        {
            ItemRepository rep = new ItemRepository(config, NAVVersion);
            return rep.ItemsPage(pageSize, pageNumber, itemCategoryId, productGroupId, search, storeId, includeDetails);
        }

        public virtual UnitOfMeasure ItemUOMGetByIds(string itemid, string uomid)
        {
            ItemUOMRepository rep = new ItemUOMRepository(config);
            return rep.ItemUOMGetByIds(itemid, uomid);
        }

        #endregion

        #region ItemCategory and ProductGroup and Hierarchy

        public virtual List<ItemCategory> ItemCategoriesGet(string storeId, string culture)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository(config, NAVVersion);
            return rep.ItemCategoriesGetAll(storeId, culture);
        }

        public virtual ItemCategory ItemCategoriesGetById(string id)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository(config, NAVVersion);
            return rep.ItemCategoryGetById(id);
        }

        public virtual List<ProductGroup> ProductGroupGetByItemCategoryId(string itemcategoryId, string culture, bool includeChildren, bool includeItems)
        {
            ProductGroupRepository rep = new ProductGroupRepository(config, NAVVersion);
            return rep.ProductGroupGetByItemCategoryId(itemcategoryId, culture, includeChildren, includeItems);
        }

        public virtual ProductGroup ProductGroupGetById(string id, string culture, bool includeItems, bool includeItemDetail)
        {
            ProductGroupRepository rep = new ProductGroupRepository(config, NAVVersion);
            return rep.ProductGroupGetById(id, culture, includeItems, includeItemDetail);
        }

        public virtual List<Hierarchy> HierarchyGet(string storeId)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error(config.LSKey.Key, "Only supported in NAV 10.x and later");
                return new List<Hierarchy>();
            }

            HierarchyRepository rep = new HierarchyRepository(config, NAVVersion);
            return rep.HierarchyGetByStore(storeId);
        }

        public virtual MobileMenu MenuGet(string storeId, string salesType, Currency currency)
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

        #region Basket

        public virtual Order BasketCalcToOrder(OneList list)
        {
            return NavWSBase.BasketCalcToOrder(list);
        }

        #endregion

        #region Order

        public virtual OrderStatusResponse OrderStatusCheck(string orderId)
        {
            if (NAVVersion < new Version("13.5"))
                return new OrderStatusResponse();

            return NavWSBase.OrderStatusCheck(orderId);
        }

        public virtual void OrderCancel(string orderId, string storeId, string userId)
        {
            if (NAVVersion < new Version("13.5"))
                return;

            NavWSBase.OrderCancel(orderId, storeId, userId);
        }

        public virtual OrderAvailabilityResponse OrderAvailabilityCheck(OneList request)
        {
            return NavWSBase.OrderAvailabilityCheck(request);
        }

        public virtual string OrderCreate(Order request, string tenderMapping, out string orderId)
        {
            return NavWSBase.OrderCreate(request, tenderMapping, out orderId);
        }

        public virtual SalesEntry SalesEntryGet(string entryId, DocumentIdType type, string tenderMapping)
        {
            if (type == DocumentIdType.Receipt)
            {
                SalesEntryRepository trepo = new SalesEntryRepository(config, NAVVersion);
                return trepo.SalesEntryGetById(entryId);
            }

            OrderRepository repo = new OrderRepository(config, NAVVersion);
            SalesEntry entry = repo.OrderGetById(entryId, true, (type == DocumentIdType.External));
            if (entry == null)
                return null;

            if (entry.Payments != null)
            {
                foreach (SalesEntryPayment line in entry.Payments)
                {
                    line.TenderType = NavWSBase.TenderTypeMapping(tenderMapping, line.TenderType, true); //map tendertype between lsomni and nav
                    if (line.TenderType == null)
                        throw new ApplicationException("TenderType_Mapping failed for type: " + line.TenderType);
                }
            }

            return entry;
        }

        public virtual List<SalesEntry> SalesEntriesGetByCardId(string cardId, int maxNumberOfTransactions, string culture)
        {
            SalesEntryRepository repo = new SalesEntryRepository(config, NAVVersion);
            return repo.SalesEntriesByCardId(cardId, maxNumberOfTransactions);
        }

        #endregion

        #region Offer and Advertisement

        public virtual List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId)
        {
            return NavWSBase.PublishedOffersGet(cardId, itemId, storeId);
        }

        public virtual List<Advertisement> AdvertisementsGetById(string id)
        {
            return NavWSBase.AdvertisementsGetById(id);
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

            if (NAVVersion < new Version("13.01"))
            {
                StoreRepository rep = new StoreRepository(config, NAVVersion);
                return rep.StoreHoursGetByStoreId(storeId, offset);
            }
            return NavWSBase.StoreHoursGetByStoreId(storeId, offset);
        }

        public virtual Store StoreGetById(string id)
        {
            StoreRepository rep = new StoreRepository(config, NAVVersion);
            Store store = rep.StoreLoyGetById(id, true);
            store.StoreHours = StoreHoursGetByStoreId(id);
            return store;
        }

        public virtual List<Store> StoresLoyGetByCoordinates(double latitude, double longitude, double maxDistance, int maxNumberOfStores, Store.DistanceType units)
        {
            StoreRepository rep = new StoreRepository(config, NAVVersion);
            List<Store> stores = rep.StoresLoyGetByCoordinates(latitude, longitude, maxDistance, maxNumberOfStores, units);
            foreach (Store store in stores)
            {
                store.StoreHours = StoreHoursGetByStoreId(store.Id);
            }
            return stores;
        }

        public virtual List<Store> StoresGetAll(bool clickAndCollectOnly)
        {
            StoreRepository rep = new StoreRepository(config, NAVVersion);
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
            return NavWSBase.StoreServicesGetByStoreId(storeId);
        }

        #endregion

        #region Ecomm Replication

        public virtual List<ReplImageLink> ReplEcommImageLinks(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ImageRepository rep = new ImageRepository(config);
            return rep.ReplEcommImageLink(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplImage> ReplEcommImages(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ImageRepository rep = new ImageRepository(config);
            return rep.ReplEcommImage(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplAttribute> ReplEcommAttribute(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            AttributeRepository rep = new AttributeRepository(config);
            return rep.ReplicateEcommAttribute(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplAttributeValue> ReplEcommAttributeValue(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            AttributeValueRepository rep = new AttributeValueRepository(config);
            return rep.ReplicateEcommAttributeValue(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplAttributeOptionValue> ReplEcommAttributeOptionValue(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            AttributeOptionValueRepository rep = new AttributeOptionValueRepository(config);
            return rep.ReplicateEcommAttributeOptionValue(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplLoyVendorItemMapping> ReplEcommVendorItemMapping(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            VendorItemMappingRepository rep = new VendorItemMappingRepository(config);
            return rep.ReplicateEcommVendorItemMapping(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDataTranslation> ReplEcommDataTranslation(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error(config.LSKey.Key, "Only supported in NAV 10.x and later");
                return new List<ReplDataTranslation>();
            }

            DataTranslationRepository rep = new DataTranslationRepository(config);
            return rep.ReplicateEcommDataTranslation(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplShippingAgent> ReplEcommShippingAgent(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ShippingRepository rep = new ShippingRepository(config);
            return rep.ReplicateShipping(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCustomer> ReplEcommMember(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ContactRepository rep = new ContactRepository(config, NAVVersion);
            return rep.ReplicateMembers(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCountryCode> ReplEcommCountryCode(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ShippingRepository rep = new ShippingRepository(config);
            lastKey = string.Empty;
            maxKey = string.Empty;
            recordsRemaining = 0;
            return rep.ReplicateCountryCode();
        }

        public virtual List<ReplInvStatus> ReplEcommInventoryStatus(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            InvStatusRepository rep = new InvStatusRepository(config);
            return rep.ReplicateInventoryStatus(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<LoyItem> ReplEcommFullItem(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemRepository rep = new ItemRepository(config, NAVVersion);
            return rep.ReplicateEcommFullItems(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        #endregion
    }
}
