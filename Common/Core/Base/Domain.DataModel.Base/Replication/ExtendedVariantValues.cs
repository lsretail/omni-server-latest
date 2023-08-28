using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplExtendedVariantValuesResponse : IDisposable
    {
        public ReplExtendedVariantValuesResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            ExtendedVariantValue = new List<ReplExtendedVariantValue>();
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
                if (ExtendedVariantValue != null)
                    ExtendedVariantValue.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplExtendedVariantValue> ExtendedVariantValue { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplExtendedVariantValue : IDisposable
    {
        public ReplExtendedVariantValue()
        {
            IsDeleted = false;
            Timestamp = string.Empty;
            Dimensions = string.Empty;
            Code = string.Empty;
            CodeDescription = string.Empty;
            Value = string.Empty;
            ValueDescription = string.Empty;
            FrameworkCode = string.Empty;
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
        public string Timestamp { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string Dimensions { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string CodeDescription { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public string ValueDescription { get; set; }
        [DataMember]
        public int LogicalOrder { get; set; }
        [DataMember]
        public int DimensionLogicalOrder { get; set; }
        [DataMember]
        public string FrameworkCode { get; set; }
    }
}
