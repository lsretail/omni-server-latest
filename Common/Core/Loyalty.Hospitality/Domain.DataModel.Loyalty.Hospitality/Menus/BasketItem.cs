using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Menu;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Menus
{
    public class BasketItem : Entity
    {
        [DataMember]
        public MenuItem Item { get; set; }

        [DataMember]
        public decimal Quantity { get; set; }

        [DataMember]
        public bool Selected { get; set; }

        public BasketItem()
        {
        }

        public BasketItem(string id)
            : base(id)
        {
        }
    }
}
