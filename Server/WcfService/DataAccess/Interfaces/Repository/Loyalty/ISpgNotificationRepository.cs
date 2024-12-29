using System;
using LSOmni.Common.Util;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface ISpgNotificationRepository
    {
        void RegisterNotification(string cardId, string token, Statistics stat);
        void Delete(string cardId, Statistics stat);
        string GetToken(string cardId, Statistics stat);
        void AddLogEntry(string cardId, string message, bool hasError, string errorMsg, Statistics stat);
    }
}