using System;
using System.Collections.Generic;

using LSOmni.DataAccess.Interface.BOConnection;

using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
using System.Linq;

namespace LSOmni.DataAccess.BOConnection.NavWS
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
            string ver = NavWSBase.NavVersionToUse(true, true);
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
            MemberContact contact = NavWSBase.ContactGet(string.Empty, string.Empty, card, string.Empty, string.Empty, includeDetails);

            if (numberOfTrans > 0 && contact != null)
            {
                contact.SalesEntries = SalesEntriesGetByCardId(card, string.Empty, DateTime.MinValue, false, numberOfTrans);
            }
            return contact;
        }

        public virtual MemberContact ContactGetByUserName(string user, bool includeDetails)
        {
            if (NAVVersion < new Version("16.2"))
                return NavWSBase.ContactGetByUserName(user, includeDetails);

            return NavWSBase.ContactGet(string.Empty, string.Empty, string.Empty, user, string.Empty, includeDetails);
        }

        public virtual MemberContact ContactGet(ContactSearchType searchType, string searchValue)
        {
            if (NAVVersion < new Version("16.2"))
                return NavWSBase.ContactGetByEmail(searchValue, false);

            switch (searchType)
            {
                case ContactSearchType.Email:
                    return NavWSBase.ContactGet(string.Empty, string.Empty, string.Empty, string.Empty, searchValue, false);
                case ContactSearchType.CardId:
                    return NavWSBase.ContactGet(string.Empty, string.Empty, searchValue, string.Empty, string.Empty, false);
                case ContactSearchType.UserName:
                    return NavWSBase.ContactGet(string.Empty, string.Empty, string.Empty, searchValue, string.Empty, false);
                case ContactSearchType.ContactNumber:
                    return NavWSBase.ContactSearch(ContactSearchType.ContactNumber, searchValue, 1).FirstOrDefault();
            }
            return null;
        }

        public virtual double ContactAddCard(string contactId, string accountId, string cardId)
        {
            return ContactAddCard(contactId, accountId, cardId);
        }

        public MemberContact Login(string userName, string password, string deviceID, string deviceName, bool includeDetails)
        {
            return NavWSBase.Logon(userName, password, deviceID, deviceName, includeDetails);
        }

        //Change the password in NAV
        public virtual void ChangePassword(string userName, string token, string newPassword, string oldPassword)
        {
            NavWSBase.ChangePassword(userName, token, newPassword, oldPassword);
        }

        public virtual string ResetPassword(string userName, string email, string newPassword)
        {
            return NavWSBase.ResetPassword(userName, email, newPassword);
        }

        public virtual void LoginChange(string oldUserName, string newUserName, string password)
        {
            NavWSBase.LoginChange(oldUserName, newUserName, password);
        }

        public virtual List<Profile> ProfileGetByCardId(string id)
        {
            return NavWSBase.ProfileGetAll();
        }

        public virtual List<Profile> ProfileGetAll()
        {
            return ProfileGetByCardId(string.Empty);
        }

        public virtual List<Scheme> SchemeGetAll()
        {
            return NavWSBase.SchemeGetAll();
        }

        public virtual Scheme SchemeGetById(string schemeId)
        {
            if (schemeId.Equals("Ping"))
            {
                return new Scheme("NAV");
            }
            return new Scheme();
        }

        #endregion

        #region Device

        public virtual Device DeviceGetById(string id)
        {
            return NavWSBase.DeviceGetById(id);
        }

        public virtual bool IsUserLinkedToDeviceId(string userName, string deviceId)
        {
            return true;
        }

        #endregion

        #region Search

        public virtual List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned, bool exact)
        {
            List<MemberContact> list = new List<MemberContact>();
            MemberContact cont;
            switch (searchType)
            {
                case ContactSearchType.CardId:
                    cont = NavWSBase.ContactGet(string.Empty, string.Empty, search, string.Empty, string.Empty, false);
                    if (cont != null)
                        list.Add(cont);
                    break;
                case ContactSearchType.UserName:
                    cont = NavWSBase.ContactGet(string.Empty, string.Empty, string.Empty, search, string.Empty, false);
                    if (cont != null)
                        list.Add(cont);
                    break;
                default:
                    if (exact == false)
                        search = "*" + search + "*";

                    list = NavWSBase.ContactSearch(searchType, search, maxNumberOfRowsReturned);
                    break;
            }
            return list;
        }

        public virtual List<LoyItem> ItemsSearch(string search, string storeId, int maxNumberOfItems, bool includeDetails)
        {
            return NavWSBase.ItemSearch(search);
        }

        public virtual List<ItemCategory> ItemCategorySearch(string search)
        {
            return NavWSBase.ItemCategorySearch(search);
        }

        public virtual List<ProductGroup> ProductGroupSearch(string search)
        {
            return NavWSBase.ProductGroupSearch(search);
        }

        public virtual List<Store> StoreLoySearch(string search)
        {
            return NavWSBase.StoreSearch(search);
        }

        public virtual List<Profile> ProfileSearch(string cardId, string search)
        {
            return NavWSBase.ProfileSearch(search);
        }

        public virtual List<SalesEntry> SalesEntrySearch(string search, string cardId, int maxNumberOfTransactions)
        {
            return new List<SalesEntry>()
            {
                new SalesEntry()
            };
        }

        #endregion

        #region Card

        public virtual Card CardGetById(string id)
        {
            return null;
        }

        public virtual long MemberCardGetPoints(string cardId)
        {
            return NavWSBase.MemberCardGetPoints(cardId);
        }

        public virtual decimal GetPointRate()
        {
            return NavWSBase.GetPointRate();
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo, string entryType)
        {
            return NavWSBase.GiftCardGetBalance(cardNo, entryType);
        }

        #endregion

        #region Notification

        public virtual List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications)
        {
            return NavWSBase.NotificationsGetByCardId(cardId);
        }

        #endregion

        #region Item

        public virtual LoyItem ItemLoyGetByBarcode(string code, string storeId, string culture)
        {
            return NavWSBase.ItemGetByBarcode(code);
        }

        public virtual LoyItem ItemGetById(string id, string storeId, string culture, bool includeDetails)
        {
            return NavWSBase.ItemGetById(id);
        }

        public virtual List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems)
        {
            List<LoyItem> list = new List<LoyItem>();

            List<LoyItem> tmplist = NavWSBase.ItemsGetByPublishedOfferId(pubOfferId, numberOfItems);
            foreach (LoyItem item in tmplist)
            {
                list.Add(new LoyItem(item.Id));
            }
            return list;
        }

        public virtual List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, string storeId, bool includeDetails)
        {
            return new List<LoyItem>()
            {
                new LoyItem()
            };
        }

        public virtual UnitOfMeasure ItemUOMGetByIds(string itemid, string uomid)
        {
            return null;
        }

        public virtual List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items)
        {
            return NavWSBase.ItemCustomerPricesGet(storeId, cardId, items);
        }

        #endregion

        #region ItemCategory and ProductGroup and Hierarchy

        public virtual List<ItemCategory> ItemCategoriesGet(string storeId, string culture)
        {
            return NavWSBase.ItemCategories();
        }

        public virtual ItemCategory ItemCategoriesGetById(string id)
        {
            ItemCategory icat = NavWSBase.ItemCategoriesGetById(id);
            icat.ProductGroups = NavWSBase.ProductGroupGetByItemCategory(icat.Id);
            return icat;
        }

        public virtual List<ProductGroup> ProductGroupGetByItemCategoryId(string itemcategoryId, string culture, bool includeChildren, bool includeItems)
        {
            return null;
        }

        public virtual ProductGroup ProductGroupGetById(string id, string culture, bool includeItems, bool includeItemDetail)
        {
            ProductGroup pgrp = NavWSBase.ProductGroupGetById(id);
            pgrp.Items = NavWSBase.ItemsGetByProductGroup(pgrp.Id);
            return pgrp;
        }

        public virtual List<Hierarchy> HierarchyGet(string storeId)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error(config.LSKey.Key, "Only supported in NAV 10.x and later");
                return new List<Hierarchy>();
            }

            return NavWSBase.HierarchyGet(storeId);
        }

        public virtual MobileMenu MenuGet(string storeId, string salesType, Currency currency)
        {
            return NavWSBase.MenuGet(storeId, salesType, currency);
        }

        #endregion

        #region Transaction

        public virtual List<SalesEntry> SalesEntriesGetByCardId(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNumberOfEntries)
        {
            List<SalesEntry> list = NavWSBase.SalesHistory(cardId, maxNumberOfEntries);
            list.AddRange(NavWSBase.OrderHistoryGet(cardId));
            return list;
        }

        public virtual SalesEntry SalesEntryGet(string entryId, DocumentIdType type, string tenderMapping)
        {
            if (type == DocumentIdType.Receipt)
                return NavWSBase.TransactionGet(entryId, string.Empty, string.Empty, 0);

            return NavWSBase.OrderGet(entryId);
        }

        public virtual string FormatAmount(decimal amount, string culture)
        {
            return amount.ToString();
        }

        #endregion

        #region Hospitality Order

        public virtual OrderHosp HospOrderCalculate(OneList list)
        {
            return NavWSBase.HospOrderCalculate(list);
        }

        public virtual string HospOrderCreate(OrderHosp request, string tenderMapping)
        {
            return NavWSBase.HospOrderCreate(request, tenderMapping);
        }

        public virtual int HospOrderEstimatedTime(string storeId, string orderId)
        {
            return NavWSBase.HospOrderEstimatedTime(storeId, orderId);
        }

        public virtual void HospOrderCancel(string storeId, string orderId)
        {
            NavWSBase.HospOrderCancel(storeId, orderId);
        }

        public virtual OrderHospStatus HospOrderKotStatus(string storeId, string orderId)
        {
            return NavWSBase.HospOrderKotStatus(storeId, orderId);
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
            return NavWSBase.ImageGetById(imageId);
        }

        public virtual List<ImageView> ImagesGetByKey(string tableName, string key1, string key2, string key3, int imgCount, bool includeBlob)
        {
            return NavWSBase.ImagesGetByLink(tableName, key1, key2, key3);
        }

        #endregion

        #region Store

        public virtual Store StoreGetById(string id)
        {
            int offset = config.SettingsIntGetByKey(ConfigKey.Timezone_HoursOffset);

            Store store = NavWSBase.StoreGetById(id);
            store.StoreHours = NavWSBase.StoreHoursGetByStoreId(id, offset);
            return store;
        }

        public virtual List<Store> StoresLoyGetByCoordinates(double latitude, double longitude, double maxDistance, int maxNumberOfStores, Store.DistanceType units)
        {
            throw new NotImplementedException("IS THIS NEEDED?");
        }

        public virtual List<Store> StoresGetAll(bool clickAndCollectOnly)
        {
            int offset = config.SettingsIntGetByKey(ConfigKey.Timezone_HoursOffset);

            List<Store> stores = NavWSBase.StoresGet(clickAndCollectOnly);
            foreach (Store store in stores)
            {
                store.StoreHours = NavWSBase.StoreHoursGetByStoreId(store.Id, offset);
            }
            return stores;
        }

        public virtual List<StoreServices> StoreServicesGetByStoreId(string storeId)
        {
            return NavWSBase.StoreServicesGetByStoreId(storeId);
        }

        public virtual List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1)
        {
            return NavWSBase.ReturnPolicyGet(storeId, storeGroupCode, itemCategory, productGroup, itemId, variantCode, variantDim1);
        }


        #endregion

        #region EComm Replication

        public virtual List<ReplImageLink> ReplEcommImageLinks(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplEcommImageLinks(string.Empty, string.Empty, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplImage> ReplEcommImages(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplEcommImages(string.Empty, string.Empty, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplAttribute> ReplEcommAttribute(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplEcommAttribute(string.Empty, string.Empty, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplAttributeValue> ReplEcommAttributeValue(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplEcommAttributeValue(string.Empty, string.Empty, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplAttributeOptionValue> ReplEcommAttributeOptionValue(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplEcommAttributeOptionValue(string.Empty, string.Empty, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplLoyVendorItemMapping> ReplEcommVendorItemMapping(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return new List<ReplLoyVendorItemMapping>();
        }

        public virtual List<ReplDataTranslation> ReplEcommDataTranslation(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplEcommDataTranslation(string.Empty, string.Empty, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDataTranslationLangCode> ReplicateEcommDataTranslationLangCode(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateEcommDataTranslationLangCode(string.Empty, string.Empty, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplShippingAgent> ReplEcommShippingAgent(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplEcommShippingAgent(string.Empty, string.Empty, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCustomer> ReplEcommMember(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplEcommMember(string.Empty, string.Empty, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCountryCode> ReplEcommCountryCode(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplEcommCountryCode(string.Empty, string.Empty, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplInvStatus> ReplEcommInventoryStatus(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplEcommInventoryStatus(string.Empty, string.Empty, storeId, fullReplication, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<LoyItem> ReplEcommFullItem(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            // Avensia special request
            throw new NotImplementedException("Not supported in WS Mode");
        }

        #endregion
    }
}
