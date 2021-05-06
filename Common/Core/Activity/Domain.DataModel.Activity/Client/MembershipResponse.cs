using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Client
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class MembershipResponse : Entity, IDisposable
    {
        public MembershipResponse(string id) : base(id)
        {
            ItemNo = string.Empty;
        }

        public MembershipResponse() : this(string.Empty)
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
        public string BookingRef { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public decimal Discount { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
    }
}
