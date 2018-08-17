using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplBarcodeResponse : IDisposable
    {
        public ReplBarcodeResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Barcodes = new List<ReplBarcode>();
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
                if (Barcodes != null)
                    Barcodes.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplBarcode> Barcodes { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplBarcode : IDisposable
    {
        public ReplBarcode(string id)
        {
            Id = id;
        }

        public ReplBarcode() : this(string.Empty)
        {
            IsDeleted = false;
            ItemId = string.Empty;
            VariantId = string.Empty;
            UnitOfMeasure = string.Empty;
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
        public string Id { get; set; }
        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string UnitOfMeasure { get; set; }
        [DataMember]
        public int Blocked { get; set; }
        [DataMember]
        public string Description { get; set; }
    }
}
