using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Spg/2021")]
    public class ClientTokenResult
    {
        [DataMember]
        public string Token { get; set; }
        [DataMember]
        public DateTime ExpireDate { get; set; }
        [DataMember]
        public string Result { get; set; }
    }
}
