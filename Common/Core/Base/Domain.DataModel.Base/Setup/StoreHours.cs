using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum StoreHourCalendarType
    {
        [EnumMember]
        All = 0,
        [EnumMember]
        OpeningHours = 1,
        [EnumMember]
        Receiving = 2,
        [EnumMember]
        RestOrderTaking = 3,
        [EnumMember]
        Other = 7
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum StoreHourOpeningType
    {
        [EnumMember]
        Normal = 0,
        [EnumMember]
        Temporary = 1,
        [EnumMember]
        Closed = 2,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class StoreHours : IDisposable
    {
        public StoreHours(string storeId)
        {
            StoreId = storeId;
            DayOfWeek = 0;
            NameOfDay = string.Empty;
            Description = string.Empty;
            OpenFrom = new DateTime(1900, 1, 1);
            OpenTo = new DateTime(1900, 1, 1);
            StartDate = new DateTime(1900, 1, 1);
            EndDate = new DateTime(1900, 1, 1);
            CalendarType = StoreHourCalendarType.All;
            Type = StoreHourOpeningType.Normal;
        }

        public StoreHours()
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
            }
        }

        [DataMember]
        public StoreHourCalendarType CalendarType { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public int DayOfWeek { get; set; }
        [DataMember]
        public string NameOfDay { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime OpenFrom { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime OpenTo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime StartDate { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime EndDate { get; set; }
        [DataMember]
        public StoreHourOpeningType Type { get; set; }
    }
}
 