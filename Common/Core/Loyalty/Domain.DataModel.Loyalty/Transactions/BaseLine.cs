using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Transactions
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public abstract class BaseLine : Entity
    {
        public BaseLine() : this(null)
        {
        }

        public BaseLine(string id) : base(id)
        {
        }
    }
}
