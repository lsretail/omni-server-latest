using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Items
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ProductGroup : Entity, IDisposable
    {
        public ProductGroup(string id) : base(id)
        {
            Description = string.Empty;
            ItemCategoryId = string.Empty;
            Items = new List<LoyItem>();
            Images = new List<ImageView>();
        }

        public ProductGroup() : this(string.Empty)
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
                if (Items != null)
                    Items.Clear();
                if (Images != null)
                    Images.Clear();
            }
        }

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string ItemCategoryId { get; set; }
        [DataMember]
        public List<LoyItem> Items { get; set; }
        [DataMember]
        public List<ImageView> Images { get; set; }
    }
}
 