using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Hierarchies
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class HierarchyNode : HierarchyPoint, IDisposable
    {
        public HierarchyNode() : this(string.Empty)
        {
        }

        public HierarchyNode(string id) : base(id)
        {
            Nodes = new List<HierarchyNode>();
            Leafs = new List<HierarchyLeaf>();
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
                if (Nodes != null)
                    Nodes.Clear();
                if (Leafs != null)
                    Leafs.Clear();
            }
        }

        
        [DataMember]
        public int Indentation { get; set; }
        [DataMember]
        public int PresentationOrder { get; set; }
        [DataMember]
        public int ChildrenOrder { get; set; }
        [DataMember]
        public List<HierarchyNode> Nodes { get; set; }
        [DataMember]
        public List<HierarchyLeaf> Leafs { get; set; }
    }
}
