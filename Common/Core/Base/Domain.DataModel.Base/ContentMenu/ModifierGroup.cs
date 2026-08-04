using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.ContentMenu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2026")]
    public class ContentMenuModifierGroup
    {
        [DataMember]
        public string ModGroupId { get; set; } = string.Empty;

        [DataMember]
        public string ModGroupDescription { get; set; } = string.Empty;

        [DataMember]
        public decimal ModGroupMaxQuantity { get; set; }

        [DataMember]
        public decimal ModGroupMinQuantity { get; set; }

        [DataMember]
        public string ModGroupDisplayPrompt { get; set; } = string.Empty;

        [DataMember]
        public string Image { get; set; } = string.Empty;

        [DataMember]
        public int FilterByParentUOM { get; set; }

        [DataMember]
        public List<ContentMenuModifierItem> ModifierItems { get; set; } = new List<ContentMenuModifierItem>();

        [DataMember]
        public int SelQuantity { get; set; }
    }
}
