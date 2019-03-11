using System;
using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IPushNotificationRepository
    {
        void Save(PushNotificationRequest request);
        void SavePushNotification(string contactId, string notificationId);
        void Delete(string deviceId);
        List<PushNotification> PushOutNotificationGetNext(DateTime dateCreated, 
            int numberOfNotifications = 100);
        bool DoesNewPushOutNotificationExist(DateTime dateCreated);
        void UpdateToSent(string deviceId, string notificationId);
        void UpdateCounter(string deviceId, string notificationId);
    }
}