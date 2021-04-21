using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping;
using LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Loyalty;
using LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Replication;
using LSOmni.DataAccess.BOConnection.NavCommon.Mapping;

using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;

namespace LSOmni.DataAccess.BOConnection.NavCommon
{
    public partial class NavCommonBase
    {
        protected static MemCache PublishOfferMemCache = null; //minimize the calls to Nav web service

        #region Item

        public LoyItem ItemGetById(string id)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item", "No.", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository();
            LoyItem item = rep.ItemGet(table);

            xmlRequest = xml.GetGeneralWebRequestXML("Item Unit of Measure", "Item No.", item.Id);
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            table = xml.GetGeneralWebResponseXML(xmlResponse);
            item.UnitOfMeasures = rep.GetUnitOfMeasure(table);

            xmlRequest = xml.GetGeneralWebRequestXML("Item Variant Registration", "Item No.", item.Id);
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            table = xml.GetGeneralWebResponseXML(xmlResponse);
            item.VariantsRegistration = rep.GetVariantRegistrations(table);

            xmlRequest = xml.GetGeneralWebRequestXML("Extended Variant Values", "Item No.", item.Id);
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            table = xml.GetGeneralWebResponseXML(xmlResponse);
            item.VariantsExt = rep.GetVariantExt(table);

            foreach (VariantExt ext in item.VariantsExt)
            {
                xmlRequest = xml.GetGeneralWebRequestXML("Extended Variant Values", "Item No.", ext.ItemId, "Code", ext.Code);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                ext.Values = rep.GetDimValues(table);
            }

            return item;
        }

        public LoyItem ItemGetByBarcode(string barcode)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Barcodes", "Barcode No.", barcode);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            if (table == null || table.NumberOfValues == 0)
                return null;

            XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Item No."));
            string itemId = field.Values[0];
            field = table.FieldList.Find(f => f.FieldName.Equals("Variant Code"));
            string variantId = field.Values[0];
            field = table.FieldList.Find(f => f.FieldName.Equals("Unit of Measure Code"));
            string uomId = field.Values[0];

            LoyItem item = ItemGetById(itemId);

            if (string.IsNullOrWhiteSpace(variantId) == false)
            {
                item.SelectedVariant = item.VariantsRegistration.Find(v => v.Id == variantId);
            }

            if (string.IsNullOrWhiteSpace(uomId) == false)
            {
                item.SelectedUnitOfMeasure = item.UnitOfMeasures.Find(u => u.Id == uomId);
            }

