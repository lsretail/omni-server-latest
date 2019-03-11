using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplBarcodeMaskSegmentResponse : IDisposable
    {
        public ReplBarcodeMaskSegmentResponse()
        {
            LastKey = "";
            MaxKey = "";
            RecordsRemaining = 0;
            BarcodeMaskSegments = new List<ReplBarcodeMaskSegment>();
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
                if (BarcodeMaskSegments != null)
                    BarcodeMaskSegments.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplBarcodeMaskSegment> BarcodeMaskSegments { get; set; }
    }


    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplBarcodeMaskSegment : IDisposable
    {
        public ReplBarcodeMaskSegment()
        {
            Del = false;
            Char = string.Empty;
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
        public bool Del { get; set; }
        [DataMember]
        public int MaskId { get; set; }
        [DataMember]
        public int Number { get; set; }
        [DataMember]
        public int Length { get; set; }
        [DataMember]
        public int SegmentType { get; set; }
        [DataMember]
        public int Decimals { get; set; }
        [DataMember]
        public string Char { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Del)}: {Del}, {nameof(MaskId)}: {MaskId}, {nameof(Number)}: {Number}, {nameof(Length)}: {Length}, {nameof(SegmentType)}: {SegmentType}, {nameof(Decimals)}: {Decimals}, {nameof(Char)}: {Char}";
        }
    }
}
