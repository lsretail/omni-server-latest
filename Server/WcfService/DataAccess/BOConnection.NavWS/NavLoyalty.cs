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
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

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
            MemberContact contact = NavWSBase.ContactGet(string.Empty, string.Empty, card, includeDetails);

            if (numberOfTrans > 0 && contact != null)
            {
                contact.SalesEntries = SalesEntriesGetByCardId(card, numberOfTrans, string.Empty);
                contact.SalesEntries.AddRange(OrderHistoryByCardId(card));
            }
            return contact;
        }

        public virtual MemberContact ContactGetByUserName(string user, bool includeDetails)
        {
            return NavWSBase.ContactGetByUserName(user, includeDetails);
        }

        public virtual MemberContact ContactGetByEMail(string email, bool includeDetails)
        {
            return NavWSBase.ContactGetByEmail(email, includeDetails);
        }

        public virtual double ContactAddCard(string contactId, string accountId, string cardId)
        {
            return ContactAddCard(contactId, accountId, cardId);
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
            return NavWSBase.DeviceGetById(id);
        }

        public virtual bool IsUserLinkedToDeviceId(string userName, string deviceId)
        {
            return true;
        }

        #endregion

        #region Search

        public virtual List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned)
        {
            return NavWSBase.ContactSearch(searchType, search, maxNumberOfRowsReturned);
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

        public virtual List<Profile> ProfileSearch(string contactId, string search)
        {
            return NavWSBase.ProfileSearch(search);
        }

        public virtual List<SalesEntry> SalesEntrySearch(string search, string cardId, int maxNumberOfTransactions, bool includeLines)
        {
            throw new NotImplementedException("NEEDS NAV WS");
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
            return NavWSBase.NotificationsGetByCardId(cardId, numberOfNotifications);
        }

        #endregion

        #region Item

        public virtual LoyItem ItemLoyGetByBarcode(string code, string storeId, string culture)
        {
            return NavWSBase.ItemGetByBarcode(code, storeId);
        }

        public virtual LoyItem ItemGetById(string id, string storeId, string culture, bool includeDetails)
        {
            return NavWSBase.ItemGetById(id, storeId, culture, includeDetails);
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
            return new List<LoyItem>();
        }

        public virtual UnitOfMeasure ItemUOMGetByIds(string itemid, string uomid)
        {
            return null;
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

        #endregion

        #region Transaction

        public virtual List<SalesEntry> SalesEntriesGetByCardId(string cardId, int maxNumberOfTransactions, string culture)
        {
            return NavWSBase.SalesHistory(cardId, maxNumberOfTransactions);
        }

        public virtual SalesEntry SalesEntryGet(string entryId, DocumentIdType type, string tenderMapping)
        {
            if (type == DocumentIdType.Receipt)
            {
                //NavWSBase.TransactionByReceiptNumber(string.Empty, string.Empty, entryId, string.Empty);
            }

            return NavWSBase.OrderGet(entryId);
        }

        public virtual string FormatAmount(decimal amount, string culture)
        {
            return amount.ToString();
        }

        #endregion

        #region Basket

        public virtual Order BasketCalcToOrder(OneList list)
        {
            return NavWSBase.BasketCalcToOrder(list);
        }

        #endregion

        #region Order

        public virtual OrderStatusResponse OrderStatusCheck(string transactionId)
        {
            return NavWSBase.OrderStatusCheck(transactionId);
        }

        public virtual void OrderCancel(string transactionId)
        {
            NavWSBase.OrderCancel(transactionId);
        }

        public virtual OrderAvailabilityResponse OrderAvailabilityCheck(OneList request)
        {
            return NavWSBase.OrderAvailabilityCheck(request);
        }

        public virtual string OrderCreate(Order request, string tenderMapping, out string orderId)
        {
            return NavWSBase.OrderCreate(request, tenderMapping, out orderId);
        }

        public virtual List<SalesEntry> OrderHistoryByCardId(string cardId)
        {
            return NavWSBase.OrderHistoryGet(cardId);
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

        #region Menu

        public virtual MobileMenu MenusGet(string storeId, Currency currency)
        {
            return NavWSBase.MenusGet(storeId, currency);
        }

        #endregion

        #region Image

        public virtual ImageView ImageBOGetById(string imageId)
        {
            return NavWSBase.ImageGetById(imageId);
        }

        public virtual List<ImageView> ImageBOGetByKey(string tableName, string key1, string key2, string key3, int imgCount, bool includeBlob)
        {
            return NavWSBase.ImagesGetByLink(tableName, key1, key2, key3);
        }

        #endregion

        #region Store

        public virtual string GetWIStoreId()
        {
            return string.Empty;
        }

        public virtual Store StoreGetById(string id)
        {
            int offset = config.SettingsIntGetByKey(ConfigKey.Timezone_HoursOffset);

            Store store = NavWSBase.StoreGetById(id);
            store.StoreHours = NavWSBase.StoreHoursGetByStoreId(id, offset);
            return store;
        }

        public virtual List<Store> StoresLoyGetByCoordinates(double latitude, double longitude, double maxDistance, int maxNumberOfStores, Store.DistanceType units)
        {
            //only loy app?
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

        #endregion

        #region Ecomm Replication

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
            throw new NotImplementedException("IS THIS NEEDED?");
        }

        public virtual List<ReplDataTranslation> ReplEcommDataTranslation(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplEcommDataTranslation(string.Empty, string.Empty, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
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

        public virtual List<ReplInvStatus> ReplEcommInventoryStatus(string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplEcommInventoryStatus(string.Empty, string.Empty, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<LoyItem> ReplEcommFullItem(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            //Avensia special request
            throw new NotImplementedException("Not supported in WS Mode");
        }

        #endregion

        private List<ShippingAgentService> GetShippingAgentService(string agentId)
        {
            return NavWSBase.GetShippingAgentService(agentId);
        }

        private string TenderTypeMapping(string tenderMapping, string tenderType, bool toOmni)
        {
            try
            {
                int tenderTypeId = -1;
                if (string.IsNullOrWhiteSpace(tenderMapping))
                {
                    return null;
                }

                // first one is Omni TenderType, 2nd one is the NAV id
                //tenderMapping: "1=1,2=2,3=3,4=4,6=6,7=7,8=8,9=9,10=10,11=11,15=15,19=19"
                //or can be : "1  =  1  ,2=2,3= 3, 4=4,6 =6,7=7,8=8,9=9,10=10,11=11,15=15,19=19"

                string[] commaMapping = tenderMapping.Split(',');  //1=1 or 2=2  etc
                foreach (string s in commaMapping)
                {
                    string[] eqMapping = s.Split('='); //1 1
                    if (toOmni)
                    {
                        if (tenderType == eqMapping[1].Trim())
                        {
                            tenderTypeId = Convert.ToInt32(eqMapping[0].Trim());
                            break;
                        }
                    }
                    else
                    {
                        if (tenderType == eqMapping[0].Trim())
                        {
                            tenderTypeId = Convert.ToInt32(eqMapping[1].Trim());
                            break;
                        }
                    }
                }
                return tenderTypeId.ToString();
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error in TenderTypeMapping(tenderMapping:{0} tenderType:{1})", tenderMapping, tenderType.ToString());
                logger.Error(config.LSKey.Key, msg + ex.Message);
                throw;
            }
        }
    }
}
