using System;
using System.Linq;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping;
using LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping.Loyalty;
using LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping.Replication;
using LSOmni.DataAccess.BOConnection.PreCommon.Mapping;
using LSOmni.DataAccess.BOConnection.PreCommon.Mapping25;
using LSOmni.DataAccess.BOConnection.PreCommon.JMapping;

using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Checkout;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment;

namespace LSOmni.DataAccess.BOConnection.PreCommon
{
    public partial class PreCommonBase
    {
        protected static MemCache PublishOfferMemCache = null; //minimize the calls to Nav web service

        #region Item

        public LoyItem ItemGetById(string id, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item", "No.", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository(config);
            LoyItem item = rep.ItemGet(table);

            xmlRequest = xml.GetGeneralWebRequestXML("LSC Item HTML ML", "Item No.", item.Id);
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            table = xml.GetGeneralWebResponseXML(xmlResponse);
            if (table != null && table.NumberOfValues > 0)
            {
                XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Html"));
                item.Details = ConvertTo.Base64Decode(field.Values[0]);
            }

            item.Images = ImagesGetByLink("Item", item.Id, string.Empty, string.Empty, false, stat);

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

            xmlRequest = xml.GetGeneralWebRequestXML("LSC WI Price", "Item No.", item.Id);
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            table = xml.GetGeneralWebResponseXML(xmlResponse);
            item.Prices = rep.GetPrices(table);

            logger.StatisticEndSub(ref stat, index);
            return item;
        }

        public LoyItem ItemGetByBarcode(string barcode, string storeId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Barcodes", "Barcode No.", barcode);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);
            if (table == null || table.NumberOfValues == 0)
            {
                logger.StatisticEndSub(ref stat, index);
                return null;
            }

            XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Item No."));
            string itemId = field.Values[0];
            field = table.FieldList.Find(f => f.FieldName.Equals("Variant Code"));
            string variantId = field.Values[0];
            field = table.FieldList.Find(f => f.FieldName.Equals("Unit of Measure Code"));
            string uomId = field.Values[0];

            LoyItem item = ItemGetById(itemId, stat);

            if (string.IsNullOrWhiteSpace(variantId) == false)
            {
                item.SelectedVariant = item.VariantsRegistration.Find(v => v.Id == variantId);
            }
            if (string.IsNullOrWhiteSpace(uomId) == false)
            {
                item.SelectedUnitOfMeasure = item.UnitOfMeasures.Find(u => u.Id == uomId);
            }
            Price price = item.Prices.Find(p => p.StoreId == storeId && p.ItemId == itemId && p.VariantId == variantId && p.UomId == uomId);
            if (price == null)
                price = item.Prices.Find(p => p.StoreId == storeId && p.ItemId == itemId && p.VariantId == variantId && p.UomId == string.Empty);
            if (price == null)
                price = item.Prices.Find(p => p.StoreId == storeId && p.ItemId == itemId && p.VariantId == string.Empty && p.UomId == uomId);
            if (price == null)
                price = item.Prices.Find(p => p.StoreId == storeId && p.ItemId == itemId && p.VariantId == string.Empty && p.UomId == string.Empty);
            if (price != null)
            {
                item.Prices.Clear();
                item.Prices.Add(price);
                item.Price = price.Amount;
            }

            logger.StatisticEndSub(ref stat, index);
            return item;
        }

