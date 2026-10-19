using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LSOmni.DataAccess.BOConnection.PreCommon.Mapping25
{
    public class WishListMapping25 : BaseMapping25
    {
        public WishListMapping25(Version lscVersion, bool json)
        {
            LSCVersion = lscVersion;
            IsJson = json;
        }

        public LSCentral25.RootWishLists MapOneListToRoot(OneList oneList)
        {
            LSCentral25.RootWishLists root = new LSCentral25.RootWishLists();
            List<LSCentral25.WishListHeader> headers = new List<LSCentral25.WishListHeader>()
            {
                new LSCentral25.WishListHeader()
                {
                    WishListNo = string.Empty,
                    CalculationType = (oneList.IsHospitality) ? "1" : "0",
                    WishListName = oneList.Description,
                    NoOfTimesVisited = 0, // Not tracked in LS Omni
                    Barcode = null, // Not supported in LS Omni
                    ContactNo = oneList.CustomerId,
                    DateLastUsed = oneList.CreateDate,
                    DateCreated = oneList.CreateDate,
                }
            };
            root.WishListHeader = headers.ToArray();

            List<LSCentral25.WishListLine> lines = new List<LSCentral25.WishListLine>();
            foreach (OneListItem item in oneList.Items)
            {
                lines.Add(new LSCentral25.WishListLine()
                {
                    WishListNo = string.Empty,
                    ItemNo = item.ItemId,
                    VariantCode = item.VariantId,
                    Quantity = item.Quantity,
                    UnitOfMeasureCode = item.UnitOfMeasureId,
                    IsDeal = item.IsADeal,
                    Barcode = item.BarcodeId,
                    ItemDescription = item.ItemDescription,
                    VariantDescription = item.VariantDescription,
                    DateCreated = item.CreateDate,
                    LineNo = item.LineNumber
                });
            }
            root.WishListLine = lines.ToArray();
            return root;
        }

        public List<OneList> MapRootToOneList(LSCentral25.RootWishLists1 root)
        {
            if (root == null || root.WishListHeader == null)
                return new List<OneList>();

            List<OneList> list = new List<OneList>();
            foreach (LSCentral25.WishListHeader1 header in root.WishListHeader)
            {
                OneList onelist = new OneList()
                {
                    Id = header.WishListNo,
                    ListNo = header.WishListNo,
                    ListType = ListType.Wish,
                    IsHospitality = (header.CalculationType == "1"),
                    Description = header.WishListName,
                    CustomerId = header.ContactNo,
                    CreateDate = header.DateCreated,
                };
                list.Add(onelist);

                onelist.Items = new ObservableCollection<OneListItem>();
                if (root.WishListLine != null)
                {
                    List<LSCentral25.WishListLine1> lines = root.WishListLine.ToList().FindAll(l => l.WishListNo == header.WishListNo);
                    foreach (LSCentral25.WishListLine1 line in lines)
                    {
                        onelist.Items.Add(new OneListItem()
                        {
                            ItemId = line.ItemNo,
                            ItemDescription = line.ItemDescription,
                            VariantId = line.VariantCode,
                            VariantDescription = line.VariantDescription,
                            UnitOfMeasureId = line.UnitOfMeasureCode,
                            UnitOfMeasureDescription = line.UnitOfMeasureCode,
                            IsADeal = line.IsDeal,
                            LineNumber = line.LineNo,
                            Quantity = line.Quantity
                        });
                    }
                }

                onelist.CardLinks = new List<OneListLink>();
                if (root.WishListLink != null)
                {
                    List<LSCentral25.WishListLink> links = root.WishListLink.ToList().FindAll(l => l.WishListNo == header.WishListNo);
                    foreach (LSCentral25.WishListLink link in links)
                    {
                        onelist.CardLinks.Add(new OneListLink()
                        {
                            CardId = link.CardNo,
                            Name = onelist.Name,
                            Owner = link.Owner,
                            Status = (LinkStatus)ConvertTo.SafeInt(link.Status)
                        });

                        if (link.Owner)
                            onelist.CardId = link.CardNo;
                    }
                }
            }
            return list;
        }

        public LSCentral25.RootWishLists2 MapListLineToRoot(string listId, OneListItem line)
        {
            LSCentral25.RootWishLists2 root = new LSCentral25.RootWishLists2();
            List<LSCentral25.WishListLine2> lines = new List<LSCentral25.WishListLine2>()
            {
                new LSCentral25.WishListLine2()
                {
                    WishListNo = listId,
                    ItemNo = line.ItemId,
                    VariantCode = line.VariantId,
                    Quantity = line.Quantity,
                    UnitOfMeasureCode = line.UnitOfMeasureId,
                    IsDeal = line.IsADeal,
                    Barcode = line.BarcodeId,
                    DateCreated = line.CreateDate,
                    LineNo = line.LineNumber
                }
            };
            root.WishListLine = lines.ToArray();
            return root;
        }

        public LSCentral25.RootWishListLinks MapListLinkToRoot(string listId, string cardId, string contactNo, LinkStatus status)
        {
            LSCentral25.RootWishListLinks root = new LSCentral25.RootWishListLinks();
            List<LSCentral25.WishListLink1> lines = new List<LSCentral25.WishListLink1>()
            {
                new LSCentral25.WishListLink1()
                {
                    WishListNo = listId,
                    CardNo = cardId,
                    ContactNo = contactNo,
                    Status = status.ToString()
                }
            };
            root.WishListLink = lines.ToArray();
            return root;
        }
    }
}