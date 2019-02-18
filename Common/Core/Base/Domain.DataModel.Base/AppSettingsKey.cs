using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum AppSettingsKey
    {
        [EnumMember]
        ContactUs = 0,    
        [EnumMember]
        SchemeLevel = 1,      
        [EnumMember]
        TermsOfService = 2,
        [EnumMember]
        forgotpassword_email_subject = 3,
        [EnumMember]
        forgotpassword_email_body = 4,
        [EnumMember]
        forgotpassword_email_url = 5,
        [EnumMember]
        forgotpassword_device_email_subject = 6,
        [EnumMember]
        forgotpassword_device_email_body = 7,
        [EnumMember]
        forgotpassword_code_encrypted = 8,
        [EnumMember]
        forgotpassword_omni_sendemail = 9,

        //aswaaq specific
        [EnumMember]
        resetpin_email_subject = 200,
        [EnumMember]
        resetpin_email_body = 201,
        [EnumMember]
        Password_Policy = 220,
        [EnumMember]
        URL_Displayed_On_Client = 225,

        [EnumMember]
        OfflinePrintTemplate = 250,

        [EnumMember]
        GiftCard_DataEntryType = 300,

#if WCFSERVER
        //server only
        PDF_Save_FolderName = 1000,

        Currency_Code = 1021,
        Currency_Culture = 1022,

        LSNAV_Version = 1100,  //only useful when all is in LSOmni, MPOS not using this..
        Demo_Print_Enabled  = 1110,
        Timezone_HoursOffset_DD = 1111,
        Loyalty_FilterOnStore = 1113,
        Timezone_DayOfWeekOffset = 1117,
        Cache_Image_DurationInMinutes = 1200,
        Cache_Menu_DurationInMinutes = 1201,

        Security_BasicAuth_Validation = 1250,
        Security_BasicAuth_UserName = 1251,
        Security_BasicAuth_Pwd = 1252,
        Security_Validatetoken = 1253,
        TenderType_Mapping = 1280,

        Receipt_Email_Send_From_BO = 1290, //send email receipt from NAV instead of LSOmni
        Receipt_Email_Subject = 1291,
        Receipt_Email_Body = 1292,

        Registration_Email_Subject = 1295,
        Registration_Email_Body = 1296,
        Use_LSOne_Email = 1297,

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

        POS_System_Inventory = 1351,
        POS_System_Inventory_Lookup = 1352,

#endif
    }
} 
