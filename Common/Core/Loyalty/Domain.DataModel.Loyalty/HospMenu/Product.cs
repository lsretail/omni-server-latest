using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class Product : MenuItem
    {
        public Product(string id) : base(id)
        {
            UnitOfMeasures = new List<UnitOfMeasure>();
            ProductModifierGroups = new List<ProductModifierGroup>();
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
            }
            base.Dispose(disposing);
        }

        [DataMember]
        public string DefaultUnitOfMeasure { get; set; }
        [DataMember]
        public List<UnitOfMeasure> UnitOfMeasures { get; set; }
        [DataMember]
        public List<ProductModifierGroup> ProductModifierGroups { get; set; }

        public override decimal FullPrice
        {
            get
            {
                decimal price = base.FullPrice;
                price += ProductModifierPriceAdjustment;
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

        #region Functions

        public override MenuItem Clone()
        {
            Product product = (Product)base.Clone();

            product.UnitOfMeasures = new List<UnitOfMeasure>();
            product.ProductModifierGroups = new List<ProductModifierGroup>();

            this.UnitOfMeasures.ForEach(x => product.UnitOfMeasures.Add(x.Clone()));
            this.ProductModifierGroups.ForEach(x => product.ProductModifierGroups.Add(x.Clone()));

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
