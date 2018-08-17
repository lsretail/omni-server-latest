using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Transactions
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
	public class LoyTransactionHeader : BaseLine
	{
        public LoyTransactionHeader()
        {
           HeaderDescription = string.Empty;
        }
    
        [DataMember]
		public string HeaderDescription { get; set; }

	}

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
	public class LoyTransactionFooter : BaseLine
	{
        public LoyTransactionFooter()
        {
           FooterDescription = string.Empty;
        }
        [DataMember]
		public string FooterDescription { get; set; }
	}
}
