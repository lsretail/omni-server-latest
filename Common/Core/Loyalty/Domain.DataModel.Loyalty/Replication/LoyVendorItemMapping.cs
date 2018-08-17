using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ReplLoyVendorItemMappingResponse : IDisposable
    {
        public ReplLoyVendorItemMappingResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Mapping = new List<ReplLoyVendorItemMapping>();
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
                Mapping.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplLoyVendorItemMapping> Mapping { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ReplLoyVendorItemMapping : IDisposable
    {
        public ReplLoyVendorItemMapping()
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
        public bool IsDeleted { get; set; }
        [DataMember]
        public string NavProductId { get; set; }
        [DataMember]
        public string NavManufacturerId { get; set; }
        [DataMember]
        public string NavManufacturerItemId { get; set; }
        [DataMember]
        public int DisplayOrder { get; set; }
        [DataMember]
        public bool IsFeaturedProduct { get; set; }
        [DataMember]
        public bool Deleted { get; set; }
    }
}
