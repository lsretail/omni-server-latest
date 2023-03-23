using System;
using System.Collections.Generic;

using LSOmni.BLL;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Portal;

namespace LSOmni.Service
{
    public partial class LSOmniBase
    {
        public virtual PortalUser PortalLogin(string username, string password)
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("PortalLogin() - user:{0}", username));

                PortalBLL bll = new PortalBLL(config);
                return bll.PortalLogin(username, password);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to login user:{0}", username);
                return null; //never gets here
            }
        }

        public virtual BOConfiguration SaveConfig(BOConfiguration configuration)
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("SaveConfig() - Description:{0}", config.LSKey.Description));

                PortalBLL bll = new PortalBLL(config);
                return bll.SaveConfig(configuration);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to save config:{0}", config.LSKey.Description);
                return null; //never gets here
            }
        }

        public virtual BOConfiguration GetConfig(string lskey)
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("GetConfig() - LSKey:{0}", lskey));

                PortalBLL bll = new PortalBLL(config);
                return bll.GetConfig(lskey);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get config - LSKey:{0}", lskey);
                return null; //never gets here
            }
        }

        public virtual void ChangePortalPassword(string currentPassword, string newPassword)
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("PortalChangePassword() - Token:{0}", config.SecurityToken));

                PortalBLL bll = new PortalBLL(config);
                bll.ChangePassword(currentPassword, newPassword);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to change password - Token:{0}", config.SecurityToken);
            }
        }

        public virtual string CreatePortalUser(PortalUser user)
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("CreatePortalUser() - Token: {0}, user:{0}", config.SecurityToken, user.UserName));

                PortalBLL bll = new PortalBLL(config);
                return bll.CreatePortalUser(user);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to create user:{0}", user.UserName);
            }
            return "";
        }

        public virtual void SavePortalUser(PortalUser user)
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("SavePortalUser() - Token:{0}, savedUser:{1}", config.SecurityToken, user.UserName));

                PortalBLL bll = new PortalBLL(config);
                bll.SavePortalUser( user);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to save user:{0}", user.UserName);
            }
        }

        public virtual List<PortalUser> GetPortalUsers()
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("GetPortalUsers() - Token:{0}", config.SecurityToken));

                PortalBLL bll = new PortalBLL(config);
                return bll.GetUsers();
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to fetch users - Token :{0}", config.SecurityToken);
                return null; //never gets here
            }
        }

        public virtual List<LSKey> GetPortalLSKeys()
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("GetPortalLSKeys() - Token:{0}", config.SecurityToken));

                PortalBLL bll = new PortalBLL(config);
                return bll.GetAllKeys();
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to fetch LSKeys for Token:{0}", config.SecurityToken);
                return null; //never gets here
            }
        }

        public virtual void ToggleLSKey(string lskey, bool toggle)
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("ToggleLSKey() - Token:{0}, lskey:{1}, toggle:{2}", config.SecurityToken, lskey, toggle.ToString()));

                PortalBLL bll = new PortalBLL(config);
                bll.ToggleLSKey(lskey, toggle);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to toggle LSKey:{0}, Token:{1}", lskey, config.SecurityToken);
            }
        }

        public virtual BOConfiguration ResetDefault(string lskey)
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("ResetDefault() - Token:{0}, lskey:{1}", config.SecurityToken, lskey));

                PortalBLL bll = new PortalBLL(config);
                return bll.ResetDefault(lskey);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to reset to defaults LSKey:{0}", lskey);
            }
            return null;
        }

        public virtual void DeleteTenant(string lskey)
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("DeleteTenant() - Token:{0}, lskey:{1}", config.SecurityToken, lskey));

                PortalBLL bll = new PortalBLL(config);
                bll.DeleteTenant(lskey);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to delete tenant LSKey:{0}", lskey);
            }
        }

        public virtual void DeleteUser(string username)
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("DeleteUser() - Token:{0}, user:{1}", config.SecurityToken, username));

                PortalBLL bll = new PortalBLL(config);
                bll.DeleteUser(username);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to delete tenant user:{0}", username);
            }
        }

        public virtual string ResetPassword(string username)
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("ResetPassword() - Token:{0}, user:{1}", config.SecurityToken, username));

                PortalBLL bll = new PortalBLL(config);
                return bll.ResetPassword(username);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to reset password for user:{0}", username);
            }
            return "";
        }

        public virtual void ToggleUser(string username, bool toggle)
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("ToggleUser() - Token:{0}, user:{1}", config.SecurityToken, username));

                PortalBLL bll = new PortalBLL(config);
                bll.ToggleUser(username, toggle);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to toggle user:{0}", username);
            }
        }

        public virtual void PortalLogout()
        {
            try
            {
                logger.Debug("ADMINPORTAL", string.Format("Logout() - Token:{0}", config.SecurityToken));

                PortalBLL bll = new PortalBLL(config);
                bll.Logout();
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to logout, Token:{0}", config.SecurityToken);
            }
        }

        public virtual string Ping(string lskey)
        {
            string omniDb = string.Empty;
            string navDb = string.Empty;
            string navWs = string.Empty;
            string ver = string.Empty;
            string tenVer = string.Empty;
            string navDBRet = string.Empty;
            BOConfiguration config = GetConfig(lskey);
            
            try
            {
                logger.Debug(config.LSKey.Key, "Ping");
                ConfigBLL bll = new ConfigBLL(config);
                bll.PingOmniDB();
            }
            catch (Exception ex)
            {
                omniDb = string.Format("[Failed to connect to Commerce Service for LS Central DB {0}]", ex.Message);
                logger.Error(config.LSKey.Key, ex, omniDb);
            }

            try
            {
                ConfigBLL bll = new ConfigBLL(config);
                // Nav returns version number, Ax returns "AX"
                ver = bll.PingWs(out string centralVer);

                tenVer = config.SettingsGetByKey(ConfigKey.LSNAV_Version);
                if (string.IsNullOrEmpty(tenVer))
                {
                    tenVer = centralVer;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("401"))
                    navWs += "[The LS Central WS User name or Password is incorrect]";
                else
                    navWs = string.Format("[Failed to Ping LS Central Web Service {0}]", ex.Message);
                logger.Error(config.LSKey.Key, ex, navWs);
            }

            try
            {
                ConfigBLL bll = new ConfigBLL(config);
                navDBRet = bll.PingNavDb();
            }
            catch (Exception ex)
            {
                navDb = string.Format("[Failed to connect to LS Central DB {0}]", ex.Message);
                logger.Error(config.LSKey.Key, ex, navDb);
            }

            string omniver = string.Format(" Commerce Service for LS Central:{0}", Version());

            //any errors ?
            string msg = "";
            if (omniDb.Length > 0 || navWs.Length > 0 || navDb.Length > 0)
            {
                if (omniDb.Length == 0)
                    msg += " [Successfully connected to Commerce Service for LS Central DB]" + omniver;

                if (navDb.Length == 0)
                    msg += navDBRet.Equals("SaaS") ? " [SaaS Mode]" : " [Successfully connected to LS Central DB]";
                if (navWs.Length == 0)
                    msg += " [Successfully connected to LS Central WS] " + tenVer + " (" + ver + ")";

                //collect the failure
                if (omniDb.Length > 0)
                    msg += "  " + omniDb;
                if (navDb.Length > 0)
                    msg += "  " + navDb;
                if (navWs.Length > 0)
                    msg += "  " + navWs;

                logger.Debug(config.LSKey.Key, msg);
                return string.Format("*** ERROR *** {0} ", msg);
            }
            else
            {
                msg = "Successfully connected to [Commerce Service for LS Central DB] & " + (navDBRet.Equals("SaaS") ? "[LS SaaS]" : "[LS Central DB]") + " & [LS Central WS] " + tenVer + " (" + ver + ")" + omniver;
                logger.Debug(config.LSKey.Key, "PONG OK {0} ", msg);
                return string.Format("PONG OK> {0} ", msg);
            }
        }
    }
}
