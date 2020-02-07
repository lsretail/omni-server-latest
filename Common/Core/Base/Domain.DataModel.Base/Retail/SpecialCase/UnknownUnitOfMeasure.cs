using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail.SpecialCase
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class UnknownUnitOfMeasure : UnitOfMeasure
    {
        public UnknownUnitOfMeasure(string id, string itemId) : base(id, itemId)
        {
        }

        public UnknownUnitOfMeasure() : base(string.Empty, string.Empty)
        {
        }
    }
}
