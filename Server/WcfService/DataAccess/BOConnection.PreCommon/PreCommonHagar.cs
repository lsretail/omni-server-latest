using LSOmni.Common.Util;
using LSOmni.DataAccess.BOConnection.PreCommon.Mapping;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using System;
using System.Collections.Generic;

namespace LSOmni.DataAccess.BOConnection.PreCommon
{
    public partial class PreCommonBase
    {
        public virtual List<InventoryResponse> GetAXInventory(List<InventoryRequest> items, string storeId, string locationId, Statistics stat)
        {
            List<InventoryResponse> list = new List<InventoryResponse>();
            string respCode = string.Empty;
            string errorText = string.Empty;

            logger.StatisticStartSub(true, ref stat, out int index);

            List<HagarWS.InventoryBufferIn> lines = new List<HagarWS.InventoryBufferIn>();
            foreach (InventoryRequest item in items)
            {
                HagarWS.InventoryBufferIn buf = lines.Find(b => b.Number.Equals(item.ItemId) && b.Variant.Equals(item.VariantId ?? string.Empty));
                if (buf == null)
                {
                    lines.Add(new HagarWS.InventoryBufferIn()
                    {
                        Number = item.ItemId,
                        Variant = item.VariantId ?? string.Empty
                    });
                }
            }

            HagarWS.RootGetInventoryMultipleIn rootin = new HagarWS.RootGetInventoryMultipleIn()
            {
                InventoryBufferIn = lines.ToArray()
            };

            HagarWS.RootGetInventoryMultipleOut rootout = new HagarWS.RootGetInventoryMultipleOut();

            hagarWS.GetInventoryMultiple(storeId, locationId, rootin, ref respCode, ref errorText, ref rootout);
            HandleWS2ResponseCode("Hagar.GetInventoryMultiple", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "Hagar.GetInventoryMultiple Response - " + Serialization.ToXml(rootout, true));

            if (rootout.InventoryBufferOut == null)
            {
                logger.StatisticEndSub(ref stat, index);
                return list;
            }

            foreach (HagarWS.InventoryBufferOut buffer in rootout.InventoryBufferOut)
            {
                list.Add(new InventoryResponse()
                {
                    ItemId = buffer.Number,
                    VariantId = buffer.Variant,
                    QtyInventory = buffer.Inventory,
                    StoreId = buffer.Store
                });
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public virtual bool CheckCreditLimit(string cardId, decimal amount, out decimal availAmount, out string message, Statistics stat)
        {
            string respCode = string.Empty;
            string errorText = string.Empty;
            bool saleIsApproved = false;
            availAmount = 0;

            logger.StatisticStartSub(true, ref stat, out int index);
            logger.Debug(config.LSKey.Key, "Hagar.CheckCreditLimit Request - cardId:{0} amt:{1}", cardId, amount);
            skCreditWS.CallCustomerCreditCheck(cardId, amount, ref saleIsApproved, ref availAmount, ref respCode, ref errorText);
            message = errorText;
            HandleWS2ResponseCode("Hagar.CheckCreditLimit", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "Hagar.CheckCreditLimit Response - saleIsApproved:{0}  availAmt:{1} errorText:{2}", saleIsApproved, availAmount, errorText);
            logger.StatisticEndSub(ref stat, index);
            return saleIsApproved;
        }
    }
}
