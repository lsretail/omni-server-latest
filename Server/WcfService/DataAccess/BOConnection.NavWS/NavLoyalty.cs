using System;
using System.Linq;
using System.Collections.Generic;

using LSOmni.Common.Util;
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
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Checkout;

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
            string ver;
            if (NAVVersion < new Version("17.5"))
                ver = NavWSBase.NavVersionToUse(true, true);
            else
                ver = LSCWSBase.NavVersionToUse();

            if (ver.Contains("ERROR"))
                throw new ApplicationException(ver);

            return ver;
        }

        #region ScanPayGo

        public virtual ScanPayGoProfile ScanPayGoProfileGet(string profileId, string storeNo)
        {
            if (NAVVersion < new Version("17.5"))
                return new ScanPayGoProfile();

            return LSCWSBase.ScanPayGoProfileGet(profileId, storeNo);
        }

        public virtual bool SecurityCheckProfile(string orderNo, string storeNo)
        {
            if (NAVVersion < new Version("17.5"))
                return false;

            return LSCWSBase.SecurityCheckProfile(orderNo, storeNo);
        }

        public virtual string OpenGate(string qrCode, string storeNo, string devLocation, string memberAccount, bool exitWithoutShopping, bool isEntering)
        {
            if (NAVVersion < new Version("17.5"))
            {
                return "Not Supported";
            }

            return LSCWSBase.OpenGate(qrCode, storeNo, devLocation, memberAccount, exitWithoutShopping, isEntering);
        }

        public virtual OrderCheck ScanPayGoOrderCheck(string documentId)
        {
            if (NAVVersion < new Version("17.5"))
            {
                return new OrderCheck();
            }

            return LSCWSBase.ScanPayGoOrderCheck(documentId);
        }

        #endregion

        #region Contact

        public virtual string ContactCreate(MemberContact contact)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ContactCreate(contact);

            return LSCWSBase.ContactCreate(contact);
        }

        public virtual void ContactUpdate(MemberContact contact, string accountId)
        {
            if (NAVVersion < new Version("17.5"))
                NavWSBase.ContactUpdate(contact, accountId);

            LSCWSBase.ContactUpdate(contact, accountId);
        }

        public virtual MemberContact ContactGetByCardId(string card, int numberOfTrans, bool includeDetails)
        {
            MemberContact contact;
            if (NAVVersion < new Version("17.5"))
                contact = NavWSBase.ContactGet(string.Empty, string.Empty, card, string.Empty, string.Empty, includeDetails);
            else
                contact = LSCWSBase.ContactGet(string.Empty, string.Empty, card, string.Empty, string.Empty, includeDetails);

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

            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ContactGet(string.Empty, string.Empty, string.Empty, user, string.Empty, includeDetails);

            return LSCWSBase.ContactGet(string.Empty, string.Empty, string.Empty, user, string.Empty, includeDetails);
        }

        public virtual MemberContact ContactGet(ContactSearchType searchType, string searchValue)
        {
            if (NAVVersion < new Version("16.2"))
                return NavWSBase.ContactGetByEmail(searchValue, false);

            if (NAVVersion < new Version("17.5"))
            {
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
            }

            switch (searchType)
            {
                case ContactSearchType.Email:
                    return LSCWSBase.ContactGet(string.Empty, string.Empty, string.Empty, string.Empty, searchValue, false);
                case ContactSearchType.CardId:
                    return LSCWSBase.ContactGet(string.Empty, string.Empty, searchValue, string.Empty, string.Empty, false);
                case ContactSearchType.UserName:
                    return LSCWSBase.ContactGet(string.Empty, string.Empty, string.Empty, searchValue, string.Empty, false);
                case ContactSearchType.ContactNumber:
                    return LSCWSBase.ContactSearch(ContactSearchType.ContactNumber, searchValue, 1).FirstOrDefault();
            }
            return null;
        }

        public virtual double ContactAddCard(string contactId, string accountId, string cardId)
        {
            return ContactAddCard(contactId, accountId, cardId);
        }

        public virtual MemberContact Login(string userName, string password, string deviceID, string deviceName, bool includeDetails)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.Logon(userName, password, deviceID, deviceName, includeDetails);

            return LSCWSBase.Logon(userName, password, deviceID, deviceName, includeDetails);
        }

        public virtual MemberContact SocialLogon(string authenticator, string authenticationId, string deviceID, string deviceName, bool includeDetails)
        {
            if (NAVVersion < new Version("17.5"))
                throw new NotImplementedException();

            return LSCWSBase.SocialLogon(authenticator, authenticationId, deviceID, deviceName, includeDetails);
        }

        //Change the password in NAV
        public virtual void ChangePassword(string userName, string token, string newPassword, string oldPassword, ref bool oldmethod)
        {
            if (NAVVersion < new Version("17.5"))
                NavWSBase.ChangePassword(userName, token, newPassword, oldPassword, ref oldmethod);

            LSCWSBase.ChangePassword(userName, token, newPassword, oldPassword);
        }

        public virtual string ResetPassword(string userName, string email, string newPassword, ref bool oldmethod)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ResetPassword(userName, email, newPassword, ref oldmethod);

            return LSCWSBase.ResetPassword(userName, email);
        }

        public virtual string SPGPassword(string email, string token, string newPwd)
        {
            if (NAVVersion < new Version("17.5"))
                return string.Empty;

            return LSCWSBase.SPGPassword(email, token, newPwd);
        }

        public virtual void LoginChange(string oldUserName, string newUserName, string password)
        {
            if (NAVVersion < new Version("17.5"))
                NavWSBase.LoginChange(oldUserName, newUserName, password);

            LSCWSBase.LoginChange(oldUserName, newUserName, password);
        }

        public virtual List<Profile> ProfileGetByCardId(string id)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ProfileGetAll();

            return LSCWSBase.ProfileGetAll();
        }

        public virtual List<Profile> ProfileGetAll()
        {
            return ProfileGetByCardId(string.Empty);
        }

        public virtual List<Scheme> SchemeGetAll()
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.SchemeGetAll();

            return LSCWSBase.SchemeGetAll();
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
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.DeviceGetById(id);

            return LSCWSBase.DeviceGetById(id);
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
                    if (NAVVersion < new Version("17.5"))
                        cont = NavWSBase.ContactGet(string.Empty, string.Empty, search, string.Empty, string.Empty, false);
                    else
                        cont = LSCWSBase.ContactGet(string.Empty, string.Empty, search, string.Empty, string.Empty, false);

                    if (cont != null)
                        list.Add(cont);
                    break;
                case ContactSearchType.UserName:
                    if (NAVVersion < new Version("17.5"))
                        cont = NavWSBase.ContactGet(string.Empty, string.Empty, string.Empty, search, string.Empty, false);
                    else
                        cont = LSCWSBase.ContactGet(string.Empty, string.Empty, string.Empty, search, string.Empty, false);

                    if (cont != null)
                        list.Add(cont);
                    break;
                default:
                    if (exact == false)
                        search = "*" + search + "*";

                    if (NAVVersion < new Version("17.5"))
                    {
                        List<MemberContact> tmplist = NavWSBase.ContactSearch(searchType, search, maxNumberOfRowsReturned);
                        foreach (MemberContact c in tmplist)
                        {
                            cont = NavWSBase.ContactGet(c.Id, c.Account.Id, string.Empty, string.Empty, string.Empty, false);
                            if (cont != null)
                                list.Add(cont);
                        }
                    }
                    else
                    {
                        List<MemberContact> tmplist = LSCWSBase.ContactSearch(searchType, search, maxNumberOfRowsReturned);
                        foreach (MemberContact c in tmplist)
                        {
                            cont = LSCWSBase.ContactGet(c.Id, c.Account.Id, string.Empty, string.Empty, string.Empty, false);
                            if (cont != null)
                                list.Add(cont);
                        }
                    }
                    break;
            }
            return list;
        }

        public virtual List<LoyItem> ItemsSearch(string search, string storeId, int maxNumberOfItems, bool includeDetails)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemSearch(search);

            return LSCWSBase.ItemSearch(search);
        }

        public virtual List<ItemCategory> ItemCategorySearch(string search)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemCategorySearch(search);

            return LSCWSBase.ItemCategorySearch(search);
        }

        public virtual List<ProductGroup> ProductGroupSearch(string search)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ProductGroupSearch(search);

            return LSCWSBase.ProductGroupSearch(search);
        }

        public virtual List<Store> StoreLoySearch(string search)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.StoreSearch(search);

            return LSCWSBase.StoreSearch(search);
        }

        public virtual List<Profile> ProfileSearch(string cardId, string search)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ProfileSearch(search);

            return LSCWSBase.ProfileSearch(search);
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
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.MemberCardGetPoints(cardId);

            return LSCWSBase.MemberCardGetPoints(cardId);
        }

        public virtual decimal GetPointRate()
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.GetPointRate();

            return LSCWSBase.GetPointRate();
        }

        public virtual List<PointEntry> PointEntiesGet(string cardNo, DateTime dateFrom)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.PointEntiesGet(cardNo, dateFrom);

            return LSCWSBase.PointEntiesGet(cardNo, dateFrom);
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo, string entryType)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.GiftCardGetBalance(cardNo, entryType);

            return LSCWSBase.GiftCardGetBalance(cardNo, entryType);
        }

        #endregion

        #region Notification

        public virtual List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.NotificationsGetByCardId(cardId);

            return LSCWSBase.NotificationsGetByCardId(cardId);
        }

        #endregion

        #region Item

        public virtual LoyItem ItemLoyGetByBarcode(string code, string storeId, string culture)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemGetByBarcode(code);

            return LSCWSBase.ItemGetByBarcode(code);
        }

        public virtual LoyItem ItemGetById(string id, string storeId, string culture, bool includeDetails)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemGetById(id);

            return LSCWSBase.ItemGetById(id);
        }

        public virtual List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems)
        {
            List<LoyItem> list = new List<LoyItem>();
            
            List<LoyItem> tmplist;
            if (NAVVersion < new Version("17.5"))
                tmplist = NavWSBase.ItemsGetByPublishedOfferId(pubOfferId, numberOfItems);
            else
                tmplist = LSCWSBase.ItemsGetByPublishedOfferId(pubOfferId, numberOfItems);

            foreach (LoyItem item in tmplist)
            {
                list.Add(new LoyItem(item.Id));
            }
            return list;
        }

        public virtual List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, string storeId, bool includeDetails)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemPage(storeId, pageNumber, includeDetails);

            return LSCWSBase.ItemPage(storeId, pageNumber, includeDetails);
        }

        public virtual UnitOfMeasure ItemUOMGetByIds(string itemid, string uomid)
        {
            return null;
        }

        public virtual List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemCustomerPricesGet(storeId, cardId, items);

            return LSCWSBase.ItemCustomerPricesGet(storeId, cardId, items);
        }

        #endregion

        #region ItemCategory and ProductGroup and Hierarchy

        public virtual List<ItemCategory> ItemCategoriesGet(string storeId, string culture)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemCategories();

            return LSCWSBase.ItemCategories();
        }

        public virtual ItemCategory ItemCategoriesGetById(string id)
        {
            ItemCategory icat;
            if (NAVVersion < new Version("17.5"))
            {
                icat = NavWSBase.ItemCategoriesGetById(id);
                icat.ProductGroups = NavWSBase.ProductGroupGetByItemCategory(icat.Id);
            }
            else
            {
                icat = LSCWSBase.ItemCategoriesGetById(id);
                icat.ProductGroups = LSCWSBase.ProductGroupGetByItemCategory(icat.Id);
            }
            return icat;
        }

        public virtual List<ProductGroup> ProductGroupGetByItemCategoryId(string itemcategoryId, string culture, bool includeChildren, bool includeItems)
        {
            return null;
        }

        public virtual ProductGroup ProductGroupGetById(string id, string culture, bool includeItems, bool includeItemDetail)
        {
            ProductGroup pgrp;
            if (NAVVersion < new Version("17.5"))
            {
                pgrp = NavWSBase.ProductGroupGetById(id);
                pgrp.Items = NavWSBase.ItemsGetByProductGroup(pgrp.Id);
            }
            else
            {
                pgrp = LSCWSBase.ProductGroupGetById(id);
                pgrp.Items = LSCWSBase.ItemsGetByProductGroup(pgrp.Id);
            }
            return pgrp;
        }

        public virtual List<Hierarchy> HierarchyGet(string storeId)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error(config.LSKey.Key, "Only supported in NAV 10.x and later");
                return new List<Hierarchy>();
            }

            if (NAVVersion < new Version("17.5"))
                return NavWSBase.HierarchyGet(storeId);

            return LSCWSBase.HierarchyGet(storeId);
        }

        public virtual MobileMenu MenuGet(string storeId, string salesType, Currency currency)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.MenuGet(storeId, salesType, currency);

            return LSCWSBase.MenuGet(storeId, salesType, currency);
        }

        #endregion

        #region Transaction

        public virtual List<SalesEntry> SalesEntriesGetByCardId(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNumberOfEntries)
        {
            List<SalesEntry> list;
            if (NAVVersion < new Version("17.5"))
            {
                list = NavWSBase.SalesHistory(cardId);
                list.AddRange(NavWSBase.OrderHistoryGet(cardId));
            }
            else
            {
                list = LSCWSBase.SalesHistory(cardId);
                list.AddRange(LSCWSBase.OrderHistoryGet(cardId));
            }

            list = list.OrderByDescending(l => l.DocumentRegTime).ToList();

            int cnt = 0;
            List<SalesEntry> trans = new List<SalesEntry>();
            foreach (SalesEntry entry in list)
            {
                cnt++;
                SalesEntry e;
                if (entry.IdType == DocumentIdType.Receipt)
                {
                    e = SalesEntryGet(string.Empty, Convert.ToInt32(entry.Id), entry.StoreId, entry.TerminalId, entry.IdType, false);
                }
                else
                {
                    e = SalesEntryGet(entry.Id, 0, string.Empty, string.Empty, entry.IdType, false);
                }
                e.Lines.Clear();
                e.DiscountLines.Clear();
                e.Payments.Clear();
                trans.Add(e);

                if (maxNumberOfEntries > 0 && cnt >= maxNumberOfEntries)
                    break;
            }
            return trans;
        }

        public virtual SalesEntry SalesEntryGet(string entryId, DocumentIdType type)
        {
            return SalesEntryGet(entryId, 0, string.Empty, string.Empty, type, true);
        }

        public virtual List<SalesEntry> SalesEntryGetReturnSales(string receiptNo)
        {
            return new List<SalesEntry>();
        }

        public virtual SalesEntry SalesEntryGet(string docId, int transId, string storeId, string terminalId, DocumentIdType type, bool getimages)
        {
            if (NAVVersion < new Version("17.5"))
            {
                if (type == DocumentIdType.Receipt)
                    return NavWSBase.TransactionGet(docId, storeId, terminalId, transId);
                return NavWSBase.OrderGet(docId);
            }

            SalesEntry entry;
            if (type == DocumentIdType.Receipt)
                entry = LSCWSBase.TransactionGet(docId, storeId, terminalId, transId, getimages);
            else
                entry = LSCWSBase.OrderGet(docId, getimages);

            if (entry.Payments != null)
            {
                foreach (SalesEntryPayment line in entry.Payments)
                {
                    line.TenderType = ConfigSetting.TenderTypeMapping(config.SettingsGetByKey(ConfigKey.TenderType_Mapping), line.TenderType, true); //map tender type between LSOmni and NAV
                }
            }
            return entry;
        }

        public virtual string FormatAmount(decimal amount, string culture)
        {
            return amount.ToString();
        }

        #endregion

        #region Hospitality Order

        public virtual OrderHosp HospOrderCalculate(OneList list)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.HospOrderCalculate(list);

            return LSCWSBase.HospOrderCalculate(list);
        }

        public virtual string HospOrderCreate(OrderHosp request)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.HospOrderCreate(request);

            return LSCWSBase.HospOrderCreate(request);
        }

        public virtual void HospOrderCancel(string storeId, string orderId)
        {
            if (NAVVersion < new Version("17.5"))
                NavWSBase.HospOrderCancel(storeId, orderId);

            LSCWSBase.HospOrderCancel(storeId, orderId);
        }

        public virtual OrderHospStatus HospOrderStatus(string storeId, string orderId)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.HospOrderStatus(storeId, orderId);

            return LSCWSBase.HospOrderKotStatus(storeId, orderId);
        }

        #endregion

        #region Basket

        public virtual Order BasketCalcToOrder(OneList list)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.BasketCalcToOrder(list);

            return LSCWSBase.BasketCalcToOrder(list);
        }

        #endregion

        #region Order

        public virtual OrderStatusResponse OrderStatusCheck(string orderId)
        {
            if (NAVVersion < new Version("13.5"))
                return new OrderStatusResponse();

            if (NAVVersion < new Version("17.5"))
                return NavWSBase.OrderStatusCheck(orderId);

            return LSCWSBase.OrderStatusCheck(orderId);
        }

        public virtual void OrderCancel(string orderId, string storeId, string userId, List<int> lineNo)
        {
            if (NAVVersion < new Version("13.5"))
                return;

            if (NAVVersion < new Version("17.5"))
                NavWSBase.OrderCancel(orderId, storeId, userId);

            LSCWSBase.OrderCancel(orderId, storeId, userId, lineNo);
        }

        public virtual OrderAvailabilityResponse OrderAvailabilityCheck(OneList request, bool shippingOrder)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.OrderAvailabilityCheck(request, shippingOrder);

            return LSCWSBase.OrderAvailabilityCheck(request, shippingOrder);
        }

        public virtual string OrderCreate(Order request, out string orderId)
        {
            if (request.OrderType == OrderType.ScanPayGoSuspend)
            {
                orderId = string.Empty;
                if (NAVVersion < new Version("17.5"))
                    return NavWSBase.ScanPayGoSuspend(request);

                return LSCWSBase.ScanPayGoSuspend(request);
            }

            if (NAVVersion < new Version("17.5"))
                return NavWSBase.OrderCreate(request, out orderId);

            return LSCWSBase.OrderCreate(request, out orderId);
        }

        #endregion

        #region Offer and Advertisement

        public virtual List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.PublishedOffersGet(cardId, itemId, storeId);

            return LSCWSBase.PublishedOffersGet(cardId, itemId, storeId);
        }

        public virtual List<Advertisement> AdvertisementsGetById(string id)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.AdvertisementsGetById(id);

            return LSCWSBase.AdvertisementsGetById(id);
        }

        #endregion

        #region Image

        public virtual ImageView ImageGetById(string imageId, bool includeBlob)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ImageGetById(imageId);

            return LSCWSBase.ImageGetById(imageId);
        }

        public virtual List<ImageView> ImagesGetByKey(string tableName, string key1, string key2, string key3, int imgCount, bool includeBlob)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ImagesGetByLink(tableName, key1, key2, key3);

            return LSCWSBase.ImagesGetByLink(tableName, key1, key2, key3);
        }

        #endregion

        #region Store

        public virtual Store StoreGetById(string id, bool details)
        {
            int offset = config.SettingsIntGetByKey(ConfigKey.Timezone_HoursOffset);
            
            Store store;
            if (NAVVersion < new Version("17.5"))
            {
                store = NavWSBase.StoreGetById(id);
                store.StoreHours = NavWSBase.StoreHoursGetByStoreId(id, offset);
            }
            else
            {
                store = LSCWSBase.StoreGetById(id, details);
                store.StoreHours = LSCWSBase.StoreHoursGetByStoreId(id, offset);
            }
            return store;
        }

        public virtual List<Store> StoresLoyGetByCoordinates(double latitude, double longitude, double maxDistance, Store.DistanceType units)
        {
            throw new NotImplementedException("IS THIS NEEDED?");
        }

        public virtual List<Store> StoresGetAll(bool clickAndCollectOnly)
        {
            int offset = config.SettingsIntGetByKey(ConfigKey.Timezone_HoursOffset);

            List<Store> stores;
            if (NAVVersion < new Version("17.5"))
            {
                stores = NavWSBase.StoresGet(clickAndCollectOnly, true);
                foreach (Store store in stores)
                {
                    store.StoreHours = NavWSBase.StoreHoursGetByStoreId(store.Id, offset);
                }
            }
            else
            {
                stores = LSCWSBase.StoresGet(clickAndCollectOnly, true);
                foreach (Store store in stores)
                {
                    store.StoreHours = LSCWSBase.StoreHoursGetByStoreId(store.Id, offset);
                }
            }
            return stores;
        }

        public virtual List<StoreServices> StoreServicesGetByStoreId(string storeId)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.StoreServicesGetByStoreId(storeId);

            return LSCWSBase.StoreServicesGetByStoreId(storeId);
        }

        public virtual List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReturnPolicyGet(storeId, storeGroupCode, itemCategory, productGroup, itemId, variantCode, variantDim1);

            return LSCWSBase.ReturnPolicyGet(storeId, storeGroupCode, itemCategory, productGroup, itemId, variantCode, variantDim1);
        }


        #endregion

        #region EComm Replication

        public virtual List<ReplImageLink> ReplEcommImageLinks(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplEcommImageLinks(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateImageLinks(appId, string.Empty, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplImage> ReplEcommImages(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplEcommImages(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateImages(appId, string.Empty, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplAttribute> ReplEcommAttribute(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplEcommAttribute(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateAttribute(appId, string.Empty, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplAttributeValue> ReplEcommAttributeValue(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplEcommAttributeValue(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateAttributeValue(appId, string.Empty, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplAttributeOptionValue> ReplEcommAttributeOptionValue(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplEcommAttributeOptionValue(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateAttributeOptionValue(appId, string.Empty, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplLoyVendorItemMapping> ReplEcommVendorItemMapping(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return new List<ReplLoyVendorItemMapping>();
        }

        public virtual List<ReplDataTranslation> ReplEcommDataTranslation(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplEcommDataTranslation(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateDataTranslation(appId, string.Empty, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplDataTranslation> ReplEcommHtmlTranslation(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return new List<ReplDataTranslation>();
        }

        public virtual List<ReplDataTranslationLangCode> ReplicateEcommDataTranslationLangCode(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateEcommDataTranslationLangCode(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateDataTranslationLangCode(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplShippingAgent> ReplEcommShippingAgent(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplEcommShippingAgent(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateShippingAgent(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplCustomer> ReplEcommMember(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplEcommMember(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateMember(appId, string.Empty, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplCountryCode> ReplEcommCountryCode(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplEcommCountryCode(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateCountryCode(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplInvStatus> ReplEcommInventoryStatus(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplEcommInventoryStatus(appId, string.Empty, storeId, fullReplication, batchSize, ref lastKey, ref recordsRemaining);
            
            return LSCWSBase.ReplicateInventoryStatus(appId, string.Empty, storeId, fullReplication, batchSize, ref lastKey, ref recordsRemaining);
        }

        public virtual List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.CheckAvailability(request, storeId);

            return LSCWSBase.CheckAvailability(request, storeId);
        }

        public virtual List<LoyItem> ReplEcommFullItem(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            // Avensia special request
            throw new NotImplementedException("Not supported in WS Mode");
        }

        #endregion
    }
}
