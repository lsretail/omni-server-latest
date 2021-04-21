using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Hierarchies
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public abstract class HierarchyPoint : Entity
    {
        protected HierarchyPoint()
        {
        }

        protected HierarchyPoint(string id) : base(id)
        {
        }

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string ImageId { get; set; }

        [DataMember]
        public string HierarchyCode { get; set; }
        [DataMember]
        public string ParentNode { get; set; }
    }
}
