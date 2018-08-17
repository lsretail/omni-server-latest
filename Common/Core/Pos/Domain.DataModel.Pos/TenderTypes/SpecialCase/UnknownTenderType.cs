using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Pos.TenderTypes.SpecialCase
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public sealed class UnknownTenderType : TenderType
    {
        public UnknownTenderType(string tenderTypeId)
            : base(tenderTypeId)
        {
        }
    }
}
