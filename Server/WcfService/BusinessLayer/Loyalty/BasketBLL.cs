using System;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSOmni.BLL.Loyalty
{
    public class BasketBLL : BaseLoyBLL
    {
        public BasketBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
        }

        public BasketBLL(BOConfiguration config)
            : this(config, 0)
        {
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
