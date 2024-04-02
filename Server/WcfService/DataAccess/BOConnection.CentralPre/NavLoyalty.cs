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
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Checkout;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment;

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

        public virtual string Ping(out string centralVersion)
        {
            string ver = LSCentralWSBase.NavVersionToUse(true, out centralVersion);
            if (ver.Contains("ERROR"))
                throw new ApplicationException(ver);

            return ver;
        }

        #region ScanPayGo

        public virtual ScanPayGoProfile ScanPayGoProfileGet(string profileId, string storeNo, Statistics stat)
        {
            return LSCentralWSBase.ScanPayGoProfileGet(profileId, storeNo, stat);
        }

        public virtual bool SecurityCheckProfile(string orderNo, string storeNo, Statistics stat)
        {
            return LSCentralWSBase.SecurityCheckProfile(orderNo, storeNo, stat);
        }

        public bool SecurityCheckLogResponse(string orderNo, string validationError, bool validationSuccessful, Statistics stat)
        {
            return LSCentralWSBase.SecurityCheckLogResponse(orderNo, validationError, validationSuccessful, stat);
        }

        public ScanPayGoSecurityLog SecurityCheckLog(string orderNo, Statistics stat)
        {
            InvStatusRepository rep = new InvStatusRepository(config, LSCVersion);
            return rep.SecurityCheckLog(orderNo, stat);
        }

        public virtual string OpenGate(string qrCode, string storeNo, string devLocation, string memberAccount, bool exitWithoutShopping, bool isEntering, Statistics stat)
        {
            return LSCentralWSBase.OpenGate(qrCode, storeNo, devLocation, memberAccount, exitWithoutShopping, isEntering, stat);
        }

        public virtual OrderCheck ScanPayGoOrderCheck(string documentId, Statistics stat)
        {
            return LSCentralWSBase.ScanPayGoOrderCheck(documentId, stat);
        }

        public virtual bool TokenEntrySet(ClientToken token, bool deleteToken, Statistics stat)
        {
            return LSCentralWSBase.TokenEntrySet(token, deleteToken, stat);
        }

        public virtual List<ClientToken> TokenEntryGet(string accountNo, bool hotelToken, Statistics stat)
        {
            return LSCentralWSBase.TokenEntryGet(accountNo, hotelToken, stat);
        }

        #endregion

        #region Contact

        public virtual MemberContact ContactCreate(MemberContact contact, Statistics stat)
        {
            return LSCentralWSBase.ContactCreate(contact, stat);
        }

        public virtual void ContactUpdate(MemberContact contact, string accountId, Statistics stat)
        {
            LSCentralWSBase.ContactUpdate(contact, accountId, stat);
        }

        public virtual MemberContact ContactGet(ContactSearchType searchType, string searchValue, Statistics stat)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            return rep.ContactGet(searchType, searchValue, stat);
        }

        public virtual List<Customer> CustomerSearch(CustomerSearchType searchType, string search, int maxNumberOfRowsReturned, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            CustomerRepository rep = new CustomerRepository(config);
            List<Customer> list = rep.CustomerSearch(searchType, search, maxNumberOfRowsReturned);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public virtual double ContactAddCard(string contactId, string accountId, string cardId, Statistics stat)
        {
            return LSCentralWSBase.ContactAddCard(contactId, accountId, cardId, stat);
        }

        public virtual void ConatctBlock(string accountId, string cardId, Statistics stat)
        {
            LSCentralWSBase.ConatctBlock(accountId, cardId, stat);
        }

        public virtual MemberContact Login(string userName, string password, string deviceID, string deviceName, Statistics stat)
        {
            return LSCentralWSBase.Logon(userName, password, deviceID, deviceName, stat);
        }

        public virtual MemberContact SocialLogon(string authenticator, string authenticationId, string deviceID, string deviceName, Statistics stat)
        {
            return LSCentralWSBase.SocialLogon(authenticator, authenticationId, deviceID, deviceName, stat);
        }

        //Change the password in NAV
        public virtual void ChangePassword(string userName, string token, string newPassword, string oldPassword, ref bool oldmethod, Statistics stat)
        {
            LSCentralWSBase.ChangePassword(userName, token, newPassword, oldPassword, stat);
        }

        public virtual string ResetPassword(string userName, string email, string newPassword, ref bool oldmethod, Statistics stat)
        {
            return LSCentralWSBase.ResetPassword(userName, email, stat);
        }

        public virtual string SPGPassword(string email, string token, string newPwd, Statistics stat)
        {
            return LSCentralWSBase.SPGPassword(email, token, newPwd, stat);
        }

        public virtual void LoginChange(string oldUserName, string newUserName, string password, Statistics stat)
        {
            LSCentralWSBase.LoginChange(oldUserName, newUserName, password, stat);
        }

        public virtual List<Profile> ProfileGetByCardId(string id, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            List<Profile> list = rep.ProfileGetByCardId(id);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public virtual List<Profile> ProfileGetAll(Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            List<Profile> list = rep.ProfileGetAll();
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public virtual List<Scheme> SchemeGetAll(Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            List<Scheme> list = rep.SchemeGetAll();
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public virtual Scheme SchemeGetById(string schemeId, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            if (schemeId.Equals("Ping"))
            {
                rep.SchemeGetById(schemeId);
                return new Scheme("NAV");
            }
            Scheme data = rep.SchemeGetById(schemeId);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        #endregion

        #region Device

        public virtual Device DeviceGetById(string id, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            Device data = rep.DeviceGetById(id);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public virtual bool IsUserLinkedToDeviceId(string userName, string deviceId, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            bool data = rep.IsUserLinkedToDeviceId(userName, deviceId);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        #endregion

        #region Search

        public virtual List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned, Statistics stat)
        {
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            return rep.ContactSearch(searchType, search, maxNumberOfRowsReturned, stat);
        }

        public virtual List<LoyItem> ItemsSearch(string search, string storeId, int maxNumberOfItems, bool includeDetails, Statistics stat)
        {
            ItemRepository rep = new ItemRepository(config, LSCVersion);
            return rep.ItemLoySearch(search, storeId, maxNumberOfItems, includeDetails, stat);
        }

        public virtual List<ItemCategory> ItemCategorySearch(string search, Statistics stat)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository(config, LSCVersion);
            return rep.ItemCategorySearch(search, stat);
        }

        public virtual List<ProductGroup> ProductGroupSearch(string search, Statistics stat)
        {
            ProductGroupRepository rep = new ProductGroupRepository(config, LSCVersion);
            return rep.ProductGroupSearch(search, stat);
        }

        public virtual List<Store> StoreLoySearch(string search, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            StoreRepository rep = new StoreRepository(config, LSCVersion);
            List<Store> list = rep.StoreLoySearch(search);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public virtual List<Profile> ProfileSearch(string cardId, string search, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            List<Profile> list = rep.ProfileSearch(cardId, search);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public virtual List<SalesEntry> SalesEntrySearch(string search, string cardId, int maxNumberOfTransactions, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            SalesEntryRepository rep = new SalesEntryRepository(config, LSCVersion);
            List<SalesEntry> list = rep.SalesEntrySearch(search, cardId, maxNumberOfTransactions);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        #endregion

        #region Card

        public virtual Card CardGetById(string id, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            Card data = rep.CardGetById(id);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public virtual long MemberCardGetPoints(string cardId, Statistics stat)
        {
            return LSCentralWSBase.MemberCardGetPoints(cardId, stat);
        }

        public virtual decimal GetPointRate(string currency, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);

            decimal rate = 0;
            CurrencyExchRateRepository rep = new CurrencyExchRateRepository(config);

            if (string.IsNullOrEmpty(currency) == false)
            {
                ReplCurrencyExchRate baseExchrate = rep.CurrencyExchRateGetById(currency);
                if (baseExchrate != null)
                    rate = 1 / baseExchrate.CurrencyFactor;
            }
            else
            {
                string loyCur = config.SettingsGetByKey(ConfigKey.Currency_LoyCode);
                if (string.IsNullOrEmpty(loyCur))
                    loyCur = "LOY";

                ReplCurrencyExchRate exchrate = rep.CurrencyExchRateGetById(loyCur);
                if (exchrate != null)
                    rate = exchrate.CurrencyFactor;
            }

            logger.StatisticEndSub(ref stat, index);
            return rate;
        }

        public virtual List<PointEntry> PointEntriesGet(string cardNo, DateTime dateFrom, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            List<PointEntry> list = rep.PointEntriesGet(cardNo, dateFrom);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo, int pin, string entryType, Statistics stat)
        {
            if (LSCVersion >= new Version("21.1"))
                return LSCentralWSBase.GiftCardGetBalance(cardNo, pin, entryType, stat);

            logger.StatisticStartSub(false, ref stat, out int index);
            ContactRepository rep = new ContactRepository(config, LSCVersion);
            GiftCard data = rep.GetGiftCartBalance(cardNo, entryType);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public virtual List<GiftCardEntry> GiftCardGetHistory(string cardNo, int pin, string entryType, Statistics stat)
        {
            return LSCentralWSBase.GiftCardGetHistory(cardNo, pin, entryType, stat);
        }

        #endregion

        #region Notification

        public virtual List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications, Statistics stat)
        {
            return LSCentralWSBase.NotificationsGetByCardId(cardId, stat);
        }

        #endregion

        #region Item

        public virtual LoyItem ItemGetById(string id, string storeId, string culture, bool includeDetails, Statistics stat)
        {
            ItemRepository rep = new ItemRepository(config, LSCVersion);
            return rep.ItemLoyGetById(id, storeId, culture, includeDetails, stat);
        }

        public virtual LoyItem ItemLoyGetByBarcode(string code, string storeId, string culture, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ItemRepository rep = new ItemRepository(config, LSCVersion);
            LoyItem item = rep.ItemLoyGetByBarcode(code, storeId, culture, stat);
            if (item != null)
            {
                logger.StatisticEndSub(ref stat, index);
                return item;
            }

            LoyItem bitem = LSCentralWSBase.ItemFindByBarcode(code, storeId, config.SettingsGetByKey(ConfigKey.ScanPayGo_Terminal), stat);
            item = rep.ItemLoyGetById(bitem.Id, storeId, string.Empty, true, stat);
            item.GrossWeight = bitem.GrossWeight;
            item.Price = bitem.Price;
            logger.StatisticEndSub(ref stat, index);
            return item;
        }

        public virtual List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            List<LoyItem> tmplist = LSCentralWSBase.ItemsGetByPublishedOfferId(pubOfferId, numberOfItems, stat);

            ItemRepository rep = new ItemRepository(config, LSCVersion);
            List<LoyItem> list = new List<LoyItem>();
            foreach (LoyItem item in tmplist)
            {
                list.Add(rep.ItemLoyGetById(item.Id, string.Empty, string.Empty, true, stat));
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public virtual List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, string storeId, bool includeDetails, Statistics stat)
        {
            ItemRepository rep = new ItemRepository(config, LSCVersion);
            return rep.ItemsPage(pageSize, pageNumber, itemCategoryId, productGroupId, search, storeId, includeDetails, stat);
        }

        public virtual UnitOfMeasure ItemUOMGetByIds(string itemid, string uomid, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ItemUOMRepository rep = new ItemUOMRepository(config);
            UnitOfMeasure data = rep.ItemUOMGetByIds(itemid, uomid);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public virtual List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items, Statistics stat)
        {
            return LSCentralWSBase.ItemCustomerPricesGet(storeId, cardId, items, stat);
        }

        #endregion

        #region ItemCategory and ProductGroup and Hierarchy

        public virtual List<ItemCategory> ItemCategoriesGet(string storeId, string culture, Statistics stat)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository(config, LSCVersion);
            return rep.ItemCategoriesGetAll(storeId, culture, stat);
        }

        public virtual ItemCategory ItemCategoriesGetById(string id, Statistics stat)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository(config, LSCVersion);
            return rep.ItemCategoryGetById(id, stat);
        }

        public virtual List<ProductGroup> ProductGroupGetByItemCategoryId(string itemcategoryId, string culture, bool includeChildren, bool includeItems, Statistics stat)
        {
            ProductGroupRepository rep = new ProductGroupRepository(config, LSCVersion);
            return rep.ProductGroupGetByItemCategoryId(itemcategoryId, culture, includeChildren, includeItems, stat);
        }

        public virtual ProductGroup ProductGroupGetById(string id, string culture, bool includeItems, bool includeItemDetail, Statistics stat)
        {
            ProductGroupRepository rep = new ProductGroupRepository(config, LSCVersion);
            return rep.ProductGroupGetById(id, culture, includeItems, includeItemDetail, stat);
        }

        public virtual List<Hierarchy> HierarchyGet(string storeId, Statistics stat)
        {
            HierarchyRepository rep = new HierarchyRepository(config);
            return rep.HierarchyGetByStore(storeId, stat);
        }

        public virtual MobileMenu MenuGet(string storeId, string salesType, Currency currency, Statistics stat)
        {
            return LSCentralWSBase.MenuGet(storeId, salesType, currency);
        }

        #endregion

        #region Transaction

        public virtual string FormatAmount(decimal amount, string culture)
        {
            SalesEntryRepository rep = new SalesEntryRepository(config, LSCVersion);
            return rep.FormatAmountToString(amount, culture);
        }

        #endregion

        #region Hospitality Order

        public virtual OrderHosp HospOrderCalculate(OneList list, Statistics stat)
        {
            return LSCentralWSBase.HospOrderCalculate(list, stat);
        }

        public virtual string HospOrderCreate(OrderHosp request, Statistics stat)
        {
            return LSCentralWSBase.HospOrderCreate(request, stat);
        }

        public virtual void HospOrderCancel(string storeId, string orderId, Statistics stat)
        {
            LSCentralWSBase.HospOrderCancel(storeId, orderId, stat);
        }

        public virtual OrderHospStatus HospOrderStatus(string storeId, string orderId, Statistics stat)
        {
            return LSCentralWSBase.HospOrderKotStatus(storeId, orderId, stat);
        }

        public virtual List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            InvStatusRepository rep = new InvStatusRepository(config, LSCVersion);
            List<HospAvailabilityResponse> list = rep.CheckAvailability(request, storeId);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        #endregion

        #region Basket

        public virtual Order BasketCalcToOrder(OneList list, Statistics stat)
        {
            return LSCentralWSBase.BasketCalcToOrder(list, stat);
        }

        #endregion

        #region Order

        public bool CompressCOActive(Statistics stat)
        {
            return false;
        }

        public virtual OrderStatusResponse OrderStatusCheck(string orderId, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            OrderRepository repo = new OrderRepository(config, LSCVersion);
            OrderStatusResponse data = repo.OrderStatusGet(orderId);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public virtual void OrderCancel(string orderId, string storeId, string userId, List<OrderCancelLine> lines, Statistics stat)
        {
            LSCentralWSBase.OrderCancel(orderId, storeId, userId, lines, stat);
        }

        public virtual OrderAvailabilityResponse OrderAvailabilityCheck(OneList request, bool shippingOrder, Statistics stat)
        {
            return LSCentralWSBase.OrderAvailabilityCheck(request, shippingOrder, stat);
        }

        public virtual string OrderCreate(Order request, out string orderId, Statistics stat)
        {
            if (request.OrderType == OrderType.ScanPayGoSuspend)
            {
                orderId = string.Empty;
                return LSCentralWSBase.ScanPayGoSuspend(request, out orderId, stat);
            }

            return LSCentralWSBase.OrderCreate(request, out orderId, stat);
        }

        public virtual string OrderEdit(Order request, ref string orderId, OrderEditType editType, Statistics stat)
        {
            return LSCentralWSBase.OrderEdit(request, ref orderId, editType, stat);
        }

        public virtual SalesEntry SalesEntryGet(string entryId, DocumentIdType type, Statistics stat)
        {
            SalesEntry entry;
            if (type == DocumentIdType.Receipt)
            {
                SalesEntryRepository trepo = new SalesEntryRepository(config, LSCVersion);
                entry = trepo.SalesEntryGetById(entryId, string.Empty, string.Empty, 0, stat);
            }
            else if (type == DocumentIdType.HospOrder)
            {
                SalesEntryRepository trepo = new SalesEntryRepository(config, LSCVersion);
                entry = trepo.POSTransactionGetById(entryId, stat);
                if (entry == null)
                    entry = trepo.SalesEntryGetById(entryId, string.Empty, string.Empty, 0, stat);
            }
            else
            {
                OrderRepository repo = new OrderRepository(config, LSCVersion);
                entry = repo.OrderGetById(entryId, true, (type == DocumentIdType.External), stat);
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
            logger.StatisticStartSub(false, ref stat, out int index);
            SalesEntryRepository repo = new SalesEntryRepository(config, LSCVersion);
            List<SalesEntryId> list = repo.SalesEntryGetReturnSales(receiptNo);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public virtual SalesEntryList SalesEntryGetSalesByOrderId(string orderId, Statistics stat)
        {
            SalesEntryRepository repo = new SalesEntryRepository(config, LSCVersion);
            OrderRepository orepo = new OrderRepository(config, LSCVersion);
            SalesEntryList data = new SalesEntryList();
            data.OrderId = orderId;
            SalesEntry order = orepo.OrderGetById(orderId, false, false, stat);
            data.CardId = order.CardId;
            data.SalesEntries = repo.SalesEntryGetSalesByOrderId(orderId, stat);
            data.Shipments = repo.SalesEntryShipmentGet(orderId, stat);
            return data;
        }

        public virtual List<SalesEntry> SalesEntriesGetByCardId(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNumberOfEntries, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            SalesEntryRepository repo = new SalesEntryRepository(config, LSCVersion);
            List<SalesEntry> data = repo.SalesEntriesByCardId(cardId, storeId, date, dateGreaterThan, maxNumberOfEntries);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        #endregion

        #region Offer and Advertisement

        public virtual List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId, Statistics stat)
        {
            return LSCentralWSBase.PublishedOffersGet(cardId, itemId, storeId, stat);
        }

        #endregion

        #region Image

        public virtual ImageView ImageGetById(string imageId, bool includeBlob, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ImageRepository rep = new ImageRepository(config, LSCVersion);
            ImageView img = rep.ImageGetById(imageId, includeBlob);
            logger.StatisticEndSub(ref stat, index);
            return img;
        }

        public virtual ImageView ImageGetByMediaId(string mediaId, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ImageRepository rep = new ImageRepository(config, LSCVersion);
            ImageView data = rep.ImageMediaGetById(mediaId);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public virtual List<ImageView> ImagesGetByKey(string tableName, string key1, string key2, string key3, int imgCount, bool includeBlob, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ImageRepository rep = new ImageRepository(config, LSCVersion);
            List<ImageView> list = rep.ImageGetByKey(tableName, key1, key2, key3, imgCount, includeBlob);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        #endregion

        #region Store

        private List<StoreHours> StoreHoursGetByStoreId(string storeId, Statistics stat)
        {
            int offset = config.SettingsIntGetByKey(ConfigKey.Timezone_HoursOffset);
            return LSCentralWSBase.StoreHoursGetByStoreId(storeId, offset, stat);
        }

        public virtual Store StoreGetById(string id, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            StoreRepository rep = new StoreRepository(config, LSCVersion);
            AttributeValueRepository arep = new AttributeValueRepository(config);
            Store store = rep.StoreLoyGetById(id, true);
            if (store != null)
            {
                store.StoreHours = StoreHoursGetByStoreId(id, stat);
                store.Attributes = arep.AttributesGet(id, AttributeLinkType.Store, stat);
            }
            logger.StatisticEndSub(ref stat, index);
            return store;
        }

        public virtual List<Store> StoresGetAll(StoreGetType storeType, bool inclDetails, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            StoreRepository rep = new StoreRepository(config, LSCVersion);
            AttributeValueRepository arep = new AttributeValueRepository(config);
            List<Store> stores = rep.StoreLoyGetAll(storeType, inclDetails);
            if (inclDetails)
            {
                foreach (Store store in stores)
                {
                    store.StoreHours = StoreHoursGetByStoreId(store.Id, stat);
                    store.Attributes = arep.AttributesGet(store.Id, AttributeLinkType.Store, stat);
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return stores;
        }

        public virtual List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            StoreRepository rep = new StoreRepository(config, LSCVersion);
            List<ReturnPolicy> list = rep.ReturnPolicyGet(storeId, storeGroupCode, itemCategory, productGroup, itemId, variantCode, variantDim1);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        #endregion

        #region EComm Replication

        public virtual List<ReplImageLink> ReplEcommImageLinks(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ImageRepository rep = new ImageRepository(config, LSCVersion);
            return rep.ReplEcommImageLink(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplImage> ReplEcommImages(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ImageRepository rep = new ImageRepository(config, LSCVersion);
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

        public virtual List<ReplDataTranslation> ReplEcommItemHtmlTranslation(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            DataTranslationRepository rep = new DataTranslationRepository(config);
            return rep.ReplicateEcommItemHtmlTranslation(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDataTranslation> ReplEcommDealHtmlTranslation(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            DataTranslationRepository rep = new DataTranslationRepository(config);
            return rep.ReplicateEcommDealHtmlTranslation(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
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
