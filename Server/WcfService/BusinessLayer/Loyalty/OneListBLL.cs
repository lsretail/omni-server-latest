using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSOmni.Common.Util;

namespace LSOmni.BLL.Loyalty
{
    public class OneListBLL : BaseLoyBLL
    {
        private readonly IOneListRepository iRepository;

        public OneListBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
            this.iRepository = GetDbRepository<IOneListRepository>(config);
        }

        public virtual List<OneList> OneListGet(MemberContact contact, bool includeLines, Statistics stat)
        {
            List<OneList> list = new List<OneList>();
            foreach (Card card in contact.Cards)
            {
                list.AddRange(iRepository.OneListGetByCardId(card.Id, includeLines, stat));
            }
            return list;
        }

        public virtual List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines, Statistics stat)
        {
            return iRepository.OneListGetByCardId(cardId, listType, includeLines, stat);
        }

        public virtual OneList OneListGetById(string oneListId, bool includeLines, bool includeItemDetails, Statistics stat)
        {
            return iRepository.OneListGetById(oneListId, includeLines, stat);
        }

        public virtual OneList OneListSave(OneList list, bool calculate, Statistics stat)
        {
            if (list.Items == null)
                return list;

            CheckItemSetup(list);

            if (calculate)
            {
                if (list.IsHospitality)
                {
                    list = CalcuateHospList(list, stat);
                }
                else
                {
                    list = CalcuateList(list, stat);
                }
            }

            if (string.IsNullOrEmpty(list.Name))
            {
                MemberContact cont = BOLoyConnection.ContactGet(ContactSearchType.CardId, list.CardId, stat);
                if (cont != null)
                    list.Name = cont.Name;
            }

            iRepository.OneListSave(list,list.Name, calculate, stat);
            return list;
        }

        public virtual Order OneListCalculate(OneList list, Statistics stat)
        {
            if (list.Items == null)
                return new Order();

            if (list.Items.Count == 0)
                throw new LSOmniException(StatusCode.NoLinesToPost, "No Lines to calculate");

            CheckItemSetup(list);

            Order order = BOLoyConnection.BasketCalcToOrder(list, stat);
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

                    if (string.IsNullOrEmpty(oline.ItemImageId) && (olditem.Image != null))
                    {
                        oline.ItemImageId = olditem.Image.Id;
                    }
                }
            }
            return order;
        }

        public virtual OrderHosp OneListHospCalculate(OneList list, Statistics stat)
        {
            if (list.Items == null)
                return new OrderHosp();

            if (list.Items.Count == 0)
                throw new LSOmniException(StatusCode.NoLinesToPost, "No Lines to calculate");

            OrderHosp order = BOLoyConnection.HospOrderCalculate(list, stat);
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

                    if (string.IsNullOrEmpty(oline.ItemImageId) && (olditem.Image != null))
                    {
                        oline.ItemImageId = olditem.Image.Id;
                    }
                }
            }

            foreach (OrderHospLine line in order.OrderLines)
            {
                if (string.IsNullOrEmpty(line.ItemImageId) == false)
                    continue;   // load any missing images if not coming from NAV

                if (string.IsNullOrEmpty(line.VariantId))
                {
                    List<ImageView> img = BOLoyConnection.ImagesGetByKey("Item", line.ItemId, string.Empty, string.Empty, 1, false, stat);
                    if (img != null && img.Count > 0)
                        line.ItemImageId = img[0].Id;
                }
                else
                {
                    List<ImageView> img = BOLoyConnection.ImagesGetByKey("Item Variant", line.ItemId, line.VariantId, string.Empty, 1, false, stat);
                    if (img != null && img.Count > 0)
                        line.ItemImageId = img[0].Id;
                }
            }
            return order;
        }

        public virtual void OneListDeleteById(string oneListId, Statistics stat)
        {
            iRepository.OneListDeleteById(oneListId, stat);
        }

        public virtual void OneListDeleteByCardId(string cardId, Statistics stat)
        {
            iRepository.OneListRemoveLinking(cardId, stat);
            List<OneList> list = iRepository.OneListGetByCardId(cardId, false, stat);
            foreach (OneList oneList in list)
            {
                iRepository.OneListDeleteById(oneList.Id, stat);
            }
        }

        public virtual OneList OneListItemModify(string onelistId, OneListItem item, bool remove, bool calculate, Statistics stat)
        {
            OneList list = iRepository.OneListGetById(onelistId, true, stat);

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
            return OneListSave(list, calculate, stat);
        }

        public virtual void OneListLinking(string oneListId, string cardId, string email, string phone, LinkStatus status, Statistics stat)
        {
            string name = string.Empty;
            if (status == LinkStatus.Requesting)
            {
                ContactBLL cBll = new ContactBLL(config, timeoutInSeconds);
                if (string.IsNullOrEmpty(cardId))
                {
                    MemberContact member;
                    if (string.IsNullOrEmpty(email))
                    {
                        if (string.IsNullOrEmpty(phone))
                            throw new LSOmniException(StatusCode.LookupValuesMissing);

                        member = cBll.ContactGet(ContactSearchType.PhoneNumber, phone, stat);
                        if (member == null)
                            throw new LSOmniException(StatusCode.MemberAccountNotFound);

                        cardId = member.Cards[0].Id;
                        name = member.Name;
                    }
                    else
                    {
                        member = cBll.ContactGet(ContactSearchType.Email, email, stat);
                        if (member == null)
                            throw new LSOmniException(StatusCode.MemberAccountNotFound);

                        cardId = member.Cards[0].Id;
                        name = member.Name;
                    }
                }
                else
                {
                    MemberContact cont = cBll.ContactGet(ContactSearchType.CardId, cardId, stat);
                    if (cont == null)
                        throw new LSOmniException(StatusCode.MemberAccountNotFound);

                    name = cont.Name;
                }
            }
            iRepository.OneListLinking(oneListId, cardId, name, status, stat);
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

        private OneList CalcuateList(OneList list, Statistics stat)
        {
            if (list.Items.Count == 0)
                return list;

            list.TotalAmount = 0;
            list.TotalDiscAmount = 0;
            list.TotalNetAmount = 0;
            list.TotalTaxAmount = 0;

            Order calcResp = BOLoyConnection.BasketCalcToOrder(list, stat);

            list.TotalAmount = calcResp.TotalAmount;
            list.TotalNetAmount = calcResp.TotalNetAmount;
            list.TotalTaxAmount = calcResp.TotalAmount - calcResp.TotalNetAmount;
            list.TotalDiscAmount = calcResp.TotalDiscount;
            list.Currency = calcResp.Currency;

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

                OneListItem olditem = list.Items.ToList().Find(i => i.ItemId == line.ItemId && i.LineNumber == line.LineNumber);
                if (olditem != null)
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
                    item.Immutable = olditem.Immutable;
                }
                newitems.Add(item);
            }

            list.Items.Clear();
            list.Items = newitems;

            List<OneListItemDiscount> newdisclines = new List<OneListItemDiscount>();
            foreach (OrderDiscountLine disc in calcResp.OrderDiscountLines)
            {
                int lineno = disc.LineNumber;
                foreach (OrderLine oline in calcResp.OrderLines)
                {
                    if (oline.DiscountLineNumbers.Contains(disc.LineNumber))
                    {
                        lineno = oline.LineNumber;
                        break;
                    }
                }

                OneListItem line = list.Items.ToList().Find(i => i.LineNumber == lineno);
                if (line != null)
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
                    continue;
                }
            }
            return list;
        }

        private OneList CalcuateHospList(OneList list, Statistics stat)
        {
            if (list.Items.Count == 0)
                return list;

            list.TotalAmount = 0;
            list.TotalDiscAmount = 0;
            list.TotalNetAmount = 0;
            list.TotalTaxAmount = 0;

            OrderHosp calcResp = BOLoyConnection.HospOrderCalculate(list, stat);

            list.TotalAmount = calcResp.TotalAmount;
            list.TotalNetAmount = calcResp.TotalNetAmount;
            list.TotalTaxAmount = calcResp.TotalAmount - calcResp.TotalNetAmount;
            list.TotalDiscAmount = calcResp.TotalDiscount;
            list.Currency = calcResp.Currency;

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
