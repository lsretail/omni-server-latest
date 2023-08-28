using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Client
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class AdmissionEntry : Entity, IDisposable
    {
        public AdmissionEntry(string id) : base(id)
        {
            ContactNo = string.Empty;
            LocationNo = string.Empty;
            GateNo = string.Empty;
            MemberType = string.Empty;
            ProductName = string.Empty;
            Type = string.Empty;
            Name = string.Empty;
            MembershipNo = string.Empty;
            EntryTime = DateTime.MinValue.ToUniversalTime();
        }

        public AdmissionEntry() : this(string.Empty)
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
        public string ContactNo { get; set; }
        [DataMember]
        public string LocationNo { get; set; }
        [DataMember]
        public string GateNo { get; set; }
        [DataMember]
        public int LineNo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime EntryTime { get; set; }
        [DataMember]
        public string MemberType { get; set; }
        [DataMember]
        public string ProductName { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string MembershipNo { get; set; }

    }
}
