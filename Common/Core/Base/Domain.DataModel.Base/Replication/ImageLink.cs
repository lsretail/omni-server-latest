using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplImageLinkResponse : IDisposable
    {
        public ReplImageLinkResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            ImageLinks = new List<ReplImageLink>();
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
                ImageLinks.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplImageLink> ImageLinks { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplImageLink : IDisposable
    {
        /// <summary>
        /// TableName, KeyValue, Image is PK
        /// </summary>
        public ReplImageLink()
        {
            ImageId = string.Empty;
            IsDeleted = false;
            DisplayOrder = 0;
            TableName = string.Empty;
            KeyValue = string.Empty;
            Description = string.Empty;
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
        public string ImageId { get; set; }
        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string TableName { get; set; }
        [DataMember]
        public string KeyValue { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int DisplayOrder { get; set; }
    }
}
