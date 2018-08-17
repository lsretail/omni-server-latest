using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu.SpecialCase
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class UnknownDeal : MenuDeal
    {
        public UnknownDeal()
        {
        }
    }
}
