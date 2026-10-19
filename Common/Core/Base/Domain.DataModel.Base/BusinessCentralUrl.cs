using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    public class BusinessCentralUrl
    {
        private string scheme = "http";
        private string host = "";
        private string port = "";
        private string path = "";
        private string otherSettingsRaw = "";
        private bool isSaas = false;
        private Dictionary<string, string> queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private const string TenantParam = "tenant";
        private const string PageParam = "page";
        private const string CompanyParam = "company";
        private static readonly string[] SpecialKeys = { TenantParam, PageParam, CompanyParam };

        // Stores the most recent result for each segment
        private Dictionary<string, UrlSegmentInfo> segmentStatuses = new Dictionary<string, UrlSegmentInfo>(StringComparer.OrdinalIgnoreCase);

        private string tenant = "";
        private string clientType = ""; // e.g., "default.aspx", "tablet", etc.

        // Controls whether validations and error tracking are performed.
        // Default is true to preserve existing behaviour.
        private bool validationsEnabled = true;

        public bool ValidationsEnabled
        {
            get => validationsEnabled;
            set
            {
                validationsEnabled = value;
                if (!validationsEnabled)
                {
                    // clear any existing errors when disabling validations
                    Errors.Clear();
                }
            }
        }

        private static readonly string[] ValidClientTypes =
        {
            "default", "default.aspx",
            "phone", "phone.aspx",
            "tablet", "tablet.aspx"
        };

        public HashSet<UrlErrorCode> Errors { get; private set; } = new HashSet<UrlErrorCode>();
        public bool IsValid => Errors.Count == 0;

        public bool IsSaas
        {
            get => isSaas;
            set 
            { 
                isSaas = value;
                ValidateTenant();
            }
        }

        public string Tenant
        {
            get => tenant;
            set
            {
                tenant = value ?? string.Empty;
                ValidateTenant();
                ValidateFullUrl();
            }
        }

        public string Company
        {
            get => GetQueryParam(CompanyParam);
            set
            {
                SetQueryParam(CompanyParam, value);
            }
        }

        public string Page
        {
            get => GetQueryParam(PageParam);
            set
            {
                SetQueryParam(PageParam, value);
            }
        }

        // --- The OtherSettings Property ---

        public string OtherSettings
        {
            get => otherSettingsRaw;
            set
            {
                otherSettingsRaw = value ?? string.Empty;
                SyncDictionaryFromOtherSettings();
                ValidateOtherSettings();
            }
        }

        // --- Standard Properties ---

        public string Scheme
        {
            get => scheme;
            set
            {
                scheme = value.ToLower().Replace(":", "").Replace("/", "");
                ValidateScheme();
            }
        }

        public string Host
        {
            get => host;
            set 
            { 
                host = value ?? string.Empty;
                ValidateHost();
            }
        }

        public string Port
        {
            get => port;
            set 
            { 
                port = value ?? string.Empty;
                ValidatePort(); 
            }
        }

        public string Path
        {
            get => path;
            set
            {
                path = value?.Trim('/') ?? string.Empty;

                ValidatePath();
            }
        }

        public string ClientType
        {
            get => clientType;
            set 
            { 
                clientType = value ?? string.Empty; 
                ValidatePath(); 
            }
        }

        // --- Constructors ---

        public BusinessCentralUrl(string url, bool isSaas)
        {
            this.isSaas = isSaas;

            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                // --- PATH A: Standard Parsing ---
                this.scheme = uri.Scheme?.ToLower().Replace(":", "").Replace("/", "") ?? "http";
                this.host = uri.Host ?? string.Empty;
                this.port = url.Contains($"{uri.Host}:{uri.Port}") ? uri.Port.ToString() : string.Empty;
                ParseAbsoluteSegments(uri.AbsolutePath);
                ParseQuery(uri.Query);
            }
            else
            {
                // --- PATH B: Fallback Parsing (Manual Shred) ---
                // Even if Uri.TryCreate fails, we try to extract what we can
                ManualShred(url);
            }
        }

        // --- Validation Methods (Returning UrlSegmentInfo) ---

        public void ValidateOtherSettings()
        {
            if (!validationsEnabled)
                return;
            bool hasError = false;

            if (!string.IsNullOrWhiteSpace(otherSettingsRaw))
            {
                // Split by '&' to check each pair
                var pairs = otherSettingsRaw.Split('&');
                foreach (var pair in pairs)
                {
                    int equalIndex = pair.IndexOf('=');

                    // Invalid if: 
                    // - No '=' found (equalIndex == -1)
                    // - '=' is the first character (no key: "=value")
                    // - '=' is the last character (no value: "key=")
                    if (equalIndex <= 0 || equalIndex == pair.Length - 1)
                    {
                        hasError = true;
                        break;
                    }
                }
            }

            UpdateErrorList(UrlErrorCode.OtherSettingsFormat, hasError);

            segmentStatuses[nameof(OtherSettings)] = new UrlSegmentInfo
            {
                SegmentName = nameof(OtherSettings),
                Value = otherSettingsRaw,
                IsValid = !hasError,
                UrlErrorCode = hasError ? UrlErrorCode.OtherSettingsFormat : UrlErrorCode.None
            };
        }

        public void ValidateTenant()
        {
            if (!validationsEnabled)
                return;
            bool isMissing = string.IsNullOrWhiteSpace(Tenant);
            bool isError = isSaas && isMissing;

            // Sync the master Error list
            UpdateErrorList(UrlErrorCode.MissingSaasTenant, isError);

            // Cache the result for GetStatus(nameof(Tenant))
            segmentStatuses[nameof(Tenant)] = new UrlSegmentInfo
            {
                SegmentName = nameof(Tenant),
                Value = Tenant,
                IsValid = !isError,
                UrlErrorCode = isError ? UrlErrorCode.MissingSaasTenant : UrlErrorCode.None
            };
        }

        public void ValidateScheme()
        {
            if (!validationsEnabled)
                return;
            bool isValid = (scheme == "http" || scheme == "https");
            UpdateErrorList(UrlErrorCode.MissingInternetProtocol, !isValid);

            segmentStatuses[nameof(Scheme)] = new UrlSegmentInfo
            {
                SegmentName = nameof(Scheme),
                Value = scheme,
                IsValid = isValid,
                UrlErrorCode = isValid ? UrlErrorCode.None : UrlErrorCode.MissingInternetProtocol
            };
        }

        public void ValidateHost()
        {
            if (!validationsEnabled)
                return;
            bool isMissing = string.IsNullOrWhiteSpace(host);
            bool hasSpaces = !isMissing && host.Contains(" ");
            bool isMalformed = !isMissing && Uri.CheckHostName(host) == UriHostNameType.Unknown;

            UpdateErrorList(UrlErrorCode.MissingComputerName, isMissing);
            UpdateErrorList(UrlErrorCode.ComputerNameContainsInvalidCharacters, hasSpaces || isMalformed);

            segmentStatuses[nameof(Host)] = new UrlSegmentInfo
            {
                SegmentName = nameof(Host),
                Value = host,
                IsValid = !isMissing && !hasSpaces && !isMalformed,
                UrlErrorCode = isMissing ? UrlErrorCode.MissingComputerName :
                              (hasSpaces || isMalformed ? UrlErrorCode.ComputerNameContainsInvalidCharacters : UrlErrorCode.None)
            };
        }

        public void ValidatePort()
        {
            if (!validationsEnabled)
                return;
            bool isInvalid = !string.IsNullOrWhiteSpace(port) &&
                     (!int.TryParse(port, out int p) || p < 1 || p > 65535);

            // Sync the master Error list
            UpdateErrorList(UrlErrorCode.PortIsNotValidNumber, isInvalid);

            // Update the cache for GetStatus()
            segmentStatuses[nameof(Port)] = new UrlSegmentInfo
            {
                SegmentName = nameof(Port),
                Value = port,
                IsValid = !isInvalid,
                UrlErrorCode = isInvalid ? UrlErrorCode.PortIsNotValidNumber : default
            };
        }

        public void ValidatePath()
        {
            if (!validationsEnabled)
                return;
            bool isMissing = string.IsNullOrWhiteSpace(path);
            bool hasSpaces = !isMissing && path.Contains(" ");

            // Check for malformed URL characters (Let the .NET Uri class do the heavy lifting)
            bool isMalformed = !isMissing && !Uri.TryCreate($"http://localhost/{path}", UriKind.Absolute, out _);
            UpdateErrorList(UrlErrorCode.MissingEnvironmentName, isMissing);
            UpdateErrorList(UrlErrorCode.EnvironmentNameContainsInvalidCharacters, hasSpaces || isMalformed);

            segmentStatuses[nameof(Path)] = new UrlSegmentInfo
            {
                SegmentName = nameof(Path),
                Value = path,
                IsValid = !isMissing && !hasSpaces && !isMalformed,
                UrlErrorCode = isMissing ? UrlErrorCode.MissingEnvironmentName :
                              (hasSpaces || isMalformed ? UrlErrorCode.EnvironmentNameContainsInvalidCharacters : UrlErrorCode.None)
            };
        }

        // --- Helper Logic ---

        public bool IsTabletClientMode => clientType.Equals("tablet", StringComparison.OrdinalIgnoreCase) || clientType.Equals("tablet.aspx", StringComparison.OrdinalIgnoreCase);

        public UrlSegmentInfo GetStatus(string propertyName) => segmentStatuses.TryGetValue(propertyName, out var info) ? info : new UrlSegmentInfo { SegmentName = propertyName, IsValid = true };

        private void ManualShred(string url)
        {
            // 1. Extract Scheme (look for ://)
            int schemeEnd = url.IndexOf("://");
            if (schemeEnd > 0)
            {
                this.scheme = url.Substring(0, schemeEnd);
                url = url.Substring(schemeEnd + 3);
            }

            // 2. Extract Query (look for ?)
            int queryStart = url.IndexOf('?');
            if (queryStart >= 0)
            {
                string query = url.Substring(queryStart);
                ParseQuery(query);
                url = url.Substring(0, queryStart);
            }

            // 3. Extract Host/Port vs Path (look for first /)
            int pathStart = url.IndexOf('/');
            string hostAndPort = pathStart >= 0 ? url.Substring(0, pathStart) : url;
            string remainingPath = pathStart >= 0 ? url.Substring(pathStart) : "";

            // 4. Split Host and Port
            int portStart = hostAndPort.LastIndexOf(':');
            if (portStart >= 0 && !hostAndPort.EndsWith("]")) // Ensure it's not an IPv6 address
            {
                this.host = hostAndPort.Substring(0, portStart);
                this.port = hostAndPort.Substring(portStart + 1);
            }
            else
            {
                this.host = hostAndPort;
            }

            // 5. Handle the Path segments
            if (!string.IsNullOrEmpty(remainingPath))
            {
                ParseAbsoluteSegments(remainingPath);
            }
        }

        private void ParseAbsoluteSegments(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath)) return;

            // Split into segments, keeping empty entries to catch "double slash" errors
            //var segments = absolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var segments = absolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(s => WebUtility.UrlDecode(s)) // Turn %20 back into " "
                  .ToList();

            // 1. Peel the Client Type (The "Reserved Word" at the end)
            if (segments.Count > 0)
            {
                string last = segments.Last(); // No Trim() - keep it raw for validation
                if (ValidClientTypes.Contains(last.ToLower()))
                {
                    this.clientType = last;
                    segments.RemoveAt(segments.Count - 1);
                }
            }

            // 2. Map remaining segments based on isSaas
            if (this.isSaas)
            {
                // SaaS Pattern: /Tenant/Instance
                if (segments.Count >= 2)
                {
                    this.tenant = segments[0] ?? string.Empty;
                    this.path = segments[1]?.Trim('/') ?? string.Empty;
                }
                else if (segments.Count == 1)
                {
                    // If there's a single segment in SaaS mode it is expected to be the Tenant (GUID).
                    // If it's not a GUID, treat it as the instance Path instead.
                    if (Guid.TryParse(segments[0], out _))
                    {
                        this.tenant = segments[0] ?? string.Empty;
                        this.path = string.Empty; // Path is missing but we have a tenant
                    }
                    else
                    {
                        // Not a GUID -> this is actually the Path (instance name)
                        this.path = segments[0]?.Trim('/') ?? string.Empty;
                    }
                }
            }
            else
            {
                // On-Prem Pattern: /Instance
                // Everything left is part of the path (Instance Name)
                if (segments.Count >= 1)
                {
                    this.path = string.Join("/", segments);
                }
            }
        }

        private void SyncDictionaryFromOtherSettings()
        {
            var keysToRemove = queryParams.Keys
                .Where(k => !SpecialKeys.Contains(k.ToLower()))
                .ToList();
            foreach (var key in keysToRemove)
            {
                queryParams.Remove(key);
            }
            ParseQuery(otherSettingsRaw, append: true);
        }

        private void ParseQuery(string query, bool append = false)
        {
            if (!append)
            {
                queryParams.Clear();
            }
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            string cleanQuery = query.TrimStart('?');
            string[] pairs = cleanQuery.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string pair in pairs)
            {
                string[] parts = pair.Split(new[] { '=' }, 2);
                if (parts.Length > 0)
                {
                    // NO TRIM: If the user typed " company", the key is " company"
                    string key = parts[0];
                    string val = parts.Length > 1 ? WebUtility.UrlDecode(parts[1]) : string.Empty;

                    if (!string.IsNullOrEmpty(key))
                    {
                        // Note: Comparison for SpecialKeys (tenant, page, company) 
                        // should probably still be case-insensitive and trimmed 
                        // ONLY for the check, not for the storage.
                    string searchKey = key.Trim().ToLower();

                        if (searchKey == TenantParam)
                        {
                            if (!append)
                                this.tenant = val;
                            continue; // never store tenant in queryParams
                        }

                        if (append && SpecialKeys.Contains(searchKey))
                        {
                            continue;
                        }

                        queryParams[key] = val;
                    }
                }
            }
        }

        private void SetQueryParam(string key, string value)
        {
            queryParams[key] = value;
            ValidateFullUrl();
        }

        private string GetQueryParam(string key) => queryParams.TryGetValue(key, out var value) ? value : null;

        public void RunAllValidations()
        {
            if (!validationsEnabled)
                return;
            ValidateScheme();
            ValidateHost();
            ValidatePort();
            ValidatePath();
            ValidateTenant();
            ValidateOtherSettings();
            ValidateFullUrl();
        }

        public void ValidateFullUrl()
        {
            if (!validationsEnabled)
            {
                // When validations are disabled, ensure InvalidUrlFormat is not present
                UpdateErrorList(UrlErrorCode.InvalidUrlFormat, false);
                return;
            }
            string currentUrl = this.ToString();

            // 1. If the URL is empty, we don't flag "Format" errors; 
            // we let the specific segment validations (Host, Path) speak for themselves.
            if (string.IsNullOrEmpty(currentUrl))
            {
                UpdateErrorList(UrlErrorCode.InvalidUrlFormat, false);
                return;
            }

            try
            {
                // 2. The "Ultimate Authority" check
                // We create a new Uri. If this fails, it catches edge cases like 
                // "http://host:port/path" where port is "abc" or contains illegal symbols.
                var finalCheck = new Uri(currentUrl, UriKind.Absolute);

                // If we got here, the URL is technically "parsable" by the OS.
                UpdateErrorList(UrlErrorCode.InvalidUrlFormat, false);
            }
            catch (UriFormatException)
            {
                // This catches anything our manual logic missed.
                UpdateErrorList(UrlErrorCode.InvalidUrlFormat, true);
            }
            catch (Exception)
            {
                // General safety for unexpected overflows or null refs
                UpdateErrorList(UrlErrorCode.InvalidUrlFormat, true);
            }
        }

        private void UpdateErrorList(UrlErrorCode code, bool shouldAdd)
        {
            if (!validationsEnabled)
                return;

            if (shouldAdd)
            {
                Errors.Add(code);
            }
            else
            {
                Errors.Remove(code);
            }
        }

        public override string ToString()
        {
            // 1. Base URL construction
            string pStr = "";
            if (!string.IsNullOrWhiteSpace(port))
            {
                pStr = $":{port}";
            }

            // 2. Path construction (/Tenant/Instance/ClientType)
            string finalPath = (isSaas && !string.IsNullOrWhiteSpace(Tenant)) ? $"/{Tenant}/{path.Trim('/')}" : $"/{path.Trim('/')}";

            if (!string.IsNullOrWhiteSpace(clientType))
            {
                finalPath = $"{finalPath.TrimEnd('/')}/{clientType.TrimStart('/')}";
            }

            // 3. Query Parameter Logic
            var finalQueryParams = new List<string>();

            // Add Special Parameters (Tenant for On-Prem, Page, Company)
            if (!isSaas && !string.IsNullOrWhiteSpace(Tenant))
                finalQueryParams.Add($"{TenantParam}={Uri.EscapeDataString(Tenant)}");

            if (!string.IsNullOrWhiteSpace(Page))
                finalQueryParams.Add($"{PageParam}={Uri.EscapeDataString(Page)}");

            if (!string.IsNullOrWhiteSpace(Company))
                finalQueryParams.Add($"{CompanyParam}={Uri.EscapeDataString(Company)}");

            // Add OtherSettings
            if (!string.IsNullOrWhiteSpace(otherSettingsRaw))
                finalQueryParams.Add(otherSettingsRaw);

            // --- The redirect=0 Logic ---
            // Check if a parameter with the key 'redirect' already exists in our final list (case-insensitive)
            // Use key comparison instead of substring matching so parameters like 'signinRedirect' do not match.
            bool hasRedirect = finalQueryParams.Any(p =>
            {
                if (string.IsNullOrEmpty(p))
                    return false;
                var idx = p.IndexOf('=');
                var key = idx >= 0 ? p.Substring(0, idx) : p;
                return string.Equals(key?.Trim(), "redirect", StringComparison.OrdinalIgnoreCase);
            });
            if (!hasRedirect)
            {
                finalQueryParams.Add("redirect=0");
            }
            // 4. Final Assembly
            string queryStr = finalQueryParams.Count > 0 ? "?" + string.Join("&", finalQueryParams) : "";

            if(string.IsNullOrWhiteSpace(host) && string.IsNullOrWhiteSpace(pStr))
            {
                return "";
            }
            return $"{scheme}://{host}{pStr}{finalPath}{queryStr}";
        }
    }

    public enum PageExtension
    {
        Empty = 0,
        Default,
        DefaultAspx,
        Phone,
        PhoneAspx,
        Tablet,
        TabletAspx,
    }

    public enum UrlErrorCode
    {
        None,
        UrlIsRequired,
        MissingComputerName,
        MissingEnvironmentName,
        MissingInternetProtocol,
        InvalidUrlFormat,
        MissingSaasTenant,
        OtherSettingsFormat,
        ComputerNameContainsInvalidCharacters,
        EnvironmentNameContainsInvalidCharacters,        
        PortIsNotValidNumber
    }

    public class UrlSegmentInfo
    {
        public string SegmentName { get; set; }
        public string Value { get; set; }
        public bool IsValid { get; set; }
        public UrlErrorCode UrlErrorCode { get; set; }

        public static UrlSegmentInfo Valid(string segmentName, string value)
        {
            return new UrlSegmentInfo
            {
                SegmentName = segmentName,
                Value = value,
                IsValid = true,
                UrlErrorCode = UrlErrorCode.None
            };
        }
    }

}