using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public sealed class UnknownStore : Store
    {
        public UnknownStore(string id) : base(id)
        {
        }
    }
}
