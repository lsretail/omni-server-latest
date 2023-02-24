using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Portal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSOmni.DataAccess.Interface.Repository
{
    public interface IUserRepository
    {
        List<LSKey> GetKeys(string username);
        List<LSKey> GetAllKeys(string username, bool isAdmin);
        bool IsAdmin(string username);
        bool UserIsNotActive(string username);
        string GetPassword(string username);
        void ChangePassword(string username, string newPassword);
        void CreatePortalUser(string username, string password);
        List<PortalUser> GetPortalUsers();
        void SavePortalUser(PortalUser user);
        bool HasAccess(string username, string lsKey);
        void ResetPassword(string username, string password);
        void DeleteUser(string username);
        void ToggleUser(string username, bool toggle);
        PortalUser Login(string username);
        DateTime GetTokenDate(string token);
        string GetUsernameByToken(string token);
        void Logout(string token);
    }
}
