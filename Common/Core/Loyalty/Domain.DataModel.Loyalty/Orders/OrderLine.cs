using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Orders
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderLine : Entity, IDisposable
    {
        public OrderLine(string id) : base(id)
        {
            OrderId = string.Empty;
            LineNumber = 1;
            ItemId = string.Empty;
            VariantId = string.Empty;
            ItemDescription = string.Empty;
            VariantDescription = string.Empty;
            UomId = string.Empty;
            ItemImageId = string.Empty;
            StoreId = string.Empty;

            LineType = LineType.Item; //never change this unless you know what you are doing !

            Quantity = 1.0M;

            NetPrice = 0.0M;
            Price = 0.0M;
            DiscountAmount = 0.0M;
            DiscountPercent = 0.0M;
            NetAmount = 0.0M;
            TaxAmount = 0.0M;
            Amount = 0.0M;

            LineNumbers = new List<int>();
        }

        public OrderLine() : this(string.Empty)
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

        /// <summary>
        /// Order Reference 
        /// </summary>
        [DataMember]
        public string OrderId { get; set; }
        [DataMember]
        public int LineNumber { get; set; }
        [DataMember(IsRequired = true)]
        public string ItemId { get; set; }
        [DataMember(IsRequired = true)]
        public string VariantId { get; set; }
        [DataMember]
        public string ItemDescription { get; set; }
        [DataMember]
        public string VariantDescription { get; set; }
        [DataMember]
        public string UomId { get; set; }
        /// <summary>
        /// Store to collect the item from
        /// </summary>
        [DataMember]
        public string StoreId { get; set; }
        /// <summary>
        /// Click and Collect this line at Store Id set in the line
        /// </summary>
        [DataMember]
        public bool ClickAndCollectLine { get; set; }
        [DataMember]
        public string ItemImageId { get; set; }
        [DataMember(IsRequired = true)]
        public decimal Quantity { get; set; }
        [DataMember(IsRequired = true)]
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

        public List<int> LineNumbers { get; set; }

        public override string ToString()
        {
            return string.Format("LineNumber: {0} ItemId: {1} VariantId: {2} UomId: {3} Quantity: {4} LineType: {5} Amount: {6}",
                LineNumber, ItemId, VariantId, UomId, Quantity, LineType, Amount);
        }
    }
}
