using System;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Setup
{
    //only used on server, never on device clients
    public class LicenseActivityLog : Entity, IDisposable
    {
        public LicenseActivityLog(string id) : base(id)
        {
            ClientCode = string.Empty;
            LicenseType = string.Empty;
            UserId = string.Empty;
            Activity = string.Empty;
            DeviceId = string.Empty;
            DevicePlatform = string.Empty;
            DeviceOsVersion = string.Empty;
            DeviceManufacturer = string.Empty;
            DeviceModel = string.Empty;
            IPAddress = string.Empty;
            State = string.Empty; //U, I, P  unprocessed, inprocess, processed
            StateChanged = DateTime.MinValue.ToUniversalTime();
            ProcessId = string.Empty;
            Created = DateTime.MinValue.ToUniversalTime();
        }

        public LicenseActivityLog()
            : this(string.Empty)
        {
            Id = Guid.NewGuid().ToString().ToUpper();
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

        public string ClientCode { get; set; }
        public string LicenseType { get; set; }
        public string UserId { get; set; }
        public string Activity { get; set; }
        public string DeviceId { get; set; }
        public string DevicePlatform { get; set; }
        public string DeviceOsVersion { get; set; }
        public string DeviceManufacturer { get; set; }
        public string DeviceModel { get; set; }
        public string IPAddress { get; set; }
        public string State { get; set; }
        public DateTime StateChanged { get; set; }
        public string ProcessId { get; set; }
        public DateTime Created { get; set; }
    }
}
 