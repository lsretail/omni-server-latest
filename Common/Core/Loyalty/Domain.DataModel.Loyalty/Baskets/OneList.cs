using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Baskets
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OneList : Entity, IDisposable
    {
        public OneList(string id) : base(id)
        {
            Description = string.Empty;
            CardId = string.Empty;
            CustomerId = string.Empty;
            StoreId = string.Empty;
            IsDefaultList = false;
            CreateDate = DateTime.Now;
            ListType = ListType.Basket; // basket, wish
            Items = new List<OneListItem>();
            PublishedOffers = new List<OneListPublishedOffer>();
            TotalAmount = 0M;
            TotalNetAmount = 0M;
            TotalTaxAmount = 0M;
            TotalDiscAmount = 0M;
            PointAmount = 0M;
        }

        public OneList(string id, List<OneListItem> items, bool calculate) : this(id)
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
        public string CustomerId { get; set; }
        [DataMember]
        public bool IsDefaultList { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CreateDate { get; set; }
        [DataMember(IsRequired = true)]
        public ListType ListType { get; set; }
        [DataMember]
        public List<OneListItem> Items { get; set; }
        [DataMember]
        public List<OneListPublishedOffer> PublishedOffers { get; set; }
        [DataMember]
        public decimal TotalAmount { get; set; }
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
            clone.Items = new List<OneListItem>();
            clone.Items.AddRange(Items);
            return clone;
        }

        public void AddItem(OneListItem itemToAdd)
        {
            OneListItem existingItem = this.Items.FirstOrDefault(x => x.HaveTheSameItemAndVariant(itemToAdd));
            if (existingItem == null)
            {
                this.Items.Insert(0, itemToAdd);
            }
            else
            {
                existingItem.Quantity += itemToAdd.Quantity;
            }
            CalculateBasket();
        }

        public void AddItemAtPosition(int position, OneListItem item)
        {
            this.Items.Insert(position, item);
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
                if (item.Item != null && item.Item.Id != itemId)
                    continue;

                if (string.IsNullOrEmpty(variantId) == false)
                {
                    if (item.VariantReg != null && item.VariantReg.Id != variantId)
                        continue;
                }

                if (string.IsNullOrEmpty(uomId) == false)
                {
                    if (item.UnitOfMeasure != null && item.UnitOfMeasure.Id != uomId)
                        continue;
                }
                return item;
            }
            return null;
        }

        public void CalculateBasket()
        {
            TotalAmount = 0m;
            foreach (OneListItem item in Items)
            {
                if (item.Item.Prices.Count() > 0)
                    item.Price = item.Item.AmtFromVariantsAndUOM(item.VariantReg?.Id, item.UnitOfMeasure?.Id);
                
                item.Amount = item.Price * item.Quantity;
                TotalAmount += item.Amount;
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
            Items.Clear();
            PublishedOffers.Clear();
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
            return string.Format(@"Id: {0} Description: {1}  IsDefaultList: {2}  CreateDate: {3}  ",
                 Id, Description, IsDefaultList, CreateDate);
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

    public enum BasketState
    {
        Normal = 0,
        Dirty = 1,
        Calculating = 2,
        Updating = 3
    }
}
