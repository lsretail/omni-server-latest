using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ReplicateTransactionLine : Entity, IDisposable
    {
        public ReplicateTransactionLine(string id) : base(id)
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
        public int LineNo { get; set; }
        [DataMember]
        public int LineType { get; set; }
        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public string Barcode { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public string VariantCode { get; set; }
        [DataMember]
        public string UnitOfMeasureCode { get; set; }
        [DataMember]
        public decimal NetPrice { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public decimal DiscountAmount { get; set; }
        [DataMember]
        public decimal DiscountPercent { get; set; }
        [DataMember]
        public decimal NetAmount { get; set; }
        [DataMember]
        public decimal TaxAmount { get; set; }
        [DataMember]
        public string TaxProductCode { get; set; }
        [DataMember]
        public string TaxBusinessCode { get; set; }
        [DataMember]
        public string CardOrCustomerNo { get; set; }
        [DataMember]
        public decimal ManualDiscountPercent { get; set; }
        [DataMember]
        public decimal ManualDiscountAmount { get; set; }
        [DataMember]
        public string DiscountInfoLine { get; set; }
        [DataMember]
        public string TotalDiscountInfoLine { get; set; }
        [DataMember]
        public decimal ManualPrice { get; set; }
        [DataMember]
        public decimal CurrencyFactor { get; set; }
        /// <summary>
        /// EntryStatus (normal, voided ..) 
        /// </summary>
        [DataMember]
        public int EntryStatus { get; set; }
        [DataMember]
        public bool IsReturnLine { get; set; }
        [DataMember]
        public string EFTNo { get; set; }
    }
}
