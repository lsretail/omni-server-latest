using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public abstract class BaseLine : Entity
    {
        [DataMember]
        public int LineNumber { get; set; }
        [DataMember]
        public bool Voided { get; set; }

        public BaseLine(int lineNumber)
        {
            this.LineNumber = lineNumber;
            Voided = false;
        }
    }
}
