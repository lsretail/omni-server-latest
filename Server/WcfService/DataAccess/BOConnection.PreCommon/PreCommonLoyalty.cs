using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping;
using LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping.Loyalty;
using LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping.Replication;
using LSOmni.DataAccess.BOConnection.PreCommon.Mapping;

using LSRetail.Omni.DiscountEngine.DataModels;
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

namespace LSOmni.DataAccess.BOConnection.PreCommon
{
    public partial class PreCommonBase
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

            xmlRequest = xml.GetGeneralWebRequestXML("LSC Item Variant Registration", "Item No.", item.Id);
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            table = xml.GetGeneralWebResponseXML(xmlResponse);
            item.VariantsRegistration = rep.GetVariantRegistrations(table);

            xmlRequest = xml.GetGeneralWebRequestXML("LSC Extd. Variant Values", "Item No.", item.Id);
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            table = xml.GetGeneralWebResponseXML(xmlResponse);
            item.VariantsExt = rep.GetVariantExt(table);

            foreach (VariantExt ext in item.VariantsExt)
            {
                xmlRequest = xml.GetGeneralWebRequestXML("LSC Extd. Variant Values", "Item No.", ext.ItemId, "Code", ext.Code);
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
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Barcodes", "Barcode No.", barcode);
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

        public string ItemFindByBarcode(string barcode, string storeId, string terminalId)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootLeftRightLine root = new LSCentral.RootLeftRightLine();

            logger.Debug(config.LSKey.Key, "GetItemCard - StoreId:{0}, TermId:{1}, barcode:{2}", storeId, terminalId, barcode);
            centralWS.GetItemCard(ref respCode, ref errorText, terminalId, storeId, string.Empty, string.Empty, string.Empty, barcode, string.Empty, ref root);
            HandleWS2ResponseCode("GetItemCard", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetItemCard Response - " + Serialization.ToXml(root, true));

            if (root.LeftRightLine == null)
                return null;

            foreach (LSCentral.LeftRightLine line in root.LeftRightLine)
            {
                if (line.LeftLine == "Item")
                    return line.RightLine;
            }
            return null;
        }

        public List<InventoryResponse> ItemInStockGet(string itemId, string variantId, int arrivingInStockInDays, List<string> locationIds, bool skipUnAvailableStores)
        {
            //locationIds are storeIds in NAV
            string respCode = string.Empty;
            string errorText = string.Empty;
            List<InventoryResponse> list = new List<InventoryResponse>();
            LSCentral.RootGetItemInventory root = new LSCentral.RootGetItemInventory();
            foreach (string id in locationIds)
            {
                centralWS.GetItemInventory(ref respCode, ref errorText, itemId, variantId, id, string.Empty, string.Empty, string.Empty, string.Empty, arrivingInStockInDays, ref root);
                HandleWS2ResponseCode("GetItemInventory", respCode, errorText);
                logger.Debug(config.LSKey.Key, "GetItemInventory Response - " + Serialization.ToXml(root, true));

                if (root.WSInventoryBuffer == null)
                    continue;

                foreach (LSCentral.WSInventoryBuffer1 buffer in root.WSInventoryBuffer)
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

        public virtual List<InventoryResponse> ItemsInStoreGet(List<InventoryRequest> items, string storeId, string locationId)
        {
            List<InventoryResponse> list = new List<InventoryResponse>();
            string respCode = string.Empty;
            string errorText = string.Empty;

            List<LSCentral.InventoryBufferIn> lines = new List<LSCentral.InventoryBufferIn>();
            foreach (InventoryRequest item in items)
            {
                LSCentral.InventoryBufferIn buf = lines.Find(b => b.Number.Equals(item.ItemId) && b.Variant.Equals(item.VariantId ?? string.Empty));
                if (buf == null)
                {
                    lines.Add(new LSCentral.InventoryBufferIn()
                    {
                        Number = item.ItemId,
                        Variant = item.VariantId ?? string.Empty
                    });
                }
            }

            LSCentral.RootGetInventoryMultipleIn rootin = new LSCentral.RootGetInventoryMultipleIn()
            {
                InventoryBufferIn = lines.ToArray()
            };

            LSCentral.RootGetInventoryMultipleOut rootout = new LSCentral.RootGetInventoryMultipleOut();

            centralWS.GetInventoryMultiple(ref respCode, ref errorText, storeId, locationId, rootin, ref rootout);
            HandleWS2ResponseCode("GetInventoryMultiple", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetInventoryMultiple Response - " + Serialization.ToXml(rootout, true));

            if (rootout.InventoryBufferOut == null)
                return list;

            foreach (LSCentral.InventoryBufferOut buffer in rootout.InventoryBufferOut)
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

        public virtual List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId)
        {
            // filter
            List<XMLFieldData> filter = new List<XMLFieldData>();
            if (string.IsNullOrEmpty(storeId) == false)
            {
                filter.Add(new XMLFieldData()
                {
                    FieldName = "Store No.",
                    Values = new List<string>() { storeId }
                });
            }

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetBatchWebRequestXML("LSC Current Availability", null, filter, string.Empty, 0);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository();
            List<HospAvailabilityResponse> list = rep.CurrentAvail(table);
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
            string xmlRequest = xml.GetGeneralWebRequestXML("Item", "LSC Retail Product Code", prodGroup);
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
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Retail Image", "Code", imageId);
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
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Retail Image Link", "KeyValue", keyvalue, "TableName", tableName);
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

            OrderMapping map = new OrderMapping(config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootMobileTransaction root = map.MapFromCustItemToRoot(storeId, cardId, items);

            logger.Debug(config.LSKey.Key, "EcomGetCustomerPrice Request - " + Serialization.ToXml(root, true));

            centralQryWS.EcomGetCustomerPrice(ref respCode, ref errorText, ref root);
            HandleWS2ResponseCode("EcomGetCustomerPrice", respCode, errorText);
            logger.Debug(config.LSKey.Key, "EcomGetCustomerPrice Response - " + Serialization.ToXml(root, true));
            return map.MapFromRootTransactionToItemCustPrice(root);
        }

        public List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string loyaltySchemeCode)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest;
            string xmlResponse;
            List<ProactiveDiscount> list = new List<ProactiveDiscount>();
            SetupRepository rep = new SetupRepository();

            foreach (string id in itemIds)
            {
                xmlRequest = xml.GetGeneralWebRequestXML("LSC WI Discounts", "Store No.", storeId, "Item No.", id);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

                list.AddRange(rep.GetDiscount(table));

                xmlRequest = xml.GetGeneralWebRequestXML("LSC WI Mix & Match Offer", "Store No.", storeId, "Item No.", id);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);

                list.AddRange(rep.GetDiscount(table));
            }

            foreach (ProactiveDiscount disc in list)
            {
                xmlRequest = xml.GetGeneralWebRequestXML("LSC Periodic Discount", "No.", disc.Id);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

                rep.SetDiscountInfo(table, disc);
            }
            return list;
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

            string respCode = string.Empty;
            string errorText = string.Empty;
            ContactMapping map = new ContactMapping(config.IsJson);

            string clubId = string.Empty;
            string cardId = string.Empty;
            string contId = string.Empty;
            string acctId = string.Empty;
            string schmId = string.Empty;
            decimal point = 0;

            LSCentral.RootMemberContactCreate root = map.MapToRoot(contact);
            logger.Debug(config.LSKey.Key, "MemberContactCreate Request - " + Serialization.ToXml(root, true));

            centralWS.MemberContactCreate(ref respCode, ref errorText, ref clubId, ref schmId, ref acctId, ref contId, ref cardId, ref point, ref root);
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

            string respCode = string.Empty;
            string errorText = string.Empty;
            ContactMapping map = new ContactMapping(config.IsJson);

            LSCentral.RootMemberContactCreate1 root = map.MapToRoot1(contact, accountId);
            logger.Debug(config.LSKey.Key, "MemberContactUpdate Request - " + Serialization.ToXml(root, true));
            centralWS.MemberContactUpdate(ref respCode, ref errorText, ref root);
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

            LSCentral.RootGetMemberContact rootContact = new LSCentral.RootGetMemberContact();
            logger.Debug(config.LSKey.Key, "GetMemberContact - CardId: {0}", card);
            centralWS.GetMemberContact2(ref respCode, ref errorText, card, accountId, contactId, loginId.ToLower(), email.ToLower(), ref rootContact);
            if (respCode == "1000") // not found
                return null;

            HandleWS2ResponseCode("GetMemberContact", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetMemberContact Response - " + Serialization.ToXml(rootContact, true));

            List<Scheme> schemelist = SchemeGetAll();
            contact = map.MapFromRootToContact(rootContact, schemelist);
            contact.UserName = loginId;

            if (contact.Cards.Count == 0)
                return contact;

            card = contact.Cards.FirstOrDefault().Id;

            if (string.IsNullOrEmpty(loginId))
            {
                xmlRequest = xml.GetGeneralWebRequestXML("LSC Member Login Card", "Card No.", card, 1);
                xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);

                if (table == null || table.NumberOfValues == 0)
                    return null;

                field = table.FieldList.Find(f => f.FieldName.Equals("Login ID"));
                contact.UserName = field.Values[0];
            }

            decimal remainingPoints = 0;
            contact.Profiles = ProfileGet(card, ref remainingPoints);
            contact.Account.PointBalance = (remainingPoints == 0) ? contact.Account.PointBalance : Convert.ToInt64(Math.Floor(remainingPoints));

            if (includeDetails == false)
                return contact;

            xmlRequest = xml.GetGeneralWebRequestXML("LSC Member Login", "Login ID", contact.UserName.ToLower(), 1);
            xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            table = xml.GetGeneralWebResponseXML(xmlResponse);

            if (table == null || table.NumberOfValues == 0)
                return null;

            field = table.FieldList.Find(f => f.FieldName.Equals("Password"));
            contact.Password = field.Values[0];

            LSCentral.RootGetDirectMarketingInfo rootMarket = new LSCentral.RootGetDirectMarketingInfo();
            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo - CardId: {0}", card);
            centralWS.GetDirectMarketingInfo(ref respCode, ref errorText, card, string.Empty, string.Empty, ref rootMarket);
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
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Member Contact", "E-Mail", email, 1);
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
            string respCode = string.Empty;
            string errorText = string.Empty;
            decimal points = 0;

            centralWS.MemberCardToContact(ref respCode, ref errorText, cardId, accountId, contactId, ref points);
            HandleWS2ResponseCode("GetDirectMarketingInfo", respCode, errorText);
            logger.Debug(config.LSKey.Key, "MemberCardToContact Response - points:", points);
            return (double)points;
        }

        #endregion

        #region Contact Related

        public List<Profile> ProfileGetAll()
        {
            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootMobileGetProfiles root = new LSCentral.RootMobileGetProfiles();

            centralWS.MobileGetProfiles(ref respCode, ref errorText, string.Empty, string.Empty, ref root);
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
            LSCentral.RootGetMemberCard rootCard = new LSCentral.RootGetMemberCard();

            logger.Debug(config.LSKey.Key, "GetMemberCard - CardId: {0}", cardId);
            centralWS.GetMemberCard(ref respCode, ref errorText, cardId, ref remainingPoints, ref rootCard);
            HandleWS2ResponseCode("GetMemberCard", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetMemberCard Response - " + Serialization.ToXml(rootCard, true));

            foreach (LSCentral.MemberAttributeList att in rootCard.MemberAttributeList)
            {
                if (att.AttributeType != "4")
                    continue;

                list.Add(new Profile()
                {
                    Id = att.Code,
                    Description = att.Description,
                    DefaultValue = string.Empty,
                    ContactValue = att.Value.ToLower().Trim() == "yes"
                });
            }
            return list;
        }

        public List<Scheme> SchemeGetAll()
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Member Scheme");
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ContactRepository rep = new ContactRepository();
            List<Scheme> list = rep.SchemeGetAll(table);

            foreach (Scheme sc in list)
            {
                xmlRequest = xml.GetGeneralWebRequestXML("LSC Member Club", "Code", sc.Club.Id);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table == null || table.NumberOfValues == 0)
                    return null;

                XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Description"));
                sc.Club.Name = field.Values[0];
            }
            return list;
        }

        public MemberContact Logon(string userName, string password, string deviceID, string deviceName, bool includeDetails)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootMemberLogon root = new LSCentral.RootMemberLogon();
            decimal remainingPoints = 0;

            logger.Debug(config.LSKey.Key, "MemberLogon - userName: {0}", userName);
            centralWS.MemberLogon(ref respCode, ref errorText, userName, password, deviceID, deviceName, ref remainingPoints, ref root);
            HandleWS2ResponseCode("MemberLogon", respCode, errorText);
            logger.Debug(config.LSKey.Key, "MemberLogon Response - " + Serialization.ToXml(root, true));

            ContactMapping map = new ContactMapping(config.IsJson);
            MemberContact contact = map.MapFromRootToLogonContact(root, remainingPoints);
            contact.UserName = userName;
            if (includeDetails)
                contact.PublishedOffers = PublishedOffersGet(contact.Cards.FirstOrDefault().Id, string.Empty, string.Empty);

            return contact;
        }

