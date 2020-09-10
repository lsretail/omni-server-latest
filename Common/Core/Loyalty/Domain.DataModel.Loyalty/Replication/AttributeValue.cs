using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ReplAttributeValueResponse : IDisposable
    {
        public ReplAttributeValueResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Values = new List<ReplAttributeValue>();
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
                if (Values != null)
                    Values.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplAttributeValue> Values { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ReplAttributeValue : IDisposable
    {
        public ReplAttributeValue()
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
        public string LinkField1 { get; set; }
        [DataMember]
        public string LinkField2 { get; set; }
        [DataMember]
        public string LinkField3 { get; set; }
        [DataMember]
        public int LinkType { get; set; }
        [DataMember]
        public int Sequence { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public decimal NumbericValue { get; set; }
    }
}

