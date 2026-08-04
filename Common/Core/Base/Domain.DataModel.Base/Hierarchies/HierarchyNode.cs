using LSRetail.Omni.Domain.DataModel.Base.Retail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Hierarchies
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class HierarchyNode : HierarchyPoint, IDisposable
    {
        public HierarchyNode() : this(string.Empty)
        {
        }

        public HierarchyNode(string id) : base(id)
        {
            Nodes = new List<HierarchyNode>();
            Leafs = new List<HierarchyLeaf>();
            Recipies = new List<ItemRecipe>();
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
                if (Nodes != null)
                    Nodes.Clear();
                if (Leafs != null)
                    Leafs.Clear();
                if (Recipies != null)
                    Recipies.Clear();
                Nodes = null;
                Leafs = null;
                Recipies = null;
            }
        }

        
        [DataMember]
        public int Indentation { get; set; }
        [DataMember]
        public int PresentationOrder { get; set; }
        [DataMember]
        public int ChildrenOrder { get; set; }
        [DataMember]
        public int LineNo { get; set; }
        [DataMember]
        public List<HierarchyNode> Nodes { get; set; }
        [DataMember]
        public List<HierarchyLeaf> Leafs { get; set; }
        [DataMember]
        public List<ItemRecipe> Recipies { get; set; }

        [DataMember]
        public HierarchyDealType Type { get; set; }
        [DataMember]
        public string No { get; set; }
        [DataMember]
        public string DisplayPrompt { get; set; }

        [DataMember]
        public int MinSelection { get; set; }
        [DataMember]
        public int MaxSelection { get; set; }
        /// <summary>
        /// Amount to add to a deal price
        /// </summary>
        [DataMember]
        public decimal AddedAmount { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        /// <summary>
        /// Used to group deals together, f.ex. if Medium Fries goes with Medium Soda.
        /// </summary>
        [DataMember]
        public int DealModSizeGroupIndex { get; set; }

        [IgnoreDataMember]
        public List<HierarchyPoint> Points 
        {
            get
            {
                var list = new List<HierarchyPoint>(Nodes.OrderBy(x => x.PresentationOrder));
                list.AddRange(Leafs.OrderBy(x => x.SortOrder));

                return list;
            }
        }

        public bool HasModifierGroups => (Type == HierarchyDealType.Recipe || Type == HierarchyDealType.Item) && Leafs.Any();

        public List<ItemModifier> ModifierGroups
        {
            get
            {
                var modifiers = new List<ItemModifier>();

                foreach (var hierarchyLeaf in Leafs)
                {
                    modifiers.AddRange(hierarchyLeaf.Modifiers);
                }

                return modifiers;
            }
        }

        public List<object> FlattenedDeal 
        {
            get
            {
                var list = new List<object>();

                if (Nodes.Any())
                {
                    foreach (var hierarchyNode in Nodes)
                    {
                        list.Add(hierarchyNode.Description);

                        foreach (var hierarchyNodeLeaf in hierarchyNode.Leafs)
                        {
                            if (hierarchyNode.Type == HierarchyDealType.Modifier)
                            {
                                list.Add(hierarchyNodeLeaf);
                            }
                            else if (hierarchyNode.Type == HierarchyDealType.Item)
                            {
                                list.Add(hierarchyNodeLeaf.Description);

                                list.AddRange(hierarchyNodeLeaf.Modifiers);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var nodeLeaf in Leafs)
                    {
                        list.Add(nodeLeaf.Description);

                        list.AddRange(nodeLeaf.Modifiers);
                    }
                }

                return list;
            }
        }
    }
}
