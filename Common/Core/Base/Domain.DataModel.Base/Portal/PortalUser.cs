using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LSRetail.Omni.Domain.DataModel.Base.Portal
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class PortalUser
    {
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public bool Active { get; set; }
        [DataMember]
        public bool Admin { get; set; }
        [DataMember]
        public List<LSKey> LSKeys { get; set; }
        [DataMember]
        public string Token { get; set; }
    }
}
