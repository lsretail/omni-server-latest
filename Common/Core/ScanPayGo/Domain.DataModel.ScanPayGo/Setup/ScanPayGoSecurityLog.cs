using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Spg/2021")]
    public class ScanPayGoSecurityLog
    {
        [DataMember]
        public bool Source { get; set; }
        [DataMember]
        public bool Check { get; set; }
    }
}
