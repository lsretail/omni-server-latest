
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Pos.Utils;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017"), KnownType(typeof(RetailTransactionActionResult)), KnownType(typeof(OffLineRetailTransactionActionResult))]
    public abstract class TransactionActionResult
    {
        [DataMember]
        public WarningMessage WarningMessage { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class RetailTransactionActionResult : TransactionActionResult
    {
        [DataMember]
        public RetailTransaction Transaction { get; set; }
	}

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class OffLineRetailTransactionActionResult : TransactionActionResult
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string TransactionNumber { get; set; }
    }
}

