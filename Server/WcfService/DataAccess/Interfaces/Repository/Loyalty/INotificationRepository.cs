
using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface INotificationRepository
    {
        Notification NotificationGetById(string id);
        void NotificationsUpdateStatus(List<string> notificationIds, NotificationStatus notificationStatus);
        List<Notification> NotificationSearch(string contactId, string search, int maxNumberOfLists);
        void OrderMessageNotificationSave(string notificationId, long orderMessageId, string contactId, string description, string details, string qrText);
        void Save(string contactId, List<Notification> notifications);
        List<Notification> NotificationsGetByContactId(string contactId, int numberOfNotifications);
    }
}