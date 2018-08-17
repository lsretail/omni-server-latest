using System;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Exceptions
{
    public sealed class SuspendNullTransactionException : Exception
    {
        public SuspendNullTransactionException()
            : base()
        {
        }
    }
}
