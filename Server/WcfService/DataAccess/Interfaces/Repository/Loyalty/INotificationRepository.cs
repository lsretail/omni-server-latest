
using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface INotificationRepository
    {
        Notification NotificationGetById(string id);
        void NotificationsUpdateStatus(List<string> notificationIds, NotificationStatus notificationStatus);
        List<Notification> NotificationSearch(string cardId, string search, int maxNumberOfLists);
        void OrderMessageNotificationSave(string notificationId, string orderId, string cardId, string description, string details, string qrText);
        void Save(string cardId, List<Notification> notifications);
        List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications);
    }
}