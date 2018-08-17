using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplCurrencyExchRateResponse : IDisposable
    {
        public ReplCurrencyExchRateResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            CurrencyExchRates = new List<ReplCurrencyExchRate>();
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
                if (CurrencyExchRates != null)
                    CurrencyExchRates.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplCurrencyExchRate> CurrencyExchRates { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplCurrencyExchRate : IDisposable
    {
        public ReplCurrencyExchRate()
        {
            IsDeleted = false;
            StartingDate = DateTime.Now;
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
        public DateTime StartingDate { get; set; }
        [DataMember]
        public decimal CurrencyFactor { get; set; }
    }
}
