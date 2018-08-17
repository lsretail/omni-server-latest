using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ProductModifier : Modifier
    {
        [DataMember]
        public string ModifierSubCode { get; set; }

        [DataMember]
        public string ModifierGroupCode { get; set; }

        public ProductModifier(string id) : base(id)
        {
        }

        public ProductModifier() : this(string.Empty)
        {
        }

        public new ProductModifier Clone()
        {
            ProductModifier productModifier = (ProductModifier)MemberwiseClone();

            return productModifier;
        }
    }
}