        //Change the password in NAV
        public void ChangePassword(string userName, string token, string newPassword, string oldPassword)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;

            logger.Debug(config.LSKey.Key, "MemberPasswordChange - UserName:{0} Token:{1}", userName, token);
            centralWS.MemberPasswordChange(ref respCode, ref errorText, userName, token, oldPassword, newPassword);
            HandleWS2ResponseCode("MemberPasswordChange", respCode, errorText);
        }

        public string ResetPassword(string userName, string email, string newPassword)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;
            string token = string.Empty;
            DateTime tokenExp = DateTime.Now.AddMonths(1);

            logger.Debug(config.LSKey.Key, "MemberPasswordReset - UserName:{0} Email:{1}", userName, email);
            centralWS.MemberPasswordReset(ref respCode, ref errorText, userName, email, ref token, ref tokenExp);
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

            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootGetMemberCard root = new LSCentral.RootGetMemberCard();
            decimal remainingPoints = 0;

            logger.Debug(config.LSKey.Key, "GetMemberCard - CardId: {0}", cardId);
            centralWS.GetMemberCard(ref respCode, ref errorText, cardId, ref remainingPoints, ref root);
            HandleWS2ResponseCode("GetMemberCard", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetMemberCard Response - Remaining points: {0}", Convert.ToInt64(Math.Floor(remainingPoints)));
            return Convert.ToInt64(Math.Floor(remainingPoints));
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo, string entryType)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootGetDataEntryBalance root = new LSCentral.RootGetDataEntryBalance();
            logger.Debug(config.LSKey.Key, "GetDataEntryBalance - GiftCardNo: {0}", cardNo);
            centralWS.GetDataEntryBalance(ref respCode, ref errorText, entryType, cardNo, ref root);
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
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Member Contact", fldname, search, maxNumberOfRowsReturned);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ContactRepository rep = new ContactRepository();
            return rep.ContactGet(table);
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
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Member Attribute", "Description", search);
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
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Store", "Name", search);
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

            string respCode = string.Empty;
            string errorText = string.Empty;
            TransactionMapping map = new TransactionMapping(config.IsJson);
            LSCentral.RootMobileTransaction root = map.MapFromOrderToRoot(list);

            logger.Debug(config.LSKey.Key, "MobilePosCalculate Request - " + Serialization.ToXml(root, true));
            centralQryWS.MobilePosCalculate(ref respCode, ref errorText, string.Empty, ref root);
            HandleWS2ResponseCode("MobilePosCalculate", respCode, errorText);
            logger.Debug(config.LSKey.Key, "MobilePosCalculate Response - " + Serialization.ToXml(root, true));
            return map.MapFromRootToOrderHosp(root);
        }

        public string HospOrderCreate(OrderHosp request)
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
                line.TenderType = ConfigSetting.TenderTypeMapping(config.SettingsGetByKey(ConfigKey.TenderType_Mapping), line.TenderType, false); //map tender type between LSOmni and Nav
                line.LineNumber = lineno++;
            }

            string receiptNo = string.Empty;
            string respCode = string.Empty;
            string errorText = string.Empty;
            TransactionMapping map = new TransactionMapping(config.IsJson);
            LSCentral.RootHospTransaction root = map.MapFromOrderToRoot(request);

            logger.Debug(config.LSKey.Key, "CreateHospOrder Request - " + Serialization.ToXml(root, true));

            centralQryWS.CreateHospOrder(ref respCode, ref errorText, string.Empty, ref receiptNo, ref root);
            HandleWS2ResponseCode("CreateHospOrder", respCode, errorText);
            logger.Debug(config.LSKey.Key, "CreateHospOrder Response - " + Serialization.ToXml(root, true));
            return receiptNo;
        }

        public void HospOrderCancel(string storeId, string orderId)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;

            centralQryWS.CancelHospOrder(ref respCode, ref errorText, orderId, storeId);
            HandleWS2ResponseCode("CancelHospOrder", respCode, errorText);
        }

        public OrderHospStatus HospOrderKotStatus(string storeId, string orderId)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;
            int estTime = 0;

            centralQryWS.GetHospOrderEstimatedTime(ref respCode, ref errorText, storeId, orderId, ref estTime);
            HandleWS2ResponseCode("GetHospOrderEstimatedTime", respCode, errorText);

            LSCentral.RootKotStatus root = new LSCentral.RootKotStatus();
            centralQryWS.GetKotStatus(ref respCode, ref errorText, storeId, orderId, ref root);
            HandleWS2ResponseCode("GetKotStatus", respCode, errorText);
            if (root.KotStatus == null || root.KotStatus.Length == 0)
                return new OrderHospStatus();

            return new OrderHospStatus()
            {
                ReceiptNo = root.KotStatus[0].ReceiptNo,
                KotNo = root.KotStatus[0].KotNo,
                Status = (KOTStatus)Convert.ToInt32(root.KotStatus[0].Status),
                Confirmed = root.KotStatus[0].ConfirmedbyExp,
                ProductionTime = root.KotStatus[0].KotProdTime,
                EstimatedTime = estTime
            };
        }

        #endregion

        #region Order

        public Order BasketCalcToOrder(OneList list)
        {
            if (string.IsNullOrWhiteSpace(list.Id))
                list.Id = GuidHelper.NewGuidString();

            OrderMapping map = new OrderMapping(config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootMobileTransaction root = map.MapFromRetailTransactionToRoot(list);

            logger.Debug(config.LSKey.Key, "EcomCalculateBasket Request - " + Serialization.ToXml(root, true));

            centralQryWS.EcomCalculateBasket(ref respCode, ref errorText, ref root);
            HandleWS2ResponseCode("EcomCalculateBasket", respCode, errorText);
            logger.Debug(config.LSKey.Key, "EcomCalculateBasket Response - " + Serialization.ToXml(root, true));
            return map.MapFromRootTransactionToOrder(root);
        }

        public OrderStatusResponse OrderStatusCheck(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new LSOmniException(StatusCode.OrderIdNotFound, "OrderStatusCheck orderId can not be empty");

            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootCustomerOrderStatus root = new LSCentral.RootCustomerOrderStatus();
            centralWS.CustomerOrderStatus(ref respCode, ref errorText, orderId, ref root);
            HandleWS2ResponseCode("CustomerOrderStatus", respCode, errorText);
            logger.Debug(config.LSKey.Key, "CustomerOrderStatus Response - " + Serialization.ToXml(root, true));

            return new OrderStatusResponse()
            {
                DocumentNo = root.CustomerOrderStatus[0].DocumentID,
                PaymentStatus = root.CustomerOrderStatus[0].PaymentStatus,
                ShippingStatus = root.CustomerOrderStatus[0].ShippingStatus,
                OrderStatus = root.CustomerOrderStatus[0].OrderStatus.ToString()
            };
        }

        public void OrderCancel(string orderId, string storeId, string userId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new LSOmniException(StatusCode.OrderIdNotFound, "OrderStatusCheck orderId can not be empty");

            string respCode = string.Empty;
            string errorText = string.Empty;

            LSCentral.RootCustomerOrderCancel root = new LSCentral.RootCustomerOrderCancel();
            List<LSCentral.CustomerOrderStatusLog> log = new List<LSCentral.CustomerOrderStatusLog>()
            {
                new LSCentral.CustomerOrderStatusLog()
                {
                    StoreNo = storeId,
                    UserID = userId,
                    StaffID = string.Empty,
                    TerminalNo = string.Empty
                }
            };
            root.CustomerOrderStatusLog = log.ToArray();

            centralWS.CustomerOrderCancel(ref respCode, ref errorText, orderId, 0, root);
            HandleWS2ResponseCode("CustomerOrderCancel", respCode, errorText);
        }

        public SalesEntry OrderGet(string id)
        {
            OrderMapping map = new OrderMapping(config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;
            SalesEntry order;

            decimal pointUsed = 0;
            decimal pointEarned = 0;
            LSCentral.RootCustomerOrderGetV3 root = new LSCentral.RootCustomerOrderGetV3();
            centralWS.CustomerOrderGetV3(ref respCode, ref errorText, "LOOKUP", id, "P0001", ref root, ref pointEarned, ref pointUsed);
            HandleWS2ResponseCode("CustomerOrderGet", respCode, errorText);
            logger.Debug(config.LSKey.Key, "CustomerOrderGetV2 Response - " + Serialization.ToXml(root, true));

            order = map.MapFromRootToSalesEntry(root);
            order.PointsRewarded = pointEarned;
            order.PointsUsedInOrder = pointUsed;

            if (string.IsNullOrEmpty(order.CardId))
                order.AnonymousOrder = true;
            return order;
        }

        public List<SalesEntry> OrderHistoryGet(string cardId)
        {
            OrderMapping map = new OrderMapping(config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;

            LSCentral.RootCOFilteredListV2 root = new LSCentral.RootCOFilteredListV2();
            List<LSCentral.CustomerOrderHeaderV2> hd = new List<LSCentral.CustomerOrderHeaderV2>();
            hd.Add(new LSCentral.CustomerOrderHeaderV2()
            {
                MemberCardNo = cardId
            });
            root.CustomerOrderHeaderV2 = hd.ToArray();

            centralWS.COFilteredListV2(ref respCode, ref errorText, true, ref root);
            HandleWS2ResponseCode("CustomerOrderFilteredList", respCode, errorText);
            logger.Debug(config.LSKey.Key, "CustomerOrderFilteredList Response - " + Serialization.ToXml(root, true));
            return map.MapFromRootToSalesEntryHistory(root);
        }

        public OrderAvailabilityResponse OrderAvailabilityCheck(OneList request)
        {
            // Click N Collect
            if (string.IsNullOrWhiteSpace(request.Id))
                request.Id = GuidHelper.NewGuidString();

            OrderMapping map = new OrderMapping(config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;

            string pefSourceLoc = string.Empty;
            LSCentral.RootCOQtyAvailabilityExtOut rootout = new LSCentral.RootCOQtyAvailabilityExtOut();

            LSCentral.RootCOQtyAvailabilityInV2 rootin = map.MapOneListToInvReq2(request);
            logger.Debug(config.LSKey.Key, "COQtyAvailability Request - " + Serialization.ToXml(rootin, true));

            centralWS.COQtyAvailabilityV2(rootin, ref respCode, ref errorText, ref pefSourceLoc, ref rootout);

            HandleWS2ResponseCode("COQtyAvailability", respCode, errorText);
            logger.Debug(config.LSKey.Key, "COQtyAvailability Response - " + Serialization.ToXml(rootout, true));

            OrderAvailabilityResponse data = map.MapRootToOrderavailabilty2(rootin, rootout);
            data.PreferredSourcingLocation = pefSourceLoc;
            return data;
        }

        public string OrderCreate(Order request, out string orderId)
        {
            if (request == null)
                throw new LSOmniException(StatusCode.OrderIdNotFound, "OrderCreate request can not be null");

            orderId = string.Empty;
            if (string.IsNullOrWhiteSpace(request.Id))
                request.Id = GuidHelper.NewGuidString();

            // need to map the TenderType enum coming from devices to TenderTypeId that NAV knows
            if (request.OrderPayments == null)
                request.OrderPayments = new List<OrderPayment>();

            int lineno = 1;
            foreach (OrderPayment line in request.OrderPayments)
            {
                line.TenderType = ConfigSetting.TenderTypeMapping(config.SettingsGetByKey(ConfigKey.TenderType_Mapping), line.TenderType, false); //map tender type between LSOmni and Nav
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

            OrderMapping map = new OrderMapping(config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;

            LSCentral.RootCustomerOrderCreateV5 root = map.MapFromOrderV5ToRoot(request);
            logger.Debug(config.LSKey.Key, "CustomerOrderCreateV5 Request - " + Serialization.ToXml(root, true));
            centralWS.CustomerOrderCreateV5(ref respCode, ref errorText, root, ref orderId);
            HandleWS2ResponseCode("CustomerOrderCreate", respCode, errorText);
            return request.Id;
        }

        #endregion

        #region SalesEntry

        public List<SalesEntry> SalesHistory(string cardId, int numberOfTrans)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;

            LSCentral.RootGetMemberSalesHistory rootHistory = new LSCentral.RootGetMemberSalesHistory();
            logger.Debug(config.LSKey.Key, "GetMemberSalesHistory - CardId: {0}, MaxNoOfHeaders: {1}", cardId, numberOfTrans);
            centralWS.GetMemberSalesHistory(ref respCode, ref errorText, string.Empty, string.Empty, cardId, numberOfTrans, ref rootHistory);
            HandleWS2ResponseCode("GetMemberSalesHistory", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetMemberSalesHistory Response - " + Serialization.ToXml(rootHistory, true));

            TransactionMapping map = new TransactionMapping(config.IsJson);
            return map.MapFromRootToSalesEntries(rootHistory);
        }

        public SalesEntry TransactionGet(string receiptNo, string storeId, string terminalId, int transId)
        {
            TransactionMapping map = new TransactionMapping(config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootGetTransaction root = new LSCentral.RootGetTransaction();
            centralWS.GetTransaction(ref respCode, ref errorText, receiptNo, storeId, terminalId, transId, ref root);
            HandleWS2ResponseCode("GetTransaction", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetTransaction Response - " + Serialization.ToXml(root, true));

            return map.MapFromRootToRetailTransaction(root);
        }

        #endregion

        #region Store

        public Store StoreGetById(string id)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Store", "No.", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository();
            List<Store> list = rep.StoresGet(table);
            if (list.Count == 0)
                return null;

            Store store = list.FirstOrDefault();
            if (store.Currency == null || string.IsNullOrEmpty(store.Currency.Id))
            {
                xmlRequest = xml.GetGeneralWebRequestXML("General Ledger Setup");
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table == null || table.NumberOfValues == 0)
                    return null;

                XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("LCY Code"));
                store.Currency = new Currency(field.Values[0]);
            }

            store.Images = ImagesGetByLink("LSC Store", store.Id, string.Empty, string.Empty);
            return store;
        }

        public List<Store> StoresGet(bool clickAndCollectOnly)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest;
            if (clickAndCollectOnly)
                xmlRequest = xml.GetGeneralWebRequestXML("LSC Store", "Click and Collect", "1");
            else
                xmlRequest = xml.GetGeneralWebRequestXML("LSC Store");

            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository();
            List<Store> list = rep.StoresGet(table);
            foreach (Store store in list)
            {
                if (store.Currency != null && string.IsNullOrEmpty(store.Currency.Id) == false)
                    continue;

                xmlRequest = xml.GetGeneralWebRequestXML("General Ledger Setup");
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table == null || table.NumberOfValues == 0)
                    return null;

                XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("LCY Code"));
                store.Currency = new Currency(field.Values[0]);

                store.Images = ImagesGetByLink("LSC Store", store.Id, string.Empty, string.Empty);
            }
            return list;
        }

        public List<StoreHours> StoreHoursGetByStoreId(string storeId, int offset)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootGetStoreOpeningHours root = new LSCentral.RootGetStoreOpeningHours();

            logger.Debug(config.LSKey.Key, "GetStoreOpeningHours Store: " + storeId);
            centralQryWS.GetStoreOpeningHours(ref respCode, ref errorText, storeId, ref root);
            HandleWS2ResponseCode("GetStoreOpeningHours", respCode, errorText, new string[] { "1000" });
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
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Member Device", "ID", id);
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
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC POS Terminal", "No.", id);
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

        #region ScanPayGo

        public string ScanPayGoSuspend(Order request)
        {
            TransactionMapping map = new TransactionMapping(config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;
            string receiptNo = string.Empty;
            LSCentral.RootMobileTransaction root = map.MapFromOrderToRoot(request);
            root.MobileTransaction[0].TerminalId = config.SettingsGetByKey(ConfigKey.ScanPayGo_Terminal);
            root.MobileTransaction[0].StaffId = config.SettingsGetByKey(ConfigKey.ScanPayGo_Staff);

            logger.Debug(config.LSKey.Key, "MobilePosSuspend Request - " + Serialization.ToXml(root, true));
            centralWS.MobilePosSuspendV2(ref respCode, ref errorText, root, ref receiptNo);
            HandleWS2ResponseCode("MobilePosSuspend", respCode, errorText);
            logger.Debug(config.LSKey.Key, "MobilePosSuspend Response - " + Serialization.ToXml(root, true));
            return receiptNo;
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

            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootGetDirectMarketingInfo root = new LSCentral.RootGetDirectMarketingInfo();
            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo - CardId: {0}", cardId);
            centralWS.GetDirectMarketingInfo(ref respCode, ref errorText, cardId, string.Empty, string.Empty, ref root);
            HandleWS2ResponseCode("GetDirectMarketingInfo", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo Response - " + Serialization.ToXml(root, true));

            ContactMapping map = new ContactMapping(config.IsJson);
            return map.MapFromRootToNotifications(root);
        }

        public List<Hierarchy> HierarchyGet(string storeId)
        {
            List<Hierarchy> list = new List<Hierarchy>();
            List<HierarchyNode> nodes = new List<HierarchyNode>();
            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootGetHierarchyValSched rootRoot = new LSCentral.RootGetHierarchyValSched();
            logger.Debug(config.LSKey.Key, "GetHierarchy - StoreId: {0}", storeId);
            centralQryWS.GetHierarchyV2ValidationSchedule(ref respCode, ref errorText, storeId, ref rootRoot);
            HandleWS2ResponseCode("GetHierarchy", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetHierarchy Response - " + Serialization.ToXml(rootRoot, true));

            foreach (LSCentral.HierarchyValSched top in rootRoot.HierarchyValSched)
            {
                LSCentral.HierarchyDateValSched sch = rootRoot.HierarchyDateValSched.ToList().Find(s => s.HierarchyCode == top.HierarchyCode);
                list.Add(new Hierarchy()
                {
                    Id = top.HierarchyCode,
                    Description = top.Description,
                    Type = (HierarchyType)Convert.ToInt32(top.Type),
                    ValidationScheduleId = (sch != null) ? sch.ValidationScheduleID : string.Empty,
                    Priority = (sch != null) ? sch.Priority : 0,
                    SalesType = (sch != null) ? sch.SalesTypeFilter : string.Empty
                });
            }

            foreach (LSCentral.HierarchyNodesValSched val in rootRoot.HierarchyNodesValSched)
            {
                HierarchyNode node = new HierarchyNode()
                {
                    Id = val.NodeID,
                    ParentNode = val.ParentNodeID,
                    PresentationOrder = val.PresentationOrder,
                    Indentation = val.Indentation,
                    Description = val.Description,
                    HierarchyCode = val.HierarchyCode
                };
                LSCentral.HierarchyNodeImageValSched img = rootRoot.HierarchyNodeImageValSched.ToList().Find(i => i.KeyValue == node.HierarchyCode + "," + node.Id);
                node.ImageId = (img != null) ? img.ImageId : string.Empty;
                nodes.Add(node);

                LSCentral.RootGetHierarchyNodeIn rootNodeIn = new LSCentral.RootGetHierarchyNodeIn();
                LSCentral.RootGetHierarchyNodeOut rootNodeOut = new LSCentral.RootGetHierarchyNodeOut();

                logger.Debug(config.LSKey.Key, "GetHierarchyNode - HierarchyCode: {0}, NodeId: {1}, StoreId: {2}, NodeIn: {3}",
                    val.HierarchyCode, val.NodeID, storeId, Serialization.ToXml(rootNodeIn, true));
                centralQryWS.GetHierarchyNode(ref respCode, ref errorText, val.HierarchyCode, val.NodeID, storeId, rootNodeIn, ref rootNodeOut);
                HandleWS2ResponseCode("GetHierarchyNode", respCode, errorText);
                logger.Debug(config.LSKey.Key, "GetHierarchyNode Response - " + Serialization.ToXml(rootNodeOut, true));

                if (rootNodeOut.HierarchyNodeLink == null)
                    continue;

                foreach (LSCentral.HierarchyNodeLink lnk in rootNodeOut.HierarchyNodeLink)
                {
                    node.Leafs.Add(new HierarchyLeaf()
                    {
                        Id = lnk.No,
                        ParentNode = lnk.NodeID,
                        Description = lnk.Description,
                        HierarchyCode = lnk.HierarchyCode,
                        Type = (HierarchyLeafType)Convert.ToInt32(lnk.Type),
                        SortOrder = lnk.SortOrder,
                        ItemUOM = lnk.ItemUnitofMeasure
                    });
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
            string respCode = string.Empty;
            string errorText = string.Empty;
            ContactMapping map = new ContactMapping(config.IsJson);
            LSCentral.RootGetDirectMarketingInfo root = new LSCentral.RootGetDirectMarketingInfo();

            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo - CardId: {0}, ItemId: {1}", cardId, itemId);
            centralWS.GetDirectMarketingInfo(ref respCode, ref errorText, cardId, itemId, storeId, ref root);
            HandleWS2ResponseCode("GetDirectMarketingInfo", respCode, errorText);
            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo Response - " + Serialization.ToXml(root, true));
            return map.MapFromRootToPublishedOffers(root);
        }

        public List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;

            StoreMapping map = new StoreMapping();
            LSCentral.RootGetReturnPolicy root = new LSCentral.RootGetReturnPolicy();

            logger.Debug(config.LSKey.Key, "GetReturnPolicy - storeId:{0}, storeGroup:{1} itemCat:{2} prodGroup:{3}", storeId, storeGroupCode, itemCategory, productGroup);
            centralWS.GetReturnPolicy(ref respCode, ref errorText, storeId, storeGroupCode, itemCategory, productGroup, itemId, variantCode, variantDim1, ref root);
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
            throw new LSOmniServiceException(StatusCode.GenericError, "Not supported in LS Central");
        }
    }
}
