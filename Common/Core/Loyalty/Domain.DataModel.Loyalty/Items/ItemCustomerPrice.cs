using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Items
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ItemCustomerPrice : Entity, IDisposable
    {
        public ItemCustomerPrice(string id) : base(id) 
        {
        }

        public ItemCustomerPrice() : this(string.Empty)
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
        public int LineNo { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public decimal DiscountPercent { get; set; }
    }
}
