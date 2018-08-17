using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu.SpecialCase
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class UnknownTextModifier : TextModifier
	{
		public UnknownTextModifier(
			string id,
			decimal qty,
			string description,
			string externalId,
			string externalLineNumber,
			string kitchenStatus,
			string kitchenStatusCode
		) : base(id)
		{
			this.Description = description;
			this.Quantity = qty;

			this.ExternalIdRO = externalId;
			this.ExternalLineNumberRO = externalLineNumber;
			this.KitchenStatusRO = kitchenStatus;
			this.KitchenStatusCodeRO = kitchenStatusCode;
		}

        public UnknownTextModifier()
        {
        }

		public new UnknownTextModifier Clone ()
		{
			return this.MemberwiseClone() as UnknownTextModifier;
		}
	}
}