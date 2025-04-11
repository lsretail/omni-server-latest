using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
    public enum SettingsConfigUrlType
    {
        Normal = 0,
        Simplified = 1,
    }

    public enum SettingsConfigSecurityStandard
    {
        Http = 0,
        Https = 1,
    }

    public enum SettingsConfigType
    {
        WebClient = 0,
        ExternalApplication = 1,
        Folder = 2,
        PowerShell = 3,
        RemoteConfiguration = 4,
    }

    public enum SettingsConfigServiceStatus
    {
        None = 0,
        Offline = 1,
        Partial = 2,
        Online = 3,
    }

    public enum SettingsConfigServiceType
    {
        HardwareStation = 0,
        ServiceTier = 1,
    }

    public enum DevToolsBehaviorEnum
    {
        Disabled = 0, // old OpenDevTools false
        Enabled = 1,
        OpenOnStart = 2,// old OpenDevTools true
    }

    public class AppConfig
    {
        [System.Xml.Serialization.XmlElementAttribute("AppSettings")]
        public AppSettings AppSettings { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("SettingsConfigs")]
        public List<SettingsConfig> SettingsConfigs { get; set; }
    }

    public class AppSettings : Entity
    {
        private bool autoUpdate;
        private string autoUpdatePath;
        private int serviceUpdateTimer;
        private bool windowFullScreen;

        private bool requireAuthentication;
        private string settingsUsername;
        private string settingsPassword;

        private bool useFixedSizeGrid;
        private int fixedSizeGridRows;
        private int fixedSizeGridColumns;
        private int fontSize;

        private bool showHardwareOverlay;
        private DevToolsBehaviorEnum devToolsBehavior = DevToolsBehaviorEnum.Disabled;
        private bool toggleFullScreen;
        private bool hardwareOverlayToLog;
        private int hardwareMinutes;
        private bool consoleLogToLog;

        private bool ignoreCertificateErrors;

        private string webView2RuntimeLocation;

        public const string AutoUpdateKey = "AutoUpdateKey";
        public const string AutoUpdatePathKey = "AutoUpdatePathKey";
        public const string ServiceUpdateTimerKey = "ServiceUpdateTimerKey";
        public const string WindowFullScreenKey = "WindowFullScreenKey";
        public const string RequireAuthenticationKey = "RequireAuthenticationKey";
        public const string SettingsUsernameKey = "SettingsUsernameKey";
        public const string SettingsPasswordKey = "SettingsPasswordKey";

        public const string UseFixedSizeGridKey = "UseFixedSizeGridKey";
        public const string FixedSizeGridRowsKey = "FixedSizeGridRowsKey";
        public const string FixedSizeGridColumnsKey = "FixedSizeGridColumnsKey";
        public const string IsFullScreen = "IsFullScreen";
        public const string AllowToggleFullScreen = "AllowToggleFullScreen";

        
        public const string ShowHardwareOverlayKey = "ShowHardwareOverlayKey";
        public const string DevToolsBehaviorKey = "DevToolsBehaviorKey";
        public const string FontSizeKey = "FontSizeKey";
        public const string HardwareOverlayToLogKey = "HardwareOverlayToLogKey";
        public const string ConsoleLogToLogKey = "ConsoleLogToLogKey";
        public const string HardwareMinutesKey = "HardwareMinutesKey";

        public const string IgnoreCertificateErrorsKey = "IgnoreCertificateErrorsKey";

        public const string WebView2RuntimeLocationKey = "WebView2RuntimeLocationKey";

        [System.Xml.Serialization.XmlElementAttribute("AutoUpdate")]
        public bool AutoUpdate
        {
            get => autoUpdate;
            set
            {
                autoUpdate = value;

                if (autoUpdate)
                {
                    AutoUpdatePath = string.Empty;
                }

                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("AutoUpdatePath")]
        public string AutoUpdatePath
        {
            get => autoUpdatePath;
            set
            {
                autoUpdatePath = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("ServiceUpdateTimer")]
        public int ServiceUpdateTimer
        {
            get => serviceUpdateTimer;
            set
            {
                serviceUpdateTimer = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("LSStartFullScreen")]
        public bool WindowFullScreen
        {
            get => windowFullScreen;
            set
            {
                windowFullScreen = value;
                NotifyPropertyChanged();
            }
        }

        public bool ToggleFullScreen
        {
            get => toggleFullScreen;
            set
            {
                toggleFullScreen = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("RequireAuthentication")]
        public bool RequireAuthentication
        {
            get => requireAuthentication;
            set
            {
                requireAuthentication = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("SettingsUsername")]
        public string SettingsUsername
        {
            get => settingsUsername;
            set
            {
                settingsUsername = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("UseFixedSizeGrid")]
        public bool UseFixedSizeGrid
        {
            get => useFixedSizeGrid;
            set
            {
                useFixedSizeGrid = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("HardwareOverlayToLog")]
        public bool HardwareOverlayToLog
        {
            get => hardwareOverlayToLog;
            set
            {
                hardwareOverlayToLog = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("ConsoleLogToLog")]
        public bool ConsoleLogToLog
        {
            get => consoleLogToLog;
            set
            {
                consoleLogToLog = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("FixedSizeGridRows")]
        public int FixedSizeGridRows
        {
            get => fixedSizeGridRows;
            set
            {
                fixedSizeGridRows = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("FontSize")]
        public int FontSize
        {
            get => fontSize;
            set
            {
                fontSize = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("HardwareMinutes")]
        public int HardwareMinutes
        {
            get => hardwareMinutes;
            set
            {
                hardwareMinutes = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("FixedSizeGridColumns")]
        public int FixedSizeGridColumns
        {
            get => fixedSizeGridColumns;
            set
            {
                fixedSizeGridColumns = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("ShowHardwareOverlay")]
        public bool ShowHardwareOverlay
        {
            get => showHardwareOverlay;
            set
            {
                showHardwareOverlay = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("DevToolsBehavior")]
        public DevToolsBehaviorEnum DevToolsBehavior
        {
            get => devToolsBehavior;
            set
            {
                devToolsBehavior = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("SettingsPassword")]
        public string SettingsPassword
        {
            get => settingsPassword;
            set
            {
                settingsPassword = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("IgnoreCertificateErrors")]
        public bool IgnoreCertificateErrors
        {
            get => ignoreCertificateErrors;
            set
            {
                ignoreCertificateErrors = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("WebView2RuntimeLocation")]
        public string WebView2RuntimeLocation
        {
            get => webView2RuntimeLocation;
            set
            {
                webView2RuntimeLocation = value;
                NotifyPropertyChanged();
            }
        }

        public AppSettings ShallowCopy()
        {
            return (AppSettings) MemberwiseClone();
        }
    }

    [Serializable]
    [System.Xml.Serialization.XmlRoot("AppConfiguration")]
    public class SettingsConfig : Entity
    {
        private string description;
        private string folderId;
        private string backupId;
        private string url;
        private int timeout;
        private string lsKey;
        private string storeId;
        private string terminalId;
        private int batchSize;
        private int updateFrequency;
        private SettingsConfigSecurityStandard securityStandard;
        private SettingsConfigType settingsConfigType;
        private string computerName;
        private string port;
        private string webServiceInstance;
        private string page;
        private string company;
        private bool isSaas;
        private string tenant;
        private SettingsConfigUrlType urlType;
        private string userName;
        private string password;
        private string hexColor;
        private int displayOrder;
        private string parameters;
        private bool allowLegacyEdge;

        private string hardwareStationServiceName;
        private bool hardwareStationRemoteComputer;
        private string hardwareStationComputer;
        private string hardwareStationPort;
        private string hardwareStationUsername;
        private string hardwareStationPassword;

        private string serviceTierServiceName;
        private bool serviceTierRemoteComputer;
        private string serviceTierComputer;
        private string serviceTierPort;
        private string serviceTierUsername;
        private string serviceTierPassword;

        private SettingsConfigServiceStatus serviceStatus;
        private string serviceStatusMessage;

        private int level;

        private int gridRow;
        private int gridColumn;

        private static SettingsConfig emptyConfig;
        private string otherParameters;

        private string lsCentralWsUrl = string.Empty;
        private string lsCentralOdataUrl = string.Empty;
        private string lsCentralTenantId = string.Empty;
        private string lsCentralClientId = string.Empty;
        private string lsCentralClientSecret = string.Empty;
        private int lsCentralTimeout;

        public SettingsConfig(string id) : base(id)
        {
            Init();
        }

        public SettingsConfig() : base()
        {
            Init();
        }

        public SettingsConfig(SettingsConfig config)
        {
            this.description = config.description;
            this.folderId = config.folderId;
            this.backupId = config.backupId;
            this.url = config.url;
            this.timeout = config.timeout;
            this.lsKey = config.lsKey;
            this.storeId = config.storeId;
            this.terminalId = config.terminalId;
            this.batchSize = config.batchSize;
            this.updateFrequency = config.updateFrequency;
            this.securityStandard = config.securityStandard;
            this.settingsConfigType = config.settingsConfigType;
            this.computerName = config.computerName;
            this.port = config.port;
            this.webServiceInstance = config.webServiceInstance;
            this.page = config.page;
            this.company = config.company;
            this.tenant = config.tenant;
            this.urlType = config.urlType;
            this.userName = config.userName;
            this.password = config.password;
            this.hexColor = config.hexColor;
            this.parameters = config.parameters;
            this.allowLegacyEdge = config.allowLegacyEdge;
            this.gridRow = config.gridRow;
            this.gridColumn = config.gridColumn;
            EftSelectedDevice = config.EftSelectedDevice;
            EftIpAddress = config.EftIpAddress;
            EftMainPort = config.EftMainPort;
            EftSecondaryPort = config.EftSecondaryPort;
            EftTimeout = config.EftTimeout;
            EftMerchantId = config.EftMerchantId;
            EftTerminalId = config.EftTerminalId;
            EftReceiptLineWidth = config.EftReceiptLineWidth;
            EftUserFieldOnReceipt = config.EftUserFieldOnReceipt;
            EftSoftwareVersion = config.EftSoftwareVersion;
            EftSoftwareName = config.EftSoftwareName;
            EftSoftwareManufacturer = config.EftSoftwareManufacturer;
            EftSerialNumber = config.EftSerialNumber;
            EftPassword = config.EftPassword;
            EftHostAddress = config.EftHostAddress;
            EftEnvironment = config.EftEnvironment;
            EftApiKey = config.EftApiKey;
            EftSaleId = config.EftSaleId;
            EftConnection = config.EftConnection;
            EftKeyIdentifier = config.EftKeyIdentifier;
            EftVersion = config.EftVersion;
            PrinterSelectedDevice = config.PrinterSelectedDevice;
            PrinterPrintWidth = config.PrinterPrintWidth;
            PrinterNetworkAddress = config.PrinterNetworkAddress;
            PrinterModel = config.PrinterModel;
            PrinterLanguage = config.PrinterLanguage;
            PreventClosing = config.PreventClosing;
            RunOnStartup = config.RunOnStartup;
            ShowBackButton = config.ShowBackButton;
            OpenInNewWindow = config.OpenInNewWindow;

            hardwareStationServiceName = config.hardwareStationServiceName;
            hardwareStationPort = config.hardwareStationPort;
            hardwareStationRemoteComputer = config.hardwareStationRemoteComputer;
            hardwareStationComputer = config.hardwareStationComputer;
            hardwareStationUsername = config.hardwareStationUsername;
            hardwareStationPassword = config.hardwareStationPassword;

            serviceTierServiceName = config.serviceTierServiceName;
            serviceTierPort = config.serviceTierPort;
            serviceTierRemoteComputer = config.serviceTierRemoteComputer;
            serviceTierComputer = config.serviceTierComputer;
            serviceTierUsername = config.serviceTierUsername;
            serviceTierPassword = config.serviceTierPassword;

            lsCentralWsUrl = config.lsCentralWsUrl;
            lsCentralOdataUrl = config.lsCentralOdataUrl;
            lsCentralTenantId = config.lsCentralTenantId;
            lsCentralClientId = config.lsCentralClientId;
            lsCentralClientSecret = config.lsCentralClientSecret;
        }

        private void Init()
        {
            Description = "";
            folderId = "";
            backupId = "";
            UserName = "";
            Password = "";
            Url = string.Empty;
            ComputerName = string.Empty;
            Port = string.Empty;
            WebServiceInstance = string.Empty;
            Page = string.Empty;
            Company = string.Empty;
            Tenant = string.Empty;
            Timeout = 0;
            LsKey = string.Empty;
            StoreId = string.Empty;
            TerminalId = string.Empty;
            BatchSize = 0;
            UpdateFrequency = 0;
            EftSelectedDevice = 0;
            EftIpAddress = string.Empty;
            EftMainPort = string.Empty;
            EftSecondaryPort = string.Empty;
            EftTimeout = string.Empty;
            EftMerchantId = string.Empty;
            EftTerminalId = string.Empty;
            EftReceiptLineWidth = string.Empty;
            EftUserFieldOnReceipt = string.Empty;
            EftSoftwareVersion = string.Empty;
            EftSoftwareName = string.Empty;
            EftSoftwareManufacturer = string.Empty;
            EftSerialNumber = string.Empty;
            EftPassword = string.Empty;
            EftHostAddress = string.Empty;
            EftEnvironment = string.Empty;
            EftApiKey = string.Empty;
            EftSaleId = string.Empty;
            EftConnection = string.Empty;
            EftKeyIdentifier = string.Empty;
            EftVersion = string.Empty;
            PrinterSelectedDevice = 0;
            PrinterPrintWidth = 0;
            PrinterNetworkAddress = string.Empty;
            PrinterModel = 0;
            PrinterLanguage = 0;
            hexColor = string.Empty;
            parameters = string.Empty;
            allowLegacyEdge = false;
            gridRow = 0;
            gridColumn = 0;

            hardwareStationServiceName = string.Empty;
            hardwareStationRemoteComputer = false;
            hardwareStationComputer = string.Empty;
            HardwareStationPort = string.Empty;
            hardwareStationUsername = string.Empty;
            hardwareStationPassword = string.Empty;

            serviceTierServiceName = string.Empty;
            serviceTierRemoteComputer = false;
            serviceTierComputer = string.Empty;
            ServiceTierPort = string.Empty;
            serviceTierUsername = string.Empty;
            serviceTierPassword = string.Empty;
        }

        [System.Xml.Serialization.XmlIgnore]
        public static SettingsConfig EmptyConfig
        {
            get
            {
                if (emptyConfig == null)
                {
                    emptyConfig = new SettingsConfig("");
                }

                return emptyConfig;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Description")]
        public string Description
        {
            get => description;
            set
            {
                description = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("FolderId")]
        public string FolderId
        {
            get => folderId;
            set
            {
                folderId = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("BackupId")]
        public string BackupId
        {
            get => backupId;
            set
            {
                backupId = value;
                NotifyPropertyChanged();
            }
        }

        //General settings
        [System.Xml.Serialization.XmlElementAttribute("ServerUrl")]
        public string Url
        {
            get => url;
            set
            {
                url = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("HexColor")]
        public string HexColor
        {
            get => hexColor;
            set
            {
                if (value.Length == 9)
                {
                    if (value.ToLower().StartsWith("#ff"))
                    {
                        value = "#" + value.Substring(3);
                    }
                }

                hexColor = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("DisplayOrder")]
        public int DisplayOrder
        {
            get => displayOrder;
            set
            {
                displayOrder = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("ServerTimeout")]
        public int Timeout
        {
            get => timeout;
            set
            {
                timeout = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("LSKey")]
        public string LsKey
        {
            get => lsKey;
            set 
            { 
                lsKey = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("StoreId")]
        public string StoreId
        {
            get => storeId;
            set
            {
                storeId = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("TerminalId")]
        public string TerminalId
        {
            get => terminalId;
            set
            {
                terminalId = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("BatchSize")]
        public int BatchSize
        {
            get => batchSize;
            set
            {
                batchSize = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("UpdateFrequency")]
        public int UpdateFrequency
        {
            get => updateFrequency;
            set
            {
                updateFrequency = value;
                NotifyPropertyChanged();
            }
        }

        //Simplified URL
        [System.Xml.Serialization.XmlElementAttribute("UrlType")]
        public SettingsConfigUrlType UrlType
        {
            get => urlType;
            set
            {
                urlType = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("UrlToUse");
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("SecurityStandard")]
        public SettingsConfigSecurityStandard SecurityStandard
        {
            get => securityStandard;
            set
            {
                securityStandard = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("UrlToUse");
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("ConfigType")]
        public SettingsConfigType SettingsConfigType
        {
            get => settingsConfigType;
            set
            {
                settingsConfigType = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("UrlToUse");
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("ComputerName")]
        public string ComputerName
        {
            get => computerName;
            set
            {
                computerName = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("UrlToUse");
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Port")]
        public string Port
        {
            get => port;
            set
            {
                port = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("UrlToUse");
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("WebServiceInstance")]
        public string WebServiceInstance
        {
            get => webServiceInstance;
            set
            {
                webServiceInstance = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("UrlToUse");
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Page")]
        public string Page
        {
            get => page;
            set
            {
                page = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("UrlToUse");
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Company")]
        public string Company
        {
            get => company;
            set
            {
                company = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("UrlToUse");
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("IsSaas")]
        public bool IsSaas
        {
            get => isSaas;
            set
            {
                isSaas = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("UrlToUse");
            }
        }

        public string OtherParameters
        {
            get => otherParameters;
            set
            {
                otherParameters = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("UrlToUse");
            }
        }


        [System.Xml.Serialization.XmlElementAttribute("Tenant")]
        public string Tenant
        {
            get => tenant;
            set
            {
                tenant = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("UrlToUse");
            }
        }

        //User settings
        [System.Xml.Serialization.XmlElementAttribute("UserName")]
        public string UserName
        {
            get => userName;
            set
            {
                userName = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Password")]
        public string Password
        {
            get => password;
            set
            {
                password = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Parameters")]
        public string Parameters
        {
            get => parameters;
            set
            {
                parameters = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("AllowLegacyEdge")]
        public bool AllowLegacyEdge
        {
            get => allowLegacyEdge;
            set
            {
                allowLegacyEdge = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("HardwareStationServiceName")]
        public string HardwareStationServiceName
        {
            get => hardwareStationServiceName;
            set
            {
                hardwareStationServiceName = value;
                NotifyPropertyChanged();
            }
        }


        [System.Xml.Serialization.XmlElementAttribute("HardwareStationRemoteComputer")]
        public bool HardwareStationRemoteComputer
        {
            get => hardwareStationRemoteComputer;
            set
            {
                hardwareStationRemoteComputer = value;
                NotifyPropertyChanged();
            }
        }


        [System.Xml.Serialization.XmlElementAttribute("HardwareStationComputer")]
        public string HardwareStationComputer
        {
            get => hardwareStationComputer;
            set
            {
                hardwareStationComputer = value;
                NotifyPropertyChanged();
            }
        }


        [System.Xml.Serialization.XmlElementAttribute("HardwareStationPort")]
        public string HardwareStationPort
        {
            get => hardwareStationPort;
            set
            {
                hardwareStationPort = value;
                NotifyPropertyChanged();
            }
        }


        [System.Xml.Serialization.XmlElementAttribute("HardwareStationUsername")]
        public string HardwareStationUsername
        {
            get => hardwareStationUsername;
            set
            {
                hardwareStationUsername = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("HardwareStationPassword")]
        public string HardwareStationPassword
        {
            get => hardwareStationPassword;
            set
            {
                hardwareStationPassword = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("ServiceTierServiceName")]
        public string ServiceTierServiceName
        {
            get => serviceTierServiceName;
            set
            {
                serviceTierServiceName = value;
                NotifyPropertyChanged();
            }
        }


        [System.Xml.Serialization.XmlElementAttribute("ServiceTierRemoteComputer")]
        public bool ServiceTierRemoteComputer
        {
            get => serviceTierRemoteComputer;
            set
            {
                serviceTierRemoteComputer = value;
                NotifyPropertyChanged();
            }
        }


        [System.Xml.Serialization.XmlElementAttribute("ServiceTierComputer")]
        public string ServiceTierComputer
        {
            get => serviceTierComputer;
            set
            {
                serviceTierComputer = value;
                NotifyPropertyChanged();
            }
        }


        [System.Xml.Serialization.XmlElementAttribute("ServiceTierPort")]
        public string ServiceTierPort
        {
            get => serviceTierPort;
            set
            {
                serviceTierPort = value;
                NotifyPropertyChanged();
            }
        }


        [System.Xml.Serialization.XmlElementAttribute("ServiceTierUsername")]
        public string ServiceTierUsername
        {
            get => serviceTierUsername;
            set
            {
                serviceTierUsername = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("ServiceTierPassword")]
        public string ServiceTierPassword
        {
            get => serviceTierPassword;
            set
            {
                serviceTierPassword = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlIgnore()]
        public SettingsConfigServiceStatus ServiceStatus
        {
            get => serviceStatus;
            set
            {
                serviceStatus = value; 
                NotifyPropertyChanged("IsEnabled");
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlIgnore()]
        public string ServiceStatusMessage
        {
            get => serviceStatusMessage;
            set
            {
                serviceStatusMessage = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("GridRow")]
        public int GridRow
        {
            get => gridRow;
            set
            {
                gridRow = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("GridColumn")]
        public int GridColumn
        {
            get => gridColumn;
            set
            {
                gridColumn = value;
                NotifyPropertyChanged();
            }
        }

        //Payment settings
        [System.Xml.Serialization.XmlElementAttribute("EftSelectedDevice")]
        public int EftSelectedDevice { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("EftIpAddress")]
        public string EftIpAddress { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftMainPort")]
        public string EftMainPort { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftSecondaryPort")]
        public string EftSecondaryPort { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftTimeout")]
        public string EftTimeout { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftMerchantId")]
        public string EftMerchantId { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftTerminalId")]
        public string EftTerminalId { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftReceiptLineWidth")]
        public string EftReceiptLineWidth { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftUserFieldOnReceipt")]
        public string EftUserFieldOnReceipt { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftSoftwareVersion")]
        public string EftSoftwareVersion { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftSoftwareName")]
        public string EftSoftwareName { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftSoftwareManufacturer")]
        public string EftSoftwareManufacturer { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftSerialNumber")]
        public string EftSerialNumber { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftPassword")]
        public string EftPassword { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftHostAddress")]
        public string EftHostAddress { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftEnvironment")]
        public string EftEnvironment { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftApiKey")]
        public string EftApiKey { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftSaleId")]
        public string EftSaleId { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftConnection")]
        public string EftConnection { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftKeyIdentifier")]
        public string EftKeyIdentifier { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("EftVersion")]
        public string EftVersion { get; set; }

        //Printer
        [System.Xml.Serialization.XmlElementAttribute("PrinterSelectedDevice")]
        public int PrinterSelectedDevice { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("PrinterPrintWidth")]
        public int PrinterPrintWidth { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("PrinterNetworkAddress")]
        public string PrinterNetworkAddress { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("PrinterModel")]
        public int PrinterModel { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("PrinterLanguage")]
        public int PrinterLanguage { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("PreventClosing")]
        public bool PreventClosing { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("RunOnStartup")]
        public bool RunOnStartup { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("TabletMode")]
        public bool TabletMode { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("ShowBackButton")]
        public bool ShowBackButton { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("UseWebView2")]
        public bool UseWebView2 { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("OpenInNewWindow")]
        public bool OpenInNewWindow { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("DisableWhenOffline")]
        public bool DisableWhenOffline { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("EnableCache")]
        public bool EnableCache { get; set; }

        //RemoteConfiguration
        [XmlElementAttribute("LSCentralWsUrl")]
        public string LsCentralWsUrl
        {
            get => lsCentralWsUrl;
            set
            {
                lsCentralWsUrl = value;
                NotifyPropertyChanged();
            }
        }
        [XmlElementAttribute("LSCentralODataUrl")]
        public string LsCentralOdataUrl
        {
            get => lsCentralOdataUrl;
            set
            {
                lsCentralOdataUrl = value;
                NotifyPropertyChanged();
            }
        }

        [XmlElementAttribute("LSCentralTenantId")]
        public string LsCentralTenantId
        {
            get => lsCentralTenantId;
            set
            {
                lsCentralTenantId = value;
                NotifyPropertyChanged();
            }
        }

        [XmlElementAttribute("LSCentralClientId")]
        public string LsCentralClientId
        {
            get => lsCentralClientId;
            set
            {
                lsCentralClientId = value;
                NotifyPropertyChanged();
            }
        }

        [XmlElementAttribute("LSCentralClientSecret")]
        public string LsCentralClientSecret
        {
            get => lsCentralClientSecret;
            set
            {
                lsCentralClientSecret = value;
                NotifyPropertyChanged();
            }
        }

        [XmlElementAttribute("LSCentralTimeout")]
        public int LsCentralTimeout
        {
            get => lsCentralTimeout;
            set
            {
                lsCentralTimeout = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public int Level
        {
            get => level;
            set
            {
                level = value;
                NotifyPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public bool IsEnabled
        {
            get => !DisableWhenOffline || ServiceStatus == SettingsConfigServiceStatus.Online;
        }


        public string UrlToUse
        {
            get
            {
                if (urlType == SettingsConfigUrlType.Simplified)
                {
                    var baseUrl = $"{(securityStandard == SettingsConfigSecurityStandard.Http ? "http://" : "https://")}{computerName}{(string.IsNullOrEmpty(port) ? "" : $":{port}")}";
                    var path = $"{(isSaas && !string.IsNullOrEmpty(tenant) ? $"/{tenant}" : "")}/{webServiceInstance}{(TabletMode ? "/tablet.aspx" : "")}";
                    var queryParameters = new List<string>();

                    if (!string.IsNullOrEmpty(company)) queryParameters.Add($"company={company}");
                    if (!string.IsNullOrEmpty(page)) queryParameters.Add($"page={page}");
                    if (!isSaas && !string.IsNullOrEmpty(tenant)) queryParameters.Add($"tenant={tenant}");
                    if (allowLegacyEdge) queryParameters.Add("AllowLegacyEdge=1");

                    // Add OtherParameters if they exist
                    if (!string.IsNullOrEmpty(OtherParameters)) queryParameters.Add(OtherParameters);

                    var queryString = string.Join("&", queryParameters);
                    var fullUrl = $"{baseUrl}{path}{(queryParameters.Any() ? "?" + queryString : "")}";

                    Url = fullUrl;
                    return fullUrl;
                }
                else
                {
                    Url = url;
                    return url;
                }
            }
        }

        public string UrlToUseWithAuth
        {
            get
            {
                var userNameWithDomain = string.Empty;

                if (userName.Contains("\\"))
                {
                    var splitName = userName.Split('\\');

                    userNameWithDomain = $"{splitName[1]}@{splitName[0]}";
                }
                else
                {
                    userNameWithDomain = userName;
                }

                if (urlType == SettingsConfigUrlType.Simplified)
                {
                    var url = string.Empty;
                    var urlParameters = "?";

                    if (securityStandard == SettingsConfigSecurityStandard.Http)
                    {
                        url = "http://";
                    }
                    else
                    {
                        url = "https://";
                    }

                    if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                    {
                        url += $"{userNameWithDomain}:{password}@";
                    }

                    url += computerName;

                    if (!string.IsNullOrEmpty(port))
                    {
                        url += ":" + port;
                    }

                    url += "/" + webServiceInstance;

                    if (TabletMode)
                    {
                        url += "/tablet.aspx";
                    }

                    if (!string.IsNullOrEmpty(company))
                    {
                        urlParameters += $"company={company}&";
                    }

                    if (!string.IsNullOrEmpty(page))
                    {
                        urlParameters += $"page={page}&";
                    }

                    if (!string.IsNullOrEmpty(tenant))
                    {
                        urlParameters += $"tenant={tenant}&";
                    }

                    if (allowLegacyEdge)
                    {
                        urlParameters += $"AllowLegacyEdge=1&";
                    }
                    
                    Url = url + urlParameters.TrimEnd('&', '?');
                    return url + urlParameters.TrimEnd('&', '?');
                }
                else
                {
                    var url = this.url;

                    if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                    {
                        if (url.StartsWith("http://"))
                        {
                            url = url.Insert(7, $"{userNameWithDomain}:{password}@");
                        }
                        else if (url.StartsWith("https://"))
                        {
                            url = url.Insert(8, $"{userNameWithDomain}:{password}@");
                        }
                    }
                    Url = url;
                    return url;
                }
            }
        }

        public string UsernameToUse => Environment.ExpandEnvironmentVariables(userName);

        public SettingsConfig ShallowCopy()
        {
            return (SettingsConfig) MemberwiseClone();
        }

        public override string ToString()
        {
            return $"{nameof(Description)}: {Description}, {nameof(Url)}: {Url}, {nameof(Timeout)}: {Timeout}, {nameof(LsKey)}: {LsKey}, {nameof(StoreId)}: {StoreId}, {nameof(TerminalId)}: {TerminalId}, {nameof(BatchSize)}: {BatchSize}, {nameof(UpdateFrequency)}: {UpdateFrequency}, {nameof(UrlType)}: {UrlType}, {nameof(SecurityStandard)}: {SecurityStandard}, {nameof(ComputerName)}: {ComputerName}, {nameof(Port)}: {Port}, {nameof(WebServiceInstance)}: {WebServiceInstance}, {nameof(Page)}: {Page}, {nameof(Company)}: {Company}, {nameof(Tenant)}: {Tenant}, {nameof(UserName)}: {UserName}, {nameof(EftSelectedDevice)}: {EftSelectedDevice}, {nameof(EftIpAddress)}: {EftIpAddress}, {nameof(EftMainPort)}: {EftMainPort}, {nameof(EftSecondaryPort)}: {EftSecondaryPort}, {nameof(EftTimeout)}: {EftTimeout}, {nameof(EftMerchantId)}: {EftMerchantId}, {nameof(EftTerminalId)}: {EftTerminalId}, {nameof(EftReceiptLineWidth)}: {EftReceiptLineWidth}, {nameof(EftUserFieldOnReceipt)}: {EftUserFieldOnReceipt}, {nameof(EftSoftwareVersion)}: {EftSoftwareVersion}, {nameof(EftSoftwareName)}: {EftSoftwareName}, {nameof(EftSoftwareManufacturer)}: {EftSoftwareManufacturer}, {nameof(EftSerialNumber)}: {EftSerialNumber}, {nameof(EftHostAddress)}: {EftHostAddress}, {nameof(EftEnvironment)}: {EftEnvironment}, {nameof(EftSaleId)}: {EftSaleId}, {nameof(EftConnection)}: {EftConnection}, {nameof(EftKeyIdentifier)}: {EftKeyIdentifier}, {nameof(EftVersion)}: {EftVersion}, {nameof(PrinterSelectedDevice)}: {PrinterSelectedDevice}, {nameof(PrinterPrintWidth)}: {PrinterPrintWidth}, {nameof(PrinterNetworkAddress)}: {PrinterNetworkAddress}, {nameof(PrinterModel)}: {PrinterModel}, {nameof(PrinterLanguage)}: {PrinterLanguage}, {nameof(PreventClosing)}: {PreventClosing}, {nameof(RunOnStartup)}: {RunOnStartup}";
        }
    }
}
