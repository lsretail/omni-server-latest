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
        public AccountType Type { get; set; }
        [DataMember]
        public AccountStatus Status { get; set; }
        [DataMember]
        public bool Blocked { get; set; }
        [DataMember]
        public string BlockedBy;
        [DataMember]
        public string BlockedReason;
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime BlockedDate;
        [DataMember]
        public long PointBalance { get; set; }
        [DataMember]
        public string CustomerId { get; set; }
        [DataMember]
        public Scheme Scheme { get; set; } //Member scheme

        public string SchemeCode { get; set; }
        public string ClubCode { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum AccountType
    {
        [EnumMember]
        Private = 0,
        [EnumMember]
        Family = 1,
        [EnumMember]
        Company = 2
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum AccountStatus
    {
        [EnumMember]
        Unassigned = 0,
        [EnumMember]
        Active = 1,
        [EnumMember]
        Closed = 2
    }
}
 