using System;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Hagar;

namespace LSOmni.DataAccess.Interface.BOConnection
{
    public interface IHagarBO
    {
        // Interface for NavCustom functions
        List<ReplItemWebExtended> ReplicateItemWebExtended(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining);
        bool CheckCreditLimit(string cardId, decimal amount, out decimal availAmount, out string message, Statistics stat);
    }
}
