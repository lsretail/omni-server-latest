
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Pos.Items;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Calculators.Price
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public abstract class PriceCalculator
    {
        public abstract RetailItem CalculateItemPrice(RetailItem item, RetailTransaction retailTransaction);
    }
}
