using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.ContentMenu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2026")]
    public class ContentMenuDealModGroupItem
    {
        [DataMember]
        public string ItemNo { get; set; } = string.Empty;

        [DataMember]
        public string Description { get; set; } = string.Empty;

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
        public string ParentDealNo { get; set; } = string.Empty;

        [DataMember]
        public List<ContentMenuItemUom> ItemUom { get; set; } = new List<ContentMenuItemUom>();

        [DataMember]
        public bool SelectedItem { get; set; }

        [DataMember]
        public int SelQuantity { get; set; }
    }
}
