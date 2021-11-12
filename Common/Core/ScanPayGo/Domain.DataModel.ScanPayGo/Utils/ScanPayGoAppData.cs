using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Utils
{
    public enum ScanPayGoDeviceType
    {
        CustomerDevice,
        RetailOwnedDevice
    }

    public class ScanPayGoAppData
    {
        public static ScanPayGoDeviceType DeviceType { get; set; } = ScanPayGoDeviceType.CustomerDevice;
        public static ScanPayGoSetup AppSetup { get; set; }
    }
}
