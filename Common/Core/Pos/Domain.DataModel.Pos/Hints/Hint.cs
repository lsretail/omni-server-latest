using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Pos.Hints
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class Hint
    {
        public enum HintTypes
        {
            CheckoutHint = 0,
            OverrideQtyHint = 1,
            SeeOtherStoreStockHint = 2
        }

        [DataMember]
        public string StaffId { get; set; }
        [DataMember]
        public HintTypes HintType { get; set; }
        [DataMember]
        public int ShowCountLeft { get; set; }
    }
}
