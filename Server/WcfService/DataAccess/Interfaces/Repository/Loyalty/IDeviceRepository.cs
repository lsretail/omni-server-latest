using System;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IDeviceRepository
    {
        void DeviceSave(string deviceId, string contactId, string securityToken);
        StatusCode ValidateSecurityToken(string securityToken, out string deviceId, out string cardId);
        bool DoesDeviceIdExist(string deviceId);
        bool SpgUnlockRodDevice(string storeId, string cardId);
        string SpgUnlockRodDeviceCheck(string storeId);
    }
}
