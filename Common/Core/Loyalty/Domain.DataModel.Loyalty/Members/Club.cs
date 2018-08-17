using System;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Members
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class Club : Entity, IDisposable
    {
        public Club(string id) : base(id)
        {
            Name = string.Empty;
        }

        public Club() : this (string.Empty)
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
        public string Name { get; set; }
    }
}
 