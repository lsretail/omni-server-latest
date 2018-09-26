using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using LSOmni.BLL;
using LSOmni.BLL.Loyalty;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;

using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.Service
{
    /* Security 
     * User + pwd is always sent from client in Basi Auth string. (Headers["Authorization"])
     * When basic auth is enabled on IIS the web.config under LSOmniService needs to updated
     *   authenticationScheme="Anonymous"  to "Basic"  for json and then IIS will block unwanted users
     *   and LSOmniService will never see the incoming request.
     * When Anonymous auth is used we still get the username pwd from client and can check if they are valid
     *   by comparing to keys in the AppSettings table 
     *    <add key="security_basicauth_validation" value="false"/>   set to true
     *    and then the user/pwd is "compared to security_basicauth_username"   Keys
     *    no changed to web.config needed and no configuration needed on IIS
     *    
     * The username and password should be unique for each customer - when app is built
     * */

    /// <summary>
    /// Base class for JSON, SOAP client and XML 
    /// </summary>
    public partial class LSOmniBase
    {
        private const string HEADER_TOKEN = "LSRETAIL-TOKEN";           // used for device security
        private const string LSRETAIL_VERSION = "LSRETAIL-VERSION";
        private const string LSRETAIL_LANGCODE = "LSRETAIL-LANGCODE";
        private const string LSRETAIL_DEVICEID = "LSRETAIL-DEVICEID";
        private const string LSRETAIL_TIMEOUT = "LSRETAIL-TIMEOUT";     // timeout set by client

        private const int maxNumberReturned = 1000;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const System.Net.HttpStatusCode exStatusCode = System.Net.HttpStatusCode.RequestedRangeNotSatisfiable; //code=416

        private string securityToken = string.Empty;
        private string version = string.Empty;
        private string languageCode = string.Empty;
        private string deviceId = string.Empty;
        private int clientTimeOutInSeconds = 0;
        protected string clientIPAddress = string.Empty;

        private static object ConfigValidationRunOnce = null; //

        private string serverUri = string.Empty; //absoluteUri
        private string port = string.Empty;
        private string baseUriOrignalString = string.Empty;

        public LSOmniBase()
        {
            this.baseUriOrignalString = string.Empty;
            this.port = string.Empty;
            string userAgent = string.Empty;
            string basicAuthHeader = string.Empty;
            this.version = "1.0";
            this.languageCode = "";
            this.deviceId = "";
            this.clientTimeOutInSeconds = 0; // set to 0, read timeout from config file if it is 0

            try
            {
                if (OperationContext.Current != null)
                {
                    //get the IPAddress of client
                    var endpoint = (System.ServiceModel.Channels.RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name];
                    clientIPAddress = endpoint.Address;
                    //get uri and port that server listen on
                    if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest != null && WebOperationContext.Current.IncomingRequest.UriTemplateMatch != null)
                    {
                        // Used when returnin URL back to client
                        //orginalString has the string without any changes made. 
                        // orgstring:     HTTP://www.ConToso.com:80//thick%20and%20thin.htm  
                        // vs formatted   http://www.contoso.com/thick and thin.htm
                        //however this does not give us the web request, ex: the ping is missing http://localhost/LSOmniService/json.svc/ping
                        //others may have Uri where the Host may have been changed 
                        // this gives us  "lsretail.cloudapp.net"  vs only getting "lsretail"
                        //  and I get the IPAddess, not  macbook.lsretail.local when I use IPAddress on client
                        //
                        baseUriOrignalString = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri.OriginalString;
                    }
                    else
                        baseUriOrignalString = OperationContext.Current.IncomingMessageProperties.Via.OriginalString;

                    serverUri = OperationContext.Current.IncomingMessageProperties.Via.AbsoluteUri;
                    port = OperationContext.Current.IncomingMessageProperties.Via.Port.ToString();
                }

                //WebOperationContext.Current.IncomingRequest.Headers["Authorization"]
                //get the securtytoken from request header
                if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers[HEADER_TOKEN] != null)
                    securityToken = WebOperationContext.Current.IncomingRequest.Headers[HEADER_TOKEN].ToString();

                if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers[LSRETAIL_VERSION] != null)
                    version = WebOperationContext.Current.IncomingRequest.Headers[LSRETAIL_VERSION];

                if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers[LSRETAIL_LANGCODE] != null)
                    languageCode = WebOperationContext.Current.IncomingRequest.Headers[LSRETAIL_LANGCODE];

                if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers[LSRETAIL_DEVICEID] != null)
                    deviceId = WebOperationContext.Current.IncomingRequest.Headers[LSRETAIL_DEVICEID];

                if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers[LSRETAIL_TIMEOUT] != null)
                {
                    string tout = WebOperationContext.Current.IncomingRequest.Headers[LSRETAIL_TIMEOUT];
                    try
                    {
                        int.TryParse(tout, out clientTimeOutInSeconds);
                    }
                    catch
                    {
                    }
                }

                if (WebOperationContext.Current != null)
                    userAgent = WebOperationContext.Current.IncomingRequest.Headers["User-Agent"];

                //silverlight must send x instead of empty token!! strip it out
                if (securityToken.ToLower() == "x")
                    securityToken = "";

                //token should be here except for login
                logger.Info(@"{0}=[{1}] {2} port:{3} - clientIP:[{4}] UserAgent: [{5}]  - Version: [{6}]  ClientVersion: [{7}] LangCode: [{8}]  deviceId: [{9}] clientTimeOut: [{10}] ",
                    HEADER_TOKEN, securityToken, serverUri, port, clientIPAddress, userAgent, Version(), version, languageCode, deviceId, clientTimeOutInSeconds.ToString());

                //ValidateVersion(version, Version()); //throws exception if fails
                ValidateConfiguration();
            }
            catch (Exception ex)
            {
                logger.Error("SecurityToken:{0} = [{1}] {2} port:{3} - clientIP:[{4}] UserAgent: [{5}]  - Version: [{6}]  ClientVersion: [{7}] LangCode: [{8}]  deviceId: [{9}] clientTimeOut: [{10}]",
                    HEADER_TOKEN, securityToken, serverUri, port, clientIPAddress, userAgent, Version(), version, languageCode, deviceId, clientTimeOutInSeconds.ToString());
                logger.Log(LogLevel.Error, ex, "LSOmniServiceBase() exception");
                throw ex;
            }
        }

        #region PING test methods

        /// <summary>
        /// Simple ping that connects to database and web service
        /// </summary>
        /// <returns>Text, success or failure</returns>
        public virtual string Ping()
        {
            string omniDb = "";
            string navDb = "";
            string navWs = "";
            string ver = "";
            try
            {
                logger.Debug("Ping");
                AppSettingsBLL bll = new AppSettingsBLL();
                bll.PingOmniDb();
            }
            catch (Exception ex)
            {
                omniDb = string.Format("[Failed to connect to LSOmni Db {0}]", ex.Message);
                logger.Log(LogLevel.Error, ex, omniDb);
            }

            try
            {
                AppSettingsBLL bll = new AppSettingsBLL();
                // Nav returns version number, Ax returns "AX", One returns "LS One"
                ver = bll.PingWs(clientIPAddress);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("AX"))
                {
                    if (ex.Message.Contains("401"))
                        navWs += "[The AX web service User name or Password are incorrect]";
                    else
                        navWs = string.Format("[Failed to Ping AX web service {0}]", ex.Message);
                }
                else if (ex.Message.Contains("LS One") || (ex.InnerException != null && ex.InnerException.Message.Contains("LS One")))
                {
                    // We can basically discard this message since the web service is not applicable when using LS One. If we don't set an empty string
                    // the app will show a duplicate error string which we don't want. The whitespace is here so we don't get a "Succsessfully connected to Nav web service" message.
                    navWs = " ";
                }
                else
                {
                    if (ex.Message.Contains("401"))
                        navWs += "[The NAV web service User name or Password are incorrect]";
                    else
                        navWs = string.Format("[Failed to Ping NAV web service {0}]", ex.Message);
                }
                logger.Log(LogLevel.Error, ex, navWs);
            }

            try
            {
                AppSettingsBLL bll = new AppSettingsBLL();
                bll.PingNavDb();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("LS One") || (ex.InnerException != null && ex.InnerException.Message.Contains("LS One")))
                {
                    navDb += $"[Could not connect to LS One. {(ex.InnerException != null ? ex.InnerException.Message : ex.Message)}]";
                }
                else
                {
                    navDb = string.Format("[Failed to connect to NAV Db {0}]", ex.Message);
                }
                logger.Log(LogLevel.Error, ex, navDb);
            }

            string omniver = string.Format(" OMNI: {0}", Version());

            //any errors ?
            string msg = "";
            if (omniDb.Length > 0 || navWs.Length > 0 || navDb.Length > 0)
            {
                if (omniDb.Length == 0)
                    msg += " [Successfully connected to LSOmni Db]" + omniver;

                if (ver.Contains("AX"))
                {
                    if (navDb.Length == 0)
                        msg += " [Successfully connected to AX Db]";
                    if (navWs.Length == 0)
                        msg += " [Successfully connected to AX web service]";
                }
                else if (ver.Contains("One"))
                {
                    if (navDb.Length == 0)
                        msg += " [Successfully connected to One Db]";
                }
                else
                {
                    if (navDb.Length == 0)
                        msg += " [Successfully connected to NAV Db]";
                    if (navWs.Length == 0)
                        msg += " [Successfully connected to NAV web service] " + ver;
                }

                //collect the failure
                if (omniDb.Length > 0)
                    msg += "  " + omniDb;
                if (navDb.Length > 0)
                    msg += "  " + navDb;
                if (navWs.Length > 0)
                    msg += "  " + navWs;

                logger.Debug(msg);
                return string.Format("*** ERROR *** {0} ", msg);
            }
            else
            {
                if (ver.Contains("AX"))
                {
                    msg = "Successfully connected to [LSOmni Db] & [AX Db] & [AX web service] " + ver + omniver;
                }
                else if (ver.Contains("One"))
                {
                    msg = "Successfully connected to [LSOmni Db] & [One Db] " + omniver;
                }
                else
                {
                    msg = "Successfully connected to [LSOmni Db] & [NAV Db] & [NAV web service] " + ver + omniver;
                }

                logger.Debug("PONG OK  {0} ", msg);
                return string.Format("PONG OK. {0} ", msg);
            }
        }

        /// <summary>
        /// Simple ping that connects to database and returns a status code
        /// </summary>
        /// <returns>StatusCode</returns>
        public virtual StatusCode PingStatus()
        {
            return StatusCode.OK;
        }

        /// <summary>
        /// Get web service version number
        /// </summary>
        /// <returns>Version number</returns>
        public virtual string Version()
        {
            return System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;
        }

        /// <summary>
        /// Get the web service environment  
        /// </summary>
        /// <returns>Environment</returns>
        public virtual OmniEnvironment Environment()
        {
            try
            {
                logger.Debug("EnvironmentGet()");
                CurrencyBLL bll = new CurrencyBLL(clientTimeOutInSeconds);
                OmniEnvironment env = new OmniEnvironment();
                env.Currency = bll.CurrencyGetLocal();
                env.Version = this.Version();

                AppSettingsBLL appBll = new AppSettingsBLL();
                env.PasswordPolicy = appBll.AppSettingsGetByKey(AppSettingsKey.Password_Policy, this.languageCode);
                return env;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get Environment");
                return null; //never gets here
            }
        }

        #endregion

        #region images

        /// <summary>
        /// Get image based on id
        /// </summary>
        /// <param name="id">id of image</param>
        /// <param name="imageSize">size of image 100x100</param>
        /// <returns>List of ImageViews</returns>
        public virtual ImageView ImageGetById(string id, ImageSize imageSize)
        {
            try
            {
                logger.Debug("Id: {0}  imageSize: {1}", id, imageSize.ToString());

                ImageBLL bll = new ImageBLL();
                ImageView imgView = bll.ImageSizeGetById(id, imageSize);
                if (imgView != null)
                {
                    // http://localhost/LSOmniService/json.svc/ImageStreamGetById?width=255&height=455&id=66
                    imgView.Location = GetImageStreamUrl(imgView);
                }
                return imgView;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("Failed to ImageGetById() id: {0}  imageSize: {1}", id, imageSize.ToString()));
                return null; // never gets here
            }
        }

        #endregion images

        #region AppSettings

        public virtual string AppSettingsGetByKey(AppSettingsKey key, string languageCode)
        {
            try
            {
                logger.Debug("key:{0} languageCode:{1}", key, languageCode);
                AppSettingsBLL appSettingsBLL = new AppSettingsBLL(); //no security token neede
                return appSettingsBLL.AppSettingsGetByKey(key, languageCode);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("key:{0} languageCode:{1}", key, languageCode));
                return null; //never gets here
            }
        }

        public virtual void AppSettingSetByKey(AppSettingsKey key, string value, string languageCode)
        {
            try
            {
                logger.Debug("key:{0} languageCode:{1} value:{2}", key, languageCode, value);
                AppSettingsBLL appSettingsBLL = new AppSettingsBLL(); //no security token neede
                appSettingsBLL.AppSettingsSetByKey(key, value, languageCode);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, string.Format("key:{0} languageCode:{1}", key, languageCode));
            }
        }

        #endregion

        #region PushNotification

        public virtual bool PushNotificationSave(PushNotificationRequest pushNotificationRequest)
        {
            try
            {
                logger.Debug(LogJson(pushNotificationRequest));
                PushNotificationBLL bll = new PushNotificationBLL(this.deviceId, clientTimeOutInSeconds); //no security token needed
                return bll.PushNotificationSave(pushNotificationRequest);
            }
            catch (Exception ex)
            {
                logger.Error(LogJson(pushNotificationRequest));
                HandleExceptions(ex, string.Format("pushNotificationRequest:{0}  ", pushNotificationRequest.ToString()));
                return false; //never gets here
            }
        }
        public virtual bool PushNotificationDelete(string deviceId)
        {
            try
            {
                logger.Debug("deviceId: " + deviceId);
                PushNotificationBLL bll = new PushNotificationBLL(this.deviceId, clientTimeOutInSeconds);
                return bll.PushNotificationDelete(deviceId);
            }
            catch (Exception ex)
            {
                logger.Debug("deviceId: " + deviceId);
                HandleExceptions(ex, string.Format("deviceId:{0}  ", deviceId));
                return false; //never gets here
            }
        }

        #endregion PushNotification

        public virtual bool ActivityLogSave(ActivityLog activityLog)
        {
            try
            {
                logger.Debug(LogJson(activityLog));
                ActivityLogBLL bll = new ActivityLogBLL(this.deviceId, this.clientIPAddress, clientTimeOutInSeconds); //no security token needed
                return bll.Save(activityLog);
            }
            catch (Exception ex)
            {
                logger.Error(LogJson(activityLog));
                HandleExceptions(ex, string.Format("activityLog:{0}  ", activityLog.ToString()));
                return false; //never gets here
            }
        }

        public virtual string GetLogLines()
        {
            try
            {
                FileTarget fileTarget = null;
                Target target = LogManager.Configuration.FindTargetByName("file");
                if (target == null)
                {
                    throw new Exception("Could not find target named: file");
                }

                WrapperTargetBase wrapperTarget = target as WrapperTargetBase;
                if (wrapperTarget == null)
                {
                    fileTarget = target as FileTarget;
                }
                else
                {
                    fileTarget = wrapperTarget.WrappedTarget as FileTarget;
                }

                if (fileTarget == null)
                {
                    throw new Exception("Could not get a FileTarget from " + target.GetType());
                }

                LogEventInfo logEventInfo = new LogEventInfo()
                {
                    TimeStamp = DateTime.Now
                };

                string fileName = fileTarget.FileName.Render(logEventInfo);

                StringBuilder txt = new StringBuilder();
                foreach (string s in File.ReadLines(fileName).Reverse().Take(200).ToList())
                {
                    txt.AppendLine(s);
                }
                return txt.ToString();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
                return ex.Message;
            }
        }
    }
}
