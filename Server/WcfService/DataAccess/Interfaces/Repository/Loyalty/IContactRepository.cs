using System;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IContactRepository
    {
        MemberContact ContactGetById(string id, string alternateId);
        MemberContact ContactGetByCardId(string cardId);
        MemberContact ContactGetByUserName(string userName);
        string ContactUpdate(MemberContact contact);
        string DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model);
        bool DoesContactExist(string userName);
        bool DoesEmailExist(string email);
        bool DoesDeviceIdExist(string deviceId);
        string Login(MemberContact contact, string deviceId, string cardId, string securityToken);
        void Logout(string userName, string deviceId);
        void LoginLog(string userName, string deviceId, string ipAddress, bool failed, bool logout);
        void UpdateAccountBalance(string accountNumber, double balance);
        void DeleteSecurityTokens(string contactId, string skipThisDeviceId);
        void ContactCreate(MemberContact contact, string deviceId);
        void ContactDelete(string userName);
        string ContactGetAccountId(string contactId);
    }
}
 
