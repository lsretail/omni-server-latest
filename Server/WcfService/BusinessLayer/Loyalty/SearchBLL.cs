using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.BLL.Loyalty
{
    public class SearchBLL : BaseLoyBLL
    {
        private INotificationRepository iNotificationRepository;
        private IOneListRepository iOneListRepository;

        public SearchBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
            this.iNotificationRepository = GetDbRepository<INotificationRepository>(config);
            this.iOneListRepository = GetDbRepository<IOneListRepository>(config);
        }

        public SearchBLL(BOConfiguration config)
            : this(config, 0)
        {
        }

        public virtual SearchRs Search(string cardId, string search, int maxResultset, SearchType searchTypes)
        {
            SQLHelper.CheckForSQLInjection(search);

            SearchRs searchRs = new SearchRs();
            if ((searchTypes & SearchType.Item) != 0)
            {
                searchRs.Items = BOLoyConnection.ItemsSearch(search, "", maxResultset, false);
            }
            if ((searchTypes & SearchType.ProductGroup) != 0)
            {
                searchRs.ProductGroups = BOLoyConnection.ProductGroupSearch(search);
            }
            if ((searchTypes & SearchType.ItemCategory) != 0)
            {
                searchRs.ItemCategories = BOLoyConnection.ItemCategorySearch(search);
            }
            if ((searchTypes & SearchType.SalesEntry) != 0)
            {
                searchRs.SalesEntries = BOLoyConnection.SalesEntrySearch(search, cardId, maxResultset);
            }
            if ((searchTypes & SearchType.Store) != 0)
            {
                searchRs.Stores = BOLoyConnection.StoreLoySearch(search);
            }
            if ((searchTypes & SearchType.Profile) != 0)
            {
                searchRs.Profiles = BOLoyConnection.ProfileSearch(cardId, search);
            }
            if ((searchTypes & SearchType.Notification) != 0)
            {
                searchRs.Notifications = iNotificationRepository.NotificationSearch(cardId, search, maxResultset);
            }
            if ((searchTypes & SearchType.OneList) != 0)
            {
                searchRs.OneLists = iOneListRepository.OneListSearch(cardId, search, maxResultset, ListType.Basket, true);
            }
            return searchRs;
        }
    }
}
