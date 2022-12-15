using System;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSOmni.DataAccess.BOConnection.CentralPre.Dal;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Hagar;

namespace LSOmni.DataAccess.BOConnection.CentralPre
{
    public class NavHagar : NavBase, IHagarBO
    {
        public NavHagar(BOConfiguration config) : base(config)
        {
        }

        public virtual List<ReplItemWebExtended> ReplicateItemWebExtended(string appId, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            HagarItemWebExtRepository rep = new HagarItemWebExtRepository(config);
            return rep.ReplicateItemWebExtended(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
        }

        public virtual bool CheckCreditLimit(string cardId, decimal amount, out decimal availAmount, out string message, Statistics stat)
        {
            return LSCentralWSBase.CheckCreditLimit(cardId, amount, out availAmount, out message, stat);
        }
    }
}
