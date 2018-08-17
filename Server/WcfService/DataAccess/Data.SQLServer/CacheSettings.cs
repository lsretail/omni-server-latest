using System;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.Dal
{
    public sealed class CacheSettings
    {
        private static volatile CacheSettings instance;
        private static object syncRoot = new Object();

        public int CacheImageDurationInMinutes;
        public int CacheMenuDurationInMinutes;

        public bool CacheImage { get { return (CacheImageDurationInMinutes > 0); } }
        public bool CacheMenu { get { return (CacheMenuDurationInMinutes > 0); } }

        private CacheSettings()
        {
            //0 is no cache
            AppSettingsRepository appRep = new AppSettingsRepository();
            CacheImageDurationInMinutes = appRep.AppSettingsIntGetByKey(AppSettingsKey.Cache_Image_DurationInMinutes);
            CacheMenuDurationInMinutes = appRep.AppSettingsIntGetByKey(AppSettingsKey.Cache_Menu_DurationInMinutes);
        }

        public static CacheSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new CacheSettings();
                    }
                }
                return instance;
            }
        }
    }
}
