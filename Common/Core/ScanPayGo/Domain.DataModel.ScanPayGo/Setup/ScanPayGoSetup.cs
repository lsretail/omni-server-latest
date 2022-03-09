using System;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Utils;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup
{
    public enum ScanPayGoCatalogType
    {
        Hierarchies,
        ItemCategories
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
        public ScanPayGoCatalogType CatalogType { get; set; }
        public string DefaultWebStore { get; set; }
        public ScanPayGoPaymentType AllowedPaymentTypes { get; set; }
        public ScanPayCardPaymentType CardPaymentType { get; set; }
        public ScanPayGoDeviceType DeviceType { get; set; }

        public int CheckStatusTimer { get; set; }
        public string TermsAndConditionURL { get; set; }
        public string TermsAndConditionVersion { get; set; }
        public bool EnablePlatformPayment { get; set; }
        public string PlatformPaymentCountryCode { get; set; }
        public string PlatformPaymentCurrencyCode { get; set; }
        public string GooglePayGatewayJson { get; set; }
        public string GooglePayAllowedAuthMethodsJson { get; set; }
        public string GooglePayAllowedCardNetworksJson { get; set; }
        public string GooglePayMerchantName { get; set; }
        public bool AllowAnonymousUser { get; set; }
        public string AllowShopHome { get; set; }
        public string ApplePayMerchantName { get; set; }

        public string AudkenniClientID { get; set; }
        public string AudkenniSecret { get; set; }
        public string AudkenniBaseUrl { get; set; }
        public string AudkenniRedirectUrl { get; set; }
        public string AudkenniMessageToUser { get; set; }
        public bool AudkenniLoginEnabled { get; set; }
        public bool GoogleLoginEnabled { get; set; }
        public string GoogleIosClientId { get; set; }
        public bool FacebookLoginEnabled { get; set; }
        public bool AppleLoginEnabled { get; set; }
        public bool OpenGateEnabled { get; set; }
        public bool CloseGateEnabled { get; set; }
        public bool ExperienceSurveyEnabled { get; set; } = true;
    }
}
