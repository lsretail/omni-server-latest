using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu.SpecialCase
{
    /// <summary>
    /// Unknown modifier. Modifiers that we receive with a transaction from the BO, but can't find in the menus.
    /// </summary>
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class UnknownModifier : Modifier
	{
		private string itemId;
		//private string description;

		public string ItemId { get { return this.itemId; } } 
		//public string Description { get { return this.description; } }

		public UnknownModifier(
			string id,
			string itemId, 
			string description, 
			decimal price, 
			decimal quantity,
			string externalId,
			string externalLineNumber,
			string kitchenStatus,
			string kitchenStatusCode
		) : base(id)
		{
			this.itemId = itemId;
			this.Description = description;

			this.Price = price;

			this.Quantity = quantity;
			this.OriginalQty = quantity;
			this.MaximumSelection = quantity;
			this.MinimumSelection = quantity;

			this.ExternalIdRO = externalId;
			this.ExternalLineNumberRO = externalLineNumber;
			this.KitchenStatusRO = kitchenStatus;
			this.KitchenStatusCodeRO = kitchenStatusCode;
		}

        public UnknownModifier()
        {
        }
			
		public new UnknownModifier Clone()
		{
			UnknownModifier um = this.MemberwiseClone() as UnknownModifier;
			um.Item = (this.Item != null ? this.Item.Clone() : null);
			return um;
		}
	}
}