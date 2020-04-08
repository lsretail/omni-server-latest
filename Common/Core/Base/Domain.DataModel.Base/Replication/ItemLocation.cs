using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplItemLocationResponse : IDisposable
    {
        public ReplItemLocationResponse(string id)
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Locations = new List<ReplItemLocation>();
        }

        public ReplItemLocationResponse()
            : this(string.Empty)
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
                if (Locations != null)
                    Locations.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplItemLocation> Locations { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplItemLocation : IDisposable
    {
        public ReplItemLocation()
        {
            IsDeleted = false;
            ItemId = string.Empty;
            StoreId = string.Empty;
            SectionCode = string.Empty;
            SectionDescription = string.Empty;
            ShelfCode = string.Empty;
            ShelfDescription = string.Empty;
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
        public string ItemId { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string SectionCode { get; set; }
        [DataMember]
        public string SectionDescription { get; set; }
        [DataMember]
        public string ShelfCode { get; set; }
        [DataMember]
        public string ShelfDescription { get; set; }
    }
}
