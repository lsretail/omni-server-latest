using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Transactions
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class TaxLine : IDisposable
	{
        public TaxLine(string taxCode)
        {
            TaxCode = taxCode; //taxCode
            TaxDesription = string.Empty;
            TaxDesription = string.Empty;
            Amount = string.Empty;
            NetAmt = string.Empty;
            TaxAmount = string.Empty;

            Amt = 0.0M;
            NetAmnt = 0.0M;
            TaxAmt = 0.0M;
        }

        public TaxLine()
            : this(string.Empty)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        [DataMember]
		public string TaxCode { get; set; }
        [DataMember]
		public string TaxDesription { get; set; }
        [DataMember]
		public string TaxAmount { get; set; }
		[DataMember]
		public string NetAmt { get; set; }
		[DataMember]
		public string Amount { get; set; }
        [DataMember]
        public decimal Amt { get; set; }
        [DataMember]
        public decimal NetAmnt { get; set; }
        [DataMember]
        public decimal TaxAmt { get; set; }
    }
}
