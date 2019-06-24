using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IDeviceRepository
    {
        void DeviceSave(string deviceId, string contactId, string securityToken);
        void Logout(string userName, string deviceId);
        bool DoesDeviceIdExist(string deviceId);
    }
}
