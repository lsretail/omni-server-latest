using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class Account : Entity, IDisposable
    {
        public Account(string id) : base(id)
        {
            PointBalance = 0;
            Scheme = new Scheme();
        }

        public Account() : this(string.Empty)
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
                if (Scheme != null)
                    Scheme.Dispose();
            }
        }

        [DataMember]
        public long PointBalance { get; set; }
        [DataMember]
        public Scheme Scheme { get; set; } //Member scheme
    }
}
 