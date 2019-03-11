using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplStoreResponse : IDisposable
    {
        public ReplStoreResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Stores = new List<ReplStore>();
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
                if (Stores != null)
                    Stores.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplStore> Stores { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplStore : IDisposable
    {
        public ReplStore()
        {
            IsDeleted = false;
            Id = string.Empty;
            Name = string.Empty;
            Street = string.Empty;
            ZipCode = string.Empty;
            City = string.Empty;
            Country = string.Empty;
            County = string.Empty;
            State = string.Empty;
            Phone = string.Empty;
            Currency = string.Empty;
            CultureName = string.Empty;
            FunctionalityProfile = string.Empty;
            TaxGroup = string.Empty;
            DefaultCustomerAccount = string.Empty;
            MainMenuID = string.Empty;
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
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Street { get; set; }
        [DataMember]
        public string ZipCode { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public string County { get; set; }
        [DataMember]
        public string State { get; set; }
        [DataMember]
        public string Country { get; set; }
        [DataMember]
        public string Phone { get; set; }
        [DataMember]
        public string Currency { get; set; }
        [DataMember]
        public string CultureName { get; set; }
        [DataMember]
        public string FunctionalityProfile { get; set; }
        [DataMember]
        public string TaxGroup { get; set; }
        [DataMember]
        public string DefaultCustomerAccount { get; set; }
        [DataMember]
        public int UserDefaultCustomerAccount { get; set; }
        [DataMember]
        public decimal Latitute { get; set; }
        [DataMember]
        public decimal Longitude { get; set; }
        [DataMember]
        public bool ClickAndCollect { get; set; }
        [DataMember]
        public string MainMenuID { get; set; }
    }
}
