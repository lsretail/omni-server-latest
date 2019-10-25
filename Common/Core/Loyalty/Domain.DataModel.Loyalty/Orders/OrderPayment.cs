using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Orders
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderPayment : IDisposable
    {
        public OrderPayment()
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
        /// <summary>
        /// Omni TenderType.<p/>
        /// Default mapping to NAV: 0=Cash, 1=Card, 2=Coupon, 3=Loyalty Points, 4=Gift Card<p/>
        /// Tendertype Mapping can be modified in LSOmni Database - Appsettings table - Key:TenderType_Mapping
        /// </summary>
        [DataMember]
        public string TenderType { get; set; }
        [DataMember]
        public string CardType { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public decimal CurrencyFactor { get; set; }
        [DataMember]
        public string AuthorizationCode { get; set; }
        [DataMember]
        public string TokenNumber { get; set; }
        [DataMember]
        public string ExternalReference { get; set; }
        [DataMember]
        public string CardNumber { get; set; }
        [DataMember]
        public PaymentType PaymentType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime PreApprovedValidDate { get; set; }
    }
}
