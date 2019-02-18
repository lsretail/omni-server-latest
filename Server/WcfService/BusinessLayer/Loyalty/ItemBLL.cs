using System;
using System.Collections.Generic;

using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Requests;

namespace LSOmni.BLL.Loyalty
{
    public class ItemBLL : BaseLoyBLL
    {
        public ItemBLL(string securityToken, int timeoutInSeconds)
            : base(securityToken, timeoutInSeconds)
        {
        }

        public ItemBLL(int timeoutInSeconds)
            : this("", timeoutInSeconds)
        {
        }

        public virtual LoyItem ItemGetById(string id, string storeId)
        {
            if (string.IsNullOrEmpty(storeId))
            {
                IAppSettingsRepository iAppRepo = GetDbRepository<IAppSettingsRepository>();
                storeId = iAppRepo.AppSettingsGetByKey(AppSettingsKey.Loyalty_FilterOnStore);
            }
            return BOLoyConnection.ItemGetById(id, storeId, GetAppSettingCurrencyCulture(), true);
        }

        public virtual LoyItem ItemGetByBarcode(string barcode, string storeId)
        {
            if (string.IsNullOrEmpty(storeId))
            {
                IAppSettingsRepository iAppRepo = GetDbRepository<IAppSettingsRepository>();
                storeId = iAppRepo.AppSettingsGetByKey(AppSettingsKey.Loyalty_FilterOnStore);
            }
            return this.BOLoyConnection.ItemLoyGetByBarcode(barcode, storeId, GetAppSettingCurrencyCulture());
        }

        public virtual List<LoyItem> ItemsSearch(string search, int maxNumberOfItems, bool includeDetails)
        {
            IAppSettingsRepository iAppRepo = GetDbRepository<IAppSettingsRepository>();
            string storeId = iAppRepo.AppSettingsGetByKey(AppSettingsKey.Loyalty_FilterOnStore);
            return BOLoyConnection.ItemsSearch(search, storeId, maxNumberOfItems, includeDetails); ;
        }

        public virtual List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems)
        {
            if (numberOfItems <= 0)
                numberOfItems = 50;

            //new in LS Nav 9.00.03, but returns empty list if web service not found in nav 
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
            return base.BOAppConnection.ItemsInStockGet(items, storeId, string.Empty);
        }

        public virtual List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, bool includeDetails)
        {
            IAppSettingsRepository iAppRepo = GetDbRepository<IAppSettingsRepository>();
            string storeId = iAppRepo.AppSettingsGetByKey(AppSettingsKey.Loyalty_FilterOnStore);
            return BOLoyConnection.ItemsPage(pageSize, pageNumber, itemCategoryId, productGroupId, search, storeId, includeDetails);
        }

        public virtual List<ItemCategory> ItemCategoriesGetAll()
        {
            IAppSettingsRepository iAppRepo = GetDbRepository<IAppSettingsRepository>();
            string storeId = iAppRepo.AppSettingsGetByKey(AppSettingsKey.Loyalty_FilterOnStore);
            return BOLoyConnection.ItemCategoriesGet(storeId, string.Empty);
        }

        public virtual ItemCategory ItemCategoriesGetById(string id)
        {
            return BOLoyConnection.ItemCategoriesGetById(id);
        }

        public virtual ProductGroup ProductGroupGetById(string id, bool includeDetails)
        {
            return BOLoyConnection.ProductGroupGetById(id, GetAppSettingCurrencyCulture(), includeDetails, includeDetails);
        }
    }
}

 