using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplGS1BarcodeResponse : IDisposable
    {
        public ReplGS1BarcodeResponse()
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
                if (Setups != null)
                    Setups.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplGS1BarcodeSetup> Setups { get; set; }
    }

    public class ReplGS1BarcodeSetup : IDisposable
    {
        public ReplGS1BarcodeSetup()
        {
            Identifier = string.Empty;
            Value = string.Empty;
            ValueDate = DateTime.MinValue;
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
        public int Type { get; set; }
        [DataMember]
        public string Identifier { get; set; }
        [DataMember]
        public int SectionType { get; set; }
        [DataMember]
        public int SectionSize { get; set; }
        [DataMember]
        public int IdentifierSize { get; set; }
        [DataMember]
        public int SectionMapping { get; set; }
        [DataMember]
        public int MappingStartingChar { get; set; }
        [DataMember]
        public int PreferredSequence { get; set; }
        [DataMember]
        public decimal Decimals { get; set; }
        [DataMember]
        public int ValueType { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public decimal ValueDec { get; set; }
        [DataMember]
        public DateTime ValueDate { get; set; }
    }
}
