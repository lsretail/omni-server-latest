using System;
using System.Linq;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
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
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment;

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

        public virtual string Ping(out string centralVersion)
        {
            string ver;
            if (NAVVersion < new Version("17.5"))
                ver = NavWSBase.NavVersionToUse(true, true, out centralVersion);
            else
                ver = LSCWSBase.NavVersionToUse(out centralVersion);

            if (ver.Contains("ERROR"))
                throw new ApplicationException(ver);

            return ver;
        }

        #region ScanPayGo

        public virtual ScanPayGoProfile ScanPayGoProfileGet(string profileId, string storeNo, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 17.5 for SaaS");
                return new ScanPayGoProfile();
            }

            return LSCWSBase.ScanPayGoProfileGet(profileId, storeNo, stat);
        }

        public virtual bool SecurityCheckProfile(string orderNo, string storeNo, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return false;

            return LSCWSBase.SecurityCheckProfile(orderNo, storeNo, stat);
        }

        public virtual bool SecurityCheckLogResponse(string orderNo, string validationError, bool validationSuccessful, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 17.5 for SaaS");
                return false;
            }

            return LSCWSBase.SecurityCheckLogResponse(orderNo, validationError, validationSuccessful, stat);
        }

        public virtual ScanPayGoSecurityLog SecurityCheckLog(string orderNo, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 17.5 for SaaS");
                return new ScanPayGoSecurityLog();
            }

            return LSCWSBase.SecurityCheckLog(orderNo, stat);
        }

        public virtual string OpenGate(string qrCode, string storeNo, string devLocation, string memberAccount, bool exitWithoutShopping, bool isEntering, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
            {
                return "Not supported by LS Central version < 17.5 for SaaS";
            }

            return LSCWSBase.OpenGate(qrCode, storeNo, devLocation, memberAccount, exitWithoutShopping, isEntering, stat);
        }

        public virtual OrderCheck ScanPayGoOrderCheck(string documentId, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 17.5 for SaaS");
                return new OrderCheck();
            }

            return LSCWSBase.ScanPayGoOrderCheck(documentId, stat);
        }

        public virtual bool TokenEntrySet(ClientToken token, bool deleteToken, Statistics stat)
        {
            if (NAVVersion < new Version("20.3"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 20.3 for SaaS");
                return false;
            }

            return LSCWSBase.TokenEntrySet(token, deleteToken, stat);
        }

        public virtual List<ClientToken> TokenEntryGet(string accountNo, bool hotelToken, Statistics stat)
        {
            if (NAVVersion < new Version("20.3"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 20.3 for SaaS");
                return new List<ClientToken>();
            }

            return LSCWSBase.TokenEntryGet(accountNo, hotelToken, stat);
        }

        #endregion

        #region Contact

        public virtual MemberContact ContactCreate(MemberContact contact, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ContactCreate(contact);

            return LSCWSBase.ContactCreate(contact, stat);
        }

        public virtual void ContactUpdate(MemberContact contact, string accountId, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                NavWSBase.ContactUpdate(contact, accountId);

            LSCWSBase.ContactUpdate(contact, accountId, stat);
        }

        public virtual MemberContact ContactGet(ContactSearchType searchType, string searchValue, Statistics stat)
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
                    return LSCWSBase.ContactGet(string.Empty, string.Empty, string.Empty, string.Empty, searchValue, stat);
                case ContactSearchType.CardId:
                    return LSCWSBase.ContactGet(string.Empty, string.Empty, searchValue, string.Empty, string.Empty, stat);
                case ContactSearchType.UserName:
                    return LSCWSBase.ContactGet(string.Empty, string.Empty, string.Empty, searchValue, string.Empty, stat);
                case ContactSearchType.ContactNumber:
                    return LSCWSBase.ContactSearch(ContactSearchType.ContactNumber, searchValue, 1, true, stat).FirstOrDefault();
            }
            return null;
        }

        public virtual List<Customer> CustomerSearch(CustomerSearchType searchType, string search, int maxNumberOfRowsReturned, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.CustomerSearch(searchType, search, maxNumberOfRowsReturned);

            return LSCWSBase.CustomerSearch(searchType, search, maxNumberOfRowsReturned, stat);
        }

        public virtual double ContactAddCard(string contactId, string accountId, string cardId, Statistics stat)
        {
            return ContactAddCard(contactId, accountId, cardId, stat);
        }

        public virtual void ConatctBlock(string accountId, string cardId, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 17.5 for SaaS");
                return;
            }

            LSCWSBase.ConatctBlock(accountId, cardId, stat);
        }

        public virtual MemberContact Login(string userName, string password, string deviceID, string deviceName, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.Logon(userName, password, deviceID, deviceName);

            return LSCWSBase.Logon(userName, password, deviceID, deviceName, stat);
        }

        public virtual MemberContact SocialLogon(string authenticator, string authenticationId, string deviceID, string deviceName, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 17.5 for SaaS");
                return new MemberContact();
            }

            return LSCWSBase.SocialLogon(authenticator, authenticationId, deviceID, deviceName, stat);
        }

        //Change the password in NAV
        public virtual void ChangePassword(string userName, string token, string newPassword, string oldPassword, ref bool oldmethod, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                NavWSBase.ChangePassword(userName, token, newPassword, oldPassword, ref oldmethod);

            LSCWSBase.ChangePassword(userName, token, newPassword, oldPassword, stat);
        }

        public virtual string ResetPassword(string userName, string email, string newPassword, ref bool oldmethod, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ResetPassword(userName, email, newPassword, ref oldmethod);

            return LSCWSBase.ResetPassword(userName, email, stat);
        }

        public virtual string SPGPassword(string email, string token, string newPwd, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return string.Empty;

            return LSCWSBase.SPGPassword(email, token, newPwd, stat);
        }

        public virtual void LoginChange(string oldUserName, string newUserName, string password, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                NavWSBase.LoginChange(oldUserName, newUserName, password);

            LSCWSBase.LoginChange(oldUserName, newUserName, password, stat);
        }

        public virtual List<Profile> ProfileGetByCardId(string id, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ProfileGetAll();

            return LSCWSBase.ProfileGetAll(stat);
        }

        public virtual List<Profile> ProfileGetAll(Statistics stat)
        {
            return ProfileGetByCardId(string.Empty, stat);
        }

        public virtual List<Scheme> SchemeGetAll(Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.SchemeGetAll();

            return LSCWSBase.SchemeGetAll(stat);
        }

        public virtual Scheme SchemeGetById(string schemeId, Statistics stat)
        {
            if (schemeId.Equals("Ping"))
            {
                return new Scheme("NAV");
            }
            return new Scheme();
        }

        #endregion

        #region Device

        public virtual Device DeviceGetById(string id, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.DeviceGetById(id);

            return LSCWSBase.DeviceGetById(id, stat);
        }

        public virtual bool IsUserLinkedToDeviceId(string userName, string deviceId, Statistics stat)
        {
            return true;
        }

        #endregion

        #region Search

        public virtual List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned, Statistics stat)
        {
            List<MemberContact> list = new List<MemberContact>();
            bool exact = maxNumberOfRowsReturned == 1;
            MemberContact cont;
            switch (searchType)
            {
                case ContactSearchType.CardId:
                    if (NAVVersion < new Version("17.5"))
                        cont = NavWSBase.ContactGet(string.Empty, string.Empty, search, string.Empty, string.Empty, false);
                    else
                        cont = LSCWSBase.ContactGet(string.Empty, string.Empty, search, string.Empty, string.Empty, stat);

                    if (cont != null)
                        list.Add(cont);
                    break;
                case ContactSearchType.UserName:
                    if (NAVVersion < new Version("17.5"))
                        cont = NavWSBase.ContactGet(string.Empty, string.Empty, string.Empty, search, string.Empty, false);
                    else
                        cont = LSCWSBase.ContactGet(string.Empty, string.Empty, string.Empty, search, string.Empty, stat);

                    if (cont != null)
                        list.Add(cont);
                    break;
                default:
                    if (NAVVersion < new Version("21.3"))
                    {
                        if (exact == false)
                            search = "*" + search + "*";
                    }

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
                        List<MemberContact> tmplist = LSCWSBase.ContactSearch(searchType, search, maxNumberOfRowsReturned, exact, stat);
                        if (NAVVersion < new Version("21.3"))
                        {
                            foreach (MemberContact c in tmplist)
                            {
                                cont = LSCWSBase.ContactGet(c.Id, c.Account.Id, string.Empty, string.Empty, string.Empty, stat);
                                if (cont != null)
                                    list.Add(cont);
                            }
                        }
                        else
                        {
                            list = tmplist;
                        }
                    }
                    break;
            }
            return list;
        }

        public virtual List<LoyItem> ItemsSearch(string search, string storeId, int maxNumberOfItems, bool includeDetails, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemSearch(search);

            return LSCWSBase.ItemSearch(search, stat);
        }

        public virtual List<ItemCategory> ItemCategorySearch(string search, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemCategorySearch(search);

            return LSCWSBase.ItemCategorySearch(search, stat);
        }

        public virtual List<ProductGroup> ProductGroupSearch(string search, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ProductGroupSearch(search);

            return LSCWSBase.ProductGroupSearch(search, stat);
        }

        public virtual List<Store> StoreLoySearch(string search, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.StoreSearch(search);

            return LSCWSBase.StoreSearch(search, stat);
        }

        public virtual List<Profile> ProfileSearch(string cardId, string search, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ProfileSearch(search);

            return LSCWSBase.ProfileSearch(search, stat);
        }

        public virtual List<SalesEntry> SalesEntrySearch(string search, string cardId, int maxNumberOfTransactions, Statistics stat)
        {
            logger.Warn(config.LSKey.Key, "Not supported by LS Central version for SaaS");
            return new List<SalesEntry>();
        }

        #endregion

        #region Card

        public virtual Card CardGetById(string id, Statistics stat)
        {
            return null;
        }

        public virtual long MemberCardGetPoints(string cardId, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.MemberCardGetPoints(cardId);

            return LSCWSBase.MemberCardGetPoints(cardId, stat);
        }

        public virtual decimal GetPointRate(string currency, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.GetPointRate();

            return LSCWSBase.GetPointRate(currency, stat);
        }

        public virtual List<PointEntry> PointEntriesGet(string cardNo, DateTime dateFrom, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.PointEntriesGet(cardNo, dateFrom);

            return LSCWSBase.PointEntriesGet(cardNo, dateFrom, stat);
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo, int pin, string entryType, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.GiftCardGetBalance(cardNo, entryType);

            return LSCWSBase.GiftCardGetBalance(cardNo, pin, entryType, stat);
        }

        public virtual List<GiftCardEntry> GiftCardGetHistory(string cardNo, int pin, string entryType, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 17.5 for SaaS");
                return new List<GiftCardEntry>();
            }

            return LSCWSBase.GiftCardGetHistory(cardNo, pin, entryType, stat);
        }

        #endregion

        #region Notification

        public virtual List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.NotificationsGetByCardId(cardId);

            return LSCWSBase.NotificationsGetByCardId(cardId, stat);
        }

        #endregion

        #region Item

        public virtual LoyItem ItemLoyGetByBarcode(string code, string storeId, string culture, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemGetByBarcode(code);

            LoyItem item = LSCWSBase.ItemGetByBarcode(code, storeId, stat);
            if (item != null)
                return item;

            LoyItem bitem = LSCWSBase.ItemFindByBarcode(code, storeId, config.SettingsGetByKey(ConfigKey.ScanPayGo_Terminal), stat);
            item = ItemGetById(bitem.Id, storeId, string.Empty, true, stat);
            item.GrossWeight = bitem.GrossWeight;
            if (string.IsNullOrEmpty(bitem.Price) == false && bitem.Price != "0")
                item.Price = bitem.Price;
            return item;
        }

        public virtual LoyItem ItemGetById(string id, string storeId, string culture, bool includeDetails, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemGetById(id);

            return LSCWSBase.ItemGetById(id, stat);
        }

        public virtual List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems, Statistics stat)
        {
            List<LoyItem> list = new List<LoyItem>();

            List<LoyItem> tmplist;
            if (NAVVersion < new Version("17.5"))
                tmplist = NavWSBase.ItemsGetByPublishedOfferId(pubOfferId, numberOfItems);
            else
                tmplist = LSCWSBase.ItemsGetByPublishedOfferId(pubOfferId, numberOfItems, stat);

            foreach (LoyItem item in tmplist)
            {
                list.Add(new LoyItem(item.Id));
            }
            return list;
        }

        public virtual List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, string storeId, bool includeDetails, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemPage(storeId, pageNumber);

            return LSCWSBase.ItemPage(storeId, pageNumber, includeDetails, stat);
        }

        public virtual UnitOfMeasure ItemUOMGetByIds(string itemid, string uomid, Statistics stat)
        {
            return null;
        }

        public virtual List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemCustomerPricesGet(storeId, cardId, items);

            return LSCWSBase.ItemCustomerPricesGet(storeId, cardId, items, stat);
        }

        #endregion

        #region ItemCategory and ProductGroup and Hierarchy

        public virtual List<ItemCategory> ItemCategoriesGet(string storeId, string culture, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemCategories();

            return LSCWSBase.ItemCategories(stat);
        }

        public virtual ItemCategory ItemCategoriesGetById(string id, Statistics stat)
        {
            ItemCategory icat;
            if (NAVVersion < new Version("17.5"))
            {
                icat = NavWSBase.ItemCategoriesGetById(id);
                icat.ProductGroups = NavWSBase.ProductGroupGetByItemCategory(icat.Id);
            }
            else
            {
                icat = LSCWSBase.ItemCategoriesGetById(id, stat);
                icat.ProductGroups = LSCWSBase.ProductGroupGetByItemCategory(icat.Id, stat);
            }
            return icat;
        }

        public virtual List<ProductGroup> ProductGroupGetByItemCategoryId(string itemcategoryId, string culture, bool includeChildren, bool includeItems, Statistics stat)
        {
            return null;
        }

        public virtual ProductGroup ProductGroupGetById(string id, string culture, bool includeItems, bool includeItemDetail, Statistics stat)
        {
            ProductGroup pgrp;
            if (NAVVersion < new Version("17.5"))
            {
                pgrp = NavWSBase.ProductGroupGetById(id);
                pgrp.Items = NavWSBase.ItemsGetByProductGroup(pgrp.Id);
            }
            else
            {
                pgrp = LSCWSBase.ProductGroupGetById(id, stat);
                pgrp.Items = LSCWSBase.ItemsGetByProductGroup(pgrp.Id, stat);
            }
            return pgrp;
        }

        public virtual List<Hierarchy> HierarchyGet(string storeId, Statistics stat)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 10.0 for SaaS");
                return new List<Hierarchy>();
            }

            if (NAVVersion < new Version("17.5"))
                return NavWSBase.HierarchyGet(storeId);

            return LSCWSBase.HierarchyGet(storeId, stat);
        }

        public virtual MobileMenu MenuGet(string storeId, string salesType, Currency currency, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.MenuGet(storeId, salesType, currency);

            return LSCWSBase.MenuGet(storeId, salesType, currency);
        }

        #endregion

        #region Transaction

        public virtual List<SalesEntry> SalesEntriesGetByCardId(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNumberOfEntries, Statistics stat)
        {
            List<SalesEntry> list;
            if (NAVVersion < new Version("17.5"))
            {
                list = NavWSBase.SalesHistory(cardId);
                list.AddRange(NavWSBase.OrderHistoryGet(cardId));
            }
            else if (NAVVersion >= new Version("21.3"))
            {
                list = LSCWSBase.SalesEntryGetByCardId(cardId, maxNumberOfEntries, storeId, stat);
                return list;
            }
            else
            {
                list = LSCWSBase.SalesHistory(cardId, stat);
                list.AddRange(LSCWSBase.OrderHistoryGet(cardId, stat));
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
                    e = SalesEntryGet(string.Empty, Convert.ToInt32(entry.Id), entry.StoreId, entry.TerminalId, entry.IdType, stat);
                }
                else
                {
                    e = SalesEntryGet(entry.Id, 0, string.Empty, string.Empty, entry.IdType, stat);
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

        public virtual SalesEntry SalesEntryGet(string entryId, DocumentIdType type, Statistics stat)
        {
            return SalesEntryGet(entryId, 0, string.Empty, string.Empty, type, stat);
        }

        public virtual List<SalesEntryId> SalesEntryGetReturnSales(string receiptNo, Statistics stat)
        {
            logger.Warn(config.LSKey.Key, "Not supported by LS central version for SaaS");
            return new List<SalesEntryId>();
        }

        public virtual SalesEntryList SalesEntryGetSalesByOrderId(string orderId, Statistics stat)
        {
            logger.Warn(config.LSKey.Key, "Not supported by LS central version for SaaS");
            return new SalesEntryList();
        }

        public virtual SalesEntry SalesEntryGet(string docId, int transId, string storeId, string terminalId, DocumentIdType type, Statistics stat)
        {
            SalesEntry entry;

            if (NAVVersion < new Version("17.5"))
            {
                if (type == DocumentIdType.Receipt)
                    return NavWSBase.TransactionGet(docId, storeId, terminalId, transId);
                entry = NavWSBase.OrderGet(docId);
            }
            else if (NAVVersion >= new Version("22.2"))
            {
                entry = LSCWSBase.SalesEntryGetById(docId, type, stat);
            }
            else
            {
                if (type == DocumentIdType.Receipt || type == DocumentIdType.HospOrder)
                    entry = LSCWSBase.TransactionGet(docId, storeId, terminalId, transId, stat);
                else
                    entry = LSCWSBase.OrderGet(docId, stat);
            }

            if (entry == null || entry.Payments == null)
                return entry;

            foreach (SalesEntryPayment line in entry.Payments)
            {
                line.TenderType = ConfigSetting.TenderTypeMapping(config.SettingsGetByKey(ConfigKey.TenderType_Mapping), line.TenderType, true); //map tender type between LSOmni and NAV
            }
            return entry;
        }

        public virtual string FormatAmount(decimal amount, string culture)
        {
            return amount.ToString();
        }

        #endregion

        #region Hospitality Order

        public virtual OrderHosp HospOrderCalculate(OneList list, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.HospOrderCalculate(list);

            return LSCWSBase.HospOrderCalculate(list, stat);
        }

        public virtual string HospOrderCreate(OrderHosp request, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.HospOrderCreate(request);

            return LSCWSBase.HospOrderCreate(request, stat);
        }

        public virtual void HospOrderCancel(string storeId, string orderId, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                NavWSBase.HospOrderCancel(storeId, orderId);

            LSCWSBase.HospOrderCancel(storeId, orderId, stat);
        }

        public virtual OrderHospStatus HospOrderStatus(string storeId, string orderId, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.HospOrderStatus(storeId, orderId);

            return LSCWSBase.HospOrderKotStatus(storeId, orderId, stat);
        }

        public virtual List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.CheckAvailability(request, storeId);

            return LSCWSBase.CheckAvailability(request, storeId, stat);
        }

        #endregion

        #region Basket

        public virtual Order BasketCalcToOrder(OneList list, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.BasketCalcToOrder(list);

            return LSCWSBase.BasketCalcToOrder(list, stat);
        }

        #endregion

        #region Order

        public bool CompressCOActive(Statistics stat)
        {
            if (NAVVersion < new Version("24.0"))
                return false;

            return LSCWSBase.CompressCOActive(stat);
        }

        public virtual OrderStatusResponse OrderStatusCheck(string orderId, Statistics stat)
        {
            if (NAVVersion < new Version("13.5"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 13.5 for SaaS");
                return new OrderStatusResponse();
            }

            if (NAVVersion < new Version("17.5"))
                return NavWSBase.OrderStatusCheck(orderId);

            return LSCWSBase.OrderStatusCheck(orderId, stat);
        }

        public virtual void OrderCancel(string orderId, string storeId, string userId, List<OrderCancelLine> lines, Statistics stat)
        {
            if (NAVVersion < new Version("13.5"))
                return;

            if (NAVVersion < new Version("17.5"))
                NavWSBase.OrderCancel(orderId, storeId, userId);

            LSCWSBase.OrderCancel(orderId, storeId, userId, lines, stat);
        }

        public virtual OrderAvailabilityResponse OrderAvailabilityCheck(OneList request, bool shippingOrder, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.OrderAvailabilityCheck(request, shippingOrder);

            return LSCWSBase.OrderAvailabilityCheck(request, shippingOrder, stat);
        }

        public virtual string OrderCreate(Order request, out string orderId, Statistics stat)
        {
            if (request.OrderType == OrderType.ScanPayGoSuspend)
            {
                if (NAVVersion < new Version("17.5"))
                    return NavWSBase.ScanPayGoSuspend(request, out orderId);

                return LSCWSBase.ScanPayGoSuspend(request, out orderId, stat);
            }

            if (NAVVersion < new Version("17.5"))
                return NavWSBase.OrderCreate(request, out orderId);

            return LSCWSBase.OrderCreate(request, out orderId, stat);
        }

        public virtual string OrderEdit(Order request, ref string orderId, OrderEditType editType, Statistics stat)
        {
            return LSCWSBase.OrderEdit(request, ref orderId, editType, stat);
        }

        public virtual bool OrderUpdatePayment(string orderId, string storeId, OrderPayment payment, Statistics stat)
        {
            return LSCWSBase.OrderUpdatePayment(orderId, storeId, payment, stat);
        }

        #endregion

        #region Offer and Advertisement

        public virtual List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.PublishedOffersGet(cardId, itemId, storeId);

            return LSCWSBase.PublishedOffersGet(cardId, itemId, storeId, stat);
        }

        #endregion

        #region Image

        public virtual ImageView ImageGetById(string imageId, bool includeBlob, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ImageGetById(imageId);

            return LSCWSBase.ImageGetById(imageId, stat);
        }

        public virtual ImageView ImageGetByMediaId(string mediaId, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 17.5 for SaaS");
                return new ImageView();
            }

            return LSCWSBase.ImageGetByMediaId(mediaId, stat);
        }

        public virtual List<ImageView> ImagesGetByKey(string tableName, string key1, string key2, string key3, int imgCount, bool includeBlob, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ImagesGetByLink(tableName, key1, key2, key3);

            return LSCWSBase.ImagesGetByLink(tableName, key1, key2, key3, includeBlob, stat);
        }

        #endregion

        #region Store

        public virtual Store StoreGetById(string id, Statistics stat)
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
                store = LSCWSBase.StoreGetById(id, stat);
                store.StoreHours = LSCWSBase.StoreHoursGetByStoreId(id, offset, stat);
                store.Attributes = LSCWSBase.AttributeGet(id, AttributeLinkType.Store, stat);
            }
            return store;
        }

        public virtual List<Store> StoresGetAll(StoreGetType storeType, bool inclDetails, Statistics stat)
        {
            int offset = config.SettingsIntGetByKey(ConfigKey.Timezone_HoursOffset);

            List<Store> stores;
            if (NAVVersion < new Version("17.5"))
            {
                stores = NavWSBase.StoresGet(storeType, inclDetails);
                if (inclDetails)
                {
                    foreach (Store store in stores)
                    {
                        store.StoreHours = NavWSBase.StoreHoursGetByStoreId(store.Id, offset);
                    }
                }
            }
            else
            {
                stores = LSCWSBase.StoresGet(storeType, inclDetails, stat);
                if (inclDetails)
                {
                    foreach (Store store in stores)
                    {
                        store.StoreHours = LSCWSBase.StoreHoursGetByStoreId(store.Id, offset, stat);
                        store.Attributes = LSCWSBase.AttributeGet(store.Id, AttributeLinkType.Store, stat);
                    }
                }
            }
            return stores;
        }

        public virtual List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReturnPolicyGet(storeId, storeGroupCode, itemCategory, productGroup, itemId, variantCode, variantDim1);

            return LSCWSBase.ReturnPolicyGet(storeId, storeGroupCode, itemCategory, productGroup, itemId, variantCode, variantDim1, stat);
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
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateVendorItems(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateVendorItems(appId, string.Empty, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplDataTranslation> ReplEcommDataTranslation(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplEcommDataTranslation(appId, string.Empty, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateDataTranslation(appId, string.Empty, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplDataTranslation> ReplEcommItemHtmlTranslation(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("20.4"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 20.4 for SaaS");
                return new List<ReplDataTranslation>();
            }

            return LSCWSBase.ReplicateItemHtmlTranslation(appId, string.Empty, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplDataTranslation> ReplEcommDealHtmlTranslation(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("20.4"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 20.4 for SaaS");
                return new List<ReplDataTranslation>();
            }

            return LSCWSBase.ReplicateDealHtmlTranslation(appId, string.Empty, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
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

            return LSCWSBase.ReplicateCountryCode(appId, string.Empty, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplInvStatus> ReplEcommInventoryStatus(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplEcommInventoryStatus(appId, string.Empty, storeId, fullReplication, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateInventoryStatus(appId, string.Empty, storeId, fullReplication, batchSize, ref lastKey, ref recordsRemaining);
        }

        public virtual List<LoyItem> ReplEcommFullItem(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            logger.Warn(config.LSKey.Key, "Not supported by LS central version for SaaS");
            return new List<LoyItem>();
        }

        #endregion
    }
}
