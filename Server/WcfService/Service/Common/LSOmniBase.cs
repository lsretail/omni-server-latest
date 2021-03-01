﻿using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

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
        private const string LSRETAIL_LANGCODE = "LSRETAIL-LANGCODE";
        private const string LSRETAIL_DEVICEID = "LSRETAIL-DEVICEID";
        private const string LSRETAIL_TIMEOUT = "LSRETAIL-TIMEOUT";     // timeout set by client

        private const int maxNumberReturned = 1000;
        private static LSLogger logger = new LSLogger();
        private const System.Net.HttpStatusCode exStatusCode = System.Net.HttpStatusCode.RequestedRangeNotSatisfiable; //code=416

        private string version = string.Empty;
        private string languageCode = string.Empty;
        private string deviceId = string.Empty;
        private int clientTimeOutInSeconds = 0;
        protected string clientIPAddress = string.Empty;
        protected BOConfiguration config = null;

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
                }

                //WebOperationContext.Current.IncomingRequest.Headers["Authorization"]
                //get the security token from request header
                if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers[HEADER_TOKEN] != null)
                    config.SecurityToken = WebOperationContext.Current.IncomingRequest.Headers[HEADER_TOKEN].ToString();

                if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers[HEADER_KEY] != null)
                    config.LSKey.Key = WebOperationContext.Current.IncomingRequest.Headers[HEADER_KEY].ToString();

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

                //silver light must send x instead of empty token!! strip it out
                if (config.SecurityToken.ToLower() == "x" || config.SecurityToken.ToLower() == "securitytoken")
                    config.SecurityToken = "";

                //token should be here except for login
                logger.Info(config.LSKey.Key, @"{0}=[{1}] {2} port:{3} - clientIP:[{4}] UserAgent: [{5}]  - Version: [{6}]  ClientVersion: [{7}] LangCode: [{8}]  deviceId: [{9}] clientTimeOut: [{10}] ",
                    HEADER_TOKEN, string.Empty, serverUri, port, clientIPAddress, userAgent, Version(), version, languageCode, deviceId, clientTimeOutInSeconds.ToString());

                config = GetConfig(config);

                //ValidateConfiguration();
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, "SecurityToken:{0} = [{1}] {2} port:{3} - clientIP:[{4}] UserAgent: [{5}]  - Version: [{6}]  ClientVersion: [{7}] LangCode: [{8}]  deviceId: [{9}] clientTimeOut: [{10}]",
                    HEADER_TOKEN, string.Empty, serverUri, port, clientIPAddress, userAgent, Version(), version, languageCode, deviceId, clientTimeOutInSeconds.ToString());
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
            string omniDb = "";
            string navDb = "";
            string navWs = "";
            string ver = "";
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
                // Nav returns version number, Ax returns "AX", One returns "LS One"
                ver = bll.PingWs();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("LS One") || (ex.InnerException != null && ex.InnerException.Message.Contains("LS One")))
                {
                    // We can basically discard this message since the web service is not applicable when using LS One. If we don't set an empty string
                    // the app will show a duplicate error string which we don't want. The whitespace is here so we don't get a "Successfully connected to Nav web service" message.
                    navWs = " ";
                }
                else
                {
                    if (ex.Message.Contains("401"))
                        navWs += "[The LS Central WS User name or Password is incorrect]";
                    else
                        navWs = string.Format("[Failed to Ping LS Central Web Service {0}]", ex.Message);
                }
                logger.Error(config.LSKey.Key, ex, navWs);
            }

            try
            {
                ConfigBLL bll = new ConfigBLL(config);
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
                    navDb = string.Format("[Failed to connect to LS Central DB {0}]", ex.Message);
                }
                logger.Error(config.LSKey.Key, ex, navDb);
            }

            string omniver = string.Format(" LS Commerce Service:{0}", Version());

            //any errors ?
            string msg = "";
            if (omniDb.Length > 0 || navWs.Length > 0 || navDb.Length > 0)
            {
                if (omniDb.Length == 0)
                    msg += " [Successfully connected to LS Commerce Service DB]" + omniver;

                if (ver.Contains("One"))
                {
                    if (navDb.Length == 0)
                        msg += " [Successfully connected to LS One DB]";
                }
                else
                {
                    if (navDb.Length == 0)
                        msg += " [Successfully connected to LS Central DB]";
                    if (navWs.Length == 0)
                        msg += " [Successfully connected to LS Central WS] " + ver;
                }

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
                if (ver.Contains("One"))
                {
                    msg = "Successfully connected to [LS Commerce Service DB] & [LS One DB] " + omniver;
                }
                else
                {
                    msg = "Successfully connected to [LS Commerce Service DB] & [LS Central DB] & [LS Central WS] " + ver + omniver;
                }

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
                OmniEnvironment env = new OmniEnvironment();
                env.Currency = bll.CurrencyGetLocal();
                env.Version = this.Version();
                env.PasswordPolicy = config.SettingsGetByKey(ConfigKey.Password_Policy);
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
                logger.Debug(config.LSKey.Key, "Id: {0}  imageSize: {1}", id, imageSize.ToString());

                ImageBLL bll = new ImageBLL(config);
                ImageView imgView = bll.ImageSizeGetById(id, imageSize);
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
