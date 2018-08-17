using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Items
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ItemCategory : Entity, IDisposable
    {
        public ItemCategory(string id) : base(id)
        {
            Id = id;
            Description = string.Empty;
            ProductGroups = new List<ProductGroup>();
            Images = new List<ImageView>();
        }

        public ItemCategory() : this(string.Empty)
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
        public string Description { get; set; }
        [DataMember]
        public List<ProductGroup> ProductGroups { get; set; }
        [DataMember]
        public List<ImageView> Images { get; set; }
    }
}
 