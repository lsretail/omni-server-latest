using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2020")]
    public class ReturnPolicy
    {
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string ItemCategory { get; set; }
        [DataMember]
        public string ProductGroup { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantDimension1 { get; set; }
        [DataMember]
        public string VariantCode { get; set; }
        [DataMember]
        public string StoreGroup { get; set; }
        [DataMember]
        public bool RefundNotAllowed { get; set; }
        [DataMember]
        public bool ManagerPrivileges { get; set; }
        [DataMember]
        public string RefundPeriodLength { get; set; }
        [DataMember]
        public string Message1 { get; set; }
        [DataMember]
        public string Message2 { get; set; }
        [DataMember]
        public string ReturnPolicyHTML { get; set; }
    }
}
