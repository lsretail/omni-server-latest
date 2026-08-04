using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.ContentMenu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2026")]
    public class ContentMenu
    {
        // Common fields (many nodes share these)
        [DataMember]
        public string NodeType { get; set; } = string.Empty;

        [DataMember]
        public string NodeID { get; set; } = string.Empty;

        [DataMember]
        public string Description { get; set; } = string.Empty;

        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public string Image { get; set; } = string.Empty;

        [DataMember]
        public string Parent { get; set; } = string.Empty;

        [DataMember]
        public List<ContentMenuItem> MenuItems { get; set; } = new List<ContentMenuItem>();
    }
}
