namespace LSRetail.Omni.Domain.DataModel.Pos.DTO
{
    public enum UIErrorMsgCode
    {
        GenericError = 0,   // The operation could not be completed
        GetLoyaltyContactError,
        ContactSearchError,
        RegistrationError,
        CreateLoyaltyContactError,
        GetItemImageError,
        GetItemsInStockError,
        GetItemPriceError,
        PostTransactionError,
        TransactionCalcError,
        SuspendTransactionError,
        GetSavedTransactionError,
        PrintReceiptError,
        EmailReceiptError,
        TransactionSearchError,
        GetCrossSellItemsError,
        GetLatestTransactionsError,
        GetJournalTransactionError,
        PrintSettlementError,
        GetReturnTransactionError,
        DeviceIsBlocked,
        UnsupportedTransactionError,
        PingError,
        ImagePrinterLogoGetByStoreId,
        CustomerSearchError,
        CustomerGetPointError,
        GetTransactionByReceiptNumberError,

        // General errors
        GetMobileMenuError,
        ImageGetByIdError,
        SaveTransactionError,
        NetworkError
    }
}
