using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Client
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class ResHeader : Entity, IDisposable
    {
        public ResHeader(string id) : base(id)
        {
            Description = string.Empty;
            Status = string.Empty;
            PaymentStatus = string.Empty;
            InternalStatus = string.Empty;
            Location = string.Empty;
            ContactNo = string.Empty;
            ContactName = string.Empty;
            Comment = string.Empty;
            ReservationType = string.Empty;
            InternalContact = string.Empty;
            CustomerAccount = string.Empty;
            Comment = string.Empty;
            EMail = string.Empty;
            Reference = string.Empty;
            MobileNo = string.Empty;
            Language = string.Empty;
            GroupNo = string.Empty;
        }

        public ResHeader() : this(string.Empty)
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
        public string ContactNo { get; set; }
        [DataMember]
        public string ContactName { get; set; }
        [DataMember]
        public string ReservationType { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateFrom { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateTo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TimeFrom { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TimeTo { get; set; }
        [DataMember]
        public decimal NoOfPersons { get; set; }
        [DataMember]
        public decimal TotalActivityCharges { get; set; }
        [DataMember]
        public decimal TotalAdditionalCharges { get; set; }
        [DataMember]
        public decimal DepositsBalance { get; set; }
        [DataMember]
        public decimal POSSale { get; set; }
        [DataMember]
        public string Balance { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string InternalStatus { get; set; }
        [DataMember]
        public string PaymentStatus { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public string InternalContact { get; set; }
        [DataMember]
        public string CustomerAccount { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public string EMail { get; set; }
        [DataMember]
        public string Reference { get; set; }
        [DataMember]
        public string MobileNo { get; set; }
        [DataMember]
        public string Language { get; set; }
        [DataMember]
        public string GroupNo { get; set; }
    }
}
