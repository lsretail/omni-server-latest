using System;
using System.IO;
using System.Collections.Generic;
using System.Web.Services.Protocols;

using LSOmni.Common.Util;
using LSOmni.DataAccess.BOConnection.NavSQL.XmlMapping.Loyalty;
using LSOmni.DataAccess.BOConnection.NavSQL.Mapping;
using LSOmni.DataAccess.BOConnection.NavSQL.Dal;
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
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;

namespace LSOmni.DataAccess.BOConnection.NavSQL
{
    //Navision back office connection
    public class NavLoyalty : NavBase, ILoyaltyBO
    {
        protected static MemCache PublishOfferMemCache = null; //minimize the calls to Nav web service

        public int TimeoutInSeconds
        {
            set { base.TimeOutInSeconds = value; }
        }

        public NavLoyalty() : base()
        {
            if (PublishOfferMemCache == null)
                PublishOfferMemCache = new MemCache(5); //5 minutes
        }

        public virtual string Ping(string ipAddress)
        {
            string ver = base.Ping(); //checks NAV version, throws exception on failure, 
            if (ver.Contains("ERROR"))
                throw new ApplicationException(ver);

            return ver;
        }

        #region Contact

        public virtual string ContactCreate(MemberContact contact)
        {
            if (contact == null)
                throw new ApplicationException("Contact can not be null");

            //must have a deviceId, otherwise no "Member Login Card" entry is made in nav
            if (contact.LoggedOnToDevice == null)
            {
                contact.LoggedOnToDevice = new Device(GetDefaultDeviceId(contact.UserName));
            }
            if (string.IsNullOrWhiteSpace(contact.LoggedOnToDevice.Id))
            {
                contact.LoggedOnToDevice.Id = GetDefaultDeviceId(contact.UserName);
            }
            if (string.IsNullOrWhiteSpace(contact.LoggedOnToDevice.DeviceFriendlyName))
                contact.LoggedOnToDevice.DeviceFriendlyName = "Web application";

            if (navWS == null)
            {
                NavXml navXml = new NavXml();
                string xmlRequest = navXml.ContactCreateRequestXML(contact);
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                ContactRs contactRs = navXml.ContactCreateResponseXML(xmlResponse);

                //CALL NAV web service and send in the cardId so it will link the card to user 
                //and we only want to link the card to the user, not to the device
                CreateDeviceAndLinkToUser(contact.UserName, "", "", contactRs.CardId);
                return contactRs.ContactId;
            }
            
            string respCode = string.Empty;
            string errorText = string.Empty;
            ContactMapping map = new ContactMapping();

            string clubId = string.Empty;
            string cardId = string.Empty;
            string contId = string.Empty;
            string acctId = string.Empty;
            string schmId = string.Empty;
            decimal point = 0;

            NavWS.RootMemberContactCreate root = map.MapToRoot(contact);
            try
            {
                navWS.MemberContactCreate(ref respCode, ref errorText, ref clubId, ref schmId, ref acctId, ref contId, ref cardId, ref point, ref root);
                if (respCode != "0000")
                    throw new LSOmniServiceException(StatusCode.Error, errorText);
            }
            catch (SoapException e)
            {
                if (e.Message.Contains("Method"))
                    throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
                else
                    throw;
            }
            return contId;
        }

