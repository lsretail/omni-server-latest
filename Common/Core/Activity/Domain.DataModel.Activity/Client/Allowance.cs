using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Client
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class Allowance : Entity, IDisposable
    {
        public Allowance(string id) : base(id)
        {
            Description = string.Empty;
            ItemNo = string.Empty;
            ValidLocation = string.Empty;
            ClientNo = string.Empty;
            ClientName = string.Empty;
        }

        public Allowance() : this(string.Empty)
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
        public string ItemNo { get; set; }
        [DataMember]
        public string ValidLocation { get; set; }
        [DataMember]
        public decimal QtyIssued { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateIssued { get; set; }
        [DataMember]
        public decimal UnitPrice { get; set; }
        [DataMember]
        public string ClientNo { get; set; }
        [DataMember]
        public string ClientName { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime ExpiryDate { get; set; }
        [DataMember]
        public decimal QuantityConsumed { get; set; }
        [DataMember]
        public decimal IssuedTotalAmount { get; set; }
    }
}
