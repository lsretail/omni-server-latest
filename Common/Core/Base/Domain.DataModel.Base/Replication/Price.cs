using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplPriceResponse : IDisposable
    {
        public ReplPriceResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Prices = new List<ReplPrice>();
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
                if (Prices != null)
                    Prices.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplPrice> Prices { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplPrice : IDisposable
    {
        public ReplPrice()
        {
            IsDeleted = false;
            StoreId = string.Empty;
            ItemId = string.Empty;
            UnitOfMeasure = string.Empty;
            VariantId = string.Empty;
            CustomerDiscountGroup = string.Empty;
            LoyaltySchemeCode = string.Empty;
            CurrencyCode = string.Empty;
            UnitPrice = 0M;
            ModifyDate = DateTime.Now;
            EndingDate = new DateTime(1900, 1, 1);
            StartingDate = new DateTime(1900, 1, 1);
            QtyPerUnitOfMeasure = 0M;
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
        public bool IsDeleted { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string UnitOfMeasure { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string CustomerDiscountGroup { get; set; }
        [DataMember]
        public string LoyaltySchemeCode { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public decimal UnitPrice { get; set; }
        [DataMember]
        public decimal UnitPriceInclVat { get; set; }
        [DataMember]
        public DateTime ModifyDate { get; set; }
        [DataMember]
        public int SaleType { get; set; }
        [DataMember]
        public int Priority { get; set; }
        [DataMember]
        public string SaleCode { get; set; }
        [DataMember]
        public string VATPostGroup { get; set; }
        [DataMember]
        public DateTime StartingDate { get; set; }
        [DataMember]
        public DateTime EndingDate { get; set; }
        [DataMember]
        public decimal MinimumQuantity { get; set; }
        [DataMember]
        public bool PriceInclVat { get; set; }
        [DataMember]
        public decimal QtyPerUnitOfMeasure { get; set; }
    }
}
