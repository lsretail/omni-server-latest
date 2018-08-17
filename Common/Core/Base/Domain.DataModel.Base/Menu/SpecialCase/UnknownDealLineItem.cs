using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu.SpecialCase
{
    /// <summary>
    /// Unknown deal line item, i.e. a deal line item that we received from the BO but couldn't find in the menus. Really just a data storage class.
    /// </summary>
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class UnknownDealLineItem : Entity
    {
        private string itemId;
        private decimal priceAdjustment;
        private string externalIdRO;
        private string externalLineNoRO;
        private string kitchenStatusRO;
        private string kitchenStatusCodeRO;
        private decimal quantity;
        private string description;
        private List<UnknownModifier> unknownModifiers;
        private List<UnknownTextModifier> unknownTextModifiers;

        public string ItemId { get { return this.itemId; } }
        public decimal PriceAdjustment { get { return this.priceAdjustment; } }
        public string ExternalIdRO { get { return this.externalIdRO; } }
        public string ExternalLineNoRO { get { return this.externalLineNoRO; } }
        public string KitchenStatusRO { get { return this.kitchenStatusRO; } }
        public string KitchenStatusCodeRO { get { return this.kitchenStatusCodeRO; } }
        public decimal Quantity { get { return this.quantity; } }
        public string Description { get { return this.description; } }
        public List<UnknownModifier> UnknownModifiers { get { return this.unknownModifiers; } }
        public List<UnknownTextModifier> UnknownTextModifiers { get { return this.unknownTextModifiers; } }

        public UnknownDealLineItem(MenuDealLineItem dto) : base(dto.Id)
        {
            this.itemId = dto.ItemId;
            this.priceAdjustment = dto.PriceAdjustment;
            this.externalIdRO = dto.ExternalIdRO;
            this.externalLineNoRO = dto.ExternalLineNoRO;
            this.kitchenStatusRO = dto.KitchenStatusRO;
            this.kitchenStatusCodeRO = dto.KitchenStatusCodeRO;
            this.quantity = dto.Quantity;
            this.description = dto.Description;

            // NOTE: All modifiers under this unknown deal line item will also be unknown.
            this.unknownModifiers = new List<UnknownModifier>();
            this.unknownTextModifiers = new List<UnknownTextModifier>();
        }

        public UnknownDealLineItem()
        {
            this.unknownModifiers = new List<UnknownModifier>();
            this.unknownTextModifiers = new List<UnknownTextModifier>();
        }

        public UnknownDealLineItem Clone()
        {
            UnknownDealLineItem udli = this.MemberwiseClone() as UnknownDealLineItem;

            udli.unknownModifiers = new List<UnknownModifier>();
            udli.unknownTextModifiers = new List<UnknownTextModifier>();

            this.unknownModifiers.ForEach(x => udli.unknownModifiers.Add(x.Clone()));
            this.unknownTextModifiers.ForEach(x => udli.unknownTextModifiers.Add(x.Clone()));

            return udli;
        }

        private bool AnyUnsentModifiers
        {
            get
            {
                foreach (UnknownModifier unknownMod in UnknownModifiers)
                {
                    if (unknownMod.ShouldPromptSendToKitchen)
                        return true;
                }

                foreach (UnknownTextModifier unknownTextMod in UnknownTextModifiers)
                {
                    if (unknownTextMod.ShouldPromptSendToKitchen)
                        return true;
                }
                return false;
            }
        }
    }
}
