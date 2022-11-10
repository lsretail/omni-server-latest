using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Spg/2021")]
    public class ClientToken
    {
        [DataMember]
        public bool HotelToken { get; set; }
        [DataMember]
        public string Token { get; set; }
        [DataMember]
        public string TokenId { get; set; }
        [DataMember]
        public bool DefaultToken { get; set; }
        [DataMember]
        public string Result { get; set; }

        [DataMember]
        public int EntryNo { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string pSPID { get; set; }
        [DataMember]
        public string CardMask { get; set; }
        [DataMember]
        public string CustomerId { get; set; }
        [DataMember]
        public string AccountNo { get; set; }
        [DataMember]
        public string ContactNo { get; set; }
        [DataMember]
        public string ContactType { get; set; }
        [DataMember]
        public string CardNo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime Created { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime ExpiryDate { get; set; }
    }
}
