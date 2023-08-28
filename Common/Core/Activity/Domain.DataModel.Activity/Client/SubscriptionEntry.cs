using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Client
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class SubscriptionEntry : Entity, IDisposable
    {
        public SubscriptionEntry(string id) : base(id)
        {
            ItemNo = string.Empty;
            ContactNo = string.Empty;
            ProductType = string.Empty;
            Description = string.Empty;
            Comment = string.Empty;
            InvoiceNo = string.Empty;
            MembershipNo = string.Empty;
            MembershipType = string.Empty;
            PostingDate = DateTime.MinValue.ToUniversalTime();
            PeriodFrom = DateTime.MinValue.ToUniversalTime();
            PeriodTo = DateTime.MinValue.ToUniversalTime();
        }

        public SubscriptionEntry() : this(string.Empty)
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
        public string ItemNo { get; set; }
        [DataMember]
        public string ContactNo { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public decimal Discount { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public string ProductType { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public string InvoiceNo { get; set; }
        [DataMember]
        public string MembershipNo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime PostingDate { get; set; }
        [DataMember]
        public decimal AdditionalCharges { get; set; }
        [DataMember]
        public string MembershipType { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime PeriodFrom { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime PeriodTo { get; set; }
    }
}
