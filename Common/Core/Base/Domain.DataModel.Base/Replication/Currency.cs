using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplCurrencyResponse : IDisposable
    {
        public ReplCurrencyResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Currencies = new List<ReplCurrency>();
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
                if (Currencies != null)
                    Currencies.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplCurrency> Currencies { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplCurrency : IDisposable
    {
        public ReplCurrency()
        {
            IsDeleted = false;
            CurrencyCode = string.Empty;
            Description = string.Empty;
            Symbol = string.Empty;
            CurrencyPrefix = string.Empty;
            CurrencySuffix = string.Empty;
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
        public string CurrencyCode { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int RoundOfTypeSales { get; set; }
        [DataMember]
        public decimal RoundOfSales { get; set; }
        [DataMember]
        public int RoundOfTypeAmount { get; set; }
        [DataMember]
        public decimal RoundOfAmount { get; set; }
        [DataMember]
        public string Symbol { get; set; }
        [DataMember]
        public string CurrencyPrefix { get; set; }
        [DataMember]
        public string CurrencySuffix { get; set; }
    }
}
