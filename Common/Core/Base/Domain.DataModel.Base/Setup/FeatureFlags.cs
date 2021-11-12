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
        None,
        AllowAutoLogoff,
        AutoLogOffAfterMin,
        AllowOffline,
        ExitAfterEachTransaction,
        SendReceiptInEmail,
        ShowNumberPad,
        UseLoyaltySystem,
        PosShowInventory,
        PosInventoryLookup,
        SettingsPassword,
        HideVoidedTransaction,
        AllowCentralLogin,

        //ScanPayGo
        AllowAnonymousUser,
        AllowShopHome,
        DeviceType,
        CatalogType,
        DefaultWebStore,
        AllowedPaymentWithPOS,
        AllowedPaymentWithCard,
        AllowedPaymentWithLoyalty,
        CardPaymentType,
        CheckStatusTimer,
        TermsAndConditionURL,
        TermsAndConditionVersion,

        //ScanPayGoPaymentFlags
        EnablePlatformPayment,
        PlatformPaymentCurrencyCode,
        PlatformPaymentCountryCode,
        GooglePayGatewayJson,
        GooglePayAllowedAuthMethodsJson,
        GooglePayAllowedCardNetworksJson,
        GooglePayMerchantName,

        //AudkenniFlags
        AudkenniBaseURL,
        AudkenniClientId,
        AudkenniRedirectURL,
        AudkenniSecret,
        AudkenniMessageToUser,
        AudkenniLoginEnabled
    }
}
