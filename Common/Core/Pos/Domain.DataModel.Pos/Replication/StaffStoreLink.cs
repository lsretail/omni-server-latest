using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ReplStaffStoreLinkResponse : IDisposable
    {
        public ReplStaffStoreLinkResponse()
        {
            LastKey = "";
            MaxKey = "";
            RecordsRemaining = 0;
            StaffStoreLinks = new List<ReplStaffStoreLink>();
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
                if (StaffStoreLinks != null)
                    StaffStoreLinks.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplStaffStoreLink> StaffStoreLinks { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ReplStaffStoreLink : IDisposable
    {
        public ReplStaffStoreLink()
        {
            StoreId = string.Empty;
            StaffId = string.Empty;
            DefaultHospType = string.Empty;
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
        public string StoreId { get; set; }
        [DataMember]
        public string StaffId { get; set; }
        [DataMember]
        public string DefaultHospType { get; set; }
    }
}
