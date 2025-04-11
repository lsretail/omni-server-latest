﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
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

        public virtual void OrderCancel(string orderId, string storeId, string userId, List<OrderCancelLine> lines, Statistics stat)
        {
            BOLoyConnection.OrderCancel(orderId, storeId, userId, lines, stat);
        }

        private void ValidateOrder(Order request, Statistics stat)
        {
            if (request == null)
                throw new LSOmniException(StatusCode.ObjectMissing, "Order request is empty");

            if (string.IsNullOrWhiteSpace(request.Id))
                request.Id = GuidHelper.NewGuidWithoutDashes(20);

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

                if (contact.Addresses.Count > 0 && ((request.ContactAddress == null) || (string.IsNullOrEmpty(request.ContactAddress.Address1))))
                {
                    request.ContactAddress = contact.Addresses[0];
                }
            }

            if ((request.ShipToAddress == null) || (string.IsNullOrEmpty(request.ShipToAddress.Address1)))
            {
                if (request.OrderType != OrderType.Sale && request.ShipOrder == false)
                {
                    request.ShipToAddress = new Address();
                }
                else
                {
                    throw new LSOmniException(StatusCode.AddressIsEmpty, "ShipToAddress can not be null if ClickAndCollectOrder is false");
                }
            }

            if ((request.ContactAddress == null) || (string.IsNullOrEmpty(request.ContactAddress.Address1)))
            {
                if (request.OrderType == OrderType.ClickAndCollect)
                {
                    request.ContactAddress = new Address();
                }
                else
                {
                    request.ContactAddress = request.ShipToAddress;
                }
            }

            // need to map the TenderType enum coming from devices to TenderTypeId that NAV knows
            if (request.OrderPayments == null)
                request.OrderPayments = new List<OrderPayment>();

            int lineNo = 1;
            foreach (OrderPayment line in request.OrderPayments)
            {
                line.TenderType = ConfigSetting.TenderTypeMapping(config.SettingsGetByKey(ConfigKey.TenderType_Mapping), line.TenderType, false); //map tender type between LSOmni and Nav
                if (line.LineNumber == 0)
                    line.LineNumber = lineNo++;
                else
                    lineNo = line.LineNumber;
            }
        }

        public virtual SalesEntry OrderCreate(Order request, bool returnOrderIdOnly, Statistics stat)
        {
            ValidateOrder(request, stat);
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

        public virtual SalesEntry OrderEdit(Order request, string orderId, OrderEditType editType, bool returnOrderIdOnly, Statistics stat)
        {
            ValidateOrder(request, stat);

            string extId = BOLoyConnection.OrderEdit(request, ref orderId, editType, stat);
            if (returnOrderIdOnly && (string.IsNullOrEmpty(orderId) == false))
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

        public virtual bool OrderUpdatePayment(string orderId, string storeId, OrderPayment payment, Statistics stat)
        {
            return BOLoyConnection.OrderUpdatePayment(orderId, storeId, payment, stat);
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

        public virtual List<OrderHospStatus> HospOrderStatus(string storeId, string orderId, Statistics stat)
        {
            return BOLoyConnection.HospOrderStatus(storeId, orderId, stat);
        }
    }
}
