using System;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Exceptions
{

    public sealed class SuspendWithNoSaleLinesException : Exception
    {
        public SuspendWithNoSaleLinesException()
            : base()
        {
        }
    }
}
