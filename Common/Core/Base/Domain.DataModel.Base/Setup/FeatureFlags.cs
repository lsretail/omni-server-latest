using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
    public class FeatureFlags
    {
        [DataMember]
        public List<FeatureFlag> Flags { get; set; } = new List<FeatureFlag>();

        public void AddFlag(FeatureFlagName flagName, string flagValue)
        {
            Flags.Add(new FeatureFlag()
            {
                name = flagName,
                value = flagValue
            });
        }

        public void AddFlag(FeatureFlagName flagName, int flagValue)
        {
            Flags.Add(new FeatureFlag()
            {
                name = flagName,
                value = flagValue.ToString()
            });
        }

        public void AddFlag(string flagCode, string flagValue)
        {
            FeatureFlagName flagName = FeatureFlagName.None;

            switch (flagCode)
            {
                case "ALLOW AUTO LOGOFF":
                    flagName = FeatureFlagName.AllowAutoLogoff;
                    break;
                case "ALLOW LS CENTRAL LOGIN":
                    flagName = FeatureFlagName.AllowCentralLogin;
                    break;
                case "ALLOW OFFLINE":
                    flagName = FeatureFlagName.AllowOffline;
                    break;
                case "SEND RECEIPT IN EMAIL":
                    flagName = FeatureFlagName.SendReceiptInEmail;
                    break;
                case "USE LOYALITY SYSTEM":
                    flagName = FeatureFlagName.UseLoyaltySystem;
                    break;
                case "POS SHOW INVENTORY":
                    flagName = FeatureFlagName.PosShowInventory;
                    break;
                case "POS INVENTORY LOOKUP":
                    flagName = FeatureFlagName.PosInventoryLookup;
                    break;
                case "SETTINGS PASSWORD":
                    flagName = FeatureFlagName.SettingsPassword;
                    break;
                case "HIDE VOIDED TRANSACTION":
                    flagName = FeatureFlagName.HideVoidedTransaction;
                    break;
                //SPG
                case "ALLOW ANONYMOUS":
                    flagName = FeatureFlagName.AllowAnonymousUser;
                    break;
                case "ALLOW SHOP HOME":
                    flagName = FeatureFlagName.AllowShopHome;
                    break;
                case "DEVICE TYPE":
                    flagName = FeatureFlagName.DeviceType;
                    break;
                case "CATALOG TYPE":
                    flagName = FeatureFlagName.CatalogType;
                    break;
                case "DEFAULT WEB STORE":
                    flagName = FeatureFlagName.DefaultWebStore;
                    break;
                case "ALLOWED PAYMENT WITH POS":
                    flagName = FeatureFlagName.AllowedPaymentWithPOS;
                    break;
                case "ALLOWED PAYMENT WITH CARD":
                    flagName = FeatureFlagName.AllowedPaymentWithCard;
                    break;
                case "ALLOWED PAYMENT WITH LOYALTY":
                    flagName = FeatureFlagName.AllowedPaymentWithLoyalty;
                    break;
                case "CARD PAYMENT TYPE":
                    flagName = FeatureFlagName.CardPaymentType;
                    break;
                case "CHECK STATUS TIMER":
                    flagName = FeatureFlagName.CheckStatusTimer;
                    break;
                case "TERMS AND CONDITION URL":
                    flagName = FeatureFlagName.TermsAndConditionURL;
                    break;
                case "TERMS AND CONDITION VERSION":
                    flagName = FeatureFlagName.TermsAndConditionVersion;
                    break;
                case "PRIVACY POLICY URL":
                    flagName = FeatureFlagName.PrivacyPolicyURL;
                    break;
                case "PRIVACY POLICY VERSION":
                    flagName = FeatureFlagName.PrivacyPolicyVersion;
                    break;
                case "ENABLE PLATFORM PAYMENT":
                    flagName = FeatureFlagName.EnablePlatformPayment;
                    break;
                case "PLATFORM PAYMENT CURRENCY CODE":
                    flagName = FeatureFlagName.PlatformPaymentCurrencyCode;
                    break;
                case "PLATFORM PAYMENT COUNTRY CODE":
                    flagName = FeatureFlagName.PlatformPaymentCountryCode;
                    break;
                case "GOOGLE PAY GATEWAYJSON":
                    flagName = FeatureFlagName.GooglePayGatewayJson;
                    break;
                case "GOOGLE PAY ALLOWED AUTH METHODSJSON":
                    flagName = FeatureFlagName.GooglePayAllowedAuthMethodsJson;
                    break;
                case "GOOGLE PAY ALLOWED CARD NETWORKJSON":
                    flagName = FeatureFlagName.GooglePayAllowedCardNetworksJson;
                    break;
                case "GOOGLE PAY MERCHANT NAME":
                    flagName = FeatureFlagName.GooglePayMerchantName;
                    break;
                case "APPLE PAY MERCHANT NAME":
                    flagName = FeatureFlagName.GooglePayMerchantName;
                    break;

                case "ADD CARD BEFORE SHOPPING":
                    flagName = FeatureFlagName.AddCardBeforeShopping;
                    break;

                case "SHOW CUSTOMER QR CODE":
                    flagName = FeatureFlagName.ShowCustomerQrCode;
                    break;

                //AUDKENNI SPG
                case "AUDKENNI BASE URL":
                    flagName = FeatureFlagName.AudkenniBaseURL;
                    break;
                case "AUDKENNI CLIENT ID":
                    flagName = FeatureFlagName.AudkenniClientId;
                    break;
                case "AUDKENNI REDIRECT URL":
                    flagName = FeatureFlagName.AudkenniRedirectURL;
                    break;
                case "AUDKENNI SECRET":
                    flagName = FeatureFlagName.AudkenniSecret;
                    break;
                case "AUDKENNI MESSAGE TO USER":
                    flagName = FeatureFlagName.AudkenniMessageToUser;
                    break;
                case "AUDKENNI LOGIN ENABLED":
                    flagName = FeatureFlagName.AudkenniLoginEnabled;
                    break;
                case "FACEBOOK LOGIN ENABLED":
                    flagName = FeatureFlagName.FacebookLoginEnabled;
                    break;
                case "GOOGLE LOGIN ENABLED":
                    flagName = FeatureFlagName.GoogleLoginEnabled;
                    break;
                case "GOOGLE IOS CLIENT ID":
                    flagName = FeatureFlagName.GoogleIosClientId;
                    break;
                case "APPLE LOGIN ENABLED":
                    flagName = FeatureFlagName.AppleLoginEnabled;
                    break;
                case "OPEN GATE":
                    flagName = FeatureFlagName.OpenGate;
                    break;
                case "CLOSE GATE":
                    flagName = FeatureFlagName.CloseGate;
                    break;
                case "SHOW CUSTOMER SURVEY":
                    flagName = FeatureFlagName.ShowCustomerSurvey;
                    break;

                //Card Payments
                case "CARD PAYMENT METHOD":
                    flagName = FeatureFlagName.CardPaymentMethod;
                    break;
                case "LS PAY SERVICE IP ADDRESS":
                    flagName = FeatureFlagName.LsPayServiceIpAddress;
                    break;
                case "LS PAY SERVICE PORT":
                    flagName = FeatureFlagName.LsPayServicePort;
                    break;
                case "LS PAY PLUGIN ID":
                    flagName = FeatureFlagName.LsPayPluginId;
                    break;
            }

            Flags.Add(new FeatureFlag()
            {
                name = flagName,
                value = flagValue
            });
        }

        public bool GetFlagBool(FeatureFlagName flagName, bool defaultValue)
        {
            FeatureFlag flag = Flags.Find(f => f.name == flagName);
            if (flag == null)
                return defaultValue;

            try
            {
                return Convert.ToInt16(flag.value) == 1;
            }
            catch
            {
                try
                {
                    return Convert.ToBoolean(flag.value);
                }
                catch
                {
                    try
                    {
                        return flag.value.Equals("Yes", StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
            }
        }

        public int GetFlagInt(FeatureFlagName flagName, int defaultValue)
        {
            FeatureFlag flag = Flags.Find(f => f.name == flagName);
            if (flag == null)
                return defaultValue;

            try
            {
                return Convert.ToInt32(flag.value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public string GetFlagString(FeatureFlagName flagName, string defaultValue)
        {
            FeatureFlag flag = Flags.Find(f => f.name == flagName);
            if (flag == null)
                return defaultValue;

            return (flag.value == null) ? string.Empty : flag.value;
        }
    }

    public class FeatureFlag
    {
        public FeatureFlagName name = FeatureFlagName.None;
        public string value = string.Empty;
    }

    public enum FeatureFlagName
    {
        None = 100,
        AllowAutoLogoff = 101,
        AutoLogOffAfterMin = 102,
        AllowOffline = 103,
        ExitAfterEachTransaction = 104,
        SendReceiptInEmail = 105,
        ShowNumberPad = 106,
        UseLoyaltySystem = 107,
        PosShowInventory = 108,
        PosInventoryLookup = 109,
        SettingsPassword = 110,
        HideVoidedTransaction = 111,
        AllowCentralLogin = 112,

        //ScanPayGo
        AllowAnonymousUser = 200,
        AllowShopHome = 201,
        DeviceType = 202,
        CatalogType = 203,
        DefaultWebStore = 204,
        AllowedPaymentWithPOS = 205,
        AllowedPaymentWithCard = 206,
        AllowedPaymentWithLoyalty = 207,
        CardPaymentType = 208,
        CheckStatusTimer = 209,
        TermsAndConditionURL = 210,
        TermsAndConditionVersion = 211,
        OpenGate = 212,
        CloseGate = 213,
        PrivacyPolicyURL = 214,
        PrivacyPolicyVersion = 215,
        ShowCustomerSurvey = 216,
        AddCardBeforeShopping = 217,
        ShowCustomerQrCode = 218,

        //ScanPayGoPaymentFlags
        EnablePlatformPayment = 300,
        PlatformPaymentCurrencyCode = 301,
        PlatformPaymentCountryCode = 302,
        GooglePayGatewayJson = 303,
        GooglePayAllowedAuthMethodsJson = 304,
        GooglePayAllowedCardNetworksJson = 305,
        GooglePayMerchantName = 306,
        CardPaymentMethod = 307,
        LsPayServiceIpAddress = 308,
        LsPayServicePort = 309,
        LsPayPluginId = 310,

        //Alternate Logins
        //AudkenniFlags
        AudkenniBaseURL = 400,
        AudkenniClientId = 401,
        AudkenniRedirectURL = 402,
        AudkenniSecret = 403,
        AudkenniMessageToUser = 404,
        AudkenniLoginEnabled = 405,

        //Google
        GoogleLoginEnabled = 410,
        GoogleIosClientId = 411,

        //Facebook
        FacebookLoginEnabled = 420,

        //Apple
        AppleLoginEnabled = 430,
    }
}
