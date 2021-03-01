using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplHierarchyResponse : IDisposable
    {
        public ReplHierarchyResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Hierarchies = new List<ReplHierarchy>();
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
                if (Hierarchies != null)
                    Hierarchies.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplHierarchy> Hierarchies { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplHierarchy : IDisposable
    {
        public ReplHierarchy()
        {
            Description = string.Empty;
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
        public string Id { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public DateTime StartDate { get; set; }
        [DataMember]
        public HierarchyType Type { get; set; }
    }
}
