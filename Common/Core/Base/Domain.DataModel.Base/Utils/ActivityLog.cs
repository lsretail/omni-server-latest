using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Utils
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ActivityLog : IDisposable
    {
        public ActivityLog()
        {
            LogSolution = ActivityLogSolution.Unknown;
            LogType = ActivityLogType.Unknown;
            Value = string.Empty;  //value for logtype. ex. 40030 for ActivityLogType.Item
            ContactId = string.Empty;  //the logged on contactId
#if WCFSERVER
            DeviceId = string.Empty;
            IPAddress = string.Empty;
#endif
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
        public ActivityLogSolution LogSolution { get; set; }
        [DataMember]
        public ActivityLogType LogType { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public string ContactId { get; set; }

        public override string ToString()
        {
            string s = string.Format("ContactId: {0} Value: {1} LogType: {2} LogSolution: {3} ",
                ContactId, Value, LogType.ToString(), LogSolution.ToString());
            return s;
        }

#if WCFSERVER
        public string DeviceId { get; set; }
        public string IPAddress { get; set; }
#endif
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    [Flags]
    public enum ActivityLogSolution
    {
        [EnumMember]
        LoyaltyRetail = 0,
        [EnumMember]
        LoyaltyHospitality = 1,
        [EnumMember]
        MPOSRetail = 2,
        [EnumMember]
        MPOSHospitality = 3,
        [EnumMember]
        ECommerce = 4,
        [EnumMember]
        Inventory = 5,
        [EnumMember]
        Unknown = 999,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    [Flags]
    public enum ActivityLogType
    {
        [EnumMember]
        ItemGroup = 0,
        [EnumMember]
        ProductGroup = 1,
        [EnumMember]
        Item = 2,
        [EnumMember]
        Search = 3,   //search sstring
        [EnumMember]
        Coupon = 4,
        [EnumMember]
        Offer = 5,
        [EnumMember]
        Notification = 6,
        [EnumMember]
        Login = 7,
        [EnumMember]
        Logoff = 8,
        [EnumMember]
        Contact = 9,  //contact use
        [EnumMember]
        ClickCollect = 10,
        [EnumMember]
        SalesOrder = 11,
        [EnumMember]
        Store = 12,  // store number
        [EnumMember]
        TransHistory = 13,
        [EnumMember]
        Unknown = 999,
    }
}
