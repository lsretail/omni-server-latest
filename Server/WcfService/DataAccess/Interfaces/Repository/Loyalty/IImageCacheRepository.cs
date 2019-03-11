using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IImageCacheRepository
    {
        ImageView ImageCacheGetById(string id);
        List<ImageView> ImagesCacheGetById(string id);
        ImageView ImageSizeCacheGetById(string id, ImageSize imgSize);

        void SaveCache(ImageView imgView, string description, ImageSize orgImgSize);
        void SaveImageCache(ImageView imgView, string description, ImageSize orgImgSize);
        void SaveImageSizeCache(ImageView imgView);

        CacheState Validate(string id, ImageSize imageSize);
        bool CacheImage();
    }
}
