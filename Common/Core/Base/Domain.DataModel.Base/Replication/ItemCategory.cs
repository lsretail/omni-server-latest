using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplItemCategoryResponse : IDisposable
    {
        public ReplItemCategoryResponse(string id)
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            ItemCategories = new List<ReplItemCategory>();
        }

        public ReplItemCategoryResponse()
            : this(string.Empty)
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
                if (ItemCategories != null)
                    ItemCategories.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplItemCategory> ItemCategories { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplItemCategory : IDisposable
    {
        public ReplItemCategory(string id)
        {
            Id = id;
            IsDeleted = false;
            Description = string.Empty;
        }

        public ReplItemCategory() : this(string.Empty)
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
        public string Id { get; set; }
        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string Description { get; set; }
    }
}