            return item;
        }

        public List<InventoryResponse> ItemInStockGet(string itemId, string variantId, int arrivingInStockInDays, List<string> locationIds, bool skipUnAvailableStores)
        {
            //locationIds are storeIds in NAV
            if (navWS == null)
            {
                ItemInventoryXml inventoryXml = new ItemInventoryXml();
                string xmlRequest = inventoryXml.ItemsInStockRequestXML(itemId, variantId, arrivingInStockInDays, locationIds);
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                List<InventoryResponse> stores = inventoryXml.ItemsInStockResponseXML(xmlResponse, itemId, variantId);
                if (skipUnAvailableStores == false)
                    return stores;

                //only return those stores that have inventory 
                List<InventoryResponse> storeList = new List<InventoryResponse>();
                foreach (InventoryResponse navStore in stores)
                {
                    if (navStore.QtyInventory > 0)   //actual inventory are positive when item exist in inventory, otherwise negative if not exist
                    {
                        storeList.Add(navStore);
                    }
                }
                return storeList;
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            List<InventoryResponse> list = new List<InventoryResponse>();
            NavWS.RootGetItemInventory root = new NavWS.RootGetItemInventory();
            foreach (string id in locationIds)
            {
                navWS.GetItemInventory(ref respCode, ref errorText, itemId, variantId, id, string.Empty, string.Empty, string.Empty, string.Empty, arrivingInStockInDays, ref root);
                HandleWS2ResponseCode("GetItemInventory", respCode, errorText);
                logger.Debug(config.LSKey.Key, "GetItemInventory Response - " + Serialization.ToXml(root, true));

                if (root.WSInventoryBuffer == null)
                    continue;

                foreach (NavWS.WSInventoryBuffer1 buffer in root.WSInventoryBuffer)
                {
                    if (skipUnAvailableStores && buffer.ActualInventory <= 0)
                        continue;

                    list.Add(new InventoryResponse()
                    {
                        ItemId = buffer.ItemNo,
                        VariantId = buffer.VariantCode,
                        BaseUnitOfMeasure = buffer.BaseUnitofMeasure,
                        QtyInventory = buffer.ActualInventory,
                        StoreId = buffer.StoreNo
                    });
                }
            }
            return list;
        }

        public virtual List<InventoryResponse> ItemsInStockGet(List<InventoryRequest> items, string storeId, string locationId)
        {
            List<InventoryResponse> list = new List<InventoryResponse>();

            if (navWS == null || NAVVersion < new Version("13.5"))
            {
                OneList olist = new OneList()
                {
                    StoreId = storeId
                };
                foreach (InventoryRequest ir in items)
                {
                    olist.Items.Add(new OneListItem()
                    {
                        ItemId = ir.ItemId,
                        VariantId = ir.VariantId
                    });
                }
                OrderAvailabilityResponse rep = OrderAvailabilityCheck(olist);
                foreach (OrderLineAvailabilityResponse ol in rep.Lines)
                {
                    list.Add(new InventoryResponse()
                    {
                        ItemId = ol.ItemId,
                        VariantId = ol.VariantId,
                        BaseUnitOfMeasure = ol.UnitOfMeasureId,
                        QtyInventory = ol.Quantity,
                        StoreId = storeId
                    });
                }
                return list;
            }

            string respCode = string.Empty;
            string errorText = string.Empty;

            List<NavWS.InventoryBufferIn> lines = new List<NavWS.InventoryBufferIn>();
            foreach (InventoryRequest item in items)
            {
                NavWS.InventoryBufferIn buf = lines.Find(b => b.Number.Equals(item.ItemId) && b.Variant.Equals(item.VariantId ?? string.Empty));
                if (buf == null)
                {
                    lines.Add(new NavWS.InventoryBufferIn()
                    {
                        Number = item.ItemId,
                        Variant = item.VariantId ?? string.Empty,
                    });
                }
            }

            NavWS.RootGetInventoryMultipleIn rootin = new NavWS.RootGetInventoryMultipleIn()
            {
                InventoryBufferIn = lines.ToArray()
            };

            NavWS.RootGetInventoryMultipleOut rootout = new NavWS.RootGetInventoryMultipleOut();

            navWS.GetInventoryMultiple(ref respCode, ref errorText, storeId, locationId, rootin, ref rootout);
            HandleWS2ResponseCode("GetInventoryMultiple", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetInventoryMultiple Response - " + Serialization.ToXml(rootout, true));

            if (rootout.InventoryBufferOut == null)
                return list;

            foreach (NavWS.InventoryBufferOut buffer in rootout.InventoryBufferOut)
            {
                list.Add(new InventoryResponse()
                {
                    ItemId = buffer.Number,
                    VariantId = buffer.Variant,
                    QtyInventory = buffer.Inventory,
                    StoreId = buffer.Store
                });
            }
            return list;
        }

        public List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems)
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
                logger.Error(config.LSKey.Key, "responseCode {0} - LOAD_PUBLISHED_OFFER_ITEMS is only supported by default in LS Nav 9.00.03", navResponseCode);
                return list; //request not found so return empty list instead of breaking
            }
            HandleResponseCode(ref xmlResponse);
            return xml.PublishedOfferItemsResponseXML(xmlResponse);
        }

        public List<LoyItem> ItemsGetByProductGroup(string prodGroup)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item", ((NAVVersion > new Version("14.2")) ? "Retail Product Code" : "Product Group Code"), prodGroup);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository();
            return rep.ItemsGet(table);
        }

        #endregion

        #region Item Related

        public VariantRegistration VariantRegGetById(string id, string itemId)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item Variant Registration", "Variant", id, "Item No.", itemId);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository();
            List<VariantRegistration> list = rep.GetVariantRegistrations(table);
            return (list.Count == 0) ? null : list[0];
        }

        public ProductGroup ProductGroupGetById(string id)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML(pgtablename, "Code", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository();
            List<ProductGroup> list = rep.ProductGroupGet(table);
            return (list.Count == 0) ? null : list[0];
        }

        public List<ProductGroup> ProductGroupGetByItemCategory(string id)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML(pgtablename, "Item Category Code", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository();
            return rep.ProductGroupGet(table);
        }

        public ItemCategory ItemCategoriesGetById(string id)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item Category", "Code", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository();
            List<ItemCategory> list = rep.ItemCategoryGet(table);
            return (list.Count == 0) ? null : list[0];
        }

        public List<ItemCategory> ItemCategories()
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item Category");
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository();
            return rep.ItemCategoryGet(table);
        }

        public ImageView ImageGetById(string imageId)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Retail Image", "Code", imageId);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository();
            return rep.GetImage(table);
        }

        public List<ImageView> ImagesGetByLink(string tableName, string key1, string key2, string key3)
        {

            string keyvalue = key1;
            if (string.IsNullOrWhiteSpace(key2) == false)
                keyvalue += "," + key2;
            if (string.IsNullOrWhiteSpace(key3) == false)
                keyvalue += "," + key3;

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Retail Image Link", "KeyValue", keyvalue, "TableName", tableName);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository();
            List<ImageView> list = rep.GetImageLinks(table);

            foreach (ImageView link in list)
            {
                using (ImageView img = ImageGetById(link.Id))
                {
                    link.Location = img.Location;
                    link.LocationType = img.LocationType;
                    link.Image = img.Image;
                    link.ImgBytes = img.ImgBytes;
                }
            }
            return list;
        }

        public List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items)
        {
            if (string.IsNullOrWhiteSpace(storeId))
                throw new LSOmniServiceException(StatusCode.MissingStoreId, "Missing Store Id");
            if (string.IsNullOrWhiteSpace(cardId))
                throw new LSOmniServiceException(StatusCode.MemberCardNotFound, "Missing Member Card Id");

            if (items == null && items.Count == 0)
                return new List<ItemCustomerPrice>();

            OrderMapping map = new OrderMapping(NAVVersion, config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;
            NavWS.RootMobileTransaction root = map.MapFromCustItemToRoot(storeId, cardId, items);

            logger.Debug(config.LSKey.Key, "EcomGetCustomerPrice Request - " + Serialization.ToXml(root, true));

            navQryWS.EcomGetCustomerPrice(ref respCode, ref errorText, ref root);
            HandleWS2ResponseCode("EcomGetCustomerPrice", respCode, errorText);
            logger.Debug(config.LSKey.Key, "EcomGetCustomerPrice Response - " + Serialization.ToXml(root, true));
            return map.MapFromRootTransactionToItemCustPrice(root);
        }

        #endregion

        #region Contact

        public string ContactCreate(MemberContact contact)
        {
            if (contact == null)
                throw new LSOmniException(StatusCode.ContactIdNotFound, "Contact can not be null");

            //must have a deviceId, otherwise no "Member Login Card" entry is made in nav
            if (contact.LoggedOnToDevice == null)
            {
                contact.LoggedOnToDevice = new Device(GetDefaultDeviceId(contact.UserName));
            }
            if (string.IsNullOrWhiteSpace(contact.LoggedOnToDevice.Id))
            {
                contact.LoggedOnToDevice.Id = GetDefaultDeviceId(contact.UserName);
            }
            if (contact.Profiles == null)
            {
                contact.Profiles = new List<Profile>();
            }
            if (string.IsNullOrWhiteSpace(contact.LoggedOnToDevice.DeviceFriendlyName))
                contact.LoggedOnToDevice.DeviceFriendlyName = "Web application";

            if (navWS == null)
            {
                NavXml navXml = new NavXml();
                string xmlRequest = navXml.ContactCreateRequestXML(contact);
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                return navXml.ContactCreateResponseXML(xmlResponse);
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            ContactMapping map = new ContactMapping(config.IsJson);

            string clubId = string.Empty;
            string cardId = string.Empty;
            string contId = string.Empty;
            string acctId = string.Empty;
            string schmId = string.Empty;
            decimal point = 0;

            NavWS.RootMemberContactCreate root = map.MapToRoot(contact);
            logger.Debug(config.LSKey.Key, "MemberContactCreate Request - " + Serialization.ToXml(root, true));

            navWS.MemberContactCreate(ref respCode, ref errorText, ref clubId, ref schmId, ref acctId, ref contId, ref cardId, ref point, ref root);
            HandleWS2ResponseCode("MemberContactCreate", respCode, errorText);
            logger.Debug(config.LSKey.Key, "MemberContactCreate Response - ClubId: {0}, SchemeId: {1}, AccountId: {2}, ContactId: {3}, CardId: {4}, PointsRemaining: {5}",
                clubId, schmId, acctId, contId, cardId, point);
            return cardId;
        }

        public void ContactUpdate(MemberContact contact, string accountId)
        {
            NavXml navXml = new NavXml();
            if (contact == null)
                throw new LSOmniException(StatusCode.ContactIdNotFound, "ContactRq can not be null");

            if (contact.Profiles == null)
            {
                contact.Profiles = new List<Profile>();
            }

            if (navWS == null)
            {
                string xmlRequest = navXml.ContactUpdateRequestXML(contact, accountId);
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                return;
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            ContactMapping map = new ContactMapping(config.IsJson);

            NavWS.RootMemberContactCreate1 root = map.MapToRoot1(contact, accountId);
            logger.Debug(config.LSKey.Key, "MemberContactUpdate Request - " + Serialization.ToXml(root, true));
            navWS.MemberContactUpdate(ref respCode, ref errorText, ref root);
            HandleWS2ResponseCode("MemberContactUpdate", respCode, errorText);
        }

        public MemberContact ContactGet(string contactId, string accountId, string card, string loginId, string email, bool includeDetails)
        {
            string xmlRequest;
            string xmlResponse;
            string respCode = string.Empty;
            string errorText = string.Empty;
            XMLTableData table;
            XMLFieldData field;
            NAVWebXml xml = new NAVWebXml();
            ContactMapping map = new ContactMapping(config.IsJson);
            MemberContact contact = null;

            NavWS.RootGetMemberContact rootContact = new NavWS.RootGetMemberContact();
            logger.Debug(config.LSKey.Key, "GetMemberContact - CardId: {0}", card);
            if (NAVVersion < new Version("16.2"))
            {
                if (string.IsNullOrEmpty(loginId) == false)
                {
                    xmlRequest = xml.GetGeneralWebRequestXML("Member Login Card", "Login ID", loginId.ToLower(), 1);
                    xmlResponse = RunOperation(xmlRequest);
                    HandleResponseCode(ref xmlResponse);
                    table = xml.GetGeneralWebResponseXML(xmlResponse);

                    if (table == null || table.NumberOfValues == 0)
                        return null;

                    field = table.FieldList.Find(f => f.FieldName.Equals("Card No."));
                    card = field.Values[0];
                }
                navWS.GetMemberContact(ref respCode, ref errorText, card, accountId, contactId, ref rootContact);
            }
            else
            {
                navWS.GetMemberContact2(ref respCode, ref errorText, card, accountId, contactId, loginId.ToLower(), email.ToLower(), ref rootContact);
            }
            if (respCode == "1000") // not found
                return null;

            HandleWS2ResponseCode("GetMemberContact", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetMemberContact Response - " + Serialization.ToXml(rootContact, true));
            
            contact = map.MapFromRootToContact(rootContact);
            contact.UserName = loginId;
            card = contact.Cards.FirstOrDefault().Id;

            if (string.IsNullOrEmpty(loginId))
            {
                xmlRequest = xml.GetGeneralWebRequestXML("Member Login Card", "Card No.", card, 1);
                xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);

                if (table == null || table.NumberOfValues == 0)
                    return null;

                field = table.FieldList.Find(f => f.FieldName.Equals("Login ID"));
                contact.UserName = field.Values[0];
            }

            xmlRequest = xml.GetGeneralWebRequestXML("Member Login", "Login ID", contact.UserName.ToLower(), 1);
            xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            table = xml.GetGeneralWebResponseXML(xmlResponse);

            if (table == null || table.NumberOfValues == 0)
                return null;

            field = table.FieldList.Find(f => f.FieldName.Equals("Password"));
            contact.Password = field.Values[0];

            if (includeDetails == false)
                return contact;

            decimal remainingPoints = 0;
            contact.Profiles = ProfileGet(card, ref remainingPoints);
            contact.Account.PointBalance = (remainingPoints == 0) ? contact.Account.PointBalance : Convert.ToInt64(Math.Floor(remainingPoints));

            NavWS.RootGetDirectMarketingInfo rootMarket = new NavWS.RootGetDirectMarketingInfo();
            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo - CardId: {0}", card);
            navWS.GetDirectMarketingInfo(ref respCode, ref errorText, card, string.Empty, string.Empty, ref rootMarket);
            HandleWS2ResponseCode("GetDirectMarketingInfo", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetDirectMarketing Response - " + Serialization.ToXml(rootMarket, true));

            contact.PublishedOffers = map.MapFromRootToPublishedOffers(rootMarket);

            contact.SalesEntries = new List<SalesEntry>();
            contact.OneLists = new List<OneList>();
            return contact;
        }

        public MemberContact ContactGetByUserName(string username, bool includeDetails)
        {
            return ContactGet(string.Empty, string.Empty, string.Empty, username, string.Empty, includeDetails);
        }

        public MemberContact ContactGetByEmail(string email, bool includeDetails)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Member Contact", "E-Mail", email, 1);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            if (table == null || table.NumberOfValues == 0)
                return null;

            XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Contact No."));
            string contactId = field.Values[0];
            field = table.FieldList.Find(f => f.FieldName.Equals("Account No."));
            string accountId = field.Values[0];

            return ContactGet(contactId, accountId, string.Empty, string.Empty, string.Empty, includeDetails);
        }

        public double ContactAddCard(string contactId, string accountId, string cardId)
        {
            if (NAVVersion < new Version("16.2"))
            {
                NavXml navXml = new NavXml();
                string xmlRequest = navXml.ContactAddCardRequestXML(contactId, accountId, cardId);
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                return navXml.ContactAddCardResponseXml(xmlResponse);
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            decimal points = 0;

            navWS.MemberCardToContact(ref respCode, ref errorText, cardId, accountId, contactId, ref points);
            HandleWS2ResponseCode("GetDirectMarketingInfo", respCode, errorText);
            logger.Debug(config.LSKey.Key, "MemberCardToContact Response - points:", points);
            return (double)points;
        }

        #endregion

        #region Contact Related

        public List<Profile> ProfileGetAll()
        {
            if (NAVVersion < new Version("16.2"))
            {
                NavXml navXml = new NavXml();
                string xmlRequest = navXml.ProfilesRequestXML(string.Empty);
                string xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                return navXml.ProfileResponseXML(xmlResponse);
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            NavWS.RootMobileGetProfiles root = new NavWS.RootMobileGetProfiles();

            navWS.MobileGetProfiles(ref respCode, ref errorText, string.Empty, string.Empty, ref root);
            HandleWS2ResponseCode("MobileGetProfiles", respCode, errorText);
            logger.Debug(config.LSKey.Key, "MobileGetProfiles Response - " + Serialization.ToXml(root, true));
            ContactMapping map = new ContactMapping(config.IsJson);
            return map.MapFromRootToProfiles(root);
        }


        public List<Profile> ProfileGet(string cardId, ref decimal remainingPoints)
        {
            List<Profile> list = new List<Profile>();

            string respCode = string.Empty;
            string errorText = string.Empty;
            NavWS.RootGetMemberCard rootCard = new NavWS.RootGetMemberCard();

            logger.Debug(config.LSKey.Key, "GetMemberCard - CardId: {0}", cardId);
            navWS.GetMemberCard(ref respCode, ref errorText, cardId, ref remainingPoints, ref rootCard);
            HandleWS2ResponseCode("GetMemberCard", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetMemberCard Response - " + Serialization.ToXml(rootCard, true));

            foreach (NavWS.MemberAttributeList att in rootCard.MemberAttributeList)
            {
                if (att.Type != "0")
                    continue;

                list.Add(new Profile()
                {
                    Id = att.Code,
                    Description = att.Description,
                    DefaultValue = att.Value,
                    ContactValue = true
                });
            }
            return list;
        }

        public List<Scheme> SchemeGetAll()
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Member Scheme");
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ContactRepository rep = new ContactRepository();
            return rep.SchemeGetAll(table);
        }

        public MemberContact Logon(string userName, string password, string deviceID, string deviceName, bool includeDetails)
        {
            MemberContact contact = null;
            if (NAVVersion < new Version("16.3"))
            {
                NavXml navXml = new NavXml();
                NAVWebXml xml = new NAVWebXml();
                XMLTableData table;
                XMLFieldData field;
                string xmlRequest;
                string xmlResponse;

                xmlRequest = xml.GetGeneralWebRequestXML("Member Login Card", "Login ID", userName.ToLower(), 1);
                xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);

                if (table == null || table.NumberOfValues == 0)
                    return null;

                field = table.FieldList.Find(f => f.FieldName.Equals("Card No."));
                string card = field.Values[0];

                xmlRequest = navXml.LoginRequestXML(userName, password, card);
                xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);

                //CALL NAV web service but don't send in the cardId since that will try and link the card to user 
                //and we only want to link the device to the user
                xmlRequest = navXml.CreateDeviceAndLinkToUser(userName, deviceID, deviceName, card);
                xmlResponse = RunOperation(xmlRequest);
                return contact;
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            NavWS.RootMemberLogon root = new NavWS.RootMemberLogon();
            decimal remainingPoints = 0;

            logger.Debug(config.LSKey.Key, "MemberLogon - userName: {0}", userName);
            navWS.MemberLogon(ref respCode, ref errorText, userName, password, deviceID, deviceName, ref remainingPoints, ref root);
            HandleWS2ResponseCode("MemberLogon", respCode, errorText);
            logger.Debug(config.LSKey.Key, "MemberLogon Response - " + Serialization.ToXml(root, true));
            
            ContactMapping map = new ContactMapping(config.IsJson);
            contact = map.MapFromRootToLogonContact(root, remainingPoints);
            contact.UserName = userName;
            if (includeDetails)
                contact.PublishedOffers = PublishedOffersGet(contact.Cards.FirstOrDefault().Id, string.Empty, string.Empty);

            return contact;
        }

        //Change the password in NAV
        public void ChangePassword(string userName, string token, string newPassword, string oldPassword)
        {
            if (NAVVersion < new Version("16.2"))
            {
                if (string.IsNullOrEmpty(oldPassword))
                {
                    logger.Debug(config.LSKey.Key, "Calling old NAV WS1 ChangePassword works only with oldPassword provided");
                    return;
                }

                NavXml navXml = new NavXml();
                string xmlRequest = navXml.ChangePasswordRequestXML(userName, newPassword, oldPassword);
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                return;
            }

            string respCode = string.Empty;
            string errorText = string.Empty;

            logger.Debug(config.LSKey.Key, "MemberPasswordChange - UserName:{0} Token:{1}", userName, token);
            navWS.MemberPasswordChange(ref respCode, ref errorText, userName, token, oldPassword, newPassword);
            HandleWS2ResponseCode("MemberPasswordChange", respCode, errorText);
        }

        public string ResetPassword(string userName, string email, string newPassword)
        {
            //Reset the password in NAV
            //used when a user forgot his password, and has been validated (URL sent to him)

            //If newPassword is empty then NAV creates a random password ?
            //return the Encrypted password that NAV returned to us
            if (NAVVersion < new Version("16.2"))
            {
                if (string.IsNullOrEmpty(newPassword))
                {
                    logger.Debug(config.LSKey.Key, "Calling old NAV WS1 ResetPassword works only with newPassword provided");
                    return string.Empty;
                }

                NavXml navXml = new NavXml();
                string xmlRequest = navXml.ResetPasswordRequestXML(userName, newPassword);
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                return string.Empty;
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            string token = string.Empty;
            DateTime tokenExp = DateTime.Now.AddMonths(1);

            logger.Debug(config.LSKey.Key, "MemberPasswordReset - UserName:{0} Email:{1}", userName, email);
            navWS.MemberPasswordReset(ref respCode, ref errorText, userName, email, ref token, ref tokenExp);
            HandleWS2ResponseCode("MemberPasswordReset", respCode, errorText);
            logger.Debug(config.LSKey.Key, "MemberPasswordReset Response - Token:{0}", token);
            return token;
        }

        public void LoginChange(string oldUserName, string newUserName, string password)
        {
            NavXml navXml = new NavXml();
            string xmlRequest = navXml.ChangeLoginRequestXML(oldUserName, newUserName, password);
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
        }

        public long MemberCardGetPoints(string cardId)
        {
            if (string.IsNullOrWhiteSpace(cardId))
                throw new LSOmniException(StatusCode.CardIdInvalid, "cardId can not be empty");

            if (navWS == null)
            {
                NavXml xml = new NavXml();
                string xmlRequest = xml.MemberCardRequestXML(cardId);
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                return xml.MemberCardPointsResponse(xmlResponse);
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            NavWS.RootGetMemberCard root = new NavWS.RootGetMemberCard();
            decimal remainingPoints = 0;

            logger.Debug(config.LSKey.Key, "GetMemberCard - CardId: {0}", cardId);
            navWS.GetMemberCard(ref respCode, ref errorText, cardId, ref remainingPoints, ref root);
            HandleWS2ResponseCode("GetMemberCard", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetMemberCard Response - Remaining points: {0}", Convert.ToInt64(Math.Floor(remainingPoints)));
            return Convert.ToInt64(Math.Floor(remainingPoints));
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo, string entryType)
        {
            if (navWS == null)
                throw new LSOmniException(StatusCode.NAVWebFunctionNotFound, "Only supported in WS2");

            string respCode = string.Empty;
            string errorText = string.Empty;
            NavWS.RootGetDataEntryBalance root = new NavWS.RootGetDataEntryBalance();
            logger.Debug(config.LSKey.Key, "GetDataEntryBalance - GiftCardNo: {0}", cardNo);
            navWS.GetDataEntryBalance(ref respCode, ref errorText, entryType, cardNo, ref root);
            if (root.POSDataEntry == null)
            {
                throw new LSOmniServiceException(StatusCode.GiftCardNotFound, "Gift card not found");
            }

            logger.Debug(config.LSKey.Key, "GetDataEntryBalance response - Balance:{0} Expire:{1}", root.POSDataEntry.First().Balance, root.POSDataEntry.First().ExpiryDate);
            return new GiftCard(cardNo)
            {
                Balance = root.POSDataEntry.First().Balance,
                ExpireDate = root.POSDataEntry.First().ExpiryDate
            };
        }

        #endregion

        #region Searches

        public List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned)
        {
            string fldname = "Search Name";
            switch (searchType)
            {
                case ContactSearchType.ContactNumber:
                    fldname = "Contact No.";
                    break;
                case ContactSearchType.Email:
                    fldname = "E-Mail";
                    break;
                case ContactSearchType.PhoneNumber:
                    fldname = "Mobile Phone No.";
                    break;
                case ContactSearchType.Name:
                    fldname = "Search Name";
                    search = search.ToUpper();
                    break;
            }

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Member Contact", fldname, search, maxNumberOfRowsReturned);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ContactRepository rep = new ContactRepository();
            List<MemberContact> list = rep.ContactGet(table);

            foreach (MemberContact contact in list)
            {
                xmlRequest = xml.GetGeneralWebRequestXML("Membership Card", "Contact No.", contact.Id);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                contact.Cards = rep.CardGet(table);
            }
            return list;
        }

        public List<LoyItem> ItemSearch(string search)
        {
            search = string.Format("*{0}*", search.ToUpper());

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item", "Search Description", search);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository();
            return rep.ItemsGet(table);
        }

        public List<ItemCategory> ItemCategorySearch(string search)
        {
            search = string.Format("*{0}*", search);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item Category", "Description", search);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository();
            return rep.ItemCategoryGet(table);
        }

        public List<ProductGroup> ProductGroupSearch(string search)
        {
            search = string.Format("*{0}*", search);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML(pgtablename, "Description", search);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository();
            return rep.ProductGroupGet(table);
        }

        public List<Profile> ProfileSearch(string search)
        {
            search = string.Format("*{0}*", search);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Member Attribute", "Description", search);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ContactRepository rep = new ContactRepository();
            return rep.ProfileGet(table);
        }

        public List<Store> StoreSearch(string search)
        {
            search = string.Format("*{0}*", search);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Store", "Name", search);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository();
            return rep.StoresGet(table);
        }

        #endregion

        #region Hospitality Order

        public OrderHosp HospOrderCalculate(OneList list)
        {
            if (string.IsNullOrWhiteSpace(list.Id))
                list.Id = GuidHelper.NewGuidString();

            string[] salesTypes = config.SettingsGetByKey(ConfigKey.Hosp_SalesType).Split(',');
            if (salesTypes.Length == 0)
                throw new LSOmniServiceException(StatusCode.GetSalesTypeError, "Hosp_SalesType data missing in TenderConfig table");

            string salesType = salesTypes[0];
            if (list.HospitalityMode == HospMode.Takeaway && salesTypes.Length == 3)
                salesType = salesTypes[1];
            if (list.HospitalityMode == HospMode.Delivery && salesTypes.Length == 3)
                salesType = salesTypes[2];

            if (navWS == null)
            {
                HospitalityXml xml = new HospitalityXml();
                string xmlRequest = xml.OrderToPOSRequestXML(list, "CALCULATE",
                    config.SettingsGetByKey(ConfigKey.Hosp_Terminal),
                    config.SettingsGetByKey(ConfigKey.Hosp_Staff),
                    salesType);
                string xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                return xml.TransactionResponseXML(xmlResponse);
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            TransactionMapping map = new TransactionMapping(NAVVersion, config.IsJson);
            NavWS.RootMobileTransaction root = map.MapFromOrderToRoot(list, salesType);

            logger.Debug(config.LSKey.Key, "MobilePosPost Request - " + Serialization.ToXml(root, true));
            navQryWS.MobilePosCalculate(ref respCode, ref errorText, string.Empty, ref root);
            HandleWS2ResponseCode("MobilePosPost", respCode, errorText);
            logger.Debug(config.LSKey.Key, "MobilePosPost Response - " + Serialization.ToXml(root, true));
            return map.MapFromRootToOrderHosp(root, salesTypes);
        }

        public string HospOrderCreate(OrderHosp request, string tenderMapping)
        {
            if (request == null)
                throw new LSOmniException(StatusCode.OrderIdNotFound, "OrderCreate request can not be null");

            if (string.IsNullOrWhiteSpace(request.Id))
                request.Id = GuidHelper.NewGuidString();

            // need to map the TenderType enum coming from devices to TenderTypeId that NAV knows
            if (request.OrderPayments == null)
                request.OrderPayments = new List<OrderPayment>();

            int lineno = 1;
            foreach (OrderPayment line in request.OrderPayments)
            {
                line.TenderType = TenderTypeMapping(tenderMapping, line.TenderType, false); //map tender type between LSOmni and Nav
                if (line.TenderType == null)
                    throw new LSOmniServiceException(StatusCode.TenderTypeNotFound, "TenderType_Mapping failed for type: " + line.TenderType);
                line.LineNumber = lineno++;
            }

            string[] salesTypes = config.SettingsGetByKey(ConfigKey.Hosp_SalesType).Split(',');
            if (salesTypes.Length == 0)
                throw new LSOmniServiceException(StatusCode.GetSalesTypeError, "Hosp_SalesType data missing in TenderConfig table");

            string salesType = salesTypes[0];
            if (request.DeliveryType == HospDeliveryType.Takeout && salesTypes.Length == 3)
                salesType = salesTypes[1];
            if ((request.DeliveryType == HospDeliveryType.Home || request.DeliveryType == HospDeliveryType.Work) && salesTypes.Length == 3)
                salesType = salesTypes[2];

            if (navWS == null)
            {
                HospitalityXml xml = new HospitalityXml();
                string xmlRequest = xml.OrderToPOSRequestXML(request, "POST",
                    config.SettingsGetByKey(ConfigKey.Hosp_Terminal),
                    config.SettingsGetByKey(ConfigKey.Hosp_Staff),
                    salesType);
                string xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                return xml.TransactionReceiptNoResponseXML(xmlResponse);
            }

            string receiptNo = string.Empty;
            string respCode = string.Empty;
            string errorText = string.Empty;
            TransactionMapping map = new TransactionMapping(NAVVersion, config.IsJson);
            NavWS.RootHospTransaction root = map.MapFromOrderToRoot(request, salesType);

            logger.Debug(config.LSKey.Key, "CreateHospOrder Request - " + Serialization.ToXml(root, true));

            navQryWS.CreateHospOrder(ref respCode, ref errorText, string.Empty, ref receiptNo, ref root);
            HandleWS2ResponseCode("CreateHospOrder", respCode, errorText);
            logger.Debug(config.LSKey.Key, "CreateHospOrder Response - " + Serialization.ToXml(root, true));
            return receiptNo;
        }

        public int HospOrderEstimatedTime(string storeId, string orderId)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;
            int estTime = 0;

            navQryWS.GetHospOrderEstimatedTime(ref respCode, ref errorText, storeId, orderId, ref estTime);
            HandleWS2ResponseCode("GetHospOrderEstimatedTime", respCode, errorText);
            return estTime;
        }

        public void HospOrderCancel(string storeId, string orderId)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;

            navQryWS.CancelHospOrder(ref respCode, ref errorText, orderId, storeId);
            HandleWS2ResponseCode("CancelHospOrder", respCode, errorText);
        }

        public OrderHospStatus HospOrderKotStatus(string storeId, string orderId)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;

            NavWS.RootKotStatus root = new NavWS.RootKotStatus();
            navQryWS.GetKotStatus(ref respCode, ref errorText, storeId, orderId, ref root);
            HandleWS2ResponseCode("GetKotStatus", respCode, errorText);

            if (root.KotStatus == null || root.KotStatus.Length == 0)
                return new OrderHospStatus();

            return new OrderHospStatus()
            {
                ReceiptNo = root.KotStatus[0].ReceiptNo,
                KotNo = root.KotStatus[0].KotNo,
                Status = root.KotStatus[0].Status,
                Confirmed = root.KotStatus[0].ConfirmedbyExp,
                ProductionTime = root.KotStatus[0].KotProdTime
            };
        }

        #endregion

        #region Order

        public Order BasketCalcToOrder(OneList list)
        {
            if (string.IsNullOrWhiteSpace(list.Id))
                list.Id = GuidHelper.NewGuidString();

            if (navWS == null)
            {
                BasketXml xml = new BasketXml();
                string xmlRequest = xml.BasketCalcRequestXML(list, NAVVersion.Major);
                string xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                return xml.BasketCalcToOrderResponseXML(xmlResponse, NAVVersion.Major);
            }

            OrderMapping map = new OrderMapping(NAVVersion, config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;
            NavWS.RootMobileTransaction root = map.MapFromRetailTransactionToRoot(list);

            logger.Debug(config.LSKey.Key, "EcomCalculateBasket Request - " + Serialization.ToXml(root, true));

            navQryWS.EcomCalculateBasket(ref respCode, ref errorText, ref root);
            HandleWS2ResponseCode("EcomCalculateBasket", respCode, errorText);
            logger.Debug(config.LSKey.Key, "EcomCalculateBasket Response - " + Serialization.ToXml(root, true));
            return map.MapFromRootTransactionToOrder(root);
        }

        public OrderStatusResponse OrderStatusCheck(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new LSOmniException(StatusCode.OrderIdNotFound, "OrderStatusCheck orderId can not be empty");

            if (navWS == null)
                throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Status check WS2 not available in LS Central/NAV");

            string respCode = string.Empty;
            string errorText = string.Empty;
            NavWS.RootCustomerOrderStatus root = new NavWS.RootCustomerOrderStatus();
            navWS.CustomerOrderStatus(ref respCode, ref errorText, orderId, ref root);
            HandleWS2ResponseCode("CustomerOrderStatus", respCode, errorText);
            logger.Debug(config.LSKey.Key, "CustomerOrderStatus Response - " + Serialization.ToXml(root, true));

            return new OrderStatusResponse()
            {
                DocumentNo = root.CustomerOrderStatus[0].DocumentID,
                DocumentType = root.CustomerOrderStatus[0].DocumentType,
                CustomerOrderPaymentStatus = root.CustomerOrderStatus[0].PaymentStatus,
                CustomerOrderShippingStatus = root.CustomerOrderStatus[0].ShippingStatus,
                CustomerOrderStatus = root.CustomerOrderStatus[0].OrderStatus
            };
        }

        public void OrderCancel(string orderId, string storeId, string userId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new LSOmniException(StatusCode.OrderIdNotFound, "OrderStatusCheck orderId can not be empty");

            if (navWS == null)
                throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Status check WS2 not available in LS Central/NAV");

            string respCode = string.Empty;
            string errorText = string.Empty;

            NavWS.RootCustomerOrderCancel root = new NavWS.RootCustomerOrderCancel();
            List<NavWS.CustomerOrderStatusLog> log = new List<NavWS.CustomerOrderStatusLog>()
            {
                new NavWS.CustomerOrderStatusLog()
                {
                    StoreNo = storeId,
                    UserID = userId,
                    StaffID = string.Empty,
                    TerminalNo = string.Empty
                }
            };
            root.CustomerOrderStatusLog = log.ToArray();

            navWS.CustomerOrderCancel(ref respCode, ref errorText, orderId, 0, root);
            HandleWS2ResponseCode("CustomerOrderCancel", respCode, errorText);
        }

        public SalesEntry OrderGet(string id)
        {
            OrderMapping map = new OrderMapping(NAVVersion, config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;
            SalesEntry order;

            if (NAVVersion > new Version("14.2"))
            {
                decimal pointUsed = 0;
                decimal pointEarned = 0;
                NavWS.RootCustomerOrderGetV3 root = new NavWS.RootCustomerOrderGetV3();
                navWS.CustomerOrderGetV3(ref respCode, ref errorText, "LOOKUP", id, "P0001", ref root, ref pointEarned, ref pointUsed);
                HandleWS2ResponseCode("CustomerOrderGet", respCode, errorText);
                logger.Debug(config.LSKey.Key, "CustomerOrderGetV2 Response - " + Serialization.ToXml(root, true));

                order = map.MapFromRootV2ToSalesEntry(root);
                order.PointsRewarded = pointEarned;
                order.PointsUsedInOrder = pointUsed;
            }
            else
            {
                NavWS.RootCustomerOrderGet root = new NavWS.RootCustomerOrderGet();
                navWS.CustomerOrderGet(ref respCode, ref errorText, "LOOKUP", id, "P0001", ref root);
                HandleWS2ResponseCode("CustomerOrderGet", respCode, errorText);
                logger.Debug(config.LSKey.Key, "CustomerOrderGet Response - " + Serialization.ToXml(root, true));

                order = map.MapFromRootToSalesEntry(root);
            }

            if (string.IsNullOrEmpty(order.CardId))
                order.AnonymousOrder = true;
            return order;
        }

        public List<SalesEntry> OrderHistoryGet(string cardId)
        {
            OrderMapping map = new OrderMapping(NAVVersion, config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;

            if (NAVVersion.Major < 16)
            {
                NavWS.RootCustomerOrderFilteredList root = new NavWS.RootCustomerOrderFilteredList();
                List<NavWS.CustomerOrderHeader3> hd = new List<NavWS.CustomerOrderHeader3>();
                hd.Add(new NavWS.CustomerOrderHeader3()
                {
                    MemberCardNo = cardId
                });
                root.CustomerOrderHeader = hd.ToArray();

                navWS.CustomerOrderFilteredList(ref respCode, ref errorText, true, ref root);
                HandleWS2ResponseCode("CustomerOrderFilteredList", respCode, errorText);
                logger.Debug(config.LSKey.Key, "CustomerOrderFilteredList Response - " + Serialization.ToXml(root, true));
                return map.MapFromRootToSalesEntryHistory(root);
            }
            else
            {
                NavWS.RootCOFilteredListV2 root = new NavWS.RootCOFilteredListV2();
                List<NavWS.CustomerOrderHeaderV2> hd = new List<NavWS.CustomerOrderHeaderV2>();
                hd.Add(new NavWS.CustomerOrderHeaderV2()
                {
                    MemberCardNo = cardId
                });
                root.CustomerOrderHeaderV2 = hd.ToArray();

                navWS.COFilteredListV2(ref respCode, ref errorText, true, ref root);
                HandleWS2ResponseCode("CustomerOrderFilteredList", respCode, errorText);
                logger.Debug(config.LSKey.Key, "CustomerOrderFilteredList Response - " + Serialization.ToXml(root, true));
                return map.MapFromRootToSalesEntryHistory(root);
            }
        }

        public OrderAvailabilityResponse OrderAvailabilityCheck(OneList request)
        {
            // Click N Collect
            if (string.IsNullOrWhiteSpace(request.Id))
                request.Id = GuidHelper.NewGuidString();

            if (navWS == null || NAVVersion < new Version("13.5"))
            {
                ClickCollectXml xml = new ClickCollectXml();
                string xmlRequest = xml.CheckAvailabilityRequestXML(request);
                string xmlResponse = RunOperation(xmlRequest);

                //need to ignore some nav response codes
                string navResponseCode = GetResponseCode(ref xmlResponse);
                if (navResponseCode == "0004")
                {
                    //<Response_Text>Unknown Request_ID CO_QTY_AVAILABILITY</Response_Text>
                    //0004 = Unknown Request_ID,  NavResponseCode.Error = 0004
                    logger.Info(config.LSKey.Key, "responseCode {0} - this is only supported by default for LS Nav 11.0 and later", navResponseCode);
                    return new OrderAvailabilityResponse();
                }

                if (navResponseCode != "0000")
                {
                    logger.Info(config.LSKey.Key, "responseCode {0} - Inventory Not available for store {1}", navResponseCode, request.StoreId);
                    OrderAvailabilityResponse resp = new OrderAvailabilityResponse();
                    foreach (OneListItem item in request.Items)
                    {
                        resp.Lines.Add(new OrderLineAvailabilityResponse()
                        {
                            ItemId = item.ItemId,
                            VariantId = item.VariantId,
                            UnitOfMeasureId = item.UnitOfMeasureId,
                            Quantity = 0
                        });
                    }
                    return resp;
                }

                HandleResponseCode(ref xmlResponse);
                return xml.CheckAvailabilityExtResponseXML(xmlResponse);
            }

            OrderMapping map = new OrderMapping(NAVVersion, config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;

            string pefSourceLoc = string.Empty;
            NavWS.RootCOQtyAvailabilityExtOut rootout = new NavWS.RootCOQtyAvailabilityExtOut();

            OrderAvailabilityResponse data;
            if (NAVVersion < new Version("16.3"))
            {
                NavWS.RootCOQtyAvailabilityExtIn rootin = map.MapOneListToInvReq(request);
                logger.Debug(config.LSKey.Key, "COQtyAvailability Request - " + Serialization.ToXml(rootin, true));

                navWS.COQtyAvailability(rootin, ref respCode, ref errorText, ref pefSourceLoc, ref rootout);

                HandleWS2ResponseCode("COQtyAvailability", respCode, errorText);
                logger.Debug(config.LSKey.Key, "COQtyAvailability Response - " + Serialization.ToXml(rootout, true));

                data = map.MapRootToOrderavailabilty(rootin, rootout);
            }
            else
            {
                NavWS.RootCOQtyAvailabilityInV2 rootin = map.MapOneListToInvReq2(request);
                logger.Debug(config.LSKey.Key, "COQtyAvailability Request - " + Serialization.ToXml(rootin, true));

                navWS.COQtyAvailabilityV2(rootin, ref respCode, ref errorText, ref pefSourceLoc, ref rootout);

                HandleWS2ResponseCode("COQtyAvailability", respCode, errorText);
                logger.Debug(config.LSKey.Key, "COQtyAvailability Response - " + Serialization.ToXml(rootout, true));

                data = map.MapRootToOrderavailabilty2(rootin, rootout);
            }
            data.PreferredSourcingLocation = pefSourceLoc;
            return data;
        }

        public string OrderCreate(Order request, string tenderMapping, out string orderId)
        {
            if (request == null)
                throw new LSOmniException(StatusCode.OrderIdNotFound, "OrderCreate request can not be null");

            orderId = string.Empty;
            if (NAVVersion > new Version("13.5"))
            {
                if (string.IsNullOrWhiteSpace(request.Id))
                    request.Id = GuidHelper.NewGuidString();
            }
            else
            {
                if (Common.Util.Validation.IsValidGuid(request.Id) == false)
                    request.Id = GuidHelper.NewGuidString();
            }

            // need to map the TenderType enum coming from devices to TenderTypeId that NAV knows
            if (request.OrderPayments == null)
                request.OrderPayments = new List<OrderPayment>();

            int lineno = 1;
            foreach (OrderPayment line in request.OrderPayments)
            {
                line.TenderType = TenderTypeMapping(tenderMapping, line.TenderType, false); //map tender type between LSOmni and Nav
                if (line.TenderType == null)
                    throw new LSOmniServiceException(StatusCode.TenderTypeNotFound, "TenderType_Mapping failed for type: " + line.TenderType);
                line.LineNumber = lineno++;
            }

            if (request.ShipToAddress == null)
            {
                if (request.OrderType == OrderType.ClickAndCollect)
                {
                    request.ShipToAddress = new Address();
                }
                else
                {
                    throw new LSOmniException(StatusCode.AddressIsEmpty, "ShipToAddress can not be null if ClickAndCollectOrder is false");
                }
            }

            if (request.ContactAddress == null)
            {
                if (request.OrderType == OrderType.ClickAndCollect)
                {
                    request.ContactAddress = new Address();
                }
                else
                {
                    request.ContactAddress = request.ShipToAddress;
                }
            }

            if (navWS == null)
            {
                string xmlRequest;
                string xmlResponse;
                if (NAVVersion.Major < 10)
                {
                    switch (request.OrderType)
                    {
                        case OrderType.ClickAndCollect:
                            ClickCollectXml xmlcac = new ClickCollectXml();
                            xmlRequest = xmlcac.OrderCreateCACRequestXML(request);
                            break;
                        default:
                            BasketXml xmlbask = new BasketXml();
                            xmlRequest = xmlbask.BasketPostSaleRequestXML(request);
                            break;
                    }
                    xmlResponse = RunOperation(xmlRequest);
                    HandleResponseCode(ref xmlResponse);
                }
                else
                {
                    if (request.ShipToAddress == null)
                    {
                        if (request.OrderType == OrderType.ClickAndCollect)
                        {
                            request.ShipToAddress = new Address();
                        }
                        else
                        {
                            throw new LSOmniException(StatusCode.AddressIsEmpty, "ShipToAddress can not be null if ClickAndCollectOrder is false");
                        }
                    }

                    ClickCollectXml xml = new ClickCollectXml();
                    xmlRequest = xml.OrderCreateRequestXML(request);
                    xmlResponse = RunOperation(xmlRequest);
                    HandleResponseCode(ref xmlResponse);
                }
                return request.Id;
            }

            OrderMapping map = new OrderMapping(NAVVersion, config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;

            if (NAVVersion > new Version("16.2.0.0"))
            {
                NavWS.RootCustomerOrderCreateV5 root = map.MapFromOrderV5ToRoot(request);
                logger.Debug(config.LSKey.Key, "CustomerOrderCreateV5 Request - " + Serialization.ToXml(root, true));
                navWS.CustomerOrderCreateV5(ref respCode, ref errorText, root, ref orderId);
            }
            else if (NAVVersion > new Version("14.2"))
            {
                NavWS.RootCustomerOrderCreateV4 root = map.MapFromOrderV4ToRoot(request);
                logger.Debug(config.LSKey.Key, "CustomerOrderCreateV4 Request - " + Serialization.ToXml(root, true));
                navWS.CustomerOrderCreateV4(ref respCode, ref errorText, root, ref orderId);
            }
            else if (NAVVersion > new Version("13.5"))
            {
                NavWS.RootCustomerOrderCreateV2 root = map.MapFromOrderV2ToRoot(request);
                logger.Debug(config.LSKey.Key, "CustomerOrderCreateV2 Request - " + Serialization.ToXml(root, true));
                navWS.CustomerOrderCreateV2(ref respCode, ref errorText, root, ref orderId);
            }
            else
            {
                NavWS.RootCustomerOrder root = map.MapFromOrderToRoot(request);
                logger.Debug(config.LSKey.Key, "CustomerOrderCreate Request - " + Serialization.ToXml(root, true));
                navWS.CustomerOrderCreate(ref respCode, ref errorText, root);
            }
            HandleWS2ResponseCode("CustomerOrderCreate", respCode, errorText);
            return request.Id;
        }

        #endregion

        #region SalesEntry

        public List<SalesEntry> SalesHistory(string cardId, int numberOfTrans)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;

            NavWS.RootGetMemberSalesHistory rootHistory = new NavWS.RootGetMemberSalesHistory();
            logger.Debug(config.LSKey.Key, "GetMemberSalesHistory - CardId: {0}, MaxNoOfHeaders: {1}", cardId, numberOfTrans);
            navWS.GetMemberSalesHistory(ref respCode, ref errorText, string.Empty, string.Empty, cardId, numberOfTrans, ref rootHistory);
            HandleWS2ResponseCode("GetMemberSalesHistory", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetMemberSalesHistory Response - " + Serialization.ToXml(rootHistory, true));

            TransactionMapping map = new TransactionMapping(NAVVersion, config.IsJson);
            return map.MapFromRootToSalesEntries(rootHistory);
        }

        public SalesEntry TransactionGet(string receiptNo, string storeId, string terminalId, int transId)
        {
            TransactionMapping map = new TransactionMapping(NAVVersion, config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;
            NavWS.RootGetTransaction root = new NavWS.RootGetTransaction();
            navWS.GetTransaction(ref respCode, ref errorText, receiptNo, storeId, terminalId, transId, ref root);
            HandleWS2ResponseCode("GetTransaction", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetTransaction Response - " + Serialization.ToXml(root, true));

            return map.MapFromRootToRetailTransaction(root);
        }

        #endregion

        #region Store

        public Store StoreGetById(string id)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Store", "No.", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository();
            List<Store> st = rep.StoresGet(table);
            return (st.Count > 0) ? st[0] : null;
        }

        public List<Store> StoresGet(bool clickAndCollectOnly)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest;
            if (clickAndCollectOnly)
                xmlRequest = xml.GetGeneralWebRequestXML("Store", "Click and Collect", "1");
            else
                xmlRequest = xml.GetGeneralWebRequestXML("Store");

            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository();
            return rep.StoresGet(table);
        }

        public List<StoreHours> StoreHoursGetByStoreId(string storeId, int offset)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;
            NavWS.RootGetStoreOpeningHours root = new NavWS.RootGetStoreOpeningHours();

            logger.Debug(config.LSKey.Key, "GetStoreOpeningHours Store: " + storeId);
            navQryWS.GetStoreOpeningHours(ref respCode, ref errorText, storeId, ref root);
            HandleWS2ResponseCode("GetStoreOpeningHours", respCode, errorText, new string[] { "1000" } );
            logger.Debug(config.LSKey.Key, "GetStoreOpeningHours Response - " + Serialization.ToXml(root, true));

            StoreMapping map = new StoreMapping();
            return map.MapFromRootToOpeningHours(root, offset);
        }

        public List<StoreServices> StoreServicesGetByStoreId(string storeId)
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

        #endregion

        #region Device

        public Device DeviceGetById(string id)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Member Device", "ID", id);
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ContactRepository rep = new ContactRepository();
            return rep.DeviceGetById(table);
        }

        private string GetDefaultDeviceId(string userName)
        {
            return ("WEB-" + userName);
        }

        public Terminal TerminalGetById(string id)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("POS Terminal", "No.", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository();
            Terminal ter = rep.GetTerminalData(table);

            if (ter.InventoryMainMenuId == null)
                ter.InventoryMainMenuId = string.Empty;

            return ter;
        }

        #endregion

        public Currency CurrencyGetById(string id, string culture)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Currency", "Code", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository();
            return rep.GetCurrency(table, culture);
        }

        public List<ShippingAgentService> GetShippingAgentService(string agentId)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Shipping Agent Services", "Shipping Agent Code", agentId);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository();
            return rep.GetShippingAgentServices(table);
        }

        public decimal GetPointRate()
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Currency Exchange Rate", "Currency Code", "LOY");
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository();
            return rep.GetPointRate(table);
        }

        public List<Notification> NotificationsGetByCardId(string cardId)
        {
            List<Notification> pol = new List<Notification>();
            if (string.IsNullOrWhiteSpace(cardId))
                return pol;

            if (navWS == null)
            {
                PublishedOfferXml xml = new PublishedOfferXml();
                string xmlRequest = xml.PublishedOfferMemberRequestXML(cardId, string.Empty, string.Empty);
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                return xml.NotificationMemberResponseXML(xmlResponse);
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            NavWS.RootGetDirectMarketingInfo root = new NavWS.RootGetDirectMarketingInfo();
            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo - CardId: {0}", cardId);
            navWS.GetDirectMarketingInfo(ref respCode, ref errorText, cardId, string.Empty, string.Empty, ref root);
            HandleWS2ResponseCode("GetDirectMarketingInfo", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo Response - " + Serialization.ToXml(root, true));

            ContactMapping map = new ContactMapping(config.IsJson);
            return map.MapFromRootToNotifications(root);
        }

        public List<Hierarchy> HierarchyGet(string storeId)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error(config.LSKey.Key, "Only supported in NAV 10.x and later");
                return new List<Hierarchy>();
            }

            List<Hierarchy> list = new List<Hierarchy>();
            List<HierarchyNode> nodes = new List<HierarchyNode>();
            if (navWS == null)
            {
                HierarchyXml xml = new HierarchyXml();
                string xmlRequest = xml.HierarchyRequestXML(storeId);
                string xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                list = xml.HierarchyResponseXML(xmlResponse, out nodes);

                // Load node links for all nodes
                foreach (HierarchyNode node in nodes)
                {
                    xmlRequest = xml.HierarchyNodeRequestXML(node.HierarchyCode, node.Id);
                    xmlResponse = RunOperation(xmlRequest, true);
                    HandleResponseCode(ref xmlResponse);
                    node.Leafs = xml.HierarchyNodeResponseXML(xmlResponse);
                }
            }
            else
            {
                string respCode = string.Empty;
                string errorText = string.Empty;
                NavWS.RootGetHierarchy rootRoot = new NavWS.RootGetHierarchy();
                logger.Debug(config.LSKey.Key, "GetHierarchy - StoreId: {0}", storeId);
                navQryWS.GetHierarchy(ref respCode, ref errorText, storeId, ref rootRoot);
                HandleWS2ResponseCode("GetHierarchy", respCode, errorText);
                logger.Debug(config.LSKey.Key, "GetHierarchy Response - " + Serialization.ToXml(rootRoot, true));

                foreach (NavWS.Hierarchy top in rootRoot.Hierarchy)
                {
                    list.Add(new Hierarchy()
                    {
                        Id = top.HierarchyCode,
                        Description = top.Description,
                        Type = (HierarchyType)Convert.ToInt32(top.Type)
                    });
                }

                foreach (NavWS.HierarchyNodes val in rootRoot.HierarchyNodes)
                {
                    HierarchyNode node = new HierarchyNode()
                    {
                        Id = val.NodeID,
                        ParentNode = val.ParentNodeID,
                        PresentationOrder = val.PresentationOrder,
                        Indentation = val.Indentation,
                        Description = val.Description,
                        HierarchyCode = val.HierarchyCode,
                        ImageId = string.Join("", val.RetailImageCode)
                    };
                    nodes.Add(node);

                    NavWS.RootGetHierarchyNodeIn rootNodeIn = new NavWS.RootGetHierarchyNodeIn();
                    NavWS.RootGetHierarchyNodeOut rootNodeOut = new NavWS.RootGetHierarchyNodeOut();

                    logger.Debug(config.LSKey.Key, "GetHierarchyNode - HierarchyCode: {0}, NodeId: {1}, StoreId: {2}, NodeIn: {3}",
                        val.HierarchyCode, val.NodeID, storeId, Serialization.ToXml(rootNodeIn, true));
                    navQryWS.GetHierarchyNode(ref respCode, ref errorText, val.HierarchyCode, val.NodeID, storeId, rootNodeIn, ref rootNodeOut);
                    HandleWS2ResponseCode("GetHierarchyNode", respCode, errorText);
                    logger.Debug(config.LSKey.Key, "GetHierarchyNode Response - " + Serialization.ToXml(rootNodeOut, true));

                    if (rootNodeOut.HierarchyNodeLink == null)
                        continue;

                    foreach (NavWS.HierarchyNodeLink lnk in rootNodeOut.HierarchyNodeLink)
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

        public List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId)
        {
            if (navWS == null)
            {
                //the name of the Nav requestID changed between 9.00.04 and 9.00.05 
                //TODO need to get the Notifications for this WS call too
                PublishedOfferXml xml = new PublishedOfferXml();
                string xmlRequest = xml.PublishedOfferMemberRequestXML(cardId, itemId, storeId);
                string xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                return xml.PublishedOfferMemberResponseXML(xmlResponse);
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            ContactMapping map = new ContactMapping(config.IsJson);
            NavWS.RootGetDirectMarketingInfo root = new NavWS.RootGetDirectMarketingInfo();

            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo - CardId: {0}, ItemId: {1}", cardId, itemId);
            navWS.GetDirectMarketingInfo(ref respCode, ref errorText, cardId, itemId, storeId, ref root);
            HandleWS2ResponseCode("GetDirectMarketingInfo", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo Response - " + Serialization.ToXml(root, true));
            return map.MapFromRootToPublishedOffers(root);
        }

        public List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;

            StoreMapping map = new StoreMapping();
            NavWS.RootGetReturnPolicy root = new NavWS.RootGetReturnPolicy();

            logger.Debug(config.LSKey.Key, "GetReturnPolicy - storeId:{0}, storeGroup:{1} itemCat:{2} prodGroup:{3}", storeId, storeGroupCode, itemCategory, productGroup);
            navWS.GetReturnPolicy(ref respCode, ref errorText, storeId, storeGroupCode, itemCategory, productGroup, itemId, variantCode, variantDim1, ref root);
            string ret = HandleWS2ResponseCode("GetReturnPolicy", respCode, errorText, new string[] { "1000" });
            if (ret == "1000")
                return new List<ReturnPolicy>();

            logger.Debug(config.LSKey.Key, "GetReturnPolicy Response - " + Serialization.ToXml(root, true));
            return map.MapFromRootToReturnPolicy(root);
        }

        public List<Advertisement> AdvertisementsGetById(string id)
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

        public MobileMenu MenuGet(string storeId, string salesType, Currency currency)
        {
            MenuXml menuxml = new MenuXml(config.LSKey.Key);
            string xmlRequest = menuxml.MenuGetAllRequestXML(storeId, salesType);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            return menuxml.MenuGetAllResponseXML(xmlResponse, currency);
        }
    }
}
