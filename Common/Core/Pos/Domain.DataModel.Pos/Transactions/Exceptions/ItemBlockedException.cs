using System;
using LSRetail.Omni.Domain.DataModel.Pos.Items;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Exceptions
{
    public sealed class ItemBlockedException : Exception
    {
        public ItemBlockedException(RetailItem item)
            : base()
        {
        }
    }
}
