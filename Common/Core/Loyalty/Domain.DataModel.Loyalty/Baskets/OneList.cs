using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Baskets
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OneList : Entity, IDisposable
    {
        private decimal totalAmount;
        private bool isChecked;

        public OneList(string id) : base(id)
        {
            Description = string.Empty;
            CardId = string.Empty;
            StoreId = string.Empty;
            CreateDate = DateTime.Now;
            ListType = ListType.Basket; // basket, wish
            Items = new ObservableCollection<OneListItem>();
            CardLinks = new List<OneListLink>();
            PublishedOffers = new List<OneListPublishedOffer>();
            TotalAmount = 0M;
            TotalNetAmount = 0M;
            TotalTaxAmount = 0M;
            TotalDiscAmount = 0M;
            PointAmount = 0M;

            //Added for SPG
            IsChecked = true;
        }

        public OneList(string id, ObservableCollection<OneListItem> items, bool calculate) : this(id)
        {
            Items = items;
            State = BasketState.Normal;

            if (calculate)
            {
                CalculateBasket();
            }
        }

        public OneList()
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
                if (Items != null)
                    Items.Clear();
                if (PublishedOffers != null)
                    PublishedOffers.Clear();
            }
        }

        [DataMember(IsRequired = true)]
        public string StoreId { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember(IsRequired = true)]
        public string CardId { get; set; }
        [DataMember]
        public virtual List<OneListLink> CardLinks { get; set; }

        [IgnoreDataMember]
        public string GetNamesFromCardLinks
        {
            get
            {
                var cardLinkNames = string.Empty;
                
                OneListLink lastItem = CardLinks.Last();
                foreach (var name in CardLinks)
                {
                    if (name.Equals(lastItem))
                    {
                        cardLinkNames += name.Name;
                    }
                    else
                    {
                        cardLinkNames += name.Name + ", ";
                    }
                }

                return cardLinkNames;
            }
        }

        [IgnoreDataMember]
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                NotifyPropertyChanged();
            }
        }
        /// <summary>
        /// Type indicator to use in external system
        /// </summary>
        [DataMember]
        public int ExternalType { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CreateDate { get; set; }
        [DataMember(IsRequired = true)]
        public ListType ListType { get; set; }
        [DataMember]
        public bool IsHospitality { get; set; }
        [DataMember]
        public string SalesType { get; set; }
        [DataMember]
        public string ShipToCountryCode { get; set; }
        [DataMember]
        public virtual ObservableCollection<OneListItem> Items { get; set; }
        [DataMember]
        public virtual List<OneListPublishedOffer> PublishedOffers { get; set; }

        [DataMember]
        public decimal TotalAmount
        {
            get => totalAmount;
            set
            {
                totalAmount = value;
                NotifyPropertyChanged();
            }
        }

        [DataMember]
        public decimal TotalTaxAmount { get; set; }
        [DataMember]
        public decimal TotalNetAmount { get; set; }
        [DataMember]
        public decimal TotalDiscAmount { get; set; }
        [DataMember]
        public decimal ShippingAmount { get; set; }
        [DataMember]
        public decimal PointAmount { get; set; }

        public BasketState State { get; set; }

        public OneList Clone()
        {
            OneList clone = (OneList)MemberwiseClone();
            clone.Items = new ObservableCollection<OneListItem>(Items);
            return clone;
        }

        public void AddItem(OneListItem itemToAdd, bool moveExistingItemToTop = false)
        {
            OneListItem existingItem = this.Items.FirstOrDefault(x => x.HaveTheSameItemAndVariant(itemToAdd));
            if (existingItem == null)
            {
                this.Items.Insert(0, itemToAdd);
            }
            else
            {
                existingItem.Quantity += itemToAdd.Quantity;

                if (moveExistingItemToTop)
                {
                    Items.Remove(existingItem);
                    Items.Insert(0, existingItem);
                }
            }
            CalculateBasket();
        }

        public void AddItemAtPosition(int position, OneListItem item)
        {
            this.Items.Insert(position, item);
        }

        public void RemoveItem(OneListItem itemToRemove)
        {
            if (itemToRemove == null || itemToRemove.Quantity == 1)
            {
                this.Items.Remove(itemToRemove);
            }
            else
            {
                itemToRemove.Quantity -= 1;
            }
            CalculateBasket();
        }

        public void RemoveItemAtPosition(int position)
        {
            this.Items.RemoveAt(position);
        }

        public OneListItem ItemGetByIds(string itemId, string variantId, string uomId)
        {
            if (string.IsNullOrEmpty(itemId))
                return null;

            foreach (OneListItem item in Items)
            {
                if (item.ItemId != itemId)
                    continue;

                if (string.IsNullOrEmpty(variantId) == false && item.VariantId != variantId)
                    continue;

                if (string.IsNullOrEmpty(uomId) == false && item.UnitOfMeasureId != uomId)
                    continue;

                return item;
            }
            return null;
        }

        public void CalculateBasket()
        {
            TotalAmount = 0m;
            foreach (OneListItem item in Items)
            {
                item.Amount = (item.Price * item.Quantity) - item.DiscountAmount;
                TotalAmount += item.Amount;
                TotalDiscAmount += item.DiscountAmount;
            }
            State = BasketState.Dirty;
        }

        public static decimal CalculateBasket(List<OneListItem> basketItems)
        {
            decimal basketAmount = 0m;
            foreach (OneListItem item in basketItems)
            {
                basketAmount += item.Amount;
            }
            return basketAmount;
        }

        public void Clear()
        {
            Id = string.Empty;
            Items.Clear();
            PublishedOffers.Clear();

            CalculateBasket();
        }

        public bool IsEmpty
        {
            get
            {
                if (Items.Count != 0)
                    return false;

                if (PublishedOffers.Count != 0)
                    return false;

                return true;
            }
        }

        public override string ToString()
        {
            return string.Format(@"Id:{0} Description:{1} ListType:{2} ExtType:{3}", Id, Description, ListType, ExternalType);
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OneListLink : Entity, IDisposable
    {
        [DataMember]
        public string CardId { get; set; }
        [DataMember]
        public LinkStatus Status { get; set; }
        [DataMember]
        public bool Owner { get; set; }

        [DataMember]
        public string Name { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    [Flags]
    public enum ListType
    {
        [EnumMember]
        Basket = 0,
        [EnumMember]
        Wish = 1,  //Wish list
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    [Flags]
    public enum LinkStatus
    {
        [EnumMember]
        Requesting = 0,
        [EnumMember]
        Active = 1,
        [EnumMember]
        Blocked = 2,
        [EnumMember]
        Remove = 3
    }

    public enum BasketState
    {
        Normal = 0,
        Dirty = 1,
        Calculating = 2,
        Updating = 3
    }
}
