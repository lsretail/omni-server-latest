using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.SalesEntries
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class SalesEntryDiscountLine : Entity, IDisposable
    {
        public SalesEntryDiscountLine(string id) : base(id)
        {
            OrderId = string.Empty;
            LineNumber = 1;
            No = "1";
            DiscountType = DiscountType.Line;
            OfferNumber = string.Empty;
            PeriodicDiscType = PeriodicDiscType.Unknown;
            PeriodicDiscGroup = string.Empty;
            Description = string.Empty;
            DiscountAmount = 0M;
            DiscountPercent = 0M;
        }

        public SalesEntryDiscountLine() : this(string.Empty)
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
        public string OrderId { get; set; }
        [DataMember]
        public int LineNumber { get; set; }
        /// <summary>
        /// Line + No identifies the item discount
        /// </summary>
        [DataMember]
        public string No { get; set; }
        [DataMember]
        public DiscountType DiscountType { get; set; }
        [DataMember]
        public string OfferNumber { get; set; }
        [DataMember]
        public PeriodicDiscType PeriodicDiscType { get; set; }
        [DataMember]
        public string PeriodicDiscGroup { get; set; }
        [DataMember]
        public decimal DiscountAmount { get; set; }
        [DataMember]
        public decimal DiscountPercent { get; set; }
        [DataMember]
        public string Description { get; set; }

        public override string ToString()
        {
            string s = string.Format(@"OrderId: {0} LineNumber: {1} No: {2}  DiscountType: {3}  PeriodicDiscType: {4} OfferNumber: {5}
            DiscountAmount: {6} PeriodicDiscGroup: {7} DiscountPercent: {8} Description: {9}",
                OrderId, LineNumber, No, DiscountType, PeriodicDiscType, OfferNumber, DiscountAmount, PeriodicDiscGroup, DiscountPercent, Description);
            return s;
        }
    }
}
