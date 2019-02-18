using System;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Web;
using System.Text;

using NLog;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.Service
{
    /// <summary>
    /// Base class for private methods shared 
    /// </summary>
    public partial class LSOmniBase
    {
        #region image processing

        /// <summary>
        /// Get image from byte[] array
        /// </summary>
        /// <param name='image'>the byte array</param>
        /// <param name='imageSize'>imageSize</param>
        /// <returns>base64 string of </returns>
        private string GetImageFromByte(byte[] image, ImageSize imageSize)
        {
            try
            {

                //if (image == null || image.Length < 10)
                //    image = GetImageBytesFile("na.jpg");

                int height = imageSize.Height;
                int width = imageSize.Width;
                System.Drawing.Imaging.ImageFormat imgFormat = System.Drawing.Imaging.ImageFormat.Jpeg; //default everything to Png

                //string imageBase64 = ImageConverter.BytesToBase64(image, width, height, imgFormat);
                return ImageConverter.BytesToBase64(image, width, height, imgFormat);
            }
            catch (Exception ex)
            {
                //HandleExceptions(ex, "Failed to GetImageFromByte() ");  //dont throw an error but return something
                string msg = "GetImageFromByte() failed but returning empty base64 string";
                if (image == null)
                    msg += " image bytes are null";
                else
                    msg += " image bytes length: " + image.Length;

                logger.Log(LogLevel.Warn, ex, msg);
                return "";// GetImageFile("na.jpg");
            }
        }

        private string GetImageStreamUrl(ImageView imgView)
        {
            if (imgView == null)
            {
                logger.Info("imgView is null");
                return "";
            }
            // returns when imgView is null http://localhost/LSOmniService/json.svc/ImageStreamGetById?id={0}&width={1}&height={2} 
            // return when imgView is not null http://localhost/LSOmniService/json.svc/ImageStreamGetById?id=66&width=255&height=455
            string url = BaseUrlReturnedToClient() + @"/ImageStreamGetById";
            //if (imgView.ImgSize.Width == 0 && imgView.ImgSize.Height == 0)
            url += "?id=" + imgView.Id + "&width={0}&height={1}";
            //else
            //    url += string.Format("?id={0}&width={1}&height={2}", imgView.Id, imgView.ImgSize.Width, imgView.ImgSize.Height);
            return url;
        }

        private string GetImageStreamUrlEx(ImageView imgView)
        {
            if (imgView == null)
            {
                logger.Info("imgView is null");
                return "";
            }
            // returns when imgView is null http://localhost/LSOmniService/json.svc/ImageStreamGetById?id={0}&width={1}&height={2} 
            // return when imgView is not null http://localhost/LSOmniService/json.svc/ImageStreamGetById?id=66&width=255&height=455
            string url = BaseUrlReturnedToClient() + @"/ImageStreamGetById";
            //if (imgView.ImgSize.Width == 0 && imgView.ImgSize.Height == 0)
            url += "?id=" + imgView.Id + "&width={0}&height={1}";
            //else
            //    url += string.Format("?id={0}&width={1}&height={2}", imgView.Id, imgView.ImgSize.Width, imgView.ImgSize.Height);
            return url;
        }

        private string BaseUrlReturnedToClient()
        {
            // returns the base URL

            //dont fully trust baseUri - hence this method...

            //have to be carefull of the url reurned to client since Host unde IIS is tricky
            //returns the correct URL without the URL Request
            //It returns  http://macbookjij.lsretail.local/LSOmniService/json.svc 
            //  or http://192.22.11.12/LSOmniService.json.svc  

            //System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath "/LSOmniService" 

            //build the URL based on defaults
            // serverUri = http://macbookjij.lsretail.local/LSOmniService/Json.svc/ImageGetById
            //  baseUriOrignalString =  http://macbookjij.lsretail.local/LSOmniService/Json.svc   

            //find the index of .svc just in case some crap is added to baseOriginalString
            string svc = ".svc"; //http://macbookjij.lsretail.local/LSOmniService/Json.svc/ 
            string srvUrl = baseUriOrignalString.ToLower();  //note orgianlString also has the port number = good

            int idx = srvUrl.IndexOf(svc);
            if (idx > 0)
            {
                srvUrl = srvUrl.Substring(0, idx + 4); // => http://macbookjij.lsretail.local/LSOmniService/json.svc
            }
            else
                logger.Debug("baseUriOrignalString {0}  something not correct here..", srvUrl);

            return srvUrl;
        }

        #endregion image processing

        #region protected members

        protected string LogJson(object objIn)
        {
            string sOut = "";
            if (logger.IsDebugEnabled || logger.IsErrorEnabled)
            {
                if (objIn == null)
                    return "";
                try
                {
                    using (System.IO.MemoryStream memstream = new System.IO.MemoryStream())
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(objIn.GetType());
                        ser.WriteObject(memstream, objIn);
                        sOut = Encoding.Default.GetString(memstream.ToArray());
                        //remove the password
                        if (sOut.Contains("\"Password\":"))
                        {
                            int idx = sOut.IndexOf("\"Password\":");
                            if (idx > 0)
                            {
                                int nextIdx = sOut.IndexOf(",", idx + 1); //find  the next comma and remove
                                sOut = sOut.Remove(idx + "\"Password\":".Length, nextIdx - idx - "\"Password\":".Length);
                            }
                        }
                        sOut = string.Format("\r\nFormatted json for logging. Size(bytes):{0}\r\n{1}\r\n", sOut.Length.ToString(), sOut);
                    }
                }
                catch (Exception)
                {
                    logger.Info(Serialization.SerializeToXmlPrint(objIn));
                }
            }
            return sOut; //FormatOutput(sOut);
        }

        protected virtual void HandleExceptions(Exception ex, string errMsg)
        {
            //handle all errors in once place
            //if LoyaltyException is thrown then dont mess with it, let it flow to client
            if (ex.GetType() == typeof(LSOmniServiceException))
            {
                LSOmniServiceException lEx = (LSOmniServiceException)ex;
                logger.Log(LogLevel.Error, lEx, lEx.Message);
                throw new WebFaultException<LSOmniException>(new LSOmniException(lEx.StatusCode, lEx.Message), exStatusCode);
            }

            logger.Log(LogLevel.Error, ex, errMsg);
            throw new WebFaultException<LSOmniException>(new LSOmniException(StatusCode.Error, errMsg + " - " + ex.Message), exStatusCode);
        }

        #endregion protected members

        #region private members

        private void ValidateConfiguration()
        {
            if (ConfigValidationRunOnce == null)
            {
                ConfigValidationRunOnce = new object();
                //make sure this is NAV
                if (ConfigSetting.KeyExists("BOConnection.Nav.Url"))
                {
                    string url = ConfigSetting.GetString("BOConnection.Nav.Url").ToLower();
                    url = url.Replace("%20", " "); //what when user put %20 in it
                    string connStr = "";
                    if (ConfigSetting.KeyExists("SqlConnectionString.Nav"))
                        connStr = ConfigSetting.GetString("SqlConnectionString.Nav");
                    else if (ConfigSetting.KeyExists("NavSqlConnectionString"))
                        connStr = ConfigSetting.GetString("NavSqlConnectionString");

                    System.Data.Common.DbConnectionStringBuilder builder = new System.Data.Common.DbConnectionStringBuilder();
                    builder.ConnectionString = connStr;

                    string navCompanyName = "";
                    if (builder.ContainsKey("NAVCompanyName"))
                    {
                        navCompanyName = builder["NAVCompanyName"] as string; //get the 
                        navCompanyName = navCompanyName.Trim();
                        navCompanyName = navCompanyName.Replace("$", "");
                    }
                    if (url.Contains(navCompanyName.ToLower()) == false)
                    {
                        string msg = string.Format("Config error. NAVCompanyName: {0}  not found in url:  {1} ", navCompanyName, url);
                        msg += "There is a mismatch between company name in URL and SqlConnectionString.Nav  in appsettings.config file";
                        logger.Error(msg);
                        //throw new WebFaultException<LSOmniServiceFault>(new LSOmniServiceFault(StatusCode.Error, msg), exStatusCode);
                    }
                }
            }
        }

        #endregion private members

        #region setlocation

        private void SearchSetLocation(SearchRs rs)
        {
            if (rs == null)
                return;

            foreach (ItemCategory ic in rs.ItemCategories)
            {
                ItemCategorySetLocation(ic);
            }
            foreach (LoyItem item in rs.Items)
            {
                ItemSetLocation(item);
            }

            foreach (Notification noty in rs.Notifications)
            {
                NotificationSetLocation(noty);
            }
            foreach (ProductGroup p in rs.ProductGroups)
            {
                ProductGroupSetLocation(p);
            }

            foreach (OneList s in rs.OneLists)
            {
                OneListSetLocation(s);
            }
            foreach (Store s in rs.Stores)
            {
                StoreSetLocation(s);
            }
        }

        private void ItemCategorySetLocation(ItemCategory itemCategory)
        {
            if (itemCategory == null)
                return;

            foreach (ImageView iv in itemCategory.Images)
            {
                iv.Location = GetImageStreamUrl(iv);
            }
            foreach (ProductGroup pg in itemCategory.ProductGroups)
            {
                foreach (ImageView iv in pg.Images)
                {
                    iv.Location = GetImageStreamUrl(iv);
                }
                foreach (LoyItem it in pg.Items)
                {
                    foreach (ImageView iv in it.Images)
                    {
                        iv.Location = GetImageStreamUrl(iv);
                    }
                }
            }
        }

        private void ProductGroupSetLocation(ProductGroup productGroup)
        {
            if (productGroup == null)
                return;

            foreach (ImageView iv in productGroup.Images)
            {
                iv.Location = GetImageStreamUrl(iv);
            }
            foreach (LoyItem it in productGroup.Items)
            {
                foreach (ImageView iv in it.Images)
                {
                    iv.Location = GetImageStreamUrl(iv);
                }
            }
        }

        private void StoreSetLocation(Store store)
        {
            if (store == null || store.Images == null)
                return;

            foreach (ImageView iv in store.Images)
            {
                iv.Location = GetImageStreamUrl(iv);
            }
        }

        private void NotificationSetLocation(Notification notification)
        {
            if (notification == null)
                return;
            foreach (ImageView iv in notification.Images)
            {
                iv.Location = GetImageStreamUrl(iv);
            }
        }

        private void ItemSetLocation(LoyItem item)
        {
            if (item == null)
                return;

            foreach (ImageView iv in item.Images)
            {
                iv.Location = GetImageStreamUrl(iv);
            }
            foreach (VariantRegistration variant in item.VariantsRegistration)
            {
                foreach (ImageView iv2 in variant.Images)
                {
                    iv2.Location = GetImageStreamUrl(iv2);
                }
            }
            foreach (VariantRegistration variant in item.VariantsRegistration)
            {
                foreach (ImageView iv2 in variant.Images)
                {
                    iv2.Location = GetImageStreamUrl(iv2);
                }
            }
        }

        private void BasketSetLocation(OneList basket)
        {
            if (basket?.Items?.Count > 0)
            {
                foreach (OneListItem basketItem in basket.Items)
                {
                    if (basketItem.Item != null)
                    {
                        foreach (ImageView iv in basketItem.Item.Images)
                        {
                            iv.Location = GetImageStreamUrl(iv);
                        }
                        if (basketItem.Item.VariantsRegistration != null)
                        {
                            foreach (VariantRegistration variant in basketItem.Item.VariantsRegistration)
                            {
                                foreach (ImageView iv2 in variant.Images)
                                {
                                    iv2.Location = GetImageStreamUrl(iv2);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OneListSetLocation(OneList list)
        {
            if (list == null)
                return;

            if (list != null && list.Items != null)
            {
                foreach (OneListItem line in list.Items)
                {
                    if (line.Item != null)
                    {
                        foreach (ImageView iv in line.Item.Images)
                        {
                            iv.Location = GetImageStreamUrl(iv);
                        }
                        if (line.Item.VariantsRegistration != null)
                        {
                            foreach (VariantRegistration variant in line.Item.VariantsRegistration)
                            {
                                foreach (ImageView iv2 in variant.Images)
                                {
                                    iv2.Location = GetImageStreamUrl(iv2);
                                }
                            }
                        }
                    }
                    if (line.VariantReg != null && line.VariantReg.Images != null)
                    {
                        foreach (ImageView iv2 in line.VariantReg.Images)
                        {
                            iv2.Location = GetImageStreamUrl(iv2);
                        }
                    }
                }
            }
        }

        private void ContactSetLocation(MemberContact contact)
        {
            foreach (Notification notification in contact.Notifications)
            {
                NotificationSetLocation(notification);
            }

            this.OneListSetLocation(contact.WishList);
            this.BasketSetLocation(contact.Basket);

            foreach (PublishedOffer list in contact.PublishedOffers)
            {
                foreach (ImageView iv in list.Images)
                {
                    iv.Location = GetImageStreamUrl(iv);
                }
                foreach (OfferDetails od in list.OfferDetails)
                {
                    od.Image.Location = GetImageStreamUrl(od.Image);
                }
            }
        }

        private void MenuSetLocation(MobileMenu mobileMenu)
        {
            if (mobileMenu == null)
                return;


            for (int k = 0; k < (mobileMenu.Items != null ? mobileMenu.Items.Count : 0); k++)
            {
                //get all the iviews Location
                if (mobileMenu.Items[k].Images != null && mobileMenu.Items[k].Images.Count > 0)
                {
                    for (int m = 0; m < mobileMenu.Items[k].Images.Count; m++)
                    {
                        if (mobileMenu.Items[k].Images[m] != null)
                            mobileMenu.Items[k].Images[m].Location = GetImageStreamUrl(new ImageView(mobileMenu.Items[k].Images[m].Id));
                    }
                }
            }

            //foreach (Menu.Product prod in mobileMenu.Prods)
            for (int k = 0; k < (mobileMenu.Products != null ? mobileMenu.Products.Count : 0); k++)
            {
                //get all the iviews Location
                if (mobileMenu.Products[k].Images.Count > 0)
                {
                    for (int m = 0; m < mobileMenu.Products[k].Images.Count; m++)
                    {
                        if (mobileMenu.Products[k].Images[m] != null)
                            mobileMenu.Products[k].Images[m].Location = GetImageStreamUrl(new ImageView(mobileMenu.Products[k].Images[m].Id));
                    }
                }
            }

            //foreach (Menu.Recipe recipe in mobileMenu.Recipes)
            for (int k = 0; k < (mobileMenu.Recipes != null ? mobileMenu.Recipes.Count : 0); k++)
            {
                //get all the iviews Location
                if (mobileMenu.Recipes[k].Images.Count > 0)
                {
                    for (int m = 0; m < mobileMenu.Recipes[k].Images.Count; m++)
                    {
                        if (mobileMenu.Recipes[k].Images[m] != null)
                            mobileMenu.Recipes[k].Images[m].Location = GetImageStreamUrl(new ImageView(mobileMenu.Recipes[k].Images[m].Id));
                    }
                }
            }

            for (int k = 0; k < (mobileMenu.Deals != null ? mobileMenu.Deals.Count : 0); k++)
            {
                //get all the iviews Location
                if (mobileMenu.Deals[k].Images.Count > 0)
                {
                    for (int m = 0; m < mobileMenu.Deals[k].Images.Count; m++)
                    {
                        if (mobileMenu.Deals[k].Images[m] != null)
                            mobileMenu.Deals[k].Images[m].Location = GetImageStreamUrl(new ImageView(mobileMenu.Deals[k].Images[m].Id));
                    }
                }
            }

            for (int k = 0; k < (mobileMenu.MenuNodes != null ? mobileMenu.MenuNodes.Count : 0); k++)
            {
                if (mobileMenu.MenuNodes[k].Image != null)
                    mobileMenu.MenuNodes[k].Image.Location = GetImageStreamUrl(new ImageView(mobileMenu.MenuNodes[k].Image.Id));

                if (mobileMenu.MenuNodes[k].MenuNodes != null)
                {
                    for (int m = 0; m < mobileMenu.MenuNodes[k].MenuNodes.Count; m++)
                    {
                        if (mobileMenu.MenuNodes[k].MenuNodes[m] != null)
                            mobileMenu.MenuNodes[k].MenuNodes[m].Image.Location = GetImageStreamUrl(new ImageView(mobileMenu.MenuNodes[k].MenuNodes[m].Image.Id));
                    }
                }
            }
        }

        private void AdvertisementSetLocation(Advertisement advertisement)
        {
            if (advertisement != null && advertisement.ImageView != null)
            {
                advertisement.ImageView.Location = GetImageStreamUrl(advertisement.ImageView);
            }
        }

        #endregion setlocation
    }
}
