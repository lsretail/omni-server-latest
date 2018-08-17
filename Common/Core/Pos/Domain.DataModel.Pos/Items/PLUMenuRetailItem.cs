using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSRetail.Omni.Domain.DataModel.Pos.Items
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class PLUMenuRetailItem
    {
        [DataMember]
        public Store Store { get; set; }
        [DataMember]
        public int PageId { get; set; }
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public RetailItem Item { get; set; }
    }
}
