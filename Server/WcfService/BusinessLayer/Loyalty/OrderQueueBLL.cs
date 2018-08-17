using System.Collections.Generic;
using NLog;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSOmni.BLL.Loyalty
{
    public class OrderQueueBLL : BaseLoyBLL
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IOrderQueueRepository iOrderQueueRepository;

        public OrderQueueBLL(string securityToken, string deviceId, int timeoutInSeconds)
            : base(securityToken, deviceId, timeoutInSeconds)
        {
            iOrderQueueRepository = base.GetDbRepository<IOrderQueueRepository>();
        }

        public OrderQueueBLL(string deviceId, int timeoutInSeconds)
            : this("", deviceId, timeoutInSeconds)
        {
        }

        public OrderQueueBLL(int timeoutInSeconds)
            : this("", "", timeoutInSeconds)
        {
        }

        public virtual OrderQueue Save(OrderQueue order)
        {
            string theGuid = "";
            //validation
            if (order == null)
            {
                string msg = "Save() order is null";
                throw new LSOmniServiceException(StatusCode.Error, msg);
            }
            theGuid = iOrderQueueRepository.Save(order);
            return OrderGetById(theGuid);
        }

        public virtual OrderQueue OrderGetById(string orderId)
        {
            //validation
            if (string.IsNullOrWhiteSpace(orderId))
            {
                string msg = "orderId is empty";
                throw new LSOmniServiceException(StatusCode.Error, msg);
            }
            OrderQueue order = iOrderQueueRepository.OrderGetById(orderId);
            return order;
        }

        public virtual List<OrderQueue> OrderSearch(OrderSearchRequest searchRequest)
        {
            //validation
            if (searchRequest == null)
            {
                string msg = "search is empty";
                throw new LSOmniServiceException(StatusCode.Error, msg);
            }
            return iOrderQueueRepository.OrderSearch(searchRequest);
        }

        public virtual void UpdateStatus(string guid, OrderQueueStatus status)
        {
            iOrderQueueRepository.UpdateStatus(guid, status);
        }

        public virtual void OrderCreate(Order request)
        {
            //validation
            if (request == null)
            {
                string msg = "OrderCreateClickCollect() request is empty";
                throw new LSOmniServiceException(StatusCode.Error, msg);
            }
            if (Validation.IsValidGuid(request.Id) == false)
                request.Id = GuidHelper.NewGuidString();

            if (request.OrderLines != null)
            {
                foreach (OrderLine r in request.OrderLines)
                {
                    if (r.LineNumber < 10000)
                        r.LineNumber *= 10000;
                }
            }

            if (request.OrderDiscountLines != null)
            {
                foreach (OrderDiscountLine r in request.OrderDiscountLines)
                {
                    if (r.LineNumber < 10000)
                        r.LineNumber *= 10000;
                }
            }

            OrderQueue orderQueue = new OrderQueue();
            orderQueue.Id = request.Id;
            orderQueue.ContactId = request.ContactId;
            orderQueue.Description = (request.ClickAndCollectOrder) ? "ClickAndCollect" : "Shipping";
            orderQueue.DeviceId = this.DeviceId;
            orderQueue.OrderQueueStatus = OrderQueueStatus.New;
            orderQueue.OrderQueueType = OrderQueueType.ClickCollect;
            orderQueue.SearchKey = "";
            orderQueue.StoreId = request.StoreId;
            orderQueue.PhoneNumber = request.PhoneNumber;
            orderQueue.Email = request.Email;

            //this is ordertype click and collect, so serialize it
            orderQueue.OrderXml = Serialization.SerializeToXml(request);
            orderQueue = this.Save(orderQueue);
        }

        public virtual List<Order> OrderSearchClickCollect(OrderSearchRequest searchRequest)
        {
            //validation
            if (searchRequest == null)
            {
                throw new LSOmniServiceException(StatusCode.Error, "search is empty");
            }

            List<OrderQueue> orders = iOrderQueueRepository.OrderSearch(searchRequest);

            List<Order> reqList = new List<Order>();
            foreach (OrderQueue order in orders)
            {
                if (order.OrderQueueType == OrderQueueType.ClickCollect && Validation.IsValidXml(order.OrderXml))
                {
                    reqList.Add(Serialization.DeserializeFromXml<Order>(order.OrderXml));
                }
            }
            return reqList;
        }
    }
}
