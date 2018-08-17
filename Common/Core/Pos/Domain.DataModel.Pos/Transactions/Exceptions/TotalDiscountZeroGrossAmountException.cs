using System;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Exceptions
{
    public sealed class TotalDiscountZeroGrossAmountException : Exception
    {
        public TotalDiscountZeroGrossAmountException()
            : base()
        {
        }
    }
}
