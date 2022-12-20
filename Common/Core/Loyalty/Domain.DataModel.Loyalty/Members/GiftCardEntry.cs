using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Members
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class GiftCardEntry : IDisposable
    {
        [DataMember]
        public string VoucherNo { get; set; }
        [DataMember]
        public string StoreNo { get; set; }
        [DataMember]
        public string TerminalNo { get; set; }
        [DataMember]
        public int TransactionNo { get; set; }
        [DataMember]
        public int LineNo { get; set; }
        [DataMember]
        public string ReceiptNumber { get; set; }
        [DataMember]
        public string EntryType { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime RegTime { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public bool Voided { get; set; }
        [DataMember]
        public decimal RemainingAmountNow { get; set; }
        [DataMember]
        public string VoucherType { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public decimal CurrencyFactor { get; set; }
        [DataMember]
        public string StoreCurrencyCode { get; set; }
        [DataMember]
        public decimal AmountInStoreCurrency { get; set; }

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
    }
}