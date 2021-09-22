using System;

using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;

namespace LSOmni.BLL.Loyalty
{
    public class OrderBLL : BaseLoyBLL
    {
        public OrderBLL(BOConfiguration config, string deviceId, int timeoutInSeconds)
            : base(config, deviceId, timeoutInSeconds)
        {
        }

        public OrderBLL(BOConfiguration config, int timeoutInSeconds)
           : this(config, "", timeoutInSeconds)
        {
        }

        public virtual OrderAvailabilityResponse OrderAvailabilityCheck(OneList request, bool shippingOrder)
        {
            //validation
            if (request == null)
                throw new LSOmniException(StatusCode.ObjectMissing, "OrderAvailabilityCheck() request is empty");

            return BOLoyConnection.OrderAvailabilityCheck(request, shippingOrder);
        }

        public virtual OrderStatusResponse OrderStatusCheck(string orderId)
        {
            return BOLoyConnection.OrderStatusCheck(orderId);
        }

        public virtual void OrderCancel(string orderId, string storeId, string userId)
        {
            BOLoyConnection.OrderCancel(orderId, storeId, userId);
        }

        public virtual SalesEntry OrderCreate(Order request)
        {
            //validation
            if (request == null)
                throw new LSOmniException(StatusCode.ObjectMissing, "OrderCreate() request is empty");

            if (string.IsNullOrEmpty(request.CardId) == false)
            {
                MemberContact contact = BOLoyConnection.ContactGetByCardId(request.CardId, 0, false);
                if (contact == null)
                    throw new LSOmniServiceException(StatusCode.CardIdInvalid, "Invalid card number");
                if (string.IsNullOrEmpty(request.ContactId))
                    request.ContactId = contact.Id;
                if (string.IsNullOrEmpty(request.Email))
                    request.Email = contact.Email;
                if (string.IsNullOrEmpty(request.ContactName))
                    request.ContactName = contact.Name;
                if (request.ContactAddress == null && contact.Addresses.Count > 0)
                {
                    request.ContactAddress = contact.Addresses[0];
                }
            }

            string extId = BOLoyConnection.OrderCreate(request, out string orderId);

            if (request.OrderType == OrderType.ScanPayGoSuspend)
            {
                return new SalesEntry(extId);
            }

            if (string.IsNullOrEmpty(orderId))
                return BOLoyConnection.SalesEntryGet(extId, DocumentIdType.External);

            return BOLoyConnection.SalesEntryGet(orderId, DocumentIdType.Order);
        }

        public virtual SalesEntry OrderHospCreate(OrderHosp request)
        {
            if (request == null)
                throw new LSOmniException(StatusCode.ObjectMissing, "OrderCreate() request is empty");

            string extId = BOLoyConnection.HospOrderCreate(request);
            return BOLoyConnection.SalesEntryGet(extId, DocumentIdType.HospOrder);
        }

        public virtual void HospOrderCancel(string storeId, string orderId)
        {
            BOLoyConnection.HospOrderCancel(storeId, orderId);
        }

        public virtual OrderHospStatus HospOrderStatus(string storeId, string orderId)
        {
            return BOLoyConnection.HospOrderStatus(storeId, orderId);
        }
    }
}
