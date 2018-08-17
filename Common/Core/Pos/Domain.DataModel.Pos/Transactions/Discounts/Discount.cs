using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Discounts
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017"), KnownType(typeof(DiscountLine))]
    public class Discount
    {
        [DataMember]
        public Money Amount { get; set; }
        [DataMember]
        public decimal Percentage { get; set; }
        [DataMember]
        public DiscountEntryType EntryType { get; set; }

        public Discount()
        {
        }

        public Discount(Currency currency, decimal amount, decimal percentage, DiscountEntryType entryType)
        {
            Amount = new Money(amount, currency);
            Percentage = percentage;
            EntryType = entryType;
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public enum DiscountEntryType
    {
        [EnumMember]
        Percentage = 0,
        [EnumMember]
        Amount = 1,
        [EnumMember]
        Both = 2
    }
}
