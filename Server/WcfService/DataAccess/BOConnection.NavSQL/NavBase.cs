using System;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net.Security;
using System.Diagnostics;

using LSOmni.Common.Util;
using LSOmni.DataAccess.BOConnection.NavSQL.Dal;
using LSOmni.DataAccess.BOConnection.NavSQL.NavWS;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.NavSQL
{
    //Navision back office connection
    public class NavBase
    {
        protected int TimeOutInSeconds { get; set; }

        protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private NavWebReference.RetailWebServices navWebReference = null;
        public static OmniWrapper navWS = null;
        private int base64ConversionMinLength = 1024 * 100; //50KB 75KB  minimum length to base64 conversion
        private static readonly object Locker = new object();

        public static Version NAVVersion = null; //use this in code to check Nav version
        public static bool NavDirect = true;

        public NavBase()
        {
            //get the boversion
            if (NAVVersion == null)
            {
                NavVersionToUse(true); //check the nav version
            }

            if (ConfigSetting.KeyExists("Nav.Base64.MinXmlSizeInKB"))
                base64ConversionMinLength = ConfigSetting.GetInt("Nav.Base64.MinXmlSizeInKB") * 1024; //in KB

            //Username is now  DOMAIN\Username (changed in version 2.0)
            string userName = ConfigSetting.GetString("BOConnection.Nav.UserName");
            string pwd = ConfigSetting.GetString("BOConnection.Nav.Password");
            string domain = "";
            NetworkCredential credentials = null;

            //this is the old way to get domain, keeping it here just in case!!
            if (ConfigSetting.KeyExists("BOConnection.Nav.Domain"))
                domain = ConfigSetting.GetString("BOConnection.Nav.Domain");

            //check if domain is part of the username
            if (userName.Contains("/") || userName.Contains(@"\"))
            {
                userName = userName.Replace(@"/", @"\");
                string[] splitter = userName.Split('\\');
                domain = splitter[0];
                userName = splitter[1];
                //logger.Debug("domain:{0} userName:{1}", domain, userName);
            }

            //check if the password has been encrypted by our LSOmniPasswordGenerator.exe
            if (DecryptConfigValue.IsEncryptedPwd(pwd))
            {
                pwd = DecryptConfigValue.DecryptString(pwd);
            }
            else if (ConfigSetting.KeyExists("BOConnection.Nav.EncryptedPassword"))
            {
                //support old way
                if (ConfigSetting.GetBoolean("BOConnection.Nav.EncryptedPassword"))
                    pwd = DecryptConfigValue.DecryptString(pwd);
            }

            if (string.IsNullOrWhiteSpace(userName) == false && string.IsNullOrWhiteSpace(pwd) == false)
            {
                credentials = new NetworkCredential(userName, pwd, domain);
            }

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);

            //navWsVersion is Unknown for the first time 
            if (NAVVersion == null || NAVVersion.Major >= 7)
            {
                navWebReference = new NavWebReference.RetailWebServices();
                navWebReference.Url = ConfigSetting.GetString("BOConnection.Nav.Url");
                //TimeoutInSeconds from client can overwrite BOConnection.NavSQL.Timeout
                navWebReference.Timeout = ConfigSetting.GetInt("BOConnection.Nav.Timeout") * 1000;  //millisecs,  60 seconds

                //dont set the credentials unless we have them. Can use the app pool too.
                if (credentials != null)
                    navWebReference.Credentials = credentials;

                navWebReference.PreAuthenticate = true;
                navWebReference.AllowAutoRedirect = true;
                if (ConfigSetting.KeyExists("Proxy.Server"))
                    navWebReference.Proxy = GetWebProxy();
            }

            // Use NAV Web Service V2
            bool useNavV2 = false;
            if (ConfigSetting.KeyExists("BOConnection.Nav.WS2"))
            {
                useNavV2 = ConfigSetting.GetBoolean("BOConnection.Nav.WS2"); //read from ConfigSetting file
            }

            NavWSRepository repo = new NavWSRepository();
            if ((NAVVersion == null || NAVVersion.Major >= 11) && useNavV2)
            {
                navWS = new OmniWrapper();
                navWS.Url = ConfigSetting.GetString("BOConnection.Nav.Url").Replace("RetailWebServices", "OmniWrapper");
                navWS.Timeout = ConfigSetting.GetInt("BOConnection.Nav.Timeout") * 1000;  //millisecs,  60 seconds

                if (credentials != null)
                    navWS.Credentials = credentials;

                navWS.PreAuthenticate = true;
                navWS.AllowAutoRedirect = true;
            }

            // Omni Service is connected to main NAV Database, means we can do some local db calls instead of using Web Services
            if (ConfigSetting.KeyExists("BOConnection.Nav.Direct"))
            {
                NavDirect = ConfigSetting.GetBoolean("BOConnection.Nav.Direct"); //read from ConfigSetting file
            }
        }

        public static string NavVersionToUse(bool forceCallToNav = true)
        {
            NAVVersion = new Version("11.0");

            //this methods is called in PING and in constructor
            try
            {
                //To overwrite what comes from NAV (or if Nav doesn't implement TEST_CONNECTION
                //in AppSettings add this key.
                //  <add key="LSNAV.Version" value="8.0"/>    or "7.0"  "7.1"

                string navver = string.Empty;
                NavBase navBase = new NavBase();
                if (forceCallToNav)
                {
                    XmlMapping.Loyalty.NavXml navXml = new XmlMapping.Loyalty.NavXml();
                    string xmlRequest = navXml.TestConnectionRequestXML();
                    string xmlResponse = navBase.RunOperation(xmlRequest);
                    logger.Info("Nav Version: " + xmlResponse);
                    string rCode = navBase.GetResponseCode(ref xmlResponse);
                    string ver = "Unknown";

                    //ignore unknown Request_ID 
                    //the 0004   Unknown Request_ID   TEST_CONNECTION
                    if (rCode != "0004")
                    {
                        navBase.HandleResponseCode(ref xmlResponse);
                        ver = navXml.TestConnectionResponseXML(xmlResponse);

                        int st = ver.IndexOf('(');
                        int ed = ver.IndexOf(')');
                        string vv1 = ver.Substring(st + 1, ed - st - 1);

                        st = ver.IndexOf('[');
                        ed = ver.IndexOf(']');
                        string vv2 = ver.Substring(st + 1, ed - st - 1);

                        navver = string.Format("NAV: {0} [{1}]", vv1, vv2);
                        NAVVersion = new Version(vv1);
                    }
                    logger.Info("{0}  NavVersionToUse: {1}", ver, (NAVVersion == null) ? "None" : NAVVersion.ToString());
                }

                //can overwrite what comes from NAV by adding key LSNAV.Version to the appConfig FILE not table.. LSNAV_Version
                if (ConfigSetting.KeyExists("LSNAV.Version"))
                {
                    string val = ConfigSetting.GetString("LSNAV.Version"); //read from ConfigSetting file
                    NAVVersion = new Version(val);

                    logger.Info("Value {0} of key LSNAV.Version from appsettings.config file is being used : {1}", val, NAVVersion);
                }
                return navver;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to determine NavVersion");
                return "ERROR " + ex.Message;
            }
        }

        protected string Ping()
        {
            return NavVersionToUse(true);
        }

        #region private members

        private void Base64StringConvertion(ref string xmlRequest)
        {
            string base64String = Convert.ToBase64String(new UTF8Encoding().GetBytes(xmlRequest));
            //Dont want to load hundreds of KB in xdoc just to get the requestId
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
        protected string RunOperation(string xmlRequest)
        {
            bool doBase64 = false;
            string originalxmlRequest = "";
            //only larger requests should be converted to base64
            if (xmlRequest.Length >= base64ConversionMinLength && (xmlRequest.Contains("WEB_POS") || xmlRequest.Contains("IM_SEND_DOCUMENT") || xmlRequest.Contains("IM_SEND_INVENTORY_TRANSACTION")))
            {
                //add key Nav.SkipBase64Conversion  true to skip this basel64 trick
                if (!(ConfigSetting.KeyExists("Nav.SkipBase64Conversion") && ConfigSetting.GetBoolean("Nav.SkipBase64Conversion")))
                {
                    originalxmlRequest = xmlRequest;//save it for logging
                    doBase64 = true;
                    Base64StringConvertion(ref xmlRequest);
                    logger.Debug("Base64 string sent to Nav as <Encoded_Request>");
                }
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //WebService Request
            string xmlResponse = string.Empty;
            ExecuteWebRequest(ref xmlRequest, ref xmlResponse);

            stopWatch.Stop();
            if (string.IsNullOrWhiteSpace(xmlResponse))
            {
                logger.Error("xmlResponse from NAV is empty");
                logger.Debug("xmlRequest: " + xmlRequest);
                throw new LSOmniServiceException(StatusCode.Error, "xmlResponse from NAV is empty");
            }

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("NAV ws call. ElapsedTime (mi:sec.msec): {0:00}:{1:00}.{2:000}",
                ts.Minutes, ts.Seconds, ts.Milliseconds);

            if (logger.IsDebugEnabled)
            {
                string reqId = GetRequestID(ref xmlRequest);
                string resId = GetRequestID(ref xmlResponse);
                if (reqId != resId)
                {
                    logger.Debug("WARNING (DEBUG ONLY) Request and Response not the same from NAV: requestId:{0}  ResponesId:{1}", reqId, resId);
                }
                if (doBase64)
                {
                    string responseCode = GetResponseCode(ref xmlResponse);
                    // 0020 = Request Node Request_Body not found in request
                    if (responseCode == "0020")
                    {
                        logger.Debug("WARNING Base64 string was passed to Nav but Nav failed. Has Codeunit 99009510 been updated to support base64 and Encoded_Request? requestId:{0}  ResponesId:{1}",
                            reqId, resId);
                    }
                }
            }

            LogXml(xmlRequest, xmlResponse, elapsedTime);
            return xmlResponse;
        }

        private void LogXml(string xmlRequest, string xmlResponse, string elapsedTime)
        {
            string reqId = GetRequestID(ref xmlRequest);
            //too much data even for debug mode. Only write some requestId if trace is enabled
            if (!logger.IsTraceEnabled && (reqId == "WEB_GET_DINING_TABLE_LIST" || reqId == "GET_DYNAMIC_CONTENT"))
            {
                return;
            }
            //only log xml in debug mode, since passwords are logged
            else if (logger.IsDebugEnabled)
            {
                xmlRequest = RemoveNode("Password", xmlRequest);
                xmlRequest = RemoveNode("NewPassword", xmlRequest);
                xmlRequest = RemoveNode("OldPassword", xmlRequest);
                XDocument doc = XDocument.Parse(xmlResponse);
                xmlResponse = doc.ToString();//get better xml parsing
                XDocument docRq = XDocument.Parse(xmlRequest);
                xmlRequest = docRq.ToString();//get better xml parsing
                logger.Debug("\r\nNLOG DEBUG MODE ONLY:  {0}\r\n{1}\r\n  \r\n{2}\r\n", elapsedTime, xmlRequest, xmlResponse);
            }
        }

        protected string RemoveNode(string nodeName, string xml)
        {
            try
            {
                //remove a node from xml,, nodeName=Password removes value between <Password>
                string regex = "<" + nodeName + ">.*?</" + nodeName + ">"; //strip out 
                return System.Text.RegularExpressions.Regex.Replace(xml, regex, "<" + nodeName + ">XXX</" + nodeName + ">");
            }
            catch { return xml; }
        }

        //first time executing this I check if this is nav 7 or 6 web service
        protected void ExecuteWebRequest(ref string xmlRequest, ref string xmlResponse)
        {
            try
            {
                //first time comes in at NavWsVersion.Unknown so defaults to NAV7
                if (TimeOutInSeconds > 0)
                    navWebReference.Timeout = (TimeOutInSeconds - 2) * 1000;//-2 to make sure server timeout before client
                navWebReference.WebRequest(ref xmlRequest, ref xmlResponse);
            }
            catch (Exception ex)
            {
                //note pxmlResponce  vs  pxmlResponse
                if (ex.Message.Contains("pxmlResponse in method WebRequest in service"))
                {
                    // Are you connecting to NAV 2013 instead 2009?   7 vs 6
                    lock (Locker)
                    {
                        navWebReference.WebRequest(ref xmlRequest, ref xmlResponse);
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

        protected string HandleResponseCode(ref string xmlResponse, string[] codesToHandle = null)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResponse);
            string responseCode = ParseResponseCode(doc.GetElementsByTagName("Response_Code"));
            if (responseCode != "0000")
            {
                string navResponseId = ParseResponseCode(doc.GetElementsByTagName("Request_ID"));
                string navResponseText = ParseResponseText(doc.GetElementsByTagName("Response_Text"));

                //NavResponseCode navResponseCode = (NavResponseCode)Convert.ToInt32(responseCode);
                StatusCode statusCode = MapResponseToStatusCode(navResponseId, responseCode);
                string msg = string.Format("navResponseCode: {0} - {1}  [statuscode: {2}]", responseCode, navResponseText, statusCode.ToString());
                logger.Error(msg);

                if (codesToHandle != null && codesToHandle.Length > 0)
                {
                    foreach (string code in codesToHandle)
                    {
                        if (code.Equals(responseCode))
                            return code;
                    }
                }
                //expected return codes, so dont throw unexpected exception, rather return the known codes to client  
                throw new LSOmniServiceException(statusCode, msg);
            }
            return null;
        }

        protected StatusCode MapResponseToStatusCode(string navResponseId, string navCode)
        {
            /*
                MissingStoreNumber = 1001,
                MissingTenderLines = 1002,
             * 
             * */
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
                    if (navResponseId == "WEB_POS")
                        statusCode = StatusCode.InvalidNode;
                    break;
                case "0131":
                    statusCode = StatusCode.PasswordInvalid;
                    break;
                case "1001":
                    statusCode = StatusCode.Error;
                    if (navResponseId == "GET_DYN_CONT_HMP_MENUS" || navResponseId == "GET_DYNAMIC_CONTENT")
                        statusCode = StatusCode.NoHMPMenuFound;
                    else if (navResponseId == "MM_LOGIN_CHANGE") // cannot for "MM_CREATE_LOGIN_LINKS")
                        statusCode = StatusCode.UserNameInvalid;
                    else if (navResponseId == "IM_GET_CUSTOMER_CARD_01")
                        statusCode = StatusCode.CustomerNotFound;
                    else if (navResponseId == "IM_GET_ITEM_CARD_01")
                        statusCode = StatusCode.ItemNotFound;
                    else if (navResponseId == "IM_GET_VENDOR_CARD_01")
                        statusCode = StatusCode.VendorNotFound;
                    break;
                case "1002":
                    if (navResponseId == "MM_LOGIN_CHANGE")
                        statusCode = StatusCode.PasswordInvalid;
                    else if (navResponseId == "MM_CREATE_LOGIN_LINKS")
                        statusCode = StatusCode.UserNameNotFound;
                    break;
                case "1003":
                    if (navResponseId == "MM_LOGIN_CHANGE")
                        statusCode = StatusCode.UserNameInvalid;
                    else if (navResponseId == "MM_CREATE_LOGIN_LINKS")
                        statusCode = StatusCode.MemberCardNotFound;
                    break;
                case "1004":
                    if (navResponseId == "MM_LOGIN_CHANGE")
                        statusCode = StatusCode.UserNameExists;
                    break;
                case "1010":
                    if (navResponseId == "LOAD_PUBOFFERS_AND_PERSCOUPONS")
                        statusCode = StatusCode.UserNameExists;
                    break;
                case "1011":
                    if (navResponseId == "LOAD_PUBOFFERS_AND_PERSCOUPONS")
                        statusCode = StatusCode.ItemNotFound;
                    break;
                case "1012":
                    if (navResponseId == "LOAD_PUBOFFERS_AND_PERSCOUPONS")
                        statusCode = StatusCode.StoreNotExists;
                    break;
                case "1013":
                    if (navResponseId == "GET_DYN_CONT_HMP_MENUS" || navResponseId == "GET_DYNAMIC_CONTENT")
                        statusCode = StatusCode.HMPMenuNotEnabled;
                    break;
                case "1014":
                    if (navResponseId == "GET_DYN_CONT_HMP_MENUS" || navResponseId == "GET_DYNAMIC_CONTENT")
                        statusCode = StatusCode.HMPMenuNoDynamicContentFoundToday;
                    break;
                case "1020":
                    if (navResponseId == "LOAD_PUBOFFERS_AND_PERSCOUPONS")
                        statusCode = StatusCode.MemberCardNotFound;
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
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.ContactIdNotFound;
                    break;
                case "1600":
                    statusCode = StatusCode.PosNotExists;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.MemberCardNotFound;
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
                    statusCode = StatusCode.PriceToHigh;
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
                    statusCode = StatusCode.NoEntriesFound2;
                    break;
                case "1677":
                    statusCode = StatusCode.NoEntriesFound3;
                    break;
                case "1678":
                    statusCode = StatusCode.NoEntriesFound4;
                    break;

                case "1698":
                    statusCode = StatusCode.InvalidPrinterId;
                    break;
                case "1700":
                    if (navResponseId == "MM_CARD_TO_CONTACT")
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
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.CardInvalidStatus;
                    break;
                case "2000":
                    statusCode = StatusCode.InvalidPrintMethod;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.ContactIsBlocked;
                    break;
                case "0403":
                    statusCode = StatusCode.ServerRefusingToRespond;
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

            string proxyServer = ConfigSetting.GetString("Proxy.Server");
            proxyServer = proxyServer.Trim();
            int iProxyPort = 0;
            string proxyPort = ConfigSetting.GetString("Proxy.Port");
            string proxyUser = ConfigSetting.GetString("Proxy.User");
            string proxyPassword = ConfigSetting.GetString("Proxy.Password");
            string proxyDomain = ConfigSetting.GetString("Proxy.Domain");

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
