using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplItemUnitOfMeasureResponse : IDisposable
    {
        public ReplItemUnitOfMeasureResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            ItemUnitOfMeasures = new List<ReplItemUnitOfMeasure>();
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
                if (ItemUnitOfMeasures != null)
                    ItemUnitOfMeasures.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplItemUnitOfMeasure> ItemUnitOfMeasures { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplItemUnitOfMeasure : IDisposable
    {
        public ReplItemUnitOfMeasure()
        {
            IsDeleted = false;
            ItemId = string.Empty;
            Description = string.Empty;
            ShortDescription = string.Empty;
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
        public bool IsDeleted { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string ShortDescription { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public int Order { get; set; }
        [DataMember]
        public decimal QtyPrUOM { get; set; }
        [DataMember]
        public bool CountAsOne { get; set; }
        [DataMember]
        public int Selection { get; set; }
    }
}
