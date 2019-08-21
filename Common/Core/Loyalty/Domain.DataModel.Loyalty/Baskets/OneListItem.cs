using System;
using System.Collections.Generic;
using System.Linq;
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
            Quantity = 0M;
            Item = new LoyItem();
            CreateDate = DateTime.Now;
            DisplayOrderId = 1;
            VariantReg = null;
            UnitOfMeasure = null;
            BarcodeId = string.Empty;
            NetPrice = 0M;
            Price = 0M;
            NetAmount = 0M;
            TaxAmount = 0M;
            OnelistItemDiscounts = new List<OneListItemDiscount>();
        }

        public OneListItem() : this(string.Empty)
        {
        }

        public OneListItem(LoyItem item, decimal quantity, string uomId, string variantId) : this("")
        {
            Item = item;
            Quantity = quantity;

            if (!string.IsNullOrEmpty(uomId))
            {
                UnitOfMeasure = item.UnitOfMeasures.FirstOrDefault(x => x.Id == uomId);
            }

            if (!string.IsNullOrEmpty(variantId))
            {
                VariantReg = item.VariantsRegistration.FirstOrDefault(x => x.Id == variantId);
            }

            Price = item.AmtFromVariantsAndUOM(variantId, uomId);
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
                if (Item != null)
                    Item.Dispose();
                if (VariantReg != null)
                    VariantReg.Dispose();
                if (UnitOfMeasure != null)
                    UnitOfMeasure.Dispose();
            }
        }

        [DataMember(IsRequired = true)]
        public decimal Quantity { get; set; }
        [DataMember(IsRequired = true)]
        public LoyItem Item { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CreateDate { get; set; }
        [DataMember]
        public VariantRegistration VariantReg { get; set; }
        [DataMember]
        public UnitOfMeasure UnitOfMeasure { get; set; }
        [DataMember]
        public string BarcodeId { get; set; }
        [DataMember]
        public int DisplayOrderId { get; set; }
        [DataMember]
        public decimal NetPrice { get; set; }
        [DataMember]
        public decimal Price { get; set; }
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
        public List<OneListItemDiscount> OnelistItemDiscounts { get; set; } // decimal got truncated

        //not a data member
        public string OneListId { get; set; }
        public string ItemId { get; set; }
        public string UnitOfMeasureId { get; set; }
        public string VariantId { get; set; }

        public ImageView Image
        {
            get
            {
                if (VariantReg != null && VariantReg.Images != null && VariantReg.Images.Count > 0)
                {
                    return VariantReg.Images[0];
                }
                return Item.DefaultImage;
            }
        }

        public string FormatQuantity(decimal qty)
        {
            string returnString = "";

            if (UnitOfMeasure == null)
            {
                returnString += qty.ToString("N0");
            }
            else
            {
                string formatString = "0";
                if (UnitOfMeasure.Decimals > 0)
                {
                    formatString += ".";
                    for (int i = 0; i < UnitOfMeasure.Decimals; i++)
                    {
                        formatString += "#";
                    }
                }
                returnString = qty.ToString(formatString) + " " + UnitOfMeasure.ShortDescription.ToLower();
            }
            return returnString;
        }

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
            if (this.Item.Id != itemToCompare.Item.Id)
                return false;

            // Compare variants
            if (this.VariantReg != null && itemToCompare.VariantReg != null)
            {
                // Both items have variants, must compare them
                if (this.VariantReg.Id != itemToCompare.VariantReg.Id)
                    return false;
            }
            else if (this.VariantReg == null && itemToCompare.VariantReg == null)
            {
                // Neither has a variant, no need to compare
            }
            else
            {
                // One item has a variant, the other doesn't
                return false;
            }

            // Compare UOMs
            if (this.UnitOfMeasure != null && itemToCompare.UnitOfMeasure != null)
            {
                // Both items have UOMs, must compare them
                if (this.UnitOfMeasure.Id != itemToCompare.UnitOfMeasure.Id)
                    return false;
            }
            else if (this.UnitOfMeasure == null && itemToCompare.UnitOfMeasure == null)
            {
                // Neither has a UOM, no need to compare
            }
            else
            {
                // One item has a UOM, the other doesn't
                return false;
            }
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
            string item = (Item != null ? Item.ToString() : "");
            string variant = (VariantReg != null ? VariantReg.ToString() : "");
            string uom = (UnitOfMeasure != null ? UnitOfMeasure.ToString() : "");

            return string.Format(@"Id: {0} Quantity: {1}  Item: {2}  CreateDate: {3} Variant: {4}  Uom: {5} BarcodeId: {6}",
                 Id, Quantity, item, CreateDate, variant, uom, BarcodeId);
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
            DisplayOrderId = 1;
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
        public int DisplayOrderId { get; set; }

        //not a data member
        public string OneListId { get; set; }
        
        [DataMember]
        public OfferDiscountType Type { get; set; }
    }
}
