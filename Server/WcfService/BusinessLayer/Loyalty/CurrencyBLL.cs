using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;

namespace LSOmni.BLL.Loyalty
{
    public class CurrencyBLL : BaseLoyBLL
    {
        private IAppSettingsRepository iAppSettingsRepository;

        public CurrencyBLL(int timeoutInSeconds)
            : this("", timeoutInSeconds)
        {
        }

        public CurrencyBLL(string securityToken, int timeoutInSeconds)
            : base(securityToken, timeoutInSeconds)
        {
            this.iAppSettingsRepository = GetDbRepository<IAppSettingsRepository>();
        }

        public virtual Currency CurrencyGetLocal()
        {
            string id = iAppSettingsRepository.AppSettingsGetByKey(AppSettingsKey.Currency_Code);
            string culture = iAppSettingsRepository.AppSettingsGetByKey(AppSettingsKey.Currency_Culture, "en");
            return BOAppConnection.CurrencyGetById(id, culture);
        }

        public virtual decimal GetPointRate()
        {
            return this.BOLoyConnection.GetPointRate();
        }

        public virtual GiftCard GiftCardGetBalance(string cardNo)
        {
            string entryType = iAppSettingsRepository.AppSettingsGetByKey(AppSettingsKey.GiftCard_DataEntryType, "en");
            return BOLoyConnection.GiftCardGetBalance(cardNo, entryType);
        }
    }
}

 