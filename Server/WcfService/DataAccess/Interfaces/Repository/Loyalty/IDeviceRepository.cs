using System;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IDeviceRepository
    {
        void DeviceSave(string deviceId, string contactId, string securityToken);
        bool DoesDeviceIdExist(string deviceId);
    }
}
