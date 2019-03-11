using System;
using System.Linq;

using NLog;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.BLL
{
    public class ImageBLL : BaseBLL
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IImageCacheRepository iImageCacheRepository;
        private IImageRepository iImageRepository;
        private ImageSize maxImageSize = null;

        #region BOConnection
        private ILoyaltyBO iLoyBOConnection = null;

        protected ILoyaltyBO BOLoyConnection
        {
            get
            {
                if (iLoyBOConnection == null)
                    iLoyBOConnection = GetBORepository<ILoyaltyBO>();
                return iLoyBOConnection;
            }
        }

        #endregion BOConnection

        public ImageBLL()
        {
            maxImageSize = new ImageSize(1000, 1000); //no image can be larger than this
            this.iImageCacheRepository = GetDbRepository<IImageCacheRepository>();
            this.iImageRepository = GetDbRepository<IImageRepository>();
        }

        public virtual ImageView ImageSizeGetById(string id, ImageSize imageSize)
        {
            imageSize.Width = (imageSize.Width <= maxImageSize.Width ? imageSize.Width : maxImageSize.Width);
            imageSize.Height = (imageSize.Height <= maxImageSize.Height ? imageSize.Height : maxImageSize.Height);

            //when NO caching, then don't bother saving anything in database...
            // no AvgColor or Format returned, too much cpu unless caching!
            if (iImageCacheRepository.CacheImage() == false)
            {
                return ImageGetById(id, imageSize);
            }

            // check if image is cached, if not then update the cache now..
            try
            {
                ImageView iv = null;
                if (imageSize.Width == 0 && imageSize.Height == 0)
                {
                    // get orginal image
                    return ImageGetById(id, imageSize);
                }

                //does not exist or is expired, need new from BO 
                CacheState cState = iImageCacheRepository.Validate(id, imageSize);
                if (cState != CacheState.Exists)
                {
                    //get the image from LSOmni Image table and put into cache
                    iv = ImageGetById(id, imageSize);
                    if (iv != null)
                        SaveCache(iv, "From NAV Image table");   //save to ImagesCache and ImagesSizeCache 

                    //saved, now return it to client
                    return iv;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Updating the Imagecache faile.");
            }

            //now the image should be in cache, if not for some reason.. then return null.. not found
            ImageView imgSizeView = iImageCacheRepository.ImageSizeCacheGetById(id, imageSize);
            //does it exist
            if (imgSizeView == null)
            {
                ImageView imgView = ImageGetById(id, imageSize);
                //images does not exist, something is wrong 
                if (imgView != null)
                {
                    //save it
                    SaveImageSizeCache(imgView);
                    imgSizeView = iImageCacheRepository.ImageSizeCacheGetById(id, imageSize);
                }
            }
            return imgSizeView;
        }

        //All original images are retrieved thru this method. Can be from db, UNC or URL
        private ImageView ImageGetById(string id, ImageSize imgSize)
        {
            //get the original image from Image table
            ImageView iv = BOLoyConnection.ImageBOGetById(id);
            if (iv == null)
            {
                // check if image is available in local db
                iv = iImageRepository.NotificationImagesById(id).FirstOrDefault();
                if (iv == null)
                    return null;
            }

            if (iv.LocationType == LocationType.File)
            {
                //get the original image from UNC file location
                iv.ImgBytes = Common.Util.ImageConverter.GetImageFromFile(id);
            }
            else if (iv.LocationType == LocationType.Url)
            {
                //get the original image from URL file location
            }

            iv.ImgSize = imgSize;
            iv.Image = base.Base64GetFromByte(iv.ImgBytes, imgSize);
            return iv;
        }

        public virtual void SaveCache(ImageView imgView, string description)
        {
            if (imgView == null || imgView.ImgBytes == null)
                return;

            imgView.Format = LSOmni.Common.Util.ImageConverter.DefaultImgFormat.ToString();
            System.Drawing.Image img = LSOmni.Common.Util.ImageConverter.ByteToImage(imgView.ImgBytes);
            //JIJ, use the imgView and make sure not crazy sizes are used
            int width = (imgView.ImgSize.Width <= maxImageSize.Width ? imgView.ImgSize.Width : maxImageSize.Width);
            int height = (imgView.ImgSize.Height <= maxImageSize.Height ? imgView.ImgSize.Height : maxImageSize.Height);
            imgView.AvgColor = LSOmni.Common.Util.ImageConverter.CalculateAverageColor(img);
            imgView.ImgSize = new ImageSize(width, height);

            //base64 put into Image, coming form ax, this may be filled out
            //if (string.IsNullOrWhiteSpace(imgView.Image))
            imgView.Image = LSOmni.Common.Util.ImageConverter.BytesToBase64(imgView.ImgBytes, width, height);

            //want to save the original size in ImageCache table
            iImageCacheRepository.SaveCache(imgView, description, new ImageSize(img.Width, img.Height));
        }

        #region Save image

        private void SaveImageSizeCache(ImageView imgView)
        {
            if (imgView == null)
                return;

            //save it
            ImageView saveImgView = new ImageView();
            saveImgView.Id = imgView.Id;
            saveImgView.AvgColor = ""; //not used
            saveImgView.DisplayOrder = 0;  //not used
            saveImgView.Format = LSOmni.Common.Util.ImageConverter.DefaultImgFormat.ToString();
            //base64
            if (string.IsNullOrWhiteSpace(imgView.Image))
            {
                if (imgView.ImgBytes == null)
                    return;
                saveImgView.Image = LSOmni.Common.Util.ImageConverter.BytesToBase64(imgView.ImgBytes,
                    imgView.ImgSize.Width, imgView.ImgSize.Height);
            }
            else
                saveImgView.Image = imgView.Image;

            saveImgView.ImgSize = imgView.ImgSize;
            saveImgView.Location = imgView.Location;
            saveImgView.LocationType = imgView.LocationType;
            iImageCacheRepository.SaveImageSizeCache(saveImgView);
            imgView.ImgBytes = null;
        }

        #endregion Save image
    }
}
