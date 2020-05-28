using System;
using System.Collections.Generic;
using System.Globalization;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;

namespace LSOmni.BLL.Loyalty
{
    //There is no Interface for LS_Recommends so simply checking if the dll is available in the distribution
    //  read from the appsettings.config
    public class LSRecommendsBLL : BaseLoyBLL
    {
        private static LSLogger logger = new LSLogger();
        private LSRecommend.LSRecommend lsr = null;

        public LSRecommendsBLL(BOConfiguration config) : base(config, 0)
        {
            string value = config.SettingsGetByKey(ConfigKey.LSRecommend_EndPointUrl);
            if (string.IsNullOrEmpty(value))
                throw new LSOmniServiceException(StatusCode.LSRecommendSetupMissing, "LS Recommend Setup has not yet been sent to Omni from LS Central");

            lsr = new LSRecommend.LSRecommend(
                config.SettingsGetByKey(ConfigKey.LSRecommend_EndPointUrl),
                string.Empty,   // apiAdminKey
                config.SettingsGetByKey(ConfigKey.LSRecommend_AzureAccountKey),
                config.SettingsGetByKey(ConfigKey.LSRecommend_AccountConnection),
                config.SettingsGetByKey(ConfigKey.LSRecommend_AzureName),
                config.SettingsIntGetByKey(ConfigKey.LSRecommend_NumberOfRecommendedItems),
                config.SettingsBoolGetByKey(ConfigKey.LSRecommend_CalculateStock),
                config.SettingsGetByKey(ConfigKey.LSRecommend_WsURI),
                config.SettingsGetByKey(ConfigKey.LSRecommend_WsUserName),
                config.SettingsGetByKey(ConfigKey.LSRecommend_WsPassword),
                config.SettingsGetByKey(ConfigKey.LSRecommend_WsDomain),
                config.SettingsGetByKey(ConfigKey.LSRecommend_StoreNo),
                config.SettingsGetByKey(ConfigKey.LSRecommend_Location),
                config.SettingsIntGetByKey(ConfigKey.LSRecommend_MinStock)
            );
        }

        public virtual void LSRecommendSetting(string lskey, string endPointUrl, string accountConnection, string azureAccountKey, string azureName, int numberOfRecommendedItems, bool calculateStock, string wsURI, string wsUserName, string wsPassword, string wsDomain, string storeNo, string location, int minStock)
        {
            ConfigBLL bll = new ConfigBLL();
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_EndPointUrl, endPointUrl, "string", true, "Endpoint URI");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_AccountConnection, accountConnection, "string", true, "Storage Account Connection");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_AzureAccountKey, azureAccountKey, "string", true, "LS Recommend Account Key");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_AzureName, azureName, "string", true, "Azure name");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_NumberOfRecommendedItems, numberOfRecommendedItems.ToString(), "int", true, "Number Of Recommended Items");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_CalculateStock, calculateStock.ToString(), "bool", true, "Filter With Regard To Stock");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_WsURI, wsURI, "string", true, "Web Service URI");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_WsUserName, wsUserName, "string", true, "Web Service User");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_WsPassword, wsPassword, "string", true, "Web Service Password");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_WsDomain, wsDomain, "string", true, "Web Service Domain");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_StoreNo, storeNo, "string", true, "Store No");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_Location, location, "string", true, "Location");
            bll.ConfigSetByKey(lskey, ConfigKey.LSRecommend_MinStock, minStock.ToString(), "int", true, "Minimum Item Stock");
        }

        public virtual List<RecommendedItem> RecommendedItemsGetByUserId(string userId, List<LoyItem> items, int maxNumberOfItems, double minRating = 0.0)
        {
            string lsrItemsIn = string.Empty;
            foreach (LoyItem itemIn in items)
            {
                lsrItemsIn += itemIn.Id + ",";
            }

            string[] lsrItems = lsr.GetRecommendation(userId, lsrItemsIn).Split();
            List<RecommendedItem> list = new List<RecommendedItem>();
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

        /// <summary>
        /// Get Recommended items from NAV
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="storeId">store id</param>
        /// <param name="items">one or more items to get recommend for (item1,item2,item3)</param>
        /// <returns>List of recommended items</returns>
        public virtual List<RecommendedItem> RecommendedItemsGet(string userId, string storeId, string items)
        {
            string lsrRet = lsr.GetRecommendation(userId, items);
            string[] lsrItems = lsrRet.Split(new char[] { ',' });

            NumberFormatInfo pr = new NumberFormatInfo();
            pr.NumberDecimalSeparator = ".";

            List<RecommendedItem> list = new List<RecommendedItem>();
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
    }
}

 