using System;
using System.Collections.Generic;

using NLog;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.BLL.Loyalty
{
    public class PushNotificationBLL : BaseLoyBLL
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IPushNotificationRepository iPushRepository;

        public PushNotificationBLL(string securityToken, string deviceId, int timeoutInSeconds)
            : base(securityToken, deviceId, timeoutInSeconds)
        {
            this.iPushRepository = GetDbRepository<IPushNotificationRepository>();
        }

        public PushNotificationBLL(string deviceId, int timeoutInSeconds)
            : this("", deviceId, timeoutInSeconds)
        {
        }

        public PushNotificationBLL(int timeoutInSeconds = 15)
            : this("", "", timeoutInSeconds)
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
                logger.Log(LogLevel.Error, "PushNotificationSave failed", ex);
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
                logger.Log(LogLevel.Error, "PushNotificationDelete failed", ex);
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
                logger.Log(LogLevel.Error, "PushNotificationUpdateToSent failed", ex);
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
                logger.Log(LogLevel.Error, "PushNotificationUpdateCounter failed", ex);
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
                logger.Error(ex);
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
                logger.Error(ex);
                throw;
            }
        }
    }
}

 