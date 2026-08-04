using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.ContentMenu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2026")]
    public class ContentMenuOfferLine
    {
        [DataMember]
        public string ItemNo { get; set; } = string.Empty;

        [DataMember]
        public string Description { get; set; } = string.Empty;

        [DataMember]
        public string DealModGroupId { get; set; } = string.Empty;

        [DataMember]
        public string DealModGroupDescription { get; set; } = string.Empty;

        [DataMember]
        public string DealModDisplayPrompt { get; set; } = string.Empty;

        [DataMember]
        public decimal DealModGroupMaxQuantity { get; set; }

        [DataMember]
        public decimal DealModGroupMinQuantity { get; set; }

        [DataMember]
        public decimal Quantity { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public string ItemUOM { get; set; } = string.Empty;

        [DataMember]
        public string UoMDescription { get; set; } = string.Empty;

        [DataMember]
        public decimal UoMFactor { get; set; }

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
        public string OfferLineType { get; set; } = string.Empty;

        [DataMember]
        public int OfferLine { get; set; }

        [DataMember]
        public List<ContentMenuIngredient> Ingredients { get; set; } = new List<ContentMenuIngredient>();

        [DataMember]
        public List<ContentMenuModifierGroup> ModifierGroups { get; set; } = new List<ContentMenuModifierGroup>();

        [DataMember]
        public List<ContentMenuDealModGroupItem> DealModGroupItems { get; set; } = new List<ContentMenuDealModGroupItem>();
    }
}
