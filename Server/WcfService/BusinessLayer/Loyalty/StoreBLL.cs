using System;
using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSOmni.Common.Util;
using System.IO;
using System.Xml.Linq;

namespace LSOmni.BLL.Loyalty
{
    public class StoreBLL : BaseLoyBLL
    {
        public StoreBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
        }

        public virtual List<Store> StoresGetAll(StoreGetType storeType, bool inclDetails, bool inclImages, Statistics stat)
        {
            List<Store> stores = BOLoyConnection.StoresGetAll(storeType, inclDetails, stat);
            if (inclImages || inclDetails)
            {
                ImageBLL imgBLL = new ImageBLL(config);
                foreach (Store store in stores)
                {
                    store.Images = imgBLL.ImagesGetByKey("LSC Store", store.Id, string.Empty, string.Empty, 0, inclImages, stat);
                    store.StoreServices = StoreServicesGetByStoreId(store.Id, stat);
                }
            }
            return stores;
        }

        public virtual Store StoreGetById(string id, bool inclImages, Statistics stat)
        {
            Store store = BOLoyConnection.StoreGetById(id, stat);
            ImageBLL imgBLL = new ImageBLL(config);
            store.Images = imgBLL.ImagesGetByKey("LSC Store", store.Id, string.Empty, string.Empty, 0, inclImages, stat);
            store.StoreServices = StoreServicesGetByStoreId(store.Id, stat);
            return store;
        }

        public virtual List<Store> StoresGetByCoordinates(double latitude, double longitude, double maxDistance, Statistics stat)
        {
            List<Store> list = BOLoyConnection.StoresGetAll(StoreGetType.ClickAndCollect, true, stat);
            Store.Position startpos = new Store.Position()
            {
                Latitude = latitude,
                Longitude = longitude
            };

            List<Store> stores = new List<Store>();
            foreach (Store store in list)
            {
                Store.Position endpos = new Store.Position()
                {
                    Latitude = store.Latitude,
                    Longitude = store.Longitude
                };

                if (store.CalculateDistance(startpos, endpos, Store.DistanceType.Kilometers) <= maxDistance)
                    stores.Add(store);
            }
            return stores;
        }

        public virtual List<Store> StoresGetbyItemInStock(string itemId, string variantId, double latitude, double longitude, double maxDistance, Statistics stat)
        {
            List<Store> storeListOut = new List<Store>();
            List<string> locationIds = new List<string>();

            //get locationIds from latitude and longitude(if != 0) ..
            if (latitude == 0 || longitude == 0)
                maxDistance = 9000000000000.0; //so all stores are returned

            List<Store> storeListByCoords = StoresGetByCoordinates(latitude, longitude, maxDistance, stat);
            foreach (Store store in storeListByCoords)
            {
                locationIds.Add(store.Id);
            }

            List<InventoryResponse> storeList = BOAppConnection.ItemInStockGet(itemId, variantId, 0, locationIds, true, stat);
            //now add the distance before sending it back to client
            foreach (InventoryResponse store in storeList)
            {
                foreach (Store storeWithDistance in storeListByCoords)
                {
                    if (storeWithDistance.Id == store.StoreId)
                    {
                        storeListOut.Add(storeWithDistance);
                        break;
                    }
                }
            }
            return storeListOut;
        }

        public virtual List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1, Statistics stat)
        {
            return BOLoyConnection.ReturnPolicyGet(storeId, storeGroupCode, itemCategory, productGroup, itemId, variantCode, variantDim1, stat);
        }

        /*
         <?xml version="1.0" encoding="utf-8"?> 
         <StoreServices Version="">
             <StoreService>
               <StoreId>S0001</StoreId>
                 <Description>Free Wi-Fi</Description>
                 <StoreServiceType enum="none=0,FreeWiFi=2,DriveThruWindow=3,GiftCard=4,PlayPlace=5,FreeRefill=6">2</StoreServiceType>
             </StoreService>
             <StoreService>
               <StoreId>S0001</StoreId>
                 <Description>PlayPlaces. Super-fun for the kids, stress-free for you!</Description>
                 <StoreServiceType enum="none=0,FreeWiFi=2,DriveThruWindow=3,GiftCard=4,PlayPlace=5,FreeRefill=6">5</StoreServiceType>
             </StoreService>
             <StoreService>
               <StoreId>S0001</StoreId>
                 <Description>Drive thru window</Description>
                 <StoreServiceType enum="none=0,FreeWiFi=2,DriveThruWindow=3,GiftCard=4,PlayPlace=5,FreeRefill=6">3</StoreServiceType>
             </StoreService>	
             <StoreService>
               <StoreId>S0001</StoreId>
                 <Description>Free refill</Description>
                 <StoreServiceType enum="none=0,FreeWiFi=2,DriveThruWindow=3,GiftCard=4,PlayPlace=5,FreeRefill=6">6</StoreServiceType>
             </StoreService>		
         </StoreServices>
         */

        private List<StoreServices> StoreServicesGetByStoreId(string storeId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            List<StoreServices> serviceListFound = new List<StoreServices>();
            string fullFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Xml", "navdata_StoreFeatures.xml");
            if (File.Exists(fullFileName) == false)
            {
                logger.StatisticEndSub(ref stat, index);
                return serviceListFound;
            }

            string xml = File.ReadAllText(fullFileName);
            XDocument doc = XDocument.Parse(xml, LoadOptions.None);
            List<StoreServices> serviceList = new List<StoreServices>();
            IEnumerable<XElement> elServices = doc.Element("StoreServices").Descendants("StoreService");
            foreach (XElement elSer in elServices)
            {
                StoreServices service = new StoreServices();
                service.StoreId = XMLHelper.GetXMLValue(elSer, "StoreId");
                service.Description = XMLHelper.GetXMLValue(elSer, "Description");
                service.StoreServiceType = (StoreServiceType)Convert.ToInt32(XMLHelper.GetXMLValue(elSer, "StoreServiceType"));
                serviceList.Add(service);
            }

            foreach (StoreServices serv in serviceList)
            {
                if (serv.StoreId.ToLowerInvariant() == storeId.ToLowerInvariant())
                    serviceListFound.Add(serv);
            }
            logger.StatisticEndSub(ref stat, index);
            return serviceListFound;
        }
    }
}
