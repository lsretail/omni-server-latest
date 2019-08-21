using System;
using System.Collections.Generic;
using System.Globalization;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;

namespace LSOmni.BLL.Loyalty
{
    //LSRecommendsBLLStatic is just for temp usage while there LS Recommends.dll isn't a part of the official release.
    //  to give a useful error message back
    public static class LSRecommendsBLLStatic
    {
        private static object lsRecommendsObj = null;
        private static bool isAvail = false;

        public static bool IsLSRecommendsAvail()
        {
            if (lsRecommendsObj == null)
            {
                string asm = "";
                string key = "LSRecommends.AssemblyName"; //key in app.settings

                if (ConfigSetting.KeyExists(key))
                    asm = ConfigSetting.GetString(key);
                else
                    asm = "LSRecommend.dll"; //just in case the key is missing in app.settings file

                string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                appPath = appPath.Replace("file:\\", "");
                asm = appPath + "\\" + asm;
                if (System.IO.File.Exists(asm))
                {
                    isAvail = true;
                    lsRecommendsObj = new object();
                }
            }

            return isAvail;
        }
    }

    //There is no Interface for LS_Recommends so simply checking if the dll is available in the distribution
    //  read from the appsettings.config
    public class LSRecommendsBLL : BaseLoyBLL
    {
        private static LSLogger logger = new LSLogger();
        private LSRecommend.LSRecommend lsr;

        public LSRecommendsBLL(BOConfiguration config) : base(config, 0)
        {
        }

        public LSRecommendsBLL(BOConfiguration config, int timeoutInSeconds) : base (config, timeoutInSeconds)
        {
            lsr = new LSRecommend.LSRecommend(
                config.SettingsGetByKey(ConfigKey.LSReccomend_EndPointUrl),
                string.Empty,   // apiAdminKey
                config.SettingsGetByKey(ConfigKey.LSReccomend_AzureAccountKey),
                config.SettingsGetByKey(ConfigKey.LSReccomend_AccountConnection),
                config.SettingsGetByKey(ConfigKey.LSReccomend_AzureName),
                config.SettingsIntGetByKey(ConfigKey.LSReccomend_NumberOfRecommendedItems),
                config.SettingsBoolGetByKey(ConfigKey.LSReccomend_CalculateStock),
                config.SettingsGetByKey(ConfigKey.LSReccomend_WsURI),
                config.SettingsGetByKey(ConfigKey.LSReccomend_WsUserName),
                config.SettingsGetByKey(ConfigKey.LSReccomend_WsPassword),
                config.SettingsGetByKey(ConfigKey.LSReccomend_WsDomain),
                config.SettingsGetByKey(ConfigKey.LSReccomend_StoreNo),
                config.SettingsGetByKey(ConfigKey.LSReccomend_Location),
                config.SettingsIntGetByKey(ConfigKey.LSReccomend_MinStock)
            );
        }

        public virtual void LSRecommendSetting(string endPointUrl, string accountConnection, string azureAccountKey, string azureName, int numberOfRecommendedItems, bool calculateStock, string wsURI, string wsUserName, string wsPassword, string wsDomain, string storeNo, string location, int minStock)
        {
            ConfigBLL bll = new ConfigBLL();

            bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSReccomend_EndPointUrl, endPointUrl, "string");
            bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSReccomend_AccountConnection, accountConnection, "string");
            bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSReccomend_AzureAccountKey, azureAccountKey, "string");
            bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSReccomend_AzureName, azureName, "string");
            bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSReccomend_NumberOfRecommendedItems, numberOfRecommendedItems.ToString(), "int");
            bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSReccomend_CalculateStock, calculateStock.ToString(), "bool");
            bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSReccomend_WsURI, wsURI, "string");
            bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSReccomend_WsUserName, wsUserName, "string");
            bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSReccomend_WsPassword, wsPassword, "string");
            bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSReccomend_WsDomain, wsDomain, "string");
            bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSReccomend_StoreNo, storeNo, "string");
            bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSReccomend_Location, location, "string");
            bll.ConfigSetByKey(config.LSKey.Key, ConfigKey.LSReccomend_MinStock, minStock.ToString(), "int");
        }

        public virtual List<RecommendedItem> RecommendedItemsGetByUserId(string userId, List<LoyItem> items, int maxNumberOfItems, double minRating = 0.0)
        {
            try
            {
                if (maxNumberOfItems < 1)
                    maxNumberOfItems = 50;
                List<RecommendedItem> list = new List<RecommendedItem>();

                string lsrItemsIn = string.Empty;
                foreach (LoyItem itemIn in items)
                {
                    lsrItemsIn += itemIn.Id + ",";
                }

                string[] lsrItems = lsr.GetRecommendation(userId, lsrItemsIn).Split();
                list = new List<RecommendedItem>();
                foreach (string itemIn in lsrItems)
                {
                    RecommendedItem i = new RecommendedItem();
                    string[] values = itemIn.Split(new char[] { ':' });
                    i.Id = values[0];
                    i.Rating = Convert.ToDecimal(values[1]);
                    i.Reasoning = values[2];
                    list.Add(i);
                }
                return list;
            }
            catch (LSOmniServiceException ex)
            {
                logger.Error(config.LSKey.Key, ex, "LSRecommendsGetByUserAndItems failed");
                throw;
            }
        }

        /// <summary>
        /// Get Recommended items from NAV
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="storeId">store id</param>
        /// <param name="items">one or more items to get recommend for (item1,item2,item3)</param>
        /// <returns>List of recommended items</returns>
        public virtual List<RecommendedItem> RecommendedItemsGet(string userId, string storeId, string items)
        {
            try
            {
                List<RecommendedItem> list = new List<RecommendedItem>();
                string lsrRet = lsr.GetRecommendation(userId, items);
                string[] lsrItems = lsrRet.Split(new char[] { ',' });

                NumberFormatInfo pr = new NumberFormatInfo();
                pr.NumberDecimalSeparator = ".";

                list = new List<RecommendedItem>();
                foreach (string itemIn in lsrItems)
                {
                    if (string.IsNullOrWhiteSpace(itemIn))
                        continue;

                    RecommendedItem i = new RecommendedItem();
                    string[] values = itemIn.Split(new char[] { ':' });
                    i.Id = values[0];
                    i.Rating = Convert.ToDecimal(values[1], pr);
                    i.Reasoning = values[2];
                    list.Add(i);
                }

                logger.Debug(config.LSKey.Key, "Found {0} Recommended Items: {1}", list.Count, lsrRet);
                return list;
            }
            catch (LSOmniServiceException ex)
            {
                logger.Error(config.LSKey.Key, ex, "LSRecommendsGetByUserAndItems failed");
                throw;
            }
        }
    }
}

 