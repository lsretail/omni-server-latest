using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Orders
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderPayment : IDisposable
    {
        public OrderPayment()
        {
            OrderId = string.Empty;
            LineNumber = 1;
            No = "1";
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
        public string OrderId { get; set; }
        [DataMember]
        public int LineNumber { get; set; }
        [DataMember]
        public string No { get; set; }
        [DataMember]
        public decimal PreApprovedAmount { get; set; }
        [DataMember]
        public decimal FinalizedAmount { get; set; }
        /// <summary>
        /// Omni TenderType.<p/>
        /// Default mapping to NAV: 0=Cash, 1=Card, 2=Coupon, 3=Loyalty Points<p/>
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
        public string AuthorisationCode { get; set; }
        [DataMember]
        public string CardNumber { get; set; }
        [DataMember]
        public DateTime PreApprovedValidDate { get; set; }
    }
}
