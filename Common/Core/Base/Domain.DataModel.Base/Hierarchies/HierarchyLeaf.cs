using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Hierarchies
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class HierarchyLeaf : HierarchyPoint, IDisposable
    {
        private bool selected;

        public HierarchyLeaf() : this(string.Empty)
        {
        }

        public HierarchyLeaf(string id) : base(id)
        {
            Recipies = new List<ItemRecipe>();
            Modifiers = new List<ItemModifier>();
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
                if (Recipies != null)
                    Recipies.Clear();
                if (Modifiers != null)
                    Modifiers.Clear();
                Recipies = null;
                Modifiers = null;
            }
        }

        [DataMember]
        public HierarchyLeafType Type { get; set; }
        [DataMember]
        public string ItemUOM { get; set; }
        [DataMember]
        public List<UnitOfMeasure> ItemUOMs { get; set; }
        [DataMember]
        public int SortOrder { get; set; }

        [DataMember]
        public int DealLineNo { get; set; }
        [DataMember]
        public string DealLineCode { get; set; }
        [DataMember]
        public string DisplayPrompt { get; set; }

        [DataMember]
        public int LineNo { get; set; }
        [DataMember]
        public string ItemNo { get; set; }

        [DataMember]
        public int MinSelection { get; set; }
        [DataMember]
        public int MaxSelection { get; set; }
        [DataMember]
        public decimal AddedAmount { get; set; }
        [DataMember]
        public decimal Prepayment { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public decimal PriceWithTax { get; set; }
        [DataMember]
        public List<ItemModifier> Modifiers { get; set; }
        [DataMember]
        public List<ItemRecipe> Recipies { get; set; }

        [IgnoreDataMember]
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                NotifyPropertyChanged();
            }
        }

        [IgnoreDataMember]
        public bool HasModifiers => Modifiers != null && Modifiers.Count > 0;

        [IgnoreDataMember]
        public decimal CurrentSelection => Modifiers?.Sum(m => m.Selection) ?? 0;
    }
}
