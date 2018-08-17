using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu.SpecialCase
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public sealed class UnknownMenuItem : MenuItem
	{
		public bool IsADeal;

		public List<Modifier> ModifierDTOs;
		public List<TextModifier> TextModifierDTOs;
        public List<MenuDealLine> DealLineDTOs;

		/// Unknown modifiers are modifiers that we get from the BO when retrieving a transaction,
		/// but aren't part of the menu that the hosp item came from,
		/// and so aren't part of the hosp item's 'normal' set of modifiers (those that are defined in the menu it came from).
		public List<UnknownModifier> UnknownModifiers { get; private set; }
		public List<UnknownTextModifier> UnknownTextModifiers { get; private set; }
		public List<UnknownDealLine> UnknownDealLines { get; private set; }

		public UnknownMenuItem(string itemId) : base(itemId)
		{
			this.ModifierDTOs = new List<Modifier>();
			this.TextModifierDTOs = new List<TextModifier>();
            this.DealLineDTOs = new List<MenuDealLine>();

			this.UnknownModifiers = new List<UnknownModifier>();
			this.UnknownTextModifiers = new List<UnknownTextModifier>();
			this.UnknownDealLines = new List<UnknownDealLine>();
		}

        public UnknownMenuItem() : this(string.Empty)
        {
        }

		public override MenuItem Clone()
		{
            UnknownMenuItem item = (UnknownMenuItem)base.Clone();

			item.UnknownModifiers = new List<UnknownModifier>();
			item.UnknownTextModifiers = new List<UnknownTextModifier>();
			item.UnknownDealLines = new List<UnknownDealLine>();

			this.UnknownModifiers.ForEach(x => item.UnknownModifiers.Add(x.Clone()));
			this.UnknownTextModifiers.ForEach(x => item.UnknownTextModifiers.Add(x.Clone()));
			this.UnknownDealLines.ForEach(x => item.UnknownDealLines.Add(x.Clone()));

			item.IsADeal = this.IsADeal;

			return item;
		}

		private void AddUnknownModifier(Modifier dto)
		{
			this.UnknownModifiers.Add(
				new UnknownModifier(
					dto.Id,
                    dto.Item.Id,
					dto.Description,
					dto.Price,
                    dto.Quantity,
					dto.ExternalIdRO,
					dto.ExternalLineNumberRO,
					dto.KitchenStatusRO,
					dto.KitchenStatusCodeRO
				)
			);
		}

		private void AddUnknownTextModifier(TextModifier dto)
		{
			this.UnknownTextModifiers.Add(
				new UnknownTextModifier(
					dto.Id,
                    dto.Quantity,
					dto.Description,
					dto.ExternalIdRO,
					dto.ExternalLineNumberRO,
					dto.KitchenStatusRO,
					dto.KitchenStatusCodeRO
				)
			);
		}

		private void AddUnknownDealLine(MenuDealLine dto)
		{
			this.UnknownDealLines.Add(new UnknownDealLine(dto));
		}

		internal void ConvertDTOsToUnknownModifiers()
		{
			this.ModifierDTOs.ForEach(x => AddUnknownModifier(x));
			this.TextModifierDTOs.ForEach(x => AddUnknownTextModifier(x));
			this.DealLineDTOs.ForEach(x => AddUnknownDealLine(x));
		}

		public override void SatisfyModifierGroupsMinSelectionRestrictions()
		{
			// Do nothing
		}

		/* Don't need this. We can't mess with unknown hosp items, will just use the sale line price, don't have to calculate the price
        public override Money GetFullPrice()
        {
            return new Money(FullPrice, this.Price.Currency);
        }
            
        public override decimal FullPrice
        {
            get
            {
                var price = base.FullPrice;

                foreach (var unknownDealLine in UnknownDealLines)
                {
                    price += unknownDealLine.UnknownDealLinePriceAdjustment;
                }

                price += UnknownModifierPriceAdjustment;

                return price;
            }
        }


        public decimal UnknownModifierPriceAdjustment
        {
            get
            {
                var price = 0m;

                foreach (var unknownModifier in UnknownModifiers)
                {
                    // TODO Verify this, is this correct?
                    // Unknown modifiers will always increase the price, since they aren't part of the menu, and therefore not factored into the hosp item price
                    price += unknownModifier.Price * unknownModifier.Qty;   
                }

                return price;
            }
        }
        */
	}
}