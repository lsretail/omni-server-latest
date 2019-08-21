using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase
{
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
	public class UnknownCurrency : Currency
	{
		public UnknownCurrency() : base()
		{
		}

		public UnknownCurrency(string id) : base(id)
		{
		}
	}
}
