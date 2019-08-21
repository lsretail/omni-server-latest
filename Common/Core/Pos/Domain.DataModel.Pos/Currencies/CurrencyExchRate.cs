using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Pos.Currencies.SpecialCase;

namespace LSRetail.Omni.Domain.DataModel.Pos.Currencies
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017"), KnownType(typeof(UnknownCurrencyExchRate))]
    public class CurrencyExchRate : Entity, IAggregateRoot
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime StartingDate { get; set; }
        [DataMember]
        public decimal CurrencyFactor { get; set; }

        public CurrencyExchRate ()
		{
			StartingDate = new DateTime();
			CurrencyFactor = 0;
		}

		public CurrencyExchRate (string currencyCode) : base (currencyCode)
		{
			StartingDate = new DateTime();
			CurrencyFactor = 0;
		}
	}
}