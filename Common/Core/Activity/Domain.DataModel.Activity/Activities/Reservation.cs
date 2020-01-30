using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Activities
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class Reservation : Entity, IDisposable
    {
        public Reservation(string id) : base(id)
        {
            ReservationType = string.Empty;
            CustomerAccount = string.Empty;
            Description = string.Empty;
            Comment = string.Empty;
            Reference = string.Empty;
            ContactNo = string.Empty;
            ContactName = string.Empty;
            Email = string.Empty;
            Location = string.Empty;
            SalesPerson = string.Empty;
            Status = string.Empty;
        }

        public Reservation() : this(string.Empty)
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

        [DataMember(IsRequired = true)]
        public string ReservationType { set; get; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime ResDateFrom { set; get; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime ResTimeFrom { set; get; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime ResDateTo { set; get; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime ResTimeTo { set; get; }
        [DataMember]
        public string CustomerAccount { set; get; }
        [DataMember]
        public string Description { set; get; }
        [DataMember]
        public string Comment { set; get; }
        [DataMember]
        public string Reference { set; get; }
        [DataMember]
        public string ContactNo { set; get; }
        [DataMember]
        public string ContactName { set; get; }
        [DataMember]
        public string Email { set; get; }
        [DataMember]
        public string Location { set; get; }
        [DataMember]
        public string SalesPerson { set; get; }
        [DataMember]
        public int Internalstatus { set; get; }
        [DataMember]
        public string Status { set; get; }
    }
}
