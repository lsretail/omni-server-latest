using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum StatusCode
    {
		/// <summary>
        /// Success!
        /// </summary>
        [EnumMember]
        OK = 0,
        /// <summary>
        /// Error unknown
        /// </summary>
        [EnumMember]
        Error = 1,      
        /// <summary>
        /// Authentication failed, user name and password entered do not match data in system
        /// </summary>
        [EnumMember]
        AuthFailed = 2,   
        /// <summary>
        /// User name exists and in use in system.
        /// Signup / registration can return this code
        /// </summary>
        [EnumMember]
        UserNameExists = 3,   
        /// <summary>
        /// User name OR password are invalid. Do not pass the basic validation routine
        /// </summary>
        [EnumMember]
        UserNamePasswordInvalid = 4, 
        /// <summary>
        /// Required parameter is missing, invalid or empty.  (ex. if deviceId is required)
        /// </summary>
        [EnumMember]
        ParameterInvalid = 5,
  
        [EnumMember]
        ItemNotFound = 6,  // not used yet, item object is NULL if item is not found!
        /// <summary>
        /// Account not found. Back office may require the account id in their web service 
        /// </summary>
        [EnumMember]
        AccountNotFound = 7,  
        /// <summary>
        /// Device ID is missing (ex. during login)
        /// </summary>
        [EnumMember]
        DeviceIdMissing = 8,   
        /// <summary>
        /// ContactId not found in system. Methods that pass in contact id can return this code
        /// </summary>
        [EnumMember]
        ContactIdNotFound = 9,   
        /// <summary>
        /// Current list can not be deleted
        /// </summary>
        [EnumMember]
        CurrentListDeleteNotAllowed = 10,   
        /// <summary>
        /// Password or new password is invalid.
        /// ChangePassword and ContactCreate can return this code
        /// </summary>
        [EnumMember]
        PasswordInvalid = 11,   
        /// <summary>
        /// Old password is invalid.  ChangePassword can return this code
        /// </summary>
        [EnumMember]
        PasswordOldInvalid = 12,   
        /// <summary>
        /// User name is invalid. ContactCreate can return this code
        /// </summary>
        [EnumMember]
        UserNameInvalid = 13,
        /// <summary>
        /// Email is invalid. ContactCreate can return this code
        /// </summary>
        [EnumMember]
        EmailInvalid = 14,   
        /// <summary>
        /// Device has been blocked. All methods can return this code
        /// </summary>
        [EnumMember]
        DeviceIsBlocked = 15,   
        //[EnumMember]
        //ContactCreateFailed = 16,  //not used
        /// <summary>
        /// Email already in use.  ContactCreate and ContactUpdate can return this code
        /// </summary>
        [EnumMember]
        EmailExists = 17,   
        /// <summary>
        /// Last name is missing. ContactCreate and ContactUpdate can return this code
        /// </summary>
        [EnumMember]
        MissingLastName = 18,
        /// <summary>
        /// First name is missing. ContactCreate and ContactUpdate can return this code
        /// </summary>
        [EnumMember]
        MissingFirstName = 19,
        /// <summary>
        /// Item Id was expected in backoffice
        /// </summary>
        [EnumMember]
        MissingItemId = 20,
        /// <summary>
        /// StoreId was expected in backoffice
        /// </summary>
        [EnumMember]
        MissingStoreId = 21,
        /// <summary>
        /// TenantConfig not found. 
        /// </summary>
        [EnumMember]
        TenantConfigNotFound = 22,
        /// <summary>
        /// Order in Queue not found. 
        /// </summary>
        [EnumMember]
        OrderQueueIdNotFound  = 23,
        /// <summary>
        /// Config not found. 
        /// </summary>
        [EnumMember]
        ConfigNotFound = 24,

        [EnumMember]
        OneAccountInvalid = 30,
        [EnumMember]
        PrivateAccountInvalid = 31,
        [EnumMember]
        ClubOrSchemeInvalid = 32,
        [EnumMember]
        ResetPasswordCodeNotFound = 35,
        [EnumMember]
        ResetPasswordCodeExpired = 36,
        [EnumMember]
        TerminalTypeNotMobile = 37,
        [EnumMember]
        ResetPasswordCodeInvalid = 38,
        [EnumMember]
        OneListNotFound = 39,

        /// <summary>
        ///Need to upgrade the client, the client version and server version logic enforce this
        /// </summary>
        [EnumMember]
        Upgrade = 50,
        [EnumMember]
        CurrencyCodeNotFound = 51,

        [EnumMember]
        TaskStatusCannotChange = 61,

		[EnumMember]
		AddressIsEmpty = 81,

        //security related status codes,  ALL methods can return these enums
        /// <summary>
        /// Security token is invalid. Security token is missing, empty or not found in system
        /// </summary>
        [EnumMember]
        SecurityTokenInvalid= 100,   
        /// <summary>
        /// Logged in user does not have permission to view the requested data 
        /// </summary>
        [EnumMember]
        AccessNotAllowed = 101,     
        /// <summary>
        /// User is not logged in. SecurityToken used is valid but user is not longer logged into system
        /// </summary>
        [EnumMember]
        UserNotLoggedIn = 102,
        /// <summary>
        /// User is not logged in. SecurityToken used is valid but user is not longer logged into system
        /// </summary>
        [EnumMember]
        LSKeyInvalid = 103,
        [EnumMember]
        LSRecommendSetupMissing = 104,
        [EnumMember]
        LSRecommendError = 105,

        /// <summary>
        /// Primary key duplication
        /// </summary>
        [EnumMember]
        PrimaryKeyDuplication = 150,
        [EnumMember]
        ObjectIdMissing = 151,
        [EnumMember]
        ObjectMissing = 152,

        //only used on client side
        /// <summary>
        /// Communication error. ONLY used in REST client code.
        ///  server not found, incorrect url, timeout etc
        /// </summary>
        [EnumMember]
        CommunicationFailure = 200,

        [EnumMember]
        GeneralErrorCode = 1001,

        [EnumMember]
        MissingTenderLines = 1002,

        [EnumMember]
        UserNameNotFound = 1101,

        [EnumMember]
        QtyMustBePositive = 1106,
        [EnumMember]
        LineNoMission = 1107,
         
        [EnumMember]
        CardIdInvalid = 1130,
        [EnumMember]
        LoginIdNotMemberOfClub = 1140,
        [EnumMember]
        DeviceIdNotFound = 1150,

        [EnumMember]
        CouponNotFound = 1210,  //
        [EnumMember]
        GiftCardNotFound = 1211,  //

        [EnumMember]
        AccountContactIdInvalid = 1220,
        [EnumMember]
        AccountExistsInOtherClub = 1230,

        [EnumMember]
        NoDiscountAmount = 1240,

        /// <summary>
        /// Missing Item Id
        /// </summary>
        [EnumMember]
        MissingItemNumer = 1400,

        /// <summary>
        /// Missing Store ID
        /// </summary>
        //[EnumMember]
        MissingStoreNumber = 1410,

        #region WebPos

        [EnumMember]
        PosNotExists = 1600,
        [EnumMember]
        StoreNotExists = 1601,
        [EnumMember]
        StaffNotExists = 1602,
        [EnumMember]
        ItemNotExists = 1603,
        [EnumMember]
        VATSetupMissing = 1604,
        [EnumMember]
        InvalidUom = 1605,
        [EnumMember]
        ItemBlocked = 1606,
        [EnumMember]
        InvalidVariant = 1607,
        [EnumMember]
        InvalidPriceChange = 1608,
        [EnumMember]
        PriceChangeNotAllowed = 1609,
        [EnumMember]
        PriceTooHigh = 1610,
        [EnumMember]
        InvalidDiscPercent = 1611,
        [EnumMember]
        IncExpNotFound = 1612,
        [EnumMember]
        TenderTypeNotFound = 1613,
        [EnumMember]
        InvalidTOTDiscount = 1614,
        [EnumMember]
        NotMobilePos = 1615,
        [EnumMember]
        InvalidPostingBalance = 1619,
        [EnumMember]
        SuspendWithPayment = 1620,
        [EnumMember]
        UnknownSuspError = 1621,
        [EnumMember]
        SuspKeyNotFound = 1625,
        [EnumMember]
        TransServError = 1626,
        [EnumMember]
        SuspTransNotFound = 1627,
        [EnumMember]
        PaymentPointsMissing = 1628, //1620 from WEB_POS
        [EnumMember]
        NAVWebFunctionNotFound = 1629,  //error 0004
        [EnumMember]
        MemberPointBalanceToLow = 1630,

        #endregion

        #region WEB_POS_SALES_HISTORY

        [EnumMember]
        CustomerNotFound = 1670,
        [EnumMember]
        NoEntriesFound = 1673,
        [EnumMember]
        MemberAccountNotFound = 1674,
        [EnumMember]
        MemberCardNotFound = 1675,

        #endregion

        [EnumMember]
        CardInvalidInUse = 1700,
        [EnumMember]
        CardInvalidStatus = 1701,

        [EnumMember]
        InvalidPrinterId = 1711,
        [EnumMember]
        InvalidPrintMethod = 1712,

        #region CreateCustomer / ContactCreate

        [EnumMember]
        LoginidExists=1401,
        [EnumMember]
        InvalidLoginid=1402,
        [EnumMember]
        InvalidPassword=1403,
        [EnumMember]
        InvalidEmail=1404,
        [EnumMember]
        MissingLastname=1405,
        [EnumMember]
        MissingFirstname=1406,
        [EnumMember]
        InvalidAccount =1407,
        [EnumMember]
        InvalidOneaccount =1408,
        [EnumMember]
        InvalidPrivate	=1409,
        [EnumMember]
        InvalidClub	=1410,
        [EnumMember]
        InvalidScheme = 1411,
        [EnumMember]
        InvalidClubScheme=1412,
        [EnumMember]
        InvalidSchemeClub	=1413,
        [EnumMember]
        TerminalIdMissing= 1414,
        [EnumMember]
        TransacitionIdMissing = 1415,
        [EnumMember]
        EmailMissing = 1416,
        [EnumMember]
        ClubInvalid = 1417,
        [EnumMember]
        SchemeInvalid = 1418,
        [EnumMember]
        SchemeClubInvalid = 1419,
        [EnumMember]
        ReceiptNoMissing = 1420,

        #endregion

        [EnumMember]
        ContactIsBlocked = 2000,  //2000 in nav

        #region Customer Order

        [EnumMember]
        OrderAlreadyExist = 2201,
        [EnumMember]
        OrderIdNotFound = 2202,
        [EnumMember]
        PaymentError = 2203,

        #endregion

        //not used on client
        InvalidNode = 3030, // 0030 from nav,0030 - Invalid value RETSAVE in Request Node Command  

        #region hospPos

        //WEB_SEND_DINING_TABLE_ACTION uses these errors. < 1000 General error code
        [EnumMember]
        ServerRefusingToRespond = 4002,  //Forbidden. The request was a valid request, but the server is refusing to respond to it
        [EnumMember]
        DiningTableStatusNotAbleToChange = 4003,  //Not able to change Dining Table Status.
        [EnumMember]
        CannotChangeNumberOfCoverOnTableNotSeated = 4004,  //Cannot change the number of cover on table because not seated
        [EnumMember]
        CannotChangeNumberOfCoverOnTableNoSetup = 4005,  //Cannot change the number of cover on table (not setup for this dining area profile)
        [EnumMember]
        SeatingNotUsedInHospType = 4006,  //Seating is not in use for this hospitality type
        [EnumMember]
        StatusOfTableAlredySeated = 4007,  //The status of the table is already Seated. Use CHANGECOVER to change the cover.
        [EnumMember]
        SeatingNotPossible = 4008,  //Seating is not possible.
        [EnumMember]
        POSTransNotFoundForActiveOrder = 4009,  //POS Transaction not found for active order. //WEB_POS_SPLITBILL
        [EnumMember]
        NoKitchenStatusFound = 4010,  //No Kitchen status found
        [EnumMember]
        OpenPOSNotALlowed = 4011,  //Opening the POS is not possible. Seat guests first
        [EnumMember]
        MainStatusNorCorrect = 4020,  //The Main Status of table is not correct.
        [EnumMember]
        TableAlreadyLocked = 4021,  //Table is already locked
        [EnumMember]
        SuspendFailure = 4030,  //first used by Percassi
        //GET_DYN_CONT_HMP_MENUS uses these errors
        [EnumMember]
        NoHMPMenuFound = 4031,//1001  from nav
        [EnumMember]
        HMPMenuNotEnabled = 4032, // 1013 from nav,
        [EnumMember]
        HMPMenuNoDynamicContentFoundToday= 4033, // 1014 from nav,

        #endregion hospPos

        //Inventory - LSOne
        [EnumMember]
        VendorNotFound = 5001,
        [EnumMember]
        DocumentError = 5002,
        [EnumMember]
        TemplateNotFound = 5003,
        [EnumMember]
        DocumentNotFound = 5004,
        [EnumMember]
        MissingUnitConversion = 5005,
        [EnumMember]
        NoLinesToPost = 5006,
        [EnumMember]
        AlreadyPosted = 5007,
        [EnumMember]
        AlreadyProcessing = 5008,
        [EnumMember]
        InvalidReceivingQty = 5009,
        [EnumMember]
        NoVendorItems = 5010,
        [EnumMember]
        TransferOrderNotFound = 5011,
        [EnumMember]
        TransferOrderProcessing = 5012,
        [EnumMember]
        TransferOrderReceived = 5013,
        [EnumMember]
        TransferOrderFetched = 5014,
        [EnumMember]
        TransferOrderSent = 5015,
        [EnumMember]
        TransferOrderRejected = 5016,
        [EnumMember]
        WorksheetNotFound = 5017,
        [EnumMember]
        NotReadyForNextCount = 5018,
        [EnumMember]
        LinesNotFound = 5019,

        //POS - LSOne
        [EnumMember]
        InvalidSuspensionWithPartialPayment = 6001,
        [EnumMember]
        NoItemsToSuspend = 6002,
        [EnumMember]
        CannotMixNormalSaleAndReturn = 6003,

        //New Core added
        [EnumMember]
        UnknownUser = 7001,

        //New NAV WS added
        [EnumMember]
        NavWSError = 8000,

        //Inventory App codes
        [EnumMember]
        CustomerOrderNotFound = 9000,
        [EnumMember]
        CustomerOrderItemNotFound = 9001,
        [EnumMember]
        CustomerOrderItemAlreadyInBox = 9002,
        [EnumMember]
        CustomerOrderItemQuantityTooHigh = 9003,
        [EnumMember]
        CustomerOrderAlreadyScanned = 9004,

        #region SQLITE Error Codes 10.000 to 19.999

        GenericError = 10000,

        LogSaveError = 10100,
        LogGetError = 10101,
        LogDeleteError = 10102,

        CustomerSearchError = 10200,

        GetItemsError = 10300,
        GetItemsByItemSearchError = 10301,
        GetItemByBarcodeError = 10302,
        GetPluMenuItemsError = 10303,
        GetItemByIdError = 10304,
        GetUnitOfMeasureByIdError = 10305,
        GetVariantsByItemIdError = 10306,
        GetVariantByIdError = 10307,
        GetItemPriceError = 10308,
        GetBarcodeMasks = 10309,

        TerminalOrStoreError = 10400,

        GetTenderTypeError = 10500,
        GetTaxSetupError = 10501,
        GetSalesTypeError = 10502,

        GetUserError = 10600,
        LoginUserError = 10601,

        SaveSettingsError = 10700,

        GetDefaultHospTypeIdError = 10800,
        GetHospTypesError = 10801,

        GetGs1BarcodeSetupsError = 10900,

        SettingsConfigDelete = 11000,
        SettingsConfigDeleteAll = 11001,
        SettingsConfigSave = 11002,

        #endregion
    }
}
 