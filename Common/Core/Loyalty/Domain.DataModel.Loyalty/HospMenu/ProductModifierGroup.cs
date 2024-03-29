﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ProductModifierGroup : ModifierGroup
    {
        public ProductModifierGroup(string id) : base(id)
        {
            ProductModifiers = new List<ProductModifier>();
        }

        public ProductModifierGroup() : this(string.Empty)
        {
        }

        [DataMember]
        public List<ProductModifier> ProductModifiers { get; set; }

        public override int Selected
        {
            get
            {
                int selected = 0;
                foreach (ProductModifier productModifier in ProductModifiers)
                {
                    if (productModifier.Quantity > productModifier.OriginalQty)
                        selected += Convert.ToInt32(productModifier.Quantity - productModifier.OriginalQty);
                }
                return selected;
            }
        }

        public override void Reset()
        {
            foreach (ProductModifier productModifier in ProductModifiers)
            {
                productModifier.Quantity = productModifier.OriginalQty;
            }
        }

        public ProductModifierGroup Clone()
        {
            ProductModifierGroup productModifierGroup = (ProductModifierGroup)MemberwiseClone();
            productModifierGroup.ProductModifiers = new List<ProductModifier>();
            foreach (ProductModifier productModifier in ProductModifiers)
            {
                productModifierGroup.ProductModifiers.Add(productModifier.Clone());
            }
            return productModifierGroup;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ProductModifiers != null)
                    ProductModifiers.Clear();
            }
        }

        /// <summary>
        /// Make sure that we satisfy the minimum selection restriction.
        /// Do this by adding quantity to productmodifiers (in order, one by one, no preference) until the restriction has been met.
        /// If the restriction has not been met after going through all the productmodifiers, increasing their quantity up to their max quantity, 
        /// </summary>
        public override void SatisfyMinSelectionRestriction()
        {
            foreach (ProductModifier prodMod in this.ProductModifiers)
                prodMod.SatisfyMinQuantityRestriction();

            if (this.MinimumSelection > 0 && this.ProductModifiers.Count > 0)
            {
                decimal currentSelection = 0;
                this.ProductModifiers.ForEach(x => currentSelection += x.Quantity);

                if (currentSelection < this.MinimumSelection)
                {
                    foreach (ProductModifier prodMod in this.ProductModifiers)
                    {
                        decimal qtyMissing = this.MinimumSelection - currentSelection;
                        decimal availableQtyToAdd = prodMod.MaximumSelection - prodMod.Quantity;

                        prodMod.Quantity += Math.Min(availableQtyToAdd, qtyMissing);

                        currentSelection = 0;
                        this.ProductModifiers.ForEach(x => currentSelection += x.Quantity);

                        if (currentSelection >= this.MinimumSelection)
                            break;
                    }

                    if (currentSelection >= this.MinimumSelection)
                        return;
                }
            }
        }
    }
}
