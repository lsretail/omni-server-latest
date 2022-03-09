using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ReplCountryCodeResponse : IDisposable
    {
        public ReplCountryCodeResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Codes = new List<ReplCountryCode>();
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
                if (Codes != null)
                    Codes.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplCountryCode> Codes { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ReplCountryCode : IDisposable
    {
        public ReplCountryCode()
        {
            Code = string.Empty;
            Name = string.Empty;
            CustomerNo = string.Empty;
            TaxPostGroup = string.Empty;
            TaxItemGroups = new List<TaxItemGroup>();
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
                if (TaxItemGroups != null)
                    TaxItemGroups.Clear();
            }
        }

        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string CustomerNo { get; set; }
        [DataMember]
        public string TaxPostGroup { get; set; }
        [DataMember]
        public List<TaxItemGroup> TaxItemGroups { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class TaxItemGroup : IDisposable
    {
        public TaxItemGroup()
        {
            Code = string.Empty;
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
        public string Code { get; set; }
        [DataMember]
        public decimal TaxPercent { get; set; }
    }
}
