using System;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class Device : Entity, IDisposable, IAggregateRoot
    {
        public Device(string id) : base(id)
        {
            DeviceFriendlyName = string.Empty;
            Platform = string.Empty;
            OsVersion = string.Empty;
            Manufacturer = string.Empty;
            Model = string.Empty;
            SecurityToken = string.Empty;
            BlockedDate = new DateTime(1900, 1, 1);
        }

        public Device() : this(string.Empty)
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
        public string DeviceFriendlyName { get; set; }
        [DataMember]
        public string Platform { get; set; }
        [DataMember]
        public string OsVersion { get; set; }
        [DataMember]
        public string Manufacturer { get; set; }
        [DataMember]
        public string Model { get; set; }
        [DataMember]
        public string CardId { get; set; }
        [DataMember]
        public string SecurityToken { get; set; }
        [IgnoreDataMember]
        public MemberContact UserLoggedOnToDevice;
        [DataMember]
        public int Status;
        [DataMember]
        public string BlockedBy;
        [DataMember]
        public string BlockedReason;
        [DataMember]
        public DateTime BlockedDate;
    }
}
