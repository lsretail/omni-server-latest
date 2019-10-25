using System;
using System.Collections.Generic;

using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
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
                list = CalcuateList(list);

            MemberContact cont = BOLoyConnection.ContactGetByCardId(list.CardId, 0, false);

            iRepository.OneListSave(list, (cont == null) ? string.Empty : cont.Name, calculate);
            return OneListGetById(list.Id, true, includeItemDetails);
        }

        public virtual Order OneListCalculate(OneList list)
        {
            if (list.Items == null)
                return new Order();

            if (list.Items.Count == 0)
                throw new LSOmniServiceException(StatusCode.NoLinesToPost, "No Lines to calculate");

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

        public virtual void OneListDeleteById(string oneListId)
        {
            iRepository.OneListDeleteById(oneListId);
        }

        public virtual OneList OneListItemModify(string onelistId, OneListItem item, bool remove, bool calculate)
        {
            OneList list = iRepository.OneListGetById(onelistId, true);
            if (remove)
            {
                int i = list.Items.FindIndex(t => t.Id == item.Id);
                if (i < 0)
                    throw new LSOmniException(StatusCode.ItemNotFound, string.Format("OneList Item {0} not found", item.Id));

                list.Items.RemoveAt(i);
            }
            else
            {
                if (item.UnitOfMeasureId == null)
                    item.UnitOfMeasureId = string.Empty;
                if (item.VariantId == null)
                    item.VariantId = string.Empty;

                int i = list.Items.FindIndex(t => t.ItemId == item.ItemId && t.VariantId == item.VariantId && t.UnitOfMeasureId == item.UnitOfMeasureId);
                if (i >= 0)
                {
                    list.Items[i].Quantity += item.Quantity;
                }
                else
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

            List<OneListItem> newitems = new List<OneListItem>();
            foreach (OrderLine line in calcResp.OrderLines)
            {
                OneListItem olditem = list.Items.Find(i => i.ItemId == line.ItemId);

                OneListItem item = new OneListItem();
                item.DisplayOrderId = olditem.DisplayOrderId;
                item.ItemId = line.ItemId;
                item.ItemDescription = line.ItemDescription;

                if (string.IsNullOrEmpty(line.ItemImageId))
                    item.Image = olditem.Image;
                else
                    item.Image = new ImageView(line.ItemImageId);

                item.UnitOfMeasureId = line.UomId;
                if (string.IsNullOrEmpty(item.UnitOfMeasureId) && string.IsNullOrEmpty(olditem.UnitOfMeasureId) == false)
                    item.UnitOfMeasureId = olditem.UnitOfMeasureId;

                item.VariantId = line.VariantId;
                item.VariantDescription = line.VariantDescription;
                if (string.IsNullOrEmpty(item.VariantId) && string.IsNullOrEmpty(olditem.VariantId) == false)
                {
                    item.VariantId = olditem.VariantId;
                    item.VariantDescription = olditem.VariantDescription;
                }

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
            return list;
        }
    }
}
