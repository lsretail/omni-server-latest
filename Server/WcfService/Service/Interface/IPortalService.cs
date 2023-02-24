using System;
using System.Collections.Generic;
using System.ServiceModel;

using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Portal;

namespace LSOmni.Service
{
    [ServiceContract(Namespace = "http://lsretail.com/LSOmniService/Portal/2019/Service")]
    public interface IPortalService
    {
        [OperationContract]
        PortalUser PortalLogin(string username, string password);
        [OperationContract]
        BOConfiguration SaveConfig(BOConfiguration config);
        [OperationContract]
        void ChangePortalPassword(string currentPassword, string newPassword);
        [OperationContract]
        string CreatePortalUser(PortalUser user);
        [OperationContract]
        void SavePortalUser(PortalUser user);
        [OperationContract]
        List<PortalUser> GetPortalUsers();
        [OperationContract]
        List<LSKey> GetPortalLSKeys();
        [OperationContract]
        void ToggleLSKey(string lsKey, bool toggle);
        [OperationContract]
        BOConfiguration ResetDefault(string lsKey);
        [OperationContract]
        BOConfiguration GetConfig(string lsKey);

        [OperationContract]
        string Ping(string lskey);
        [OperationContract]
        void DeleteTenant(string lsKey);
        [OperationContract]
        void DeleteUser(string username);
        [OperationContract]
        string ResetPassword(string username);
        [OperationContract]
        void ToggleUser(string username, bool toggle);
        [OperationContract]
        void PortalLogout();
    }
}
