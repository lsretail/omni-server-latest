using System;
using System.Collections.Generic;
using System.Web.Services.Protocols;

using LSOmni.DataAccess.Interface.BOConnection;
using LSOmni.DataAccess.BOConnection.NavSQL.Dal;
using LSOmni.DataAccess.BOConnection.NavSQL.XmlMapping.Loyalty;

using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Requests;

namespace LSOmni.DataAccess.BOConnection.NavSQL
{
    
    //Navision back office connection
    public class NavApps : NavBase, IAppBO
    {
        private static readonly object Locker = new object();

        public NavApps() : base()
        {
        }

        public virtual List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds,
            string loyaltySchemeCode)
        {
            DiscountOfferRepository rep = new DiscountOfferRepository();
            return rep.DiscountsGet(storeId, itemIds, loyaltySchemeCode);
        }

        public virtual Terminal TerminalGetById(string terminalId)
        {
            TerminalRepository rep = new TerminalRepository();
            return rep.TerminalBaseGetById(terminalId);
        }

        public virtual UnitOfMeasure UnitOfMeasureGetById(string id)
        {
            UnitOfMeasureRepository rep = new UnitOfMeasureRepository();
            return rep.UnitOfMeasureGetById(id);
        }

        public virtual VariantRegistration VariantRegGetById(string id, string itemId)
        {
            ItemVariantRegistrationRepository rep = new ItemVariantRegistrationRepository();
            return rep.VariantRegGetById(id, itemId);
        }

        public virtual Currency CurrencyGetById(string id, string culture)
        {
            CurrencyRepository rep = new CurrencyRepository(NAVVersion);
            return rep.CurrencyLoyGetById(id, culture);
        }

        public virtual string ItemDetailsGetById(string itemId)
        {
            ItemRepository itemRep = new ItemRepository();
            return itemRep.ItemDetailsGetById(itemId);
        }

