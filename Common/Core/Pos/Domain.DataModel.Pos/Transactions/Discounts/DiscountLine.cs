using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Discounts
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class DiscountLine : Discount
    {
        [DataMember]
        public int LineNumber { get; set; }
        [DataMember]
        public string No { get; set; }
        [DataMember]
        public string OfferNo { get; set; }
        [DataMember]
        public DiscountType Type { get; set; }
        [DataMember]
        public PeriodicDiscType PeriodicType { get; set; }
        [DataMember]
        public string Description { get; set; }

        public DiscountLine()
        {
        }

        public DiscountLine(DiscountType type, Currency currency, decimal amount, decimal percentage, string description, DiscountEntryType entryType)
            : base(currency, amount, percentage, entryType)
        {
            Type = type;
            Description = description;
        }
    }
}
