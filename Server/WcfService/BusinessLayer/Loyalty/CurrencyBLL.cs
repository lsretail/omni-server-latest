using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using System.Collections.Generic;
using System;

namespace LSOmni.BLL.Loyalty
{
    public class CurrencyBLL : BaseLoyBLL
    {
        public CurrencyBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
        }

        public virtual Currency CurrencyGetLocal(Statistics stat)
        {
            string id = config.SettingsGetByKey(ConfigKey.Currency_Code);
            string culture = config.SettingsGetByKey(ConfigKey.Currency_Culture);
            return BOAppConnection.CurrencyGetById(id, culture, stat);
        }

        public virtual decimal GetPointRate(Statistics stat)
        {
            return this.BOLoyConnection.GetPointRate(stat);
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo, string entryType, Statistics stat)
        {
            string etype = (string.IsNullOrEmpty(entryType) ? config.SettingsGetByKey(ConfigKey.GiftCard_DataEntryType) : entryType);
            return BOLoyConnection.GiftCardGetBalance(cardNo, etype, stat);
        }

        public virtual List<GiftCardEntry> GiftCardGetHistory(string cardNo, string entryType, Statistics stat)
        {
            string etype = (string.IsNullOrEmpty(entryType) ? config.SettingsGetByKey(ConfigKey.GiftCard_DataEntryType) : entryType);
            return BOLoyConnection.GiftCardGetHistory(cardNo, etype, stat);
        }
    }
}

 