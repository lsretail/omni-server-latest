using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class Disclaimer : Entity, IDisposable
    {
        public Disclaimer(string id) : base(id)
        {
            Personalized = false;
            Description = string.Empty;
            DateCreated = DateTime.Now;
        }

        public Disclaimer()
            : this(string.Empty)
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
        public bool Personalized { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public DateTime DateCreated { get; set; }
    }
}
