using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu.SpecialCase
{
    /// <summary>
    /// Unknown deal line, i.e. a deal line that we received from the BO, but couldn't find in the menus. Really just a data storage class.
    /// </summary>
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class UnknownDealLine : Entity
    {
        private List<UnknownDealLineItem> unknownDealLineItems;
        private List<UnknownModifier> unknownDealModifiers;

        public List<UnknownDealLineItem> UnknownDealLineItems { get { return this.unknownDealLineItems; } }
        public List<UnknownModifier> UnknownDealModifiers { get { return this.unknownDealModifiers; } }

        public UnknownDealLine(MenuDealLine dto) : base(dto.Id)
        {
            // Note: All items and modifiers belonging to this unknown deal line will also be unknown.
            this.unknownDealLineItems = new List<UnknownDealLineItem>();
            dto.DealLineItems.ForEach(x => this.unknownDealLineItems.Add(new UnknownDealLineItem(x)));
            this.unknownDealModifiers = new List<UnknownModifier>();
        }

        public UnknownDealLine()
        {
            this.unknownDealLineItems = new List<UnknownDealLineItem>();
            this.unknownDealModifiers = new List<UnknownModifier>();
        }

        public UnknownDealLine Clone()
        {
            UnknownDealLine udl = this.MemberwiseClone() as UnknownDealLine;

            udl.unknownDealLineItems = new List<UnknownDealLineItem>();
            udl.unknownDealModifiers = new List<UnknownModifier>();

            this.unknownDealLineItems.ForEach(x => udl.unknownDealLineItems.Add(x.Clone()));
            this.unknownDealModifiers.ForEach(x => udl.unknownDealModifiers.Add(x.Clone()));

            return udl;
        }

        public decimal UnknownDealLinePriceAdjustment
        {
            get
            {
                decimal price = 0m;
                price += UnknownDealModifierPriceAdjustment;

                UnknownDealLineItem item = UnknownDealLineItems.FirstOrDefault(x => x.Quantity != 0);
                if (item != null)
                {
                    price += item.PriceAdjustment;
                }
                return price;
            }
        }

        public decimal UnknownDealModifierPriceAdjustment
        {
            get
            {
                decimal price = 0m;
                foreach (UnknownModifier unknownDealModifier in unknownDealModifiers)
                {
                    // TODO Verify this, is this correct?
                    // Unknown modifiers will always increase the price, since they aren't part of the menu, and therefore not factored into the hosp item price
                    price += unknownDealModifier.Price * unknownDealModifier.Quantity;
                }
                return price;
            }
        }
    }
}
