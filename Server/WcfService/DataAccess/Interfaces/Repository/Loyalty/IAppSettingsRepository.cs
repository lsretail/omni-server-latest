using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IAppSettingsRepository
    {
        string AppSettingsGetByKey(AppSettingsKey key, string languageCode="en");
        int AppSettingsIntGetByKey(AppSettingsKey key, string languageCode = "en");
        bool AppSettingsBoolGetByKey(AppSettingsKey key, string languageCode = "en");
        decimal AppSettingsDecimalGetByKey(AppSettingsKey key, string languageCode = "en");
        void AppSettingsSetByKey(AppSettingsKey key, string value, string valuetype, string comment, string languageCode = "en");
        bool AppSettingsKeyExists(AppSettingsKey key, string languageCode = "en");
        void DbCleanUp(int daysLog, int daysQueue, int daysNotify, int daysUser, int daysOneList);
        void Ping();
    }
}
