using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Activities
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class ActivityLocation : Entity, IDisposable
    {
        public ActivityLocation(string id) : base(id)
        {
            Description = string.Empty;
            RetailStore = string.Empty;
        }

        public ActivityLocation() : this(string.Empty)
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
        public string RetailStore { get; set; }
    }
}
