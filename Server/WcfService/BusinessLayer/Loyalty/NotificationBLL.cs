using System;
using System.Collections.Generic;
using System.Linq;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.BLL.Loyalty
{
    public class NotificationBLL : BaseLoyBLL
    {
        private INotificationRepository iRepository;
        private IPushNotificationRepository iPushRepository;
        private IImageRepository iImageRepository;

        public NotificationBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
            this.iRepository = GetDbRepository<INotificationRepository>(config);
            this.iPushRepository = GetDbRepository<IPushNotificationRepository>(config);
            this.iImageRepository = GetDbRepository<IImageRepository>(config);
        }

        public virtual Notification NotificationGetById(string id)
        {
            return this.iRepository.NotificationGetById(id);
        }

        public virtual List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications)
        {
            List<Notification> notificationlist = null;

            notificationlist = BOLoyConnection.NotificationsGetByCardId(cardId, numberOfNotifications);

            if (notificationlist == null)
                return new List<Notification>();

            string contactId = notificationlist.FirstOrDefault()?.ContactId;

            foreach (Notification notification in notificationlist)
            {
                //Check if images exist in local db
                List<ImageView> images = iImageRepository.NotificationImagesById(notification.Id);
                if (images == null || images.Count == 0)
                {
                    //if not, get images from BO and save them locally
                    notification.Images = BOLoyConnection.ImageBOGetByKey("Member Notification", notification.Id, string.Empty, string.Empty, 1, true);
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
                    iPushRepository.SavePushNotification(notification.ContactId, notification.Id);
                }
            }

            iRepository.Save(contactId, notificationlist);

            return iRepository.NotificationsGetByContactId(contactId, numberOfNotifications);
        }

        public virtual void NotificationsUpdateStatus(List<string> notificationIds, NotificationStatus notifacationStatus)
        {
            iRepository.NotificationsUpdateStatus(notificationIds, notifacationStatus);
        }
    }
}

 