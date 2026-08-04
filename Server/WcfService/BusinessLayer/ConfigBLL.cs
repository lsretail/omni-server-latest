using System;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace LSOmni.BLL
{
    public class ConfigBLL : BaseBLL
    {
        private readonly IConfigRepository iConfigRepository;
        private readonly ILoyaltyBO BOLoyConnection = null;
        private static List<TokenList> tokenList = new List<TokenList>();

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
            if (myconfig == null || myconfig.LSKey == null || myconfig.LSKey.Key == null)
                return string.Empty;

            string protocol = myconfig.SettingsGetByKey(ConfigKey.BOProtocol);
            if (protocol.ToUpper().Equals("S2S") == false)
                return string.Empty;

            TokenList token = null;
            ConfigBLL bll = new ConfigBLL();
            try
            {
                if (tokenList == null)
                {
                    tokenList = new List<TokenList>();
                    logger.Warn(myconfig.LSKey.Key, "TokenList is null, initializing new list.");
                }

                token = tokenList.Find(x => x.Key == myconfig.LSKey.Key);
                if (token != null)
                {
                    token.LastId++;
                    if (token.LastId >= token.Tokens.Count)
                        token.LastId = 0;

                    if (token.Tokens[token.LastId].Expires > DateTime.UtcNow)
                    {
                        logger.Debug(myconfig.LSKey.Key, "Token for LS Central is still valid, using existing token for Id:{0}", token.LastId);
                        bll.ConfigSetByKey(myconfig.LSKey.Key, ConfigKey.Central_Token, token.Tokens[token.LastId].AccessToken, "string", true, "Active token");
                        myconfig.SettingsUpdateByKey(ConfigKey.Central_Token, token.Tokens[token.LastId].AccessToken);
                        return token.Tokens[token.LastId].AccessToken;
                    }
                }
                else
                {
                    token = new TokenList();
                    token.Tokens = new List<TokenData>();
                }
            }
            catch (Exception ex)
            {
                logger.Error(myconfig.LSKey.Key, ex, "Error checking token for LS Central");
                tokenList = new List<TokenList>();
                token = new TokenList()
                {
                    Tokens = new List<TokenData>()
                };
            }

            if (token.Tokens.Count == 0)
            {
                logger.Debug(myconfig.LSKey.Key, "No tokens found, creating new token list for Key:{0}", myconfig.LSKey.Key);

                token = new TokenList()
                {
                    Key = myconfig.LSKey.Key,
                    LastId = 0,
                    Tokens = new List<TokenData>()
                };

                string user = myconfig.SettingsGetByKey(ConfigKey.BOUser);
                string pwd = myconfig.SettingsGetByKey(ConfigKey.BOPassword);
                if (user.StartsWith("["))
                {
                    string[] userList = user.Split(new char[] { ']' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] pwdList = pwd.Split(new char[] { ']' }, StringSplitOptions.RemoveEmptyEntries);
                    if (userList.Length != pwdList.Length)
                        throw new ArgumentException("User and Password lists are not the same length.");

                    for (int i = 0; i < userList.Length; i++)
                    {
                        token.Tokens.Add(new TokenData()
                        {
                            Id = i,
                            ClientId = userList[i].TrimStart('[').Trim(),
                            ClientSecret = pwdList[i].TrimStart('[').Trim(),
                            AccessToken = string.Empty,
                            Expires = DateTime.MinValue
                        });
                    }
                }
                else
                {
                    token.Tokens.Add(new TokenData()
                    {
                        Id = 0,
                        ClientId = user,
                        ClientSecret = pwd,
                        AccessToken = string.Empty,
                        Expires = DateTime.MinValue
                    });
                }
                tokenList.Add(token);
            }

            TokenData tokenData = token.Tokens[token.LastId];
            if (string.IsNullOrEmpty(tokenData.ClientId))
                return string.Empty;

            string tenant = myconfig.SettingsGetByKey(ConfigKey.BOTenant);

            //check if the password has been encrypted by our LSOmniPasswordGenerator.exe
            if (DecryptConfigValue.IsEncryptedPwd(tokenData.ClientSecret))
            {
                tokenData.ClientSecret = DecryptConfigValue.DecryptString(tokenData.ClientSecret, myconfig.SettingsGetByKey(ConfigKey.EncrCode));
            }

            string scope = "https://api.businesscentral.dynamics.com/.default";
            string authurl = $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token";
            string body = $"grant_type=client_credentials&scope={scope}&client_id={tokenData.ClientId}&client_secret={tokenData.ClientSecret}";

            try
            {
                Uri posturl = new Uri(authurl);
                HttpWebRequest httpWebRequest = (HttpWebRequest)System.Net.WebRequest.Create(posturl);
                httpWebRequest.Method = "POST";

                logger.Debug(myconfig.LSKey.Key, "Send Token request for LS Central to:{0} ClientId:{1}", posturl.AbsoluteUri, tokenData.ClientId);
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

                    int postsec = myconfig.SettingsIntGetByKey(ConfigKey.BOS2SExpAdjust);
                    if (postsec == 0)
                        postsec = 300;

                    TokenS2S data = Serialization.Deserialize<TokenS2S>(result);
                    tokenData.AccessToken = data.token_type + " " + data.access_token;
                    tokenData.Expires = DateTime.UtcNow.AddSeconds(data.expires_in - postsec);

                    if (string.IsNullOrEmpty(token.Key))
                    {
                        bll.ConfigSetByKey(myconfig.LSKey.Key, ConfigKey.Central_Token, tokenData.AccessToken, "string", true, "Active token");
                        myconfig.SettingsUpdateByKey(ConfigKey.Central_Token, tokenData.AccessToken);
                    }
                }

                logger.Debug(myconfig.LSKey.Key, "Token for LS Central created for Id:{0} Expires:{1}", token.LastId, tokenData.Expires);
                return tokenData.AccessToken;
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

    internal class TokenList
    {
        public TokenList()
        {
            Tokens = new List<TokenData>();
            Key = string.Empty;
        }

        public string Key { get; set; }
        public int LastId { get; set; }
        public List<TokenData> Tokens { get; set; }
    }

    internal class TokenData
    {
        public TokenData()
        {
            ClientId = string.Empty;
            ClientSecret = string.Empty;
            AccessToken = string.Empty;
            Expires = DateTime.MinValue;
        }

        public int Id { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AccessToken { get; set; }
        public DateTime Expires { get; set; }
    }
}
