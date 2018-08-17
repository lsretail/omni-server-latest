using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class DealModifier : Modifier, IDisposable
    {
        public DealModifier(string id) : base(id)
        {
        }

        public DealModifier() : this(string.Empty)
        {
        }

        [DataMember]
        public string DealModifierGroupId { get; set; }

        public new DealModifier Clone()
        {
            return (DealModifier)MemberwiseClone();
        }
    }
}
