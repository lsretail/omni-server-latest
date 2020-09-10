using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    /// <summary>
    /// Response from Item Replication
    /// </summary>
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplItemResponse : IDisposable
    {
        public ReplItemResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Items = new List<ReplItem>();
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
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplItem> Items { get; set; }
    }

    /// <summary>
    /// Item replication object
    /// </summary>
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplItem : IDisposable
    {
        public ReplItem(string id)
        {
            Id = id;
            IsDeleted = false;
            DateBlocked = new DateTime(1900, 1, 1);
            DateToActivateItem = new DateTime(1900, 1, 1);
            Description = string.Empty;
            Details = string.Empty;
            ProductGroupId = string.Empty;
            BaseUnitOfMeasure = string.Empty;
            SalseUnitOfMeasure = string.Empty;
            PurchUnitOfMeasure = string.Empty;
            TaxItemGroupId = string.Empty;
            VendorId = string.Empty;
            VendorItemId = string.Empty;
            SeasonCode = string.Empty;
            ItemCategoryCode = string.Empty;
            ItemFamilyCode = string.Empty;
            MustKeyInComment = 0;
        }

        public ReplItem() : this(string.Empty)
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
        public string Id { get; set; }
        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Details { get; set; }
        [DataMember]
        public string ProductGroupId { get; set; }
        [DataMember]
        public string BaseUnitOfMeasure { get; set; }
        [DataMember]
        public string SalseUnitOfMeasure { get; set; }
        [DataMember]
        public string PurchUnitOfMeasure { get; set; }
        [DataMember]
        public string TaxItemGroupId { get; set; }
        [DataMember]
        public int ZeroPriceValId { get; set; }
        [DataMember]
        public int NoDiscountAllowed { get; set; }
        [DataMember]
        public int KeyingInPrice { get; set; }
        [DataMember]
        public int KeyingInQty { get; set; }
        [DataMember]
        public int ScaleItem { get; set; }
        [DataMember]
        public int? MustKeyInComment { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateBlocked { get; set; }
        [DataMember]
        public int Blocked { get; set; }
        /// <summary>
        /// Block Sale on POS from Item Status
        /// </summary>
        [DataMember]
        public int BlockedOnPos { get; set; }
        /// <summary>
        /// Block Discount from Item Status
        /// </summary>
        [DataMember]
        public int BlockDiscount { get; set; }
        /// <summary>
        /// Block Manual Price Change from Item Status
        /// </summary>
        [DataMember]
        public int BlockManualPriceChange { get; set; }
        /// <summary>
        /// Block Negative Adjustment from Item Status
        /// </summary>
        [DataMember]
        public int BlockNegativeAdjustment { get; set; }
        /// <summary>
        /// Block Positive Adjustment from Item Status
        /// </summary>
        [DataMember]
        public int BlockPositiveAdjustment { get; set; }
        /// <summary>
        /// Block Purchase Return from Item Status
        /// </summary>
        [DataMember]
        public int BlockPurchaseReturn { get; set; }
        /// <summary>
        /// Status field from Distribution setup on Item Page (Store Distribution) 0=Active
        /// </summary>
        [DataMember]
        public int BlockDistribution { get; set; }
        /// <summary>
        /// Blocked from ECom Web Site
        /// </summary>
        [DataMember]
        public int BlockedOnECom { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateToActivateItem { get; set; }
        [DataMember]
        public int CrossSellingExists { get; set; }
        [DataMember]
        public decimal UnitPrice { get; set; }
        [DataMember]
        public string VendorId { get; set; }
        [DataMember]
        public string VendorItemId { get; set; }

        [DataMember]
        public decimal GrossWeight { get; set; }
        [DataMember]
        public string SeasonCode { get; set; }
        [DataMember]
        public string ItemCategoryCode { get; set; }
        [DataMember]
        public string ItemFamilyCode { get; set; }
        [DataMember]
        public decimal UnitsPerParcel { get; set; }
        [DataMember]
        public decimal UnitVolume { get; set; }
    }
}
