using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class IngredientItem : Entity
    {
        public IngredientItem(string id) : base(id)
        {
            DefaultMenuType = 1; // 1=Starter, 2=main course, MobileRestaurantMenuType
            Images = new List<ImageView>();
        }

        public IngredientItem() : this(string.Empty)
        {
        }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Details { get; set; }

        [DataMember]
        public int DefaultMenuType { get; set; }

        [DataMember]
        public string UnitOfMeasure { get; set; }

        [DataMember]
        public List<ImageView> Images { get; set; }

        public MenuItem Clone()
        {
            MenuItem item = (MenuItem)MemberwiseClone();
            foreach (ImageView img in Images)
            {
                item.Images.Add(img);
            }
            return item;
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
    }
}
