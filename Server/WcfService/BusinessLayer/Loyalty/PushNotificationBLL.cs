using System;
using System.Collections.Generic;

using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base;
using LSOmni.Common.Util;

namespace LSOmni.BLL.Loyalty
{
    public class PushNotificationBLL : BaseLoyBLL
    {
        private static LSLogger logger = new LSLogger();
        private IPushNotificationRepository iPushRepository;

        public PushNotificationBLL(BOConfiguration config, string deviceId, int timeoutInSeconds)
            : base(config, deviceId, timeoutInSeconds)
        {
            this.iPushRepository = GetDbRepository<IPushNotificationRepository>(config);
        }

        public PushNotificationBLL(BOConfiguration config, int timeoutInSeconds = 15)
            : this(config, "", timeoutInSeconds)
        {
        }

        public virtual bool PushNotificationSave(PushNotificationRequest pushNotificationRequest)
        {
            //if no ad id is given, get one
            if (pushNotificationRequest == null)
                throw new ApplicationException("pushNotificationRequest cannot be null");

            try
            {
                //
                this.iPushRepository.Save(pushNotificationRequest);
            }
            catch (LSOmniServiceException ex)
            {
                logger.Error(config.LSKey.Key, ex, "PushNotificationSave failed");
                throw;
            }
            return true;
        }

        public virtual bool PushNotificationDelete(string deviceId)
        {
            try
            {
                //
                this.iPushRepository.Delete(deviceId);
                return true;
            }
            catch (LSOmniServiceException ex)
            {
                logger.Error(config.LSKey.Key, ex, "PushNotificationDelete failed");
                throw;
            }
        }

        public virtual void PushNotificationUpdateToSent(string contactId, string notificationId)
        {
            try
            {
                this.iPushRepository.UpdateToSent(contactId, notificationId);
            }
            catch (LSOmniServiceException ex)
            {
                logger.Error(config.LSKey.Key, ex, "PushNotificationUpdateToSent failed");
                throw;
            }
        }

        public virtual void PushNotificationUpdateCounter(string contactId, string notificationId)
        {
            try
            {
                this.iPushRepository.UpdateCounter(contactId, notificationId);
            }
            catch (LSOmniServiceException ex)
            {
                logger.Error(config.LSKey.Key, ex, "PushNotificationUpdateCounter failed");
                throw;
            }
        }
        public virtual List<PushNotification> PushNotificationsGetNext(DateTime dateCreated)
        {
            try
            {
                return iPushRepository.PushOutNotificationGetNext(dateCreated);
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex);
                throw;
            }
        }

        public virtual bool PushNotificationsDoesExist(DateTime dateCreated)
        {
            try
            {
                return iPushRepository.DoesNewPushOutNotificationExist(dateCreated);
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex);
                throw;
            }
        }
    }
}

 