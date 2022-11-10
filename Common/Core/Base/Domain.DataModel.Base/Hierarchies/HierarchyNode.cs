using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

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
        public int LineNo { get; set; }
        [DataMember]
        public List<HierarchyNode> Nodes { get; set; }
        [DataMember]
        public List<HierarchyLeaf> Leafs { get; set; }

        [DataMember]
        public HierarchyDealType Type { get; set; }
        [DataMember]
        public string No { get; set; }
        [DataMember]
        public string VariantCode { get; set; }
        [DataMember]
        public string UnitOfMeasure { get; set; }

        [DataMember]
        public int MinSelection { get; set; }
        [DataMember]
        public int MaxSelection { get; set; }
        /// <summary>
        /// Amount to add to a deal price
        /// </summary>
        [DataMember]
        public decimal AddedAmount { get; set; }
        /// <summary>
        /// Used to group deals together, f.ex. if Medium Fries goes with Medium Soda.
        /// </summary>
        [DataMember]
        public int DealModSizeGroupIndex { get; set; }
    }
}
