using System;
using System.Net;
using System.Net.Security;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSOmni.DataAccess.BOConnection.PreCommon;

namespace LSOmni.DataAccess.BOConnection.CentralPre
{
    //Navision back office connection
    public class NavBase
    {
        private int TimeoutSec = 0;
        protected int TimeOutInSeconds 
        { 
            get
            {
                return TimeoutSec;
            }
            set
            {
                TimeoutSec = value;
                LSCentralWSBase.TimeOutInSeconds = TimeoutSec;
            }
        }

        protected static LSLogger logger = new LSLogger();

        protected static BOConfiguration config = null;

        public static Version LSCVersion = null; //use this in code to check Nav version
        public static PreCommonBase LSCentralWSBase = null;

        public NavBase(BOConfiguration configuration)
        {
            config = configuration;

            LSCentralWSBase = new PreCommonBase(configuration);
            LSCVersion = LSCentralWSBase.LSCVersion;

            NetworkCredential credentials = null;

            string username = config.SettingsGetByKey(ConfigKey.BOUser);
            string password = config.SettingsGetByKey(ConfigKey.BOPassword);
            string domain = string.Empty;

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

        #region private members

        private bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        #endregion private members
    }
}
