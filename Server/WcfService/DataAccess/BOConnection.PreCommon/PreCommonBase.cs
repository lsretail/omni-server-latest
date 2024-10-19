using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Net.Security;
using System.Diagnostics;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping;
using LSOmni.DataAccess.BOConnection.PreCommon.JMapping;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.PreCommon
{
    //Navision back office connection
    public partial class PreCommonBase
    {
        private int TimeoutSec = 0;
        public int TimeOutInSeconds
        {
            get
            {
                return TimeoutSec;
            }
            set
            {
                // sever timeout before client (2 sec)
                TimeoutSec = value;
                if (centralWS != null)
                    centralWS.Timeout = (value - 2) * 1000;
                if (centralWS25 != null)
                    centralWS25.Timeout = (value - 2) * 1000;
                if (centralQryWS != null)
                    centralQryWS.Timeout = (value - 2) * 1000;
                if (centralQryWS25 != null)
                    centralQryWS25.Timeout = (value - 2) * 1000;
                if (activityWS != null)
                    activityWS.Timeout = (value - 2) * 1000;
                if (odataWS != null)
                    odataWS.Timeout = (value - 2) * 1000;
            }
        }

        protected static LSLogger logger = new LSLogger();

        private readonly NavWebReference.RetailWebServices navWebReference = null;
        private readonly NavWebReference.RetailWebServices navWebQryReference = null;
        public LSCentral.OmniWrapper centralWS = null;
        public LSCentral25.OmniWrapper centralWS25 = null;
        public LSCentral.OmniWrapper centralQryWS = null;
        public LSCentral25.OmniWrapper centralQryWS25 = null;
        public LSActivity.Activity activityWS = null;
        public LSOData.ODataRequest odataWS = null;
        private readonly int base64ConversionMinLength = 1024 * 100; //50KB 75KB  minimum length to base64 conversion
        private static readonly object Locker = new object();

        public Version LSCVersion = null; //use this in code to check Nav version
        public string NavCompany = string.Empty;

        protected BOConfiguration config = null;

        private readonly string ecomAppId = string.Empty;
        private readonly string ecomAppType = string.Empty;
        private bool ecomAppRestore = false;
        private readonly string ecomAppRestoreFileName = string.Empty;

        public PreCommonBase(BOConfiguration configuration, bool ping = false)
        {
            if (configuration == null && !ping)
            {
                throw new LSOmniServiceException(StatusCode.SecurityTokenInvalid, "SecurityToken invalid");
            }
            config = configuration;

            base64ConversionMinLength = config.SettingsIntGetByKey(ConfigKey.Base64MinXmlSizeInKB) * 1024; //in KB
            ecomAppId = config.SettingsGetByKey(ConfigKey.NavAppId);
            ecomAppType = config.SettingsGetByKey(ConfigKey.NavAppType);

            string rpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dat");
            ecomAppRestoreFileName = Path.Combine(rpath, $"restore-{(string.IsNullOrEmpty(configuration.AppId) ? ecomAppId : configuration.AppId)}.txt");
            if (Directory.Exists(rpath) == false)
            {
                Directory.CreateDirectory(rpath);
            }
            if (File.Exists(ecomAppRestoreFileName) == false)
            {
                File.WriteAllText(ecomAppRestoreFileName, true.ToString());
            }
            string restoredata = File.ReadAllText(ecomAppRestoreFileName);
            ecomAppRestore = Convert.ToBoolean(restoredata);

            string url = config.SettingsGetByKey(ConfigKey.BOUrl);
            int sp = url.ToLower().IndexOf("/ws/");
            int ep = url.ToLower().IndexOf("/codeunit");
            NavCompany = HttpUtility.UrlDecode(url.Substring(sp + 4, ep - sp - 4));

            //navWsVersion is Unknown for the first time 
            if (LSCVersion != null)
                return;

            //check if domain is part of the user name
            NetworkCredential credentials = null;
            string username = config.SettingsGetByKey(ConfigKey.BOUser);
            string password = config.SettingsGetByKey(ConfigKey.BOPassword);
            string protocol = config.SettingsGetByKey(ConfigKey.BOProtocol);

            //check if the password has been encrypted by our LSOmniPasswordGenerator.exe
            if (DecryptConfigValue.IsEncryptedPwd(password))
            {
                password = DecryptConfigValue.DecryptString(password, config.SettingsGetByKey(ConfigKey.EncrCode));
            }

            if (protocol.ToUpper().Equals("S2S") == false)
            {
                //check if domain is part of the config.UserName
                string domain = String.Empty;
                if (username.Contains("/") || username.Contains(@"\"))
                {
                    username = username.Replace(@"/", @"\");
                    string[] splitter = username.Split('\\');
                    domain = splitter[0];
                    username = splitter[1];
                }

                if (string.IsNullOrWhiteSpace(username) == false && string.IsNullOrWhiteSpace(password) == false)
                {
                    credentials = new NetworkCredential(username, password, domain);
                }

                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
                if (string.IsNullOrWhiteSpace(protocol) == false)
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)EnumHelper.StringToEnum(typeof(SecurityProtocolType), protocol);
                }
            }

            //TimeoutInSeconds from client can overwrite BOConnection.NavSQL.Timeout
            string timeout = config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOTimeout.ToString()).Value;

            navWebReference = new NavWebReference.RetailWebServices()
            {
                Url = url,
                Timeout = (timeout == null ? 20 : ConvertTo.SafeInt(timeout)) * 1000,  //millisecs,  60 seconds
                PreAuthenticate = true,
                AllowAutoRedirect = true
            };

            string qryurl = config.SettingsGetByKey(ConfigKey.BOQryUrl);
            navWebQryReference = new NavWebReference.RetailWebServices()
            {
                Url = string.IsNullOrEmpty(qryurl) ? navWebReference.Url : qryurl,
                Timeout = (timeout == null ? 20 : ConvertTo.SafeInt(timeout)) * 1000,  //millisecs,  60 seconds
                PreAuthenticate = true,
                AllowAutoRedirect = true
            };

            centralWS = new LSCentral.OmniWrapper();
            centralWS25 = new LSCentral25.OmniWrapper();
            centralQryWS = new LSCentral.OmniWrapper();
            centralQryWS25 = new LSCentral25.OmniWrapper();
            activityWS = new LSActivity.Activity();
            odataWS = new LSOData.ODataRequest();

            centralWS.Url = url.Replace("RetailWebServices", "OmniWrapper");
            centralWS.Timeout = config.SettingsIntGetByKey(ConfigKey.BOTimeout) * 1000;  //millisecs,  60 seconds
            centralWS.PreAuthenticate = true;
            centralWS.AllowAutoRedirect = true;

            centralWS25.Url = url.Replace("RetailWebServices", "OmniWrapper");
            centralWS25.Timeout = config.SettingsIntGetByKey(ConfigKey.BOTimeout) * 1000;  //millisecs,  60 seconds
            centralWS25.PreAuthenticate = true;
            centralWS25.AllowAutoRedirect = true;

            centralQryWS.Url = string.IsNullOrEmpty(qryurl) ? centralWS.Url : qryurl.Replace("RetailWebServices", "OmniWrapper");
            centralQryWS.Timeout = config.SettingsIntGetByKey(ConfigKey.BOTimeout) * 1000;  //millisecs,  60 seconds
            centralQryWS.PreAuthenticate = true;
            centralQryWS.AllowAutoRedirect = true;

            centralQryWS25.Url = string.IsNullOrEmpty(qryurl) ? centralWS.Url : qryurl.Replace("RetailWebServices", "OmniWrapper");
            centralQryWS25.Timeout = config.SettingsIntGetByKey(ConfigKey.BOTimeout) * 1000;  //millisecs,  60 seconds
            centralQryWS25.PreAuthenticate = true;
            centralQryWS25.AllowAutoRedirect = true;

            activityWS.Url = url.Replace("RetailWebServices", "Activity");
            activityWS.Timeout = config.SettingsIntGetByKey(ConfigKey.BOTimeout) * 1000;  //millisecs,  60 seconds
            activityWS.PreAuthenticate = true;
            activityWS.AllowAutoRedirect = true;

            odataWS.Url = string.IsNullOrEmpty(qryurl) ? url.Replace("RetailWebServices", "ODataRequest") : qryurl.Replace("RetailWebServices", "ODataRequest");
            odataWS.Timeout = config.SettingsIntGetByKey(ConfigKey.BOTimeout) * 1000;  //millisecs,  60 seconds
            odataWS.PreAuthenticate = true;
            odataWS.AllowAutoRedirect = true;

            string token = config.SettingsGetByKey(ConfigKey.Central_Token);
            if (protocol.ToUpper().Equals("S2S") && string.IsNullOrEmpty(token) == false)
            {
                navWebReference.GetType().GetProperty("AuthToken").SetValue(navWebReference, token);
                navWebQryReference.GetType().GetProperty("AuthToken").SetValue(navWebQryReference, token);

                centralWS.GetType().GetProperty("AuthToken").SetValue(centralWS, token);
                centralWS25.GetType().GetProperty("AuthToken").SetValue(centralWS25, token);
                centralQryWS.GetType().GetProperty("AuthToken").SetValue(centralQryWS, token);
                centralQryWS25.GetType().GetProperty("AuthToken").SetValue(centralQryWS25, token);
                activityWS.GetType().GetProperty("AuthToken").SetValue(activityWS, token);
                odataWS.GetType().GetProperty("AuthToken").SetValue(odataWS, token);
            }
            else
            {
                //don't set the credentials unless we have them. Can use the app pool too.
                if (credentials != null)
                {
                    navWebReference.Credentials = credentials;
                    navWebQryReference.Credentials = credentials;

                    centralWS.Credentials = credentials;
                    centralWS25.Credentials = credentials;
                    centralQryWS.Credentials = credentials;
                    centralQryWS25.Credentials = credentials;
                    activityWS.Credentials = credentials;
                    odataWS.Credentials = credentials;
                }
            }

            if (string.IsNullOrEmpty(config.SettingsGetByKey(ConfigKey.Proxy_Server)) == false)
            {
                navWebReference.Proxy = GetWebProxy();
                navWebQryReference.Proxy = GetWebProxy();

                centralWS.Proxy = GetWebProxy();
                centralWS25.Proxy = GetWebProxy();
                centralQryWS.Proxy = GetWebProxy();
                centralQryWS25.Proxy = GetWebProxy();
                activityWS.Proxy = GetWebProxy();
                odataWS.Proxy = GetWebProxy();
            }
            NavVersionToUse(out string cVer);
        }

        public string SendToOData(string command, string data, bool sendget)
        {
            try
            {
                Uri url = new Uri(string.Format("{0}/{1}?company={2}", config.SettingsGetByKey(ConfigKey.BOODataUrl), command, NavCompany));
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = (sendget) ? "GET" : "POST";
                httpWebRequest.PreAuthenticate = true;
                httpWebRequest.AllowAutoRedirect = true;

                string token = config.SettingsGetByKey(ConfigKey.Central_Token);
                string protocol = config.SettingsGetByKey(ConfigKey.BOProtocol);
                if (protocol.ToUpper().Equals("S2S") && string.IsNullOrEmpty(token) == false)
                {
                    httpWebRequest.Headers.Add("Authorization", token);
                }
                else
                {
                    //check if domain is part of the user name
                    string username = config.SettingsGetByKey(ConfigKey.BOUser);
                    string password = config.SettingsGetByKey(ConfigKey.BOPassword);

                    //check if the password has been encrypted by our LSOmniPasswordGenerator.exe
                    if (DecryptConfigValue.IsEncryptedPwd(password))
                    {
                        password = DecryptConfigValue.DecryptString(password, config.SettingsGetByKey(ConfigKey.EncrCode));
                    }

                    //check if domain is part of the config.UserName
                    string domain = String.Empty;
                    if (username.Contains("/") || username.Contains(@"\"))
                    {
                        username = username.Replace(@"/", @"\");
                        string[] splitter = username.Split('\\');
                        domain = splitter[0];
                        username = splitter[1];
                    }

                    if (string.IsNullOrWhiteSpace(username) == false && string.IsNullOrWhiteSpace(password) == false)
                    {
                        httpWebRequest.Credentials = new NetworkCredential(username, password, domain);
                    }
                }

                logger.Debug(config.LSKey.Key, "OData Req Sent to:{0} ReqData:{1}", url.LocalPath, data);
                using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                string ret = string.Empty;
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    ret = streamReader.ReadToEnd();
                }

                logger.Debug(config.LSKey.Key, "OData Result:[{0}]", ret);
                return ret;
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex);
                return "ERROR:" + ex.Message;
            }
        }

        public bool ResetReplication(bool fullreplication, string lastkey)
        {
            if (fullreplication && (string.IsNullOrEmpty(lastkey) || lastkey == "0"))
            {
                File.WriteAllText(ecomAppRestoreFileName, true.ToString());
                ecomAppRestore = true;
                return true;
            }
            return false;
        }

        public XMLTableData DoReplication(int tableid, string storeId, string appid, string apptype, int batchSize, ref string lastKey, out int totalrecs)
        {
            if (string.IsNullOrEmpty(appid))
            {
                appid = ecomAppId;
            }
            if (string.IsNullOrEmpty(apptype))
            {
                apptype = ecomAppType;
            }

            int restorepoint = Convert.ToInt32(string.IsNullOrEmpty(lastKey) ? "0" : lastKey);
            int resPoint;
            NAVWebXml xml = new NAVWebXml(storeId, appid, apptype);
            List<XMLTableData> tablist = new List<XMLTableData>();

            if (restorepoint == 0 && ecomAppRestore)
            {
                // restart to beginning
                restorepoint = 1;
                RestoreWebReplication(xml, restorepoint);
                tablist = StartWebReplication(xml, batchSize, ref restorepoint);

                File.WriteAllText(ecomAppRestoreFileName, false.ToString());
            }
            else
            {
                NAVSyncCycleStatus status = GetTableStatus(xml, tableid, out resPoint);
                switch (status)
                {
                    case NAVSyncCycleStatus.InProgress:
                        tablist = new List<XMLTableData>();
                        XMLTableData t = new XMLTableData();
                        t.TableId = tableid;
                        t.SyncCycleStatus = NAVSyncCycleStatus.InProgress;
                        tablist.Add(t);
                        break;
                    case NAVSyncCycleStatus.New:
                    case NAVSyncCycleStatus.Finished:
                        tablist = StartWebReplication(xml, batchSize, ref restorepoint);
                        break;
                }
            }

            XMLTableData data = GetTableData(xml, tablist, tableid, out bool endoftable, out totalrecs, out resPoint);
            if (endoftable)
                totalrecs = 0;
            if (resPoint > 0)
                restorepoint = resPoint;

            lastKey = restorepoint.ToString();
            return data;
        }


        public string NavVersionToUse(out string centralVersion)
        {
            if (LSCVersion == null)
                LSCVersion = new Version("25.0");

            centralVersion = LSCVersion.ToString();
            if (centralWS == null)
                return "ERROR: not connected";

            //this methods is called in PING and in constructor
            try
            {
                //To overwrite what comes from NAV (or if Nav doesn't implement TEST_CONNECTION
                //in AppSettings add this key.
                //  <add key="LSNAV.Version" value="8.0"/>    or "7.0"  "7.1"
                //can overwrite what comes from NAV by adding key LSNAV.Version to the appConfig FILE not table.. LSNAV_Version
                string version = config.SettingsGetByKey(ConfigKey.LSNAV_Version);
                if (string.IsNullOrEmpty(version) == false)
                {
                    LSCVersion = new Version(version);
                    logger.Debug(config.LSKey.Key, "LSNAV.Version Value {0} from TenantConfig is being used", version);
                    return string.Format("{0} [conf]", version);
                }

                string navver = string.Empty;
                string appVersion = string.Empty;
                string appBuild = string.Empty;
                string retailCopyright = string.Empty;
                string retailVersion = string.Empty;
                string respCode = string.Empty;
                string errorText = string.Empty;

                if (centralWS.Url.Equals(centralQryWS.Url) == false)
                {
                    // ping 2nd server first
                    centralQryWS.TestConnection(ref respCode, ref errorText, ref appVersion, ref appBuild, ref retailVersion, ref retailCopyright);
                    logger.Debug(config.LSKey.Key, "Nav WS2 Query Response > Ver:{0} ErrCode:{1} ErrText:{2}",
                        retailVersion, respCode, errorText);
                    if (respCode != "0000")
                        throw new LSOmniServiceException(StatusCode.NavWSError, respCode, errorText);
                }

                centralWS.TestConnection(ref respCode, ref errorText, ref appVersion, ref appBuild, ref retailVersion, ref retailCopyright);
                logger.Debug(config.LSKey.Key, "Nav WS2 Main Response > appVersion:{0} appBuild:{1} retailVersion:{2} retailCopyright:{3} ErrCode:{4} ErrText:{5}",
                    appVersion, appBuild, retailVersion, retailCopyright, respCode, errorText);
                if (respCode != "0000")
                    throw new LSOmniServiceException(StatusCode.NavWSError, respCode, errorText);

                int st = retailVersion.IndexOf('(');
                int ed = retailVersion.IndexOf(')');
                if (st == -1)
                {
                    if (retailVersion.Contains("LS Central"))
                    {
                        string vv1 = retailVersion.Substring(11, retailVersion.Length - 11);
                        LSCVersion = new Version(vv1);
                        navver = string.Format("{0} [{1}]", vv1, appBuild);
                    }
                    else
                    {
                        navver = string.Format("{0} [{1}]", LSCVersion, appBuild);
                    }
                }
                else
                {
                    string vv1 = retailVersion.Substring(st + 1, ed - st - 1);
                    LSCVersion = new Version(vv1);
                    navver = string.Format("{0} [{1}]", vv1, appBuild);
                }

                string asm = ConfigSetting.GetString("BOConnection.AssemblyName").ToLower();
                if (LSCVersion >= new Version("21.3"))
                {
                    if (config.SettingsKeyExists(ConfigKey.BOODataUrl) == false)
                        throw new LSOmniServiceException(StatusCode.NavODataError, "9001", "BOConnection.Nav.ODataUrl is missing from AppSettings file");

                    string ourl = config.SettingsGetByKey(ConfigKey.BOODataUrl);
                    if (string.IsNullOrEmpty(ourl))
                        throw new LSOmniServiceException(StatusCode.NavODataError, "9002", "BOConnection.Nav.ODataUrl is empty in AppSettings file");

                    if (LSCVersion >= new Version("25.0"))
                    {
                        Statistics stat = new Statistics();
                        string ret = SendToOData("TestConnectionOData_TestConnection", "{ }", false);
                        logger.Debug(config.LSKey.Key, "Central OData TestConnection Response > " + ret);
                        SetupJMapping map = new SetupJMapping(config.IsJson);
                        map.GetTestConnection(ret, out appVersion, out retailVersion, out appBuild, out retailCopyright, out bool licActive, out bool ecomLic, out errorText, out respCode);
                        HandleWS2ResponseCode("TestConnectionOData", respCode, errorText, ref stat, 0);
                        navver += $" CL:{licActive} EL:{ecomLic}";
                    }
                    else
                    {
                        string ret = SendToOData("GetMemberContactInfo_GetRequestDef", "{ }", false);
                        logger.Debug(config.LSKey.Key, "Central OData Response > " + ret);
                        if (ret.StartsWith("ERROR:"))
                            throw new LSOmniServiceException(StatusCode.NavODataError, "9003", ret);
                    }
                }

                logger.Debug(config.LSKey.Key, "appVer:{0} appBuild:{1} retailVer:{2} retailCopyright:{3} NavVersionToUse:{4}",
                    appVersion, appBuild, retailVersion, retailCopyright, (LSCVersion == null) ? "None" : LSCVersion.ToString());

                centralVersion = LSCVersion.ToString();
                return navver;
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex, "Failed to determine NavVersion");
                return "ERROR " + ex.Message + ((ex.InnerException == null) ? string.Empty : " >> " + ex.InnerException.Message);
            }
        }

        public List<XMLTableData> StartWebReplication(NAVWebXml xml, int batchSize, ref int restorePoint)
        {
            string xmlRequest;
            string xmlResponse;

            // get tables to replicate and current status
            xmlRequest = xml.StartSyncRequestXML(batchSize);
            xmlResponse = RunOperation(xmlRequest, true);
            string ret = HandleResponseCode(ref xmlResponse, new string[] { "1921" }, false);
            if (string.IsNullOrEmpty(ret) == false)
            {
                // App is not registered, so lets register it
                xmlRequest = xml.RegisterApplicationRequestXML(LSCVersion);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse, new string[] { "1920" }, false); // seems like its already registered

                // Now try again to start Sync Cycle
                xmlRequest = xml.StartSyncRequestXML(batchSize);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
            }
            return xml.SyncResponseXML(xmlResponse, out restorePoint);
        }

        #region private members

        private void Base64StringConvertion(ref string xmlRequest)
        {
            string base64String = Convert.ToBase64String(new UTF8Encoding().GetBytes(xmlRequest));
            //Don't want to load hundreds of KB in xdoc just to get the requestId
            //XDocument doc = XDocument.Parse(xmlRequest); //to get the requstId
            //string reqId = doc.Element("Request").Element("Request_ID").Value;
            int first = xmlRequest.IndexOf("Request_ID>") + "Request_ID>".Length;
            int last = xmlRequest.LastIndexOf("</Request_ID");
            string reqId = xmlRequest.Substring(first, last - first);

            XDocument doc64 = new XDocument(new XDeclaration("1.0", "utf-8", "no"));
            XElement root =
                            new XElement("Request", new XAttribute("Encoded", "Base64"),
                                new XElement("Request_ID", reqId),
                                new XElement("Encoded_Request", base64String)
                            );
            ;
            doc64.Add(root);
            xmlRequest = doc64.ToString();
            /*
            <Request Encoded="Base64">
                <Request_ID>TEST_CONNECTION</Request_ID>
                <Encoded_Request>xxxx yy
                </Encoded_Request>
            </Request>
            */

        }

        //run the nav web service operation
        protected string RunOperation(string xmlRequest, bool useQuery = false, bool logResponse = true)
        {
            bool doBase64 = false;
            //only larger requests should be converted to base64
            if (xmlRequest.Length >= base64ConversionMinLength && (xmlRequest.Contains("WEB_POS") || xmlRequest.Contains("IM_SEND_DOCUMENT") || xmlRequest.Contains("IM_SEND_INVENTORY_TRANSACTION")))
            {
                //add key Nav.SkipBase64Conversion  true to skip this basel64 trick
                if (config.SettingsBoolGetByKey(ConfigKey.SkipBase64Conversion) == false)
                {
                    doBase64 = true;
                    Base64StringConvertion(ref xmlRequest);
                    logger.Debug(config.LSKey.Key, "Base64 string sent to Nav as <Encoded_Request>");
                }
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //WebService Request
            string xmlResponse = string.Empty;
            ExecuteWebRequest(ref xmlRequest, ref xmlResponse, useQuery);

            stopWatch.Stop();
            if (string.IsNullOrWhiteSpace(xmlResponse))
            {
                logger.Error(config.LSKey.Key, "xmlResponse from NAV is empty");
                logger.Debug(config.LSKey.Key, "xmlRequest: " + xmlRequest);
                throw new LSOmniServiceException(StatusCode.NavWSError, "xmlResponse from NAV is empty");
            }

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("NAV WS call. ElapsedTime (mi:sec.msec): {0:00}:{1:00}.{2:000}",
                ts.Minutes, ts.Seconds, ts.Milliseconds);

            if (logger.IsDebugEnabled)
            {
                string reqId = GetRequestID(ref xmlRequest);
                string resId = GetRequestID(ref xmlResponse);
                if (reqId != resId)
                {
                    logger.Debug(config.LSKey.Key, "WARNING (DEBUG ONLY) Request and Response not the same from NAV: requestId:{0}  ResponesId:{1}", reqId, resId);
                }
                if (doBase64)
                {
                    string responseCode = GetResponseCode(ref xmlResponse);
                    // 0020 = Request Node Request_Body not found in request
                    if (responseCode == "0020")
                    {
                        logger.Debug(config.LSKey.Key, "WARNING Base64 string was passed to Nav but Nav failed. Has Codeunit 99009510 been updated to support base64 and Encoded_Request? requestId:{0}  ResponesId:{1}",
                            reqId, resId);
                    }
                }
            }

            LogXml(xmlRequest, xmlResponse, elapsedTime, logResponse);
            return xmlResponse;
        }

        private List<XMLTableData> RestoreWebReplication(NAVWebXml xml, int restorePoint)
        {
            string xmlRequest;
            string xmlResponse;

            xmlRequest = xml.RestoreSyncRequestXML(restorePoint);
            xmlResponse = RunOperation(xmlRequest, true);
            string ret = HandleResponseCode(ref xmlResponse, new string[] { "1921", "1923" }, false);
            if (string.IsNullOrEmpty(ret) == false)
                return new List<XMLTableData>();

            return xml.SyncResponseXML(xmlResponse, out _);
        }

        private XMLTableData GetTableData(NAVWebXml xml, List<XMLTableData> tablist, int tableidtoget, out bool endoftable, out int totalrecs, out int restorePoint)
        {
            string xmlRequest;
            string xmlResponse;
            endoftable = true;
            totalrecs = 0;
            restorePoint = 0;

            foreach (XMLTableData table in tablist)
            {
                if (table.TableId != tableidtoget || table.SyncCycleStatus == NAVSyncCycleStatus.Finished)
                    continue;

                try
                {
                    xmlRequest = xml.GetTableDataRequestXML(table.TableId);
                    xmlResponse = RunOperation(xmlRequest, true);
                    HandleResponseCode(ref xmlResponse);
                    XMLTableData ret = xml.GetTableDataResponseXML(xmlResponse, table, out endoftable, out restorePoint);
                    totalrecs = ret.NumberOfValues;
                    return ret;
                }
                catch (Exception ex)
                {
                    logger.Error(config.LSKey.Key, ex, "Fetching Table data for table" + table.TableName);
                }
            }
            return null;
        }

        private NAVSyncCycleStatus GetTableStatus(NAVWebXml xml, int tableNo, out int restorePoint)
        {
            string xmlRequest;
            string xmlResponse;

            xmlRequest = xml.GetSyncStatusRequestXML();
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            return xml.GetSyncStatusResponseXML(xmlResponse, tableNo, out restorePoint);
        }

        private void LogXml(string xmlRequest, string xmlResponse, string elapsedTime, bool logResponse)
        {
            string reqId = GetRequestID(ref xmlRequest);
            //too much data even for debug mode. Only write some requestId if trace is enabled
            if (!logger.IsTraceEnabled && (reqId == "WEB_GET_DINING_TABLE_LIST" || reqId == "GET_DYNAMIC_CONTENT"))
            {
                return;
            }
            //only log XML in debug mode, since passwords are logged
            else if (logger.IsDebugEnabled)
            {
                xmlRequest = Serialization.RemoveNode("Password", xmlRequest);
                xmlRequest = Serialization.RemoveNode("NewPassword", xmlRequest);
                xmlRequest = Serialization.RemoveNode("OldPassword", xmlRequest);
                XDocument doc = XDocument.Parse(xmlResponse);
                xmlResponse = doc.ToString();//get better XML parsing
                XDocument docRq = XDocument.Parse(xmlRequest);
                xmlRequest = docRq.ToString();//get better XML parsing
                logger.Debug(config.LSKey.Key, "\r\nNLOG DEBUG MODE ONLY:  {0}\r\n{1}\r\n  \r\n{2}\r\n", elapsedTime, xmlRequest, (logResponse) ? xmlResponse : string.Empty);
            }
        }

        //first time executing this I check if this is nav 7 or 6 web service
        protected void ExecuteWebRequest(ref string xmlRequest, ref string xmlResponse, bool useQuery)
        {
            NavWebReference.RetailWebServices wsToUse = (useQuery) ? navWebQryReference : navWebReference;

            try
            {
                if (TimeoutSec > 0)
                    wsToUse.Timeout = (TimeoutSec - 2) * 1000;//-2 to make sure server timeout before client

                wsToUse.WebRequest(ref xmlRequest, ref xmlResponse);
            }
            catch (Exception ex)
            {
                //note pxmlResponce  vs  pxmlResponse
                if (ex.Message.Contains("pxmlResponse in method WebRequest in service"))
                {
                    // Are you connecting to NAV 2013 instead 2009?   7 vs 6
                    lock (Locker)
                    {
                        wsToUse.WebRequest(ref xmlRequest, ref xmlResponse);
                    }
                }
                else
                    throw ex;
            }
        }

        private bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        protected string GetResponseCode(ref string xmlResponse)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResponse);
            string navResponseCode = ParseResponseCode(doc.GetElementsByTagName("Response_Code"));
            return navResponseCode;
        }

        protected StatusCode GetStatusCode(ref string xmlResponse)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResponse);
            string navResponseCode = ParseResponseCode(doc.GetElementsByTagName("Response_Code"));
            string navResponseId = ParseResponseCode(doc.GetElementsByTagName("Request_ID"));
            return MapResponseToStatusCode(navResponseId, navResponseCode);
        }

        private string GetRequestID(ref string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return ParseResponseCode(doc.GetElementsByTagName("Request_ID"));
        }

        protected string ParseResponseCode(XmlNodeList responseCode)
        {
            XmlNode node = responseCode.Item(0);
            return node.InnerText;
        }

        protected string ParseResponseText(XmlNodeList responseText)
        {
            XmlNode node = responseText.Item(0);
            return node.InnerText;
        }

        protected string HandleResponseCode(ref string xmlResponse, string[] codesToHandle = null, bool useMsgText = false)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResponse);
            string responseCode = ParseResponseCode(doc.GetElementsByTagName("Response_Code"));
            if (responseCode != "0000")
            {
                string navResponseId = ParseResponseCode(doc.GetElementsByTagName("Request_ID"));
                string navResponseText = ParseResponseText(doc.GetElementsByTagName("Response_Text"));

                StatusCode statusCode = MapResponseToStatusCode(navResponseId, responseCode);
                string msg = string.Format("navResponseCode: {0}-{1}  [StatusCode: {2}]", responseCode, navResponseText, statusCode.ToString());
                logger.Error(config.LSKey.Key, msg);

                if (codesToHandle != null && codesToHandle.Length > 0)
                {
                    foreach (string code in codesToHandle)
                    {
                        if (useMsgText)
                        {
                            if (navResponseText.Contains(code))
                                return responseCode;

                            continue;
                        }

                        //expected return codes, so don't throw unexpected exception, rather return the known codes to client  
                        if (code.Equals(responseCode))
                            return code;
                    }
                }
                throw new LSOmniServiceException(statusCode, msg);
            }
            return string.Empty;
        }

        protected string HandleWS2ResponseCode(string funcName, string respCode, string errText, ref Statistics stat, int statIndex, string[] codesToHandle = null)
        {
            if (respCode == "0000" || respCode == "")
                return string.Empty;

            StatusCode statusCode = MapResponseToStatusCode(funcName, respCode);
            logger.Error(config.LSKey.Key, "LS Central [{0}] Error: [{1}]:{2} [OmniCode:{3}]", funcName, respCode, errText, statusCode);

            if (codesToHandle != null && codesToHandle.Length > 0)
            {
                foreach (string code in codesToHandle)
                {
                    //expected return codes, so don't throw unexpected exception, rather return the known codes to client  
                    if (code.Equals(respCode))
                    {
                        logger.Warn(config.LSKey.Key, "LS Central [{0}] Warning: [{1}]:{2} [OmniCode:{3}]", funcName, respCode, errText, statusCode);
                        return code;
                    }
                }
            }
            logger.StatisticEndSub(ref stat, statIndex);
            throw new LSOmniServiceException(statusCode, respCode, errText);
        }

        protected StatusCode MapResponseToStatusCode(string navResponseId, string navCode)
        {
            //mapping response code from NAV to LSOmni, but sometimes the same navCode is used for different navResponseId
            // so need to check them both - sometimes
            navResponseId = navResponseId.ToUpper().Trim();
            StatusCode statusCode = StatusCode.Error; //default to Error
            switch (navCode)
            {
                case "0000":
                    statusCode = StatusCode.OK;
                    break;
                case "0004":
                    statusCode = StatusCode.NAVWebFunctionNotFound;
                    break;
                case "0030":
                    statusCode = StatusCode.InvalidNode;
                    break;
                case "0099":
                    statusCode = StatusCode.GeneralErrorCode;
                    break;
                case "0131":
                    statusCode = StatusCode.PasswordInvalid;
                    break;
                case "0403":
                    statusCode = StatusCode.ServerRefusingToRespond;
                    break;
                case "1001":
                    statusCode = StatusCode.Error;
                    if (navResponseId == "GET_DYN_CONT_HMP_MENUS" || navResponseId == "GET_DYNAMIC_CONTENT")
                        statusCode = StatusCode.NoHMPMenuFound;
                    if (navResponseId == "IM_GET_CUSTOMER_CARD_01" || navResponseId == "GETCUSTOMERCARD")
                        statusCode = StatusCode.CustomerNotFound;
                    if (navResponseId == "IM_GET_ITEM_CARD_01" || navResponseId == "GETITEMCARD")
                        statusCode = StatusCode.ItemNotFound;
                    if (navResponseId == "IM_GET_VENDOR_CARD_01" || navResponseId == "GETVENDORCARD")
                        statusCode = StatusCode.VendorNotFound;
                    break;
                case "1002":
                    if (navResponseId == "MM_CREATE_LOGIN_LINKS")
                        statusCode = StatusCode.UserNameNotFound;
                    break;
                case "1003":
                    if (navResponseId == "MM_CREATE_LOGIN_LINKS")
                        statusCode = StatusCode.MemberCardNotFound;
                    break;
                case "1013":
                    if (navResponseId == "GET_DYN_CONT_HMP_MENUS" || navResponseId == "GET_DYNAMIC_CONTENT")
                        statusCode = StatusCode.HMPMenuNotEnabled;
                    break;
                case "1014":
                    if (navResponseId == "GET_DYN_CONT_HMP_MENUS" || navResponseId == "GET_DYNAMIC_CONTENT")
                        statusCode = StatusCode.HMPMenuNoDynamicContentFoundToday;
                    break;
                case "1100":
                    statusCode = StatusCode.UserNameExists;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.AccountNotFound;
                    break;
                case "1101":
                    statusCode = StatusCode.UserNameNotFound;
                    break;
                case "1102":
                    statusCode = StatusCode.ContactIdNotFound;
                    break;
                case "1103":
                    statusCode = StatusCode.ContactIdNotFound;
                    break;
                case "1106":
                    statusCode = StatusCode.QtyMustBePositive;
                    break;
                case "1107":
                    statusCode = StatusCode.LineNoMission;
                    break;
                case "1110":
                    statusCode = StatusCode.UserNameInvalid;
                    break;
                case "1120":
                    statusCode = StatusCode.PasswordInvalid;
                    if (navResponseId == "MM_MOBILE_LOGON")
                        statusCode = StatusCode.AuthFailed;
                    break;
                case "1130":
                    statusCode = StatusCode.EmailInvalid;
                    if (navResponseId == "MM_MOBILE_LOGON")
                        statusCode = StatusCode.CardIdInvalid;
                    break;
                case "1135":
                    statusCode = StatusCode.EmailExists;
                    break;
                case "1140":
                    statusCode = StatusCode.MissingLastName;
                    if (navResponseId == "MM_MOBILE_LOGON")
                        statusCode = StatusCode.LoginIdNotMemberOfClub;
                    break;
                case "1150":
                    statusCode = StatusCode.MissingFirstName;
                    if (navResponseId == "MM_MOBILE_LOGON")
                        statusCode = StatusCode.DeviceIdNotFound;
                    break;
                case "1160":
                    statusCode = StatusCode.AccountNotFound;
                    break;
                case "1170":
                    statusCode = StatusCode.OneAccountInvalid;
                    break;
                case "1180":
                    statusCode = StatusCode.PrivateAccountInvalid;
                    break;
                case "1190":
                    statusCode = StatusCode.ClubInvalid;
                    break;
                case "1200":
                    statusCode = StatusCode.SchemeInvalid;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.ContactIdNotFound;
                    break;
                case "1210":
                    statusCode = StatusCode.ClubOrSchemeInvalid;
                    break;
                case "1212":
                    statusCode = StatusCode.SchemeClubInvalid;
                    break;
                case "1220":
                    statusCode = StatusCode.AccountContactIdInvalid;
                    break;
                case "1230":
                    statusCode = StatusCode.AccountExistsInOtherClub;
                    break;

                case "1300":
                    statusCode = StatusCode.PasswordOldInvalid;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.ContactIdNotFound;
                    break;
                case "1310":
                    statusCode = StatusCode.PasswordInvalid; //invalid new password
                    break;
                case "1400":
                    statusCode = StatusCode.MissingItemNumer;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.AccountNotFound;
                    break;
                case "1410":
                    statusCode = StatusCode.MissingStoreNumber;
                    break;
                case "1450":
                    statusCode = StatusCode.DeviceIdMissing;
                    break;
                case "1500":
                    statusCode = StatusCode.ContactIdNotFound;
                    break;
                case "1600":
                    statusCode = StatusCode.PosNotExists;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.MemberCardNotFound;
                    if (navResponseId == "IM_SEND_INVENTORY_TRANSACTION" || navResponseId == "STOREINVTRANSACTIONSEND")
                        statusCode = StatusCode.WorksheetNotFound;
                    break;
                case "1601":
                    statusCode = StatusCode.StoreNotExists;
                    break;
                case "1602":
                    statusCode = StatusCode.StaffNotExists;
                    break;
                case "1603":
                    statusCode = StatusCode.ItemNotExists;
                    break;
                case "1604":
                    statusCode = StatusCode.VATSetupMissing;
                    break;
                case "1605":
                    statusCode = StatusCode.InvalidUom;
                    break;
                case "1606":
                    statusCode = StatusCode.ItemBlocked;
                    break;
                case "1607":
                    statusCode = StatusCode.InvalidVariant;
                    break;
                case "1608":
                    statusCode = StatusCode.InvalidPriceChange;
                    break;
                case "1609":
                    statusCode = StatusCode.PriceChangeNotAllowed;
                    break;
                case "1610":
                    statusCode = StatusCode.PriceTooHigh;
                    if (navResponseId == "IM_SEND_INVENTORY_TRANSACTION" || navResponseId == "STOREINVTRANSACTIONSEND")
                        statusCode = StatusCode.NotReadyForNextCount;
                    break;
                case "1611":
                    statusCode = StatusCode.InvalidDiscPercent;
                    break;
                case "1612":
                    statusCode = StatusCode.IncExpNotFound;
                    break;
                case "1613":
                    statusCode = StatusCode.TenderTypeNotFound;
                    break;
                case "1614":
                    statusCode = StatusCode.InvalidTOTDiscount;
                    break;
                case "1615":
                    statusCode = StatusCode.NotMobilePos;
                    break;
                case "1619":
                    statusCode = StatusCode.InvalidPostingBalance;
                    break;
                case "1620":
                    statusCode = StatusCode.SuspendWithPayment;
                    if (navResponseId == "WEB_POS")
                        statusCode = StatusCode.PaymentPointsMissing; //Payment with member points, %1 missing.
                    if (navResponseId == "IM_SEND_INVENTORY_TRANSACTION" || navResponseId == "STOREINVTRANSACTIONSEND")
                        statusCode = StatusCode.LinesNotFound;
                    if (navResponseId == "STOREINVENTORYLINESGET")
                        statusCode = StatusCode.LinesNotFound;
                    break;
                case "1621":
                    statusCode = StatusCode.UnknownSuspError;
                    if (navResponseId == "WEB_POS")
                        statusCode = StatusCode.MemberCardNotFound; //Unable to load member information – ErrorText from LOAD_MEMBER _INFO displayed
                    break;
                case "1625":
                    statusCode = StatusCode.SuspKeyNotFound;
                    break;
                case "1626":
                    statusCode = StatusCode.TransServError;
                    break;
                case "1627":
                    statusCode = StatusCode.SuspTransNotFound;
                    break;
                case "1670":
                    statusCode = StatusCode.CustomerNotFound;
                    break;
                case "1673":
                    statusCode = StatusCode.NoEntriesFound;
                    break;
                case "1674":
                    statusCode = StatusCode.MemberAccountNotFound;
                    break;
                case "1675":
                    statusCode = StatusCode.MemberCardNotFound;
                    break;
                case "1676":
                    statusCode = StatusCode.NoEntriesFound;
                    break;
                case "1677":
                    statusCode = StatusCode.NoEntriesFound;
                    break;
                case "1678":
                    statusCode = StatusCode.NoEntriesFound;
                    break;
                case "1698":
                    statusCode = StatusCode.InvalidPrinterId;
                    break;
                case "1700":
                    statusCode = StatusCode.CardInvalidInUse;
                    break;
                case "1800":
                    statusCode = StatusCode.TerminalIdMissing;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.ClubInvalid;
                    break;
                case "1801":
                    statusCode = StatusCode.TransacitionIdMissing;
                    break;
                case "1802":
                    statusCode = StatusCode.EmailMissing;
                    break;
                case "1900":
                    statusCode = StatusCode.CardInvalidStatus;
                    break;
                case "2000":
                    statusCode = StatusCode.InvalidPrintMethod;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.ContactIsBlocked;
                    break;
                case "2201":
                    statusCode = StatusCode.OrderAlreadyExist;
                    break;
                case "2202":
                    statusCode = StatusCode.OrderIdNotFound;
                    break;
                case "2203":
                    statusCode = StatusCode.MemberCardNotFound;
                    break;
                case "2224":
                    statusCode = StatusCode.PaymentError;
                    break;
                case "2230":
                    statusCode = StatusCode.CardIdInvalid;
                    break;
                case "2231":
                    statusCode = StatusCode.MemberCardNotFound;
                    break;
                case "2252":
                    statusCode = StatusCode.MemberPointBalanceToLow;
                    break;
                case "2253":
                    statusCode = StatusCode.CardInvalidStatus;
                    break;
                case "4003":
                    statusCode = StatusCode.DiningTableStatusNotAbleToChange;
                    break;
                case "4004":
                    statusCode = StatusCode.CannotChangeNumberOfCoverOnTableNotSeated;
                    break;
                case "4005":
                    statusCode = StatusCode.CannotChangeNumberOfCoverOnTableNoSetup;
                    break;
                case "4006":
                    statusCode = StatusCode.SeatingNotUsedInHospType;
                    break;
                case "4007":
                    statusCode = StatusCode.StatusOfTableAlredySeated;
                    break;
                case "4008":
                    statusCode = StatusCode.SeatingNotPossible;
                    break;
                case "4009":
                    statusCode = StatusCode.POSTransNotFoundForActiveOrder;
                    break;
                case "4010":
                    statusCode = StatusCode.NoKitchenStatusFound;
                    break;
                case "4011":
                    statusCode = StatusCode.OpenPOSNotALlowed;
                    break;
                case "4020":
                    statusCode = StatusCode.MainStatusNorCorrect;
                    break;
                case "4021":
                    statusCode = StatusCode.TableAlreadyLocked;
                    break;
                case "7750":  //Percassi only
                    statusCode = StatusCode.SuspendFailure;
                    break;

                default:
                    break;
            }
            return statusCode;
        }

        private WebProxy GetWebProxy()
        {
            //<!-- Keep Proxy values blank if not used -->
            //<add key="Proxy.Server" value=""/>
            //<add key="Proxy.Port" value=""/>
            //<add key="Proxy.User" value=""/>
            //<add key="Proxy.Password" value=""/>
            //<add key="Proxy.Domain" value=""/>

            string proxyServer = config.SettingsGetByKey(ConfigKey.Proxy_Server);
            proxyServer = proxyServer.Trim();
            int iProxyPort = 0;
            string proxyPort = config.SettingsGetByKey(ConfigKey.Proxy_Port);
            string proxyUser = config.SettingsGetByKey(ConfigKey.Proxy_User);
            string proxyPassword = config.SettingsGetByKey(ConfigKey.Proxy_Password);
            string proxyDomain = config.SettingsGetByKey(ConfigKey.Proxy_Domain);

            if (string.IsNullOrEmpty(proxyPort) == false)
                iProxyPort = Convert.ToInt32(proxyPort);

            WebProxy oWebProxy = new WebProxy(proxyServer, iProxyPort);
            if (string.IsNullOrEmpty(proxyUser) == false)
            {
                oWebProxy.Credentials = new NetworkCredential(proxyUser, proxyPassword, proxyDomain); //
            }

            return oWebProxy;
        }

        #endregion private members
    }
}
