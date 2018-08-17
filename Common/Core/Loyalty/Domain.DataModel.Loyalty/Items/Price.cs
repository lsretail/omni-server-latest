using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Items
{
    /// <summary>
    /// Price Class
    /// </summary>
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class Price : IDisposable
    {
        public Price(string itemId, string variantId, string uomId)
        {
            ItemId = itemId;
            VariantId = variantId;
            UomId = uomId;
            Amount = string.Empty;
            Amt = 0M;
        }

        public Price()
            : this(string.Empty, string.Empty, string.Empty)
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
        public string ItemId { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string UomId { get; set; }
        /// <summary>
        /// amount as string, with currency code
        /// </summary>
        [DataMember]
        public string Amount { get; set; }  //amount to display
        /// <summary>
        /// amount as decimal
        /// </summary>
        [DataMember]
        public decimal Amt { get; set; }
    }
}
 
