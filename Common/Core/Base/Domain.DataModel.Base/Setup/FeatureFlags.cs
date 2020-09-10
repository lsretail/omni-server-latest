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
                    return defaultValue;
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
        HideVoidedTransaction
    }
}
