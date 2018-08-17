using System;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Exceptions
{
    public sealed class LineDiscountPercentageTooHighException : Exception
    {
        public LineDiscountPercentageTooHighException(decimal percentage)
            : base()
        {
        }
    }
}
