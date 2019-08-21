using System;
using System.Collections.Generic;

using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSOmni.BLL.Loyalty
{
    public class OneListBLL : BaseLoyBLL
    {
        private IOneListRepository iRepository;

        public OneListBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
            this.iRepository = GetDbRepository<IOneListRepository>(config);
        }

        public virtual List<OneList> OneListGet(MemberContact contact, bool includeLines)
        {
            List<OneList> list = new List<OneList>();
            foreach (Card card in contact.Cards)
            {
                list.AddRange(iRepository.OneListGetByCardId(card.Id, includeLines));
            }

            foreach (OneList lst in list)
            {
                foreach (OneListItem oitem in lst.Items)
                {
                    oitem.Item = BOLoyConnection.ItemGetById(oitem.ItemId, string.Empty, GetAppSettingCurrencyCulture(), includeLines);
                    if (string.IsNullOrWhiteSpace(oitem.VariantId) == false)
                        oitem.VariantReg = BOAppConnection.VariantRegGetById(oitem.VariantId, oitem.ItemId);
                }
            }
            return list;
        }

        public virtual List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines = false)
        {
            List<OneList> list = iRepository.OneListGetByCardId(cardId, listType, includeLines);
            foreach (OneList lst in list)
            {
                foreach (OneListItem oitem in lst.Items)
                {
                    oitem.Item = BOLoyConnection.ItemGetById(oitem.ItemId, string.Empty, GetAppSettingCurrencyCulture(), includeLines);
                    if (string.IsNullOrWhiteSpace(oitem.VariantId) == false)
                        oitem.VariantReg = BOAppConnection.VariantRegGetById(oitem.VariantId, oitem.ItemId);
                }
            }
            return list;
        }

        public virtual OneList OneListGetById(string oneListId, ListType listType, bool includeLines, bool includeItemDetails)
        {
            base.ValidateOneList(oneListId);
            OneList retlist = iRepository.OneListGetById(oneListId, listType, includeLines);
            if (retlist == null)
                throw new LSOmniServiceException(StatusCode.OneListNotFound, oneListId + " Not found");

            foreach (OneListItem it in retlist.Items)
            {
                it.Item = BOLoyConnection.ItemGetById(it.ItemId, string.Empty, string.Empty, includeItemDetails);
                if (string.IsNullOrWhiteSpace(it.UnitOfMeasureId) == false)
                {
                    it.UnitOfMeasure = BOAppConnection.UnitOfMeasureGetById(it.UnitOfMeasureId);
                }
                if (string.IsNullOrWhiteSpace(it.VariantId) == false)
                {
                    it.VariantReg = BOAppConnection.VariantRegGetById(it.VariantId, it.ItemId);
                }
            }
            return retlist;
        }

        public virtual OneList OneListSave(OneList list, bool calculate, bool includeItemDetails)
        {
            CheckItemSetup(list);

            list.TotalAmount = 0;
            list.TotalDiscAmount = 0;
            list.TotalNetAmount = 0;
            list.TotalTaxAmount = 0;

            if (calculate && list.Items.Count > 0)
            {
                Order calcResp = BOLoyConnection.BasketCalcToOrder(list);

                list.TotalAmount = calcResp.TotalAmount;
                list.TotalNetAmount = calcResp.TotalNetAmount;
                list.TotalTaxAmount = calcResp.TotalAmount - calcResp.TotalNetAmount;
                list.TotalDiscAmount = calcResp.TotalDiscount;

                List<OneListItem> newitems = new List<OneListItem>();
                foreach (OrderLine line in calcResp.OrderLines)
                {
                    OneListItem olditem = list.Items.Find(i => i.Item.Id == line.ItemId);

                    OneListItem item = new OneListItem();
                    item.DisplayOrderId = line.LineNumber;
                    item.ItemId = line.ItemId;
                    item.Item = new LoyItem(line.ItemId);
                    item.Item.Description = line.ItemDescription;
                    item.Item.Images.Add(new ImageView(line.ItemImageId));
                    item.UnitOfMeasureId = line.UomId;
                    if (string.IsNullOrEmpty(item.UnitOfMeasureId) && string.IsNullOrEmpty(olditem.UnitOfMeasureId) == false)
                        item.UnitOfMeasureId = olditem.UnitOfMeasureId;

                    item.VariantId = line.VariantId;
                    if (string.IsNullOrEmpty(item.VariantId) && string.IsNullOrEmpty(olditem.VariantId) == false)
                        item.VariantId = olditem.VariantId;

                    item.BarcodeId = olditem.BarcodeId;

                    item.Quantity = line.Quantity;
                    item.NetPrice = line.NetPrice;
                    item.Price = line.Price;
                    item.Amount = line.Amount;
                    item.NetAmount = line.NetAmount;
                    item.TaxAmount = line.TaxAmount;
                    item.DiscountAmount = line.DiscountAmount;
                    item.DiscountPercent = line.DiscountPercent;

                    item.OnelistItemDiscounts = new List<OneListItemDiscount>();
                    newitems.Add(item);
                }

                list.Items.Clear();
                list.Items = newitems;

                foreach (OrderDiscountLine disc in calcResp.OrderDiscountLines)
                {
                    OneListItem line = list.Items.Find(i => i.DisplayOrderId == disc.LineNumber / 10000);
                    if (line == null)
                    {
                        line = list.Items.Find(i => i.DisplayOrderId == disc.LineNumber);
                        if (line == null)
                            continue;
                    }

                    OneListItemDiscount discount = new OneListItemDiscount();
                    discount.Description = disc.Description;
                    discount.DiscountAmount = disc.DiscountAmount;
                    discount.DiscountPercent = disc.DiscountPercent;
                    discount.DiscountType = disc.DiscountType;
                    discount.LineNumber = disc.LineNumber;
                    discount.No = disc.No;
                    discount.OfferNumber = disc.OfferNumber;
                    discount.PeriodicDiscGroup = disc.PeriodicDiscGroup;
                    discount.PeriodicDiscType = disc.PeriodicDiscType;
                    discount.Quantity = line.Quantity;
                    line.OnelistItemDiscounts.Add(discount);
                }
            }

            iRepository.OneListSave(list, calculate);
            return OneListGetById(list.Id, list.ListType, true, includeItemDetails);
        }

        public virtual Order OneListCalculate(OneList list)
        {
            CheckItemSetup(list);

            // temp solution till we get this from NAV
            Order order = BOLoyConnection.BasketCalcToOrder(list);
            foreach (OrderLine line in order.OrderLines)
            {
                if (string.IsNullOrEmpty(line.ItemImageId) == false)
                    continue;   // load any missing images if not coming from NAV

                if (string.IsNullOrEmpty(line.VariantId))
                {
                    List<ImageView> img = BOLoyConnection.ImagesGetByKey("Item", line.ItemId, string.Empty, string.Empty, 1, false);
                    if (img != null && img.Count > 0)
                        line.ItemImageId = img[0].Id;
                }
                else
                {
                    List<ImageView> img = BOLoyConnection.ImagesGetByKey("Item Variant", line.ItemId, line.VariantId, string.Empty, 1, false);
                    if (img != null && img.Count > 0)
                        line.ItemImageId = img[0].Id;
                }
            }
            return order;
        }

        public virtual void OneListDeleteById(string oneListId, ListType listType)
        {
            base.ValidateOneList(oneListId);
            iRepository.OneListDeleteById(oneListId, listType);
        }

        private void CheckItemSetup(OneList list)
        {
            // check item setup
            foreach (OneListItem line in list.Items)
            {
                if (string.IsNullOrWhiteSpace(line.ItemId))
                {
                    if (line.Item != null && line.Item.Id != null)
                        line.ItemId = line.Item.Id;
                    else
                        line.ItemId = string.Empty;
                }

                if (string.IsNullOrWhiteSpace(line.VariantId))
                {
                    if (line.VariantReg != null && line.VariantReg.Id != null)
                        line.VariantId = line.VariantReg.Id;
                    else
                        line.VariantId = string.Empty;
                }

                if (string.IsNullOrWhiteSpace(line.UnitOfMeasureId))
                {
                    if (line.UnitOfMeasure != null && line.UnitOfMeasure.Id != null)
                        line.UnitOfMeasureId = line.UnitOfMeasure.Id;
                    else
                        line.UnitOfMeasureId = string.Empty;
                }
            }
        }
    }
}
