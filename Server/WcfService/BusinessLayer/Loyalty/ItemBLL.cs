using System;
using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;

namespace LSOmni.BLL.Loyalty
{
    public class ItemBLL : BaseLoyBLL
    {
        public ItemBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
        }

        public virtual LoyItem ItemGetById(string id, string storeId)
        {
            return BOLoyConnection.ItemGetById(id, storeId, GetAppSettingCurrencyCulture(), true);
        }

        public virtual LoyItem ItemGetByBarcode(string barcode, string storeId)
        {
            return this.BOLoyConnection.ItemLoyGetByBarcode(barcode, storeId, GetAppSettingCurrencyCulture());
        }

        public virtual List<LoyItem> ItemsSearch(string search, int maxNumberOfItems, bool includeDetails)
        {
            return BOLoyConnection.ItemsSearch(search, string.Empty, maxNumberOfItems, includeDetails); ;
        }

        public virtual List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems)
        {
            if (numberOfItems <= 0)
                numberOfItems = 50;

            return base.BOLoyConnection.ItemsGetByPublishedOfferId(pubOfferId, numberOfItems);
        }

        public virtual List<InventoryResponse> ItemsInStockGet(string storeId, string itemId, string variantId, int arrivingInStockInDays)
        {
            if (arrivingInStockInDays < 1)
                arrivingInStockInDays = 1;
            if (variantId == null)
                variantId = string.Empty;

            List<string> list = new List<string>();
            if (string.IsNullOrWhiteSpace(storeId) == false)
                list.Add(storeId);

            return base.BOAppConnection.ItemInStockGet(itemId, variantId, arrivingInStockInDays, list, false);
        }

        public virtual List<InventoryResponse> ItemsInStoreGet(List<InventoryRequest> items, string storeId)
        {
            return base.BOAppConnection.ItemsInStoreGet(items, storeId, string.Empty);
        }

        public virtual List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId)
        {
            return BOLoyConnection.CheckAvailability(request, storeId);
        }

        public virtual List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, bool includeDetails)
        {
            return BOLoyConnection.ItemsPage(pageSize, pageNumber, itemCategoryId, productGroupId, search, "", includeDetails);
        }

        public virtual List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items)
        {
            return BOLoyConnection.ItemCustomerPricesGet(storeId, cardId, items);
        }

        public virtual List<ItemCategory> ItemCategoriesGetAll()
        {
            return BOLoyConnection.ItemCategoriesGet("", string.Empty);
        }

        public virtual ItemCategory ItemCategoriesGetById(string id)
        {
            return BOLoyConnection.ItemCategoriesGetById(id);
        }

        public virtual ProductGroup ProductGroupGetById(string id, bool includeDetails)
        {
            return BOLoyConnection.ProductGroupGetById(id, GetAppSettingCurrencyCulture(), includeDetails, includeDetails);
        }

        public virtual List<Hierarchy> HierarchyGet(string storeId)
        {
            return BOLoyConnection.HierarchyGet(storeId);
        }
    }
}

 