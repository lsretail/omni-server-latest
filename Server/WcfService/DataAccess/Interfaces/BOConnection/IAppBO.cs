using System.Collections.Generic;

using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Requests;

namespace LSOmni.DataAccess.Interface.BOConnection
{
    //Inventory Interface to the back office, Nav, Ax, etc.
    public interface IAppBO
    {
        Terminal TerminalGetById(string terminalId);
        UnitOfMeasure UnitOfMeasureGetById(string id);
        VariantRegistration VariantRegGetById(string id, string itemId);
        Currency CurrencyGetById(string id, string culture);
        List<InventoryResponse> ItemInStockGet(string itemId, string variantId, int arrivingInStockInDays, List<string> locationIds, bool skipUnAvailableStores);
        List<InventoryResponse> ItemsInStockGet(List<InventoryRequest> items, string storeId, string locationId);
        string ItemDetailsGetById(string itemId);
        List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string loyaltySchemeCode);

        #region Replication

        // Item
        List<ReplItem> ReplicateItems(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplItemUnitOfMeasure> ReplicateItemUOM(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplItemCategory> ReplicateItemCategory(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplPrice> ReplicatePrice(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplPrice> ReplicateBasePrice(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplProductGroup> ReplicateProductGroups(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);

        // Barcode
        List<ReplBarcode> ReplicateBarcodes(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplBarcodeMask> ReplicateBarcodeMasks(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplBarcodeMaskSegment> ReplicateBarcodeMaskSegments(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);

        // Variant
        List<ReplExtendedVariantValue> ReplicateExtendedVariantValues(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplItemVariantRegistration> ReplicateItemVariantRegistration(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplUnitOfMeasure> ReplicateUnitOfMeasure(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);

        // Hierarchy
        List<ReplHierarchy> ReplicateHierarchy(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplHierarchyNode> ReplicateHierarchyNode(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplHierarchyLeaf> ReplicateHierarchyLeaf(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);

        // Setup
        List<ReplStore> ReplicateStores(string appId, string appType, string storeId, string terminalId);
        List<ReplStore> ReplicateStores(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplStaff> ReplicateStaff(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplVendor> ReplicateVendors(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplCurrency> ReplicateCurrency(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplCurrencyExchRate> ReplicateCurrencyExchRate(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplCustomer> ReplicateCustomer(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplDiscount> ReplicateDiscounts(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplDiscount> ReplicateMixAndMatch(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplDiscountValidation> ReplicateDiscountValidations(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplStoreTenderType> ReplicateStoreTenderType(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplTaxSetup> ReplicateTaxSetup(string appId, string appType, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);

        #endregion
    }
}
