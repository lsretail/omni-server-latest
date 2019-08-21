using System;

using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IImageCacheRepository
    {
        ImageView ImageCacheGetById(string lsKey, string id, ImageSize imageSize);
        void SaveImageCache(string lsKey, ImageView imgView, bool doUpdate);
        CacheState Validate(string lsKey, string id, ImageSize imageSize, out DateTime lastModeTime);
    }
}
