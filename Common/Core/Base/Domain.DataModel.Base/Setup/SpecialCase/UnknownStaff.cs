using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public sealed class UnknownStaff : Staff
    {
        public UnknownStaff(string staffId) : base(staffId)
        {
        }
    }
}
