using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Members
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class GiftCard : Entity, IDisposable
    {
        public GiftCard(string id) : base(id)
        {
            ExpireDate = DateTime.MaxValue;
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
        public decimal Balance { get; set; }
        [DataMember]
        public DateTime ExpireDate { get; set; }
    }
}
