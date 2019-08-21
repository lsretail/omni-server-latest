using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplVendorResponse : IDisposable
    {
        public ReplVendorResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Vendors = new List<ReplVendor>();
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
                if (Vendors != null)
                    Vendors.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplVendor> Vendors { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplVendor : IDisposable
    {
        /// <summary>
        /// Vendor or Manufacturer
        /// </summary>
        public ReplVendor(string id)
        {
            IsDeleted = false;
            Id = id;
            Name = string.Empty;
            PageSizeOptions = string.Empty;
            UpdatedOnUtc = new DateTime(1900, 1, 1);
        }

        public ReplVendor() : this(string.Empty)
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
        [DataMember]
        public int ManufacturerTemplateId { get; set; }
        [DataMember]
        public int PictureId { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public bool AllowCustomersToSelectPageSize { get; set; }
        [DataMember]
        public string PageSizeOptions { get; set; }
        [DataMember]
        public bool Published { get; set; }
        [DataMember]
        public bool Blocked { get; set; }
        [DataMember]
        public int DisplayOrder { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime UpdatedOnUtc { get; set; }
    }
}