        public virtual void ContactUpdate(MemberContact contact, string accountId)
        {
            NavXml navXml = new NavXml();
            if (contact == null)
                throw new ApplicationException("ContactRq can not be null");

            if (navWS == null)
            {
                string xmlRequest = navXml.ContactUpdateRequestXML(contact, accountId);
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                return;
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            ContactMapping map = new ContactMapping();

            NavWS.RootMemberContactCreate1 root = map.MapToRoot1(contact, accountId);
            try
            {
                navWS.MemberContactUpdate(ref respCode, ref errorText, ref root);
                if (respCode != "0000")
                    throw new LSOmniServiceException(StatusCode.MemberAccountNotFound, errorText);
            }
            catch (SoapException e)
            {
                if (e.Message.Contains("Method"))
                    throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
                else
                    throw;
            }
        }

        public virtual MemberContact ContactGetById(string id, int numberOfTrans)
        {
            MemberContact contact = null;
            ContactRepository rep = new ContactRepository();
            contact = rep.ContactGet(ContactSearchType.ContactNumber, id);

            if (contact != null)
            {
                long totalPoints = MemberCardGetPoints(contact.Card.Id);
                contact.Account.PointBalance = (totalPoints == 0) ? contact.Account.PointBalance : totalPoints;

                // get trans history
                contact.Transactions = SalesEntriesGetByContactId(contact.Id, numberOfTrans, string.Empty);
            }
            return contact;
        }

        public virtual MemberContact ContactGetByCardId(string card, int numberOfTrans)
        {
            MemberContact contact = null;
            long totalPoints = 0;

            if (NavDirect)
            {
                ContactRepository rep = new ContactRepository();
                contact = rep.ContactGet(ContactSearchType.CardId, card);
                if (contact != null)
                {
                    totalPoints = MemberCardGetPoints(contact.Card.Id);
                    contact.Account.PointBalance = (totalPoints == 0) ? contact.Account.PointBalance : totalPoints;

                    // get trans history
                    contact.Transactions = SalesEntriesGetByContactId(contact.Id, numberOfTrans, string.Empty);
                }
                return contact;
            }

            if (navWS == null)
            {
                ContactXml xml = new ContactXml();
                string xmlRequest = xml.RequestXML(card, numberOfTrans);
                string xmlResponse = RunOperation(xmlRequest);
                //need to ignore some nav response codes
                //1680 = The last valid date for the card is 05/30/13
                if (GetResponseCode(ref xmlResponse) == "1680")
                {
                    return null; //not found
                }
                HandleResponseCode(ref xmlResponse);
                return xml.ResponseXML(xmlResponse);
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            ContactMapping map = new ContactMapping();

            NavWS.Root5 root5 = new NavWS.Root5();
            try
            {
                navWS.GetMemberContact(ref respCode, ref errorText, card, string.Empty, string.Empty, ref root5);
                if (respCode != "0000")
                    throw new LSOmniServiceException(StatusCode.MemberCardNotFound, errorText);
            }
            catch (SoapException e)
            {
                if (e.Message.Contains("Method"))
                    throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
                else
                    throw;
            }

            contact = map.MapFromRootToContact(root5);

            NavWS.Root4 root4 = new NavWS.Root4();
            decimal remainingPoints = 0;

            try
            {
                navWS.GetMemberCard(ref respCode, ref errorText, card, ref remainingPoints, ref root4);
                if (respCode != "0000")
                    throw new LSOmniServiceException(StatusCode.NoEntriesFound, errorText);
            }
            catch (SoapException e)
            {
                if (e.Message.Contains("Method"))
                    throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
                else
                    throw;
            }

            contact.Account.PointBalance = (remainingPoints == 0) ? contact.Account.PointBalance : Convert.ToInt64(Math.Floor(remainingPoints));
            contact.Profiles = new List<Profile>();
            foreach (NavWS.MemberAttributeList list in root4.MemberAttributeList)
            {
                if (list.Type != "0")
                    continue;

                contact.Profiles.Add(new Profile()
                {
                    Id = list.Code,
                    Description = list.Description,
                    DefaultValue = list.Value
                });
            }

            NavWS.Root13 root13 = new NavWS.Root13();
            try
            {
                navWS.GetDirectMarketingInfo(ref respCode, ref errorText, card, string.Empty, string.Empty, ref root13);
                if (respCode != "0000")
                    throw new LSOmniServiceException(StatusCode.NoEntriesFound, errorText);
            }
            catch (SoapException e)
            {
                if (e.Message.Contains("Method"))
                    throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
                else
                    throw;
            }

            contact.PublishedOffers = map.MapFromRootToPublishedOffers(root13);

            NavWS.Root12 root12 = new NavWS.Root12();
            try
            {
                navWS.GetMemberSalesHistory(ref respCode, ref errorText, string.Empty, string.Empty, card, numberOfTrans, ref root12);
                if (respCode != "0000")
                    throw new LSOmniServiceException(StatusCode.NoEntriesFound, errorText);
            }
            catch (SoapException e)
            {
                if (e.Message.Contains("Method"))
                    throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
                else
                    throw;
            }

            contact.Transactions = map.MapFromRootToSalesEntries(root12);
            return contact;
        }

        public virtual MemberContact ContactGetByUserName(string user)
        {
            ContactRepository rep = new ContactRepository();
            return rep.ContactGetByUserName(user);
        }

        public virtual MemberContact ContactGetByEMail(string email)
        {
            ContactRepository rep = new ContactRepository();
            return rep.ContactGet(ContactSearchType.Email, email);
        }

        public virtual double ContactAddCard(string contactId, string accountId, string cardId)
        {
            NavXml navXml = new NavXml();
            string xmlRequest = navXml.ContactAddCardRequestXML(contactId, accountId, cardId);

            //return the Encrypted pwd that NAV returned to us
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            if (NAVVersion.Major > 7)
            {
                return navXml.ContactAddCardResponseXml(xmlResponse);
            }
            else
            {
                throw new LSOmniServiceException(StatusCode.CardIdInvalid, "ContactAddCardResponseXml not suppored in LS Nav 7.0");
            }
        }

        public virtual ContactRs Login(string userName, string password, string cardId)
        {
            //first login is to see if login buffer has more than 2, if so need to login again with correct CardId that was picked.
            ContactRs contactRs = Logon(userName, password, cardId);
            if (string.IsNullOrWhiteSpace(contactRs.ContactId))
                contactRs = Logon(userName, password, contactRs.CardId);
            return contactRs;
        }

        private ContactRs Logon(string userName, string password, string cardId)
        {
            NavXml navXml = new NavXml();
            string xmlRequest = navXml.LoginRequestXML(userName, password, cardId);
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            ContactRs contactRs = navXml.LoginResponseXML(xmlResponse);
            return contactRs;
        }

        //Change the password in NAV
        public virtual string ChangePassword(string userName, string newPassword, string oldPassword)
        {
            NavXml navXml = new NavXml();
            string xmlRequest = navXml.ChangePasswordRequestXML(userName, newPassword, oldPassword);
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            return string.Empty;
        }

        public virtual string ResetPassword(string userName, string newPassword)
        {
            //Reset the password in NAV
            //used when a user forgot his pwd, and has been validated (URL sent to him)

            //If newPassword is empty then NAV creates a random password ?
            //return the Encrypted pwd that NAV returned to us
            NavXml navXml = new NavXml();
            string xmlRequest = navXml.ResetPasswordRequestXML(userName, newPassword);
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            return string.Empty;
        }

        public virtual List<Profile> ProfileGetByContactId(string id)
        {
            ContactRepository rep = new ContactRepository();
            return rep.ProfileGetByContactId(id);
        }

        public virtual List<Profile> ProfileGetAll()
        {
            ContactRepository rep = new ContactRepository();
            return rep.ProfileGetAll();
        }

        public virtual Account AccountGetById(string accountId)
        {
            ContactRepository rep = new ContactRepository();
            return rep.AccountGetById(accountId);
        }

        public virtual List<Scheme> SchemeGetAll()
        {
            ContactRepository rep = new ContactRepository();
            return rep.SchemeGetAll();
        }

        public virtual Scheme SchemeGetById(string schemeId)
        {
            var rep = new ContactRepository();
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
            NavXml navXml = new NavXml();
            string xmlRequest = navXml.CreateDeviceAndLinkToUser(userName, deviceId, deviceFriendlyName, cardId);
            string xmlResponse = RunOperation(xmlRequest);
            StatusCode statusCode = GetStatusCode(ref xmlResponse);
            if (statusCode == StatusCode.Error)
            {
                string navResponseCode = GetResponseCode(ref xmlResponse);
                //1001 = Member Login Device already exist,  NavResponseCode.Error = 1001
                logger.Info("responseCode {0} ignored for userName {1}  deviceId {2}", navResponseCode, userName, deviceId);
                return;
            }
            else if (statusCode != StatusCode.OK)
            {
                switch (statusCode)
                {
                    case StatusCode.DeviceIdMissing: //MissingDeviceId:
                    case StatusCode.UserNotLoggedIn: //LoginIdNotFound:
                        return;
                }
            }
            HandleResponseCode(ref xmlResponse);
        }

        private string GetDefaultDeviceId(string userName)
        {
            return ("WEB-" + userName);
        }

        public virtual Device DeviceGetById(string id)
        {
            ContactRepository rep = new ContactRepository();
            return rep.DeviceGetById(id);
        }

        public virtual bool IsUserLinkedToDeviceId(string userName, string deviceId, out string cardId)
        {
            ContactRepository rep = new ContactRepository();
            return rep.IsUserLinkedToDeviceId(userName, deviceId, out cardId);
        }

        #endregion

        #region Search

        public virtual List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned)
        {
            ContactRepository rep = new ContactRepository();
            return rep.ContactSearch(searchType, search, maxNumberOfRowsReturned);
        }

        public virtual List<LoyItem> ItemsSearch(string search, string storeId, int maxNumberOfItems, bool includeDetails)
        {
            ItemRepository rep = new ItemRepository();
            return rep.ItemLoySearch(search, storeId, maxNumberOfItems, includeDetails);
        }

        public virtual List<ItemCategory> ItemCategorySearch(string search)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository();
            return rep.ItemCategorySearch(search);
        }

