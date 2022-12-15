using System;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Hagar;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.BLL
{
    public class HagarBLL : BaseBLL
    {
        private IHagarBO iBOCustom = null;

        protected IHagarBO BOCustom
        {
            get
            {
                if (iBOCustom == null)
                    iBOCustom = GetBORepository<IHagarBO>(config.LSKey.Key, config.IsJson);
                return iBOCustom;
            }
        }

        public HagarBLL(BOConfiguration config) : base(config)
        {
        }

        public virtual ReplHagarItemWebExtResponse ReplHagarItemWebExtendedInfo(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplHagarItemWebExtResponse rs = new ReplHagarItemWebExtResponse()
            {
                WebExtension = BOCustom.ReplicateItemWebExtended(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.WebExtension.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual bool CheckCreditLimit(string cardId, decimal amount, out decimal availAmount, out string message, Statistics stat)
        {
            return BOCustom.CheckCreditLimit(cardId, amount, out availAmount, out message, stat);
        }
    }
}
