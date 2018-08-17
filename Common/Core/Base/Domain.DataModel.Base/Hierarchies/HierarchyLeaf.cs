using LSRetail.Omni.Domain.DataModel.Base.Base;
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
    }
}
