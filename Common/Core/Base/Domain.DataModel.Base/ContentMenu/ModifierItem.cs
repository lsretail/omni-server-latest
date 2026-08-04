using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.ContentMenu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2026")]
    public class ContentMenuModifierItem
    {
        [DataMember]
        public string ItemId { get; set; } = string.Empty;

        [DataMember]
        public string Description { get; set; } = string.Empty;

        [DataMember]
        public decimal MaxQuantity { get; set; }

        [DataMember]
        public decimal MinQuantity { get; set; }

        [DataMember]
        public bool AvailabilitySet { get; set; }

        [DataMember]
        public decimal Availability { get; set; }

        [DataMember]
        public string AvailabilityUOM { get; set; } = string.Empty;

        [DataMember]
        public decimal AvailabilityFactor { get; set; }

        [DataMember]
        public string Image { get; set; } = string.Empty;

        [DataMember]
        public string ModGroupId { get; set; } = string.Empty;

        [DataMember]
        public int SelQuantity { get; set; }

        [DataMember]
        public int DefaultQuantity { get; set; }

        [DataMember]
        public string ItemModPriceType { get; set; } = string.Empty;

        [DataMember]
        public decimal ItemModPriceAmount { get; set; }

        [DataMember]
        public string ModGroupSubCode { get; set; } = string.Empty;

        [DataMember]
        public string ItemModRequired { get; set; } = string.Empty;

        [DataMember]
        public string TextModifier { get; set; } = string.Empty;

        [DataMember]
        public string ItemModUom { get; set; } = string.Empty;

        [DataMember]
        public decimal UomFactor { get; set; }

        [DataMember]
        public bool DefaultSelection { get; set; }
    }
}
