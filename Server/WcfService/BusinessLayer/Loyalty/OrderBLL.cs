using System;
using System.Collections.Generic;

using NLog;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;

namespace LSOmni.BLL.Loyalty
{
    public class OrderBLL : BaseLoyBLL
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IOrderRepository iOrderRepository;
        private string tenderMapping;

        public OrderBLL(string securityToken, string deviceId, int timeoutInSeconds)
            : base(securityToken, deviceId, timeoutInSeconds)
        {
            iOrderRepository = base.GetDbRepository<IOrderRepository>();

            AppSettingsBLL appBll = new AppSettingsBLL();
            tenderMapping = appBll.AppSettingsGetByKey(AppSettingsKey.TenderType_Mapping);   //will throw exception if not found
        }

        public OrderBLL(string deviceId, int timeoutInSeconds)
            : this("", deviceId, timeoutInSeconds)
        {
        }

        public virtual List<OrderLineAvailability> OrderAvailabilityCheck(OrderAvailabilityRequest request)
        {
            //validation
            if (request == null)
            {
                string msg = "OrderAvailabilityCheck() request is empty";
                throw new LSOmniServiceException(StatusCode.Error, msg);
            }
            return BOLoyConnection.OrderAvailabilityCheck(request);
        }

        public virtual OrderAvailabilityResponse OrderAvailabilityCheck(OneList request)
        {
            //validation
            if (request == null)
            {
                string msg = "OrderAvailabilityCheck() request is empty";
                throw new LSOmniServiceException(StatusCode.Error, msg);
            }
            return BOLoyConnection.OrderAvailabilityCheck(request);
        }

        public virtual Order OrderCreate(Order request)
        {
            //validation
            if (request == null)
            {
                string msg = "OrderCreate() request is empty";
                throw new LSOmniServiceException(StatusCode.Error, msg);
            }
            if (Validation.IsValidGuid(request.Id) == false)
                request.Id = GuidHelper.NewGuidString();

            if (request.AnonymousOrder == false && string.IsNullOrEmpty(request.CardId) == false)
            {
                MemberContact contact = BOLoyConnection.ContactGetByCardId(request.CardId, 0);
                if (contact == null)
                    throw new LSOmniServiceException(StatusCode.CardIdInvalid, "Invalid card number");
                if (string.IsNullOrEmpty(request.ContactId))
                    request.ContactId = contact.Id;
                if (string.IsNullOrEmpty(request.Email))
                    request.Email = contact.Email;
                if (string.IsNullOrEmpty(request.PhoneNumber))
                    request.PhoneNumber = contact.MobilePhone;
                if (string.IsNullOrEmpty(request.ContactName))
                    request.ContactName = contact.Name;
            }

            BOLoyConnection.OrderCreate(request, tenderMapping);

            try
            {
                //líka save í OrderQueue töflunni - bara að ganni svo við eigum þetta?
                OrderQueueBLL qBLL = new OrderQueueBLL(base.DeviceId, timeoutInSeconds);
                qBLL.OrderCreate(request);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Failed to save OrderClickCollect in LSOmni db, but was saved in NAV");
            }

            return OrderGetByWebId(request.Id, true);
        }

        public virtual Order OrderGetById(string id, bool includeLines)
        {
            return BOLoyConnection.OrderGetById(id, includeLines, tenderMapping);
        }

        public virtual Order OrderGetByWebId(string id, bool includeLines)
        {
            return BOLoyConnection.OrderGetByWebId(id, includeLines, tenderMapping);
        }

        public virtual Order OrderGetByReceiptId(string id, bool includeLines)
        {
            return BOLoyConnection.OrderGetByReceiptId(id, includeLines, tenderMapping);
        }

        public virtual List<Order> OrderHistoryByContactId(string contactId, bool includeLines, bool includeTransactions)
        {
            return BOLoyConnection.OrderHistoryByContactId(contactId, includeLines, includeTransactions, tenderMapping);
        }
    }
}
