using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using Newtonsoft.Json;

namespace LSOmni.BLL.Loyalty
{
    //There is no Interface for LS_Recommends so simply checking if the dll is available in the distribution
    //  read from the appsettings.config
    public class LSRecommendsBLL : BaseLoyBLL
    {
        private static LSLogger logger = new LSLogger();

        public LSRecommendsBLL(BOConfiguration config, bool settings) : base(config, 0)
        {
            if (settings)   // sending settings, don't load them
                return;

            string value = config.SettingsGetByKey(ConfigKey.LSRecommend_ModelUrl);
            if (string.IsNullOrEmpty(value))
                throw new LSOmniServiceException(StatusCode.LSRecommendSetupMissing, "LS Recommend Setup has not yet been sent to LS Commerce Service from LS Central");
        }

        public virtual void LSRecommendSetting(string lskey, string batchNo, string modelReaderURL, string authenticationURL, string clientId, string clientSecret, string userName, string password, int numberOfDownloadedItems, int numberOfDisplayedItems, bool filterByInventory, decimal minInvStock)
        {
            ConfigBLL bll = new ConfigBLL();
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_BatchNo, batchNo, "string", true, "Batch Number");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_ModelUrl, modelReaderURL, "string", true, "Model Reader URL");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_AuthUrl, authenticationURL, "string", true, "Authentication URL");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_ClientId, clientId, "string", true, "Client Id");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_ClientSecret, clientSecret, "string", true, "Client Secret");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_UserName, userName, "string", true, "Web Service User");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_Password, password, "string", true, "Web Service Password");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_NoOfDownloadedItems, numberOfDownloadedItems.ToString(), "int", true, "Number of Downloaded Items");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_NoOfDisplayedItems, numberOfDisplayedItems.ToString(), "int", true, "Number of Displayed Items");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_FilterByInv, filterByInventory.ToString(), "bool", true, "Filter by Inventory");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_MinStock, minInvStock.ToString(), "int", true, "Minimum Item Inventory Stock");
        }

        public virtual List<RecommendedItem> RecommendedItemsGet(List<string> items)
        {
            string token = CheckToken();
            if (string.IsNullOrEmpty(token))
                throw new LSOmniServiceException(StatusCode.LSRecommendError, "Cannot Get Token to access LS Recommend Service");

            string url = config.SettingsGetByKey(ConfigKey.LSRecommend_ModelUrl);
            if (string.IsNullOrEmpty(url))
                throw new LSOmniServiceException(StatusCode.LSRecommendSetupMissing, "Missing LSRecommend_ModelUrl in Appsettings");

            string requrl = $"{url}/api/{config.SettingsGetByKey(ConfigKey.LSRecommend_BatchNo)}/BasketRecommendation?numberOfRecommendations={config.SettingsGetByKey(ConfigKey.LSRecommend_NoOfDownloadedItems)}";

            string json = string.Format("{{\"items\": {0}}}", JsonConvert.SerializeObject(items));
            string result = SendCommand(requrl, json, token);
            return JsonConvert.DeserializeObject<List<RecommendedItem>>(result);
        }

        private string CheckToken()
        {
            if (config.LSRecommToken != null)
            {
                if (DateTime.Now < config.LSRecommToken.lastCheck.AddSeconds(config.LSRecommToken.expires_in - 100))
                {
                    return config.LSRecommToken.token_type + " " + config.LSRecommToken.access_token;
                }
            }

            string url = config.SettingsGetByKey(ConfigKey.LSRecommend_AuthUrl);
            if (string.IsNullOrEmpty(url))
                throw new LSOmniServiceException(StatusCode.LSRecommendSetupMissing, "Missing LSRecommend_AuthUrl in Appsettings");

            string body = $"grant_type=password&client_id={config.SettingsGetByKey(ConfigKey.LSRecommend_ClientId)}&client_secret={config.SettingsGetByKey(ConfigKey.LSRecommend_ClientSecret)}&username={config.SettingsGetByKey(ConfigKey.LSRecommend_UserName)}&password={config.SettingsGetByKey(ConfigKey.LSRecommend_Password)}";

            string result = SendCommand(url, body, string.Empty);
            config.LSRecommToken = JsonConvert.DeserializeObject<LSRecommendToken>(result);
            config.LSRecommToken.lastCheck = DateTime.Now;
            return config.LSRecommToken.token_type + " " + config.LSRecommToken.access_token;
        }

        protected string SendCommand(string url, string data, string authcode)
        {
            try
            {
                Uri posturl = new Uri(url);
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(posturl);
                httpWebRequest.Method = "POST";

                logger.Debug(config.LSKey.Key, "Send LS Recommend Cmd to:{0} Message:{1}", posturl.AbsoluteUri, data);
                if (string.IsNullOrEmpty(authcode))
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(data); //json

                    httpWebRequest.Accept = "application/json";
                    httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                    httpWebRequest.ContentLength = byteArray.Length;

                    Stream streamWriter = httpWebRequest.GetRequestStream();
                    streamWriter.Write(byteArray, 0, byteArray.Length);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                else
                {
                    httpWebRequest.Headers["Authorization"] = authcode;
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.ContentLength = data.Length;

                    using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        streamWriter.Write(data);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }

                string ret = string.Empty;
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    ret = streamReader.ReadToEnd();
                }

                logger.Debug(config.LSKey.Key, "ECOM Result:[{0}]", ret);
                return ret;
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex);
                throw new LSOmniServiceException(StatusCode.LSRecommendError, "Error sending request to LS Recommend Engine", ex);
            }
        }
    }
}

 