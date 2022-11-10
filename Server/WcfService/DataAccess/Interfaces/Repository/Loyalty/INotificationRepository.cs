
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface INotificationRepository
    {
        Notification NotificationGetById(string id, Statistics stat);
        void NotificationsUpdateStatus(List<string> notificationIds, NotificationStatus notificationStatus, Statistics stat);
        List<Notification> NotificationSearch(string cardId, string search, int maxNumberOfLists, Statistics stat);
        void OrderMessageNotificationSave(string notificationId, string orderId, string cardId, string description, string details, string qrText, Statistics stat);
        void Save(string cardId, List<Notification> notifications, Statistics stat);
        List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications, Statistics stat);
    }
}