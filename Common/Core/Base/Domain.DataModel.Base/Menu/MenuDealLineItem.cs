using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class MenuDealLineItem : Entity, IDisposable
    {
        public MenuDealLineItem(string id) : base(id)
        {
        }

        public MenuDealLineItem() : this(string.Empty)
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
            }
        }

        [DataMember]
        public decimal PriceAdjustment { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string ItemId { get; set; }

        [DataMember]
        public decimal Quantity { get; set; }

        [IgnoreDataMember]
        public MenuItem MenuItem { get; set; }

        public string ExternalIdRO { get; set; }
        public string ExternalLineNoRO { get; set; }
        public string KitchenStatusRO { get; set; }
        public string KitchenStatusCodeRO { get; set; }

        public MenuDealLineItem Clone()
        {
            MenuDealLineItem dealLineItem = (MenuDealLineItem)MemberwiseClone();
            if (MenuItem != null)
                dealLineItem.MenuItem = MenuItem.Clone();

            return dealLineItem;
        }
    }
}

