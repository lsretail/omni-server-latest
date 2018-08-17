using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Base;
using Domain.Items;
using Domain.OneList;
using Domain.Transactions;
using Domain.Stores;
using Domain.Profiles;
using Domain.Notifications;
using Domain.Offers;
using System.Collections.ObjectModel;

namespace Domain.Search
{
    public class NormalizedSearchRs : EntityBase, IAggregateRoot
        {
        #region Member variables

        private string itemsHdr;
        private ObservableCollection<Item> items;
        private ObservableCollection<ProductGroup> productGroups;
        private ObservableCollection<ItemCategory> itemCategories;
        private string transactionsHdr;
        private ObservableCollection<Transaction> transactions;        
        private ObservableCollection<Store> stores;
        private ObservableCollection<Profile> profiles;
        private ObservableCollection<Notification> notifications;
		private ObservableCollection<PublishedOffer> publishedOffers;
 
        #endregion

        #region Properties

        public string ItemsHdr
        {
            get { return itemsHdr; }
            set { itemsHdr = value; }
        }

        public ObservableCollection<Item> Items
        {
            get { return items; }
            set { items = value; }
        }

        public ObservableCollection<ProductGroup> ProductGroups
        {
            get { return productGroups; }
            set { productGroups = value; }
        }

        public ObservableCollection<ItemCategory> ItemCategories
        {
            get { return itemCategories; }
            set { itemCategories = value; }
        }

        public string TransactionsHdr
        {
            get { return transactionsHdr; }
            set { transactionsHdr = value; }
        }

        public ObservableCollection<Transaction> Transactions
        {
            get { return transactions; }
            set { transactions = value; }
        }			      

        public ObservableCollection<Store> Stores
        {
            get { return stores; }
            set { stores = value; }
        }

        public ObservableCollection<Profile> Profiles
        {
            get { return profiles; }
            set { profiles = value; }
        }

        public ObservableCollection<Notification> Notifications
        {
            get { return notifications; }
            set { notifications = value; }
        }

		public ObservableCollection<PublishedOffer> PublishedOffers
        {
            get { return publishedOffers; }
            set { publishedOffers = value; }
        }
 
        #endregion


        #region Constructors

        public NormalizedSearchRs(SearchRs inititator)
        {
            transactions = new ObservableCollection<Transaction>(inititator.Transactions);
            items = new ObservableCollection<Item>(inititator.Items);
            productGroups = new ObservableCollection<ProductGroup>(inititator.ProductGroups);
            itemCategories = new ObservableCollection<ItemCategory>(inititator.ItemCategories);            
            stores = new ObservableCollection<Store>(inititator.Stores);
            profiles = new ObservableCollection<Profile>(inititator.Profiles);
            notifications = new ObservableCollection<Notification>(inititator.Notifications);
			publishedOffers = new ObservableCollection<PublishedOffer>(inititator.PublishedOffers);
        }

        #endregion
    }

    public class SearchRs : EntityBase, IAggregateRoot
    {
        #region Member variables

        private List<Item> items;
        private List<ProductGroup> productGroups;
        private List<ItemCategory> itemCategories;
        private List<Transaction> transactions;        
        private List<Store> stores;
        private List<Profile> profiles;
        private List<Notification> notifications;
		private List<PublishedOffer> publishedOffers;
        private List<OneList.OneList> oneLists;
        private List<OneListItem> oneListItems;
        
        #endregion

        #region Properties

        public List<Item> Items
        {
            get { return items; }
            set { items = value; }
        }

        public List<ProductGroup> ProductGroups
        {
            get { return productGroups; }
            set { productGroups = value; }
        }

        public List<ItemCategory> ItemCategories
        {
            get { return itemCategories; }
            set { itemCategories = value; }
        }

        public List<Transaction> Transactions
        {
            get { return transactions; }
            set { transactions = value; }
        }
			        
        public List<Store> Stores
        {
            get { return stores; }
            set { stores = value; }
        }

        public List<Profile> Profiles
        {
            get { return profiles; }
            set { profiles = value; }
        }

        public List<Notification> Notifications
        {
            get { return notifications; }
            set { notifications = value; }
        }

        public List<PublishedOffer> PublishedOffers
        {
            get { return publishedOffers; }
            set { publishedOffers = value; }
        }

        public List<OneList.OneList> OneLists
        {
            get { return oneLists; }
            set { oneLists = value; }
        }

        public List<OneListItem> OneListItems
        {
            get { return oneListItems; }
            set { oneListItems = value; }
        }

        #endregion


        #region Constructors

        public SearchRs(string id) : base(id)
        {
            transactions = new List<Transaction>();
            items = new List<Item>();
            productGroups = new List<ProductGroup>();
            itemCategories = new List<ItemCategory>();
            stores = new List<Store>();
            profiles = new List<Profile>();
            notifications = new List<Notification>();
            publishedOffers = new List<PublishedOffer>();
            oneLists = new List<OneList.OneList>();
            oneListItems = new List<OneListItem>();
        }
        public SearchRs()
            : this(null)
        {

        }

        #endregion
    }

	[Flags]
	public enum SearchType
	{
		Item = 1,   
		ProductGroup = 2,      
		ItemCategory = 4,             
		Transaction = 8,
		Store = 16,
		Profile = 32,
		Notification = 64,
		Offer = 128,
		Coupon = 256,        
		OneList = 512,
		OneListLine = 1024,
		All = Item | ProductGroup | ItemCategory | Transaction | Store | Profile | Notification | Offer | Coupon | OneList | OneListLine,  // 8191 All search types returned
	}
}
