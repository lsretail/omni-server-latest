using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017"), KnownType(typeof(Recipe)), KnownType(typeof(MenuDeal)), KnownType(typeof(Product))]
    public class MenuItem : Item
    {
        private string name;

        [DataMember]
        public int DefaultMenuType { get; set; }

        [DataMember]
        public List<ImageView> Images { get; set; }

        [DataMember]
        public Validation Validation { get; set; }

        [DataMember]
        public string MenuId { get; set; }

        [DataMember]
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    return Description;
                }
                return name;
            }
            set { name = value; }
        }

        public MenuItem(string id) : base(id)
        {
            DefaultMenuType = 1; // 1=Starter, 2=main course, MobileRestaurantMenuType
            Images = new List<ImageView>();
        }

        public MenuItem() : this(string.Empty)
        {
        }

        #region Functions

        public virtual Money GetFullPrice()
        {
            return new Money(FullPrice, this.Price.Currency);
        }

        public virtual decimal FullPrice
        {
            get { return UnitPrice; }
        }

        public virtual bool AnyRequiredModifiers
        {
            get { return false; }
        }

        public virtual bool AnyModifiers
        {
            get { return false; }
        }

        public virtual bool AnyUnsentModifiers
        {
            get { return false; }
        }

        public virtual bool AllRequiredModifiers
        {
            get { return true; }
        }

        public virtual MenuItem Clone()
        {
            MenuItem item = (MenuItem)MemberwiseClone();

            item.Images = new List<ImageView>();
            Images.ForEach(x => item.Images.Add(new ImageView(x.Id) 
            { 
                AvgColor = x.AvgColor, 
                DisplayOrder = x.DisplayOrder, 
                Format = x.Format, 
                Image = x.Image, 
                ImgSize = x.ImgSize, 
                Location = x.Location, 
                StreamURL = x.StreamURL, 
                LocationType = x.LocationType
            }));

            item.DefaultMenuType = this.DefaultMenuType;
            return item;
        }

        public virtual void SatisfyModifierGroupsMinSelectionRestrictions() { }

        #endregion
    }
}