        public LoyItem ItemFindByBarcode(string barcode, string storeId, string terminalId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            string itemHTML = string.Empty;
            LSCentral.RootLeftRightLine root = new LSCentral.RootLeftRightLine();

            if (LSCVersion < new Version("18.2"))
            {
                logger.Debug(config.LSKey.Key, "GetItemCard - StoreId:{0}, TermId:{1}, barcode:{2}", storeId, terminalId, barcode);
                centralQryWS.GetItemCard(ref respCode, ref errorText, terminalId, storeId, string.Empty, string.Empty, string.Empty, barcode, string.Empty, ref root);
                HandleWS2ResponseCode("GetItemCard", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "GetItemCard Response - " + Serialization.ToXml(root, true));
            }
            else
            {
                logger.Debug(config.LSKey.Key, "GetItemWithBarcode - StoreId:{0}, TermId:{1}, barcode:{2}", storeId, terminalId, barcode);
                centralQryWS.GetItemWithBarcode(barcode, ref root, ref respCode, ref errorText, ref itemHTML);
                HandleWS2ResponseCode("GetItemWithBarcode", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "GetItemWithBarcode Response - " + Serialization.ToXml(root, true));
            }

            if (root.LeftRightLine == null)
            {
                logger.StatisticEndSub(ref stat, index);
                return null;
            }

            LoyItem item = new LoyItem();
            foreach (LSCentral.LeftRightLine line in root.LeftRightLine)
            {
                switch (line.LeftLine)
                {
                    case "Item":
                        item.Id = line.RightLine;
                        break;
                    case "Price":
                        item.Price = line.RightLine;
                        break;
                    case "Qty":
                        item.GrossWeight = ConvertTo.SafeDecimal(line.RightLine);
                        break;
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return item;
        }

        public List<InventoryResponse> ItemInStockGet(string itemId, string variantId, int arrivingInStockInDays, List<string> locationIds, bool skipUnAvailableStores, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            //locationIds are storeIds in NAV
            string respCode = string.Empty;
            string errorText = string.Empty;
            List<InventoryResponse> list = new List<InventoryResponse>();
            LSCentral.RootGetItemInventory root = new LSCentral.RootGetItemInventory();
            foreach (string id in locationIds)
            {
                centralQryWS.GetItemInventory(ref respCode, ref errorText, itemId, XMLHelper.GetString(variantId), id, string.Empty, string.Empty, string.Empty, string.Empty, arrivingInStockInDays, ref root);
                HandleWS2ResponseCode("GetItemInventory", respCode, errorText, ref stat, index);
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
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public virtual List<InventoryResponse> ItemsInStoreGet(List<InventoryRequest> items, string storeId, string locationId, bool useSourcingLocation, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

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

            if (LSCVersion >= new Version("18.4"))
            {
                centralQryWS.GetInventoryMultipleV2(storeId, locationId, useSourcingLocation, rootin, ref respCode, ref errorText, ref rootout);
                HandleWS2ResponseCode("GetInventoryMultipleV2", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "GetInventoryMultipleV2 Response - " + Serialization.ToXml(rootout, true));
            }
            else
            {
                centralQryWS.GetInventoryMultiple(ref respCode, ref errorText, storeId, locationId, rootin, ref rootout);
                HandleWS2ResponseCode("GetInventoryMultiple", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "GetInventoryMultiple Response - " + Serialization.ToXml(rootout, true));
            }

            if (rootout.InventoryBufferOut == null)
            {
                logger.StatisticEndSub(ref stat, index);
                return list;
            }

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
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public virtual List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId, Statistics stat)
        {
            // filter
            logger.StatisticStartSub(true, ref stat, out int index);

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

            ItemRepository rep = new ItemRepository(config);
            List<HospAvailabilityResponse> list = rep.CurrentAvail(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            if (numberOfItems <= 0)
                numberOfItems = 50;

            List<LoyItem> list = new List<LoyItem>();
            if (string.IsNullOrWhiteSpace(pubOfferId))
            {
                logger.StatisticEndSub(ref stat, index);
                return list;
            }

            PublishedOfferXml xml = new PublishedOfferXml();
            string xmlRequest = xml.PublishedOfferItemsRequestXML(pubOfferId, numberOfItems);
            string xmlResponse = RunOperation(xmlRequest);
            string navResponseCode = GetResponseCode(ref xmlResponse);
            if (navResponseCode == "0004")
            {
                //<Response_Text>Unknown Request_ID LOAD_PUBLISHED_OFFER_ITEMS</Response_Text>
                //0004 = Unknown Request_ID,  NavResponseCode.Error = 0004
                logger.Error(config.LSKey.Key, "responseCode {0} - LOAD_PUBLISHED_OFFER_ITEMS is only supported by default in LS Nav 9.00.03", navResponseCode);
                logger.StatisticEndSub(ref stat, index);
                return list; //request not found so return empty list instead of breaking
            }
            HandleResponseCode(ref xmlResponse);
            list = xml.PublishedOfferItemsResponseXML(xmlResponse);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<LoyItem> ItemsGetByProductGroup(string prodGroup, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item", "LSC Retail Product Code", prodGroup);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository(config);
            List<LoyItem> list = rep.ItemsGet(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<LoyItem> ItemPage(string storeId, int page, bool details, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            if (page < 1)
                page = 1;

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC MobilePlu", "StoreId", storeId, "PageId", page.ToString());
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            List<string> itemno = new List<string>();
            for (int i = 0; i < table.NumberOfValues; i++)
            {
                XMLFieldData fld = table.FieldList.Find(f => f.FieldName == "ItemId");
                itemno.Add(fld.Values[i]);
            }

            List<LoyItem> items = new List<LoyItem>();
            ItemRepository rep = new ItemRepository(config);
            foreach (string no in itemno)
            {
                items.Add(ItemGetById(no, stat));
            }
            logger.StatisticEndSub(ref stat, index);
            return items;
        }

        #endregion

        #region Item Related

        public VariantRegistration VariantRegGetById(string id, string itemId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item Variant Registration", "Variant", id, "Item No.", itemId);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository(config);
            List<VariantRegistration> list = rep.GetVariantRegistrations(table);
            logger.StatisticEndSub(ref stat, index);
            return (list.Count == 0) ? null : list[0];
        }

        public ProductGroup ProductGroupGetById(string id, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Retail Product Group", "Code", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository(config);
            List<ProductGroup> list = rep.ProductGroupGet(table);
            logger.StatisticEndSub(ref stat, index);
            return (list.Count == 0) ? null : list[0];
        }

        public List<ProductGroup> ProductGroupGetByItemCategory(string id, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Retail Product Group", "Item Category Code", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository(config);
            List<ProductGroup> list = rep.ProductGroupGet(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public ItemCategory ItemCategoriesGetById(string id, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item Category", "Code", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository(config);
            List<ItemCategory> list = rep.ItemCategoryGet(table);
            logger.StatisticEndSub(ref stat, index);
            return (list.Count == 0) ? null : list[0];
        }

        public List<ItemCategory> ItemCategories(Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item Category");
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository(config);
            List<ItemCategory> list = rep.ItemCategoryGet(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public ImageView ImageGetById(string imageId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Retail Image", "Code", imageId);
            string xmlResponse = RunOperation(xmlRequest, true, false);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);
            SetupRepository rep = new SetupRepository(config);
            ImageView img = rep.GetImage(table);
            string id = img.Id;

            xmlRequest = xml.GetGeneralWebRequestXML("Tenant Media Set", "ID", img.MediaId.ToString(), "Company Name", NavCompany);
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            table = xml.GetGeneralWebResponseXML(xmlResponse);
            if (table.NumberOfValues == 0)
            {
                logger.StatisticEndSub(ref stat, index);
                return img;
            }

            XMLFieldData fld = table.FieldList.Find(f => f.FieldName == "Media ID");
            img.MediaId = new Guid(fld.Values[0]);

            img = ImageGetByMediaId(img.MediaId.ToString(), stat);
            img.MediaId = new Guid(img.Id);
            img.Id = id;
            logger.StatisticEndSub(ref stat, index);
            return img;
        }

        public ImageView ImageGetByMediaId(string id, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Tenant Media", "ID", id);
            string xmlResponse = RunOperation(xmlRequest, true, false);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ImageView img = new ImageView(id);
            if (table.NumberOfValues == 0)
            {
                logger.StatisticEndSub(ref stat, index);
                return img;
            }

            XMLFieldData fld = table.FieldList.Find(f => f.FieldName == "Content");
            img.ImgBytes = Convert.FromBase64String(fld.Values[0]);

            fld = table.FieldList.Find(f => f.FieldName == "Height");
            int h = Convert.ToInt32(fld.Values[0]);
            fld = table.FieldList.Find(f => f.FieldName == "Width");
            int w = Convert.ToInt32(fld.Values[0]);
            img.ImgSize = new ImageSize(w, h);

            logger.StatisticEndSub(ref stat, index);
            return img;
        }

        public List<ImageView> ImagesGetByLink(string tableName, string key1, string key2, string key3, bool includeBlob, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

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

            SetupRepository rep = new SetupRepository(config);
            List<ImageView> list = rep.GetImageLinks(table);

            if (includeBlob)
            {
                foreach (ImageView link in list)
                {
                    using (ImageView img = ImageGetById(link.Id, stat))
                    {
                        if (img != null)
                        {
                            link.Location = img.Location;
                            link.LocationType = img.LocationType;
                            link.Image = img.Image;
                            link.ImgBytes = img.ImgBytes;
                            link.MediaId = img.MediaId;
                        }
                    }
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items, Statistics stat)
        {
            if (string.IsNullOrWhiteSpace(storeId))
                throw new LSOmniServiceException(StatusCode.MissingStoreId, "Missing Store Id");
            if (string.IsNullOrWhiteSpace(cardId))
                throw new LSOmniServiceException(StatusCode.MemberCardNotFound, "Missing Member Card Id");

            if (items == null && items.Count == 0)
                return new List<ItemCustomerPrice>();

            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            List<ItemCustomerPrice> list = new List<ItemCustomerPrice>();

            if (LSCVersion >= new Version("25.0"))
            {
                OrderMapping25 map = new OrderMapping25(LSCVersion, config.IsJson);
                LSCentral25.RootMobileTransaction root = map.MapFromCustItemToRoot(storeId, cardId, items);
                logger.Debug(config.LSKey.Key, "EcomGetCustomerPrice Request - " + Serialization.ToXml(root, true));
                centralQryWS25.EcomGetCustomerPrice(ref respCode, ref errorText, ref root);
                HandleWS2ResponseCode("EcomGetCustomerPrice", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "EcomGetCustomerPrice Response - " + Serialization.ToXml(root, true));
                list = map.MapFromRootTransactionToItemCustPrice(root);
            }
            else
            {
                OrderMapping map = new OrderMapping(LSCVersion, config.IsJson);
                LSCentral.RootMobileTransaction root = map.MapFromCustItemToRoot(storeId, cardId, items);
                logger.Debug(config.LSKey.Key, "EcomGetCustomerPrice Request - " + Serialization.ToXml(root, true));
                centralQryWS.EcomGetCustomerPrice(ref respCode, ref errorText, ref root);
                HandleWS2ResponseCode("EcomGetCustomerPrice", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "EcomGetCustomerPrice Response - " + Serialization.ToXml(root, true));
                list = map.MapFromRootTransactionToItemCustPrice(root);
            }

            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<ProactiveDiscount> DiscountsGetByStoreAndItem(string storeId, string itemId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string tbname = (LSCVersion >= new Version("21.5")) ? "LSC WI Mix & Match Offer Ext" : "LSC WI Mix & Match Offer";
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest;
            string xmlResponse;
            List<ProactiveDiscount> list = new List<ProactiveDiscount>();
            SetupRepository rep = new SetupRepository(config);

            xmlRequest = xml.GetGeneralWebRequestXML("LSC WI Discounts", "Store No.", storeId, "Item No.", itemId);
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);
            list.AddRange(rep.GetDiscount(table));

            xmlRequest = xml.GetGeneralWebRequestXML(tbname, "Store No.", storeId, "Item No.", itemId);
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            table = xml.GetGeneralWebResponseXML(xmlResponse);
            list.AddRange(rep.GetDiscount(table));

            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public DiscountValidation GetDiscountValidationByOfferId(string offerId, Statistics stat)
        {
            return new DiscountValidation()
            {
                TimeWithinBounds = true,
                StartDate = DateTime.MinValue,
                EndDate = DateTime.MinValue
            };
        }

        public void LoadDiscountDetails(ProactiveDiscount disc, string storeId, string loyaltySchemeCode, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest;
            string xmlResponse;

            string tbname = (LSCVersion >= new Version("21.5")) ? "LSC WI Mix & Match Offer Ext" : "LSC WI Mix & Match Offer";
            SetupRepository rep = new SetupRepository(config);
            xmlRequest = xml.GetGeneralWebRequestXML("LSC Periodic Discount", "No.", disc.Id);
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);
            rep.SetDiscountInfo(table, disc);

            if (disc.Type == ProactiveDiscountType.MixMatch)
            {
                List<string> fields = new List<string>()
                {
                    "Item No."
                };

                List<XMLFieldData> filter = new List<XMLFieldData>()
                {
                    new XMLFieldData()
                    {
                        FieldName = "Offer No.",
                        Values = new List<string>() { disc.Id }
                    },
                    new XMLFieldData()
                    {
                        FieldName = "Store No.",
                        Values = new List<string>() { storeId }
                    },
                    new XMLFieldData()
                    {
                        FieldName = "Loyalty Scheme Code",
                        Values = new List<string>() { "", loyaltySchemeCode }
                    }
                };

                try
                {
                    xmlRequest = xml.GetBatchWebRequestXML(tbname, fields, filter, string.Empty);
                    xmlResponse = RunOperation(xmlRequest, true);
                    HandleResponseCode(ref xmlResponse);
                    table = xml.GetGeneralWebResponseXML(xmlResponse);

                    disc.ItemIds = new List<string>();
                    XMLFieldData fld = table.FieldList.Find(f => f.FieldName == "Item No.");
                    for (int i = 0; i < table.NumberOfValues; i++)
                    {
                        disc.ItemIds.Add(fld.Values[i]);
                    }
                    disc.ItemIds.Remove(disc.ItemId);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "Failed to get Mix&Match items");
                }

                xmlRequest = xml.GetGeneralWebRequestXML("LSC Periodic Discount Benefits", "Offer No.", disc.Id);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);

                disc.BenefitItemIds = new List<string>();
                XMLFieldData fld2 = table.FieldList.Find(f => f.FieldName == "No.");
                for (int i = 0; i < table.NumberOfValues; i++)
                {
                    disc.BenefitItemIds.Add(fld2.Values[i]);
                }
            }
            else if (disc.Type == ProactiveDiscountType.DiscOffer)
            {
                List<string> fields = new List<string>()
                {
                    "Unit Price"
                };

                List<XMLFieldData> filter = new List<XMLFieldData>()
                {
                    new XMLFieldData()
                    {
                        FieldName = "Store No.",
                        Values = new List<string>() { storeId }
                    },
                    new XMLFieldData()
                    {
                        FieldName = "Item No.",
                        Values = new List<string>() { disc.ItemId }
                    },
                    new XMLFieldData()
                    {
                        FieldName = "Variant Code",
                        Values = new List<string>() { disc.VariantId }
                    }
                };

                try
                {
                    xmlRequest = xml.GetBatchWebRequestXML("LSC WI Price", fields, filter, string.Empty);
                    xmlResponse = RunOperation(xmlRequest, true);
                    HandleResponseCode(ref xmlResponse);
                    table = xml.GetGeneralWebResponseXML(xmlResponse);

                    disc.ItemIds = new List<string>();
                    XMLFieldData fld = table.FieldList.Find(f => f.FieldName == "Unit Price");
                    disc.Price = ConvertTo.SafeDecimal(fld.Values[0]);
                    disc.PriceWithDiscount = disc.Price * (1 - disc.Percentage / 100);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "Failed to get Mix&Match items");
                }
            }

            logger.StatisticEndSub(ref stat, index);
        }

        #endregion

        #region Contact

        public MemberContact ContactCreate(MemberContact contact, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            ContactMapping map = new ContactMapping(config.IsJson, LSCVersion);

            string clubId = string.Empty;
            string cardId = string.Empty;
            string contId = string.Empty;
            string acctId = string.Empty;
            string schmId = string.Empty;
            decimal point = 0;

            LSCentral.RootMemberContactCreate root = map.MapToRoot(contact);
            logger.Debug(config.LSKey.Key, "MemberContactCreate Request - " + Serialization.ToXml(root, true));

            centralWS.MemberContactCreate(ref respCode, ref errorText, ref clubId, ref schmId, ref acctId, ref contId, ref cardId, ref point, ref root);
            HandleWS2ResponseCode("MemberContactCreate", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "MemberContactCreate Response - ClubId: {0}, SchemeId: {1}, AccountId: {2}, ContactId: {3}, CardId: {4}, PointsRemaining: {5}",
                clubId, schmId, acctId, contId, cardId, point);

            logger.StatisticEndSub(ref stat, index);
            return new MemberContact(contId)
            {
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                MiddleName = contact.MiddleName,
                Name = contact.Name,
                ExternalSystem = contact.ExternalSystem,
                Cards = new List<Card>()
                {
                    new Card(cardId)
                    {
                        ClubId = clubId,
                    }
                },
                Account = new Account(acctId)
                {
                    Scheme = new Scheme(schmId)
                    {
                        Club = new Club(clubId)
                    },
                }
            };
        }

        public void ContactUpdate(MemberContact contact, string accountId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            if (contact.Profiles == null)
            {
                contact.Profiles = new List<Profile>();
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            ContactMapping map = new ContactMapping(config.IsJson, LSCVersion);

            LSCentral.RootMemberContactCreate1 root = map.MapToRoot1(contact, accountId);
            logger.Debug(config.LSKey.Key, "MemberContactUpdate Request - " + Serialization.ToXml(root, true));
            centralWS.MemberContactUpdate(ref respCode, ref errorText, ref root);
            HandleWS2ResponseCode("MemberContactUpdate", respCode, errorText, ref stat, index);
            logger.StatisticEndSub(ref stat, index);
        }

        public MemberContact ContactGet(string contactId, string accountId, string card, string loginId, string email, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string xmlRequest;
            string xmlResponse;
            string respCode = string.Empty;
            string errorText = string.Empty;
            XMLTableData table;
            XMLFieldData field;
            NAVWebXml xml = new NAVWebXml();
            ContactMapping map = new ContactMapping(config.IsJson, LSCVersion);
            MemberContact contact = null;

            if (LSCVersion >= new Version("21.3"))
            {
                int searchType = 0;
                string searchValue = card;

                if (string.IsNullOrEmpty(contactId) == false)
                {
                    searchType = 2;
                    searchValue = contactId;
                }
                else if (string.IsNullOrEmpty(email) == false)
                {
                    searchType = 3;
                    searchValue = email;
                }
                else if (string.IsNullOrEmpty(loginId) == false)
                {
                    searchType = 5;
                    searchValue = loginId;
                }

                // GetMemberContactInfo(ContactSearchType: Enum "LSC Member Contact Search Type"; SearchText: text; SearchMethod: Enum "LSC Search Method"; MaxResultContacts: Integer): Text
                string data = "{ \"contactSearchType\": \"" + searchType + "\"," +
                                "\"searchText\": \"" + searchValue + "\"," +
                                "\"searchMethod\": \"0\",\"maxResultContacts\": 0 }";

                string ret = SendToOData("GetMemberContactInfo_GetMemberContactInfo", data, false);
                ContactJMapping cmap = new ContactJMapping(config.IsJson);
                List<MemberContact> list = cmap.GetMemberContact(ret);
                logger.StatisticEndSub(ref stat, index);
                return list.FirstOrDefault();
            }

            LSCentral.RootGetMemberContact rootContact = new LSCentral.RootGetMemberContact();
            logger.Debug(config.LSKey.Key, "GetMemberContact2 - CardId: {0}", card);
            centralQryWS.GetMemberContact2(ref respCode, ref errorText, XMLHelper.GetString(card), XMLHelper.GetString(accountId), XMLHelper.GetString(contactId), loginId.ToLower(), email.ToLower(), ref rootContact);
            if (respCode == "1000") // not found
            {
                logger.StatisticEndSub(ref stat, index);
                return null;
            }

            HandleWS2ResponseCode("GetMemberContact2", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "GetMemberContact2 Response - " + Serialization.ToXml(rootContact, true));

            List<Scheme> schemelist = SchemeGetAll(stat);
            contact = map.MapFromRootToContact(rootContact, schemelist);
            contact.UserName = loginId;

            if (contact.Cards.Count == 0)
            {
                logger.StatisticEndSub(ref stat, index);
                return contact;
            }

            Card mcard = contact.Cards.FirstOrDefault();
            if (string.IsNullOrEmpty(loginId))
            {
                xmlRequest = xml.GetGeneralWebRequestXML("LSC Member Login Card", "Card No.", mcard.Id, 1);
                xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);

                if (table != null && table.NumberOfValues > 0)
                {
                    field = table.FieldList.Find(f => f.FieldName.Equals("Login ID"));
                    contact.UserName = field.Values[0];
                    mcard.LoginId = contact.UserName;
                }
            }

            logger.StatisticEndSub(ref stat, index);
            return contact;
        }

        public double ContactAddCard(string contactId, string accountId, string cardId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            decimal points = 0;

            centralWS.MemberCardToContact(ref respCode, ref errorText, cardId, accountId, contactId, ref points);
            HandleWS2ResponseCode("MemberCardToContact", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "MemberCardToContact Response - points:", points);
            logger.StatisticEndSub(ref stat, index);
            return (double)points;
        }

        public List<Customer> CustomerSearch(CustomerSearchType searchType, string search, int maxNumberOfRowsReturned, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string fldname = "Name";
            search += "*";
            switch (searchType)
            {
                case CustomerSearchType.CustomerId:
                    fldname = "No.";
                    break;
                case CustomerSearchType.Email:
                    fldname = "Search E-Mail";
                    break;
                case CustomerSearchType.PhoneNumber:
                    fldname = "Phone No.";
                    break;
            }

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Customer", fldname, search, maxNumberOfRowsReturned);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ContactRepository rep = new ContactRepository(config);
            List<Customer> list = rep.CustomerGet(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public void ConatctBlock(string accountId, string cardId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;

            centralWS.BlockMemberAccount(ref respCode, ref errorText, accountId, cardId);
            HandleWS2ResponseCode("BlockMemberAccount", respCode, errorText, ref stat, index);
            logger.StatisticEndSub(ref stat, index);
        }

        #endregion

        #region Contact Related

        public List<Profile> ProfileGetAll(Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootMobileGetProfiles root = new LSCentral.RootMobileGetProfiles();

            centralQryWS.MobileGetProfiles(ref respCode, ref errorText, string.Empty, string.Empty, ref root);
            HandleWS2ResponseCode("MobileGetProfiles", respCode, errorText, ref stat, index, new string[] { "1000" });
            logger.Debug(config.LSKey.Key, "MobileGetProfiles Response - " + Serialization.ToXml(root, true));
            ContactMapping map = new ContactMapping(config.IsJson, LSCVersion);
            List<Profile> list = map.MapFromRootToProfiles(root);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<Scheme> SchemeGetAll(Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Member Scheme");
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ContactRepository rep = new ContactRepository(config);
            List<Scheme> list = rep.SchemeGetAll(table);

            foreach (Scheme sc in list)
            {
                xmlRequest = xml.GetGeneralWebRequestXML("LSC Member Club", "Code", sc.Club.Id);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table != null && table.NumberOfValues > 0)
                {
                    XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Description"));
                    sc.Club.Name = field.Values[0];
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public MemberContact Logon(string userName, string password, string deviceID, string deviceName, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootMemberLogon root = new LSCentral.RootMemberLogon();
            decimal remainingPoints = 0;

            logger.Debug(config.LSKey.Key, "MemberLogon - userName: {0}", userName);
            centralWS.MemberLogon(ref respCode, ref errorText, userName, password, XMLHelper.GetString(deviceID), XMLHelper.GetString(deviceName), ref remainingPoints, ref root);
            HandleWS2ResponseCode("MemberLogon", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "MemberLogon Response - " + Serialization.ToXml(root, true));

            ContactMapping map = new ContactMapping(config.IsJson, LSCVersion);
            MemberContact contact = map.MapFromRootToLogonContact(root, remainingPoints);
            contact.UserName = userName;
            logger.StatisticEndSub(ref stat, index);
            return contact;
        }

        public MemberContact SocialLogon(string authenticator, string authenticationId, string deviceID, string deviceName, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootMemberauthLogin root = new LSCentral.RootMemberauthLogin();
            decimal remainingPoints = 0;

            logger.Debug(config.LSKey.Key, "MemberAuthenticatorLogin - authenticator: {0}", authenticator);
            centralWS.MemberAuthenticatorLogin(authenticator, authenticationId, XMLHelper.GetString(deviceID), XMLHelper.GetString(deviceName), ref remainingPoints, ref root, ref respCode, ref errorText);
            HandleWS2ResponseCode("MemberAuthenticatorLogin", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "MemberAuthenticatorLogin Response - " + Serialization.ToXml(root, true));

            ContactMapping map = new ContactMapping(config.IsJson, LSCVersion);
            MemberContact data = map.MapFromRootToLogonAuth(root, remainingPoints);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        //Change the password in NAV
        public void ChangePassword(string userName, string token, string newPassword, string oldPassword, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;

            logger.Debug(config.LSKey.Key, "MemberPasswordChange - UserName:{0} Token:{1}", userName, token);
            centralWS.MemberPasswordChange(ref respCode, ref errorText, userName, token, oldPassword, newPassword);
            HandleWS2ResponseCode("MemberPasswordChange", respCode, errorText, ref stat, index);
            logger.StatisticEndSub(ref stat, index);
        }

        public string ResetPassword(string userName, string email, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            string token = string.Empty;
            DateTime tokenExp = DateTime.Now.AddMonths(1);

            logger.Debug(config.LSKey.Key, "MemberPasswordReset - UserName:{0} Email:{1}", userName, email);
            centralWS.MemberPasswordReset(ref respCode, ref errorText, userName, email, ref token, ref tokenExp);
            HandleWS2ResponseCode("MemberPasswordReset", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "MemberPasswordReset Response - Token:{0}", token);
            logger.StatisticEndSub(ref stat, index);
            return token;
        }

        public string SPGPassword(string email, string token, string newPwd, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;

            logger.Debug(config.LSKey.Key, "SPGResetPassword - Email:{0}", email);
            centralWS.SPGResetPassword(email, ref token, ref newPwd, ref respCode, ref errorText);
            HandleWS2ResponseCode("SPGResetPassword", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "SPGResetPassword Response - Token:{0}", token);
            logger.StatisticEndSub(ref stat, index);
            return token;
        }

        public void LoginChange(string oldUserName, string newUserName, string password, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NavXml navXml = new NavXml();
            string xmlRequest = navXml.ChangeLoginRequestXML(oldUserName, newUserName, password);
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            logger.StatisticEndSub(ref stat, index);
        }

        public long MemberCardGetPoints(string cardId, Statistics stat)
        {
            if (string.IsNullOrWhiteSpace(cardId))
                throw new LSOmniException(StatusCode.CardIdInvalid, "cardId can not be empty");

            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootGetMemberCard root = new LSCentral.RootGetMemberCard();
            decimal remainingPoints = 0;

            logger.Debug(config.LSKey.Key, "GetMemberCard - CardId: {0}", cardId);
            centralWS.GetMemberCard(ref respCode, ref errorText, cardId, ref remainingPoints, ref root);
            HandleWS2ResponseCode("GetMemberCard", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "GetMemberCard Response - Remaining points: {0}", Convert.ToInt64(Math.Floor(remainingPoints)));
            logger.StatisticEndSub(ref stat, index);
            return Convert.ToInt64(Math.Floor(remainingPoints));
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo, int pin, string entryType, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;

            if (LSCVersion < new Version("24.0"))
            {
                NAVWebXml xml = new NAVWebXml();
                string xmlRequest = xml.GetGeneralWebRequestXML("LSC POS Data Entry", "Entry Type", entryType, "Entry Code", cardNo);
                string xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

                GiftCard card = new GiftCard(cardNo);
                card.EntryType = entryType;
                if (table != null && table.NumberOfValues > 0)
                {
                    XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Expiring Date"));
                    card.ExpireDate = ConvertTo.SafeDateTime(field.Values[0]);
                }

                xmlRequest = xml.GetGeneralWebRequestXML("LSC Voucher Entries", "Voucher No.", cardNo, "Voucher Type", entryType);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table != null)
                {
                    XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Amount"));
                    for (int i = 0; i < table.NumberOfValues; i++)
                    {
                        card.Balance += (decimal)ConvertTo.SafeDouble(field.Values[i]);
                    }
                }
                return card;
            }

            LSCentral.RootGetDataEntryBalance root = new LSCentral.RootGetDataEntryBalance();
            logger.Debug(config.LSKey.Key, "GetDataEntryBalanceV2 - GiftCardNo: {0}", cardNo);
            centralQryWS.GetDataEntryBalanceV2(ref respCode, ref errorText, XMLHelper.GetString(entryType), cardNo, pin, ref root);
            HandleWS2ResponseCode("GetDataEntryBalanceV2", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "GetDataEntryBalanceV2 response - " + Serialization.ToXml(root, true));
            if (root.POSDataEntry == null)
            {
                throw new LSOmniServiceException(StatusCode.GiftCardNotFound, "Gift card not found");
            }

            LSCentral.POSDataEntry entry = root.POSDataEntry.First();
            logger.StatisticEndSub(ref stat, index);
            return new GiftCard(cardNo)
            {
                Pin = entry.PIN,
                EntryType = entryType,
                Balance = entry.Balance,
                ExpireDate = entry.ExpiryDate,
                CurrencyCode = entry.CurrencyCode
            };
        }

        public virtual List<GiftCardEntry> GiftCardGetHistory(string cardNo, int pin, string entryType, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootGetVoucherEntries root = new LSCentral.RootGetVoucherEntries();

            if (LSCVersion >= new Version("24.0"))
            {
                logger.Debug(config.LSKey.Key, "GetVoucherEntriesV2 - GiftCardNo: {0}", cardNo);
                centralWS.GetVoucherEntriesV2(ref respCode, ref errorText, cardNo, pin, ref root);
                HandleWS2ResponseCode("GetVoucherEntriesV2", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "GetVoucherEntriesV2 Response - " + Serialization.ToXml(root, true));
            }
            else
            {
                logger.Debug(config.LSKey.Key, "GetVoucherEntries - GiftCardNo: {0}", cardNo);
                centralWS.GetVoucherEntries(ref respCode, ref errorText, cardNo, ref root);
                HandleWS2ResponseCode("GetVoucherEntries", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "GetVoucherEntries Response - " + Serialization.ToXml(root, true));
            }

            StoreMapping map = new StoreMapping();
            List<GiftCardEntry> list = map.MapFromRootToGiftCardEntry(root);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public decimal GetPointRate(string currency, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string loyCur = currency;
            if (string.IsNullOrEmpty(currency))
            {
                loyCur = config.SettingsGetByKey(ConfigKey.Currency_LoyCode);
                if (string.IsNullOrEmpty(loyCur))
                    loyCur = "LOY";
            }

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Currency Exchange Rate", "Currency Code", loyCur.ToUpper(), 1, true);
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository(config);
            decimal rate = rep.GetPointRate(table);

            if (string.IsNullOrEmpty(currency) == false)
            {
                rate = 1 / rate;
            }

            logger.StatisticEndSub(ref stat, index);
            return rate;
        }

        public virtual List<PointEntry> PointEntriesGet(string cardNo, DateTime dateFrom, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Member Point Entry", "Card No.", cardNo);
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ContactRepository rep = new ContactRepository(config);
            List<PointEntry> list = rep.PointEntryGet(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<Notification> NotificationsGetByCardId(string cardId, Statistics stat)
        {
            List<Notification> pol = new List<Notification>();
            if (string.IsNullOrWhiteSpace(cardId))
                return pol;

            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootGetDirectMarketingInfo root = new LSCentral.RootGetDirectMarketingInfo();
            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo - CardId: {0}", cardId);
            centralQryWS.GetDirectMarketingInfo(ref respCode, ref errorText, cardId, string.Empty, string.Empty, ref root);
            HandleWS2ResponseCode("GetDirectMarketingInfo", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo Response - " + Serialization.ToXml(root, true));

            ContactMapping map = new ContactMapping(config.IsJson, LSCVersion);
            List<Notification> list = map.MapFromRootToNotifications(root);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            ContactMapping map = new ContactMapping(config.IsJson, LSCVersion);
            LSCentral.RootGetDirectMarketingInfo root = new LSCentral.RootGetDirectMarketingInfo();

            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo - CardId: {0}, ItemId: {1}", cardId, itemId);
            centralQryWS.GetDirectMarketingInfo(ref respCode, ref errorText, XMLHelper.GetString(cardId), XMLHelper.GetString(itemId), XMLHelper.GetString(storeId), ref root);
            HandleWS2ResponseCode("GetDirectMarketingInfo", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo Response - " + Serialization.ToXml(root, true));
            List<PublishedOffer> data = map.MapFromRootToPublishedOffers(root);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        #endregion

        #region Searches

        public List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned, bool exact, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            if (LSCVersion >= new Version("21.3"))
            {
                // GetMemberContactInfo(ContactSearchType: Enum "LSC Member Contact Search Type"; SearchText: text; SearchMethod: Enum "LSC Search Method"; MaxResultContacts: Integer): Text
                string data = "{ \"contactSearchType\": \"" + ((int)searchType).ToString() + "\"," +
                                "\"searchText\": \"" + search + "\"," +
                                "\"searchMethod\": \"" + ((exact) ? "0" : "1") + "\"," +
                                "\"maxResultContacts\": " + maxNumberOfRowsReturned.ToString() +
                                " }";

                string ret = SendToOData("GetMemberContactInfo_GetMemberContactInfo", data, false);
                ContactJMapping cmap = new ContactJMapping(config.IsJson);
                return cmap.GetMemberContact(ret);
            }

            string fldname = "Search Name";
            switch (searchType)
            {
                case ContactSearchType.ContactNumber:
                    fldname = "Contact No.";
                    break;
                case ContactSearchType.Email:
                    fldname = "Search E-Mail";
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

            ContactRepository rep = new ContactRepository(config);
            List<MemberContact> list = rep.ContactGet(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<LoyItem> ItemSearch(string search, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            search = string.Format("*{0}*", search.ToUpper());

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item", "Search Description", search);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository(config);
            List<LoyItem> list = rep.ItemsGet(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<ItemCategory> ItemCategorySearch(string search, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            search = string.Format("*{0}*", search);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item Category", "Description", search);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository(config);
            List<ItemCategory> list = rep.ItemCategoryGet(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<ProductGroup> ProductGroupSearch(string search, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            search = string.Format("*{0}*", search);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Retail Product Group", "Description", search);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ItemRepository rep = new ItemRepository(config);
            List<ProductGroup> list = rep.ProductGroupGet(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<Profile> ProfileSearch(string search, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            search = string.Format("*{0}*", search);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Member Attribute", "Description", search);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ContactRepository rep = new ContactRepository(config);
            List<Profile> list = rep.ProfileGet(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<Store> StoreSearch(string search, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            search = string.Format("*{0}*", search);
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Store", "Name", search);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository(config);
            List<Store> list = rep.StoresGet(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        #endregion

        #region Hospitality Order

        public OrderHosp HospOrderCalculate(OneList list, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            if (string.IsNullOrWhiteSpace(list.Id))
                list.Id = GuidHelper.NewGuidString();

            // find highest lineNo count in sub line, if user has set some
            int lineno = list.Items.Max(i => i.LineNumber);
            if (lineno == 0)
                lineno = 1;

            if (lineno >= 10000)
                lineno = lineno / 10000;

            int sublineno = 0;
            foreach (OneListItem item in list.Items)
            {
                if (item.OnelistSubLines.Count == 0)
                    continue;

                int x = item.OnelistSubLines.Max(i => i.LineNumber);
                if (x >= 10000)
                    x = x / 10000;
                if (x > sublineno)
                    sublineno = x;
            }
            sublineno++;

            foreach (OneListItem item in list.Items)
            {
                if (item.LineNumber == 0)
                    item.LineNumber = XMLHelper.LineNumberToNav(lineno++);

                if (item.OnelistSubLines != null)
                {
                    foreach (OneListItemSubLine sub in item.OnelistSubLines)
                    {
                        if (sub.LineNumber == 0)
                            sub.LineNumber = XMLHelper.LineNumberToNav(sublineno++);
                    }
                }
            }

            if (list.PublishedOffers != null)
            {
                foreach (OneListPublishedOffer off in list.PublishedOffers)
                {
                    off.LineNumber = XMLHelper.LineNumberToNav(lineno++);
                }
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            OrderHosp data = null;

            if (LSCVersion >= new Version("25.0"))
            {
                TransactionMapping25 map = new TransactionMapping25(LSCVersion, config.IsJson);
                LSCentral25.RootMobileTransaction root = map.MapFromOrderToRoot(list);
                logger.Debug(config.LSKey.Key, "MobilePosCalculate Request - " + Serialization.ToXml(root, true));
                centralQryWS25.MobilePosCalculate(ref respCode, ref errorText, string.Empty, ref root);
                HandleWS2ResponseCode("MobilePosCalculate", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "MobilePosCalculate Response - " + Serialization.ToXml(root, true));
                data = map.MapFromRootToOrderHosp(root);
            }
            else
            {
                TransactionMapping map = new TransactionMapping(LSCVersion, config.IsJson);
                LSCentral.RootMobileTransaction root = map.MapFromOrderToRoot(list);
                logger.Debug(config.LSKey.Key, "MobilePosCalculate Request - " + Serialization.ToXml(root, true));
                centralQryWS.MobilePosCalculate(ref respCode, ref errorText, string.Empty, ref root);
                HandleWS2ResponseCode("MobilePosCalculate", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "MobilePosCalculate Response - " + Serialization.ToXml(root, true));
                data = map.MapFromRootToOrderHosp(root);
            }

            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public string HospOrderCreate(OrderHosp request, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

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

            if (LSCVersion >= new Version("25.0"))
            {
                TransactionMapping25 map = new TransactionMapping25(LSCVersion, config.IsJson);
                LSCentral25.RootHospTransaction root = map.MapFromOrderToRoot(request);
                logger.Debug(config.LSKey.Key, "CreateHospOrder Request - " + Serialization.ToXml(root, true));
                centralWS25.CreateHospOrder(ref respCode, ref errorText, string.Empty, ref receiptNo, ref root);
                HandleWS2ResponseCode("CreateHospOrder", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "CreateHospOrder Response - " + Serialization.ToXml(root, true));
            }
            else
            {
                TransactionMapping map = new TransactionMapping(LSCVersion, config.IsJson);
                LSCentral.RootHospTransaction root = map.MapFromOrderToRoot(request);
                logger.Debug(config.LSKey.Key, "CreateHospOrder Request - " + Serialization.ToXml(root, true));
                centralWS.CreateHospOrder(ref respCode, ref errorText, string.Empty, ref receiptNo, ref root);
                HandleWS2ResponseCode("CreateHospOrder", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "CreateHospOrder Response - " + Serialization.ToXml(root, true));
            }

            logger.StatisticEndSub(ref stat, index);
            return receiptNo;
        }

        public void HospOrderCancel(string storeId, string orderId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;

            centralWS.CancelHospOrder(ref respCode, ref errorText, orderId, storeId);
            HandleWS2ResponseCode("CancelHospOrder", respCode, errorText, ref stat, index);
            logger.StatisticEndSub(ref stat, index);
        }

        public OrderHospStatus HospOrderKotStatus(string storeId, string orderId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            int estTime = 0;

            centralQryWS.GetHospOrderEstimatedTime(ref respCode, ref errorText, storeId, orderId, ref estTime);
            HandleWS2ResponseCode("GetHospOrderEstimatedTime", respCode, errorText, ref stat, index);

            OrderHospStatus hstatus = new OrderHospStatus();
            if (LSCVersion >= new Version("25.0"))
            {
                LSCentral25.RootKotStatus root = new LSCentral25.RootKotStatus();
                centralQryWS25.GetKotStatus(ref respCode, ref errorText, storeId, orderId, ref root);
                HandleWS2ResponseCode("GetKotStatus", respCode, errorText, ref stat, index);
                logger.StatisticEndSub(ref stat, index);
                if (root.KotStatus != null && root.KotStatus.Length > 0)
                {
                    hstatus = new OrderHospStatus()
                    {
                        ReceiptNo = root.KotStatus[0].ReceiptNo,
                        KotNo = root.KotStatus[0].KotNo,
                        QueueCounter = root.KotStatus[0].OrderID,
                        Status = (KOTStatus)Convert.ToInt32(root.KotStatus[0].Status),
                        Confirmed = root.KotStatus[0].ConfirmedbyExp,
                        ProductionTime = root.KotStatus[0].KotProdTime,
                        EstimatedTime = estTime
                    };
                }
            }
            else
            {
                LSCentral.RootKotStatus root = new LSCentral.RootKotStatus();
                centralQryWS.GetKotStatus(ref respCode, ref errorText, storeId, orderId, ref root);
                HandleWS2ResponseCode("GetKotStatus", respCode, errorText, ref stat, index);
                if (root.KotStatus != null && root.KotStatus.Length > 0)
                {
                    hstatus = new OrderHospStatus()
                    {
                        ReceiptNo = root.KotStatus[0].ReceiptNo,
                        KotNo = root.KotStatus[0].KotNo,
                        Status = (KOTStatus)Convert.ToInt32(root.KotStatus[0].Status),
                        Confirmed = root.KotStatus[0].ConfirmedbyExp,
                        ProductionTime = root.KotStatus[0].KotProdTime,
                        EstimatedTime = estTime
                    };
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return hstatus;
        }

        #endregion

        #region Order

        public string OrderCreate(Order request, out string orderId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            orderId = string.Empty;
            string respCode = string.Empty;
            string errorText = string.Empty;
            string loyCur = config.SettingsGetByKey(ConfigKey.Currency_LoyCode);
            if (string.IsNullOrEmpty(loyCur))
                loyCur = "LOY";

            if (LSCVersion >= new Version("25.0"))
            {
                OrderMapping25 map = new OrderMapping25(LSCVersion, config.IsJson);
                LSCentral25.RootCustomerOrderCreateV5 root = map.MapFromOrderV5ToRoot(request, loyCur);
                logger.Debug(config.LSKey.Key, "CustomerOrderCreateV5 Request - " + Serialization.ToXml(root, true));
                centralWS25.CustomerOrderCreateV5(ref respCode, ref errorText, root, ref orderId);
            }
            else
            {
                OrderMapping map = new OrderMapping(LSCVersion, config.IsJson);
                LSCentral.RootCustomerOrderCreateV5 root = map.MapFromOrderV5ToRoot(request, loyCur);
                logger.Debug(config.LSKey.Key, "CustomerOrderCreateV5 Request - " + Serialization.ToXml(root, true));
                centralWS.CustomerOrderCreateV5(ref respCode, ref errorText, root, ref orderId);
            }
            if ((respCode == "1000") && errorText.StartsWith("External ID") && errorText.Contains("already exists"))
            {
                // wrong error code return, check Error text
                logger.Warn(config.LSKey.Key, "Error:{0} >> Ignore Error and return External ID (assume Order exist)", errorText);
            }
            else
            {
                string ret = HandleWS2ResponseCode("CustomerOrderCreateV5", respCode, errorText, ref stat, index, new string[] { "2201" });
                if (string.IsNullOrEmpty(ret) == false)     // ExtId Exists
                    logger.Warn(config.LSKey.Key, "Error:{0} >> Ignore Error and return External ID (assume Order exist)", errorText);
            }
            logger.StatisticEndSub(ref stat, index);
            return request.Id;
        }

        public string OrderEdit(Order request, ref string orderId, OrderEditType editType, Statistics stat)
        {
            if (editType == OrderEditType.None)
                return request.Id;

            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            string coEditType = string.Empty;
            if (editType != OrderEditType.None)
                coEditType = ((int)editType).ToString();
            string loyCur = config.SettingsGetByKey(ConfigKey.Currency_LoyCode);
            if (string.IsNullOrEmpty(loyCur))
                loyCur = "LOY";

            OrderMapping map = new OrderMapping(LSCVersion, config.IsJson);
            LSCentral.RootCustomerOrderEdit root = map.MapFromOrderEditToRoot(request, orderId, editType, loyCur);
            logger.Debug(config.LSKey.Key, "CustomerOrderEdit Request - " + Serialization.ToXml(root, true));
            centralWS.CustomerOrderEdit(ref respCode, ref errorText, root, ref orderId, coEditType);
            HandleWS2ResponseCode("CustomerOrderEdit", respCode, errorText, ref stat, index);
            logger.StatisticEndSub(ref stat, index);
            return request.Id;
        }

        public bool OrderUpdatePayment(string orderId, string storeId, OrderPayment payment, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            string loyCur = config.SettingsGetByKey(ConfigKey.Currency_LoyCode);
            if (string.IsNullOrEmpty(loyCur))
                loyCur = "LOY";

            bool preAuthNotAuth = false;
            OrderMapping25 map = new OrderMapping25(LSCVersion, config.IsJson);
            LSCentral25.RootCOUpdatePayment root = map.MapFromOrderPaymentToRoot(orderId, storeId, payment, loyCur);
            logger.Debug(config.LSKey.Key, "COUpdatePayment Request - " + Serialization.ToXml(root, true));
            centralWS25.COUpdatePayment(ref respCode, ref errorText, ref preAuthNotAuth, ref root);
            HandleWS2ResponseCode("COUpdatePayment", respCode, errorText, ref stat, index);
            logger.StatisticEndSub(ref stat, index);
            return preAuthNotAuth;
        }

        public bool CompressCOActive(Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            bool isActive = false;
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Customer Order Setup");
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);
            if (table != null && table.NumberOfValues > 0)
            {
                XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Compressed Lines Active"));
                if (field != null)
                    isActive = ConvertTo.SafeBoolean(field.Values[0]);
            }
            logger.Debug(config.LSKey.Key, $"CO Compress Status: {isActive}");
            logger.StatisticEndSub(ref stat, index);
            return isActive;
        }

        public Order BasketCalcToOrder(OneList list, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            if (string.IsNullOrWhiteSpace(list.Id))
                list.Id = GuidHelper.NewGuidString();

            int lineno = 1;
            foreach (OneListItem item in list.Items)
            {
                item.LineNumber = XMLHelper.LineNumberToNav(lineno++);
            }
            if (list.PublishedOffers != null)
            {
                foreach (OneListPublishedOffer off in list.PublishedOffers)
                {
                    off.LineNumber = XMLHelper.LineNumberToNav(lineno++);
                }
            }

            string respCode = string.Empty;
            string errorText = string.Empty;
            Order order = null;

            if (LSCVersion >= new Version("25.0"))
            {
                OrderMapping25 map = new OrderMapping25(LSCVersion, config.IsJson);
                LSCentral25.RootMobileTransaction root = map.MapFromRetailTransactionToRoot(list);
                logger.Debug(config.LSKey.Key, "EcomCalculateBasket Request - " + Serialization.ToXml(root, true));
                centralQryWS25.EcomCalculateBasket(ref respCode, ref errorText, ref root);
                HandleWS2ResponseCode("EcomCalculateBasket", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "EcomCalculateBasket Response - " + Serialization.ToXml(root, true));
                order = map.MapFromRootTransactionToOrder(root);
            }
            else
            {
                OrderMapping map = new OrderMapping(LSCVersion, config.IsJson);
                LSCentral.RootMobileTransaction root = map.MapFromRetailTransactionToRoot(list);
                logger.Debug(config.LSKey.Key, "EcomCalculateBasket Request - " + Serialization.ToXml(root, true));
                centralQryWS.EcomCalculateBasket(ref respCode, ref errorText, ref root);
                HandleWS2ResponseCode("EcomCalculateBasket", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "EcomCalculateBasket Response - " + Serialization.ToXml(root, true));
                order = map.MapFromRootTransactionToOrder(root);
            }

            if (CompressCOActive(stat) == false)
            {
                List<OrderLine> lines = new List<OrderLine>();
                foreach (OrderLine line in order.OrderLines)
                {
                    OrderLine oline = lines.Find(l =>
                            l.ItemId == line.ItemId &&
                            l.VariantId == line.VariantId &&
                            l.UomId == line.UomId &&
                            l.LineType == line.LineType);

                    if (oline == null)
                    {
                        lines.Add(line);
                    }
                    else
                    {
                        OneListItem item = list.Items.ToList().Find(i => i.ItemId == line.ItemId && i.VariantId == line.VariantId && i.UnitOfMeasureId == line.UomId);
                        if (item == null)
                            item = list.Items.ToList().Find(i => i.ItemId == line.ItemId && i.VariantId == line.VariantId);

                        if (item != null && item.Immutable)
                        {
                            lines.Add(line);
                            continue;
                        }

                        if ((oline.DiscountLineNumbers.Count() > 0 && line.DiscountLineNumbers.Count() == 0) ||
                            (oline.DiscountLineNumbers.Count() == 0 && line.DiscountLineNumbers.Count() > 0))
                        {
                            lines.Add(line);
                            continue;
                        }

                        oline.DiscountAmount += line.DiscountAmount;
                        oline.NetAmount += line.NetAmount;
                        oline.Quantity += line.Quantity;
                        oline.TaxAmount += line.TaxAmount;
                        oline.Amount += line.Amount;
                    }
                }
                order.OrderLines = lines;
            }
            logger.StatisticEndSub(ref stat, index);
            return order;
        }

        public OrderStatusResponse OrderStatusCheck(string orderId, Statistics stat)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new LSOmniException(StatusCode.OrderIdNotFound, "OrderStatusCheck orderId can not be empty");

            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            if (LSCVersion < new Version("18.2"))
            {
                LSCentral.RootCustomerOrderStatus root = new LSCentral.RootCustomerOrderStatus();
                centralWS.CustomerOrderStatus(ref respCode, ref errorText, orderId, ref root);
                HandleWS2ResponseCode("CustomerOrderStatus", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "CustomerOrderStatus Response - " + Serialization.ToXml(root, true));

                logger.StatisticEndSub(ref stat, index);
                return new OrderStatusResponse()
                {
                    DocumentNo = root.CustomerOrderStatus[0].DocumentID,
                    OrderStatus = root.CustomerOrderStatus[0].OrderStatus.ToString()
                };
            }
            else
            {
                LSCentral.RootNodeName root = new LSCentral.RootNodeName();
                centralWS.CustomerOrderStatusV2(ref respCode, ref errorText, orderId, ref root);
                HandleWS2ResponseCode("CustomerOrderStatusV2", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "CustomerOrderStatusV2 Response - " + Serialization.ToXml(root, true));

                OrderStatusResponse resp = new OrderStatusResponse();
                if (root.CustomerOrderStatusLog == null)
                {
                    logger.StatisticEndSub(ref stat, index);
                    return resp;
                }

                resp.DocumentNo = root.CustomerOrderStatusLog[0].DocumentID;
                resp.Description = root.CustomerOrderStatusLog[0].Description;
                resp.ExtCode = root.CustomerOrderStatusLog[0].ExtCode;
                resp.OrderStatus = root.CustomerOrderStatusLog[0].StatusCode;

                if (root.CustomerOrderStatusLineLog != null)
                {
                    foreach (LSCentral.CustomerOrderStatusLineLog it in root.CustomerOrderStatusLineLog)
                    {
                        resp.Lines.Add(new OrderLineStatus()
                        {
                            LineNumber = it.ItemLineNo,
                            LineStatus = it.StatusCode,
                            ExtCode = it.ExtCode,
                            ItemId = it.ItenNo,
                            VariantId = it.VariantCode,
                            UnitOfMeasureId = it.UnitOfMeasure,
                            Quantity = it.Quantity,
                            Description = it.Description,
                            AllowModify = it.AllowModify,
                            AllowCancel = it.AllowCancel
                        });
                    }
                }
                logger.StatisticEndSub(ref stat, index);
                return resp;
            }
        }

        public void OrderCancel(string orderId, string storeId, string userId, List<OrderCancelLine> lines, Statistics stat)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new LSOmniException(StatusCode.OrderIdNotFound, "OrderStatusCheck orderId can not be empty");

            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;

            if (LSCVersion >= new Version("25.0"))
            {
                OrderMapping25 map = new OrderMapping25(LSCVersion, config.IsJson);
                LSCentral25.RootCustomerOrderCancel root = map.OrderCancelToRoot(orderId, storeId, userId, lines);
                logger.Debug(config.LSKey.Key, "CustomerOrderCancel Request - " + Serialization.ToXml(root, true));
                centralWS25.CustomerOrderCancel(ref respCode, ref errorText, orderId, 0, root);
                HandleWS2ResponseCode("CustomerOrderCancel", respCode, errorText, ref stat, index);
            }
            else
            {
                OrderMapping map = new OrderMapping(LSCVersion, config.IsJson);
                LSCentral.RootCustomerOrderCancel root = map.OrderCancelToRoot(orderId, storeId, userId, lines);
                logger.Debug(config.LSKey.Key, "CustomerOrderCancel Request - " + Serialization.ToXml(root, true));
                centralWS.CustomerOrderCancel(ref respCode, ref errorText, orderId, 0, root);
                HandleWS2ResponseCode("CustomerOrderCancel", respCode, errorText, ref stat, index);
            }
            logger.StatisticEndSub(ref stat, index);
        }

        public SalesEntry OrderGet(string id, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            OrderMapping map = new OrderMapping(LSCVersion, config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;
            SalesEntry order;

            decimal pointUsed = 0;
            decimal pointEarned = 0;
            LSCentral.RootCustomerOrderGetV3 root = new LSCentral.RootCustomerOrderGetV3();
            centralWS.CustomerOrderGetV3(ref respCode, ref errorText, "LOOKUP", id, string.Empty, ref root, ref pointEarned, ref pointUsed);
            HandleWS2ResponseCode("CustomerOrderGetV3", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "CustomerOrderGetV3 Response - " + Serialization.ToXml(root, true));

            order = map.MapFromRootToSalesEntry(root);
            order.PointsRewarded = pointEarned;
            order.PointsUsedInOrder = pointUsed;

            Store store = StoreGetById(order.StoreId, stat);
            order.StoreName = store.Description;
            order.StoreCurrency = store.Currency.Id;

            bool compress = CompressCOActive(stat);

            List<SalesEntryLine> list = new List<SalesEntryLine>();
            foreach (SalesEntryLine line in order.Lines)
            {
                SalesEntryLine exline = list.Find(l => l.Id.Equals(line.Id) && l.ItemId.Equals(line.ItemId) && l.VariantId.Equals(line.VariantId) && l.UomId.Equals(line.UomId));
                if (exline == null || compress)
                {
                    if (string.IsNullOrEmpty(line.VariantId))
                    {
                        List<ImageView> img = ImagesGetByLink("Item", line.ItemId, string.Empty, string.Empty, false, stat);
                        if (img != null && img.Count > 0)
                            line.ItemImageId = img[0].Id;
                    }
                    else
                    {
                        List<ImageView> img = ImagesGetByLink("Item Variant", line.ItemId, line.VariantId, string.Empty, false, stat);
                        if (img != null && img.Count > 0)
                            line.ItemImageId = img[0].Id;
                    }

                    list.Add(line);
                    continue;
                }

                SalesEntryDiscountLine dline = order.DiscountLines.Find(l => l.LineNumber >= line.LineNumber && l.LineNumber < line.LineNumber + 10000);
                if (dline != null)
                {
                    // update discount line number to match existing record, as we will sum up the orderlines
                    dline.LineNumber = exline.LineNumber + dline.LineNumber / 100;
                }

                exline.Amount += line.Amount;
                exline.NetAmount += line.NetAmount;
                exline.DiscountAmount += line.DiscountAmount;
                exline.TaxAmount += line.TaxAmount;
                exline.Quantity += line.Quantity;
            }
            order.Lines = list;

            if (string.IsNullOrEmpty(order.CardId))
                order.AnonymousOrder = true;
            logger.StatisticEndSub(ref stat, index);
            return order;
        }

        public List<SalesEntry> OrderHistoryGet(string cardId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            OrderMapping map = new OrderMapping(LSCVersion, config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;

            LSCentral.RootCOFilteredListV2 root = new LSCentral.RootCOFilteredListV2();
            List<LSCentral.CustomerOrderHeaderV2> hd = new List<LSCentral.CustomerOrderHeaderV2>();
            hd.Add(new LSCentral.CustomerOrderHeaderV2()
            {
                MemberCardNo = cardId,
                StatusInt = "2"
            });
            root.CustomerOrderHeaderV2 = hd.ToArray();

            centralQryWS.COFilteredListV2(ref respCode, ref errorText, false, ref root);
            HandleWS2ResponseCode("COFilteredListV2", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "COFilteredListV2 Response - " + Serialization.ToXml(root, true));
            List<SalesEntry> list = map.MapFromRootToSalesEntryHistory(root);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public OrderAvailabilityResponse OrderAvailabilityCheck(OneList request, bool shippingOrder, Statistics stat)
        {
            // Click N Collect
            logger.StatisticStartSub(true, ref stat, out int index);

            if (string.IsNullOrWhiteSpace(request.Id))
                request.Id = GuidHelper.NewGuidString();

            OrderMapping map = new OrderMapping(LSCVersion, config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;

            string pefSourceLoc = string.Empty;
            LSCentral.RootCOQtyAvailabilityExtOut rootout = new LSCentral.RootCOQtyAvailabilityExtOut();

            LSCentral.RootCOQtyAvailabilityInV2 rootin = map.MapOneListToInvReq2(request, shippingOrder);
            logger.Debug(config.LSKey.Key, "COQtyAvailabilityV2 Request - " + Serialization.ToXml(rootin, true));

            centralWS.COQtyAvailabilityV2(rootin, ref respCode, ref errorText, ref pefSourceLoc, ref rootout);
            HandleWS2ResponseCode("COQtyAvailabilityV2", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "COQtyAvailabilityV2 Response - " + Serialization.ToXml(rootout, true));

            OrderAvailabilityResponse data = map.MapRootToOrderavailabilty2(rootin, rootout);
            data.PreferredSourcingLocation = pefSourceLoc;
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        #endregion

        #region SalesEntry

        public List<SalesEntry> SalesHistory(string cardId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            // fields to get 
            List<string> fields = new List<string>()
            {
                "Receipt No.", "Store No.", "POS Terminal No.", "Transaction No.", "Date", "Time"
            };

            List<XMLFieldData> filter = new List<XMLFieldData>()
            {
                new XMLFieldData()
                {
                    FieldName = "Member Card No.",
                    Values = new List<string>() { cardId }
                }
            };

            // get Qty for UOM
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetBatchWebRequestXML("LSC Transaction Header", fields, filter, string.Empty);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository(config);
            List<SalesEntry> list = rep.SalesEntryList(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<SalesEntry> SalesEntryGetByCardId(string cardId, int maxRecs, string storeNo, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            // GetMemberContactSalesHistory(MemberCardNo: Text[100]; MaxResultContacts: Integer): Text
            string data = "{ \"memberCardNo\": \"" + cardId + "\"," +
                            "\"maxResultContacts\": " + maxRecs.ToString() + " }";

            string ret = SendToOData("GetMemberContSalesHistory_GetMemberContactSalesHistory", data, false);
            OrderJMapping omap = new OrderJMapping(config.IsJson);
            List<SalesEntry> list;
            list = omap.GetSalesEntryHistory(ret);
            list = list.OrderByDescending(o => o.DocumentRegTime).ToList();

            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public SalesEntry SalesEntryGetById(string docId, DocumentIdType type, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            int dtype = 0;
            if (type == DocumentIdType.Order)
                dtype = 1;
            if (type == DocumentIdType.HospOrder)
                dtype = 2;
            if (type == DocumentIdType.External)
            {
                NAVWebXml xml = new NAVWebXml();
                string xmlRequest = xml.GetGeneralWebRequestXML("LSC Customer Order Header", "External ID", docId);
                string xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table == null || table.NumberOfValues == 0)
                    return null;

                XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Document ID"));
                docId = field.Values[table.NumberOfValues - 1];
                dtype = 1;
            }

            // GetSelectedSalesDoc(DocumentSourceType: Enum "LSC member Sales Source Type"; DocumentID: Code[20]): Text
            string data = "{ \"documentSourceType\": \"" + dtype.ToString() + "\", " +
                            "\"documentID\": \"" + docId + "\" }";

            string ret = SendToOData("GetSelectedSalesDoc_GetSelectedSalesDoc", data, false);
            OrderJMapping omap = new OrderJMapping(config.IsJson);
            SalesEntry entry = omap.GetSalesEntry(ret);
            if (dtype == 2 && entry == null)
            {
                dtype = 0;  // no pos trans, get normal trans
                data = "{ \"documentSourceType\": \"" + dtype.ToString() + "\", " +
                                "\"documentID\": \"" + docId + "\" }";
                ret = SendToOData("GetSelectedSalesDoc_GetSelectedSalesDoc", data, false);
                entry = omap.GetSalesEntry(ret);
            }

            logger.StatisticEndSub(ref stat, index);
            return entry;
        }

        public SalesEntry TransactionGet(string receiptNo, string storeId, string terminalId, int transId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            TransactionMapping map = new TransactionMapping(LSCVersion, config.IsJson);
            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootGetTransaction root = new LSCentral.RootGetTransaction();
            centralQryWS.GetTransaction(ref respCode, ref errorText, receiptNo, XMLHelper.GetString(storeId), XMLHelper.GetString(terminalId), transId, ref root);
            HandleWS2ResponseCode("GetTransaction", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "GetTransaction Response - " + Serialization.ToXml(root, true));

            SalesEntry entry = map.MapFromRootToRetailTransaction(root);

            Store store = StoreGetById(entry.StoreId, stat);
            entry.StoreName = store.Description;
            if (string.IsNullOrEmpty(entry.StoreCurrency))
                entry.StoreCurrency = store.Currency.Id;

            if (string.IsNullOrEmpty(entry.CustomerOrderNo) == false)
            {
                SalesEntry order = OrderGet(entry.CustomerOrderNo, stat);
                entry.ShipToEmail = order.ShipToEmail;
                entry.ShipToName = order.ShipToName;
                entry.ShipToAddress = order.ShipToAddress;
                entry.ContactAddress = order.ContactAddress;
                entry.PointsRewarded = order.PointsRewarded;
                entry.PointsUsedInOrder = order.PointsUsedInOrder;
                entry.ContactDayTimePhoneNo = order.ContactDayTimePhoneNo;
                entry.ContactEmail = order.ContactEmail;
                entry.ContactName = order.ContactName;
                entry.ExternalId = order.ExternalId;
            }

            foreach (SalesEntryLine line in entry.Lines)
            {
                LoyItem item = ItemGetById(line.ItemId, stat);
                line.ItemDescription = item.Description;

                if (item.VariantsRegistration.Count > 0)
                {
                    VariantRegistration vreg = item.VariantsRegistration.Find(v => v.Id == line.VariantId);
                    if (vreg != null)
                    {
                        line.VariantDescription = vreg.Dimension1;
                        if (string.IsNullOrEmpty(vreg.Dimension2) == false)
                            line.VariantDescription += "/" + vreg.Dimension2;
                        if (string.IsNullOrEmpty(vreg.Dimension3) == false)
                            line.VariantDescription += "/" + vreg.Dimension3;
                        if (string.IsNullOrEmpty(vreg.Dimension4) == false)
                            line.VariantDescription += "/" + vreg.Dimension4;
                        if (string.IsNullOrEmpty(vreg.Dimension5) == false)
                            line.VariantDescription += "/" + vreg.Dimension5;
                        item.Images = ImagesGetByLink("Item Variant", line.ItemId, line.VariantId, string.Empty, false, stat);
                    }
                }
                else
                {
                    item.Images = ImagesGetByLink("Item", line.ItemId, string.Empty, string.Empty, false, stat);
                }

                if (item.Images.Count > 0)
                    line.ItemImageId = item.Images[0].Id;
            }
            logger.StatisticEndSub(ref stat, index);
            return entry;
        }

        #endregion

        #region Store

        public Store StoreGetById(string id, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Store", "No.", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository(config);
            List<Store> list = rep.StoresGet(table);
            if (list.Count == 0)
            {
                logger.StatisticEndSub(ref stat, index);
                return null;
            }

            Store store = list.FirstOrDefault();
            if (store.Currency == null || string.IsNullOrEmpty(store.Currency.Id))
            {
                xmlRequest = xml.GetGeneralWebRequestXML("General Ledger Setup");
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table != null && table.NumberOfValues > 0)
                {
                    XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("LCY Code"));
                    store.Currency = new Currency(field.Values[0]);
                }
            }

            xmlRequest = xml.GetGeneralWebRequestXML("LSC POS Terminal", "No.", store.WebOmniTerminal);
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            table = xml.GetGeneralWebResponseXML(xmlResponse);
            if (table != null && table.NumberOfValues > 0)
            {
                XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Sales Type Filter"));
                string[] st = field.Values[0].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in st)
                {
                    xmlRequest = xml.GetGeneralWebRequestXML("LSC Sales Type", "Code", s);
                    xmlResponse = RunOperation(xmlRequest, true);
                    HandleResponseCode(ref xmlResponse);
                    table = xml.GetGeneralWebResponseXML(xmlResponse);
                    if (table != null && table.NumberOfValues > 0)
                    {
                        field = table.FieldList.Find(f => f.FieldName.Equals("Description"));
                        store.HospSalesTypes.Add(new SalesType()
                        {
                            Code = s,
                            Description = field.Values[0]
                        });
                    }
                }
            }

            logger.StatisticEndSub(ref stat, index);
            return store;
        }

        public List<Store> StoresGet(StoreGetType storeType, bool inclDetails, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest;
            switch (storeType)
            {
                case StoreGetType.ClickAndCollect:
                    xmlRequest = xml.GetGeneralWebRequestXML("LSC Store", "Click and Collect", "1");
                    break;
                case StoreGetType.WebStore:
                    xmlRequest = xml.GetGeneralWebRequestXML("LSC Store", "Web Store", "1");
                    break;
                default:
                    xmlRequest = xml.GetGeneralWebRequestXML("LSC Store");
                    break;
            }

            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository(config);
            List<Store> list = rep.StoresGet(table);
            if (inclDetails == false)
            {
                logger.StatisticEndSub(ref stat, index);
                return list;
            }

            string localCur = string.Empty;
            xmlRequest = xml.GetGeneralWebRequestXML("General Ledger Setup");
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            table = xml.GetGeneralWebResponseXML(xmlResponse);
            if (table != null && table.NumberOfValues > 0)
            {
                XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("LCY Code"));
                localCur = field.Values[0];
            }

            foreach (Store store in list)
            {
                if (store.Currency == null || string.IsNullOrEmpty(store.Currency.Id))
                {
                    store.Currency = new Currency(localCur);
                }

                xmlRequest = xml.GetGeneralWebRequestXML("LSC POS Terminal", "No.", store.WebOmniTerminal);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table != null && table.NumberOfValues > 0)
                {
                    XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Sales Type Filter"));
                    string[] st = field.Values[0].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in st)
                    {
                        xmlRequest = xml.GetGeneralWebRequestXML("LSC Sales Type", "Code", s);
                        xmlResponse = RunOperation(xmlRequest, true);
                        HandleResponseCode(ref xmlResponse);
                        table = xml.GetGeneralWebResponseXML(xmlResponse);
                        if (table != null && table.NumberOfValues > 0)
                        {
                            field = table.FieldList.Find(f => f.FieldName.Equals("Description"));
                            store.HospSalesTypes.Add(new SalesType()
                            {
                                Code = s,
                                Description = field.Values[0]
                            });
                        }
                    }
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<StoreHours> StoreHoursGetByStoreId(string storeId, int offset, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootGetStoreOpeningHours root = new LSCentral.RootGetStoreOpeningHours();

            logger.Debug(config.LSKey.Key, "GetStoreOpeningHours Store: " + storeId);
            centralQryWS.GetStoreOpeningHours(ref respCode, ref errorText, storeId, ref root);
            HandleWS2ResponseCode("GetStoreOpeningHours", respCode, errorText, ref stat, index, new string[] { "1000" });
            logger.Debug(config.LSKey.Key, "GetStoreOpeningHours Response - " + Serialization.ToXml(root, true));

            StoreMapping map = new StoreMapping();
            List<StoreHours> list = map.MapFromRootToOpeningHours(root, offset);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<RetailAttribute> AttributeGet(string value, AttributeLinkType type, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Attribute Value", "Link Field 1", value, "Link Type", ((int)type).ToString());
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository(config);
            List<RetailAttribute> list = rep.GetAttribute(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        #endregion

        #region Device

        public Device DeviceGetById(string id, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Member Device", "ID", id);
            string xmlResponse = RunOperation(xmlRequest);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ContactRepository rep = new ContactRepository(config);
            Device data = rep.DeviceGetById(table);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public Terminal TerminalGetById(string id, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC POS Terminal", "No.", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository(config);
            Terminal ter = rep.GetTerminalData(table);

            if (ter.InventoryMainMenuId == null)
                ter.InventoryMainMenuId = string.Empty;

            logger.StatisticEndSub(ref stat, index);
            return ter;
        }

        public string TerminalGetLicense(string termId, string appId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string licence = string.Empty;
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC MobileLicenseRegistration", "Terminal ID", termId, "App ID", appId);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);
            if (table == null || table.NumberOfValues == 0)
                return licence;

            XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Device License Key"));
            licence = field.Values[0];

            logger.StatisticEndSub(ref stat, index);
            return licence;
        }

        #endregion

        #region ScanPayGo

        public string ScanPayGoSuspend(Order request, out string orderId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            orderId = string.Empty;
            string respCode = string.Empty;
            string errorText = string.Empty;
            
            if (LSCVersion >= new Version("25.0"))
            {
                TransactionMapping25 map = new TransactionMapping25(LSCVersion, config.IsJson);
                LSCentral25.RootMobileTransaction root = map.MapFromOrderToRoot(request);
                root.MobileTransaction[0].TerminalId = config.SettingsGetByKey(ConfigKey.ScanPayGo_Terminal);
                root.MobileTransaction[0].StaffId = config.SettingsGetByKey(ConfigKey.ScanPayGo_Staff);

                logger.Debug(config.LSKey.Key, "MobilePosSuspendV2 Request - " + Serialization.ToXml(root, true));
                centralWS25.MobilePosSuspendV2(ref respCode, ref errorText, root, ref orderId);
                HandleWS2ResponseCode("MobilePosSuspendV2", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "MobilePosSuspendV2 Response - " + Serialization.ToXml(root, true));
            }
            else
            {
                TransactionMapping map = new TransactionMapping(LSCVersion, config.IsJson);
                LSCentral.RootMobileTransaction root = map.MapFromOrderToRoot(request);
                root.MobileTransaction[0].TerminalId = config.SettingsGetByKey(ConfigKey.ScanPayGo_Terminal);
                root.MobileTransaction[0].StaffId = config.SettingsGetByKey(ConfigKey.ScanPayGo_Staff);

                logger.Debug(config.LSKey.Key, "MobilePosSuspendV2 Request - " + Serialization.ToXml(root, true));
                centralWS.MobilePosSuspendV2(ref respCode, ref errorText, root, ref orderId);
                HandleWS2ResponseCode("MobilePosSuspendV2", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "MobilePosSuspendV2 Response - " + Serialization.ToXml(root, true));
            }

            logger.StatisticEndSub(ref stat, index);
            return request.Id;
        }

        public ScanPayGoProfile ScanPayGoProfileGet(string profileId, string storeNo, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootSPGProfileGet rootProfile = new LSCentral.RootSPGProfileGet();
            LSCentral.RootSPGProfileTender rootTender = new LSCentral.RootSPGProfileTender();
            LSCentral.RootSPGProfileFlags rootFlag = new LSCentral.RootSPGProfileFlags();

            logger.Debug(config.LSKey.Key, $"SPGProfileGet Request - Profile:{profileId} store:{storeNo}");
            centralQryWS.SPGProfileGet(XMLHelper.GetString(profileId), XMLHelper.GetString(storeNo), ref rootProfile, ref rootTender, ref rootFlag, ref respCode, ref errorText);
            HandleWS2ResponseCode("SPGProfileGet", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "SPGProfileGet Response - Profile:" + Serialization.ToXml(rootProfile, true) +
                                                                "> Tender:" + Serialization.ToXml(rootTender, true) +
                                                                "> Flags:" + Serialization.ToXml(rootFlag, true));

            ScanPayGoProfile profile = new ScanPayGoProfile();
            profile.Id = profileId;
            profile.Flags = new FeatureFlags();
            if (rootProfile.SPGProfileGet != null && rootProfile.SPGProfileGet.Length == 1)
            {
                profile.SecurityTrigger = (SPGSecurityTrigger)Convert.ToInt32(rootProfile.SPGProfileGet[0].SecurityCheckTrigger);
            }

            if (rootFlag.SPGProfileFeatureFlags != null)
            {
                foreach (LSCentral.SPGProfileFeatureFlags flag in rootFlag.SPGProfileFeatureFlags)
                {
                    profile.Flags.AddFlag(flag.FeatureId, string.Join("", flag.FeatureValue));
                }
            }

            if (rootTender.SPGProfileTender != null)
            {
                profile.TenderTypes = new List<ScanPayGoTender>();
                foreach (LSCentral.SPGProfileTender ten in rootTender.SPGProfileTender)
                {
                    profile.TenderTypes.Add(new ScanPayGoTender()
                    {
                        Id = ten.TenderType,
                        Description = ten.Description
                    });
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return profile;
        }

        public OrderCheck ScanPayGoOrderCheck(string documentId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            bool orderPayed = false;
            bool doCheck = false;
            int numOfItem = 0;

            if (LSCVersion >= new Version("20.5"))
            {
                logger.Debug(config.LSKey.Key, $"SPGOrderCheckV2 Request - docId:{documentId}");
                LSCentral.RootSPGOrderCheck root2 = new LSCentral.RootSPGOrderCheck();
                centralWS.SPGOrderCheckV2(documentId, ref orderPayed, ref doCheck, ref numOfItem, ref root2, ref respCode, ref errorText);
                HandleWS2ResponseCode("SPGOrderCheckV2", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "SPGOrderCheckV2 Response - " + Serialization.ToXml(root2, true));
                OrderMapping map = new OrderMapping(LSCVersion, config.IsJson);
                OrderCheck order = map.RootToOrderCheck(root2);
                order.DoCheck = doCheck;
                order.OrderPayed = orderPayed;
                order.NumberOfItemsToCheck = numOfItem;
                logger.StatisticEndSub(ref stat, index);
                return order;
            }
            else
            {
                logger.Debug(config.LSKey.Key, $"SPGOrderCheck Request - docId:{documentId}");
                LSCentral.RootSPGOrderCheck1 root = new LSCentral.RootSPGOrderCheck1();
                centralWS.SPGOrderCheck(documentId, ref orderPayed, ref doCheck, ref numOfItem, ref root, ref respCode, ref errorText);
                HandleWS2ResponseCode("SPGOrderCheck", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, "SPGOrderCheck Response - " + Serialization.ToXml(root, true));

                List<OrderCheckLines> lines = new List<OrderCheckLines>();
                if (root.SPGOrderCheckCOLine != null)
                {
                    foreach (LSCentral.SPGOrderCheckCOLine1 line in root.SPGOrderCheckCOLine)
                    {
                        lines.Add(new OrderCheckLines()
                        {
                            DocumentID = line.DocumentID,
                            ItemId = line.Number,
                            ItemDescription = line.ItemDescription,
                            VariantCode = line.VariantCode,
                            VariantDescription = line.VariantDescription,
                            UnitofMeasureCode = line.UnitofMeasureCode,
                            UOMDescription = line.UoMDescription,
                            Amount = line.Amount,
                            LineNo = line.LineNo,
                            Quantity = line.Quantity
                        });
                    }
                }
                OrderCheck orderCheck = new OrderCheck()
                {
                    OrderPayed = orderPayed,
                    DoCheck = doCheck,
                    NumberOfItemsToCheck = numOfItem,
                    Lines = lines
                };
                logger.StatisticEndSub(ref stat, index);
                return orderCheck;
            }
        }

        public bool SecurityCheckProfile(string orderNo, string storeNo, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            bool exist = false;
            logger.Debug(config.LSKey.Key, $"SecurityCheckProfile Request - OrderNo:{orderNo} storeNo:{storeNo}");
            centralWS.SecurityCheckProfile(ref respCode, ref errorText, ref exist, XMLHelper.GetString(storeNo), XMLHelper.GetString(orderNo));
            HandleWS2ResponseCode("SecurityCheckProfile", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, $"SecurityCheckProfile Response - ProfileExist:{exist}");
            logger.StatisticEndSub(ref stat, index);
            return exist;
        }

        public bool SecurityCheckLogResponse(string orderNo, string validationError, bool validationSuccessful, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            logger.Debug(config.LSKey.Key, $"SPGLogSecurityCheckResponse Request - OrderNo:{orderNo} validationError:{validationError} validationSuccessful:{validationSuccessful}");
            centralWS.SPGLogSecurityCheckResponse(ref orderNo, ref validationSuccessful, XMLHelper.GetString(validationError), ref respCode, ref errorText);
            HandleWS2ResponseCode("SPGLogSecurityCheckResponse", respCode, errorText, ref stat, index);
            logger.StatisticEndSub(ref stat, index);
            return true;
        }

        public ScanPayGoSecurityLog SecurityCheckLog(string orderNo, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC SPG Security Check Log", "Customer Order ID", orderNo);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);
            ScanPayGoSecurityLog log = new ScanPayGoSecurityLog();
            if (table == null || table.NumberOfValues == 0)
                return log;

            XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Source"));
            log.Source = ConvertTo.SafeBoolean(field.Values[table.NumberOfValues - 1]);
            field = table.FieldList.Find(f => f.FieldName.Equals("Check"));
            log.Check = ConvertTo.SafeBoolean(field.Values[table.NumberOfValues - 1]);
            logger.StatisticEndSub(ref stat, index);
            return log;
        }

        public string OpenGate(string qrCode, string storeNo, string devLocation, string memberAccount, bool exitWithoutShopping, bool isEntering, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            bool retValue = false;
            string errorText = string.Empty;

            logger.Debug(config.LSKey.Key, $"SecurityCheckProfile Request - QRCode:{qrCode} store:{storeNo} devLoc:{devLocation} memAccount:{memberAccount} exitWoutShop:{exitWithoutShopping} isEntering:{isEntering}");
            centralWS.SPGOpenGate(qrCode, ref storeNo, ref devLocation, ref memberAccount, ref exitWithoutShopping, ref isEntering, ref retValue, ref respCode, ref errorText);
            logger.Debug(config.LSKey.Key, $"SPGOpenGate Response -  resCode:{respCode} errMsg:{errorText}");
            logger.StatisticEndSub(ref stat, index);
            if (respCode != "0000")
                return errorText;

            return string.Empty;
        }

        public bool TokenEntrySet(ClientToken token, bool deleteToken, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            bool retValue = true;

            if (deleteToken)
            {
                logger.Debug(config.LSKey.Key, $"DeleteMemberCardToken Request - token:{Serialization.ToXml(token, true)}");
                centralWS.DeleteMemberCardToken(ref respCode, ref errorText, token.CardNo, token.Token);
                HandleWS2ResponseCode("DeleteMemberCardToken", respCode, errorText, ref stat, index);
            }
            else
            {
                List<LSCentral.Token1> tokens = new List<LSCentral.Token1>()
                {
                    new LSCentral.Token1()
                    {
                        AccountNo = XMLHelper.GetString(token.AccountNo),
                        CardMask = XMLHelper.GetString(token.CardMask),
                        ContactNo = XMLHelper.GetString(token.ContactNo),
                        Created = token.Created,
                        ExpiryDate = token.ExpiryDate,
                        DefaultToken = token.DefaultToken,
                        EntryNo = token.EntryNo,
                        PSPID = XMLHelper.GetString(token.pSPID),
                        Token = XMLHelper.GetString(token.Token),
                        TokenId = XMLHelper.GetString(token.TokenId),
                        TokenIDExternal = XMLHelper.GetString(token.TokenIDExternal),
                        TokenType = XMLHelper.GetString(token.Type),
                        Initiator = XMLHelper.GetString(token.Initiator),
                        InitiatorReason = XMLHelper.GetString(token.InitiatorReason)
                    }
                };

                LSCentral.RootSetTokenEntry entry = new LSCentral.RootSetTokenEntry();
                entry.Token = tokens.ToArray();

                if (token.HotelToken)
                {
                    logger.Debug(config.LSKey.Key, $"SetTokenEntry Request - token:{Serialization.ToXml(token, true)}");
                    centralWS.SetTokenEntry(token.ContactNo, token.CardNo, entry, ref retValue, ref respCode, ref errorText);
                    HandleWS2ResponseCode("SetTokenEntry", respCode, errorText, ref stat, index);
                }
                else
                {
                    logger.Debug(config.LSKey.Key, $"SetMemberCardToken Request - token:{Serialization.ToXml(token, true)}");
                    centralWS.SetMemberCardToken(token.CardNo, entry, ref respCode, ref errorText);
                    HandleWS2ResponseCode("SetMemberCardToken", respCode, errorText, ref stat, index);
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return retValue;
        }

        public List<ClientToken> TokenEntryGet(string accountNo, bool hotelToken, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            List<ClientToken> tokens = new List<ClientToken>();

            if (hotelToken)
            {
                string result = string.Empty;
                string token = string.Empty;
                DateTime exp = DateTime.MinValue;

                logger.Debug(config.LSKey.Key, $"GetTokenEntry Request - accountNo:{accountNo}");
                centralQryWS.GetTokenEntry(string.Empty, accountNo, ref token, ref exp, ref result, ref respCode, ref errorText);
                HandleWS2ResponseCode("GetTokenEntry", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, $"GetTokenEntry Response - token:{token} ExpD:{exp} Result:{result}");
                tokens.Add(new ClientToken()
                {
                    HotelToken = true,
                    Token = token,
                    ExpiryDate = exp,
                    Result = result
                });
            }
            else
            {
                LSCentral.RootGetTokenEntryXML root = new LSCentral.RootGetTokenEntryXML();
                logger.Debug(config.LSKey.Key, $"GetMemberCardToken Request - accountNo:{accountNo}");
                centralQryWS.GetMemberCardToken(accountNo, ref root, ref respCode, ref errorText);
                HandleWS2ResponseCode("GetMemberCardToken", respCode, errorText, ref stat, index);
                logger.Debug(config.LSKey.Key, $"GetMemberCardToken Response: {Serialization.ToXml(root, true)}");
                if (root.Token != null)
                {
                    foreach (LSCentral.Token token in root.Token)
                    {
                        tokens.Add(new ClientToken()
                        {
                            Token = token.TokenValue,
                            TokenId = token.TokenId,
                            TokenIDExternal = token.TokenIDExternal,
                            Type = token.TokenType,
                            AccountNo = token.AccountNo,
                            pSPID = token.PSPID,
                            CardMask = token.CardMask,
                            Created = token.CreatedDateTime,
                            ExpiryDate = token.ExpiryDate,
                            DefaultToken = token.DefaultToken,
                            Initiator = token.Initiator,
                            InitiatorReason = token.InitiatorReason
                        });
                    }
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return tokens;
        }

        #endregion

        #region Misc functions

        public Currency CurrencyGetById(string id, string culture, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Currency", "Code", id);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository(config);
            Currency data = rep.GetCurrency(table, culture);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public List<ShippingAgentService> GetShippingAgentService(string agentId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Shipping Agent Services", "Shipping Agent Code", agentId);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            SetupRepository rep = new SetupRepository(config);
            List<ShippingAgentService> list = rep.GetShippingAgentServices(table);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<Hierarchy> HierarchyGet(string storeId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            List<Hierarchy> list = new List<Hierarchy>();
            List<HierarchyNode> nodes = new List<HierarchyNode>();
            string respCode = string.Empty;
            string errorText = string.Empty;
            LSCentral.RootGetHierarchyValSched rootRoot = new LSCentral.RootGetHierarchyValSched();
            logger.Debug(config.LSKey.Key, "GetHierarchyV2ValidationSchedule - StoreId: {0}", storeId);
            centralQryWS.GetHierarchyV2ValidationSchedule(ref respCode, ref errorText, storeId, ref rootRoot);
            HandleWS2ResponseCode("GetHierarchyV2ValidationSchedule", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "GetHierarchyV2ValidationSchedule Response - " + Serialization.ToXml(rootRoot, true));

            if (rootRoot.HierarchyValSched == null || rootRoot.HierarchyDateValSched == null)
                throw new LSOmniServiceException(StatusCode.NoEntriesFound, "No Hierarchy found");

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
                HandleWS2ResponseCode("GetHierarchyNode", respCode, errorText, ref stat, index);
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
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<ReplValidationSchedule> ValidationScheduleGet(string storeId)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Hierar. Date", "Store Code", storeId);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);
            if (table == null || table.NumberOfValues == 0)
                return null;

            List<ReplValidationSchedule> list = new List<ReplValidationSchedule>();
            for (int i = 0; i < table.NumberOfValues; i++)
            {
                XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Validation Schedule ID"));

                ReplValidationSchedule sch = new ReplValidationSchedule();
                sch.Id = field.Values[i];

                xmlRequest = xml.GetGeneralWebRequestXML("LSC Validation Schedule", "ID", sch.Id);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table == null || table.NumberOfValues == 0)
                    return null;

                field = table.FieldList.Find(f => f.FieldName.Equals("Description"));
                sch.Description = field.Values[0];

                xmlRequest = xml.GetGeneralWebRequestXML("LSC Validation Schedule Line", "Validation Schedule ID", sch.Id);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table == null || table.NumberOfValues == 0)
                    return null;

                sch.Lines = new List<ValidationScheduleLine>();
                for (int x = 0; x < table.NumberOfValues; x++)
                {
                    ValidationScheduleLine line = new ValidationScheduleLine();
                    string dateid = string.Empty;
                    string timeid = string.Empty;
                    foreach (XMLFieldData fld in table.FieldList)
                    {
                        switch (fld.FieldName)
                        {
                            case "Line No.": line.LineNo = ConvertTo.SafeInt(fld.Values[x]); break;
                            case "Description": line.Description = fld.Values[x]; break;
                            case "Comment": line.Comment = fld.Values[x]; break;
                            case "Priority": line.Priority = ConvertTo.SafeInt(fld.Values[x]); break;
                            case "Date Schedule ID": dateid = fld.Values[x]; break;
                            case "Time Schedule ID": timeid = fld.Values[x]; break;
                        }
                    }

                    xmlRequest = xml.GetGeneralWebRequestXML("LSC Date Schedule", "ID", dateid);
                    xmlResponse = RunOperation(xmlRequest, true);
                    HandleResponseCode(ref xmlResponse);
                    XMLTableData table2 = xml.GetGeneralWebResponseXML(xmlResponse);
                    if (table2 != null && table2.NumberOfValues > 0)
                    {
                        line.DateSchedule = new VSDateSchedule();
                        line.DateSchedule.Id = dateid;
                        foreach (XMLFieldData fld in table2.FieldList)
                        {
                            switch (fld.FieldName)
                            {
                                case "Description": line.DateSchedule.Description = fld.Values[0]; break;
                                case "Mondays": line.DateSchedule.Mondays = ConvertTo.SafeBoolean(fld.Values[0]); break;
                                case "Tuesdays": line.DateSchedule.Tuesdays = ConvertTo.SafeBoolean(fld.Values[0]); break;
                                case "Wednesdays": line.DateSchedule.Wednesdays = ConvertTo.SafeBoolean(fld.Values[0]); break;
                                case "Thursdays": line.DateSchedule.Thursdays = ConvertTo.SafeBoolean(fld.Values[0]); break;
                                case "Fridays": line.DateSchedule.Fridays = ConvertTo.SafeBoolean(fld.Values[0]); break;
                                case "Saturdays": line.DateSchedule.Saturdays = ConvertTo.SafeBoolean(fld.Values[0]); break;
                                case "Sundays": line.DateSchedule.Sundays = ConvertTo.SafeBoolean(fld.Values[0]); break;
                                case "Valid All Weekdays": line.DateSchedule.ValidAllWeekdays = ConvertTo.SafeBoolean(fld.Values[0]); break;
                            }
                        }

                        xmlRequest = xml.GetGeneralWebRequestXML("LSC Date Schedule Line", "Date Schedule ID", dateid);
                        xmlResponse = RunOperation(xmlRequest, true);
                        HandleResponseCode(ref xmlResponse);
                        table2 = xml.GetGeneralWebResponseXML(xmlResponse);
                        if (table2 != null && table2.NumberOfValues > 0)
                        {
                            line.DateSchedule.Lines = new List<VSDateScheduleLine>();
                            for (int y = 0; y < table2.NumberOfValues; y++)
                            {
                                VSDateScheduleLine dline = new VSDateScheduleLine();
                                foreach (XMLFieldData fld in table2.FieldList)
                                {
                                    switch (fld.FieldName)
                                    {
                                        case "Line No.": dline.LineNo = ConvertTo.SafeInt(fld.Values[y]); break;
                                        case "Starting Date": dline.StartingDate = ConvertTo.SafeJsonDate(XMLHelper.GetWebDateTime(fld.Values[y]), config.IsJson); break;
                                        case "Ending Date": dline.StartingDate = ConvertTo.SafeJsonDate(XMLHelper.GetWebDateTime(fld.Values[y]), config.IsJson); break;
                                        case "Exclude": dline.Exclude = ConvertTo.SafeBoolean(fld.Values[y]); break;
                                    }
                                }
                                line.DateSchedule.Lines.Add(dline);
                            }
                        }
                    }

                    xmlRequest = xml.GetGeneralWebRequestXML("LSC Time Schedule", "ID", timeid);
                    xmlResponse = RunOperation(xmlRequest, true);
                    HandleResponseCode(ref xmlResponse);
                    table2 = xml.GetGeneralWebResponseXML(xmlResponse);
                    if (table2 != null && table2.NumberOfValues > 0)
                    {
                        line.TimeSchedule = new VSTimeSchedule();
                        line.TimeSchedule.Id = timeid;
                        foreach (XMLFieldData fld in table2.FieldList)
                        {
                            switch (fld.FieldName)
                            {
                                case "Description": line.TimeSchedule.Description = fld.Values[0]; break;
                                case "Schedule Type": line.TimeSchedule.Type = (VSTimeScheduleType)ConvertTo.SafeInt(fld.Values[0]); break;
                            }
                        }

                        xmlRequest = xml.GetGeneralWebRequestXML("LSC Time Schedule Line", "Time Schedule ID", timeid);
                        xmlResponse = RunOperation(xmlRequest, true);
                        HandleResponseCode(ref xmlResponse);
                        table2 = xml.GetGeneralWebResponseXML(xmlResponse);
                        if (table2 != null && table2.NumberOfValues > 0)
                        {
                            line.TimeSchedule.Lines = new List<VSTimeScheduleLine>();
                            for (int y = 0; y < table2.NumberOfValues; y++)
                            {
                                VSTimeScheduleLine tline = new VSTimeScheduleLine(config.IsJson);
                                foreach (XMLFieldData fld in table2.FieldList)
                                {
                                    switch (fld.FieldName)
                                    {
                                        case "Period": tline.Period = fld.Values[y]; break;
                                        case "Time From": tline.TimeFrom = ConvertTo.SafeJsonTime(XMLHelper.GetWebDateTime(fld.Values[y]), config.IsJson); break;
                                        case "Time To": tline.TimeTo = ConvertTo.SafeJsonTime(XMLHelper.GetWebDateTime(fld.Values[y]), config.IsJson); break;
                                        case "Time To Is Past Midnight": tline.TimeToIsPastMidnight = ConvertTo.SafeBoolean(fld.Values[y]); break;
                                        case "Dining Duration Code": tline.DiningDurationCode = fld.Values[y]; break;
                                        case "Selected by Default": tline.SelectedByDefault = ConvertTo.SafeBoolean(fld.Values[y]); break;
                                        case "Reservation Interval (Min.)": tline.ReservationInterval = ConvertTo.SafeInt(fld.Values[y]); break;
                                    }
                                }
                                line.TimeSchedule.Lines.Add(tline);
                            }
                        }
                    }
                    sch.Lines.Add(line);
                }
                list.Add(sch);
            }
            return list;
        }

        public List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;

            StoreMapping map = new StoreMapping();
            LSCentral.RootGetReturnPolicy root = new LSCentral.RootGetReturnPolicy();

            logger.Debug(config.LSKey.Key, "GetReturnPolicy - storeId:{0}, storeGroup:{1} itemCat:{2} prodGroup:{3}", storeId, storeGroupCode, itemCategory, productGroup);
            centralQryWS.GetReturnPolicy(ref respCode, ref errorText, XMLHelper.GetString(storeId), XMLHelper.GetString(storeGroupCode), XMLHelper.GetString(itemCategory), XMLHelper.GetString(productGroup), XMLHelper.GetString(itemId), XMLHelper.GetString(variantCode), XMLHelper.GetString(variantDim1), ref root);
            HandleWS2ResponseCode("GetReturnPolicy", respCode, errorText, ref stat, index, new string[] { "1000" });
            logger.Debug(config.LSKey.Key, "GetReturnPolicy Response - " + Serialization.ToXml(root, true));
            List<ReturnPolicy> list = map.MapFromRootToReturnPolicy(root);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public MobileMenu MenuGet(string storeId, string salesType, Currency currency)
        {
            logger.Warn(config.LSKey.Key, "Not supported by LS Central version for SaaS");
            return new MobileMenu();
        }

        #endregion
    }
}
