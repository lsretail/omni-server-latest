using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.BLL.Loyalty
{
    public class SearchBLL : BaseLoyBLL
    {
        private readonly INotificationRepository iNotificationRepository;
        private readonly IOneListRepository iOneListRepository;

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

        public virtual SearchRs Search(string cardId, string search, int maxResultset, SearchType searchTypes, Statistics stat)
        {
            SQLHelper.CheckForSQLInjection(search);

            SearchRs searchRs = new SearchRs();
            if ((searchTypes & SearchType.Item) != 0)
            {
                searchRs.Items = BOLoyConnection.ItemsSearch(search, "", maxResultset, false, stat);
            }
            if ((searchTypes & SearchType.ProductGroup) != 0)
            {
                searchRs.ProductGroups = BOLoyConnection.ProductGroupSearch(search, stat);
            }
            if ((searchTypes & SearchType.ItemCategory) != 0)
            {
                searchRs.ItemCategories = BOLoyConnection.ItemCategorySearch(search, stat);
            }
            if ((searchTypes & SearchType.SalesEntry) != 0)
            {
                searchRs.SalesEntries = BOLoyConnection.SalesEntrySearch(search, cardId, maxResultset, stat);
            }
            if ((searchTypes & SearchType.Store) != 0)
            {
                searchRs.Stores = BOLoyConnection.StoreLoySearch(search, stat);
            }
            if ((searchTypes & SearchType.Profile) != 0)
            {
                searchRs.Profiles = BOLoyConnection.ProfileSearch(cardId, search, stat);
            }
            if ((searchTypes & SearchType.Notification) != 0)
            {
                searchRs.Notifications = iNotificationRepository.NotificationSearch(cardId, search, maxResultset, stat);
            }
            if ((searchTypes & SearchType.OneList) != 0)
            {
                searchRs.OneLists = iOneListRepository.OneListSearch(cardId, search, maxResultset, ListType.Basket, true, stat);
            }
            return searchRs;
        }
    }
}
