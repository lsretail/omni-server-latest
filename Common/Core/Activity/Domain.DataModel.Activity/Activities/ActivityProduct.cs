using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Activities
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class ActivityProduct : Entity, IDisposable
    {
        public ActivityProduct(string id) : base(id)
        {
            Description = string.Empty;
            ActivityType = string.Empty;
            PricedPerPerson = string.Empty;
            RetailItem = string.Empty;
            QuantityCaption = string.Empty;
            PaymentRequired = string.Empty;
            PriceCurrency = string.Empty;
            CancelPolicy = string.Empty;
            CancelPolicyDescription = string.Empty;
            ProductType = string.Empty;
            FixedLocation = string.Empty;
        }

        public ActivityProduct() : this(string.Empty)
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
        public string ActivityType { get; set; }
        [DataMember]
        public string RetailItem { get; set; }
        [DataMember]
        public decimal DefaultQty { get; set; }
        [DataMember]
        public string PricedPerPerson { get; set; }
        [DataMember]
        public bool AllowQuantityChange { get; set; }
        [DataMember]
        public string QuantityCaption { get; set; }
        [DataMember]
        public bool AllowNoOfPersonChange { get; set; }
        [DataMember]
        public int MinQty { get; set; }
        [DataMember]
        public int MaxQty { get; set; }
        [DataMember]
        public int MinPersons { get; set; }
        [DataMember]
        public int MaxPersons { get; set; }
        [DataMember]
        public string PaymentRequired { get; set; }
        [DataMember]
        public decimal DefaultUnitPrice { get; set; }
        [DataMember]
        public string PriceCurrency { get; set; }
        [DataMember]
        public string CancelPolicy { get; set; }
        [DataMember]
        public string CancelPolicyDescription { get; set; }
        [DataMember]
        public string ProductType { get; set; }
        [DataMember]
        public string FixedLocation { get; set; }
    }
}
