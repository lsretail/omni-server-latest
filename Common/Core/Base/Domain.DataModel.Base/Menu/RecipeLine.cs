using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class RecipeLine : Entity, IEquatable<RecipeLine>
    {
        public RecipeLine()
        {
            ProductModifierGroups = new List<ProductModifierGroup>();
        }

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string ImageHint { get; set; }
        [DataMember]
        public List<ProductModifierGroup> ProductModifierGroups { get; set; }

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

        public RecipeLine Clone()
        {
            RecipeLine recipeLine = (RecipeLine)MemberwiseClone();
            recipeLine.ProductModifierGroups = new List<ProductModifierGroup>();
            ProductModifierGroups.ForEach(x => recipeLine.ProductModifierGroups.Add(x.Clone()));
            return recipeLine;
        }

        public bool Equals(RecipeLine recipeLine)
        {
            if (!base.Equals(recipeLine))
                return false;

            foreach (ProductModifierGroup productModifierGroup in ProductModifierGroups)
            {
                ProductModifierGroup matchProductModifierGroup = recipeLine.ProductModifierGroups.FirstOrDefault(x => x.Equals(productModifierGroup));
                if (matchProductModifierGroup == null)
                    return false;
            }
            return true;
        }
    }
}
