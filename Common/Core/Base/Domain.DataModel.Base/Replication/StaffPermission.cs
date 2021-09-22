using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplStaffPermissionResponse : IDisposable
    {
        public ReplStaffPermissionResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Permissions = new List<ReplStaffPermission>();
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
                if (Permissions != null)
                    Permissions.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplStaffPermission> Permissions { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplStaffPermission : IDisposable
    {
        public ReplStaffPermission()
        {
            IsDeleted = false;
            Value = string.Empty;
            StaffId = string.Empty;
            Id = PermissionEntry.Unknown;
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
        public bool IsDeleted { get; set; }
        [DataMember]
        public PermissionEntry Id { get; set; }
        [DataMember]
        public string StaffId { get; set; }
        [DataMember]
        public PermissionType Type { get; set; }
        [DataMember]
        public string Value { get; set; }
    }
}
