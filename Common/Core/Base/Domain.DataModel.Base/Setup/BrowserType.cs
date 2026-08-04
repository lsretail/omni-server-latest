namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
    /// <summary>
    /// Browser used to render a SettingsConfig URL.
    /// Ordinal values are persisted in XML via SettingsConfig.BrowserType,
    /// so the order MUST NOT change without a migration.
    /// </summary>
    public enum BrowserType
    {
        Internal = 0,
        /// <summary>Google Chrome browser</summary>
        Chrome   = 1,
        /// <summary>Microsoft Edge browser</summary>
        Edge     = 2,
    }
}
