﻿using System;
using System.Linq;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.BOConnection;
using LSOmni.DataAccess.BOConnection.CentralPre.Dal;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Requests;

namespace LSOmni.DataAccess.BOConnection.CentralPre
{
    
    //Navision back office connection
    public class NavApps : NavBase, IAppBO
    {
        public int TimeoutInSeconds
        {
            set { base.TimeOutInSeconds = value; }
        }

        public NavApps(BOConfiguration config) : base(config)
        {
        }

        public virtual List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string schemeCode, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            DiscountRepository rep = new DiscountRepository(config, LSCVersion);
            List<ProactiveDiscount> discounts = rep.DiscountsGetByStoreAndItem(storeId, itemIds);

            if (string.IsNullOrEmpty(schemeCode))
            {
                discounts = discounts.Where(disc => disc.LoyaltySchemeCode == string.Empty).ToList();
            }
            else
            {
                discounts = discounts.Where(disc => disc.LoyaltySchemeCode == string.Empty || disc.LoyaltySchemeCode == schemeCode).ToList();
            }

            List<ProactiveDiscount> list = new List<ProactiveDiscount>();
            foreach (ProactiveDiscount disc in discounts)
            {
                DiscountValidation dValid = rep.GetDiscountValidationByOfferId(disc.PeriodId);
                if (dValid.OfferIsValid())
                {
                    rep.LoadDiscountDetails(disc, storeId, schemeCode);
                    list.Add(disc);
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public virtual Terminal TerminalGetById(string terminalId, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            TerminalRepository rep = new TerminalRepository(config);
            Terminal data = rep.TerminalGetById(terminalId);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public virtual UnitOfMeasure UnitOfMeasureGetById(string id, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            UnitOfMeasureRepository rep = new UnitOfMeasureRepository(config);
            UnitOfMeasure data = rep.UnitOfMeasureGetById(id);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public virtual VariantRegistration VariantRegGetById(string id, string itemId, Statistics stat)
        {
            ItemVariantRegistrationRepository rep = new ItemVariantRegistrationRepository(config, LSCVersion);
            return rep.VariantRegGetById(id, itemId, stat);
        }

        public virtual Currency CurrencyGetById(string id, string culture, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            CurrencyRepository rep = new CurrencyRepository(config);
            Currency data = rep.CurrencyLoyGetById(id, culture);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public virtual List<InventoryResponse> ItemInStockGet(string itemId, string variantId, int arrivingInStockInDays, List<string> locationIds, bool skipUnAvailableStores, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            if (locationIds.Count == 0)
            {
                //JIJ add the distance calc for them, radius of X km ?
                int recordsRemaining = 0;
                string lastKey = "", maxKey = "";
                StoreRepository rep = new StoreRepository(config, LSCVersion);
                List<ReplStore> totalList = rep.ReplicateStores(100, true, ref lastKey, ref maxKey, ref recordsRemaining);

                //get the storeIds close to this store
                foreach (ReplStore store in totalList)
                    locationIds.Add(store.Id);
            }

            List<InventoryResponse> list = LSCentralWSBase.ItemInStockGet(itemId, variantId, arrivingInStockInDays, locationIds, skipUnAvailableStores, stat);
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public virtual List<InventoryResponse> ItemsInStoreGet(List<InventoryRequest> items, string storeId, string locationId, bool useSourcingLocation, Statistics stat)
        {
            return LSCentralWSBase.ItemsInStoreGet(items, storeId, locationId, useSourcingLocation, stat);
        }

        public virtual string ItemDetailsGetById(string itemId, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            ItemRepository itemRep = new ItemRepository(config, LSCVersion);
            string data = itemRep.ItemDetailsGetById(itemId);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        #region replication

        public virtual List<ReplItem> ReplicateItems(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemRepository rep = new ItemRepository(config, LSCVersion);
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
            ExtendedVariantValuesRepository rep = new ExtendedVariantValuesRepository(config, LSCVersion);
            return rep.ReplicateExtendedVariantValues(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplItemUnitOfMeasure> ReplicateItemUOM(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemUOMRepository rep = new ItemUOMRepository(config);
            return rep.ReplicateItemUOM(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplItemVariantRegistration> ReplicateItemVariantRegistration(string appId, string appType, string storeId, int batchSize, bool fullReplication,ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemVariantRegistrationRepository rep = new ItemVariantRegistrationRepository(config, LSCVersion);
            return rep.ReplicateItemVariantRegistration(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplItemVariant> ReplicateItemVariant(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemVariantRegistrationRepository rep = new ItemVariantRegistrationRepository(config, LSCVersion);
            return rep.ReplicateItemVariant(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplStaff> ReplicateStaff(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            StaffRepository rep = new StaffRepository(config);
            return rep.ReplicateStaff(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplStaffPermission> ReplicateStaffPermission(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            StaffPermissionRepository rep = new StaffPermissionRepository(config);
            return rep.ReplicateStaffPermission(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplVendor> ReplicateVendors(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            VendorRepository rep = new VendorRepository(config);
            return rep.ReplicateVendor(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCurrency> ReplicateCurrency(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            CurrencyRepository rep = new CurrencyRepository(config);
            return rep.ReplicateCurrency(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCurrencyExchRate> ReplicateCurrencyExchRate(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            CurrencyExchRateRepository rep = new CurrencyExchRateRepository(config);
            return rep.ReplicateCurrencyExchRate(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplTaxSetup> ReplicateTaxSetup(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
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
            ItemCategoryRepository rep = new ItemCategoryRepository(config, LSCVersion);
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
            if (LSCVersion >= new Version("21.3") && rep.UsesNewPriceLines())
                return rep.ReplicatePriceLines(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);

            return rep.ReplicateAllPrice(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplProductGroup> ReplicateProductGroups(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ProductGroupRepository rep = new ProductGroupRepository(config, LSCVersion);
            return rep.ReplicateProductGroups(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplStore> ReplicateStores(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            StoreRepository rep = new StoreRepository(config, LSCVersion);
            return rep.ReplicateStores(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplStore> ReplicateInvStores(string appId, string appType, string storeId, bool fullReplication, string terminalId)
        {
            StoreRepository rep = new StoreRepository(config, LSCVersion);
            return rep.ReplicateInvStores(storeId, terminalId);
        }

        public virtual List<ReplUnitOfMeasure> ReplicateUnitOfMeasure(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            UnitOfMeasureRepository rep = new UnitOfMeasureRepository(config);
            return rep.ReplicateUnitOfMeasure(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCollection> ReplicateCollection(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            UnitOfMeasureRepository rep = new UnitOfMeasureRepository(config);
            return rep.ReplicateCollection(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscount> ReplicateDiscounts(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            DiscountOfferRepository rep = new DiscountOfferRepository(config, LSCVersion);
            return rep.ReplicateDiscounts(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscount> ReplicateMixAndMatch(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            DiscountOfferRepository rep = new DiscountOfferRepository(config, LSCVersion);
            return rep.ReplicateMixAndMatch(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscountSetup> ReplicateDiscountSetup(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            DiscountOfferRepository rep = new DiscountOfferRepository(config, LSCVersion);
            return rep.ReplicateDiscountSetup(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscountValidation> ReplicateDiscountValidations(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            DiscountOfferRepository rep = new DiscountOfferRepository(config, LSCVersion);
            return rep.ReplicateDiscountValidations(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplStoreTenderType> ReplicateStoreTenderType(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            StoreTenderTypeRepository rep = new StoreTenderTypeRepository(config);
            return rep.ReplicateStoreTenderType(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplValidationSchedule> ReplicateValidationSchedule(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ValidationScheduleRepository rep = new ValidationScheduleRepository(config);
            return rep.ReplicateValidationSchedule(batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchy> ReplicateHierarchy(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            HierarchyRepository rep = new HierarchyRepository(config);
            return rep.ReplicateHierarchy(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyNode> ReplicateHierarchyNode(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            HierarchyNodeRepository rep = new HierarchyNodeRepository(config);
            return rep.ReplicateHierarchyNode(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyLeaf> ReplicateHierarchyLeaf(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            HierarchyNodeRepository rep = new HierarchyNodeRepository(config);
            return rep.ReplicateHierarchyLeaf(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyHospDeal> ReplicateHierarchyHospDeal(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            HierarchyHospLeafRepository rep = new HierarchyHospLeafRepository(config);
            return rep.ReplicateHierarchyHospDeal(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyHospDealLine> ReplicateHierarchyHospDealLine(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            HierarchyHospLeafRepository rep = new HierarchyHospLeafRepository(config);
            return rep.ReplicateHierarchyHospDealLine(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplItemRecipe> ReplicateItemRecipe(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemRecipeRepository rep = new ItemRecipeRepository(config);
            return rep.ReplicateItemRecipe(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplItemModifier> ReplicateItemModifier(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            ItemModifierRepository rep = new ItemModifierRepository(config);
            return rep.ReplicateItemModifier(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        #endregion
    }
}
 