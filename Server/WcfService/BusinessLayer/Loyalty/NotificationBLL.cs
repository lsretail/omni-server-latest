using System;
using System.Collections.Generic;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.BLL.Loyalty
{
    public class NotificationBLL : BaseLoyBLL
    {
        private INotificationRepository iRepository;
        private IImageRepository iImageRepository;
        private IPushNotificationRepository iPushRepository;

        public NotificationBLL(int timeoutInSeconds)
            : this("", timeoutInSeconds)
        {
        }

        public NotificationBLL(string securityToken, int timeoutInSeconds)
            : base(securityToken, timeoutInSeconds)
        {
            this.iRepository = GetDbRepository<INotificationRepository>();
            this.iImageRepository = GetDbRepository<IImageRepository>();
            this.iPushRepository = GetDbRepository<IPushNotificationRepository>();
        }

        public virtual Notification NotificationGetById(string id)
        {
            return this.iRepository.NotificationGetById(id);
        }

        public virtual List<Notification> NotificationsGetByContactId(string contactId, int numberOfNotifications)
        {
            ValidateContact(contactId);
            List<Notification> notificationlist = null;

            //need the cardId
            Card card = BOLoyConnection.CardGetByContactId(contactId);
            if (card == null)
            {
                //just in case
                notificationlist = BOLoyConnection.NotificationsGetByContactId(contactId, numberOfNotifications);
            }
            else
            {
                notificationlist = BOLoyConnection.NotificationsGetByCardId(card.Id, numberOfNotifications);
            }

            if (notificationlist == null)
                return new List<Notification>();

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
                    iPushRepository.SavePushNotification(contactId, notification.Id);
                }
            }

            iRepository.Save(contactId, notificationlist);

            return iRepository.NotificationsGetByContactId(contactId, numberOfNotifications);
        }

        public virtual void NotificationsUpdateStatus(string contactId, List<string> notificationIds,
            NotificationStatus notifacationStatus)
        {
            ValidateContact(contactId);
            iRepository.NotificationsUpdateStatus(contactId, base.DeviceId, notificationIds, notifacationStatus);
        }
    }
}

 