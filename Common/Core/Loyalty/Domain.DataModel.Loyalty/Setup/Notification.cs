using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class Notification : Entity, IDisposable
    {
        public Notification(string id) : base(id)
        {
            ContactId = string.Empty;
            Description = string.Empty;
            Details = string.Empty;
            Status = NotificationStatus.New;
            NotificationTextType = NotificationTextType.Plain;
            ExpiryDate = null;
            Created = DateTime.Now;
            Images = new List<ImageView>();
            QRText = string.Empty;
#if WCFSERVER  
            ImgBytes = null;
            DateLastModified = DateTime.Now;
            NotificationType = NotificationType.BO;
#endif
        }

        public Notification()
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
                if (Images != null)
                    Images.Clear();
            }
        }

        [DataMember]
        public string ContactId { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Details { get; set; }
        [DataMember]
        public string QRText { get; set; }
        [DataMember]
        public NotificationStatus Status { get; set; }
        [DataMember]
        public NotificationTextType NotificationTextType { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? ExpiryDate { get; set; }
        [DataMember]
        public List<ImageView> Images { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime Created { get; set; }
#if WCFSERVER
        //not all data goes to wcf clients
        public byte[] ImgBytes { get; set; }
        public DateTime DateLastModified { get; set; }
        public NotificationType NotificationType { get; set; }
#endif
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class NotificationUnread : IDisposable
    {
        public NotificationUnread()
        {
            Count = 0;
            Created = DateTime.Now;
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
        public int Count { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime Created { get; set; }
    }


    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum NotificationPermission
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        None = 1,
        [EnumMember]
        All = 2,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    [Flags]
    public enum NotificationStatus
    {
        [EnumMember]
        New = 0,
        [EnumMember]
        Read = 1,
        [EnumMember]
        Closed = 2,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum NotificationTextType
    {
        [EnumMember]
        Plain = 0,
        [EnumMember]
        Html = 1,
    }

#if WCFSERVER
    //only used on LSOmniServer
    public enum NotificationType
    {
        BO = 0,
        OrderMessage = 1,
    }
#endif
}

