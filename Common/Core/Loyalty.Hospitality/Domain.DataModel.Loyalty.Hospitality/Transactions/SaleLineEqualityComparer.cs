using System.Collections.Generic;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Transactions
{
    public class SaleLineEqualityComparer : IEqualityComparer<SaleLine>
    {
        public bool Equals(SaleLine x, SaleLine y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(SaleLine saleLine)
        {
            return saleLine.Item.Id.GetHashCode();
        }
    }
}
