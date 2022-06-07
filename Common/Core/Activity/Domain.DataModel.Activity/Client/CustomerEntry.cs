using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Client
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class CustomerEntry : Entity, IDisposable
    {
        public CustomerEntry(string id) : base(id)
        {
            Description = string.Empty;
            CustomerNo = string.Empty;
            DocumentType = string.Empty;
            DocumentNo = string.Empty;
            Currency = string.Empty;
            ExternalRef = string.Empty;
            ContactNo = string.Empty;
        }

        public CustomerEntry() : this(string.Empty)
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
        public string CustomerNo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime PostingDate { get; set; }
        [DataMember]
        public string DocumentType { get; set; }
        [DataMember]
        public string DocumentNo { get; set; }
        [DataMember]
        public string Currency { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public decimal AmountLCY { get; set; }
        [DataMember]
        public string ExternalRef { get; set; }
        [DataMember]
        public string ContactNo { get; set; }
    }
}
