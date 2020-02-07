using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplTaxSetupResponse : IDisposable
    {
        public ReplTaxSetupResponse()
        {
            LastKey = "";
            MaxKey = "";
            RecordsRemaining = 0;
            TaxSetups = new List<ReplTaxSetup>();
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
                if (TaxSetups != null)
                    TaxSetups.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplTaxSetup> TaxSetups { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplTaxSetup : IDisposable
    {
        public ReplTaxSetup()
        {
            BusinessTaxGroup = string.Empty;
            ProductTaxGroup = string.Empty;
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
        public string BusinessTaxGroup { get; set; }
        [DataMember]
        public string ProductTaxGroup { get; set; }
        [DataMember]
        public decimal TaxPercent { get; set; }
    }
}
