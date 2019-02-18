using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Requests
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class InventoryRequest : IDisposable
    {
        public InventoryRequest()
        {
            ItemId = "";
            VariantId = "";
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
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
    }

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
        /// <summary>
        /// Total inventory
        /// </summary>
        [DataMember]
        public decimal QtyInventory { get; set; }
        /// <summary>
        /// Quantity that has been sold through a POS transaction but not posted by a statement yet
        /// </summary>
        [DataMember]
        public decimal QtySoldNotPosted { get; set; }
        /// <summary>
        /// QtyInventory – QtySoldNotPosted – Quantity reserved by Customer Order
        /// </summary>
        [DataMember]
        public decimal QtyActualInventory { get; set; }
        /// <summary>
        /// Quantity on purchase order
        /// </summary>
        [DataMember]
        public decimal QtyExpectedStock { get; set; }
        /// <summary>
        /// Value calculated by the Replenishment module if the Replenishment module is active.
        /// If the Replenishment module is not active or the replenishment calculation for the item has not been run the return value is 0
        /// </summary>
        [DataMember]
        public decimal ReorderPoint { get; set; }
    }
}



