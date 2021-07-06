using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Hierarchies
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class HierarchyLeaf : HierarchyPoint
    {
        public HierarchyLeaf()
        {
        }

        public HierarchyLeaf(string id) : base(id)
        {
        }
        
        [DataMember]
        public HierarchyLeafType Type { get; set; }
        [DataMember]
        public string ItemUOM { get; set; }
        [DataMember]
        public int SortOrder { get; set; }

        [DataMember]
        public int DealLineNo { get; set; }
        [DataMember]
        public string DealLineCode { get; set; }

        [DataMember]
        public int LineNo { get; set; }
        [DataMember]
        public string ItemNo { get; set; }
        [DataMember]
        public string VariantCode { get; set; }

        [DataMember]
        public int MinSelection { get; set; }
        [DataMember]
        public int MaxSelection { get; set; }
        [DataMember]
        public decimal AddedAmount { get; set; }
    }
}
