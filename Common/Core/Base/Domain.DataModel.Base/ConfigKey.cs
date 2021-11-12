using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum ConfigKey
    {
        //General
        [EnumMember]
        LSKey = 0,
        [EnumMember]
        BOUser = 1,
        [EnumMember]
        BOPassword = 2,
        [EnumMember]
        BOUrl = 3,
        [EnumMember]
        EComUrl = 4,
        [EnumMember]
        BOQryUrl = 5,
        [EnumMember]
        BOProtocol = 6,

        [EnumMember]
        forgotpassword_code_encrypted = 10,

        //Optional
        [EnumMember]
        BOSql = 100,
        [EnumMember]
        BOTimeout = 101,
        [EnumMember]
        BOEncode = 102,

        //Proxy setup
        [EnumMember]
        Proxy_Server = 300,
        [EnumMember]
        Proxy_Port = 301,
        [EnumMember]
        Proxy_User = 302,
        [EnumMember]
        Proxy_Password = 303,
        [EnumMember]
        Proxy_Domain = 304,

        //NAV specific
        [EnumMember]
        NavAppId = 500,
        [EnumMember]
        NavAppType = 501,
        [EnumMember]
        Base64MinXmlSizeInKB = 502,
        [EnumMember]
        SkipBase64Conversion = 503,

        // aswaaq specific
        [EnumMember]
        Password_Policy = 620,
        [EnumMember]
        URL_Displayed_On_Client = 625,

        [EnumMember]
        ContactUs = 630,
        [EnumMember]
        OfflinePrintTemplate = 650,

        [EnumMember]
        GiftCard_DataEntryType = 700,

#if WCFSERVER
        //server only
        PDF_Save_FolderName = 1000,

        Currency_Code = 1021,
        Currency_Culture = 1022,

        LSNAV_Version = 1100,  //only useful when all is in LSOmni, MPOS not using this..
        Demo_Print_Enabled = 1110,
        Timezone_HoursOffset = 1111,
        Allow_Dublicate_Email = 1150,

        Hosp_Terminal = 1171,
        Hosp_Staff = 1172,

        ScanPayGo_Terminal = 1181,
        ScanPayGo_Staff = 1182,

        Inventory_Mask_IncludeCycleCounting = 1191,

        Cache_Image_DurationInMinutes = 1200,

        TenderType_Mapping = 1280,

        LSRecommend_Company = 1321,
        LSRecommend_BatchNo = 1322,
        LSRecommend_ModelUrl = 1323,
        LSRecommend_AuthUrl = 1324,
        LSRecommend_ClientId = 1325,
        LSRecommend_ClientSecret = 1326,
        LSRecommend_UserName = 1327,
        LSRecommend_Password = 1328,
        LSRecommend_NoOfDownloadedItems = 1329,
        LSRecommend_NoOfDisplayedItems = 1330,
        LSRecommend_FilterByInv = 1331,
        LSRecommend_MinStock = 1332,
#endif
    }
} 
