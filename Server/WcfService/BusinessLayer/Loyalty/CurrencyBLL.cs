using LSOmni.DataAccess.Dal;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.BLL.Loyalty
{
    public class CurrencyBLL : BaseLoyBLL
    {
        public CurrencyBLL(int timeoutInSeconds)
            : this("", timeoutInSeconds)
        {
        }

        public CurrencyBLL(string securityToken, int timeoutInSeconds)
            : base(securityToken, timeoutInSeconds)
        {
        }

        public virtual Currency CurrencyGetLocal()
        {
            AppSettingsRepository appRep = new AppSettingsRepository();
            string id = appRep.AppSettingsGetByKey(AppSettingsKey.Currency_Code);
            string culture = appRep.AppSettingsGetByKey(AppSettingsKey.Currency_Culture, "en");
            return BOAppConnection.CurrencyGetById(id, culture);
        }

        public virtual decimal GetPointRate()
        {
            return this.BOLoyConnection.GetPointRate();
        }
    }
}

 