using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    [Flags]
    public enum SearchType
    {
        [EnumMember]
        Item = 1,   
        [EnumMember]
        ProductGroup = 2,      
        [EnumMember]
        ItemCategory = 4,       
        [EnumMember]
        SalesEntry = 8,
        [EnumMember]
        Store = 16,
        [EnumMember]
        Profile = 32,
        [EnumMember]
        Notification = 64,
        [EnumMember]
        Offer = 128,
        [EnumMember]
        Coupon = 256,
        [EnumMember]
        OneList = 512,
        [EnumMember]
        All = Item | ProductGroup | ItemCategory | SalesEntry | Store | Profile | Notification | Offer | Coupon | OneList,  // 1023 All search types returned
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class SearchRs : Entity, IDisposable
    {
        public SearchRs(string id) : base(id)
        {
            SalesEntries = new List<SalesEntry>();
            Items = new List<LoyItem>();
            ProductGroups = new List<ProductGroup>();
            ItemCategories = new List<ItemCategory>();
            Stores = new List<Store>();
            Profiles = new List<Profile>();
            Notifications = new List<Notification>();
            OneLists = new List<OneList>();
        }

        public SearchRs()
            : this(string.Empty)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (SalesEntries != null)
                    SalesEntries.Clear();
                if (Items != null)
                    Items.Clear();
                if (ProductGroups != null)
                    ProductGroups.Clear();
                if (ItemCategories != null)
                    ItemCategories.Clear();
                if (Stores != null)
                    Stores.Clear();
                if (Profiles != null)
                    Profiles.Clear();
                if (Notifications != null)
                    Notifications.Clear();
                if (OneLists != null)
                    OneLists.Clear();
            }
        }

        [DataMember]
        public List<LoyItem> Items { get; set; }
        [DataMember]
        public List<ProductGroup> ProductGroups { get; set; }
        [DataMember]
        public List<ItemCategory> ItemCategories { get; set; }
        [DataMember]
        public List<SalesEntry> SalesEntries { get; set; }

        [DataMember]
        public List<Store> Stores { get; set; }
        [DataMember]
        public List<Profile> Profiles { get; set; }
        [DataMember]
        public List<Notification> Notifications { get; set; }
        [DataMember]
        public List<OneList> OneLists { get; set; }
    }
}
