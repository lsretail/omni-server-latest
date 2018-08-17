using System;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Exceptions
{
    public sealed class LineDiscountHigherThanSaleLineAmountException : Exception
    {
        public LineDiscountHigherThanSaleLineAmountException()
            : base()
        {
        }
    }
}
