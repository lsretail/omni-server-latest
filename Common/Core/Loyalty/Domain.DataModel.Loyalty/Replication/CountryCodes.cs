using LSRetail.Omni.Domain.DataModel.Base.Base;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ReplCountryCodeResponse : IDisposable
    {
        public ReplCountryCodeResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Codes = new List<ReplCountryCode>();
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
                Codes.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplCountryCode> Codes { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ReplCountryCode : IDisposable
    {
        public ReplCountryCode(string id)
        {
            Id = id;
        }

        public ReplCountryCode() : this(string.Empty)
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
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
    }
}
