using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Requests
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class InventoryResponse : IDisposable
    {
        public InventoryResponse()
        {
            StoreId = "";
            ItemId = "";
            VariantId = "";
            BaseUnitOfMeasure = "";
            QtyInventory = 0M;
            QtySoldNotPosted = 0M;
            QtyActualInventory = 0M;
            QtyExpectedStock = 0M;
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
        public string StoreId { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string BaseUnitOfMeasure { get; set; }
        [DataMember]
        public decimal QtyInventory { get; set; }
        [DataMember]
        public decimal QtySoldNotPosted { get; set; }
        [DataMember]
        public decimal QtyActualInventory { get; set; }
        [DataMember]
        public decimal QtyExpectedStock { get; set; }
        [DataMember]
        public decimal ReorderPoint { get; set; }
    }
}

 

