using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplBarcodeMaskResponse : IDisposable
    {
        public ReplBarcodeMaskResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            BarcodeMasks = new List<ReplBarcodeMask>();
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
                if (BarcodeMasks != null)
                    BarcodeMasks.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplBarcodeMask> BarcodeMasks { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplBarcodeMask : IDisposable
    {
        public ReplBarcodeMask()
        {
            IsDeleted = false;
            Mask = string.Empty;
            Description = string.Empty;
            Prefix = string.Empty;
            NumberSeries = string.Empty;
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
        public int Id { get; set; }
        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string Mask { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int MaskType { get; set; }
        [DataMember]
        public string Prefix { get; set; }
        [DataMember]
        public int Symbology { get; set; }
        [DataMember]
        public string NumberSeries { get; set; }
        [DataMember]
        public List<ReplBarcodeMaskSegment> Segments { get; set; }
    }
}
