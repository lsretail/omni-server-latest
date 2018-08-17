using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class WebServiceFault
    {
        public WebServiceFault()
        {
            FaultCode = 0;
            FaultMessage = "";
        }

        [DataMember]
        public int FaultCode { get; set; }
        [DataMember]
        public string FaultMessage { get; set; }

    }
}