        public virtual List<ProductGroup> ProductGroupSearch(string search)
        {
            ProductGroupRepository rep = new ProductGroupRepository();
            return rep.ProductGroupSearch(search);
        }

        public virtual List<Store> StoreLoySearch(string search)
        {
            StoreRepository rep = new StoreRepository(NAVVersion);
            return rep.StoreLoySearch(search);
        }

        public virtual List<Profile> ProfileSearch(string contactId, string search)
        {
            ContactRepository rep = new ContactRepository();
            return rep.ProfileSearch(contactId, search);
        }

        public virtual List<LoyTransaction> TransactionSearch(string search, string contactId, int maxNumberOfTransactions, string culture, bool includeLines)
        {
            TransactionRepository rep = new TransactionRepository(NAVVersion);
            return rep.LoyTransactionSearch(search, contactId, maxNumberOfTransactions, culture, includeLines);
        }

        #endregion

        #region Card

        public virtual Card CardGetByContactId(string contactId)
        {
            ContactRepository rep = new ContactRepository();
            return rep.CardGetByContactId(contactId);
        }

        public virtual Card CardGetById(string id)
        {
            ContactRepository rep = new ContactRepository();
            return rep.CardGetById(id);
        }

        public virtual long MemberCardGetPoints(string cardId)
        {
            if (string.IsNullOrWhiteSpace(cardId))
                throw new ApplicationException("cardId can not be empty");

            if (NAVVersion.Major < 8)
                throw new LSOmniServiceException(StatusCode.CardIdInvalid, "MemberCardGetPoints not suppored in LS Nav 7.0");

            if (navWS == null)
            {
                MemberCardXml xml = new MemberCardXml();
                string xmlRequest = xml.MemberCardRequestXML(cardId);
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                return xml.MemberCardPointsResponse(xmlResponse);
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            NavWS.Root4 root = new NavWS.Root4();
            decimal remainingPoints = 0;

            try
            {
                navWS.GetMemberCard(ref respCode, ref errorText, cardId, ref remainingPoints, ref root);
                if (respCode != "0000")
                    throw new LSOmniServiceException(StatusCode.NoEntriesFound, errorText);
            }
            catch (SoapException e)
            {
                if (e.Message.Contains("Method"))
                    throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
                else
                    throw;
            }
            return Convert.ToInt64(Math.Floor(remainingPoints));
        }

        public virtual decimal GetPointRate()
        {
            CurrencyExchRateRepository rep = new CurrencyExchRateRepository();
            ReplCurrencyExchRate exchrate = rep.CurrencyExchRateGetById("LOY");
            if (exchrate == null)
                return 0;

            return exchrate.CurrencyFactor;
        }

        #endregion

        #region Notification

        public virtual List<Notification> NotificationsGetByContactId(string contactId, int numberOfNotifications)
        {
            NotificationRepository rep = new NotificationRepository();
            return rep.NotificationsGetByContactId(contactId, numberOfNotifications);
        }

        public virtual List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications)
        {
            List<Notification> pol = new List<Notification>();
            if (string.IsNullOrWhiteSpace(cardId))
                return pol;

            try
            {
                if (NAVVersion.CompareTo(new Version("9.00.05")) >= 0)
                {
                    //the name of the Nav requestID changed between 9.00.04 and 9.00.05 
                    //TODO need to get the Notifications for this ws call too
                    if(navWS == null)
                    { 
                        PublishedOfferXml xml = new PublishedOfferXml();
                        if (PublishOfferMemCache.Contains(cardId) == false)
                        {
                            string xmlRequest = xml.PublishedOfferMemberRequestXML(cardId, string.Empty, string.Empty);
                            string xmlResponse = RunOperation(xmlRequest);
                            HandleResponseCode(ref xmlResponse);
                            return xml.NotificationMemberResponseXML(xmlResponse);
                        }
                        else
                        {
                            //exist in cache
                            string xmlResponse = (string)PublishOfferMemCache.Get(cardId);
                            return xml.NotificationMemberResponseXML(xmlResponse);
                        }
                    }

                    string respCode = string.Empty;
                    string errorText = string.Empty;
                    NavWS.Root13 root = new NavWS.Root13();
                    try
                    {
                        navWS.GetDirectMarketingInfo(ref respCode, ref errorText, cardId, string.Empty, string.Empty, ref root);
                        if (respCode != "0000")
                            throw new LSOmniServiceException(StatusCode.NoEntriesFound, errorText);
                    }
                    catch (SoapException e)
                    {
                        if (e.Message.Contains("Method"))
                            throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
                        else
                            throw;
                    }

                    ContactMapping map = new ContactMapping();
                    return map.MapFromRootToNotifications(root);
                }
                else
                {
                    return null; //calling methods knows that we didn't call NAV use use the old DD replication 
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(LSOmniServiceException))
                    throw ex;
                else
                    throw new LSOmniServiceException(StatusCode.Error, ex.Message, ex);
            }
        }

        #endregion

        #region Item

