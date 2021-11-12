using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Activities
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class ActivityResource : Entity, IDisposable
    {
        public ActivityResource(string id) : base(id)
        {
            Description = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            Group = string.Empty;
            FixedLocation = string.Empty;
        }

        public ActivityResource() : this(string.Empty)
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
        public string Email { get; set; }
        [DataMember]
        public string Phone { get; set; }
        [DataMember]
        public string FixedLocation { get; set; }
        [DataMember]
        public string Group { get; set; }
    }
}
