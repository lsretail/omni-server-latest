using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
    /// <summary>
    /// The backend a scanned settings QR code configures. Detected from the keys the payload carries,
    /// never from the mode the device is currently in.
    /// </summary>
    public enum SettingsQrPayloadKind
    {
        Unknown = 0,
        BusinessCentral = 1,
        OmniService = 2
    }

    public enum SettingsQrParseStatus
    {
        Ok = 0,
        NotJson = 1,
        UnknownPayload = 2,
        MissingMandatoryFields = 3
    }

    /// <summary>
    /// The values a settings QR code carries. Every optional numeric field is nullable because a
    /// scanned zero is a legal value that must stay distinguishable from an absent key - an absent
    /// key leaves the stored setting alone, a scanned zero overwrites it.
    /// </summary>
    public class SettingsQrPayload
    {
        public SettingsQrPayloadKind Kind { get; set; }

        public string LsCentralWsUrl { get; set; }
        public string LsCentralODataUrl { get; set; }
        public string LsCentralTenantId { get; set; }
        public string LsCentralClientId { get; set; }
        public string LsCentralClientSecret { get; set; }
        public string LsCentralProtocol { get; set; }
        public string LsCentralCompanyId { get; set; }
        public int? LsCentralTimeout { get; set; }

        public string Url { get; set; }
        public string StoreId { get; set; }
        public string TerminalId { get; set; }
        public string LsKey { get; set; }
        public int? Timeout { get; set; }
        public int? UpdateFrequency { get; set; }
        public int? BatchSize { get; set; }
    }

    /// <summary>
    /// The outcome of reading a scanned payload. <see cref="MissingFieldKeys"/> holds JSON key names,
    /// never localized text, so the reader stays free of any presentation dependency.
    /// </summary>
    public class SettingsQrPayloadResult
    {
        public SettingsQrParseStatus Status { get; set; }
        public SettingsQrPayload Payload { get; set; }
        public IReadOnlyList<string> MissingFieldKeys { get; set; }
    }

    /// <summary>
    /// Reads the JSON payload of a scanned settings QR code: detects which backend it configures,
    /// parses its fields and reports the mandatory ones it is missing. Pure - no preferences, no
    /// platform types, no network - so the whole detect/parse/validate rule set is unit testable.
    /// </summary>
    public static class SettingsQrPayloadReader
    {
        public const string LsCentralWsUrlKey = "LSCentralWsUrl";
        public const string LsCentralODataUrlKey = "LSCentralODataUrl";
        public const string LsCentralTenantIdKey = "LSCentralTenantId";
        public const string LsCentralClientIdKey = "LSCentralClientId";
        public const string LsCentralClientSecretKey = "LSCentralClientSecret";
        public const string LsCentralProtocolKey = "LSCentralProtocol";
        public const string LsCentralCompanyIdKey = "LSCentralCompanyId";
        public const string LsCentralTimeoutKey = "LSCentralTimeout";

        public const string UrlKey = "Url";
        public const string StoreIdKey = "StoreId";
        public const string TerminalIdKey = "TerminalId";
        public const string LsKeyKey = "LsKey";
        public const string TimeoutKey = "Timeout";
        public const string UpdateFrequencyKey = "UpdateFrequency";
        public const string BatchSizeKey = "BatchSize";

        /// <summary>
        /// Reads scanned QR data into a settings payload.
        /// </summary>
        /// <param name="scanData">The raw scanned string. Anything that is not a JSON object - empty,
        /// null, plain text, an array, truncated JSON - degrades to <see cref="SettingsQrParseStatus.NotJson"/>
        /// rather than throwing.</param>
        /// <param name="saasProtocolValue">The caller's marker for "this protocol string means SaaS".
        /// It only decides whether the tenant id is mandatory; the protocol itself is never normalized
        /// or rewritten, so no platform-specific protocol value has to live in shared code.</param>
        public static SettingsQrPayloadResult Read(string scanData, string saasProtocolValue)
        {
            var payload = new SettingsQrPayload { Kind = SettingsQrPayloadKind.Unknown };

            var json = ParseObject(scanData);

            if (json == null)
            {
                return BuildResult(SettingsQrParseStatus.NotJson, payload, new List<string>());
            }

            //detection is on key presence, validation is on a non-empty value: a payload carrying an
            //empty Url still belongs to OmniService, and must say Url is missing rather than that the
            //whole code is unrecognized
            if (json.ContainsKey(LsCentralWsUrlKey))
            {
                return ReadBusinessCentral(json, payload, saasProtocolValue);
            }

            if (json.ContainsKey(UrlKey))
            {
                return ReadOmniService(json, payload);
            }

            return BuildResult(SettingsQrParseStatus.UnknownPayload, payload, new List<string>());
        }

        private static SettingsQrPayloadResult ReadBusinessCentral(JObject json, SettingsQrPayload payload, string saasProtocolValue)
        {
            payload.Kind = SettingsQrPayloadKind.BusinessCentral;

            payload.LsCentralWsUrl = ReadString(json, LsCentralWsUrlKey);
            payload.LsCentralODataUrl = ReadString(json, LsCentralODataUrlKey);
            payload.LsCentralTenantId = ReadString(json, LsCentralTenantIdKey);
            payload.LsCentralClientId = ReadString(json, LsCentralClientIdKey);
            payload.LsCentralClientSecret = ReadString(json, LsCentralClientSecretKey);
            payload.LsCentralProtocol = ReadString(json, LsCentralProtocolKey);
            payload.LsCentralCompanyId = ReadString(json, LsCentralCompanyIdKey);
            payload.LsCentralTimeout = ReadInt(json, LsCentralTimeoutKey);

            //the replication settings are not OmniService-exclusive - they drive the replication call and
            //its schedule whichever backend is registered - so a BC code may carry them too and provision
            //a device in one scan. They stay optional here, unlike on OmniService where store and terminal
            //are mandatory, so every BC code already in the field keeps working unchanged.
            payload.StoreId = ReadString(json, StoreIdKey);
            payload.TerminalId = ReadString(json, TerminalIdKey);
            payload.UpdateFrequency = ReadInt(json, UpdateFrequencyKey);
            payload.BatchSize = ReadInt(json, BatchSizeKey);

            //walked in schema order, not in the order the JSON document happens to list its keys, so the
            //error message reads the same for every QR generator
            var missing = new List<string>();

            AddIfBlank(missing, LsCentralWsUrlKey, payload.LsCentralWsUrl);
            AddIfBlank(missing, LsCentralODataUrlKey, payload.LsCentralODataUrl);
            AddIfBlank(missing, LsCentralClientIdKey, payload.LsCentralClientId);
            AddIfBlank(missing, LsCentralClientSecretKey, payload.LsCentralClientSecret);

            //an on-prem service has no tenant, and the settings screen hides the field for it, so the
            //tenant is only mandatory once the payload resolves to SaaS
            if (ResolvesToSaas(payload.LsCentralProtocol, saasProtocolValue))
            {
                AddIfBlank(missing, LsCentralTenantIdKey, payload.LsCentralTenantId);
            }

            return BuildResult(StatusFor(missing), payload, missing);
        }

        private static SettingsQrPayloadResult ReadOmniService(JObject json, SettingsQrPayload payload)
        {
            payload.Kind = SettingsQrPayloadKind.OmniService;

            payload.Url = ReadString(json, UrlKey);
            payload.StoreId = ReadString(json, StoreIdKey);
            payload.TerminalId = ReadString(json, TerminalIdKey);
            payload.LsKey = ReadString(json, LsKeyKey);
            payload.Timeout = ReadInt(json, TimeoutKey);
            payload.UpdateFrequency = ReadInt(json, UpdateFrequencyKey);
            payload.BatchSize = ReadInt(json, BatchSizeKey);

            var missing = new List<string>();

            AddIfBlank(missing, UrlKey, payload.Url);
            AddIfBlank(missing, StoreIdKey, payload.StoreId);
            AddIfBlank(missing, TerminalIdKey, payload.TerminalId);

            return BuildResult(StatusFor(missing), payload, missing);
        }

        /// <summary>
        /// A blank protocol counts as SaaS, the same default the settings screen applies, so QR codes
        /// written before on-prem support existed keep working unchanged.
        /// </summary>
        private static bool ResolvesToSaas(string protocol, string saasProtocolValue)
        {
            if (string.IsNullOrWhiteSpace(protocol))
                return true;

            return protocol.Equals(saasProtocolValue, StringComparison.OrdinalIgnoreCase);
        }

        private static JObject ParseObject(string scanData)
        {
            if (string.IsNullOrWhiteSpace(scanData))
                return null;

            try
            {
                return JObject.Parse(scanData);
            }
            catch (Exception)
            {
                //anything that is not a JSON object - plain text, an array, truncated JSON - is simply
                //not a settings payload, and the caller only needs to know that much
                return null;
            }
        }

        private static string ReadString(JObject json, string key)
        {
            JToken token;

            if (!json.TryGetValue(key, out token) || token == null || token.Type == JTokenType.Null)
                return null;

            if (token.Type == JTokenType.Object || token.Type == JTokenType.Array)
                return null;

            return token.ToString();
        }

        /// <summary>
        /// Converts one optional numeric field, guarded on its own. A QR code carrying a wrong-shaped
        /// value for a single optional field only loses that field - it must not turn the whole payload
        /// into an unreadable one.
        /// </summary>
        private static int? ReadInt(JObject json, string key)
        {
            try
            {
                JToken token;

                if (!json.TryGetValue(key, out token) || token == null || token.Type == JTokenType.Null)
                    return null;

                if (token.Type == JTokenType.Integer)
                    return (int)token;

                //not every QR generator writes numbers as JSON numbers
                int parsed;
                if (int.TryParse(token.ToString(), out parsed))
                    return parsed;

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void AddIfBlank(List<string> missing, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                missing.Add(key);
            }
        }

        private static SettingsQrParseStatus StatusFor(List<string> missing)
        {
            return missing.Count > 0 ? SettingsQrParseStatus.MissingMandatoryFields : SettingsQrParseStatus.Ok;
        }

        private static SettingsQrPayloadResult BuildResult(SettingsQrParseStatus status, SettingsQrPayload payload, List<string> missing)
        {
            return new SettingsQrPayloadResult
            {
                Status = status,
                Payload = payload,
                MissingFieldKeys = missing
            };
        }
    }
}
