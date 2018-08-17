using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class StoreServices : IDisposable
    {
        public StoreServices(string storeId)
        {
            StoreId = storeId;
            Description = string.Empty;
            StoreServiceType = StoreServiceType.None;
        }

        public StoreServices()
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
        public StoreServiceType StoreServiceType { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string Description { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum StoreServiceType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Garden = 1,
        [EnumMember]
        FreeWiFi = 2,
        [EnumMember]
        DriveThruWindow = 3,
        [EnumMember]
        GiftCard = 4,
        [EnumMember]
        PlayPlace = 5,
        [EnumMember]
        FreeRefill = 6,
    }
}