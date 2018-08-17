using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Utils
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class PushNotification
    {
        public PushNotification(string id)
        {
            NotificationId = id;
            DeviceIds = new List<string>();
            Application = PushApplication.Unknown;
            Platform = PushPlatform.Unknown;
            Status = PushStatus.Enabled;
            CreatedDate = DateTime.Now;
            LastModifiedDate = DateTime.Now;
            Title = string.Empty;
            Body = string.Empty;
            ContactId = string.Empty;
        }
 
        public PushNotification()
            : this(string.Empty)
        {
        }

        [DataMember]
        public string NotificationId { get; set; }
        [DataMember]
        public List<string> DeviceIds { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Body { get; set; }
        [DataMember]
        public string ContactId { get; set; }
        [DataMember]
        public DateTime CreatedDate { get; set; }
        [DataMember]
        public DateTime LastModifiedDate { get; set; }
        [DataMember]
        public PushApplication Application { get; set; }
        [DataMember]
        public PushPlatform Platform { get; set; }
        [DataMember]
        public PushStatus Status { get; set; }

        public override string ToString()
        {
            return string.Format("NotificationId: {0} DeviceId: {1} Application: {2} Platform: {3} Status: {4} ",
                NotificationId, string.Join(", ", DeviceIds), Application.ToString(), Platform.ToString(), Status.ToString());
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class PushNotificationRequest
    {
        //Advertise 
        public PushNotificationRequest(string id)
        {
            Id = id;  //notification id
            DeviceId = string.Empty; // FCM device registration token
            Application = PushApplication.Unknown;
            Platform = PushPlatform.Unknown;
            Status = PushStatus.Enabled;
            Title = string.Empty;
            Body = string.Empty;
        }
        public PushNotificationRequest()
            : this(string.Empty)
        {
        }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string DeviceId { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Body { get; set; }
        [DataMember]
        public PushApplication Application { get; set; }
        [DataMember]
        public PushPlatform Platform { get; set; }
        [DataMember]
        public PushStatus Status { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0} DeviceId: {1} Application: {2} Platform: {3} Status: {4} ",
                Id, DeviceId, Application.ToString(), Platform.ToString(), Status.ToString());
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    [Flags]
    public enum PushApplication
    {
        [EnumMember]
        Unknown = 0,  
        [EnumMember]
        Loyalty = 1,        
        [EnumMember]
        HospLoy = 2,      
        [EnumMember]
        MPOS = 3,
        [EnumMember]
        Inventory = 4,        
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    [Flags]
    public enum PushPlatform
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        Apple = 1,   //APNS
        [EnumMember]
        Android = 2,  //GCM
        [EnumMember]
        Windows = 3,  //Windows Push
        //[EnumMember]
        //Blackberry = 4,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    [Flags]
    public enum PushStatus
    {
        [EnumMember]
        Disabled = 0,
        [EnumMember]
        Enabled = 1, 
    }
}
