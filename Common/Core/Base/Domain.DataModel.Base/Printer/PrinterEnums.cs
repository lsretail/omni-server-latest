namespace LSRetail.Omni.Domain.DataModel.Base.Printer
{
    #region Nav enums
    public enum NavFontType
    {
        Normal = 0,
        Bold = 1,
        Wide = 2,
        High = 3,
        WideHigh = 4,
        Italic = 5,
        CustomESC = 6,
        Image = 7,
        Barcode = 8
    }
    #endregion

    #region Base enums (supported types)
    public enum FontType
    {
        Normal,
        Bold,
        Large
    }

    public enum FontSize
    {
        Normal,
        Small,
        Large
    }

    public enum BarcodeSystem
    {
        UPC_A,
        UPC_E,
        EAN13,
        EAN8,
        CODE39,
        ITF,
        CODABAR,
        CODE93,
        CODE128,
        PDF417,
        Unsupported
    }
    #endregion
}