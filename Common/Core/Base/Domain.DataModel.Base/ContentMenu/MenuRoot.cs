using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.ContentMenu
{
    public class ContentMenuHierarchy
    {
        [DataMember]
        public string HTTPErrorCode { get; set; } = string.Empty;

        [DataMember]
        public string HierarcyCode { get; set; } = string.Empty;

        [DataMember]
        public string SalesType { get; set; } = string.Empty;

        [DataMember]
        public List<ContentMenu> DynamicContentMenu { get; set; } = new List<ContentMenu>();
    }
}