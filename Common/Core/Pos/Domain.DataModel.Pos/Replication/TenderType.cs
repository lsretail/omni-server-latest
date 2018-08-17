using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ReplTenderTypeResponse : IDisposable
    {
        public ReplTenderTypeResponse()
        {
            LastKey = "";
            MaxKey = "";
            RecordsRemaining = 0;
            TenderTypes = new List<ReplTenderType>();
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
                if (TenderTypes != null)
                    TenderTypes.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplTenderType> TenderTypes { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ReplTenderType : IDisposable
    {
        public ReplTenderType()
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
        public string Nam { get; set; }
        [DataMember]
        public int DefaultFunction { get; set; }
    }
}
