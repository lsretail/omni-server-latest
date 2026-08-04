using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.ContentMenu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2026")]
    public class ContentMenuItemUom
    {
        [DataMember]
        public int DefaultQuantity { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public string ItemUOM { get; set; } = string.Empty;

        [DataMember]
        public string UoMDescription { get; set; } = string.Empty;

        [DataMember]
        public decimal MaxQuantity { get; set; }

        [DataMember]
        public decimal MinQuantity { get; set; }

        [DataMember]
        public int DealLine { get; set; }

        [DataMember]
        public int DealModLine { get; set; }

        [DataMember]
        public decimal UoMFactor { get; set; }

        [DataMember]
        public bool Default { get; set; }

        [DataMember]
        public bool SelectedItem { get; set; }

        [DataMember]
        public int SelQuantity { get; set; }
    }
}
