using System;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Exceptions
{
    public sealed class SuspendedTransactionNotFoundException : Exception
    {
        public SuspendedTransactionNotFoundException()
            : base()
        {
        }
    }
}
