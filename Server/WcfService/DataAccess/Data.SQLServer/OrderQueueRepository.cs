using System;
using System.Collections.Generic;

using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSOmni.DataAccess.Dal
{
    public class OrderQueueRepository : BaseRepository, IOrderQueueRepository
    {
        public OrderQueueRepository(BOConfiguration config) : base(config)
        {
        }

        public string Save(OrderQueue order)
        {
            return order.Id.Replace("-", "");   //strip out the guild so client doesn't see it
        }

        public void UpdateStatus(string guid, OrderQueueStatus status)
        {
        }

        public OrderQueue OrderGetById(string guid)
        {
            return null;
        }

        public List<OrderQueue> OrderSearch(OrderSearchRequest searchRequest)
        {
            return new List<OrderQueue>();
        }
    }
}
