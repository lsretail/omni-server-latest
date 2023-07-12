using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
using LSOmni.Common.Util;

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

        public virtual OrderAvailabilityResponse OrderAvailabilityCheck(OneList request, bool shippingOrder, Statistics stat)
        {
            //validation
            if (request == null)
                throw new LSOmniException(StatusCode.ObjectMissing, "OrderAvailabilityCheck() request is empty");

            return BOLoyConnection.OrderAvailabilityCheck(request, shippingOrder, stat);
        }

        public virtual OrderStatusResponse OrderStatusCheck(string orderId, Statistics stat)
        {
            return BOLoyConnection.OrderStatusCheck(orderId, stat);
        }

        public virtual void OrderCancel(string orderId, string storeId, string userId, List<int> lineNo, Statistics stat)
        {
            BOLoyConnection.OrderCancel(orderId, storeId, userId, lineNo, stat);
        }

        public virtual SalesEntry OrderCreate(Order request, bool returnOrderIdOnly, Statistics stat)
        {
            //validation
            if (request == null)
                throw new LSOmniException(StatusCode.ObjectMissing, "OrderCreate() request is empty");

            if (string.IsNullOrEmpty(request.CardId) == false)
            {
                MemberContact contact = BOLoyConnection.ContactGet(ContactSearchType.CardId, request.CardId, stat);
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

            if (request.OrderType == OrderType.ScanPayGo && config.SettingsBoolGetByKey(ConfigKey.ScanPayGo_CheckPayAuth))
            {
                ScanPayGoBLL sBll = new ScanPayGoBLL(config, timeoutInSeconds);
                if (request.OrderPayments != null && request.OrderPayments.Count > 0)
                {
                    Task<bool> ok = sBll.GetAuthPaymentCodeAsync(request.OrderPayments[0].AuthorizationCode, stat);
                    Task.WaitAll(ok);
                    if (ok.Result == false)
                        throw new LSOmniServiceException(StatusCode.PaymentAuthError, "Payment Authentication error");
                }
            }

            string extId = BOLoyConnection.OrderCreate(request, out string orderId, stat);

            if (request.OrderType == OrderType.ScanPayGoSuspend || (returnOrderIdOnly && string.IsNullOrEmpty(orderId) == false))
            {
                return new SalesEntry(orderId)
                {
                    ExternalId = extId
                };
            }

            TransactionBLL tBLL = new TransactionBLL(config, timeoutInSeconds);
            if (string.IsNullOrEmpty(orderId))
                return tBLL.SalesEntryGet(extId, DocumentIdType.External, stat);

            return tBLL.SalesEntryGet(orderId, DocumentIdType.Order, stat);
        }

        public virtual SalesEntry OrderHospCreate(OrderHosp request, bool returnOrderIdOnly, Statistics stat)
        {
            if (request == null)
                throw new LSOmniException(StatusCode.ObjectMissing, "OrderCreate() request is empty");

            string orderId = BOLoyConnection.HospOrderCreate(request, stat);
            if (returnOrderIdOnly && string.IsNullOrEmpty(orderId) == false)
            {
                return new SalesEntry(orderId)
                {
                    ExternalId = request.Id
                };
            }

            TransactionBLL tBLL = new TransactionBLL(config, timeoutInSeconds);
            return tBLL.SalesEntryGet(orderId, DocumentIdType.HospOrder, stat);
        }

        public virtual void HospOrderCancel(string storeId, string orderId, Statistics stat)
        {
            BOLoyConnection.HospOrderCancel(storeId, orderId, stat);
        }

        public virtual OrderHospStatus HospOrderStatus(string storeId, string orderId, Statistics stat)
        {
            return BOLoyConnection.HospOrderStatus(storeId, orderId, stat);
        }
    }
}
