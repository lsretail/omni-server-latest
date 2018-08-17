using System;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public abstract class ModifierGroup : Entity, IDisposable
    {
        [DataMember]
        public string DefaultItemId { get; set; }

        // Note:
        // This should apply: maxSelection >= minSelection
        // If maxSelection == 1 then we may select only one ProductModifier from this group
        // If maxSelection == minSelection == 1 then we must select one and exactly one ProductModifier from this group
        [DataMember]
        public decimal MaximumSelection { get; set; }

        [DataMember]
        public decimal MinimumSelection { get; set; }

        [DataMember]
        public bool RequiredSelection { get; set; }

        public bool HasSelectionRestriction
        {
            get
            {
                return !(MinimumSelection == MaximumSelection && MinimumSelection == 0);
            }
        }

        public decimal NewQty(Modifier modifier, decimal newQty)
        {
            if (newQty > modifier.MaximumSelection)
                newQty = modifier.MaximumSelection;

            if (newQty < modifier.MinimumSelection)
                newQty = modifier.MinimumSelection;

            decimal selection = Selected - modifier.Selected + newQty;

            if (MaximumSelection > 0 && selection > MaximumSelection)
                newQty = MaximumSelection - Selected + modifier.Selected;

            if (selection < MinimumSelection)
                newQty = MinimumSelection - Selected + modifier.Selected;

            return newQty;
        }

        [DataMember]
        public string Description { get; set; }

        private string selectedId;
        public string SelectedId
        {
            get
            {
                if (string.IsNullOrEmpty(selectedId))
                    return DefaultItemId;
                return selectedId;
            }
            set { selectedId = value; }
        }

        protected ModifierGroup(string id) : base(id)
        {
            DefaultItemId = "";
            MaximumSelection = 0;
            MinimumSelection = 0;
            Description = "";
            selectedId = "";
        }

        protected ModifierGroup() : this(string.Empty)
        {
        }

        public abstract void Reset();
        public abstract int Selected { get; }

        public virtual bool Equals(ModifierGroup modifierGroup)
        {
            return base.Equals(modifierGroup);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void SatisfyMinSelectionRestriction();

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
