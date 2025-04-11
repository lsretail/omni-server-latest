using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

using LSOmni.BLL;
using LSOmni.BLL.Loyalty;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
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
        private const HttpStatusCode exStatusCode = System.Net.HttpStatusCode.RequestedRangeNotSatisfiable; //code=416

        private static LSLogger logger = new LSLogger();
        private static DateTime lastVersionCheck = DateTime.MinValue;

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

                config.Settings.Add(new TenantSetting(ConfigKey.EncrCode.ToString(), ConfigSetting.GetEncrCode(), string.Empty, "string", false, true));

                config = GetConfig(config);
                ConfigBLL bll = new ConfigBLL(config);
                bll.CheckToken(config);
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
            string navDBRet = string.Empty;
            ConfigBLL bll = null;

            try
            {
                logger.Debug(config.LSKey.Key, "Ping");
                bll = new ConfigBLL(config);
                bll.PingOmniDB();
            }
            catch (Exception ex)
            {
                omniDb = string.Format("[Failed to connect to Commerce Service for LS Central DB {0}]", ex.Message);
                logger.Error(config.LSKey.Key, ex, omniDb);
            }

            try
            {
                double minutes = (double)config.SettingsDecimalGetByKey(ConfigKey.LSNAV_Timeout);
                if (DateTime.Now > lastVersionCheck.AddMinutes(minutes))
                {
                    //reset version in Tenant config once a day
                    lastVersionCheck = DateTime.Now;
                    logger.Debug(config.LSKey.Key, "Reset LSNAV.Version in TenantConfig");
                    bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSNAV_Version, string.Empty, "string", true, "LS Central Version to use");
                }

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
                navDBRet = bll.PingNavDb();
            }
            catch (Exception ex)
            {
                navDb = string.Format("[Failed to connect to LS Central DB {0}]", ex.Message);
                logger.Error(config.LSKey.Key, ex, navDb);
            }

            string omniVer = string.Format(" CS:{0}", Version());

            //any errors ?
            string msg = "";
            if (omniDb.Length > 0 || navWs.Length > 0 || navDb.Length > 0)
            {
                if (omniDb.Length == 0)
                    msg += " [Successfully connected to Commerce Service for LS Central DB]" + omniVer;

                if (navDb.Length == 0)
                    msg += navDBRet.Equals("SaaS") ? " [SaaS Mode]" : " [Successfully connected to LS Central DB]";
                if (navWs.Length == 0)
                    msg += " [Successfully connected to LS Central WS] LS:" + tenVer + " (" + ver + ")";

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
                msg = "Successfully connected to [Commerce Service for LS Central DB] & " + (navDBRet.Equals("SaaS") ? "[LS SaaS]" : "[LS Central DB]") + " & [LS Central WS] LS:" + tenVer + " (" + ver + ")" + omniVer;
                logger.Debug(config.LSKey.Key, "PONG OK {0}", msg);
                return string.Format("PONG OK> {0}", msg);
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

        #endregion
    }
}
