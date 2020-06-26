using System;
using System.Drawing.Imaging;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base;
using System.Drawing;

namespace LSOmni.BLL
{
    public class ImageBLL : BaseBLL
    {
        private static LSLogger logger = new LSLogger();
        private IImageCacheRepository iImageCacheRepository;

        #region BOConnection

        private ILoyaltyBO iLoyBOConnection = null;

        protected ILoyaltyBO BOLoyConnection
        {
            get
            {
                if (iLoyBOConnection == null)
                    iLoyBOConnection = GetBORepository<ILoyaltyBO>(config.LSKey.Key);
                return iLoyBOConnection;
            }
        }

        #endregion BOConnection

        public ImageBLL(BOConfiguration config) : base(config)
        {
            this.iImageCacheRepository = GetDbRepository<IImageCacheRepository>(config);
        }

        public virtual ImageView ImageSizeGetById(string id, ImageSize imageSize)
        {
            // when NO caching or image is full size, then don't bother saving anything in database...
            if (config.SettingsIntGetByKey(ConfigKey.Cache_Image_DurationInMinutes) == 0 || (imageSize.Width == 0 && imageSize.Height == 0))
            {
                return ImageGetById(id, imageSize, true);
            }

            try
            {
                // check for cached image and if it is expired
                CacheState cState = iImageCacheRepository.Validate(config.LSKey.Key, id, imageSize, out DateTime lastModifyTime);
                if (cState != CacheState.Exists)
                {
                    //get the image from NAV table and put into cache
                    ImageView iv = ImageGetById(id, imageSize, true);
                    if (iv != null)
                    {
                        iImageCacheRepository.SaveImageCache(config.LSKey.Key, iv, true);
                    }
                    return iv;
                }
                else
                {
                    // check if image has changed in NAV
                    ImageView iv = ImageGetById(id, imageSize, false);
                    if (iv.ModifiedTime > lastModifyTime)
                    {
                        // load with blob and put into cache
                        iv = ImageGetById(id, imageSize, true);
                        if (iv != null)
                        {
                            iImageCacheRepository.SaveImageCache(config.LSKey.Key, iv, true);
                        }
                        return iv;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex, "Updating the Image Cache failed");
            }

            //now the image should be in cache, if not for some reason.. then return null.. not found
            ImageView imgSizeView = iImageCacheRepository.ImageCacheGetById(config.LSKey.Key, id, imageSize);
            if (imgSizeView == null)
            {
                imgSizeView = ImageGetById(id, imageSize, true);
                if (imgSizeView != null)
                {
                    //save it
                    iImageCacheRepository.SaveImageCache(config.LSKey.Key, imgSizeView, false);
                }
            }
            return imgSizeView;
        }

        //All original images are retrieved through this method. Can be from db, UNC or URL
        private ImageView ImageGetById(string id, ImageSize imgSize, bool includeBlob)
        {
            //get the original image from Image table
            ImageView iv = BOLoyConnection.ImageGetById(id, includeBlob);
            if (iv == null)
                return null;

            if (includeBlob == false)
                return iv;

            if (iv.LocationType == LocationType.File)
            {
                //get the original image from UNC file location
                iv.ImgBytes = Common.Util.ImageConverter.GetImageFromFile(id);
            }
            else if (iv.LocationType == LocationType.Url)
            {
                // get the original image from URL file location
            }

            iv.ImgSize = imgSize;
            ImageFormat imgFormat = Common.Util.ImageConverter.DefaultImgFormat;
            iv.Format = imgFormat.ToString();
            iv.Image = base.Base64GetFromByte(iv.ImgBytes, imgSize, imgFormat);
            if (string.IsNullOrEmpty(iv.Image) == false)
            {
                var bytes = Convert.FromBase64String(iv.Image);
                Image img = Common.Util.ImageConverter.ByteToImage(bytes);
                try
                {
                    iv.AvgColor = Common.Util.ImageConverter.CalculateAverageColor(img);
                }
                catch (Exception ex)
                {
                    logger.Warn(config.LSKey.Key, "Failed to get AvgColor > " + ex.Message);
                }
            }
            return iv;
        }
    }
}
