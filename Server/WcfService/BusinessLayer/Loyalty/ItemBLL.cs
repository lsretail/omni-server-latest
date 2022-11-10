using System;
using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
using LSOmni.Common.Util;

namespace LSOmni.BLL.Loyalty
{
    public class ItemBLL : BaseLoyBLL
    {
        public ItemBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
        }

        public virtual LoyItem ItemGetById(string id, string storeId, Statistics stat)
        {
            return BOLoyConnection.ItemGetById(id, storeId, GetAppSettingCurrencyCulture(), true, stat);
        }

        public virtual LoyItem ItemGetByBarcode(string barcode, string storeId, Statistics stat)
        {
            return this.BOLoyConnection.ItemLoyGetByBarcode(barcode, storeId, GetAppSettingCurrencyCulture(), stat);
        }

        public virtual List<LoyItem> ItemsSearch(string search, int maxNumberOfItems, bool includeDetails, Statistics stat)
        {
            return BOLoyConnection.ItemsSearch(search, string.Empty, maxNumberOfItems, includeDetails, stat);
        }

        public virtual List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems, Statistics stat)
        {
            if (numberOfItems <= 0)
                numberOfItems = 50;

            return base.BOLoyConnection.ItemsGetByPublishedOfferId(pubOfferId, numberOfItems, stat);
        }

        public virtual List<InventoryResponse> ItemsInStockGet(string storeId, string itemId, string variantId, int arrivingInStockInDays, Statistics stat)
        {
            if (arrivingInStockInDays < 1)
                arrivingInStockInDays = 1;
            if (variantId == null)
                variantId = string.Empty;

            List<string> list = new List<string>();
            if (string.IsNullOrWhiteSpace(storeId) == false)
                list.Add(storeId);

            return base.BOAppConnection.ItemInStockGet(itemId, variantId, arrivingInStockInDays, list, false, stat);
        }

        public virtual List<InventoryResponse> ItemsInStoreGet(List<InventoryRequest> items, string storeId, string locationId, bool useSourcingLocation, Statistics stat)
        {
            return base.BOAppConnection.ItemsInStoreGet(items, storeId, locationId, useSourcingLocation, stat);
        }

        public virtual List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId, Statistics stat)
        {
            return BOLoyConnection.CheckAvailability(request, storeId, stat);
        }

        public virtual List<LoyItem> ItemsPage(string storeId, int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, bool includeDetails, Statistics stat)
        {
            return BOLoyConnection.ItemsPage(pageSize, pageNumber, itemCategoryId, productGroupId, search, storeId, includeDetails, stat);
        }

        public virtual List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items, Statistics stat)
        {
            return BOLoyConnection.ItemCustomerPricesGet(storeId, cardId, items, stat);
        }

        public virtual List<ItemCategory> ItemCategoriesGetAll(Statistics stat)
        {
            return BOLoyConnection.ItemCategoriesGet("", string.Empty, stat);
        }

        public virtual ItemCategory ItemCategoriesGetById(string id, Statistics stat)
        {
            return BOLoyConnection.ItemCategoriesGetById(id, stat);
        }

        public virtual ProductGroup ProductGroupGetById(string id, bool includeDetails, Statistics stat)
        {
            return BOLoyConnection.ProductGroupGetById(id, GetAppSettingCurrencyCulture(), includeDetails, includeDetails, stat);
        }

        public virtual List<Hierarchy> HierarchyGet(string storeId, Statistics stat)
        {
            return BOLoyConnection.HierarchyGet(storeId, stat);
        }
    }
}

 