using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
    public class ScanPayGoProfile : Entity
    {
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public SPGSecurityTrigger SecurityTrigger { get; set; }

        [DataMember]
        public List<ScanPayGoTender> TenderTypes { get; set; }

        [DataMember]
        public FeatureFlags Flags { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
    public class ScanPayGoTender : Entity
    {
        [DataMember]
        public string Description { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
    public enum SPGSecurityTrigger
    {
        [EnumMember]
        PaymentSelect = 0,
        [EnumMember]
        AfterPayment = 1,
        [EnumMember]
        Never = 2
    }
}
