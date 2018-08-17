using System;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Exceptions
{
    public sealed class TotalDiscountHighterThanTransactionBalanceException : Exception
    {
        public TotalDiscountHighterThanTransactionBalanceException()
            : base()
        {
        }
    }
}
