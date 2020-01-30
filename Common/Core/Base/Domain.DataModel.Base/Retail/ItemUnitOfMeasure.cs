using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ItemUnitOfMeasure : Entity
    {
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public decimal QtyPerUnitOfMeasure { get; set; }
        [DataMember]
        public bool CountAsOne { get; set; }
        [DataMember]
        public int UomOrder { get; set; }
        [DataMember]
        public bool Selection { get; set; }

        public ItemUnitOfMeasure(string code) : base(code)
        {
        }

        public ItemUnitOfMeasure() : this(string.Empty)
        {
        }
    }
}