        public virtual LoyItem ItemGetById(string id, string storeId, string culture, bool includeDetails)
        {
            ItemRepository rep = new ItemRepository();
            return rep.ItemLoyGetById(id, storeId, culture, includeDetails);
        }

        public virtual LoyItem ItemLoyGetByBarcode(string code, string storeId, string culture)
        {
            ItemRepository rep = new ItemRepository();
            return rep.ItemLoyGetByBarcode(code, storeId, culture);
        }

        public virtual List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems)
        {
            if (numberOfItems <= 0)
                numberOfItems = 50;

            List<LoyItem> list = new List<LoyItem>();
            if (string.IsNullOrWhiteSpace(pubOfferId))
                return list;

            PublishedOfferXml xml = new PublishedOfferXml();
            string xmlRequest = xml.PublishedOfferItemsRequestXML(pubOfferId, numberOfItems);
            string xmlResponse = RunOperation(xmlRequest);
            string navResponseCode = GetResponseCode(ref xmlResponse);
            if (navResponseCode == "0004")
            {
                //<Response_Text>Unknown Request_ID LOAD_PUBLISHED_OFFER_ITEMS</Response_Text>
                //0004 = Unknown Request_ID,  NavResponseCode.Error = 0004
                logger.Error("responseCode {0} - LOAD_PUBLISHED_OFFER_ITEMS is only supported by default in LS Nav 9.00.03", navResponseCode);
                return list; //request not found so return empty list instead of breaking
            }

            HandleResponseCode(ref xmlResponse);

            ItemRepository rep = new ItemRepository();
            List<LoyItem> tmplist = xml.PublishedOfferItemsResponseXML(xmlResponse);
            foreach (LoyItem item in tmplist)
            {
                list.Add(rep.ItemLoyGetById(item.Id, string.Empty, string.Empty, true));
            }
            return list;
        }

