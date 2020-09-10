using System;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public abstract class Modifier : Entity, IDisposable
    {
        protected Modifier()
        {
        }

        protected Modifier(string id) : base(id)
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
        public int DisplayOrder { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public decimal Quantity { get; set; }

        [DataMember]
        public decimal OriginalQty { get; set; }

        [DataMember]
        public decimal MinimumSelection { get; set; }

        [DataMember]
        public decimal MaximumSelection { get; set; }

        [DataMember]
        public bool RequiredSelection { get; set; }

        [DataMember]
        public string UnitOfMeasure { get; set; }

        public int Selected
        {
            get
            {
                if (Quantity > OriginalQty)
                    return Decimal.ToInt32(Quantity - OriginalQty);

                return 0;
            }
        }

        public bool HasSelectionRestriction
        {
            get
            {
                return (MinimumSelection == MaximumSelection && MinimumSelection == 0) == false;
            }
        }

        /// <summary>
        /// Has this modifier been messed with by the user?
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        public bool Active
        {
            get
            {
                if (Quantity != OriginalQty)
                    return true;
                else
                    return false;
            }
        }

        #region Functions

        public void SatisfyMinQuantityRestriction()
        {
            if (this.MinimumSelection > 0 && this.Quantity < this.MinimumSelection)
            {
                decimal qtyMissing = this.MinimumSelection - this.Quantity;
                decimal availableQtyToAdd = this.MaximumSelection - this.Quantity;

                this.Quantity += Math.Min(qtyMissing, availableQtyToAdd);
            }
        }

        /// <summary>
        /// Check if the user has to be asked about this modifier. Takes the modifier group into account.
        /// All modifiers in a group that requires selection also require selection.
        /// </summary>
        /// <returns><c>true</c> If the user has to be asked about this modifier; otherwise, <c>false</c>.</returns>
        /// <param name="group">Group.</param>
        public bool IsSelectionRequired(ModifierGroup group)
        {
            if (group.RequiredSelection)
                return true;

            if (RequiredSelection)
                return true;

            return false;
        }

        public ModifierType GetModifierType(ModifierGroup group)
        {
            if (group.MinimumSelection == 1 && group.MaximumSelection == 1)
            {
                return ModifierType.Radio;
            }
            else if (group.MinimumSelection == 0 && group.MaximumSelection == 1)
            {
                return ModifierType.RadioAllowZero;
            }
            else if (MinimumSelection == 0 && MaximumSelection == 1)
            {
                return ModifierType.Checkbox;
            }
            else if (MinimumSelection != MaximumSelection)
            {
                return ModifierType.Counter;
            }
            return ModifierType.None;
        }

        public virtual bool Equals(Modifier modifier)
        {
            if (!base.Equals(modifier))
                return false;

            if (modifier.Quantity != Quantity)
                return false;

            return true;
        }

        public virtual Modifier Clone()
        {
            Modifier clonedModifier = this.MemberwiseClone() as Modifier;
            if (this.Item != null)
                clonedModifier.Item = this.Item.Clone();
            return clonedModifier;
        }

        #endregion
    }

    public enum ModifierType
    {
        None = 0,
        Counter = 1,
        Checkbox = 2,
        Radio = 3,
        RadioAllowZero = 4
    }
}
