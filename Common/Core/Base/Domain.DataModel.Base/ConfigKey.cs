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
        EcommUrl = 4,

        [EnumMember]
        forgotpassword_code_encrypted = 10,

        //Optional
        [EnumMember]
        BOSql = 100,
        [EnumMember]
        BOTimeout = 101,

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

        //aswaaq specific
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
        Cache_Image_DurationInMinutes = 1200,

        TenderType_Mapping = 1280,

        LSReccomend_AzureAccountKey = 1301,
        LSReccomend_AzureName = 1302,
        LSReccomend_EndPointUrl = 1303,
        LSReccomend_NumberOfRecommendedItems = 1304,
        LSReccomend_AccountConnection = 1305,
        LSReccomend_CalculateStock = 1306,
        LSReccomend_WsURI = 1307,
        LSReccomend_WsUserName = 1308,
        LSReccomend_WsPassword = 1309,
        LSReccomend_WsDomain = 1310,
        LSReccomend_StoreNo = 1311,
        LSReccomend_Location = 1312,
        LSReccomend_MinStock = 1313,
#endif
    }
} 
