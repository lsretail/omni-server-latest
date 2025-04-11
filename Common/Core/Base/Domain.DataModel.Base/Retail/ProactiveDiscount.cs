using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ProactiveDiscount
    {
        [DataMember]
        public ProactiveDiscountType Type { get; set; }
        [DataMember]
        public int Priority { get; set; }
        [DataMember]
        public decimal Percentage { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public decimal PriceWithDiscount { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string UnitOfMeasureId { get; set; }
        [DataMember]
        public List<string> ItemIds { get; set; }
        [DataMember]
        public List<string> BenefitItemIds { get; set; }
        [DataMember]
        public string LoyaltySchemeCode { get; set; }
        [DataMember]
        public decimal MinimumQuantity { get; set; }
        [DataMember]
        public string PeriodId { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string PopUpLine1 { get; set; }
        [DataMember]
        public string PopUpLine2 { get; set; }
        [DataMember]
        public string PopUpLine3 { get; set; }
        
        public string CustomerDiscGroup { get; set; }

        public void SetProactiveDiscountType(int type)
        {
            //NAV Types: 0 - Multibuy, 1 - Mix&Match, 2 - Disc. Offer
            switch (type)
            {
                case 0:
                    this.Type = ProactiveDiscountType.Multibuy; 
                    break;
                case 1:
                    this.Type = ProactiveDiscountType.MixMatch;
                    break;
                case 2:
                    this.Type = ProactiveDiscountType.DiscOffer;
                    break;
                case 3:
                    this.Type = ProactiveDiscountType.TotalDiscount;
                    break;
                case 4:
                    this.Type = ProactiveDiscountType.TenderType;
                    break;
                case 5:
                    this.Type = ProactiveDiscountType.ItemPoint;
                    break;
                case 6:
                    this.Type = ProactiveDiscountType.LineDiscount;
                    break;
            }
        }
    }

    /// <summary>
    /// NAV "Periodic Discount" type mapping: 0 - Multibuy, 1 - Mix and Match, 2 - Disc. Offer.
    /// This enum maps perfectly to PeriodicDiscType
    /// </summary>
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum ProactiveDiscountType
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        Multibuy = 1,
        [EnumMember]
        MixMatch = 2,
        [EnumMember]
        DiscOffer = 3,
        [EnumMember]
        ItemPoint = 4,
        [EnumMember]
        TotalDiscount = 5,
        [EnumMember]
        TenderType = 6,
        [EnumMember]
        LineDiscount = 7,
    }
}
