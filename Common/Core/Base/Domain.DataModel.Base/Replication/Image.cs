using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplImageResponse : IDisposable
    {
        public ReplImageResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Images = new List<ReplImage>();
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
                Images.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplImage> Images { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplImage : IDisposable
    {
        public ReplImage()
        {
            Id = string.Empty;
            IsDeleted = false;
            Image64 = string.Empty;
            Location = string.Empty;
            Description = string.Empty;
            LocationType = LocationType.Image;
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
        /// <summary>
        /// Image as base64 string
        /// </summary>
        [DataMember]
        public string Image64 { get; set; }
        /// <summary>
        /// Location of file or URL
        /// </summary>
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public string Description { get; set; }
        /// <summary>
        /// LocationType File = 0, Image = 1, Url = 2, NoImage = 3,
        /// </summary>
        [DataMember]
        public LocationType LocationType { get; set; }
    }
}
