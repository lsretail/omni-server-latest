using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ReplicateTransaction : Entity, IDisposable
    {
        public ReplicateTransaction(string id) : base(id)
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
                if (TransactionDiscountLines != null)
                    TransactionDiscountLines.Clear();
                if (TransactionLines != null)
                    TransactionLines.Clear();
            }
        }

        [DataMember]
        public int TransactionNo { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string TerminalId { get; set; }
        [DataMember]
        public string StaffId { get; set; }
        [DataMember]
        public int TransactionType { get; set; }
        [DataMember]
        public string ReceiptNo { get; set; }
        public DateTime TransDate { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public double CurrencyFactor { get; set; }
        [DataMember]
        public string BusinessTaxCode { get; set; }
        [DataMember]
        public string PriceGroupCode { get; set; }
        [DataMember]
        public string CustomerId { get; set; }
        [DataMember]
        public string CustDiscGroup { get; set; }
        [DataMember]
        public string MemberCardNo { get; set; }
        [DataMember]
        public string MemberPriceGroupCode { get; set; }
        [DataMember]
        public decimal ManualTotalDiscPercent { get; set; }
        [DataMember]
        public decimal ManualTotalAmount { get; set; }
        [DataMember]
        public bool IsConcluded { get; set; }

        [DataMember]
        public List<ReplicateTransactionLine> TransactionLines { get; set; }
        [DataMember]
        public List<ReplicateTransactionDiscountLine> TransactionDiscountLines { get; set; }
    }
}
