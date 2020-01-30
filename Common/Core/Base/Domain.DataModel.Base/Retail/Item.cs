using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class Item : Entity, IAggregateRoot, IDisposable
    {
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Detail { get; set; }
        [DataMember]
        public Money Price { get; set; }
        [DataMember]
        public decimal UnitPrice { get; set; }
        [DataMember]
        public UnitOfMeasure UnitOfMeasure { get; set; }
        [DataMember]
        public bool DiscountAllowed { get; set; }
        [DataMember]
        public List<ItemUnitOfMeasure> ItemUnitOfMeasures { get; set; }

        public Item()
            : this(null)
        {
        }

        public Item(string id)
            : base(id)
        {
            Description = string.Empty;
            Detail = string.Empty;
			Price = new Money();
            UnitOfMeasure = null;
			DiscountAllowed = true;
            ItemUnitOfMeasures = new List<ItemUnitOfMeasure>();
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
    }
}
