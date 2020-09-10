using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class MobileMenu : Entity, IDisposable
    {
        public MobileMenu(string id) : base(id)
        {
            Currency = new Currency();
            MenuNodes = new List<Menu>();

            Items = new List<IngredientItem>();
            Products = new List<Product>();
            Recipes = new List<Recipe>();
            Deals = new List<MenuDeal>();

            ProductModifierGroups = new List<ProductModifierGroup>();
            DealModifierGroups = new List<DealModifierGroup>();
        }

        public MobileMenu() : this(string.Empty)
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
                if (Currency != null)
                    Currency.Dispose();
                if (MenuNodes != null)
                    MenuNodes.Clear();
                if (Items != null)
                    Items.Clear();
                if (Products != null)
                    Products.Clear();
                if (Recipes != null)
                    Recipes.Clear();
                if (Deals != null)
                    Deals.Clear();
                if (ProductModifierGroups != null)
                    ProductModifierGroups.Clear();
                if (DealModifierGroups != null)
                    DealModifierGroups.Clear();
            }
        }

        [DataMember]
        public string Version { get; set; }
        [DataMember]
        public List<Menu> MenuNodes { get; set; }
        [DataMember]
        public List<Product> Products { get; set; }
        [DataMember]
        public List<Recipe> Recipes { get; set; }
        [DataMember]
        public List<MenuDeal> Deals { get; set; }
        [DataMember]
        public List<IngredientItem> Items { get; set; }
        [DataMember]
        public List<ProductModifierGroup> ProductModifierGroups { get; set; }
        [DataMember]
        public List<DealModifierGroup> DealModifierGroups { get; set; }
        [DataMember]
        public Currency Currency { get; set; }
    }
}
 