using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplHierarchyLeafResponse : IDisposable
    {
        public ReplHierarchyLeafResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Leafs = new List<ReplHierarchyLeaf>();
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
                if (Leafs != null)
                    Leafs.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplHierarchyLeaf> Leafs { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplHierarchyLeaf : IDisposable
    {
        public ReplHierarchyLeaf()
        {
            HierarchyCode = string.Empty;
            NodeId = string.Empty;
            ImageId = string.Empty;
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
        public string HierarchyCode { get; set; }
        [DataMember]
        public string NodeId { get; set; }
        [DataMember]
        public string ImageId { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public HierarchyLeafType Type { get; set; }
    }
}
