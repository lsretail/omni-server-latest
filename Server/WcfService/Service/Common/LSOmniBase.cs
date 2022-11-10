using System;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

using LSOmni.BLL;
using LSOmni.BLL.Loyalty;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;

using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.Service
{
    /// <summary>
    /// Base class for JSON, SOAP client and XML 
    /// </summary>

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public partial class LSOmniBase
    {
        private const string HEADER_KEY = "LSRETAIL-KEY";
        private const string HEADER_TOKEN = "LSRETAIL-TOKEN";           // used for device security
        private const string LSRETAIL_VERSION = "LSRETAIL-VERSION";
        private const string LSRETAIL_DEVICEID = "LSRETAIL-DEVICEID";
        private const string LSRETAIL_TIMEOUT = "LSRETAIL-TIMEOUT";     // timeout set by client

        private const int maxNumberReturned = 1000;
        private static LSLogger logger = new LSLogger();
        private const HttpStatusCode exStatusCode = System.Net.HttpStatusCode.RequestedRangeNotSatisfiable; //code=416

        private string version = string.Empty;
        private string deviceId = string.Empty;
        private int clientTimeOutInSeconds = 0;
        protected string clientIPAddress = string.Empty;
        protected BOConfiguration config = null;

        private readonly string serverUri = string.Empty; //absoluteUri
        private readonly string port = string.Empty;
        private readonly string baseUriOrignalString = string.Empty;

        public LSOmniBase()
        {
            this.baseUriOrignalString = string.Empty;
            this.port = string.Empty;
            string userAgent = string.Empty;
            this.version = "1.0";
            this.deviceId = "";
            this.clientTimeOutInSeconds = 0; // set to 0, read timeout from configuration file if it is 0
            this.config = new BOConfiguration();

            try
            {
                if (OperationContext.Current != null)
                {
                    //get the IPAddress of client
                    var endpoint = (System.ServiceModel.Channels.RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name];
                    clientIPAddress = endpoint.Address;
                    //get URl and port that server listen on
                    if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest != null && WebOperationContext.Current.IncomingRequest.UriTemplateMatch != null)
                    {
                        // Used when returning URL back to client
                        //orginalString has the string without any changes made. 
                        // org-string:     HTTP://www.ConToso.com:80//thick%20and%20thin.htm  
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
                    config.IsJson = serverUri.ToLower().Contains("json.svc");
                }

                //WebOperationContext.Current.IncomingRequest.Headers["Authorization"]
                //get the security token from request header
                if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers[HEADER_TOKEN] != null)
                    config.SecurityToken = WebOperationContext.Current.IncomingRequest.Headers[HEADER_TOKEN].ToString();

                if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers[HEADER_KEY] != null)
                    config.LSKey.Key = WebOperationContext.Current.IncomingRequest.Headers[HEADER_KEY].ToString();

                if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers[LSRETAIL_VERSION] != null)
                    version = WebOperationContext.Current.IncomingRequest.Headers[LSRETAIL_VERSION];

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

                //silver light must send x instead of empty token!! strip it out
                if (config.SecurityToken.ToLower() == "x" || config.SecurityToken.ToLower() == "securitytoken")
                    config.SecurityToken = "";

                //token should be here except for login
                logger.Debug(config.LSKey.Key, @"{0}=[{1}] {2} port:{3} - clientIP:[{4}] UserAgent:[{5}] Version:[{6}] ClientVersion:[{7}] deviceId:[{8}] clientTimeOut:[{9}]",
                    HEADER_TOKEN, config.SecurityToken, serverUri, port, clientIPAddress, userAgent, Version(), version, deviceId, clientTimeOutInSeconds);

                config = GetConfig(config);
                CheckToken();
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, "{0} port:{1} - clientIP:[{2}]", serverUri, port, clientIPAddress);
                HandleExceptions(ex, ex.Message);
            }
        }

        #region PING test methods

        /// <summary>
        /// Simple ping that connects to database and web service
        /// </summary>
        /// <returns>Text, success or failure</returns>
        public virtual string Ping()
        {
            string omniDb = string.Empty;
            string navDb = string.Empty;
            string navWs = string.Empty;
            string ver = string.Empty;
            string tenVer = string.Empty;
            try
            {
                logger.Debug(config.LSKey.Key, "Ping");
                ConfigBLL bll = new ConfigBLL(config);
                bll.PingOmniDB();
            }
            catch (Exception ex)
            {
                omniDb = string.Format("[Failed to connect to LS Commerce Service DB {0}]", ex.Message);
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
                    logger.Debug(config.LSKey.Key, "Save Retail Version {0} to LSNAV.Version in TenantConfig", centralVer);
                    bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSNAV_Version, centralVer, "string", true, "LS Central Version to use");
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
                bll.PingNavDb();
            }
            catch (Exception ex)
            {
                navDb = string.Format("[Failed to connect to LS Central DB {0}]", ex.Message);
                logger.Error(config.LSKey.Key, ex, navDb);
            }

            string omniver = string.Format(" LS Commerce Service:{0}", Version());

            //any errors ?
            string msg = "";
            if (omniDb.Length > 0 || navWs.Length > 0 || navDb.Length > 0)
            {
                if (omniDb.Length == 0)
                    msg += " [Successfully connected to LS Commerce Service DB]" + omniver;

                if (navDb.Length == 0)
                    msg += " [Successfully connected to LS Central DB]";
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
                msg = "Successfully connected to [LS Commerce Service DB] & [LS Central DB] & [LS Central WS] " + tenVer + " (" + ver + ")" + omniver;
                logger.Debug(config.LSKey.Key, "PONG OK {0} ", msg);
                return string.Format("PONG OK> {0} ", msg);
            }
        }

        /// <summary>
        /// Get the web service environment  
        /// </summary>
        /// <returns>Environment</returns>
        public virtual OmniEnvironment Environment()
        {
            try
            {
                logger.Debug(config.LSKey.Key, "EnvironmentGet()");
                CurrencyBLL bll = new CurrencyBLL(config, clientTimeOutInSeconds);
                return new OmniEnvironment()
                {
                    Currency = bll.CurrencyGetLocal(new Statistics()),
                    Version = this.Version(),
                    PasswordPolicy = config.SettingsGetByKey(ConfigKey.Password_Policy)
                };
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get Environment");
                return null; //never gets here
            }
        }

        private string CheckToken()
        {
            if (config == null)
                return string.Empty;

            string protocol = config.SettingsGetByKey(ConfigKey.BOProtocol);
            if (protocol.ToUpper().Equals("S2S") == false)
                return string.Empty;

            string token = config.SettingsGetByKey(ConfigKey.Central_Token);
            if (string.IsNullOrEmpty(token) == false)
            {
                DateTime regtime = DateTime.MinValue;
                string reg = config.SettingsGetByKey(ConfigKey.Central_TokenTime);
                if (string.IsNullOrEmpty(reg) == false)
                    regtime = Convert.ToDateTime(reg);

                if (regtime > DateTime.Now)
                {
                    return token;
                }
            }

            string clientId = config.SettingsGetByKey(ConfigKey.BOUser);
            string clientSecret = config.SettingsGetByKey(ConfigKey.BOPassword);
            string tenant = config.SettingsGetByKey(ConfigKey.BOTenant);

            //check if the password has been encrypted by our LSOmniPasswordGenerator.exe
            if (DecryptConfigValue.IsEncryptedPwd(clientSecret))
            {
                clientSecret = DecryptConfigValue.DecryptString(clientSecret);
            }

            string scope = "https://api.businesscentral.dynamics.com/.default";
            string authurl = $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token";
            string body = $"grant_type=client_credentials&scope={scope}&client_id={clientId}&client_secret={clientSecret}";

            try
            {
                Uri posturl = new Uri(authurl);
                HttpWebRequest httpWebRequest = (HttpWebRequest)System.Net.WebRequest.Create(posturl);
                httpWebRequest.Method = "POST";

                logger.Debug(config.LSKey.Key, "Send Token request for LS Central to:{0} Message:{1}", posturl.AbsoluteUri, body);
                byte[] byteArray = Encoding.UTF8.GetBytes(body); //json

                httpWebRequest.Accept = "application/json";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.ContentLength = byteArray.Length;

                using (Stream streamWriter = httpWebRequest.GetRequestStream())
                {
                    streamWriter.Write(byteArray, 0, byteArray.Length);
                    streamWriter.Flush();
                }

                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string result = streamReader.ReadToEnd();
                    logger.Debug(config.LSKey.Key, "ECOM Result:[{0}]", result);

                    TokenS2S data = Serialization.Deserialize<TokenS2S>(result);
                    token = data.token_type + " " + data.access_token;

                    ConfigBLL bll = new ConfigBLL();
                    bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.Central_Token, token, "string", true, "Active token");
                    bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.Central_TokenTime, DateTime.Now.AddSeconds(data.expires_in - 90).ToString(), "string", true, "Token Reg");
                    config.SettingsUpdateByKey(ConfigKey.Central_Token, token);
                }
                return token;
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex);
                throw new LSOmniServiceException(StatusCode.SecurityTokenInvalid, "Error getting token", ex);
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
            if (imageSize == null)
                imageSize = new ImageSize();

            try
            {
                logger.Debug(config.LSKey.Key, "Id: {0}  imageSize: {1}", id, imageSize.ToString());

                ImageBLL bll = new ImageBLL(config);
                ImageView imgView = bll.ImageSizeGetById(id, imageSize, new Statistics());
                if (imgView != null)
                {
                    // http://localhost/LSOmniService/json.svc/ImageStreamGetById?width=255&height=455&id=66
                    imgView.StreamURL = GetImageStreamUrl(imgView);
                }
                return imgView;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed: ImageGetById() id:{0} imageSize:{1}", id, imageSize);
                return null; // never gets here
            }
        }

        #endregion images

        #region PushNotification

        public virtual bool PushNotificationSave(PushNotificationRequest pushNotificationRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(pushNotificationRequest));
                PushNotificationBLL bll = new PushNotificationBLL(config, this.deviceId, clientTimeOutInSeconds); //no security token needed
                return bll.PushNotificationSave(pushNotificationRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "pushNotificationRequest:{0}", LogJson(pushNotificationRequest));
                return false; //never gets here
            }
        }

        #endregion PushNotification
    }
}
