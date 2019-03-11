using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Items
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class LoyItem : Entity, IDisposable
    {
        private string details;

        public LoyItem(string id) : base(id)
        {
            ProductGroupId = string.Empty;
            Description = string.Empty;
            Details = string.Empty;

            Price = string.Empty;
            SalesUomId = string.Empty;
            AllowedToSell = false;

            Prices = new List<Price>();
            VariantsExt = new List<VariantExt>(); //Variant extended
            VariantsRegistration = new List<VariantRegistration>();
            ItemAttributes = new List<RetailAttribute>();
            UnitOfMeasures = new List<UnitOfMeasure>();
            Images = new List<ImageView>();

#if WCFSERVER  
            ImgBytes = null;
            QtyNotInDecimal = true;
#endif
        }

        public LoyItem() : this(string.Empty)
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
                if (Images != null)
                    Images.Clear();
                if (Prices != null)
                    Prices.Clear();
                if (VariantsExt != null)
                    VariantsExt.Clear();
                if (VariantsRegistration != null)
                    VariantsRegistration.Clear();
                if (ItemAttributes != null)
                    ItemAttributes.Clear();
                if (UnitOfMeasures != null)
                    UnitOfMeasures.Clear();
            }
        }

        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Details
        {
            get { return details; }
            set
            {
                if (value == null)
                    details = string.Empty;
                else
                    details = Regex.Replace(Regex.Replace(value, "<[^>]*(>|$)", string.Empty), @"\r\n\r\n\r\n", Environment.NewLine).Trim();
            }
        }

        [DataMember]
        public List<Price> Prices { get; set; }
        [DataMember]
        public List<VariantExt> VariantsExt { get; set; }
        [DataMember]
        public List<VariantRegistration> VariantsRegistration { get; set; }
        [DataMember]
        public List<RetailAttribute> ItemAttributes { get; set; }
        [DataMember]
        public List<UnitOfMeasure> UnitOfMeasures { get; set; }
        [DataMember]
        public List<ImageView> Images { get; set; }
        [DataMember]
        public string ProductGroupId { get; set; }
        [DataMember]
        public string SalesUomId { get; set; }
        [DataMember]
        public string Price { get; set; }
        [DataMember]
        public bool AllowedToSell { get; set; } // Blocked on POS
        [DataMember]
        public bool Blocked { get; set; }
        [DataMember]
        public bool BlockDiscount { get; set; }
        [DataMember]
        public bool BlockManualPriceChange { get; set; }

        [DataMember]
        public decimal GrossWeight { get; set; }
        [DataMember]
        public string SeasonCode { get; set; }
        [DataMember]
        public string ItemCategoryCode { get; set; }
        [DataMember]
        public string ItemFamilyCode { get; set; }
        [DataMember]
        public decimal UnitsPerParcel { get; set; }
        [DataMember]
        public decimal UnitVolume { get; set; }

#if WCFSERVER
        //not all data goes to wcf clients
        public byte[] ImgBytes { get; set; }
        public bool QtyNotInDecimal { get; set; }
#endif

        public ImageView DefaultImage
        {
            get { return (Images != null && Images.Count > 0) ? Images[0] : null; }
        }

        public string PriceFromVariantsAndUOM(VariantRegistration variant, UnitOfMeasure uom)
        {
            return PriceFromVariantsAndUOM(variant?.Id, uom?.Id);
        }

        public string PriceFromVariantsAndUOM(string variant, string uom)
        {
            if (Prices == null)
                return string.Empty;
            if (variant == null)
                variant = string.Empty;
            if (uom == null)
                uom = string.Empty;

            if (Prices.Count == 1)
            {
                return Prices[0].Amount;
            }
            else if (Prices.Count > 1)
            {
                foreach (Price price in Prices)
                {
                    if (variant.Equals(price.VariantId) && uom.Equals(price.UomId))
                    {
                        return price.Amount;
                    }
                    if ((variant.Equals(price.VariantId) && price.UomId.Equals("")) || (price.VariantId.Equals("") && uom.Equals(price.UomId)))
                    {
                        return price.Amount;
                    }
                    if (price.VariantId.Equals("") && price.UomId.Equals(""))
                    {
                        return price.Amount;
                    }
                }
            }
            return string.Empty;
        }

        public decimal AmtFromVariantsAndUOM(string variantId, string uomId)
        {
            if (Prices == null)
                return 0m;
            if (variantId == null)
                variantId = string.Empty;
            if (uomId == null)
                uomId = string.Empty;

            if (Prices.Count == 1)
            {
                return Prices[0].Amt;
            }
            else if (Prices.Count > 1)
            {
                foreach (Price price in Prices)
                {
                    if (variantId == price.VariantId && uomId == price.UomId)
                    {
                        return price.Amt;
                    }
                    if ((variantId == price.VariantId && string.IsNullOrEmpty(price.UomId)) || (string.IsNullOrEmpty(price.VariantId) && uomId.Equals(price.UomId)))
                    {
                        return price.Amt;
                    }
                    if (string.IsNullOrEmpty(price.VariantId) && string.IsNullOrEmpty(price.UomId))
                    {
                        return price.Amt;
                    }
                }
            }
            return 0m;
        }

        public string FormatQty(decimal qty)
        {
            string returnString = string.Empty;
            if (UnitOfMeasures.Count == 0)
                returnString += qty.ToString("N0");
            else
                returnString += qty.ToString("N" + UnitOfMeasures[0].Decimals) + " " + UnitOfMeasures[0].Description;
            return returnString;
        }

        public LoyItem ShallowCopy()
        {
            return (LoyItem)MemberwiseClone();
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine("Id: " + Id);
            str.AppendLine("Desc: " + Description);
            str.AppendLine("Product Group: " + ProductGroupId);
            str.AppendLine("Sales UOM: " + SalesUomId);
            str.AppendLine("Allowed to Sell: " + AllowedToSell.ToString());
            str.AppendLine(string.Empty);
            str.AppendLine("Details:");
            str.AppendLine(Details);

            str.AppendLine(string.Empty);
            str.AppendLine("Prices:");
            foreach (Price p in Prices)
                str.AppendLine("-> " + p.ToString());
            str.AppendLine(string.Empty);
            str.AppendLine("Unit of Measures:");
            foreach (UnitOfMeasure u in UnitOfMeasures)
                str.AppendLine("-> " + u.ToString());
            str.AppendLine(string.Empty);
            str.AppendLine("Variant Extentions:");
            foreach (VariantExt v in VariantsExt)
                str.AppendLine("-> " + v.ToString());
            str.AppendLine(string.Empty);
            str.AppendLine("Variant Registrations:");
            foreach (VariantRegistration v in VariantsRegistration)
                str.AppendLine("-> " + v.ToString());
            str.AppendLine(string.Empty);
            str.AppendLine("Item Attributes:");
            foreach (RetailAttribute a in ItemAttributes)
                str.AppendLine("-> " + a.ToString());

            return str.ToString();
        }
    }
}
