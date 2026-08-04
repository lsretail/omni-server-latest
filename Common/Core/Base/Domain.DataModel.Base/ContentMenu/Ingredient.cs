using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.ContentMenu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2026")]
    public class ContentMenuIngredient
    {
        [DataMember]
        public string ItemNo { get; set; } = string.Empty;

        [DataMember]
        public string Description { get; set; } = string.Empty;

        [DataMember]
        public decimal SelQuantity { get; set; }

        [DataMember]
        public decimal DefaultQuantity { get; set; }

        [DataMember]
        public bool IngredientExcludeable { get; set; }

        [DataMember]
        public string PriceReductionOnExclusion { get; set; } = string.Empty;

        [DataMember]
        public decimal PriceReduction { get; set; }

        [DataMember]
        public string ItemUOM { get; set; } = string.Empty;

        [DataMember]
        public string UoMDescription { get; set; } = string.Empty;

        [DataMember]
        public decimal MaxQuantity { get; set; }

        [DataMember]
        public decimal MinQuantity { get; set; }

        [DataMember]
        public string Image { get; set; } = string.Empty;
    }
}
