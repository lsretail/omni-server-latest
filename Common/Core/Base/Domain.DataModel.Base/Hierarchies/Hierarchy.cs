using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Hierarchies
{
    public class Hierarchy : Entity
    {
        public Hierarchy()
        {
        }

        public Hierarchy(string id) : base(id)
        {
        }

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public HierarchyType Type { get; set; }
        [DataMember]
        public List<HierarchyNode> Nodes { get; set; }
        [DataMember]
        public List<HierarchyAttribute> Attributes { get; set; }

        public void RecursiveBuilder(ref HierarchyNode node, IEnumerable<HierarchyNode> nodes)
        {
            HierarchyNode mynode = node;
            node.Nodes = (from n in nodes where n.ParentNode == mynode.Id select n).ToList();
            node.Nodes.ForEach(f => RecursiveBuilder(ref f, nodes));
        }
    }
}
