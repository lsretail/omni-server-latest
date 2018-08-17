using System;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Exceptions
{
    public sealed class TotalDiscountPercentageTooHighException : Exception
    {
        public TotalDiscountPercentageTooHighException(decimal percentage)
            : base()
        {
        }
    }
}
