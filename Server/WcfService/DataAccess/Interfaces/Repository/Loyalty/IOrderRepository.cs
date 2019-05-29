using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IOrderRepository
    {
        void SaveOrderMessage(OrderMessage order);
        OrderMessage OrderMessageGetById(string guid);
        List<OrderMessage> OrderMessageSearch(OrderMessageSearchRequest searchRequest);
        void UpdateStatus(string guid, OrderMessageStatus status);
        void OrderMessageNotificationSave(string notificationId,long orderMessageId, string contactId, string description, string details, string qrText);
    }
}
