using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.SalesEntries
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class SalesEntryPayment : IDisposable
    {
        public SalesEntryPayment()
        {
            LineNumber = 1;
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
        public int LineNumber { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public PaymentType Type { get; set; }
        /// <summary>
        /// Omni TenderType.<p/>
        /// Default mapping to NAV: 0=Cash, 1=Card, 2=Coupon, 3=Loyalty Points, 4=Gift Card<p/>
        /// Tender type Mapping can be modified in LSOmni Database - TenantConfig table - Key:TenderType_Mapping
        /// </summary>
        [DataMember]
        public string TenderType { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public decimal CurrencyFactor { get; set; }
        [DataMember]
        public string CardNo { get; set; }
    }
}
