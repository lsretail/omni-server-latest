using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ReplPluResponse : IDisposable
    {
        public ReplPluResponse()
        {
            LastKey = "";
            MaxKey = "";
            RecordsRemaining = 0;
            Plus = new List<ReplPlu>();
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
                if (Plus != null)
                    Plus.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplPlu> Plus { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ReplPlu : IDisposable
    {
        public ReplPlu()
        {
            IsDeleted = false;
            StoreId = string.Empty;
            PageId = 0;
            PageIndex = 0;
            ItemId = string.Empty;
            Descritpion = string.Empty;
            ImageId = string.Empty;
            ItemImageId = string.Empty;
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
        public string StoreId { get; set; }
        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public int PageId { get; set; }
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string Descritpion { get; set; }
        [DataMember]
        public string ItemImageId { get; set; }
        [DataMember]
        public string ImageId { get; set; }

        public byte[] ItemImage { get; set; }
    }
}
