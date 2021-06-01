using System;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup
{
    public enum ScanPayGoDeviceType
    {
        CustomerDevice,
        RetailOwnedDevice
    }
    public enum ScanPayGoCatalogType
    {
        ItemCategories,
        Hierarchies
    }

    [Flags]
    public enum ScanPayGoPaymentType
    {
        None = 0,
        Pos = 1,
        Card = 2,
        Loyalty = 4,
    }
    
    public enum ScanPayCardPaymentType
    {
        Demo = 0,
        Braintree = 1,
    }

    public class ScanPayGoSetup
    {
        public ScanPayGoDeviceType DeviceType { get; set; }
        public ScanPayGoCatalogType CatalogType { get; set; }
        public string DefaultWebStore { get; set; }
        public ScanPayGoPaymentType AllowedPaymentTypes { get; set; }
        public ScanPayCardPaymentType CardPaymentType { get; set; }
    }
}
