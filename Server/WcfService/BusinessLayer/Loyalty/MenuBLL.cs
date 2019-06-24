using System;
using System.Collections.Generic;
using System.Linq;

using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSOmni.Common.Util;

namespace LSOmni.BLL.Loyalty
{
    public class MenuBLL : BaseLoyBLL
    {
        private static LSLogger logger = new LSLogger();
        private IImageCacheRepository iImageCacheRepository;

        public MenuBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
            iImageCacheRepository = base.GetDbRepository<IImageCacheRepository>(config);
        }

        public MenuBLL(BOConfiguration config) : base(config, 0)
        {
        }

        public virtual MobileMenu MenusGetAll(string id, string lastVersion)
        {
            //This is used by Ax and NAV, since we can cache it.
            //call local db or BO web service. Return as list of stores
            MobileMenu mobileMenu = new MobileMenu();
            //if no menu id is given, get one
            if (string.IsNullOrWhiteSpace(id))
                id = "ALL";
            
            MenusGetAndFill(id, lastVersion, ref mobileMenu);

            return mobileMenu;
        }

        public virtual List<Hierarchy> HierarchyGet(string storeId)
        {
            return BOLoyConnection.HierarchyGet(storeId);
        }

        #region private

        private void MenusGetAndFill(string id, string lastVersion, ref MobileMenu mobileMenu)
        {
            try
            {
                CurrencyBLL curBLL = new CurrencyBLL(config, timeoutInSeconds);
                Currency currency = curBLL.CurrencyGetLocal();

                //not exist in cache so get them from BO
                string storeid = BOLoyConnection.GetWIStoreId();
                mobileMenu = BOLoyConnection.MenusGet(storeid, currency);

                mobileMenu.Currency = currency;
                
                //we need to get the items from the database since nav menu does not have it
                //foreach (Menu.Item it in mobileMenu.Items)
                for (int k = 0; k < mobileMenu.Items.Count; k++)
                {
                    if (string.IsNullOrWhiteSpace(mobileMenu.Items[k].Details))
                    {
                        mobileMenu.Items[k].Details = BOAppConnection.ItemDetailsGetById(mobileMenu.Items[k].Id);
                    }
                    //get all the iviews related to this item image (the menu should be returning a list of images, instead we get from db)
                    if (mobileMenu.Items[k].Images.Count > 0)
                    {
                        List<ImageView> iviews = iImageCacheRepository.ImagesCacheGetById(mobileMenu.Items[k].Images[0].Id);
                        if (iviews != null && iviews.Count() > 0)
                            mobileMenu.Items[k].Images = iviews;
                    }
                }

                for (int k = 0; k < mobileMenu.Products.Count; k++)
                {
                    if (string.IsNullOrWhiteSpace(mobileMenu.Products[k].Detail))
                    {
                        mobileMenu.Products[k].Detail = BOAppConnection.ItemDetailsGetById(mobileMenu.Products[k].Id);
                    }
                    //get all the views related to this item image (the menu should be returning a list of images, instead we get from db)
                    if (mobileMenu.Products[k].Images.Count > 0)
                    {
                        List<ImageView> iviews = iImageCacheRepository.ImagesCacheGetById(mobileMenu.Products[k].Images[0].Id);
                        if (iviews != null && iviews.Count() > 0)
                            mobileMenu.Products[k].Images = iviews;
                    }
                }

                for (int k = 0; k < mobileMenu.Recipes.Count; k++)
                {
                    if (string.IsNullOrWhiteSpace(mobileMenu.Recipes[k].Detail))
                    {
                        mobileMenu.Recipes[k].Detail = BOAppConnection.ItemDetailsGetById(mobileMenu.Recipes[k].Id);
                    }
                    //get all the views related to this Recipes image (the menu should be returning a list of images, instead we get from db)
                    if (mobileMenu.Recipes[k].Images.Count > 0)
                    {
                        List<ImageView> iviews = iImageCacheRepository.ImagesCacheGetById(mobileMenu.Recipes[k].Images[0].Id);
                        if (iviews != null && iviews.Count() > 0)
                            mobileMenu.Recipes[k].Images = iviews;
                    }
                }

                for (int k = 0; k < mobileMenu.Deals.Count; k++)
                {
                    //get all the views related to this Deals image (the menu should be returning a list of images, instead we get from db)
                    if (mobileMenu.Deals[k].Images.Count > 0)
                    {
                        List<ImageView> iviews = iImageCacheRepository.ImagesCacheGetById(mobileMenu.Deals[k].Images[0].Id);
                        if (iviews != null && iviews.Count() > 0)
                            mobileMenu.Deals[k].Images = iviews;
                    }
                }

                for (int k = 0; k < mobileMenu.MenuNodes.Count; k++)
                {
                    mobileMenu.MenuNodes[k].Image = iImageCacheRepository.ImageCacheGetById(mobileMenu.MenuNodes[k].Image.Id);
                }
            }
            catch (LSOmniServiceException ex)
            {
                //ignore the error and return what is in the cache, if anything
                logger.Error(config.LSKey.Key, "Something failed getting the menu.", ex);
                throw;
            }
        }
        #endregion private
    }
}

 