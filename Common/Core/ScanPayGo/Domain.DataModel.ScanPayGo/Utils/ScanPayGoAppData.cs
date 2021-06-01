using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Utils
{
    public class ScanPayGoAppData
    {
        public static ScanPayGoSetup AppSetup
        {
            get
            {
                return new ScanPayGoSetup()
                {
                    DeviceType = ScanPayGoDeviceType.CustomerDevice,
                    CatalogType = ScanPayGoCatalogType.Hierarchies,
                    DefaultWebStore = "S0013",
                    AllowedPaymentTypes = ScanPayGoPaymentType.Pos | ScanPayGoPaymentType.Loyalty,
                    CardPaymentType = ScanPayCardPaymentType.Braintree,
                };
            }
        }
    }
}
