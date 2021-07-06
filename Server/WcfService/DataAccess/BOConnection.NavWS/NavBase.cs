using System;
using System.Net;
using System.Net.Security;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSOmni.DataAccess.BOConnection.NavCommon;
using LSOmni.DataAccess.BOConnection.PreCommon;

namespace LSOmni.DataAccess.BOConnection.NavWS
{
    //Navision back office connection
    public class NavBase
    {
        protected int TimeOutInSeconds { get; set; }

        protected static LSLogger logger = new LSLogger();
        protected static BOConfiguration config = null;

        public static Version NAVVersion = null; //use this in code to check Nav version
        public static NavCommonBase NavWSBase = null;
        public static PreCommonBase LSCWSBase = null;

        public NavBase(BOConfiguration configuration, bool ping = false)
        {
            if (configuration == null && !ping)
            {
                throw new LSOmniServiceException(StatusCode.SecurityTokenInvalid, "SecurityToken invalid");
            }
            config = configuration;

            NavWSBase = new NavCommonBase(configuration);
            LSCWSBase = new PreCommonBase(configuration);
            NAVVersion = LSCWSBase.LSCVersion;

            string domain = "";
            NetworkCredential credentials = null;

            //check if domain is part of the username
            string username = config.SettingsGetByKey(ConfigKey.BOUser);
            string password = config.SettingsGetByKey(ConfigKey.BOPassword);

            //check if domain is part of the config.UserName
            if (username.Contains("/") || username.Contains(@"\"))
            {
                username = username.Replace(@"/", @"\");
                string[] splitter = username.Split('\\');
                domain = splitter[0];
                username = splitter[1];
                //logger.Debug("domain:{0} config.UserName:{1}", domain, config.UserName);
            }

            //check if the password has been encrypted by our LSOmniPasswordGenerator.exe
            if (DecryptConfigValue.IsEncryptedPwd(password))
            {
                password = DecryptConfigValue.DecryptString(password);
            }

            if (string.IsNullOrWhiteSpace(username) == false && string.IsNullOrWhiteSpace(password) == false)
            {
                credentials = new NetworkCredential(username, password, domain);
            }

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
        }

        private bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
