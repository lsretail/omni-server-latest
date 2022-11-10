using System;
using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSOmni.Common.Util;

namespace LSOmni.BLL.Loyalty
{
    public class StoreBLL : BaseLoyBLL
    {
        public StoreBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
        }

        public virtual List<Store> StoresGetAll(bool clickAndCollectOnly, Statistics stat)
        {
            return BOLoyConnection.StoresGetAll(clickAndCollectOnly, stat);
        }

        public virtual Store StoreGetById(string id, bool details, Statistics stat)
        {
            return BOLoyConnection.StoreGetById(id, details, stat);
        }

        public virtual List<Store> StoresGetByCoordinates(double latitude, double longitude, double maxDistance, Statistics stat)
        {
            List<Store> list = BOLoyConnection.StoresGetAll(true, stat);
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
    }
}
