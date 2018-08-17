using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplUnitOfMeasureResponse : IDisposable
    {
        public ReplUnitOfMeasureResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            UnitOfMeasures = new List<ReplUnitOfMeasure>();
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
                if (UnitOfMeasures != null)
                    UnitOfMeasures.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplUnitOfMeasure> UnitOfMeasures { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplUnitOfMeasure : IDisposable
    {
        public ReplUnitOfMeasure()
        {
            IsDeleted = false;
            Id = string.Empty;
            Description = string.Empty;
            ShortDescription = string.Empty;
            UnitDecimals = 0;
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
        public string Description { get; set; }
        [DataMember]
        public string ShortDescription { get; set; }
        [DataMember]
        public int UnitDecimals { get; set; }
    }
}
