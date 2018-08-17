
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Pos.Items;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Calculators.Tax
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public abstract class TaxCalculator
    {
        public abstract decimal GetItemTaxPercentage(RetailItem item, RetailTransaction retailTransaction);
    }
}
