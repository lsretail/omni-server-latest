using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.BLL.Loyalty
{
    public class SearchBLL : BaseLoyBLL
    {
        private INotificationRepository iNotificationRepository;
        private IOneListRepository iOneListRepository;

        public SearchBLL(string securityToken, int timeoutInSeconds)
            : base(securityToken, timeoutInSeconds)
        {
            this.iNotificationRepository = GetDbRepository<INotificationRepository>();
            this.iOneListRepository = GetDbRepository<IOneListRepository>();
        }

        public SearchBLL(int timeoutInSeconds)
            : this("", timeoutInSeconds)
        {
        }

        public virtual SearchRs Search(string contactId, string search, int maxResultset, SearchType searchTypes)
        {
            SearchRs searchRs = new SearchRs();
            if ((searchTypes & SearchType.Item) != 0)
            {
                IAppSettingsRepository iAppRepo = GetDbRepository<IAppSettingsRepository>();
                string storeId = iAppRepo.AppSettingsGetByKey(AppSettingsKey.Loyalty_FilterOnStore);
                searchRs.Items = BOLoyConnection.ItemsSearch(search, storeId, maxResultset, false);
            }
            if ((searchTypes & SearchType.ProductGroup) != 0)
            {
                searchRs.ProductGroups = BOLoyConnection.ProductGroupSearch(search);
            }
            if ((searchTypes & SearchType.ItemCategory) != 0)
            {
                searchRs.ItemCategories = BOLoyConnection.ItemCategorySearch(search);
            }
            if ((searchTypes & SearchType.Transaction) != 0)
            {
                searchRs.Transactions = BOLoyConnection.TransactionSearch(search, contactId, maxResultset, base.GetAppSettingCurrencyCulture(), false);
            }
            if ((searchTypes & SearchType.Store) != 0)
            {
                IAppSettingsRepository iAppRepo = GetDbRepository<IAppSettingsRepository>();
                int offset = iAppRepo.AppSettingsIntGetByKey(AppSettingsKey.Timezone_HoursOffset_DD);

                searchRs.Stores = BOLoyConnection.StoreLoySearch(search);
                foreach (Store store in searchRs.Stores)
                {
                    store.StoreHours = BOLoyConnection.StoreHoursGetByStoreId(store.Id, offset);
                }
            }
            if ((searchTypes & SearchType.Profile) != 0)
            {
                searchRs.Profiles = BOLoyConnection.ProfileSearch(contactId, search);
            }
            if ((searchTypes & SearchType.Notification) != 0)
            {
                searchRs.Notifications = iNotificationRepository.NotificationSearch(contactId, search, maxResultset);
            }
            if ((searchTypes & SearchType.OneList) != 0)
            {
                searchRs.OneLists = iOneListRepository.OneListSearch(contactId, search, maxResultset, ListType.Basket, true);
            }
            return searchRs;
        }
    }
}
