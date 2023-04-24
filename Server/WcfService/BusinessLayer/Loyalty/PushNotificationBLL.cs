using System;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.BLL.Loyalty
{
    public class PushNotificationBLL : BaseLoyBLL
    {
        private readonly IPushNotificationRepository iPushRepository;
        private readonly INotificationRepository iRepository;
        private readonly IImageRepository iImageRepository;

        public PushNotificationBLL(BOConfiguration config, string deviceId, int timeoutInSeconds)
            : base(config, deviceId, timeoutInSeconds)
        {
            this.iPushRepository = GetDbRepository<IPushNotificationRepository>(config);
            this.iRepository = GetDbRepository<INotificationRepository>(config);
            this.iImageRepository = GetDbRepository<IImageRepository>(config);
        }

        public PushNotificationBLL(BOConfiguration config, int timeoutInSeconds = 15)
            : this(config, "", timeoutInSeconds)
        {
        }

        public virtual bool PushNotificationSave(PushNotificationRequest pushNotificationRequest)
        {
            //if no ad id is given, get one
            if (pushNotificationRequest == null)
                throw new LSOmniException(StatusCode.ObjectMissing, "pushNotificationRequest cannot be null");

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

        public virtual Notification NotificationGetById(string id, Statistics stat)
        {
            return this.iRepository.NotificationGetById(id, stat);
        }

        public virtual List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications, Statistics stat)
        {
            List<Notification> notificationlist = BOLoyConnection.NotificationsGetByCardId(cardId, numberOfNotifications, stat);
            if (notificationlist == null)
                return new List<Notification>();

            foreach (Notification notification in notificationlist)
            {
                //Check if images exist in local db
                List<ImageView> images = iImageRepository.NotificationImagesById(notification.Id, stat);
                if (images == null || images.Count == 0)
                {
                    //if not, get images from BO and save them locally
                    notification.Images = BOLoyConnection.ImagesGetByKey("Member Notification", notification.Id, string.Empty, string.Empty, 1, true, stat);
                    foreach (ImageView img in notification.Images)
                    {
                        //img.Id = notification.Id;
                        iImageRepository.SaveImageLink(img, "Member Notification", "Member Notification: " + notification.Id,
                            notification.Id, img.Id, 0);
                        iImageRepository.SaveImage(img);
                    }
                }
                if (notification.Status == NotificationStatus.New)
                {
                    iPushRepository.SavePushNotification(notification.ContactId, notification.Id, stat);
                }
            }

            iRepository.Save(cardId, notificationlist, stat);
            return notificationlist;
        }

        public virtual void NotificationsUpdateStatus(List<string> notificationIds, NotificationStatus notifacationStatus, Statistics stat)
        {
            iRepository.NotificationsUpdateStatus(notificationIds, notifacationStatus, stat);
        }
    }
}

 