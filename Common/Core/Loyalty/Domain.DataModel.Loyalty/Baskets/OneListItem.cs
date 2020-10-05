using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Baskets
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OneListItem : Entity, IDisposable
    {
        public OneListItem(string id) : base(id)
        {
            OneListId = string.Empty;
            ItemId = string.Empty;
            ItemDescription = string.Empty;
            VariantId = string.Empty;
            VariantDescription = string.Empty;
            UnitOfMeasureId = string.Empty;
            Location = string.Empty;
            BarcodeId = string.Empty;
            CreateDate = DateTime.Now;
            Detail = string.Empty;
            Quantity = 0M;
            NetPrice = 0M;
            Price = 0M;
            NetAmount = 0M;
            TaxAmount = 0M;
            OnelistItemDiscounts = new List<OneListItemDiscount>();
            OnelistSubLines = new List<OneListItemSubLine>();
            VariantRegistration = new VariantRegistration();
        }

        public OneListItem() : this(string.Empty)
        {
        }

        public OneListItem(LoyItem item, decimal qty) : this("")
        {
            ItemId = item.Id;
            ItemDescription = item.Description;
            UnitOfMeasureId = item.SelectedUnitOfMeasure?.Id;
            Image = item.DefaultImage;
            Quantity = qty;
            Price = item.AmtFromVariantsAndUOM(item.SelectedVariant?.Id, item.SelectedUnitOfMeasure?.Id);
            Detail = item.Details;
            VariantRegistration = item.SelectedVariant;
            if (item.Locations.Count != 0)
            {
                Location = item.Locations[0].ShelfCode;
            }
            else
            {
                Location = string.Empty;
            }
            if (item.SelectedVariant != null)
            {
                VariantId = item.SelectedVariant.Id;
                VariantDescription = item.SelectedVariant.ToString();
            }
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
                if (OnelistItemDiscounts != null)
                    OnelistItemDiscounts.Clear();
                if (OnelistSubLines != null)
                    OnelistSubLines.Clear();
            }
        }

        [DataMember]
        public int LineNumber { get; set; }
        [DataMember(IsRequired = true)]
        public string ItemId { get; set; }
        [DataMember]
        public string Detail { get; set; }
        [DataMember]
        public string ItemDescription { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public string UnitOfMeasureId { get; set; }
        [DataMember]
        public string UnitOfMeasureDescription { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string VariantDescription { get; set; }

        [DataMember(IsRequired = true)]
        public decimal Quantity { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CreateDate { get; set; }

        [DataMember]
        public string BarcodeId { get; set; }

        [DataMember]
        public decimal NetPrice { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public bool PriceModified { get; set; }
        [DataMember]
        public decimal NetAmount { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public decimal TaxAmount { get; set; }
        [DataMember]
        public decimal DiscountAmount { get; set; }
        [DataMember]
        public decimal DiscountPercent { get; set; }
        [DataMember]
        public List<OneListItemDiscount> OnelistItemDiscounts { get; set; }
        [DataMember]
        public ImageView Image { get; set; }
        [DataMember]
        public VariantRegistration VariantRegistration { get; set; }

        /// <summary>
        /// Hospitality Deal (Hierarchy Leaf Type=Deal)
        /// </summary>
        [DataMember]
        public bool IsADeal { get; set; }
        /// <summary>
        /// Hospitality Sub Lines
        /// </summary>
        [DataMember]
        public List<OneListItemSubLine> OnelistSubLines { get; set; }

        public string OneListId { get; set; }

        public decimal GetDiscount()
        {
            decimal amt = 0M;
            foreach (OneListItemDiscount item in OnelistItemDiscounts)
            {
                amt += item.DiscountAmount;
            }
            return amt;
        }

        public bool HaveTheSameItemAndVariant(OneListItem itemToCompare)
        {
            if (itemToCompare == null)
                return false;

            // Compare item
            if (ItemId != itemToCompare.ItemId)
                return false;

            // Compare variants
            if (VariantId != itemToCompare.VariantId)
                return false;

            // Compare UOMs
            if (UnitOfMeasureId != itemToCompare.UnitOfMeasureId)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public virtual object Clone()
        {
            // Shallow copy here should be enough (just keep the references to item, variant, uom)
            return this.MemberwiseClone();
        }

        public override string ToString()
        {
            return string.Format(@"Id:{0}  Qty:{1}  Item:{2}  CreateDate:{3}  Variant:{4}  Uom:{5}  BarcodeId:{6}",
                 Id, Quantity, ItemId, CreateDate, VariantId, UnitOfMeasureId, BarcodeId);
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OneListPublishedOffer : Entity, IDisposable
    {
        public OneListPublishedOffer() : this(string.Empty)
        {
        }

        public OneListPublishedOffer(string id) : base(id)
        {
            OneListId = "";
            CreateDate = DateTime.Now;
            LineNumber = 1;
            Type = OfferDiscountType.Unknown;
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
            }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CreateDate { get; set; }
        [DataMember]
        public int LineNumber { get; set; }

        //not a data member
        public string OneListId { get; set; }
        
        [DataMember]
        public OfferDiscountType Type { get; set; }
    }
}
