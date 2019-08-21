using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum StoreHourType
    {
        [EnumMember]
        MainStore = 0,
        [EnumMember]
        DriveThruWindow = 1,
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
            StoreHourtype = StoreHourType.MainStore;
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
        public StoreHourType StoreHourtype { get; set; }
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
        [DataMember]
        public StoreHourOpeningType Type { get; set; }
    }
}
 