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
        CustomerAccount = 8,
    }

    public enum ScanPayGoCardPaymentMethod
    {
        Demo = 0,
        LsPay = 1,
    }

    public class ScanPayGoSetup
    {
        private string defaultWebStore;
        private string termsAndConditionUrl;
        private string privacyPolicyUrl;
        private string lsPayServiceIpAddress;
        private string lsPayServicePort;
        private string lsPayPluginId;
        private string audkenniClientID;
        private string audkenniSecret;
        private string audkenniBaseUrl;
        private string audkenniRedirectUrl;
        private string audkenniMessageToUser;
        private string currencyCode;
        private string audkenniTextToMakeAHash;

        public ScanPayGoCatalogType CatalogType { get; set; }

        public string DefaultWebStore
        {
            get
            {
                return defaultWebStore?.Trim();
            }
            set => defaultWebStore = value;
        }

        public ScanPayGoPaymentType AllowedPaymentTypes { get; set; }
        public ScanPayGoDeviceType DeviceType { get; set; }

        public int CheckStatusTimer { get; set; }

        public string TermsAndConditionURL
        {
            get
            {
                return termsAndConditionUrl?.Trim();
            }
            set => termsAndConditionUrl = value;
        }

        public string TermsAndConditionVersion { get; set; }

        public string PrivacyPolicyURL
        {
            get
            {
                return privacyPolicyUrl?.Trim();
            }
            set => privacyPolicyUrl = value;
        }

        public string PrivacyPolicyVersion { get; set; }
        public ScanPayGoCardPaymentMethod CardPaymentMethod { get; set; }

        public string LsPayServiceIpAddress
        {
            get => lsPayServiceIpAddress?.Trim();
            set => lsPayServiceIpAddress = value;
        }

        public string LsPayServicePort
        {
            get => lsPayServicePort?.Trim();
            set => lsPayServicePort = value;
        }

        public string LsPayPluginId
        {
            get => lsPayPluginId?.Trim();
            set => lsPayPluginId = value;
        }

        public string AudkenniClientID
        {
            get => audkenniClientID?.Trim();
            set => audkenniClientID = value;
        }

        public string AudkenniTextToMakeAHash
        {
            get => audkenniTextToMakeAHash?.Trim();
            set => audkenniTextToMakeAHash = value;
        }

        public string AudkenniSecret
        {
            get => audkenniSecret?.Trim();
            set => audkenniSecret = value;
        }

        public string AudkenniBaseUrl
        {
            get => audkenniBaseUrl?.Trim();
            set => audkenniBaseUrl = value;
        }

        public string AudkenniRedirectUrl
        {
            get => audkenniRedirectUrl?.Trim();
            set => audkenniRedirectUrl = value;
        }

        public string AudkenniMessageToUser
        {
            get => audkenniMessageToUser?.Trim();
            set => audkenniMessageToUser = value;
        }

        public bool EnablePlatformPayment { get; set; }
        public bool AllowAnonymousUser { get; set; }
        public string AllowShopHome { get; set; }
        public string ApplePayMerchantName { get; set; }

        public bool AudkenniLoginEnabled { get; set; }
        public bool AudkenniTestUserEnabled { get; set; } = default;
        public string AudkenniTestUser { get; set; } = default;
        public string AudkenniTestCardId { get; set; } = default;

        public bool GoogleLoginEnabled { get; set; }
        public string GoogleIosClientId { get; set; }
        public bool FacebookLoginEnabled { get; set; }
        public bool AppleLoginEnabled { get; set; }
        public bool OpenGateEnabled { get; set; }
        public bool CloseGateEnabled { get; set; }
        public bool CustomerSurveyEnabled { get; set; }
        public bool AddCardBeforeShopping { get; set; }
        public bool ShowCustomerQrCode { get; set; }
        public bool ShowPointStatus { get; set; }
        public bool HidePriceOfItem { get; set; }
        public bool HideAddCreditCard { get; set; }
        public bool HideShoppingScreen { get; set; }
        public bool UseSecurityCheck { get; set; }
        public string ApplePayConnectionId { get; set; }
        public string GooglePayConnectionId { get; set; }
        public bool UseOnlineSearch { get; set; } = default;

        public string CurrencyCode
        {
            get => currencyCode?.Trim();
            set => currencyCode = value;
        }
    }
}
