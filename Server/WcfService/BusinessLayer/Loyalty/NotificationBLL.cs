using System;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.BLL.Loyalty
{
    public class NotificationBLL : BaseLoyBLL
    {
        private readonly INotificationRepository iRepository;
        private readonly IPushNotificationRepository iPushRepository;
        private readonly IImageRepository iImageRepository;

        public NotificationBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
            this.iRepository = GetDbRepository<INotificationRepository>(config);
            this.iPushRepository = GetDbRepository<IPushNotificationRepository>(config);
            this.iImageRepository = GetDbRepository<IImageRepository>(config);
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
 