        public virtual List<InventoryResponse> ItemInStockGet(string itemId, string variantId, int arrivingInStockInDays, List<string> locationIds, bool skipUnAvailableStores)
        {
            if (locationIds.Count == 0)
            {
                //JIJ add the distance calc for them, radius of X km ?
                int recordsRemaining = 0;
                string lastKey = "", maxKey = "";
                StoreRepository rep = new StoreRepository(NAVVersion);
                List<ReplStore> totalList = rep.ReplicateStores(100, true, ref lastKey, ref maxKey, ref recordsRemaining);

                //get the storeIds close to this store
                foreach (ReplStore store in totalList)
                    locationIds.Add(store.Id);
            }

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
                    if (navStore.QtyActualInventory > 0)   //actual inventory are positive when item exist in inventory, otherwise negative if not exist
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
                try
                {
                    navWS.GetItemInventory(ref respCode, ref errorText, itemId, variantId, id, string.Empty, string.Empty, string.Empty, string.Empty, arrivingInStockInDays, ref root);
                    if (respCode != "0000")
                        throw new LSOmniServiceException(StatusCode.NoEntriesFound, errorText);
                }
                catch (SoapException e)
                {
                    if (e.Message.Contains("Method"))
                        throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
                }

                if (root.WSInventoryBuffer == null)
                    continue;

                foreach (NavWS.WSInventoryBuffer buffer in root.WSInventoryBuffer)
                {
                    if (skipUnAvailableStores && buffer.ActualInventory <= 0)
                        continue;

                    list.Add(new InventoryResponse()
                    {
                        ItemId = buffer.ItemNo,
                        VariantId = buffer.VariantCode,
                        BaseUnitOfMeasure = buffer.BaseUnitofMeasure,
                        QtyActualInventory = buffer.ActualInventory,
                        QtyInventory = buffer.Inventory,
                        QtySoldNotPosted = buffer.QtySoldnotPosted,
                        QtyExpectedStock = buffer.ExpectedStock,
                        ReorderPoint = buffer.ReorderPoint,
                        StoreId = buffer.StoreNo
                    });
                }
            }
            return list;
        }

        public virtual List<InventoryResponse> ItemsInStockGet(List<InventoryRequest> items, string storeId, string locationId)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;

            List<NavWS.InventoryBufferIn> lines = new List<NavWS.InventoryBufferIn>();
            foreach (InventoryRequest item in items)
            {
                lines.Add(new NavWS.InventoryBufferIn()
                {
                    Number = item.ItemId,
                    Variant = item.VariantId
                });
            }

            NavWS.RootGetInventoryMultipleIn rootin = new NavWS.RootGetInventoryMultipleIn();
            rootin.InventoryBufferIn = lines.ToArray();

            NavWS.RootGetInventoryMultipleOut rootout = new NavWS.RootGetInventoryMultipleOut();

            List<InventoryResponse> list = new List<InventoryResponse>();

            try
            {
                navWS.GetInventoryMultiple(ref respCode, ref errorText, storeId, locationId, rootin, ref rootout);
                if (respCode != "0000")
                    throw new LSOmniServiceException(StatusCode.NoEntriesFound, errorText);
            }
            catch (SoapException e)
            {
                if (e.Message.Contains("Method"))
                    throw new LSOmniServiceException(StatusCode.NAVWebFunctionNotFound, "Set WS2 to false in Omni Config", e);
            }

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

        #region replication

        public virtual List<ReplItem> ReplicateItems(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemRepository rep = new ItemRepository();
            return rep.ReplicateItems(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplBarcode> ReplicateBarcodes(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            BarcodeRepository rep = new BarcodeRepository();
            return rep.ReplicateBarcodes(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplBarcodeMask> ReplicateBarcodeMasks(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            BarcodeMaskRepository rep = new BarcodeMaskRepository();
            return rep.ReplicateBarcodeMasks(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplBarcodeMaskSegment> ReplicateBarcodeMaskSegments(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            BarcodeMaskSegmentRepository rep = new BarcodeMaskSegmentRepository();
            return rep.ReplicateBarcodeMaskSegments(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplExtendedVariantValue> ReplicateExtendedVariantValues(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ExtendedVariantValuesRepository rep = new ExtendedVariantValuesRepository();
            return rep.ReplicateExtendedVariantValues(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplItemUnitOfMeasure> ReplicateItemUOM(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemUOMRepository rep = new ItemUOMRepository();
            return rep.ReplicateItemUOM(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplItemVariantRegistration> ReplicateItemVariantRegistration(string storeId, int batchSize, bool fullReplication,ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemVariantRegistrationRepository rep = new ItemVariantRegistrationRepository();
            return rep.ReplicateItemVariantRegistration(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplStaff> ReplicateStaff(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            StaffRepository rep = new StaffRepository();
            return rep.ReplicateStaff(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplVendor> ReplicateVendors(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            VendorRepository rep = new VendorRepository();
            return rep.ReplicateVendor(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCurrency> ReplicateCurrency(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            CurrencyRepository rep = new CurrencyRepository(NAVVersion);
            return rep.ReplicateCurrency(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCurrencyExchRate> ReplicateCurrencyExchRate(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            CurrencyExchRateRepository rep = new CurrencyExchRateRepository();
            return rep.ReplicateCurrencyExchRate(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCustomer> ReplicateCustomer(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            CustomerRepository rep = new CustomerRepository();
            return rep.ReplicateCustomer(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplItemCategory> ReplicateItemCategory(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository();
            return rep.ReplicateItemCategory(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplPrice> ReplicatePrice(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            PriceRepository rep = new PriceRepository();
            return rep.ReplicatePrice(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplPrice> ReplicateBasePrice(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            PriceRepository rep = new PriceRepository();
            return rep.ReplicateAllPrice(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplProductGroup> ReplicateProductGroups(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ProductGroupRepository rep = new ProductGroupRepository();
            return rep.ReplicateProductGroups(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplStore> ReplicateStores(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            StoreRepository rep = new StoreRepository(NAVVersion);
            return rep.ReplicateStores(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplStore> ReplicateStores(string storeId, string terminalId)
        {
            StoreRepository rep = new StoreRepository(NAVVersion);
            return rep.ReplicateInvStores(storeId, terminalId);
        }

        public virtual List<ReplUnitOfMeasure> ReplicateUnitOfMeasure(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            UnitOfMeasureRepository rep = new UnitOfMeasureRepository();
            return rep.ReplicateUnitOfMeasure(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscount> ReplicateDiscounts(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            DiscountOfferRepository rep = new DiscountOfferRepository();
            return rep.ReplicateDiscounts(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscount> ReplicateMixAndMatch(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            DiscountOfferRepository rep = new DiscountOfferRepository();
            return rep.ReplicateMixAndMatch(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscountValidation> ReplicateDiscountValidations(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            DiscountOfferRepository rep = new DiscountOfferRepository();
            return rep.ReplicateDiscountValidations(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplStoreTenderType> ReplicateStoreTenderType(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            StoreTenderTypeRepository rep = new StoreTenderTypeRepository();
            return rep.ReplicateStoreTenderType(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchy> ReplicateHierarchy(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error("Only supported in NAV 10.x and later");
                return new List<ReplHierarchy>();
            }

            HierarchyRepository rep = new HierarchyRepository(NAVVersion);
            return rep.ReplicateHierarchy(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyNode> ReplicateHierarchyNode(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error("Only supported in NAV 10.x and later");
                return new List<ReplHierarchyNode>();
            }

            HierarchyNodeRepository rep = new HierarchyNodeRepository(NAVVersion);
            return rep.ReplicateHierarchyNode(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyLeaf> ReplicateHierarchyLeaf(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error("Only supported in NAV 10.x and later");
                return new List<ReplHierarchyLeaf>();
            }

            HierarchyNodeRepository rep = new HierarchyNodeRepository(NAVVersion);
            return rep.ReplicateHierarchyLeaf(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        #endregion
    }
}
 