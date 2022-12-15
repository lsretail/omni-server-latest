using System;

using LSOmni.BLL;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Hagar;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.Service
{
    public partial class LSOmniBase
    {
        public virtual ReplHagarItemWebExtResponse ReplEcommItemWebExtendedInfo(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config, LogJson(replRequest));
                HagarBLL bll = new HagarBLL(config);
                return bll.ReplHagarItemWebExtendedInfo(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest);
                return null; //never gets here
            }
        }

        public virtual bool CheckCreditLimit(string cardId, decimal amount, out decimal availAmount, out string message)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);
            try
            {
                HagarBLL bll = new HagarBLL(config);
                return bll.CheckCreditLimit(cardId, amount, out availAmount, out message, stat);
            }
            catch (Exception ex)
            {
                message = ex.Message;
                availAmount = 0;
                HandleExceptions(ex, $"cardId:{cardId}");
                return false; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }
    }
}
