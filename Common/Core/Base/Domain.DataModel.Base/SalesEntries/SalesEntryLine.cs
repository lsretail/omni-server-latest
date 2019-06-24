using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.SalesEntries
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class SalesEntryLine : Entity, IDisposable
    {
        public SalesEntryLine(string id) : base(id)
        {
            LineNumber = 1;
            ItemId = string.Empty;
            VariantId = string.Empty;
            ItemDescription = string.Empty;
            VariantDescription = string.Empty;
            UomId = string.Empty;
            Quantity = 1.0M;

            LineType = LineType.Item; //never change this unless you know what you are doing !

            NetPrice = 0.0M;
            Price = 0.0M;
            DiscountAmount = 0.0M;
            DiscountPercent = 0.0M;
            NetAmount = 0.0M;
            TaxAmount = 0.0M;
            Amount = 0.0M;
        }

        public SalesEntryLine() : this(string.Empty)
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
        public int LineNumber { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string ItemDescription { get; set; }
        [DataMember]
        public string VariantDescription { get; set; }
        [DataMember]
        public string UomId { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public LineType LineType { get; set; }
        [DataMember]
        public decimal DiscountAmount { get; set; }
        [DataMember]
        public decimal DiscountPercent { get; set; }

        [DataMember]
        public decimal NetPrice { get; set; }
        [DataMember]
        public decimal NetAmount { get; set; }
        [DataMember]
        public decimal TaxAmount { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        /// <summary>
        /// NetAmount + TaxAmount
        /// </summary>
        [DataMember]
        public decimal Amount { get; set; }

        public override string ToString()
        {
            string s = string.Format("LineNumber: {0} ItemId: {1} VariantId: {2} UomId: {3} Quantity: {4} LineType: {5} Amount: {6}",
                LineNumber, ItemId, VariantId, UomId, Quantity, LineType.ToString(), Amount.ToString());
            return s;
        }
    }
}
