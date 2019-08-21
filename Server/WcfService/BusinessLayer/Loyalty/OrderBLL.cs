using System;

using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSOmni.Common.Util;

namespace LSOmni.BLL.Loyalty
{
    public class OrderBLL : BaseLoyBLL
    {
        private static LSLogger logger = new LSLogger();
        private string tenderMapping;
        public OrderBLL(BOConfiguration config, string deviceId, int timeoutInSeconds)
            : base(config, deviceId, timeoutInSeconds)
        {
            tenderMapping = config.SettingsGetByKey(ConfigKey.TenderType_Mapping);   //will throw exception if not found
        }

        public OrderBLL(BOConfiguration config, int timeoutInSeconds)
           : this(config, "", timeoutInSeconds)
        {
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

        public virtual OrderStatusResponse OrderStatusCheck(string orderId)
        {
            return BOLoyConnection.OrderStatusCheck(orderId);
        }

        public virtual void OrderCancel(string orderId)
        {
            BOLoyConnection.OrderCancel(orderId);
        }

        public virtual SalesEntry OrderCreate(Order request)
        {
            //validation
            if (request == null)
            {
                string msg = "OrderCreate() request is empty";
                throw new LSOmniServiceException(StatusCode.Error, msg);
            }

            if (string.IsNullOrEmpty(request.CardId) == false)
            {
                MemberContact contact = BOLoyConnection.ContactGetByCardId(request.CardId, 0, false);
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

            string extId = BOLoyConnection.OrderCreate(request, tenderMapping, out string orderId);

            try
            {
                //líka save í OrderQueue töflunni - bara að ganni svo við eigum þetta?
                OrderQueueBLL qBLL = new OrderQueueBLL(config, base.DeviceId, timeoutInSeconds);
                qBLL.OrderQueueCreate(request);
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex, "Failed to save OrderClickCollect in LSOmni db, but was saved in NAV");
            }

            if (string.IsNullOrEmpty(orderId))
                return BOLoyConnection.SalesEntryGet(extId, DocumentIdType.External, tenderMapping);

            return BOLoyConnection.SalesEntryGet(orderId, DocumentIdType.Order, tenderMapping);
        }
    }
}
