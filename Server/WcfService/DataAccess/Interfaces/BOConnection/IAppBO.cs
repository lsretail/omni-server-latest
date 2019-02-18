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
        string ItemDetailsGetById(string itemId);
        List<InventoryResponse> ItemInStockGet(string itemId, string variantId, int arrivingInStockInDays, List<string> locationIds, bool skipUnAvailableStores);
        List<InventoryResponse> ItemsInStockGet(List<InventoryRequest> items, string storeId, string locationId);
        List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string loyaltySchemeCode);

        #region Replication

        // Item
        List<ReplItem> ReplicateItems(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplItemUnitOfMeasure> ReplicateItemUOM(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplItemCategory> ReplicateItemCategory(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplPrice> ReplicatePrice(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplPrice> ReplicateBasePrice(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplProductGroup> ReplicateProductGroups(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);

        // Barcode
        List<ReplBarcode> ReplicateBarcodes(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplBarcodeMask> ReplicateBarcodeMasks(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplBarcodeMaskSegment> ReplicateBarcodeMaskSegments(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);

        // Variant
        List<ReplExtendedVariantValue> ReplicateExtendedVariantValues(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplItemVariantRegistration> ReplicateItemVariantRegistration(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplUnitOfMeasure> ReplicateUnitOfMeasure(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);

        // Hierarchy
        List<ReplHierarchy> ReplicateHierarchy(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplHierarchyNode> ReplicateHierarchyNode(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplHierarchyLeaf> ReplicateHierarchyLeaf(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);

        // Setup
        List<ReplStore> ReplicateStores(string storeId, string terminalId);
        List<ReplStore> ReplicateStores(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplStaff> ReplicateStaff(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplVendor> ReplicateVendors(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplCurrency> ReplicateCurrency(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplCurrencyExchRate> ReplicateCurrencyExchRate(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplCustomer> ReplicateCustomer(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplDiscount> ReplicateDiscounts(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplDiscount> ReplicateMixAndMatch(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplDiscountValidation> ReplicateDiscountValidations(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        List<ReplStoreTenderType> ReplicateStoreTenderType(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);

        #endregion
    }
}
