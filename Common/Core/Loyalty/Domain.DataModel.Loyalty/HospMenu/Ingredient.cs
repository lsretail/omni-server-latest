using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class Ingredient : Entity, IDisposable
    {
        public Ingredient(string id) : base(id)
        {
        }

        public Ingredient() : this(string.Empty)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        [DataMember]
        public MenuItem Item { get; set; }

        [DataMember]
        public decimal Quantity { get; set; }

        [DataMember]
        public decimal OriginalQuantity { get; set; }

        [DataMember]
        public decimal MinimumQuantity { get; set; }

        [DataMember]
        public decimal MaximumQuantity { get; set; }

        [DataMember]
        public string UnitOfMeasure { get; set; }

        [DataMember]
        public int DisplayOrder { get; set; }

        [DataMember]
        public decimal PriceReduction { get; set; }

        [DataMember]
        public bool PriceReductionOnExclusion { get; set; }

        #region Functions

        /// <summary>
        /// Has the user changed the quantity of this ingredient?
        /// </summary>
        /// <value><c>true</c> if quantity changed; otherwise, <c>false</c>.</value>
        public bool QuantityChanged
        {
            get
            {
                return (Quantity != OriginalQuantity) ? true : false;
            }
        }

        public int Selected
        {
            get
            {
                if (GetModifierType() == ModifierType.Checkbox)
                {
                    return (Quantity == 1) ? 1 : 0;
                }

                //if (Qty > OriginalQty)
                return Decimal.ToInt32(Quantity - OriginalQuantity);
            }
        }

        public decimal PriceAdjustment
        {
            get
            {
                if (PriceReductionOnExclusion && IsExcluded)
                    return -PriceReduction;
                else
                    return 0;
            }
        }

        public bool IsExcluded
        {
            get
            {
                if (OriginalQuantity > 0 && Quantity == 0)
                    return true;

                return false;
            }
        }

        public decimal NewQty(decimal newQty)
        {
            if (newQty > MaximumQuantity)
                newQty = MaximumQuantity;

            if (newQty < MinimumQuantity)
                newQty = MinimumQuantity;

            return newQty;
        }

        public Ingredient Clone()
        {
            Ingredient ingredient = (Ingredient)MemberwiseClone();
            ingredient.Item = (Item != null) ? Item.Clone() : null;
            return ingredient;
        }

        public bool Equals(Ingredient ingredient)
        {
            if (!base.Equals(ingredient))
                return false;

            if (ingredient.Quantity != Quantity)
                return false;

            return true;
        }

        public ModifierType GetModifierType()
        {
            if (MinimumQuantity == 0 && MaximumQuantity == 1)
            {
                return ModifierType.Checkbox;
            }
            else if (MinimumQuantity != MaximumQuantity)
            {
                return ModifierType.Counter;
            }
            return ModifierType.None;
        }

        #endregion
    }
}
