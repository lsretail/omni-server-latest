using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.ContentMenu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2026")]
    public class ContentMenuItem
    {
        // Common fields (many nodes share these)
        [DataMember]
        public string NodeType { get; set; } = string.Empty;

        [DataMember]
        public string ItemNo { get; set; } = string.Empty;

        [DataMember]
        public string DealNo { get; set; } = string.Empty;

        [DataMember]
        public string Description { get; set; } = string.Empty;

        [DataMember]
        public string SecondDescription { get; set; } = string.Empty;

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public string ItemUOM { get; set; } = string.Empty;

        [DataMember]
        public string UoMDescription { get; set; } = string.Empty;

        [DataMember]
        public decimal UoMFactor { get; set; }

        [DataMember]
        public bool AvailabilitySet { get; set; }

        [DataMember]
        public decimal Availability { get; set; }

        [DataMember]
        public string AvailabilityUOM { get; set; } = string.Empty;

        [DataMember]
        public decimal AvailabilityFactor { get; set; }

        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public string Image { get; set; } = string.Empty;

        [DataMember]
        public string Parent { get; set; } = string.Empty;

        [DataMember]
        public List<ContentMenuOfferLine> OfferLines { get; set; } = new List<ContentMenuOfferLine>();

        [DataMember]
        public List<ContentMenuIngredient> Ingredients { get; set; } = new List<ContentMenuIngredient>();

        [DataMember]
        public List<ContentMenuModifierGroup> ModifierGroups { get; set; } = new List<ContentMenuModifierGroup>();
    }
}
