using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail.SpecialCase
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class UnknownVariantRegistration : VariantRegistration
    {
        public UnknownVariantRegistration(string id) : base(id)
        {
        }

        public UnknownVariantRegistration() : this(string.Empty)
        {
        }
    }
}
