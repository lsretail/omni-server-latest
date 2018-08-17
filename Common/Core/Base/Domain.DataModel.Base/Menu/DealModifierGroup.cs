using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class DealModifierGroup : ModifierGroup
    {
        public DealModifierGroup(string id) : base(id)
        {
            DealModifiers = new List<DealModifier>();
        }

        public DealModifierGroup() : this(string.Empty)
        {
        }

        [DataMember]
        public List<DealModifier> DealModifiers { get; set; }

        public override int Selected
        {
            get
            {
                int selected = 0;
                foreach (DealModifier dealModifier in DealModifiers)
                {
                    if (dealModifier.Quantity > dealModifier.OriginalQty)
                        selected += Convert.ToInt32(dealModifier.Quantity - dealModifier.OriginalQty);
                }
                return selected;
            }
        }

        public override void Reset()
        {
            foreach (DealModifier dealModifier in DealModifiers)
            {
                dealModifier.Quantity = dealModifier.OriginalQty;
            }
        }

        public DealModifierGroup Clone()
        {
            DealModifierGroup dealModifierGroup = (DealModifierGroup)MemberwiseClone();
            dealModifierGroup.DealModifiers = new List<DealModifier>();
            //DealModifiers.ForEach(x => dealModifierGroup.DealModifiers.Add(x.Clone()));

            foreach (DealModifier dealModifier in DealModifiers)
            {
                dealModifierGroup.DealModifiers.Add(dealModifier.Clone());
            }
            return dealModifierGroup;
        }


        /// <summary>
        /// Make sure that we satisfy the minimum selection restriction.
        /// Do this by adding quantity to modifiers (in order, one by one, no preference) until the restriction has been met.
        /// </summary>
        public override void SatisfyMinSelectionRestriction()
        {
            foreach (DealModifier dealMod in this.DealModifiers)
                dealMod.SatisfyMinQuantityRestriction();

            if (this.MinimumSelection > 0 && this.DealModifiers.Count > 0)
            {
                decimal currentSelection = 0;
                this.DealModifiers.ForEach(x => currentSelection += x.Quantity);

                if (currentSelection < this.MinimumSelection)
                {
                    foreach (DealModifier dealMod in this.DealModifiers)
                    {
                        decimal qtyMissing = this.MinimumSelection - currentSelection;
                        decimal availableQtyToAdd = dealMod.MaximumSelection - dealMod.Quantity;

                        dealMod.Quantity += Math.Min(availableQtyToAdd, qtyMissing);

                        currentSelection = 0;
                        this.DealModifiers.ForEach(x => currentSelection += x.Quantity);

                        if (currentSelection >= this.MinimumSelection)
                            break;
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DealModifiers != null)
                    DealModifiers.Clear();
            }
        }
    }
}
