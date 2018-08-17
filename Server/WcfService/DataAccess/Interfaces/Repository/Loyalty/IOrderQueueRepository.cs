using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IOrderQueueRepository
    {
        string Save(OrderQueue order);
        OrderQueue OrderGetById(string id);
        void UpdateStatus(string guid, OrderQueueStatus status);
        List<OrderQueue> OrderSearch(OrderSearchRequest searchRequest);
    }
}
