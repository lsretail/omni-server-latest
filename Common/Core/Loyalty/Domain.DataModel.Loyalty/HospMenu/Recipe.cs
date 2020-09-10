using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class Recipe : MenuItem
    {
        public Recipe(string id) : base(id)
        {
            Images = new List<ImageView>();
            Ingredients = new List<Ingredient>();
            RecipeLines = new List<RecipeLine>();
            ProductModifierGroupIds = new List<string>();
            ProductModifierGroups = new List<ProductModifierGroup>();
            Detail = string.Empty;
        }

        public Recipe() : this(string.Empty)
        {
        }

        [DataMember]
        public List<Ingredient> Ingredients { get; set; }
        [DataMember]
        public List<string> ProductModifierGroupIds { get; set; }

        [IgnoreDataMember]
        public List<ProductModifierGroup> ProductModifierGroups { get; set; }

        public List<RecipeLine> RecipeLines { get; set; }

        public override MenuItem Clone()
        {
            Recipe item = (Recipe)base.Clone();
            item.Ingredients = new List<Ingredient>();
            item.RecipeLines = new List<RecipeLine>();
            item.ProductModifierGroups = new List<ProductModifierGroup>();

            Ingredients.ForEach(x => item.Ingredients.Add(x.Clone()));
            RecipeLines.ForEach(x => item.RecipeLines.Add(x.Clone()));
            ProductModifierGroups.ForEach(x => item.ProductModifierGroups.Add(x.Clone()));
            return item;
        }

        public override void SatisfyModifierGroupsMinSelectionRestrictions()
        {
            foreach (RecipeLine rl in this.RecipeLines)
            {
                foreach (ProductModifierGroup pmg in rl.ProductModifierGroups)
                    pmg.SatisfyMinSelectionRestriction();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Images?.Clear();
                Ingredients?.Clear();
                ProductModifierGroups?.Clear();
                ProductModifierGroupIds?.Clear();
            }
            base.Dispose(disposing);
        }
    }
}
 