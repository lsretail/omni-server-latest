using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Menu.SpecialCase;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class Product : MenuItem
    {
        public Product(string id) : base(id)
        {
            UnitOfMeasures = new List<UnitOfMeasure>();
            ProductModifierGroups = new List<ProductModifierGroup>();
            UnknownModifiers = new List<UnknownModifier>();
            UnknownTextModifiers = new List<UnknownTextModifier>();
        }

        public Product() : this(string.Empty)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Images != null)
                    Images.Clear();
                if (UnitOfMeasures != null)
                    UnitOfMeasures.Clear();
                if (ProductModifierGroups != null)
                    ProductModifierGroups.Clear();
                if (UnknownModifiers != null)
                    UnknownModifiers.Clear();
                if (UnknownTextModifiers != null)
                    UnknownTextModifiers.Clear();
            }
            base.Dispose(disposing);
        }

        [DataMember]
        public string DefaultUnitOfMeasure { get; set; }
        [DataMember]
        public List<UnitOfMeasure> UnitOfMeasures { get; set; }
        [DataMember]
        public List<ProductModifierGroup> ProductModifierGroups { get; set; }

        /// Unknown modifiers are modifiers that we get from the BO when retrieving a transaction,
        /// but aren't part of the menu that the hosp item came from,
        /// and so aren't part of the hosp item's 'normal' set of modifiers (those that are defined in the menu it came from).
        [DataMember]
        public List<UnknownModifier> UnknownModifiers { get; private set; }
        [DataMember]
        public List<UnknownTextModifier> UnknownTextModifiers { get; private set; }

        public override decimal FullPrice
        {
            get
            {
                decimal price = base.FullPrice;
                price += ProductModifierPriceAdjustment;
                price += UnknownModifierPriceAdjustment;
                return price;
            }
        }

        public decimal ProductModifierPriceAdjustment
        {
            get
            {
                decimal price = 0m;
                foreach (ProductModifierGroup productModifierGroup in ProductModifierGroups)
                {
                    foreach (ProductModifier productModifier in productModifierGroup.ProductModifiers)
                    {
                        price += productModifier.Price * (productModifier.Quantity - productModifier.OriginalQty);
                    }
                }
                return price;
            }
        }

        public decimal UnknownModifierPriceAdjustment
        {
            get
            {
                decimal price = 0m;
                foreach (UnknownModifier unknownModifier in UnknownModifiers)
                {
                    // TODO Verify this, is this correct?
                    // Unknown modifiers will always increase the price, since they aren't part of the menu, and therefore not factored into the hosp item price
                    price += unknownModifier.Price * unknownModifier.Quantity;
                }
                return price;
            }
        }

        public override bool AnyRequiredModifiers
        {
            get
            {
                foreach (ProductModifierGroup productModifierGroup in ProductModifierGroups)
                {
                    if (productModifierGroup.RequiredSelection)
                    {
                        return true;
                    }
                }
                return base.AnyRequiredModifiers;
            }
        }

        public override bool AnyModifiers
        {
            get
            {
                foreach (ProductModifierGroup productModifierGroup in ProductModifierGroups)
                {
                    if (productModifierGroup.ProductModifiers.Count > 0)
                    {
                        return true;
                    }

                    if (productModifierGroup.TextModifiers.Count > 0)
                    {
                        return true;
                    }
                }
                return base.AnyModifiers;
            }
        }

        public override bool AllRequiredModifiers
        {
            get
            {
                foreach (ProductModifierGroup productModifierGroup in ProductModifierGroups)
                {
                    if (productModifierGroup.RequiredSelection == false)
                    {
                        return false;
                    }
                }
                return base.AllRequiredModifiers;
            }
        }

        public override bool AnyUnsentModifiers
        {
            get
            {
                foreach (ProductModifierGroup productModifierGroup in ProductModifierGroups)
                {
                    foreach (ProductModifier productModifier in productModifierGroup.ProductModifiers)
                    {
                        if (productModifier.ShouldPromptSendToKitchen)
                            return true;
                    }

                    foreach (TextModifier textModifier in productModifierGroup.TextModifiers)
                    {
                        if (textModifier.ShouldPromptSendToKitchen)
                            return true;
                    }
                }

                foreach (UnknownModifier unknownModifier in UnknownModifiers)
                {
                    if (unknownModifier.ShouldPromptSendToKitchen)
                        return true;
                }

                foreach (UnknownTextModifier unknownTextModifier in UnknownTextModifiers)
                {
                    if (unknownTextModifier.ShouldPromptSendToKitchen)
                        return true;
                }
                return base.AnyUnsentModifiers;
            }
        }

        #region Functions

        public override Money GetFullPrice()
        {
            return new Money(FullPrice, this.Price.Currency);
        }

        public override MenuItem Clone()
        {
            Product product = (Product)base.Clone();

            product.UnitOfMeasures = new List<UnitOfMeasure>();
            product.ProductModifierGroups = new List<ProductModifierGroup>();
            product.UnknownModifiers = new List<UnknownModifier>();
            product.UnknownTextModifiers = new List<UnknownTextModifier>();

            this.UnitOfMeasures.ForEach(x => product.UnitOfMeasures.Add(x.Clone()));
            this.ProductModifierGroups.ForEach(x => product.ProductModifierGroups.Add(x.Clone()));
            this.UnknownModifiers.ForEach(x => product.UnknownModifiers.Add(x.Clone()));
            this.UnknownTextModifiers.ForEach(x => product.UnknownTextModifiers.Add(x.Clone()));

            return product;
        }

        public override void SatisfyModifierGroupsMinSelectionRestrictions()
        {
            foreach (ProductModifierGroup pmg in this.ProductModifierGroups)
                pmg.SatisfyMinSelectionRestriction();
        }

        #endregion
    }
}
