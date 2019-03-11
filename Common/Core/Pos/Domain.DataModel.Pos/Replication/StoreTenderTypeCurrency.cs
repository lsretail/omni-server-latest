using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]

    public class ReplStoreTenderTypeCurrencyResponse : IDisposable
    {
        public ReplStoreTenderTypeCurrencyResponse()
        {
            LastKey = "";
            MaxKey = "";
            RecordsRemaining = 0;
            StoreTenderTypeCurrencies = new List<ReplStoreTenderTypeCurrency>();
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
                if (StoreTenderTypeCurrencies != null)
                    StoreTenderTypeCurrencies.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplStoreTenderTypeCurrency> StoreTenderTypeCurrencies { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ReplStoreTenderTypeCurrency : IDisposable
    {
        public ReplStoreTenderTypeCurrency()
        {
            StoreID = string.Empty;
            TenderTypeId = string.Empty;
            CurrencyCode = string.Empty;
            Description = string.Empty;
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
        public string StoreID { get; set; }
        [DataMember]
        public string TenderTypeId { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public string Description { get; set; }
    }
}
