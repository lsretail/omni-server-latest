using System;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Baskets
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OneListItemDiscount : Entity, IDisposable
    {
        public OneListItemDiscount(string id) : base(id)
        {
            OneListId = string.Empty;
            OneListItemId = string.Empty;
            LineNumber = 0;
            No = string.Empty;
            DiscountType = DiscountType.Unknown;
            PeriodicDiscType = PeriodicDiscType.Unknown;
            PeriodicDiscGroup = string.Empty;
            Description = string.Empty;
            DiscountAmount = 0M;
            DiscountPercent = 0M;
            Quantity = 1;
            OfferNumber = string.Empty;
            ItemId = string.Empty;
            VariantId = string.Empty;
        }

        public OneListItemDiscount() : this(string.Empty)
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
        public string No { get; set; }
        [DataMember]
        public DiscountType DiscountType { get; set; }
        [DataMember]
        public PeriodicDiscType PeriodicDiscType { get; set; }
        [DataMember]
        public string PeriodicDiscGroup { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string OfferNumber { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public decimal DiscountAmount { get; set; }
        [DataMember]
        public decimal DiscountPercent { get; set; }

        public string OneListId { get; set; }
        public string OneListItemId { get; set; }
        public string ItemId { get; set; }
        public string VariantId { get; set; }
    }
}
