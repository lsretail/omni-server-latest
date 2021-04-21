using System;
using System.Collections.Generic;

using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.DiscountEngine.DataModels;
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
        public NavApps(BOConfiguration config) : base(config)
        {
        }

        public virtual Terminal TerminalGetById(string terminalId)
        {
            return NavWSBase.TerminalGetById(terminalId);
        }

        public virtual List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string loyaltySchemeCode)
        {
            throw new NotImplementedException("DISCOUNT ENGINE");
        }

        public virtual UnitOfMeasure UnitOfMeasureGetById(string id)
        {
            return null;
        }

        public virtual VariantRegistration VariantRegGetById(string id, string itemId)
        {
            return NavWSBase.VariantRegGetById(id, itemId);
        }

        public virtual Currency CurrencyGetById(string id, string culture)
        {
            return NavWSBase.CurrencyGetById(id, culture);
        }

        public virtual List<InventoryResponse> ItemInStockGet(string itemId, string variantId, int arrivingInStockInDays, List<string> locationIds, bool skipUnAvailableStores)
        {
            if (locationIds.Count == 0)
            {
                //JIJ add the distance calc for them, radius of X km ?
                //int recordsRemaining = 0;
                //string lastKey = "", maxKey = "";
                //StoreRepository rep = new StoreRepository(config, NavVersion);
                //List<ReplStore> totalList = rep.ReplicateStores(100, true, ref lastKey, ref maxKey, ref recordsRemaining);

                ////get the storeIds close to this store
                //foreach (ReplStore store in totalList)
                //    locationIds.Add(store.Id);
            }

            return NavWSBase.ItemInStockGet(itemId, variantId, arrivingInStockInDays, locationIds, skipUnAvailableStores);
        }

        public virtual List<InventoryResponse> ItemsInStockGet(List<InventoryRequest> items, string storeId, string locationId)
        {
            return NavWSBase.ItemsInStockGet(items, storeId, locationId);
        }

        public virtual string ItemDetailsGetById(string itemId)
        {
            return string.Empty;    //TODO call ws or get it with menu
        }

        #region replication

        public virtual List<ReplItem> ReplicateItems(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateItems(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplBarcode> ReplicateBarcodes(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateBarcodes(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);
        }

        public List<ReplBarcodeMaskSegment> ReplicateBarcodeMaskSegments(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateBarcodeMaskSegments(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);
        }

        public List<ReplBarcodeMask> ReplicateBarcodeMasks(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateBarcodeMasks(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplExtendedVariantValue> ReplicateExtendedVariantValues(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateExtendedVariantValues(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplItemUnitOfMeasure> ReplicateItemUOM(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateItemUOM(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplItemVariantRegistration> ReplicateItemVariantRegistration(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateItemVariantRegistration(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplStaff> ReplicateStaff(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateStaff(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplVendor> ReplicateVendors(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateVendors(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCurrency> ReplicateCurrency(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateCurrency(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCurrencyExchRate> ReplicateCurrencyExchRate(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateCurrencyExchRate(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCustomer> ReplicateCustomer(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateCustomer(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplItemCategory> ReplicateItemCategory(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateItemCategory(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplItemLocation> ReplicateItemLocation(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateItemLocation(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplPrice> ReplicatePrice(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicatePrice(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplPrice> ReplicateBasePrice(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return new List<ReplPrice>();
        }

        public List<ReplProductGroup> ReplicateProductGroups(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateProductGroups(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);
        }

        public virtual List<ReplStore> ReplicateStores(string appId, string appType, string storeId, string terminalId)
        {
            return NavWSBase.ReplicateStores(appId, appType, storeId, terminalId);
        }

        public virtual List<ReplStore> ReplicateStores(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateStores(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplUnitOfMeasure> ReplicateUnitOfMeasure(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateUnitOfMeasure(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplCollection> ReplicateCollection(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateCollection(appId, appType, storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscount> ReplicateDiscounts(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateDiscounts(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscount> ReplicateMixAndMatch(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateMixAndMatch(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplDiscountValidation> ReplicateDiscountValidations(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateDiscountValidations(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public List<ReplStoreTenderType> ReplicateStoreTenderType(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateStoreTenderType(appId, appType, storeId, batchSize, ref lastKey, ref recordsRemaining);
        }
        public virtual List<ReplTaxSetup> ReplicateTaxSetup(string appId, string appType, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateTaxSetup(appId, appType, string.Empty, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplValidationSchedule> ReplicateValidationSchedule(string appId, string appType, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateValidationSchedule(appId, appType, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchy> ReplicateHierarchy(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateHierarchy(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyNode> ReplicateHierarchyNode(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateHierarchyNode(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyLeaf> ReplicateHierarchyLeaf(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return NavWSBase.ReplicateHierarchyLeaf(appId, appType, storeId, batchSize, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual List<ReplHierarchyHospDeal> ReplicateHierarchyHospDeal(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            // TODO ?? 
            return new List<ReplHierarchyHospDeal>();
        }

        public virtual List<ReplHierarchyHospDealLine> ReplicateHierarchyHospDealLine(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            // TODO ?? 
            return new List<ReplHierarchyHospDealLine>();
        }

        public virtual List<ReplItemRecipe> ReplicateItemRecipe(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            // TODO ?? 
            return new List<ReplItemRecipe>();
        }

        public virtual List<ReplItemModifier> ReplicateItemModifier(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            // TODO ?? 
            return new List<ReplItemModifier>();
        }

        #endregion
    }
}
