using System;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.BLL.Loyalty
{
    public class MenuBLL : BaseLoyBLL
    {
        private readonly IImageCacheRepository iImageCacheRepository;

        public MenuBLL(BOConfiguration config, int timeoutInSeconds) : base(config, timeoutInSeconds)
        {
            iImageCacheRepository = GetDbRepository<IImageCacheRepository>(config);
        }

        public virtual MobileMenu MenuGet(string storeId, string salesType, bool loadDetails, ImageSize imageSize, Statistics stat)
        {
            MobileMenu mobileMenu;

            try
            {
                CurrencyBLL curBLL = new CurrencyBLL(config, timeoutInSeconds);
                Currency currency = curBLL.CurrencyGetLocal(stat);

                mobileMenu = BOLoyConnection.MenuGet(storeId, salesType, currency, stat);
                mobileMenu.Currency = currency;

                if (loadDetails == false)
                    return mobileMenu;

                //we need to get the items from the database since nav menu does not have it
                //get all the views related to this item image (the menu should be returning a list of images, instead we get from db)
                List<ImageView> iviews = new List<ImageView>();
                for (int k = 0; k < mobileMenu.Items.Count; k++)
                {
                    if (string.IsNullOrWhiteSpace(mobileMenu.Products[k].Detail))
                    {
                        mobileMenu.Items[k].Details = BOAppConnection.ItemDetailsGetById(mobileMenu.Products[k].Id, stat);
                    }

                    iviews.Clear();
                    foreach (ImageView im in mobileMenu.Items[k].Images)
                    {
                        ImageView iv = iImageCacheRepository.ImageCacheGetById(config.LSKey.Key, im.Id, imageSize);
                        if (iv != null)
                            iviews.Add(iv);
                    }
                    mobileMenu.Items[k].Images = iviews;
                }

                for (int k = 0; k < mobileMenu.Products.Count; k++)
                {
                    if (string.IsNullOrWhiteSpace(mobileMenu.Products[k].Detail))
                    {
                        mobileMenu.Products[k].Detail = BOAppConnection.ItemDetailsGetById(mobileMenu.Products[k].Id, stat);
                    }

                    iviews.Clear();
                    foreach (ImageView im in mobileMenu.Products[k].Images)
                    {
                        ImageView iv = iImageCacheRepository.ImageCacheGetById(config.LSKey.Key, im.Id, imageSize);
                        if (iv != null)
                            iviews.Add(iv);
                    }
                    mobileMenu.Products[k].Images = iviews;
                }

                for (int k = 0; k < mobileMenu.Recipes.Count; k++)
                {
                    if (string.IsNullOrWhiteSpace(mobileMenu.Recipes[k].Detail))
                    {
                        mobileMenu.Recipes[k].Detail = BOAppConnection.ItemDetailsGetById(mobileMenu.Recipes[k].Id, stat);
                    }

                    //get all the views related to this Recipes image (the menu should be returning a list of images, instead we get from db)
                    iviews.Clear();
                    foreach (ImageView im in mobileMenu.Recipes[k].Images)
                    {
                        ImageView iv = iImageCacheRepository.ImageCacheGetById(config.LSKey.Key, im.Id, imageSize);
                        if (iv != null)
                            iviews.Add(iv);
                    }
                    mobileMenu.Recipes[k].Images = iviews;
                }

                for (int k = 0; k < mobileMenu.Deals.Count; k++)
                {
                    //get all the views related to this Deals image (the menu should be returning a list of images, instead we get from db)
                    iviews.Clear();
                    foreach (ImageView im in mobileMenu.Deals[k].Images)
                    {
                        ImageView iv = iImageCacheRepository.ImageCacheGetById(config.LSKey.Key, im.Id, imageSize);
                        if (iv != null)
                            iviews.Add(iv);
                    }
                    mobileMenu.Deals[k].Images = iviews;
                }

                for (int k = 0; k < mobileMenu.MenuNodes.Count; k++)
                {
                    mobileMenu.MenuNodes[k].Image = iImageCacheRepository.ImageCacheGetById(config.LSKey.Key, mobileMenu.MenuNodes[k].Image.Id, imageSize);
                }
            }
            catch (LSOmniServiceException ex)
            {
                //ignore the error and return what is in the cache, if anything
                logger.Error("Something failed getting the menu.", ex);
                throw;
            }

            return mobileMenu;
        }
    }
}

 