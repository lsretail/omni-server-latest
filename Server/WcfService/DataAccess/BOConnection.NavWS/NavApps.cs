﻿using System;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Requests;

namespace LSOmni.DataAccess.BOConnection.NavWS
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

        public virtual Terminal TerminalGetById(string terminalId, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.TerminalGetById(terminalId);

            return LSCWSBase.TerminalGetById(terminalId, stat);
        }

        public virtual List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string schemeCode, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 17.5 for SaaS");
                return new List<ProactiveDiscount>();
            }

            return LSCWSBase.DiscountsGet(storeId, schemeCode, itemIds, stat);
        }

        public virtual UnitOfMeasure UnitOfMeasureGetById(string id, Statistics stat)
        {
            logger.Warn(config.LSKey.Key, "Not supported by LS central version for SaaS");
            return new UnitOfMeasure();
        }

        public virtual VariantRegistration VariantRegGetById(string id, string itemId, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.VariantRegGetById(id, itemId);

            return LSCWSBase.VariantRegGetById(id, itemId, stat);
        }

        public virtual Currency CurrencyGetById(string id, string culture, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.CurrencyGetById(id, culture);

            return LSCWSBase.CurrencyGetById(id, culture, stat);
        }

        public virtual List<InventoryResponse> ItemInStockGet(string itemId, string variantId, int arrivingInStockInDays, List<string> locationIds, bool skipUnAvailableStores, Statistics stat)
        {
            if (locationIds.Count == 0)
            {
                List<Store> stores;
                if (NAVVersion < new Version("17.5"))
                    stores = NavWSBase.StoresGet(StoreGetType.All, false);

                stores = LSCWSBase.StoresGet(StoreGetType.All, false, 0, stat);

                //get the storeIds close to this store
                foreach (Store store in stores)
                    locationIds.Add(store.Id);
            }

            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemInStockGet(itemId, variantId, arrivingInStockInDays, locationIds, skipUnAvailableStores);

            return LSCWSBase.ItemInStockGet(itemId, variantId, arrivingInStockInDays, locationIds, skipUnAvailableStores, stat);
        }

        public virtual List<InventoryResponse> ItemsInStoreGet(List<InventoryRequest> items, string storeId, string locationId, bool useSourcingLocation, Statistics stat)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ItemsInStoreGet(items, storeId, locationId);

            return LSCWSBase.ItemsInStoreGet(items, storeId, locationId, useSourcingLocation, stat);
        }

        public virtual string ItemDetailsGetById(string itemId, Statistics stat)
        {
            return string.Empty;    //TODO call ws or get it with menu
        }

        #region replication

        public virtual List<ReplItem> ReplicateItems(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateItems(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateItems(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public List<ReplBarcode> ReplicateBarcodes(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateBarcodes(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateBarcodes(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public List<ReplBarcodeMaskSegment> ReplicateBarcodeMaskSegments(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateBarcodeMaskSegments(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateBarcodeMaskSegments(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public List<ReplBarcodeMask> ReplicateBarcodeMasks(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateBarcodeMasks(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateBarcodeMasks(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplExtendedVariantValue> ReplicateExtendedVariantValues(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateExtendedVariantValues(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateExtendedVariantValues(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public List<ReplItemUnitOfMeasure> ReplicateItemUOM(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateItemUOM(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateItemUOM(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplItemVariantRegistration> ReplicateItemVariantRegistration(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateItemVariantRegistration(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateItemVariantRegistration(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplItemVariant> ReplicateItemVariant(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 17.5 for SaaS");
                return new List<ReplItemVariant>();
            }

            return LSCWSBase.ReplicateItemVariant(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplStaff> ReplicateStaff(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateStaff(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateStaff(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplStaffPermission> ReplicateStaffPermission(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("24.0"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 24.0 for SaaS");
                return new List<ReplStaffPermission>();
            }

            return LSCWSBase.ReplicateStaffPermission(storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplVendor> ReplicateVendors(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateVendors(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateVendors(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplCurrency> ReplicateCurrency(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateCurrency(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateCurrency(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplCurrencyExchRate> ReplicateCurrencyExchRate(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateCurrencyExchRate(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateCurrencyExchRate(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplCustomer> ReplicateCustomer(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateCustomer(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateCustomer(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplItemCategory> ReplicateItemCategory(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateItemCategory(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateItemCategory(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplItemLocation> ReplicateItemLocation(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateItemLocation(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateItemLocation(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplPrice> ReplicatePrice(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicatePrice(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicatePrice(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplPrice> ReplicateBasePrice(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("20.4"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 20.4 for SaaS");
                return new List<ReplPrice>();
            }

            return LSCWSBase.ReplicateBasePrice(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public List<ReplProductGroup> ReplicateProductGroups(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateProductGroups(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateProductGroups(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplStore> ReplicateInvStores(string appId, string appType, string storeId, bool fullReplication, string terminalId)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateInvStores(appId, appType, storeId, fullReplication, terminalId);

            return LSCWSBase.ReplicateInvStores(appId, appType, storeId, fullReplication, terminalId);
        }

        public virtual List<ReplStore> ReplicateStores(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateStores(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateStores(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplUnitOfMeasure> ReplicateUnitOfMeasure(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateUnitOfMeasure(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateUnitOfMeasure(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplCollection> ReplicateCollection(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateCollection(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateCollection(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscount> ReplicateDiscounts(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateDiscounts(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateDiscounts(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscount> ReplicateMixAndMatch(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateMixAndMatch(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateMixAndMatch(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscountSetup> ReplicateDiscountSetup(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("24.0"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 24.0 for SaaS");
                return new List<ReplDiscountSetup>();
            }

            return LSCWSBase.ReplicateDiscountSetup(batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscountValidation> ReplicateDiscountValidations(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateDiscountValidations(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateDiscountValidations(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public List<ReplStoreTenderType> ReplicateStoreTenderType(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateStoreTenderType(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateStoreTenderType(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplTaxSetup> ReplicateTaxSetup(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateTaxSetup(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateTaxSetup(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplValidationSchedule> ReplicateValidationSchedule(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateValidationSchedule(appId, appType, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ValidationScheduleGet(storeId);
        }

        public virtual List<ReplHierarchy> ReplicateHierarchy(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateHierarchy(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateHierarchy(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyNode> ReplicateHierarchyNode(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateHierarchyNode(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateHierarchyNode(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyLeaf> ReplicateHierarchyLeaf(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ReplicateHierarchyLeaf(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);

            return LSCWSBase.ReplicateHierarchyLeaf(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyHospDeal> ReplicateHierarchyHospDeal(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("19.3"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 19.3 for SaaS");
                return new List<ReplHierarchyHospDeal>();
            }

            return LSCWSBase.ReplicateHierarchyDeal(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyHospDealLine> ReplicateHierarchyHospDealLine(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("19.3"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 19.3 for SaaS");
                return new List<ReplHierarchyHospDealLine>();
            }

            return LSCWSBase.ReplicateHierarchyDealLine(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplItemRecipe> ReplicateItemRecipe(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("19.3"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 19.3 for SaaS");
                return new List<ReplItemRecipe>();
            }

            return LSCWSBase.ReplicateItemRecipe(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplItemModifier> ReplicateItemModifier(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (NAVVersion < new Version("19.3"))
            {
                logger.Warn(config.LSKey.Key, "Not supported by LS Central version < 19.3 for SaaS");
                return new List<ReplItemModifier>();
            }

            return LSCWSBase.ReplicateItemModifier(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref recordsRemaining);
        }

        #endregion
    }
}
