using System;
using System.Collections.Generic;

using LSOmni.DataAccess.Interface.BOConnection;
using LSOmni.DataAccess.BOConnection.NavSQL.Dal;

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
        public NavApps(BOConfiguration config) : base(config)
        {
        }

        public virtual List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string loyaltySchemeCode)
        {
            DiscountOfferRepository rep = new DiscountOfferRepository(config);
            return rep.DiscountsGet(storeId, itemIds, loyaltySchemeCode);
        }

        public virtual Terminal TerminalGetById(string terminalId)
        {
            TerminalRepository rep = new TerminalRepository(config, NAVVersion);
            return rep.TerminalBaseGetById(terminalId);
        }

        public virtual UnitOfMeasure UnitOfMeasureGetById(string id)
        {
            UnitOfMeasureRepository rep = new UnitOfMeasureRepository(config);
            return rep.UnitOfMeasureGetById(id);
        }

        public virtual VariantRegistration VariantRegGetById(string id, string itemId)
        {
            ItemVariantRegistrationRepository rep = new ItemVariantRegistrationRepository(config);
            return rep.VariantRegGetById(id, itemId);
        }

        public virtual Currency CurrencyGetById(string id, string culture)
        {
            CurrencyRepository rep = new CurrencyRepository(config, NAVVersion);
            return rep.CurrencyLoyGetById(id, culture);
        }

        public virtual List<InventoryResponse> ItemInStockGet(string itemId, string variantId, int arrivingInStockInDays, List<string> locationIds, bool skipUnAvailableStores)
        {
            if (locationIds.Count == 0)
            {
                //JIJ add the distance calc for them, radius of X km ?
                int recordsRemaining = 0;
                string lastKey = "", maxKey = "";
                StoreRepository rep = new StoreRepository(config, NAVVersion);
                List<ReplStore> totalList = rep.ReplicateStores(100, true, ref lastKey, ref maxKey, ref recordsRemaining);

                //get the storeIds close to this store
                foreach (ReplStore store in totalList)
                    locationIds.Add(store.Id);
            }

            return NavWSBase.ItemInStockGet(itemId, variantId, arrivingInStockInDays, locationIds, skipUnAvailableStores);
        }

        public virtual List<InventoryResponse> ItemsInStockGet(List<InventoryRequest> items, string storeId, string locationId)
        {
            return NavWSBase.ItemsInStockGet(items, storeId, locationId);
        }

        public virtual string ItemDetailsGetById(string itemId)
        {
            ItemRepository itemRep = new ItemRepository(config, NAVVersion);
            return itemRep.ItemDetailsGetById(itemId);
        }

        #region replication

        public virtual List<ReplItem> ReplicateItems(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemRepository rep = new ItemRepository(config, NAVVersion);
            return rep.ReplicateItems(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplBarcode> ReplicateBarcodes(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            BarcodeRepository rep = new BarcodeRepository(config);
            return rep.ReplicateBarcodes(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplBarcodeMask> ReplicateBarcodeMasks(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            BarcodeMaskRepository rep = new BarcodeMaskRepository(config);
            return rep.ReplicateBarcodeMasks(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplBarcodeMaskSegment> ReplicateBarcodeMaskSegments(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            BarcodeMaskSegmentRepository rep = new BarcodeMaskSegmentRepository(config);
            return rep.ReplicateBarcodeMaskSegments(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplExtendedVariantValue> ReplicateExtendedVariantValues(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ExtendedVariantValuesRepository rep = new ExtendedVariantValuesRepository(config, NAVVersion);
            return rep.ReplicateExtendedVariantValues(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplItemUnitOfMeasure> ReplicateItemUOM(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemUOMRepository rep = new ItemUOMRepository(config);
            return rep.ReplicateItemUOM(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplItemVariantRegistration> ReplicateItemVariantRegistration(string appId, string appType, string storeId, int batchSize, bool fullReplication,ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemVariantRegistrationRepository rep = new ItemVariantRegistrationRepository(config);
            return rep.ReplicateItemVariantRegistration(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplStaff> ReplicateStaff(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            StaffRepository rep = new StaffRepository(config);
            return rep.ReplicateStaff(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplVendor> ReplicateVendors(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            VendorRepository rep = new VendorRepository(config);
            return rep.ReplicateVendor(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCurrency> ReplicateCurrency(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            CurrencyRepository rep = new CurrencyRepository(config, NAVVersion);
            return rep.ReplicateCurrency(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCurrencyExchRate> ReplicateCurrencyExchRate(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            CurrencyExchRateRepository rep = new CurrencyExchRateRepository(config);
            return rep.ReplicateCurrencyExchRate(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplTaxSetup> ReplicateTaxSetup(string appId, string appType, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            TaxSetupRepository rep = new TaxSetupRepository(config);
            return rep.ReplicateTaxSetup(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCustomer> ReplicateCustomer(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            CustomerRepository rep = new CustomerRepository(config);
            return rep.ReplicateCustomer(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplItemCategory> ReplicateItemCategory(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemCategoryRepository rep = new ItemCategoryRepository(config, NAVVersion);
            return rep.ReplicateItemCategory(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplItemLocation> ReplicateItemLocation(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemLocationRepository rep = new ItemLocationRepository(config);
            return rep.ReplicateItemLocation(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplPrice> ReplicatePrice(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            PriceRepository rep = new PriceRepository(config);
            return rep.ReplicatePrice(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplPrice> ReplicateBasePrice(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            PriceRepository rep = new PriceRepository(config);
            return rep.ReplicateAllPrice(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplProductGroup> ReplicateProductGroups(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ProductGroupRepository rep = new ProductGroupRepository(config, NAVVersion);
            return rep.ReplicateProductGroups(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplStore> ReplicateStores(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            StoreRepository rep = new StoreRepository(config, NAVVersion);
            return rep.ReplicateStores(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplStore> ReplicateStores(string appId, string appType, string storeId, string terminalId)
        {
            StoreRepository rep = new StoreRepository(config, NAVVersion);
            return rep.ReplicateInvStores(storeId, terminalId);
        }

        public virtual List<ReplUnitOfMeasure> ReplicateUnitOfMeasure(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            UnitOfMeasureRepository rep = new UnitOfMeasureRepository(config);
            return rep.ReplicateUnitOfMeasure(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscount> ReplicateDiscounts(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            DiscountOfferRepository rep = new DiscountOfferRepository(config);
            return rep.ReplicateDiscounts(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscount> ReplicateMixAndMatch(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            DiscountOfferRepository rep = new DiscountOfferRepository(config);
            return rep.ReplicateMixAndMatch(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscountValidation> ReplicateDiscountValidations(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            DiscountOfferRepository rep = new DiscountOfferRepository(config);
            return rep.ReplicateDiscountValidations(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplStoreTenderType> ReplicateStoreTenderType(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            StoreTenderTypeRepository rep = new StoreTenderTypeRepository(config);
            return rep.ReplicateStoreTenderType(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchy> ReplicateHierarchy(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error(config.LSKey.Key, "Only supported in NAV 10.x and later");
                return new List<ReplHierarchy>();
            }

            HierarchyRepository rep = new HierarchyRepository(config, NAVVersion);
            return rep.ReplicateHierarchy(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyNode> ReplicateHierarchyNode(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error(config.LSKey.Key, "Only supported in NAV 10.x and later");
                return new List<ReplHierarchyNode>();
            }

            HierarchyNodeRepository rep = new HierarchyNodeRepository(config, NAVVersion);
            return rep.ReplicateHierarchyNode(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyLeaf> ReplicateHierarchyLeaf(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error(config.LSKey.Key, "Only supported in NAV 10.x and later");
                return new List<ReplHierarchyLeaf>();
            }

            HierarchyNodeRepository rep = new HierarchyNodeRepository(config, NAVVersion);
            return rep.ReplicateHierarchyLeaf(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyHospDeal> ReplicateHierarchyHospDeal(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error(config.LSKey.Key, "Only supported in NAV 10.x and later");
                return new List<ReplHierarchyHospDeal>();
            }

            HierarchyHospLeafRepository rep = new HierarchyHospLeafRepository(config, NAVVersion);
            return rep.ReplicateHierarchyHospDeal(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyHospDealLine> ReplicateHierarchyHospDealLine(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error(config.LSKey.Key, "Only supported in NAV 10.x and later");
                return new List<ReplHierarchyHospDealLine>();
            }

            HierarchyHospLeafRepository rep = new HierarchyHospLeafRepository(config, NAVVersion);
            return rep.ReplicateHierarchyHospDealLine(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyHospRecipe> ReplicateHierarchyHospRecipe(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error(config.LSKey.Key, "Only supported in NAV 10.x and later");
                return new List<ReplHierarchyHospRecipe>();
            }

            HierarchyHospLeafRepository rep = new HierarchyHospLeafRepository(config, NAVVersion);
            return rep.ReplicateHierarchyHospRecipe(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplItemModifier> ReplicateItemModifier(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion.Major < 10)
            {
                logger.Error(config.LSKey.Key, "Only supported in NAV 10.x and later");
                return new List<ReplItemModifier>();
            }

            ItemModifierRepository rep = new ItemModifierRepository(config);
            return rep.ReplicateItemModifier(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        #endregion
    }
}
 