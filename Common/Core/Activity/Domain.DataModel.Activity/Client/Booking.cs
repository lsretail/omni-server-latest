using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Client
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class Booking : Entity, IDisposable
    {
        public Booking(string id) : base(id)
        {
            Description = string.Empty;
            ActivityNo = string.Empty;
            ItemNo = string.Empty;
            Status = string.Empty;
            PaymentStatus = string.Empty;
            Location = string.Empty;
            ClientNo = string.Empty;
            ClientName = string.Empty;
            Comment = string.Empty;
            AllowanceNo = string.Empty;
            PriceCurrency = string.Empty;
            CancelPolicy = string.Empty;
            CancelPolicyDescription = string.Empty;
        }

        public Booking() : this(string.Empty)
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
        public string Description { get; set; }
        [DataMember]
        public string ActivityNo { get; set; }
        [DataMember]
        public string ItemNo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateFrom { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateTo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TimeFrom { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TimeTo { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public decimal UnitPrice { get; set; }
        [DataMember]
        public decimal LineDiscountPercentage { get; set; }
        [DataMember]
        public decimal TotalAmount { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string PaymentStatus { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public string ClientNo { get; set; }
        [DataMember]
        public string ClientName { get; set; }
        [DataMember]
        public int NoOfPersons { get; set; }
        [DataMember]
        public decimal LineDiscountAmount { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public string AllowanceNo { get; set; }
        [DataMember]
        public string PriceCurrency { get; set; }
        [DataMember]
        public string CancelPolicy { get; set; }
        [DataMember]
        public string CancelPolicyDescription { get; set; }
        [DataMember]
        public decimal CancelAmount { get; set; }
    }
}
