
using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface INotificationRepository
    {
        Notification NotificationGetById(string id);
        void NotificationsUpdateStatus(string contactId,string deviceId, List<string> notificationIds, NotificationStatus notificationStatus);
        List<Notification> NotificationSearch(string contactId, string search, int maxNumberOfLists);
        void Save(string contactId, List<Notification> notifications);
        List<Notification> NotificationsGetByContactId(string contactId, int numberOfNotifications);
    }
}