using System;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Portal;

namespace LSOmni.BLL
{
    public class PortalBLL : BaseBLL
    {
        private IUserRepository iUserRepository;
        private IConfigRepository iConfigRepository;

        public PortalBLL(BOConfiguration config) : base(config)
        {
            iUserRepository = GetDbRepository<IUserRepository>(config);
            iConfigRepository = GetDbRepository<IConfigRepository>(config);
        }
        
        public virtual PortalUser PortalLogin(string username, string password)
        {
            if (Security.NAVHash(password) != iUserRepository.GetPassword(username))
            {
                throw new LSOmniServiceException(StatusCode.AuthFailed, "Username and password do not match");
            }
            if (iUserRepository.UserIsNotActive(username))
            {
                throw new LSOmniServiceException(StatusCode.AuthFailed, "User is not active");
            }
            
            return iUserRepository.Login(username);
        }

        public virtual BOConfiguration GetConfig(string lskey)
        {
            string username = ValidateToken();
            if (iUserRepository.HasAccess(username, lskey) == false && iUserRepository.IsAdmin(username) == false)
            {
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, "User does not have permission to view requested data");
            }
            return iConfigRepository.ConfigGet(lskey);
        }

        public virtual void ChangePassword(string oldPassword, string newPassword)
        {
            string username = ValidateToken();

            if (Security.NAVHash(oldPassword) != iUserRepository.GetPassword(username))
            {
                throw new LSOmniServiceException(StatusCode.AuthFailed, "Current password does not match");
            }

            iUserRepository.ChangePassword(username, Security.NAVHash(newPassword));
        }

        public virtual string CreatePortalUser(PortalUser user)
        {
            string username = ValidateToken();
            if (iUserRepository.IsAdmin(username) == false)
            {
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, "User does not have permission add users");
            }
            string pwd = CreateRandomPassword();
            iUserRepository.CreatePortalUser(user.UserName, Security.NAVHash(pwd));
            return pwd;
        }

        public virtual BOConfiguration SaveConfig(BOConfiguration config)
        {
            string username = ValidateToken();
            if (iUserRepository.IsAdmin(username) == false && iConfigRepository.ConfigExists(config.LSKey.Key) == false)
            {
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, "User does not have permission add tenants");
            }

            iConfigRepository.SaveConfig(config);
            return GetConfig(config.LSKey.Key);
        }

        public virtual void SavePortalUser(PortalUser user)
        {
            string username = ValidateToken();
            if (iUserRepository.IsAdmin(username) == false)
            {
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, "User does not have permission add users");
            }

            iUserRepository.SavePortalUser(user);
        }

        public virtual List<PortalUser> GetUsers()
        {
            string username = ValidateToken();
            if (iUserRepository.IsAdmin(username) == false)
            {
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, "User does not have permission to view requested data");
            }
            return iUserRepository.GetPortalUsers();
        }

        public virtual List<LSKey> GetAllKeys()
        {
            string username = ValidateToken();
            return iUserRepository.GetAllKeys(username, iUserRepository.IsAdmin(username));
        }

        public virtual void ToggleLSKey(string lskey, bool toggle)
        {
            string username = ValidateToken();
            if (iUserRepository.HasAccess(username, lskey) == false && iUserRepository.IsAdmin(username) == false)
            {
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, "User does not have permission to edit LSKeys");
            }

            iConfigRepository.ToggleLSKey(lskey, toggle);
        }

        public virtual BOConfiguration ResetDefault(string lskey)
        {
            string username = ValidateToken();
            if (iUserRepository.IsAdmin(username) == false && iUserRepository.HasAccess(username, lskey) == false)
            {
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, "You do not have permission to edit this config");
            }

            iConfigRepository.ResetDefaults(lskey);
            return iConfigRepository.ConfigGet(lskey);
        }

        public virtual void DeleteTenant(string lskey)
        {
            string username = ValidateToken();
            if (iUserRepository.IsAdmin(username) == false && iUserRepository.HasAccess(username, lskey) == false)
            {
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, "You do not have permission to delete this config");
            }

            iConfigRepository.Delete(lskey);
        }

        public virtual void DeleteUser(string username)
        {
            string user = ValidateToken();
            if (iUserRepository.IsAdmin(user) == false)
            {
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, "You do not have permission to delete this user");
            }

            iUserRepository.DeleteUser(username);
        }

        public virtual string ResetPassword(string username)
        {
            string user = ValidateToken();
            if (iUserRepository.IsAdmin(user) == false)
            {
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, "You do not have permission to reset password for this user");
            }

            string password = CreateRandomPassword();
            iUserRepository.ResetPassword(username, Security.NAVHash(password));
            return password;
        }

        public virtual void ToggleUser(string username, bool toggle)
        {
            string user = ValidateToken();
            if (iUserRepository.IsAdmin(user) == false)
            {
                throw new LSOmniServiceException(StatusCode.AccessNotAllowed, "You do not have permission to toggle this user");
            }

            iUserRepository.ToggleUser(username, toggle);
        }

        public virtual void Logout()
        {
            iUserRepository.Logout(config.SecurityToken);
        }

        public string ValidateToken()
        {
            int timeout = ConfigSetting.GetInt("Portal.Token.Timeout");
            DateTime date = iUserRepository.GetTokenDate(config.SecurityToken);
            if(DateTime.UtcNow > date.AddMinutes(timeout))
            {
                Logout();
                throw new LSOmniServiceException(StatusCode.SecurityTokenInvalid, "Login session has expired, please log in again");
            }
            return iUserRepository.GetUsernameByToken(config.SecurityToken);
        }

        private static string CreateRandomPassword(int length = 10)
        {
            // Create a string of characters, numbers, special characters that allowed in the password  
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            Random random = new Random();

            // Select one random character at a time from the string  
            // and create an array of chars  
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);
        }
    }
}
