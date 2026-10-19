using LSRetail.Omni.Domain.DataModel.Base.Requests;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace LSOmni.DataAccess.BOConnection.PreCommon.JMapping
{
    public class InventoryJMapping : BaseJMapping
    {
        public InventoryJMapping(bool json)
        {
            IsJson = json;
        }

        public List<InventoryResponse> GetItemInventory(string ret, out string resCode, out string resErr)
        {
            return GetItemInventoryList(ret, "actualInventory", out resCode, out resErr);
        }

        public List<InventoryResponse> GetInventoryMultiple(string ret, out string resCode, out string resErr)
        {
            return GetItemInventoryList(ret, "inventory", out resCode, out resErr);
        }

        private List<InventoryResponse> GetItemInventoryList(string ret, string qtyField, out string resCode, out string resErr)
        {
            List<InventoryResponse> list = new List<InventoryResponse>();
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null || isArray == false)
                return list;

            JArray items = JArray.Parse(result);
            foreach (JObject rec in items)
            {
                list.Add(new InventoryResponse()
                {
                    ItemId = rec.Value<string>("itemNo"),
                    VariantId = rec.Value<string>("variantCode"),
                    BaseUnitOfMeasure = rec.Value<string>("baseUnitOfMeasure"),
                    QtyInventory = rec.Value<decimal?>(qtyField) ?? 0M,
                    StoreId = rec.Value<string>("storeNo")
                });
            }
            return list;
        }
    }
}