        public virtual List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, string storeId, bool includeDetails)
        {
            ItemRepository rep = new ItemRepository();
            return rep.ItemsPage(pageSize, pageNumber, itemCategoryId, productGroupId, search, storeId, includeDetails);
        }

        public virtual UnitOfMeasure ItemUOMGetByIds(string itemid, string uomid)
        {
            ItemUOMRepository rep = new ItemUOMRepository();
            return rep.ItemUOMGetByIds(itemid, uomid);
        }

        #endregion

        #region ItemCategory and ProductGroup and Hierarchy

        public virtual List<ItemCategory> ItemCategoriesGet(string storeId, string culture)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository();
            return rep.ItemCategoriesGetAll(storeId, culture);
        }

        public virtual ItemCategory ItemCategoriesGetById(string id)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository();
            return rep.ItemCategoryGetById(id);
        }

        public virtual List<ProductGroup> ProductGroupGetByItemCategoryId(string itemcategoryId, string culture, bool includeChildren, bool includeItems)
        {
            ProductGroupRepository rep = new ProductGroupRepository();
            return rep.ProductGroupGetByItemCategoryId(itemcategoryId, culture, includeChildren, includeItems);
        }

        public virtual ProductGroup ProductGroupGetById(string id, string culture, bool includeChildren, bool includeItems)
        {
            ProductGroupRepository rep = new ProductGroupRepository();
            return rep.ProductGroupGetById(id, culture, includeChildren, includeItems);
        }

        public virtual List<Hierarchy> HierarchyGet(string storeId)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error("Only supported in NAV 10.x and later");
                return new List<Hierarchy>();
            }

            if (NavDirect)
            {
                HierarchyRepository rep = new HierarchyRepository(NAVVersion);
                return rep.HierarchyGetByStore(storeId);
            }

            List<Hierarchy> list = new List<Hierarchy>();
            List<HierarchyNode> nodes = new List<HierarchyNode>();
            if (navWS == null)
            {
                HierarchyXml xml = new HierarchyXml();
                string xmlRequest = xml.HierarchyRequestXML(storeId);
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                list = xml.HierarchyResponseXML(xmlResponse, out nodes);

                // Load node links for all nodes
                foreach (HierarchyNode node in nodes)
                {
                    xmlRequest = xml.HierarchyNodeRequestXML(node.HierarchyCode, node.Id);
                    xmlResponse = RunOperation(xmlRequest);
                    HandleResponseCode(ref xmlResponse);
                    node.Leafs = xml.HierarchyNodeResponseXML(xmlResponse);
                }
            }
            else
            {
                string respCode = string.Empty;
                string errorText = string.Empty;
                NavWS.Root3 navroot = new NavWS.Root3();

                try
                {
                    navWS.GetHierarchy(ref respCode, ref errorText, storeId, ref navroot);
                    if (respCode != "0000")
                        throw new LSOmniServiceException(StatusCode.NoEntriesFound, errorText);
                }
                catch (SoapException e)
                {
                    if (e.Message.Contains("Method"))
                        throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
                    else
                        throw;
                }

                foreach (NavWS.Hierarchy top in navroot.Hierarchy)
                {
                    list.Add(new Hierarchy()
                    {
                        Id = top.HierarchyCode,
                        Description = top.Description,
                        Type = (HierarchyType)Convert.ToInt32(top.Type)
                    });
                }

                foreach (NavWS.HierarchyNodes val in navroot.HierarchyNodes)
                {
                    HierarchyNode node = new HierarchyNode()
                    {
                        Id = val.NodeID,
                        ParentNode = val.ParentNodeID,
                        PresentationOrder = val.PresentationOrder,
                        Indentation = val.Indentation,
                        Description = val.Description,
                        HierarchyCode = val.HierarchyCode,
                        ImageId = val.RetailImageCode,
                    };
                    nodes.Add(node);

                    NavWS.Root11 navroot1 = new NavWS.Root11();
                    NavWS.Root21 navroot2 = new NavWS.Root21();

                    try
                    {
                        navWS.GetHierarchyNode(ref respCode, ref errorText, val.HierarchyCode, val.NodeID, storeId, navroot1, ref navroot2);
                        if (respCode != "0000")
                            throw new LSOmniServiceException(StatusCode.NoEntriesFound, errorText);
                    }
                    catch (SoapException e)
                    {
                        if (e.Message.Contains("Method"))
                            throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
                        else
                            throw;
                    }

                    if (navroot2.HierarchyNodeLink == null)
                        continue;

                    foreach (NavWS.HierarchyNodeLink lnk in navroot2.HierarchyNodeLink)
                    {
                        node.Leafs.Add(new HierarchyLeaf()
                        {
                            Id = lnk.No,
                            ParentNode = lnk.NodeID,
                            Description = lnk.Description,
                            HierarchyCode = lnk.HierarchyCode,
                            Type = (HierarchyLeafType)Convert.ToInt32(lnk.Type)
                        });
                    }
                }
            }

            // build the hierarchy tree
            foreach (Hierarchy root in list)
            {
                root.Nodes = nodes.FindAll(x => x.HierarchyCode == root.Id && string.IsNullOrEmpty(x.ParentNode));
                for (int i = 0; i < root.Nodes.Count; i++)
                {
                    HierarchyNode node = root.Nodes[i];
                    root.RecursiveBuilder(ref node, nodes);
                }
            }
            return list;
        }

        #endregion

        #region Transaction

        public virtual List<LoyTransaction> SalesEntriesGetByContactId(string contactId, int maxNumberOfTransactions, string culture)
        {
            TransactionRepository rep = new Dal.TransactionRepository(NAVVersion);
            if (maxNumberOfTransactions <= 0)
                return new List<LoyTransaction>();

            return rep.LoyTransactionHeadersGetByContactId(contactId, maxNumberOfTransactions, culture);
        }

        public virtual LoyTransaction TransactionGetByReceiptNo(string receiptNo, bool includeLines)
        {
            TransactionRepository rep = new TransactionRepository(NAVVersion);
            return rep.TransactionGetByReceipt(receiptNo, string.Empty, includeLines);
        }

        public virtual LoyTransaction TransactionNavGetByIdWithPrint(string storeId, string terminalId, string transactionId)
        {
            NavXml navXml = new NavXml();
            string xmlRequest = navXml.TransactionGetByIdRequestXML(storeId, terminalId, transactionId);
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            LoyTransaction trans = navXml.TransactionGetByIdResponseXml(xmlResponse);
            trans.Store = new Store(storeId);

            ItemVariantRegistrationRepository vrep = new ItemVariantRegistrationRepository();
            foreach (LoySaleLine line in trans.SaleLines)
            {
                line.VariantReg = vrep.VariantRegGetById(line.VariantReg.Id, line.Item.Id);
            }
            return trans;
        }

        public virtual LoyTransaction SalesEntryGetById(string entryId)
        {
            TransactionRepository repo = new TransactionRepository(NAVVersion);
            return repo.SalesEntryGetById(entryId);
        }

        public virtual string FormatAmount(decimal amount, string culture)
        {
            TransactionRepository rep = new TransactionRepository(NAVVersion);
            return rep.FormatAmountToString(amount, culture);
        }

        #endregion

        #region Basket

        public virtual BasketCalcResponse BasketCalc(BasketCalcRequest basketRequest, decimal shippingPrice)
        {
            if (string.IsNullOrWhiteSpace(basketRequest.Id))
                basketRequest.Id = GuidHelper.NewGuidString();

            BasketXml xml = new BasketXml();
            string xmlRequest = xml.BasketCalcRequestXML(basketRequest, NAVVersion.Major);
            BasketCalcResponse rs = new BasketCalcResponse();
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            return xml.BasketCalcResponseXML(xmlResponse, shippingPrice, NAVVersion.Major);
        }

        public virtual Order BasketCalcToOrder(OneList list)
        {
            if (string.IsNullOrWhiteSpace(list.Id))
                list.Id = GuidHelper.NewGuidString();

            if (navWS == null)
            {
                BasketXml xml = new BasketXml();
                string xmlRequest = xml.BasketCalcRequestXML(list, NAVVersion.Major);
                BasketCalcResponse rs = new BasketCalcResponse();
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                return xml.BasketCalcToOrderResponseXML(xmlResponse, NAVVersion.Major);
            }

            OrderMapping map = new OrderMapping();
            string respCode = string.Empty;
            string errorText = string.Empty;
            NavWS.Root1 root = map.MapFromRetailTransactionToRoot(list);

            try
            {
                navWS.EcomCalculateBasket(ref respCode, ref errorText, ref root);
                if (respCode != "0000")
                    throw new LSOmniServiceException(StatusCode.TransactionCalc, errorText);
            }
            catch (SoapException e)
            {
                if (e.Message.Contains("Method"))
                    throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
                else
                    throw;
            }

            Order order = map.MapFromRootTransactionToOrder(root);
            if (string.IsNullOrEmpty(list.CardId) && string.IsNullOrEmpty(list.ContactId))
                order.AnonymousOrder = true;
            return order;
        }

        #endregion

        #region Order

        public virtual OrderStatusResponse OrderStatusCheck(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                throw new ApplicationException("OrderStatusCheck transactionId can not be empty");

            BasketXml xml = new BasketXml();
            string xmlRequest = xml.OrderStatusRequestXML(transactionId);
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            return xml.OrderStatusResponseXML(xmlResponse);
        }

        public virtual void OrderCancel(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                throw new ApplicationException("OrderCancel transactionId can not be empty");

            BasketXml xml = new BasketXml();
            string xmlRequest = xml.OrderCancelRequestXML(transactionId);
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
        }

        public virtual List<OrderLineAvailability> OrderAvailabilityCheck(OrderAvailabilityRequest request)
        {
            //clickncollect
            if (string.IsNullOrWhiteSpace(request.Id))
                request.Id = GuidHelper.NewGuidString();

            ClickCollectXml xml = new ClickCollectXml();
            string xmlRequest = xml.CheckAvailabilityRequestXML(request);
            string xmlResponse = RunOperation(xmlRequest);

            //need to ignore some nav response codes
            string navResponseCode = GetResponseCode(ref xmlResponse);
            if (navResponseCode == "0004")
            {
                //<Response_Text>Unknown Request_ID CO_QTY_AVAILABILITY</Response_Text>
                //0004 = Unknown Request_ID,  NavResponseCode.Error = 0004
                logger.Info("responseCode {0} - this is only supported by default for LS Nav 8.0  ", navResponseCode);
                return new List<OrderLineAvailability>();
            }

            HandleResponseCode(ref xmlResponse);
            return xml.CheckAvailabilityResponseXML(xmlResponse);
        }

        public virtual OrderAvailabilityResponse OrderAvailabilityCheck(OneList request)
        {
            //clickncollect
            if (string.IsNullOrWhiteSpace(request.Id))
                request.Id = GuidHelper.NewGuidString();

            ClickCollectXml xml = new ClickCollectXml();
            string xmlRequest = xml.CheckAvailabilityRequestXML(request);
            string xmlResponse = RunOperation(xmlRequest);

            //need to ignore some nav response codes
            string navResponseCode = GetResponseCode(ref xmlResponse);
            if (navResponseCode == "0004")
            {
                //<Response_Text>Unknown Request_ID CO_QTY_AVAILABILITY</Response_Text>
                //0004 = Unknown Request_ID,  NavResponseCode.Error = 0004
                logger.Info("responseCode {0} - this is only supported by default for LS Nav 11.0 and later", navResponseCode);
                return new OrderAvailabilityResponse();
            }

            if (navResponseCode == "0502")
            {
                logger.Info("responseCode {0} - Inventory Not available for store {1}", navResponseCode, request.StoreId);
                OrderAvailabilityResponse resp = new OrderAvailabilityResponse();
                foreach(OneListItem item in request.Items)
                {
                    resp.Lines.Add(new OrderLineAvailabilityResponse()
                    {
                        ItemId = item.Item.Id,
                        VariantId = (item.VariantReg == null) ? string.Empty : item.VariantReg.Id,
                        UnitOfMeasureId = (item.UnitOfMeasure == null) ? string.Empty : item.UnitOfMeasure.Id,
                        Quantity = 0
                    });
                }
                return resp;
            }

            HandleResponseCode(ref xmlResponse);
            return xml.CheckAvailabilityExtResponseXML(xmlResponse);
        }

        public virtual void OrderCreate(Order request, string tenderMapping)
        {
            if (request == null)
                throw new ApplicationException("OrderCreate request can not be null");

            // need to map the TenderType enum coming from devices to TenderTypeId that NAV knows
            if (request.OrderPayments != null)
            {
                int lineno = 1;
                foreach (OrderPayment line in request.OrderPayments)
                {
                    line.TenderType = TenderTypeMapping(tenderMapping, line.TenderType, false); //map tendertype between lsomni and nav
                    if (line.TenderType == null)
                        throw new ApplicationException("TenderType_Mapping failed for type: " + line.TenderType);
                    line.LineNumber = lineno++;
                    if (NAVVersion.Major < 10)
                        line.FinalizedAmount = line.PreApprovedAmount;
                }
            }

            if (request.ContactAddress == null)
                request.ContactAddress = new Address();

            if (navWS == null)
            {
                string xmlRequest;
                string xmlResponse;
                if (NAVVersion.Major < 10)
                {
                    if (request.ClickAndCollectOrder)
                    {
                        ClickCollectXml xmlcac = new ClickCollectXml();
                        xmlRequest = xmlcac.OrderCreateCACRequestXML(request);
                    }
                    else
                    {
                        BasketXml xmlbask = new BasketXml();
                        xmlRequest = xmlbask.BasketPostSaleRequestXML(request);
                    }
                    xmlResponse = RunOperation(xmlRequest);
                    HandleResponseCode(ref xmlResponse);
                }
                else
                {
                    if (request.ShipToAddress == null)
                    {
                        if (request.ClickAndCollectOrder)
                        {
                            request.ShipToAddress = new Address();
                        }
                        else
                        {
                            throw new ApplicationException("ShipToAddress can not be null if ClickAndCollectOrder is false");
                        }
                    }

                    ClickCollectXml xml = new ClickCollectXml();
                    xmlRequest = xml.OrderCreateRequestXML(request);
                    xmlResponse = RunOperation(xmlRequest);
                    HandleResponseCode(ref xmlResponse);
                }
            }
            else
            {
                if (request.ShipToAddress == null)
                {
                    if (request.ClickAndCollectOrder)
                    {
                        request.ShipToAddress = new Address();
                    }
                    else
                    {
                        throw new ApplicationException("ShipToAddress can not be null if ClickAndCollectOrder is false");
                    }
                }

                // new nav v2 web services
                OrderMapping map = new OrderMapping();
                string respCode = string.Empty;
                string errorText = string.Empty;
                NavWS.Root root = map.MapFromOrderToRoot(request);

                try
                {
                    navWS.CustomerOrderCreate(ref respCode, ref errorText, root);
                    if (respCode != "0000")
                        throw new LSOmniServiceException(StatusCode.TransactionCalc, errorText);
                }
                catch (SoapException e)
                {
                    if (e.Message.Contains("Method"))
                        throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
                    else
                        throw;
                }
            }
        }

        public virtual Order OrderGetById(string id, bool includeLines, string tenderMapping)
        {
            OrderRepository repo = new OrderRepository(NAVVersion);

            if (NAVVersion.Major < 10)
            {
                return repo.OrderOldGetById(id, string.Empty, includeLines);
            }

            Order order = repo.OrderGetById(id, includeLines);
            if (order == null)
                return null;

            if (order.OrderPayments != null)
            {
                foreach (OrderPayment line in order.OrderPayments)
                {
                    line.TenderType = TenderTypeMapping(tenderMapping, line.TenderType, true); //map tendertype between lsomni and nav
                    if (line.TenderType == null)
                        throw new ApplicationException("TenderType_Mapping failed for type: " + line.TenderType);
                }
            }
            return order;
        }

        public virtual Order OrderGetByWebId(string id, bool includeLines, string tenderMapping)
        {
            OrderRepository repo = new OrderRepository(NAVVersion);

            if (NAVVersion.Major < 10)
            {
                return repo.OrderOldGetById(string.Empty, id, includeLines);
            }

            Order order = repo.OrderGetByWebId(id, includeLines);
            if (order == null)
                return null;

            if (order.OrderPayments != null)
            {
                foreach (OrderPayment line in order.OrderPayments)
                {
                    line.TenderType = TenderTypeMapping(tenderMapping, line.TenderType, true); //map tendertype between lsomni and nav
                    if (line.TenderType == null)
                        throw new ApplicationException("TenderType_Mapping failed for type: " + line.TenderType);
                }
            }
            return order;
        }

        public virtual List<Order> OrderHistoryByContactId(string contactId, bool includeLines, bool includeTransactions, string tenderMapping)
        {
            List<Order> list = new List<Order>();
            if (NAVVersion.Major < 10)
            {
                logger.Error("Customer Order featur only supported in NAV 10.x and later");
                includeTransactions = true;
            }
            else
            {
                ContactRepository crepo = new ContactRepository();
                MemberContact contact = crepo.ContactGet(ContactSearchType.ContactNumber, contactId);

                OrderRepository repo = new OrderRepository(NAVVersion);
                list = repo.OrderHistoryByCardId(contact.Card.Id, includeLines);
            }

            if (includeTransactions)
            {
                TransactionRepository trepo = new TransactionRepository(NAVVersion);
                List<LoyTransaction> tlist = trepo.LoyTransactionHeadersGetByContactId(contactId, 0, string.Empty);
                foreach (LoyTransaction trans in tlist)
                {
                    Order order = list.Find(x => x.TransId == trans.Id && x.TransStore == trans.Store.Id && x.TransTerminal == trans.Terminal);
                    if (order != null)
                        continue;   // we have this already in our list

                    // transaction sale only with no order
                    order = new Order()
                    {
                        Id = trans.Id,
                        ContactId = contactId,
                        StoreId = trans.Store.Id,
                        DocumentRegTime = (DateTime)trans.Date,
                        SourceType = SourceType.Standard,
                        ClickAndCollectOrder = false,
                        AnonymousOrder = false,
                        CollectLocation = trans.Store.Id,
                        OrderStatus = OrderStatus.Complete,
                        PaymentStatus = PaymentStatus.Posted,
                        ShippingStatus = ShippingStatus.ShippigNotRequired,
                        ReceiptNo = trans.ReceiptNumber,
                        TotalAmount = trans.Amt,
                        TotalNetAmount = trans.NetAmt,
                        TotalDiscount = trans.DiscountAmt,
                        LineItemCount = (int)trans.TotalQty
                    };
                    list.Add(order);

                    foreach (LoySaleLine line in trans.SaleLines)
                    {
                        OrderLine oline = new OrderLine()
                        {
                            LineNumber = Convert.ToInt32(line.LineNo),
                            VariantId = (line.VariantReg == null) ? string.Empty : line.VariantReg.Id,
                            UomId = (line.Uom == null) ? string.Empty : line.Uom.Id,
                            Quantity = line.Quantity,
                            LineType = LineType.Item,
                            ItemId = line.Item.Id,
                            NetPrice = line.NetPrice,
                            Price = line.Price,
                            DiscountAmount = line.DiscountAmt,
                            NetAmount = line.NetAmt,
                            TaxAmount = line.VatAmt,
                            Amount = line.Amt,
                        };
                        order.OrderLines.Add(oline);
                        order.LineItemCount += (int)oline.Quantity;
                    }
                }
            }

            // fix tender mapping back
            foreach (Order order in list)
            {
                // need to map the TenderType enum coming from devices to TenderTypeId that NAV knows
                if (order.OrderPayments != null)
                {
                    foreach (OrderPayment line in order.OrderPayments)
                    {
                        line.TenderType = TenderTypeMapping(tenderMapping, line.TenderType, true); //map tendertype between lsomni and nav
                        if (line.TenderType == null)
                            throw new ApplicationException("TenderType_Mapping failed for type: " + line.TenderType);
                    }
                }
            }
            return list;
        }

        #endregion

        #region Offer and Advertisement

        public virtual List<PublishedOffer> PublishedOffersGetByCardId(string cardId, string itemId)
        {
            try
            {
                if (NAVVersion.CompareTo(new Version("9.00.05")) >= 0)
                {
                    if (navWS == null)
                    {
                        //the name of the Nav requestID changed between 9.00.04 and 9.00.05 
                        //TODO need to get the Notifications for this ws call too
                        PublishedOfferXml xml = new PublishedOfferXml();
                        string xmlRequest = xml.PublishedOfferMemberRequestXML(cardId, itemId, string.Empty);
                        string xmlResponse = RunOperation(xmlRequest);
                        HandleResponseCode(ref xmlResponse);

                        //cache the xmlresponse so notification can use the cached xmlresponse later
                        if (string.IsNullOrWhiteSpace(cardId) == false)
                        {
                            if (PublishOfferMemCache.Contains(cardId))
                            {
                                PublishOfferMemCache.Remove(cardId);
                            }
                            PublishOfferMemCache.Add(cardId, xmlResponse, 1.0);
                        }
                        return xml.PublishedOfferMemberResponseXML(xmlResponse);
                    }

                    string respCode = string.Empty;
                    string errorText = string.Empty;
                    ContactMapping map = new ContactMapping();
                    NavWS.Root13 root = new NavWS.Root13();

                    try
                    {
                        navWS.GetDirectMarketingInfo(ref respCode, ref errorText, cardId, itemId, string.Empty, ref root);
                        if (respCode != "0000")
                            throw new LSOmniServiceException(StatusCode.NoEntriesFound, errorText);
                    }
                    catch (SoapException e)
                    {
                        if (e.Message.Contains("Method"))
                            throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
                        else
                            throw;
                    }

                    return map.MapFromRootToPublishedOffers(root);
                }
                else
                {
                    PublishedOfferXml xml = new PublishedOfferXml();
                    string xmlRequest = xml.PublishedOfferRequestXML(cardId, itemId);
                    string xmlResponse = RunOperation(xmlRequest);
                    string navResponseCode = GetResponseCode(ref xmlResponse);
                    if (navResponseCode == "0004")
                    {
                        //<Response_Text>Unknown Request_ID LOAD_PUBOFFERS_AND_PERSCOUPONS</Response_Text>
                        //0004 = Unknown Request_ID,  NavResponseCode.Error = 0004
                        logger.Error(
                            "responseCode {0} - LOAD_PUBOFFERS_AND_PERSCOUPONS is only supported by default in LS Nav 9.00.03",
                            navResponseCode);
                        return
                            null; //requestID not found but don't throw an error, may want to key of the null returned
                    }
                    HandleResponseCode(ref xmlResponse);
                    return xml.PublishedOfferResponseXML(xmlResponse);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(LSOmniServiceException))
                    throw ex;
                else
                    throw new LSOmniServiceException(StatusCode.Error, ex.Message, ex);
            }
        }

        public virtual List<Advertisement> AdvertisementsGetById(string id)
        {
            try
            {
                List<Advertisement> ads = new List<Advertisement>();
                string fullFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Xml", "navdata_ads.xml");
                if (File.Exists(fullFileName) == false)
                    return ads;

                string xml = File.ReadAllText(fullFileName);
                AdvertisementXml xmlParse = new AdvertisementXml(xml);
                ads = xmlParse.ParseXml(id);
                return ads;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(LSOmniServiceException))
                    throw ex;
                else
                    throw new LSOmniServiceException(StatusCode.Error, ex.Message, ex);
            }
        }

        #endregion

        #region Menu

        public virtual MobileMenu MenusGet(string storeId, Currency currency)
        {
            try
            {
                MenuXml menuxml = new MenuXml();
                string xmlRequest = menuxml.MenuGetAllRequestXML(storeId, "", "");  //all blank behaves like old HospLoy
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                return menuxml.MenuGetAllResponseXML(xmlResponse, currency);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(LSOmniServiceException))
                    throw ex;
                else
                    throw new LSOmniServiceException(StatusCode.Error, ex.Message, ex);
            }
        }

        #endregion

        #region Image

        public virtual ImageView ImageBOGetById(string imageId)
        {
            ImageRepository rep = new ImageRepository();
            return rep.ImageGetById(imageId);
        }

        public virtual List<ImageView> ImageBOGetByKey(string tableName, string key1, string key2, string key3, int imgCount, bool includeBlob)
        {
            ImageRepository rep = new ImageRepository();
            return rep.ImageGetByKey(tableName, key1, key2, key3, imgCount, includeBlob);
        }

        #endregion

        #region Store

        public virtual List<StoreHours> StoreHoursGetByStoreId(string storeId, int offset, int dayOfWeekOffset)
        {
            StoreRepository rep = new StoreRepository(NAVVersion);
            return rep.StoreHoursGetByStoreId(storeId, offset, dayOfWeekOffset);
        }

        public virtual Store StoreGetById(string id)
        {
            StoreRepository rep = new StoreRepository(NAVVersion);
            return rep.StoreLoyGetById(id, true);
        }

        public virtual List<Store> StoresLoyGetByCoordinates(double latitude, double longitude, double maxDistance, int maxNumberOfStores, Store.DistanceType units)
        {
            StoreRepository rep = new StoreRepository(NAVVersion);
            return rep.StoresLoyGetByCoordinates(latitude, longitude, maxDistance, maxNumberOfStores, units);
        }

        public virtual List<Store> StoresGetAll(bool clickAndCollectOnly)
        {
            StoreRepository rep = new StoreRepository(NAVVersion);
            return rep.StoreLoyGetAll(clickAndCollectOnly);
        }

        public virtual string GetWIStoreId()
        {
            StoreRepository rep = new StoreRepository(NAVVersion);
            return rep.GetWIStoreId();
        }

        public virtual List<StoreServices> StoreServicesGetByStoreId(string storeId)
        {
            try
            {
                List<StoreServices> serviceListFound = new List<StoreServices>();
                string fullFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Xml", "navdata_StoreFeatures.xml");
                if (File.Exists(fullFileName) == false)
                    return serviceListFound;

                string xml = File.ReadAllText(fullFileName);
                StoreServicesXml xmlParse = new StoreServicesXml(xml);
                List<StoreServices> serviceList = xmlParse.ParseXml();
                foreach (StoreServices serv in serviceList)
                {
                    if (serv.StoreId.ToLowerInvariant() == storeId.ToLowerInvariant())
                        serviceListFound.Add(serv);
                }
                return serviceListFound;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(LSOmniServiceException))
                    throw ex;
                else
                    throw new LSOmniServiceException(StatusCode.Error, ex.Message, ex);
            }
        }

        #endregion

        #region Ecomm Replication

        public virtual List<ReplImageLink> ReplEcommImageLinks(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ImageRepository rep = new ImageRepository();
            return rep.ReplEcommImageLink(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplImage> ReplEcommImages(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ImageRepository rep = new ImageRepository();
            return rep.ReplEcommImage(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplAttribute> ReplEcommAttribute(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            AttributeRepository rep = new AttributeRepository();
            return rep.ReplicateEcommAttribute(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplAttributeValue> ReplEcommAttributeValue(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            AttributeValueRepository rep = new AttributeValueRepository();
            return rep.ReplicateEcommAttributeValue(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplAttributeOptionValue> ReplEcommAttributeOptionValue(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            AttributeOptionValueRepository rep = new AttributeOptionValueRepository();
            return rep.ReplicateEcommAttributeOptionValue(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplLoyVendorItemMapping> ReplEcommVendorItemMapping(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            VendorItemMappingRepository rep = new VendorItemMappingRepository();
            return rep.ReplicateEcommVendorItemMapping(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDataTranslation> ReplEcommDataTranslation(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error("Only supported in NAV 10.x and later");
                return new List<ReplDataTranslation>();
            }

            DataTranslationRepository rep = new DataTranslationRepository();
            return rep.ReplicateEcommDataTranslation(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplShippingAgent> ReplEcommShippingAgent(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ShippingRepository rep = new ShippingRepository();
            return rep.ReplicateShipping(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCustomer> ReplEcommMember(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ContactRepository rep = new ContactRepository();
            return rep.ReplicateMembers(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCountryCode> ReplEcommCountryCode(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ShippingRepository rep = new ShippingRepository();
            lastKey = string.Empty;
            maxKey = string.Empty;
            recordsRemaining = 0;
            return rep.ReplicateCountryCode();
        }

        public virtual List<LoyItem> ReplEcommFullItem(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemRepository rep = new ItemRepository();
            return rep.ReplicateEcommFullItems(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        #endregion

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
                logger.Error(msg + ex.Message);
                throw;
            }
        }
    }
}
