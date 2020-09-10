using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017"), KnownType(typeof(Recipe)), KnownType(typeof(MenuDeal)), KnownType(typeof(Product))]
    public class MenuItem : Entity, IAggregateRoot, IDisposable
    {
        private string name;

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Detail { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public decimal UnitPrice { get; set; }
        [DataMember]
        public string UnitOfMeasure { get; set; }

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

        public MenuItem(string id)
        {
            Id = id;
            DefaultMenuType = 1; // 1=Starter, 2=main course, MobileRestaurantMenuType
            Images = new List<ImageView>();
        }

        public MenuItem() : this(string.Empty)
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
                if (Images != null)
                    Images.Clear();
            }
        }

        #region Functions

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
