using System;
using System.Net;
using System.Net.Security;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSOmni.DataAccess.BOConnection.NavCommon;

namespace LSOmni.DataAccess.BOConnection.CentrAL
{
    //Navision back office connection
    public class NavBase
    {
        protected int TimeOutInSeconds { get; set; }

        protected static LSLogger logger = new LSLogger();

        protected static BOConfiguration config = null;

        public static Version NAVVersion = null; //use this in code to check Nav version
        public static NavCommonBase NavWSBase = null;

        public NavBase(BOConfiguration configuration)
        {
            config = configuration;

            NavWSBase = new NavCommonBase(configuration);
            NAVVersion = NavWSBase.NAVVersion;

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
