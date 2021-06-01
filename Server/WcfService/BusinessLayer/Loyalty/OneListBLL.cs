using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
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
            return list;
        }

        public virtual List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines = false)
        {
            return iRepository.OneListGetByCardId(cardId, listType, includeLines);
        }

        public virtual OneList OneListGetById(string oneListId, bool includeLines, bool includeItemDetails)
        {
            return iRepository.OneListGetById(oneListId, includeLines);
        }

        public virtual OneList OneListSave(OneList list, bool calculate, bool includeItemDetails)
        {
            if (list.Items == null)
                return list;

            CheckItemSetup(list);

            if (calculate)
            {
                if (list.HospitalityMode == HospMode.None)
                {
                    list = CalcuateList(list);
                }
                else
                {
                    list = CalcuateHospList(list);
                }
            }

            MemberContact cont = BOLoyConnection.ContactGetByCardId(list.CardId, 0, false);

            iRepository.OneListSave(list, (cont == null) ? string.Empty : cont.Name, calculate);
            return list;
        }

        public virtual Order OneListCalculate(OneList list)
        {
            if (list.Items == null)
                return new Order();

            if (list.Items.Count == 0)
                throw new LSOmniException(StatusCode.NoLinesToPost, "No Lines to calculate");

            CheckItemSetup(list);

            Order order = BOLoyConnection.BasketCalcToOrder(list);
            foreach (OneListItem olditem in list.Items)
            {
                OrderLine oline = order.OrderLines.Find(l => l.ItemId == olditem.ItemId && l.LineNumber == olditem.LineNumber);
                if (oline != null)
                {
                    if (string.IsNullOrEmpty(oline.VariantId) && string.IsNullOrEmpty(olditem.VariantId) == false)
                    {
                        oline.VariantId = olditem.VariantId;
                        oline.VariantDescription = olditem.VariantDescription;
                    }

                    if (string.IsNullOrEmpty(oline.UomId) && string.IsNullOrEmpty(olditem.UnitOfMeasureId) == false)
                    {
                        oline.UomId = olditem.UnitOfMeasureId;
                    }
                }
            }

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

        public virtual OrderHosp OneListHospCalculate(OneList list)
        {
            if (list.Items == null)
                return new OrderHosp();

            if (list.Items.Count == 0)
                throw new LSOmniException(StatusCode.NoLinesToPost, "No Lines to calculate");

            OrderHosp order = BOLoyConnection.HospOrderCalculate(list);
            foreach (OneListItem olditem in list.Items)
            {
                OrderHospLine oline = order.OrderLines.Find(l => l.ItemId == olditem.ItemId && l.LineNumber == olditem.LineNumber);
                if (oline != null)
                {
                    if (string.IsNullOrEmpty(oline.VariantId) && string.IsNullOrEmpty(olditem.VariantId) == false)
                    {
                        oline.VariantId = olditem.VariantId;
                        oline.VariantDescription = olditem.VariantDescription;
                    }

                    if (string.IsNullOrEmpty(oline.UomId) && string.IsNullOrEmpty(olditem.UnitOfMeasureId) == false)
                    {
                        oline.UomId = olditem.UnitOfMeasureId;
                    }
                }
            }

            foreach (OrderHospLine line in order.OrderLines)
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


        public virtual void OneListDeleteById(string oneListId)
        {
            iRepository.OneListDeleteById(oneListId);
        }

        public virtual OneList OneListItemModify(string onelistId, OneListItem item, bool remove, bool calculate)
        {
            OneList list = iRepository.OneListGetById(onelistId, true);

            bool notfound = true;
            if (remove)
            {
                for (int i = 0; i < list.Items.Count; i++)
                {
                    if (list.Items[i].Id == item.Id)
                    {
                        list.Items.RemoveAt(i);
                        notfound = false;
                        break;
                    }
                }

                if (notfound)
                    throw new LSOmniException(StatusCode.OneListNotFound, string.Format("OneList Item {0} not found", item.Id));
            }
            else
            {
                if (item.UnitOfMeasureId == null)
                    item.UnitOfMeasureId = string.Empty;
                if (item.VariantId == null)
                    item.VariantId = string.Empty;

                for (int i = 0; i < list.Items.Count; i++)
                {
                    if (list.Items[i].ItemId == item.ItemId && list.Items[i].VariantId == (item.VariantId ?? string.Empty) && list.Items[i].UnitOfMeasureId == (item.UnitOfMeasureId ?? string.Empty))
                    {
                        list.Items[i].Quantity += item.Quantity;
                        notfound = false;
                        break;
                    }
                }

                if (notfound)
                {
                    list.Items.Add(item);
                }
            }
            return OneListSave(list, calculate, true);
        }

        public virtual void OneListLinking(string oneListId, string cardId, string email, LinkStatus status)
        {
            string name = string.Empty;
            if (status == LinkStatus.Requesting)
            {
                ContactBLL cBll = new ContactBLL(config, timeoutInSeconds);

                if (string.IsNullOrEmpty(cardId))
                {
                    if (string.IsNullOrEmpty(email))
                        throw new LSOmniException(StatusCode.EmailMissing);

                    List<MemberContact> members = cBll.ContactSearch(ContactSearchType.Email, email, 0, true);
                    if (members.Count > 0)
                    {
                        cardId = members[0].Cards[0].Id;
                        name = members[0].Name;
                    }
                    else
                    {
                        // TODO: send EMail to invite to system?
                        return;
                    }
                }
                else
                {
                    MemberContact cont = cBll.ContactGetByCardId(cardId, false);
                    if (cont == null)
                        throw new LSOmniException(StatusCode.MemberCardNotFound);

                    name = cont.Name;
                }
            }
            iRepository.OneListLinking(oneListId, cardId, name, status);

            // TODO: send push notification to member device
        }

        private void CheckItemSetup(OneList list)
        {
            foreach (OneListItem line in list.Items)
            {
                // check item setup
                if (string.IsNullOrWhiteSpace(line.ItemId))
                    line.ItemId = string.Empty;

                if (string.IsNullOrWhiteSpace(line.VariantId))
                    line.VariantId = string.Empty;

                if (string.IsNullOrWhiteSpace(line.UnitOfMeasureId))
                    line.UnitOfMeasureId = string.Empty;
            }
        }

        private OneList CalcuateList(OneList list)
        {
            if (list.Items.Count == 0)
                return list;

            list.TotalAmount = 0;
            list.TotalDiscAmount = 0;
            list.TotalNetAmount = 0;
            list.TotalTaxAmount = 0;

            Order calcResp = BOLoyConnection.BasketCalcToOrder(list);

            list.TotalAmount = calcResp.TotalAmount;
            list.TotalNetAmount = calcResp.TotalNetAmount;
            list.TotalTaxAmount = calcResp.TotalAmount - calcResp.TotalNetAmount;
            list.TotalDiscAmount = calcResp.TotalDiscount;

            ObservableCollection<OneListItem> newitems = new ObservableCollection<OneListItem>();
            foreach (OrderLine line in calcResp.OrderLines)
            {
                OneListItem item = new OneListItem()
                {
                    ItemId = line.ItemId,
                    ItemDescription = line.ItemDescription,
                    UnitOfMeasureId = line.UomId,
                    VariantId = line.VariantId,
                    VariantDescription = line.VariantDescription,
                    LineNumber = line.LineNumber,

                    Image = new ImageView(line.ItemImageId),

                    Quantity = line.Quantity,
                    NetPrice = line.NetPrice,
                    Price = line.Price,
                    Amount = line.Amount,
                    NetAmount = line.NetAmount,
                    TaxAmount = line.TaxAmount,
                    DiscountAmount = line.DiscountAmount,
                    DiscountPercent = line.DiscountPercent,

                    OnelistItemDiscounts = new List<OneListItemDiscount>()
                };

                foreach (OneListItem olditem in list.Items)
                {
                    if (olditem.ItemId == line.ItemId && olditem.LineNumber == line.LineNumber)
                    {
                        if (string.IsNullOrEmpty(line.ItemImageId))
                            item.Image = olditem.Image;

                        if (string.IsNullOrEmpty(item.VariantId) && string.IsNullOrEmpty(olditem.VariantId) == false)
                        {
                            item.VariantId = olditem.VariantId;
                            item.VariantDescription = olditem.VariantDescription;
                        }

                        if (string.IsNullOrEmpty(item.UnitOfMeasureId) && string.IsNullOrEmpty(olditem.UnitOfMeasureId) == false)
                        {
                            item.UnitOfMeasureId = olditem.UnitOfMeasureId;
                            item.UnitOfMeasureDescription = olditem.UnitOfMeasureDescription;
                        }

                        item.BarcodeId = olditem.BarcodeId;
                        item.ProductGroup = olditem.ProductGroup;
                        item.ItemCategory = olditem.ItemCategory;
                        break;
                    }
                }
                newitems.Add(item);
            }

            list.Items.Clear();
            list.Items = newitems;

            foreach (OrderDiscountLine disc in calcResp.OrderDiscountLines)
            {
                foreach (OneListItem line in list.Items)
                {
                    if (line.LineNumber == disc.LineNumber / 10000 || line.LineNumber == disc.LineNumber)
                    {
                        OneListItemDiscount discount = new OneListItemDiscount()
                        {
                            Description = disc.Description,
                            DiscountAmount = disc.DiscountAmount,
                            DiscountPercent = disc.DiscountPercent,
                            DiscountType = disc.DiscountType,
                            LineNumber = disc.LineNumber,
                            No = disc.No,
                            OfferNumber = disc.OfferNumber,
                            PeriodicDiscGroup = disc.PeriodicDiscGroup,
                            PeriodicDiscType = disc.PeriodicDiscType,
                            Quantity = line.Quantity
                        };
                        line.OnelistItemDiscounts.Add(discount);
                    }
                }
            }
            return list;
        }

        private OneList CalcuateHospList(OneList list)
        {
            if (list.Items.Count == 0)
                return list;

            list.TotalAmount = 0;
            list.TotalDiscAmount = 0;
            list.TotalNetAmount = 0;
            list.TotalTaxAmount = 0;

            OrderHosp calcResp = BOLoyConnection.HospOrderCalculate(list);

            list.TotalAmount = calcResp.TotalAmount;
            list.TotalNetAmount = calcResp.TotalNetAmount;
            list.TotalTaxAmount = calcResp.TotalAmount - calcResp.TotalNetAmount;
            list.TotalDiscAmount = calcResp.TotalDiscount;

            ObservableCollection<OneListItem> newitems = new ObservableCollection<OneListItem>();
            foreach (OrderHospLine line in calcResp.OrderLines)
            {
                OneListItem item = new OneListItem()
                {
                    ItemId = line.ItemId,
                    IsADeal = line.IsADeal,
                    ItemDescription = line.ItemDescription,
                    LineNumber = line.LineNumber,

                    Image = new ImageView(line.ItemImageId),
                    UnitOfMeasureId = line.UomId,
                    VariantId = line.VariantId,
                    VariantDescription = line.VariantDescription,

                    Quantity = line.Quantity,
                    NetPrice = line.NetPrice,
                    Price = line.Price,
                    Amount = line.Amount,
                    NetAmount = line.NetAmount,
                    TaxAmount = line.TaxAmount,
                    DiscountAmount = line.DiscountAmount,
                    DiscountPercent = line.DiscountPercent
                };

                foreach (OneListItem olditem in list.Items)
                {
                    if (olditem.ItemId == line.ItemId && olditem.LineNumber == line.LineNumber)
                    {
                        if (string.IsNullOrEmpty(line.ItemImageId))
                            item.Image = olditem.Image;

                        if (string.IsNullOrEmpty(item.VariantId) && string.IsNullOrEmpty(olditem.VariantId) == false)
                        {
                            item.VariantId = olditem.VariantId;
                            item.VariantDescription = olditem.VariantDescription;
                        }

                        if (string.IsNullOrEmpty(item.UnitOfMeasureId) && string.IsNullOrEmpty(olditem.UnitOfMeasureId) == false)
                        {
                            item.UnitOfMeasureId = olditem.UnitOfMeasureId;
                            item.UnitOfMeasureDescription = olditem.UnitOfMeasureDescription;
                        }

                        item.BarcodeId = olditem.BarcodeId;
                        item.ProductGroup = olditem.ProductGroup;
                        item.ItemCategory = olditem.ItemCategory;
                    }
                }

                item.OnelistSubLines = new List<OneListItemSubLine>();
                foreach (OrderHospSubLine sline in line.SubLines)
                {
                    item.OnelistSubLines.Add(new OneListItemSubLine()
                    {
                        LineNumber = sline.LineNumber,
                        DealLineId = Convert.ToInt32(sline.DealLineId),
                        DealModLineId = Convert.ToInt32(sline.DealModifierLineId),
                        Description = sline.Description,
                        ItemId = sline.ItemId,
                        ModifierGroupCode = sline.ModifierGroupCode,
                        ModifierSubCode = sline.ModifierSubCode,
                        Quantity = sline.Quantity,
                        Type = sline.Type,
                        Uom = sline.Uom,
                        VariantDescription = sline.VariantDescription,
                        VariantId = sline.VariantId
                    });
                }
                newitems.Add(item);
            }

            list.Items.Clear();
            list.Items = newitems;
            return list;
        }
    }
}
