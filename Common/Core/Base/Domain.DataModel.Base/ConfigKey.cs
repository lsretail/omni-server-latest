using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    public enum ConfigKey
    {
        //General
        LSKey = 0,
        BOUser = 1,
        BOPassword = 2,
        BOUrl = 3,
        EComUrl = 4,
        BOQryUrl = 5,
        BOProtocol = 6,
        BOTenant = 7,

        forgotpassword_code_encrypted = 10,

        //Optional
        BOSql = 100,
        BOTimeout = 101,
        BOEncode = 102,

        //Proxy setup
        Proxy_Server = 300,
        Proxy_Port = 301,
        Proxy_User = 302,
        Proxy_Password = 303,
        Proxy_Domain = 304,

        //NAV specific
        NavAppId = 500,
        NavAppType = 501,
        Base64MinXmlSizeInKB = 502,
        SkipBase64Conversion = 503,

        Central_Token = 510,
        Central_TokenTime = 511,

        // aswaaq specific
        Password_Policy = 620,
        URL_Displayed_On_Client = 625,

        ContactUs = 630,
        OfflinePrintTemplate = 650,

        GiftCard_DataEntryType = 700,

        //server only
        PDF_Save_FolderName = 1000,

        Currency_Code = 1021,
        Currency_Culture = 1022,

        LSNAV_Version = 1100,  //used to change LS Central version and fast load so no need to check every time
        Demo_Print_Enabled = 1110,
        Timezone_HoursOffset = 1111,
        Allow_Dublicate_Email = 1150,

        Hosp_Terminal = 1171,
        Hosp_Staff = 1172,

        ScanPayGo_Terminal = 1181,
        ScanPayGo_Staff = 1182,
        ScanPayGo_CheckPayAuth = 1183,

        Inventory_Mask_IncludeCycleCounting = 1191,

        Cache_Image_DurationInMinutes = 1200,

        TenderType_Mapping = 1280,
    }
} 
