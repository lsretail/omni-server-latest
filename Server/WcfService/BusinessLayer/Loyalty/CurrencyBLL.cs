using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;

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

        public virtual Currency CurrencyGet(string code, Statistics stat)
        {
            string culture = config.SettingsGetByKey(ConfigKey.Currency_Culture);
            return BOAppConnection.CurrencyGetById(code, culture, stat);
        }

        public virtual decimal GetPointRate(string currency, Statistics stat)
        {
            return this.BOLoyConnection.GetPointRate(currency, stat);
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo, int pin, string entryType, Statistics stat)
        {
            string eType = (string.IsNullOrEmpty(entryType) ? config.SettingsGetByKey(ConfigKey.GiftCard_DataEntryType) : entryType);
            return BOLoyConnection.GiftCardGetBalance(cardNo, pin, eType, stat);
        }

        public virtual List<GiftCardEntry> GiftCardGetHistory(string cardNo, int pin, string entryType, Statistics stat)
        {
            string eType = (string.IsNullOrEmpty(entryType) ? config.SettingsGetByKey(ConfigKey.GiftCard_DataEntryType) : entryType);
            return BOLoyConnection.GiftCardGetHistory(cardNo, pin, eType, stat);
        }
    }
}

 