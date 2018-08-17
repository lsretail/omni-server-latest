using System;
using NLog;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSOmni.DataAccess.Interface.Repository.Loyalty;

namespace LSOmni.BLL.Loyalty
{
    public class BasketBLL : BaseLoyBLL
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public BasketBLL(int timeoutInSeconds)
            : this("", timeoutInSeconds)
        {
        }

        public BasketBLL(string securityToken, int timeoutInSeconds)
            : base(securityToken, timeoutInSeconds)
        {
        }

        public virtual BasketCalcResponse BasketCalc(BasketCalcRequest basketRequest)
        {
            //CALL NAV web service -  
            basketRequest.CardId = (string.IsNullOrWhiteSpace(basketRequest.CardId) ? "" : basketRequest.CardId);
            basketRequest.ContactId = (string.IsNullOrWhiteSpace(basketRequest.ContactId) ? "" : basketRequest.ContactId);
            basketRequest.Id = (string.IsNullOrWhiteSpace(basketRequest.Id) ? "" : basketRequest.Id);

            if (string.IsNullOrEmpty(basketRequest.StoreId))
            {
                IAppSettingsRepository iAppRepo = GetDbRepository<IAppSettingsRepository>();
                basketRequest.StoreId = iAppRepo.AppSettingsGetByKey(AppSettingsKey.Loyalty_FilterOnStore);
            }

            BasketCalcResponse resp = BOLoyConnection.BasketCalc(basketRequest); //
            return resp;
        }

        public virtual OrderStatusResponse OrderStatusCheck(string transactionId)
        {
            //CALL NAV web service -  
            return BOLoyConnection.OrderStatusCheck(transactionId); //
        }

        public virtual void OrderCancel(string transactionId)
        {
            BOLoyConnection.OrderCancel(transactionId);
        }
    }
}
