using System;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using System.IO;
using System.Net;
using System.Text;

namespace LSOmni.BLL
{
    public class ConfigBLL : BaseBLL
    {
        private readonly IConfigRepository iConfigRepository;
        private readonly ILoyaltyBO BOLoyConnection = null;

        public ConfigBLL(BOConfiguration config) : base(config)
        {
            this.iConfigRepository = GetDbRepository<IConfigRepository>(config);

            if (config != null)
                BOLoyConnection = GetBORepository<ILoyaltyBO>(config?.LSKey.Key, config.IsJson);
        }

        public ConfigBLL() : base(null)
        {
            this.iConfigRepository = GetDbRepository<IConfigRepository>(config);
        }

        public virtual bool ConfigKeyExists(ConfigKey key, string lsKey)
        {
            return this.iConfigRepository.ConfigKeyExists(lsKey, key);
        }

        public virtual void ConfigSetByKey(string lsKey, ConfigKey key, string value, string valueType, bool advanced, string comment)
        {
            this.iConfigRepository.ConfigSetByKey(lsKey, key, value, valueType, advanced, comment);
        }

        public virtual BOConfiguration ConfigGet(string key)
        {
            //check if key is active
            if (iConfigRepository.ConfigIsActive(key) == false)
            {
                throw new LSOmniServiceException(StatusCode.LSKeyInvalid, "LSKey is not active");
            }

            return iConfigRepository.ConfigGet(key);
        }

        public virtual void DbCleanup(int daysLog, int daysNotify, int daysOneList)
        {
            this.iConfigRepository.DbCleanUp(daysLog, daysNotify, daysOneList);
        }

        public virtual string CheckToken(BOConfiguration myconfig)
        {
            if (myconfig == null)
                return string.Empty;

            string protocol = myconfig.SettingsGetByKey(ConfigKey.BOProtocol);
            if (protocol.ToUpper().Equals("S2S") == false)
                return string.Empty;

            string token = myconfig.SettingsGetByKey(ConfigKey.Central_Token);
            if (string.IsNullOrEmpty(token) == false)
            {
                DateTime regtime = DateTime.MinValue;
                string reg = myconfig.SettingsGetByKey(ConfigKey.Central_TokenTime);
                if (string.IsNullOrEmpty(reg) == false)
                    regtime = Convert.ToDateTime(reg);

                if (regtime > DateTime.UtcNow)
                {
                    return token;
                }
            }

            string clientId = myconfig.SettingsGetByKey(ConfigKey.BOUser);
            string clientSecret = myconfig.SettingsGetByKey(ConfigKey.BOPassword);
            string tenant = myconfig.SettingsGetByKey(ConfigKey.BOTenant);

            if (string.IsNullOrEmpty(clientId))
                return string.Empty;

            //check if the password has been encrypted by our LSOmniPasswordGenerator.exe
            if (DecryptConfigValue.IsEncryptedPwd(clientSecret))
            {
                clientSecret = DecryptConfigValue.DecryptString(clientSecret, myconfig.SettingsGetByKey(ConfigKey.EncrCode));
            }

            string scope = "https://api.businesscentral.dynamics.com/.default";
            string authurl = $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token";
            string body = $"grant_type=client_credentials&scope={scope}&client_id={clientId}&client_secret={clientSecret}";

            try
            {
                Uri posturl = new Uri(authurl);
                HttpWebRequest httpWebRequest = (HttpWebRequest)System.Net.WebRequest.Create(posturl);
                httpWebRequest.Method = "POST";

                logger.Debug(myconfig.LSKey.Key, "Send Token request for LS Central to:{0} Message:{1}", posturl.AbsoluteUri, body);
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
                    logger.Debug(myconfig.LSKey.Key, "Token Result:[{0}]", result.Substring(0, 100));

                    TokenS2S data = Serialization.Deserialize<TokenS2S>(result);
                    token = data.token_type + " " + data.access_token;

                    ConfigBLL bll = new ConfigBLL();
                    bll.ConfigSetByKey(myconfig.LSKey.Key, ConfigKey.Central_Token, token, "string", true, "Active token");
                    bll.ConfigSetByKey(myconfig.LSKey.Key, ConfigKey.Central_TokenTime, DateTime.UtcNow.AddSeconds(data.expires_in - 90).ToString(), "string", true, "Token Reg");
                    myconfig.SettingsUpdateByKey(ConfigKey.Central_Token, token);
                }
                return token;
            }
            catch (Exception ex)
            {
                logger.Error(myconfig.LSKey.Key, ex);
                throw new LSOmniServiceException(StatusCode.SecurityTokenInvalid, "Error getting token", ex);
            }
        }

        #region ping

        public virtual void PingOmniDB()
        {
            iConfigRepository.PingOmniDB();
        }

        public virtual string PingWs(out string centralVersion)
        {
            // Nav returns the version number
            return BOLoyConnection.Ping(out centralVersion);
        }

        public virtual string PingNavDb()
        {
            BOLoyConnection.TimeoutInSeconds = 4;
            string asm = GetBOAssemblyName();
            if (asm.ToLower().Contains("navws.dll"))
                return "SaaS";

            Scheme ret = BOLoyConnection.SchemeGetById("Ping", new Statistics());
            return ret.Id;
        }

        #endregion ping
    }
}
