using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Pos.Currencies.SpecialCase
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class UnknownCurrencyExchRate : CurrencyExchRate
	{
		public UnknownCurrencyExchRate (string currencyCode) : base(currencyCode)
		{
		}
	}
}

