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

        public virtual Currency CurrencyGetLocal()
        {
            string id = config.SettingsGetByKey(ConfigKey.Currency_Code);
            string culture = config.SettingsGetByKey(ConfigKey.Currency_Culture);
            return BOAppConnection.CurrencyGetById(id, culture);
        }

        public virtual decimal GetPointRate()
        {
            return this.BOLoyConnection.GetPointRate();
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo)
        {
            return BOLoyConnection.GiftCardGetBalance(cardNo, config.SettingsGetByKey(ConfigKey.GiftCard_DataEntryType));
        }
    }
}

 