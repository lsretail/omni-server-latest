using System;
using System.Collections.Generic;
using System.Linq;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSOmni.DataAccess.BOConnection.NavCommon.Mapping
{
    public class OrderMapping : BaseMapping
    {
        private Version NavVersion;

        public OrderMapping(Version navVersion, bool json)
        {
            NavVersion = navVersion;
            IsJson = json;
        }

        public Order MapFromRootTransactionToOrder(NavWS.RootMobileTransaction root)
        {
            NavWS.MobileTransaction header = root.MobileTransaction.FirstOrDefault();

            Order order = new Order()
            {
                Id = header.Id,
                ReceiptNo = header.ReceiptNo,
                TransId = header.TransactionNo.ToString(),
                TransTerminal = header.TerminalId,
                StoreId = header.StoreId,
                CardId = header.MemberCardNo,
                CustomerId = header.CustomerId,
                TotalAmount = header.GrossAmount,
                TotalNetAmount = header.NetAmount,
                TotalDiscount = header.LineDiscount,
                PointBalance = header.PointBalance,
                PointsRewarded = header.IssuedPoints,
                PointCashAmountNeeded = header.AmountRemaining,
                PointAmount = header.BasketInPoints,
                PointsUsedInOrder = header.PointsUsedInBasket
            };

            //now loop through the lines
            order.OrderLines = new List<OrderLine>();
            if (root.MobileTransactionLine != null)
            {
                foreach (NavWS.MobileTransactionLine mobileTransLine in root.MobileTransactionLine)
                {
                    LineType lineType = (LineType)mobileTransLine.LineType;
                    if (lineType == LineType.PerDiscount || lineType == LineType.Coupon)
                        continue;

                    OrderLine oline = order.OrderLines.Find(l =>
                            l.ItemId == mobileTransLine.Number &&
                            l.VariantId == mobileTransLine.VariantCode &&
                            l.UomId == mobileTransLine.UomId &&
                            l.LineType == lineType);

                    if (oline == null)
                    {
                        OrderLine line = new OrderLine()
                        {
                            LineNumber = LineNumberFromNav(mobileTransLine.LineNo),
                            ItemId = mobileTransLine.Number,
                            Quantity = mobileTransLine.Quantity,
                            DiscountAmount = mobileTransLine.DiscountAmount,
                            DiscountPercent = mobileTransLine.DiscountPercent,
                            Price = mobileTransLine.Price,
                            NetPrice = mobileTransLine.NetPrice,
                            Amount = mobileTransLine.NetAmount + mobileTransLine.TAXAmount,
                            NetAmount = mobileTransLine.NetAmount,
                            TaxAmount = mobileTransLine.TAXAmount,
                            ItemDescription = mobileTransLine.ItemDescription,
                            VariantId = mobileTransLine.VariantCode,
                            VariantDescription = mobileTransLine.VariantDescription,
                            UomId = mobileTransLine.UomId,
                            LineType = lineType
                        };
                        if (NavVersion > new Version("14.2"))
                            line.ItemImageId = mobileTransLine.RetailImageID;

                        line.LineNumbers.Add(line.LineNumber);
                        order.OrderLines.Add(line);
                    }
                    else
                    {
                        oline.DiscountAmount += mobileTransLine.DiscountAmount;
                        oline.NetAmount += mobileTransLine.NetAmount;
                        oline.Quantity += mobileTransLine.Quantity;
                        oline.TaxAmount += mobileTransLine.TAXAmount;
                        oline.Amount += (mobileTransLine.NetAmount + mobileTransLine.TAXAmount);
                        oline.LineNumbers.Add(LineNumberFromNav(mobileTransLine.LineNo));
                    }
                }
            }

            //now loop through the discount lines
            order.OrderDiscountLines = new List<OrderDiscountLine>();
            if (root.MobileTransDiscountLine != null)
            {
                foreach (NavWS.MobileTransDiscountLine mobileTransDisc in root.MobileTransDiscountLine)
                {
                    if (mobileTransDisc.DiscountAmount == 0 && mobileTransDisc.DiscountPercent == 0)
                        continue;

                    OrderDiscountLine dline = order.OrderDiscountLines.Find(l =>
                            l.DiscountType == (DiscountType)mobileTransDisc.DiscountType &&
                            l.PeriodicDiscType == (PeriodicDiscType)mobileTransDisc.PeriodicDiscType &&
                            l.PeriodicDiscGroup == mobileTransDisc.PeriodicDiscGroup &&
                            l.OfferNumber == mobileTransDisc.OfferNo);

                    if (dline != null)
                    {
                        // find line, if found we keep that discount
                        OrderLine oline = order.OrderLines.Find(l => l.LineNumber == LineNumberFromNav(mobileTransDisc.LineNo));
                        if (oline == null)
                        {
                            dline.DiscountAmount += mobileTransDisc.DiscountAmount;
                            continue;
                        }
                    }

                    OrderDiscountLine discount = new OrderDiscountLine()
                    {
                        //DiscountType: Periodic Disc.,Customer,InfoCode,Total,Line,Promotion,Deal,Total Discount,Tender Type,Item Point,Line Discount,Member Point,Coupon
                        DiscountType = (DiscountType)mobileTransDisc.DiscountType,
                        PeriodicDiscType = (PeriodicDiscType)mobileTransDisc.PeriodicDiscType,
                        PeriodicDiscGroup = mobileTransDisc.PeriodicDiscGroup,
                        DiscountPercent = mobileTransDisc.DiscountPercent,
                        DiscountAmount = mobileTransDisc.DiscountAmount,
                        Description = mobileTransDisc.Description,
                        No = mobileTransDisc.No.ToString(),
                        OfferNumber = mobileTransDisc.OfferNo,
                        LineNumber = LineNumberFromNav(mobileTransDisc.LineNo)
                    };

                    if (discount.DiscountType == DiscountType.Coupon)
                    {
                        NavWS.MobileTransactionLine tline = root.MobileTransactionLine.ToList().Find(l => l.LineType == 6);
                        if (tline != null)
                            discount.OfferNumber = tline.CouponCode;
                    }

                    // check line number if extra discount
                    foreach (OrderLine ol in order.OrderLines)
                    {
                        if (ol.LineNumbers.Contains(discount.LineNumber))
                        {
                            discount.LineNumber = ol.LineNumber;
                            break;
                        }
                    }

                    order.OrderDiscountLines.Add(discount);
                }
            }

            return order;
        }

        public List<ItemCustomerPrice> MapFromRootTransactionToItemCustPrice(NavWS.RootMobileTransaction root)
        {
            List<ItemCustomerPrice> items = new List<ItemCustomerPrice>();

            NavWS.MobileTransaction header = root.MobileTransaction.FirstOrDefault();
            if (root.MobileTransactionLine == null)
                return items;

            foreach (NavWS.MobileTransactionLine mobileTransLine in root.MobileTransactionLine)
            {
                items.Add(new ItemCustomerPrice()
                {
                    LineNo = LineNumberFromNav(mobileTransLine.LineNo),
                    Id = mobileTransLine.Number,
                    Quantity = mobileTransLine.Quantity,
                    DiscountPercent = mobileTransLine.DiscountPercent,
                    Price = mobileTransLine.Price,
                    VariantId = mobileTransLine.VariantCode
                });
            }
            return items;
        }

        public SalesEntry MapFromRootToSalesEntry(NavWS.RootCustomerOrderGet root)
        {
            NavWS.CustomerOrderHeader4 header = root.CustomerOrderHeader.FirstOrDefault();

            SalesEntry order = new SalesEntry()
            {
                Id = header.DocumentID,
                StoreId = header.CreatedatStore,
                ExternalId = header.ExternalID,
                ClickAndCollectOrder = header.ClickandCollectOrder,
                AnonymousOrder = string.IsNullOrEmpty(header.MemberCardNo),
                DocumentRegTime = ConvertTo.SafeJsonDate(header.DocumentDateTime, IsJson),

                CardId = header.MemberCardNo,
                CustomerId = header.CustomerNo,
                ContactName = header.FullName,
                ContactEmail = header.Email,
                ContactDayTimePhoneNo = header.DaytimePhoneNo,
                ContactAddress = new Address()
                {
                    Address1 = header.Address,
                    Address2 = header.Address2,
                    HouseNo = header.HouseApartmentNo,
                    City = header.City,
                    Country = header.CountryRegionCode,
                    PostCode = header.PostCode,
                    StateProvinceRegion = header.County,
                    PhoneNumber = header.PhoneNo,
                    CellPhoneNumber = header.MobilePhoneNo
                },

                ShipToName = header.ShiptoFullName,
                ShipToEmail = header.ShiptoEmail,
                ShipToAddress = new Address()
                {
                    Address1 = header.ShiptoAddress,
                    Address2 = header.ShiptoAddress2,
                    HouseNo = header.ShiptoHouseApartmentNo,
                    City = header.ShiptoCity,
                    Country = header.ShiptoCountryRegionCode,
                    PostCode = header.ShiptoPostCode,
                    StateProvinceRegion = header.ShiptoCounty,
                    PhoneNumber = header.ShiptoPhoneNo
                }
            };

            //now loop through the discount lines
            order.DiscountLines = new List<SalesEntryDiscountLine>();
            if (root.CustomerOrderDiscountLine != null)
            {
                foreach (NavWS.CustomerOrderDiscountLine1 line in root.CustomerOrderDiscountLine)
                {
                    SalesEntryDiscountLine discount = new SalesEntryDiscountLine()
                    {
                        //DiscountType: Periodic Disc.,Customer,InfoCode,Total,Line,Promotion,Deal,Total Discount,Tender Type,Item Point,Line Discount,Member Point,Coupon
                        DiscountType = (DiscountType)line.DiscountType,
                        PeriodicDiscType = (PeriodicDiscType)line.PeriodicDiscType,
                        DiscountPercent = line.DiscountPercent,
                        DiscountAmount = line.DiscountAmount,
                        Description = line.Description,
                        No = line.EntryNo.ToString(),
                        OfferNumber = line.OfferNo,
                        LineNumber = LineNumberFromNav(line.LineNo)
                    };
                    order.DiscountLines.Add(discount);
                }
            }

            //now loop through the lines
            decimal cnt = 0;
            order.Lines = new List<SalesEntryLine>();
            if (root.CustomerOrderLine != null)
            {
                foreach (NavWS.CustomerOrderLine3 oline in root.CustomerOrderLine)
                {
                    LineType lineType = (LineType)Convert.ToInt32(oline.LineType);
                    if (lineType == LineType.PerDiscount || lineType == LineType.Coupon)
                        continue;

                    SalesEntryLine line = new SalesEntryLine()
                    {
                        LineNumber = LineNumberFromNav(oline.LineNo),
                        ItemId = oline.Number,
                        Quantity = oline.Quantity,
                        DiscountAmount = oline.DiscountAmount,
                        DiscountPercent = oline.DiscountPercent,
                        Price = oline.Price,
                        NetPrice = oline.NetPrice,
                        Amount = oline.Amount,
                        NetAmount = oline.NetAmount,
                        TaxAmount = oline.VatAmount,
                        ItemDescription = oline.ItemDescription,
                        VariantId = oline.VariantCode,
                        VariantDescription = oline.VariantDescription,
                        UomId = oline.UnitofMeasureCode,
                        LineType = lineType
                    };
                    if (NavVersion > new Version("14.2"))
                        line.ItemImageId = oline.RetailImageID;

                    cnt += oline.Quantity;
                    order.Lines.Add(line);
                }
            }
            order.LineItemCount = (int)cnt;

            //now loop through the discount lines
            order.Payments = new List<SalesEntryPayment>();
            if (root.CustomerOrderPayment != null)
            {
                foreach (NavWS.CustomerOrderPayment1 line in root.CustomerOrderPayment)
                {
                    SalesEntryPayment pay = new SalesEntryPayment()
                    {
                        LineNumber = LineNumberFromNav(line.LineNo),
                        Amount = (line.FinalisedAmount > 0) ? line.FinalisedAmount : line.PreApprovedAmount,
                        CurrencyCode = line.CurrencyCode,
                        TenderType = line.TenderType
                    };
                    order.Payments.Add(pay);
                }
            }

            return order;
        }

        public SalesEntry MapFromRootV2ToSalesEntry(NavWS.RootCustomerOrderGetV3 root)
        {
            NavWS.CustomerOrderGetCOHeaderV3 header = root.CustomerOrderGetCOHeaderV3.FirstOrDefault();

            SalesEntry order = new SalesEntry()
            {
                Id = header.DocumentID,
                StoreId = header.CreatedatStore,
                ExternalId = header.ExternalID,
                ClickAndCollectOrder = header.ClickandCollectOrder,
                AnonymousOrder = string.IsNullOrEmpty(header.MemberCardNo),
                DocumentRegTime = ConvertTo.SafeJsonDate(header.Created, IsJson),
                TotalAmount = header.TotalAmount,
                TotalDiscount = header.TotalDiscount,
                LineItemCount = (int)header.TotalQuantity,

                CardId = header.MemberCardNo,
                CustomerId = header.CustomerNo,
                ContactName = header.Name,
                ContactEmail = header.Email,
                ContactDayTimePhoneNo = header.DaytimePhoneNo,
                ContactAddress = new Address()
                {
                    Address1 = header.Address,
                    Address2 = header.Address2,
                    HouseNo = header.HouseApartmentNo,
                    City = header.City,
                    Country = header.CountryRegionCode,
                    PostCode = header.PostCode,
                    StateProvinceRegion = header.County,
                    PhoneNumber = header.PhoneNo,
                    CellPhoneNumber = header.MobilePhoneNo
                },

                ShipToName = header.ShiptoName,
                ShipToEmail = header.ShiptoEmail,
                ShipToAddress = new Address()
                {
                    Address1 = header.ShiptoAddress,
                    Address2 = header.ShiptoAddress2,
                    HouseNo = header.ShiptoHouseApartmentNo,
                    City = header.ShiptoCity,
                    Country = header.ShiptoCountryRegionCode,
                    PostCode = header.ShiptoPostCode,
                    StateProvinceRegion = header.ShiptoCounty,
                    PhoneNumber = header.ShiptoPhoneNo
                }
            };

            //now loop through the discount lines
            order.DiscountLines = new List<SalesEntryDiscountLine>();
            if (root.CustomerOrderGetCODiscountLineV3 != null)
            {
                foreach (NavWS.CustomerOrderGetCODiscountLineV3 line in root.CustomerOrderGetCODiscountLineV3)
                {
                    SalesEntryDiscountLine discount = new SalesEntryDiscountLine()
                    {
                        //DiscountType: Periodic Disc.,Customer,InfoCode,Total,Line,Promotion,Deal,Total Discount,Tender Type,Item Point,Line Discount,Member Point,Coupon
                        DiscountType = (DiscountType)line.DiscountType,
                        PeriodicDiscType = (PeriodicDiscType)line.PeriodicDiscType,
                        DiscountPercent = line.DiscountPercent,
                        DiscountAmount = line.DiscountAmount,
                        Description = line.Description,
                        No = line.EntryNo.ToString(),
                        OfferNumber = line.OfferNo,
                        LineNumber = LineNumberFromNav(line.LineNo)
                    };
                    order.DiscountLines.Add(discount);
                }
            }

            //now loop through the lines
            order.Lines = new List<SalesEntryLine>();
            if (root.CustomerOrderGetCOLineV3!= null)
            {
                foreach (NavWS.CustomerOrderGetCOLineV3 oline in root.CustomerOrderGetCOLineV3)
                {
                    LineType lineType = (LineType)Convert.ToInt32(oline.LineType);
                    if (lineType == LineType.PerDiscount || lineType == LineType.Coupon)
                        continue;

                    SalesEntryLine line = new SalesEntryLine()
                    {
                        LineNumber = LineNumberFromNav(oline.LineNo),
                        ExternalId = oline.ExternalID,
                        ItemId = oline.Number,
                        Quantity = oline.Quantity,
                        DiscountAmount = oline.DiscountAmount,
                        DiscountPercent = oline.DiscountPercent,
                        Price = oline.Price,
                        NetPrice = oline.NetPrice,
                        Amount = oline.Amount,
                        NetAmount = oline.NetAmount,
                        TaxAmount = oline.VatAmount,
                        ItemDescription = oline.ItemDescription,
                        VariantId = oline.VariantCode,
                        VariantDescription = oline.VariantDescription,
                        UomId = oline.UnitofMeasureCode,
                        LineType = lineType,
                        StoreId = oline.StoreNo,
                        ClickAndCollectLine = oline.ClickAndCollectLine
                    };
                    if (NavVersion > new Version("14.2"))
                        line.ItemImageId = oline.RetailImageID;

                    order.Lines.Add(line);
                }
            }

            //now loop through the discount lines
            order.Payments = new List<SalesEntryPayment>();
            if (root.CustomerOrderGetCOPaymentV3 != null)
            {
                foreach (NavWS.CustomerOrderGetCOPaymentV3 line in root.CustomerOrderGetCOPaymentV3)
                {
                    SalesEntryPayment pay = new SalesEntryPayment()
                    {
                        LineNumber = LineNumberFromNav(line.LineNo),
                        Amount = (line.FinalizedAmount > 0) ? line.FinalizedAmount : line.PreApprovedAmount,
                        CurrencyCode = line.CurrencyCode,
                        TenderType = line.TenderType
                    };
                    order.Payments.Add(pay);
                }
            }
            return order;
        }

        public List<SalesEntry> MapFromRootToSalesEntryHistory(NavWS.RootCustomerOrderFilteredList root)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            if (root.CustomerOrderHeader != null)
            {
                foreach (NavWS.CustomerOrderHeader3 header in root.CustomerOrderHeader)
                {
                    list.Add(new SalesEntry()
                    {
                        Id = header.DocumentID,
                        StoreId = header.CreatedatStore,
                        CardId = header.MemberCardNo,
                        StoreName = header.CreatedatStore,
                        AnonymousOrder = string.IsNullOrEmpty(header.MemberCardNo),
                        DocumentRegTime = ConvertTo.SafeJsonDate(header.Created, IsJson),
                        ShipToName = header.FullName,
                        TotalAmount = header.TotalAmount,
                        LineItemCount = (int)header.TotalQuantity
                    });
                }
            }
            return list;
        }

        public List<SalesEntry> MapFromRootToSalesEntryHistory(NavWS.RootCOFilteredListV2 root)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            if (root.CustomerOrderHeaderV2 != null)
            {
                foreach (NavWS.CustomerOrderHeaderV2 header in root.CustomerOrderHeaderV2)
                {
                    list.Add(new SalesEntry()
                    {
                        Id = header.DocumentID,
                        StoreId = header.CreatedatStore,
                        CardId = header.MemberCardNo,
                        StoreName = header.CreatedatStore,
                        AnonymousOrder = string.IsNullOrEmpty(header.MemberCardNo),
                        DocumentRegTime = ConvertTo.SafeJsonDate(header.Created, IsJson),
                        ShipToName = header.FullName,
                        TotalAmount = header.TotalAmount,
                        LineItemCount = (int)header.TotalQuantity
                    });
                }
            }
            return list;
        }

        public NavWS.RootCustomerOrderCreateV2 MapFromOrderV2ToRoot(Order order)
        {
            NavWS.RootCustomerOrderCreateV2 root = new NavWS.RootCustomerOrderCreateV2();

            List<NavWS.CustomerOrderHeaderV21> header = new List<NavWS.CustomerOrderHeaderV21>();
            header.Add(new NavWS.CustomerOrderHeaderV21()
            {
                DocumentID = string.Empty,
                ExternalID = order.Id.ToUpper(),
                StoreNo = (order.OrderType == OrderType.ClickAndCollect && string.IsNullOrEmpty(order.CollectLocation) == false) ? order.CollectLocation.ToUpper() : order.StoreId.ToUpper(),
                MemberCardNo = XMLHelper.GetString(order.CardId),
                CustomerNo = XMLHelper.GetString(order.CustomerId),
                FullName = XMLHelper.GetString(order.ContactName),
                SourceType = 1, //NAV POS = 0, Omni = 1
                Address = XMLHelper.GetString(order.ContactAddress.Address1),
                Address2 = XMLHelper.GetString(order.ContactAddress.Address2),
                HouseApartmentNo = XMLHelper.GetString(order.ContactAddress.HouseNo),
                City = XMLHelper.GetString(order.ContactAddress.City),
                County = XMLHelper.GetString(order.ContactAddress.StateProvinceRegion),
                PostCode = XMLHelper.GetString(order.ContactAddress.PostCode),
                CountryRegionCode = XMLHelper.GetString(order.ContactAddress.Country),
                PhoneNo = XMLHelper.GetString(order.ContactAddress.PhoneNumber),
                MobilePhoneNo = XMLHelper.GetString(order.ContactAddress.CellPhoneNumber),
                DaytimePhoneNo = XMLHelper.GetString(order.DayPhoneNumber),
                Email = XMLHelper.GetString(order.Email),
                ShipToFullName = XMLHelper.GetString(order.ShipToName),
                ShipToAddress = XMLHelper.GetString(order.ShipToAddress.Address1),
                ShipToAddress2 = XMLHelper.GetString(order.ShipToAddress.Address2),
                ShipToHouseApartmentNo = XMLHelper.GetString(order.ShipToAddress.HouseNo),
                ShipToCity = XMLHelper.GetString(order.ShipToAddress.City),
                ShipToCounty = XMLHelper.GetString(order.ShipToAddress.StateProvinceRegion),
                ShipToPostCode = XMLHelper.GetString(order.ShipToAddress.PostCode),
                ShipToCountryRegionCode = XMLHelper.GetString(order.ShipToAddress.Country),
                ShipToPhoneNo = XMLHelper.GetString(order.ShipToAddress.PhoneNumber),
                ShipToEmail = XMLHelper.GetString(order.ShipToEmail),
                ClickAndCollectOrder = (order.OrderType == OrderType.ClickAndCollect),
                ShipOrder = (order.ShippingStatus != ShippingStatus.ShippigNotRequired),
                ShippingAgentServiceCode = XMLHelper.GetString(order.ShippingAgentServiceCode),
                ShippingAgentCode = XMLHelper.GetString(order.ShippingAgentCode),
                CreatedAtStore = string.Empty,
                TerritoryCode = string.Empty,
                SourcingLocation = string.Empty,
                ReceiptNo = string.Empty,
                VendorSourcing = string.Empty,
                InventoryTransfer = string.Empty,
                TransactionNo = string.Empty
            });

            if (NavVersion > new Version("13.4"))
                header[0].CustomerNo = string.Empty;

            root.CustomerOrderHeaderV2 = header.ToArray();

            List<NavWS.CustomerOrderLineV2> orderLines = new List<NavWS.CustomerOrderLineV2>();
            foreach (OrderLine line in order.OrderLines)
            {
                orderLines.Add(new NavWS.CustomerOrderLineV2()
                {
                    DocumentID = string.Empty,
                    LineNo = LineNumberToNav(line.LineNumber),
                    LineType = Convert.ToInt32(line.LineType).ToString(),
                    Number = line.ItemId,
                    VariantCode = XMLHelper.GetString(line.VariantId),
                    UnitofMeasureCode = XMLHelper.GetString(line.UomId),
                    OrderReference = XMLHelper.GetString(line.OrderId),
                    Quantity = line.Quantity,
                    DiscountAmount = line.DiscountAmount,
                    DiscountPercent = line.DiscountPercent,
                    Amount = line.Amount,
                    NetAmount = line.NetAmount,
                    VatAmount = line.TaxAmount,
                    Price = line.Price,
                    NetPrice = line.NetPrice,
                    Status = string.Empty,
                    LeadTimeCalculation = string.Empty,
                    PurchaseOrderNo = string.Empty,
                    SourcingLocation = string.Empty
                });
            }
            root.CustomerOrderLineV2 = orderLines.ToArray();

            List<NavWS.CustomerOrderDiscountLineV2> discLines = new List<NavWS.CustomerOrderDiscountLineV2>();
            if (order.OrderDiscountLines != null)
            {
                foreach (OrderDiscountLine line in order.OrderDiscountLines)
                {
                    discLines.Add(new NavWS.CustomerOrderDiscountLineV2()
                    {
                        DocumentID = string.Empty,
                        LineNo = LineNumberToNav(line.LineNumber),
                        EntryNo = Convert.ToInt32(line.No),
                        DiscountType = (int)line.DiscountType,
                        OfferNo = XMLHelper.GetString(line.OfferNumber),
                        PeriodicDiscType = (int)line.PeriodicDiscType,
                        PeriodicDiscGroup = XMLHelper.GetString(string.IsNullOrEmpty(line.PeriodicDiscGroup) ? line.OfferNumber : line.PeriodicDiscGroup),
                        Description = XMLHelper.GetString(line.Description),
                        DiscountPercent = line.DiscountPercent,
                        DiscountAmount = line.DiscountAmount
                    });
                }
            }
            root.CustomerOrderDiscountLineV2 = discLines.ToArray();

            List<NavWS.CustomerOrderPaymentV2> payLines = new List<NavWS.CustomerOrderPaymentV2>();
            if (order.OrderPayments != null)
            {
                foreach (OrderPayment line in order.OrderPayments)
                {
                    payLines.Add(new NavWS.CustomerOrderPaymentV2()
                    {
                        DocumentID = string.Empty,
                        LineNo = LineNumberToNav(line.LineNumber),
                        PreApprovedAmount = line.Amount,
                        FinalisedAmount = 0,
                        TenderType = line.TenderType,
                        CardType = XMLHelper.GetString(line.CardType),
                        CurrencyCode = XMLHelper.GetString(line.CurrencyCode),
                        CurrencyFactor = line.CurrencyFactor,
                        AuthorisationCode = XMLHelper.GetString(line.AuthorizationCode),
                        PreApprovedValidDate = line.PreApprovedValidDate,
                        CardorCustomernumber = XMLHelper.GetString(line.CardNumber),
                        IncomeExpenseAccountNo = string.Empty,
                        StoreNo = string.Empty
                    });
                }
            }
            root.CustomerOrderPaymentV2 = payLines.ToArray();
            return root;
        }

        public NavWS.RootCustomerOrderCreateV4 MapFromOrderV4ToRoot(Order order)
        {
            NavWS.RootCustomerOrderCreateV4 root = new NavWS.RootCustomerOrderCreateV4();

            if (NavVersion > new Version("15.0"))
            {
                // somehow cac needs address in v 15
                order.ContactAddress.Address1 = string.IsNullOrEmpty(order.ContactAddress.Address1) ? "x" : order.ContactAddress.Address1;
                order.ContactAddress.City = string.IsNullOrEmpty(order.ContactAddress.City) ? "x" : order.ContactAddress.City;
                order.ContactAddress.PostCode = string.IsNullOrEmpty(order.ContactAddress.PostCode) ? "x" : order.ContactAddress.PostCode;
            }

            List<NavWS.CustomerOrderCreateCOHeaderV4> header = new List<NavWS.CustomerOrderCreateCOHeaderV4>();
            header.Add(new NavWS.CustomerOrderCreateCOHeaderV4()
            {
                DocumentID = string.Empty,
                ExternalID = order.Id.ToUpper(),
                StoreNo = (order.OrderType == OrderType.ClickAndCollect && string.IsNullOrEmpty(order.CollectLocation) == false) ? order.CollectLocation.ToUpper() : order.StoreId.ToUpper(),
                MemberCardNo = XMLHelper.GetString(order.CardId),
                CustomerNo = XMLHelper.GetString(order.CustomerId),
                Name = XMLHelper.GetString(order.ContactName),
                SourceType = 1, //NAV POS = 0, Omni = 1
                Address = XMLHelper.GetString(order.ContactAddress.Address1),
                Address2 = XMLHelper.GetString(order.ContactAddress.Address2),
                HouseApartmentNo = XMLHelper.GetString(order.ContactAddress.HouseNo),
                City = XMLHelper.GetString(order.ContactAddress.City),
                County = XMLHelper.GetString(order.ContactAddress.StateProvinceRegion),
                PostCode = XMLHelper.GetString(order.ContactAddress.PostCode),
                CountryRegionCode = XMLHelper.GetString(order.ContactAddress.Country),
                PhoneNo = XMLHelper.GetString(order.ContactAddress.PhoneNumber),
                MobilePhoneNo = XMLHelper.GetString(order.ContactAddress.CellPhoneNumber),
                DaytimePhoneNo = XMLHelper.GetString(order.DayPhoneNumber),
                Email = XMLHelper.GetString(order.Email),
                ShipToName = XMLHelper.GetString(order.ShipToName),
                ShipToAddress = XMLHelper.GetString(order.ShipToAddress.Address1),
                ShipToAddress2 = XMLHelper.GetString(order.ShipToAddress.Address2),
                ShipToHouseApartmentNo = XMLHelper.GetString(order.ShipToAddress.HouseNo),
                ShipToCity = XMLHelper.GetString(order.ShipToAddress.City),
                ShipToCounty = XMLHelper.GetString(order.ShipToAddress.StateProvinceRegion),
                ShipToPostCode = XMLHelper.GetString(order.ShipToAddress.PostCode),
                ShipToCountryRegionCode = XMLHelper.GetString(order.ShipToAddress.Country),
                ShipToPhoneNo = XMLHelper.GetString(order.ShipToAddress.PhoneNumber),
                ShipToEmail = XMLHelper.GetString(order.ShipToEmail),
                ClickAndCollectOrder = (order.OrderType == OrderType.ClickAndCollect),
                ShipOrder = (order.ShippingStatus != ShippingStatus.ShippigNotRequired),
                ShippingAgentServiceCode = XMLHelper.GetString(order.ShippingAgentServiceCode),
                ShippingAgentCode = XMLHelper.GetString(order.ShippingAgentCode),
                CreatedAtStore = string.Empty,
                TerritoryCode = string.Empty,
                InventoryTransfer = string.Empty,
                SourcingLocation = string.Empty,
                VendorSourcing = string.Empty
            });

            if (NavVersion > new Version("13.4"))
                header[0].CustomerNo = string.Empty;

            root.CustomerOrderCreateCOHeaderV4 = header.ToArray();

            List<NavWS.CustomerOrderCreateCOLineV4> orderLines = new List<NavWS.CustomerOrderCreateCOLineV4>();
            foreach (OrderLine line in order.OrderLines)
            {
                orderLines.Add(new NavWS.CustomerOrderCreateCOLineV4()
                {
                    DocumentID = string.Empty,
                    LineNo = LineNumberToNav(line.LineNumber),
                    LineType = Convert.ToInt32(line.LineType).ToString(),
                    Number = line.ItemId,
                    VariantCode = XMLHelper.GetString(line.VariantId),
                    UnitofMeasureCode = XMLHelper.GetString(line.UomId),
                    OrderReference = XMLHelper.GetString(line.OrderId),
                    Quantity = line.Quantity,
                    DiscountAmount = line.DiscountAmount,
                    DiscountPercent = line.DiscountPercent,
                    Amount = line.Amount,
                    NetAmount = line.NetAmount,
                    VatAmount = line.TaxAmount,
                    Price = line.Price,
                    NetPrice = line.NetPrice,
                    Status = string.Empty,
                    LeadTimeCalculation = string.Empty,
                    PurchaseOrderNo = string.Empty,
                    SourcingLocation = string.Empty,
                    InventoryTransfer = false
                });
            }
            root.CustomerOrderCreateCOLineV4 = orderLines.ToArray();

            List<NavWS.CustomerOrderCreateCODiscountLineV4> discLines = new List<NavWS.CustomerOrderCreateCODiscountLineV4>();
            if (order.OrderDiscountLines != null)
            {
                foreach (OrderDiscountLine line in order.OrderDiscountLines)
                {
                    discLines.Add(new NavWS.CustomerOrderCreateCODiscountLineV4()
                    {
                        DocumentID = string.Empty,
                        LineNo = LineNumberToNav(line.LineNumber),
                        EntryNo = Convert.ToInt32(line.No),
                        DiscountType = (int)line.DiscountType,
                        OfferNo = XMLHelper.GetString(line.OfferNumber),
                        PeriodicDiscType = (int)line.PeriodicDiscType,
                        PeriodicDiscGroup = XMLHelper.GetString(string.IsNullOrEmpty(line.PeriodicDiscGroup) ? line.OfferNumber : line.PeriodicDiscGroup),
                        Description = XMLHelper.GetString(line.Description),
                        DiscountPercent = line.DiscountPercent,
                        DiscountAmount = line.DiscountAmount
                    });
                }
            }
            root.CustomerOrderCreateCODiscountLineV4 = discLines.ToArray();

            List<NavWS.CustomerOrderCreateCOPaymentV4> payLines = new List<NavWS.CustomerOrderCreateCOPaymentV4>();
            if (order.OrderPayments != null)
            {
                foreach (OrderPayment line in order.OrderPayments)
                {
                    string curcode = XMLHelper.GetString(line.CurrencyCode);
                    bool loypayment = (string.IsNullOrEmpty(curcode)) ? false : curcode.Equals("LOY", StringComparison.InvariantCultureIgnoreCase);

                    payLines.Add(new NavWS.CustomerOrderCreateCOPaymentV4()
                    {
                        DocumentID = string.Empty,
                        LineNo = LineNumberToNav(line.LineNumber),
                        PreApprovedAmount = line.Amount,
                        Type = ((int)line.PaymentType).ToString(),
                        TenderType = line.TenderType,
                        CardType = XMLHelper.GetString(line.CardType),
                        CurrencyCode = curcode,
                        CurrencyFactor = line.CurrencyFactor,
                        AuthorizationCode = XMLHelper.GetString(line.AuthorizationCode),
                        TokenNo = XMLHelper.GetString(line.TokenNumber),
                        ExternalReference = XMLHelper.GetString(line.ExternalReference),
                        PreApprovedValidDate = line.PreApprovedValidDate,
                        CardorCustomernumber = XMLHelper.GetString(line.CardNumber),
                        LoyaltyPointpayment = loypayment,
                        IncomeExpenseAccountNo = string.Empty,
                        StoreNo = string.Empty,
                        PosTransReceiptNo = string.Empty
                    });
                }
            }
            root.CustomerOrderCreateCOPaymentV4 = payLines.ToArray();
            return root;
        }

        public NavWS.RootCustomerOrderCreateV5 MapFromOrderV5ToRoot(Order order)
        {
            NavWS.RootCustomerOrderCreateV5 root = new NavWS.RootCustomerOrderCreateV5();

            List<NavWS.CustomerOrderCreateCOHeaderV5> header = new List<NavWS.CustomerOrderCreateCOHeaderV5>();
            NavWS.CustomerOrderCreateCOHeaderV5 hd = new NavWS.CustomerOrderCreateCOHeaderV5()
            {
                DocumentID = string.Empty,
                ExternalID = order.Id.ToUpper(),
                MemberCardNo = XMLHelper.GetString(order.CardId),
                CustomerNo = XMLHelper.GetString(order.CustomerId),
                Name = XMLHelper.GetString(order.ContactName),
                SourceType = 1, //NAV POS = 0, Omni = 1
                Address = XMLHelper.GetString(order.ContactAddress.Address1),
                Address2 = XMLHelper.GetString(order.ContactAddress.Address2),
                HouseApartmentNo = XMLHelper.GetString(order.ContactAddress.HouseNo),
                City = XMLHelper.GetString(order.ContactAddress.City),
                County = XMLHelper.GetString(order.ContactAddress.StateProvinceRegion),
                PostCode = XMLHelper.GetString(order.ContactAddress.PostCode),
                CountryRegionCode = XMLHelper.GetString(order.ContactAddress.Country),
                PhoneNo = XMLHelper.GetString(order.ContactAddress.PhoneNumber),
                MobilePhoneNo = XMLHelper.GetString(order.ContactAddress.CellPhoneNumber),
                DaytimePhoneNo = XMLHelper.GetString(order.DayPhoneNumber),
                Email = XMLHelper.GetString(order.Email),
                ShipToName = XMLHelper.GetString(order.ShipToName),
                ShipToAddress = XMLHelper.GetString(order.ShipToAddress.Address1),
                ShipToAddress2 = XMLHelper.GetString(order.ShipToAddress.Address2),
                ShipToHouseApartmentNo = XMLHelper.GetString(order.ShipToAddress.HouseNo),
                ShipToCity = XMLHelper.GetString(order.ShipToAddress.City),
                ShipToCounty = XMLHelper.GetString(order.ShipToAddress.StateProvinceRegion),
                ShipToPostCode = XMLHelper.GetString(order.ShipToAddress.PostCode),
                ShipToCountryRegionCode = XMLHelper.GetString(order.ShipToAddress.Country),
                ShipToPhoneNo = XMLHelper.GetString(order.ShipToAddress.PhoneNumber),
                ShipToEmail = XMLHelper.GetString(order.ShipToEmail),
                ShipOrder = (order.ShippingStatus != ShippingStatus.ShippigNotRequired && order.ShippingStatus != 0),
                CreatedAtStore = order.StoreId,
                TerritoryCode = string.Empty
            };

            if (NavVersion > new Version("17.4.0.0"))
                hd.ShopPaygo = (order.OrderType == OrderType.ScanPayGo);

            header.Add(hd);

            bool useHeaderCAC = false;
            string storeId = order.StoreId.ToUpper();
            if (order.OrderType == OrderType.ClickAndCollect && string.IsNullOrEmpty(order.CollectLocation) == false)
            {
                storeId =  order.CollectLocation.ToUpper();
                useHeaderCAC = true;
            }

            root.CustomerOrderCreateCOHeaderV5 = header.ToArray();

            List<NavWS.CustomerOrderCreateCOLineV5> orderLines = new List<NavWS.CustomerOrderCreateCOLineV5>();
            foreach (OrderLine line in order.OrderLines)
            {
                orderLines.Add(new NavWS.CustomerOrderCreateCOLineV5()
                {
                    DocumentID = string.Empty,
                    ExternalID = (string.IsNullOrEmpty(line.Id)) ? string.Empty : line.Id.ToUpper(),
                    LineNo = LineNumberToNav(line.LineNumber),
                    LineType = Convert.ToInt32(line.LineType).ToString(),
                    Number = line.ItemId,
                    VariantCode = XMLHelper.GetString(line.VariantId),
                    UnitofMeasureCode = XMLHelper.GetString(line.UomId),
                    OrderReference = XMLHelper.GetString(line.OrderId),
                    Quantity = line.Quantity,
                    DiscountAmount = line.DiscountAmount,
                    DiscountPercent = line.DiscountPercent,
                    Amount = line.Amount,
                    NetAmount = line.NetAmount,
                    VatAmount = line.TaxAmount,
                    Price = line.Price,
                    NetPrice = line.NetPrice,
                    Status = string.Empty,
                    LeadTimeCalculation = string.Empty,
                    PurchaseOrderNo = string.Empty,
                    SourcingLocation = string.Empty,
                    SourcingLocation1 = string.Empty,
                    InventoryTransfer = false,
                    InventoryTransfer1 = false,
                    VendorSourcing = false,
                    VendorSourcing1 = false,
                    ShipOrder = false,
                    StoreNo = string.IsNullOrEmpty(line.StoreId) ? storeId : line.StoreId.ToUpper(),
                    ClickAndCollect = (useHeaderCAC) ? (order.OrderType == OrderType.ClickAndCollect) : line.ClickAndCollectLine,
                    TerminalNo = string.Empty
                });
            }
            root.CustomerOrderCreateCOLineV5 = orderLines.ToArray();

            List<NavWS.CustomerOrderCreateCODiscountLineV5> discLines = new List<NavWS.CustomerOrderCreateCODiscountLineV5>();
            if (order.OrderDiscountLines != null)
            {
                foreach (OrderDiscountLine line in order.OrderDiscountLines)
                {
                    discLines.Add(new NavWS.CustomerOrderCreateCODiscountLineV5()
                    {
                        DocumentID = string.Empty,
                        LineNo = LineNumberToNav(line.LineNumber),
                        EntryNo = Convert.ToInt32(line.No),
                        DiscountType = (int)line.DiscountType,
                        OfferNo = XMLHelper.GetString(line.OfferNumber),
                        PeriodicDiscType = (int)line.PeriodicDiscType,
                        PeriodicDiscGroup = XMLHelper.GetString(string.IsNullOrEmpty(line.PeriodicDiscGroup) ? line.OfferNumber : line.PeriodicDiscGroup),
                        Description = XMLHelper.GetString(line.Description),
                        DiscountPercent = line.DiscountPercent,
                        DiscountAmount = line.DiscountAmount
                    });
                }
            }
            root.CustomerOrderCreateCODiscountLineV5 = discLines.ToArray();

            List<NavWS.CustomerOrderCreateCOPaymentV5> payLines = new List<NavWS.CustomerOrderCreateCOPaymentV5>();
            if (order.OrderPayments != null)
            {
                foreach (OrderPayment line in order.OrderPayments)
                {
                    string curcode = XMLHelper.GetString(line.CurrencyCode);
                    bool loypayment = (string.IsNullOrEmpty(curcode)) ? false : curcode.Equals("LOY", StringComparison.InvariantCultureIgnoreCase);

                    payLines.Add(new NavWS.CustomerOrderCreateCOPaymentV5()
                    {
                        DocumentID = string.Empty,
                        LineNo = LineNumberToNav(line.LineNumber),
                        PreApprovedAmount = line.Amount,
                        PreApprovedAmountLCY = line.CurrencyFactor * line.Amount,
                        Type = ((int)line.PaymentType).ToString(),
                        TenderType = line.TenderType,
                        CardType = XMLHelper.GetString(line.CardType),
                        CurrencyCode = curcode,
                        CurrencyFactor = line.CurrencyFactor,
                        AuthorizationCode = XMLHelper.GetString(line.AuthorizationCode),
                        TokenNo = XMLHelper.GetString(line.TokenNumber),
                        ExternalReference = XMLHelper.GetString(line.ExternalReference),
                        PreApprovedValidDate = line.PreApprovedValidDate,
                        CardorCustomernumber = XMLHelper.GetString(line.CardNumber),
                        LoyaltyPointpayment = loypayment,
                        IncomeExpenseAccountNo = string.Empty,
                        StoreNo = (order.OrderType == OrderType.ClickAndCollect && string.IsNullOrEmpty(order.CollectLocation) == false) ? order.CollectLocation.ToUpper() : order.StoreId.ToUpper(),
                        PosTransReceiptNo = string.Empty
                    });
                }
            }
            root.CustomerOrderCreateCOPaymentV5 = payLines.ToArray();
            return root;
        }

        public NavWS.RootCustomerOrder MapFromOrderToRoot(Order order)
        {
            NavWS.RootCustomerOrder root = new NavWS.RootCustomerOrder();

            List<NavWS.CustomerOrderHeader2> header = new List<NavWS.CustomerOrderHeader2>();
            header.Add(new NavWS.CustomerOrderHeader2()
            {
                DocumentId = order.Id,
                StoreNo = (order.OrderType == OrderType.ClickAndCollect && string.IsNullOrEmpty(order.CollectLocation) == false) ? order.CollectLocation.ToUpper() : order.StoreId.ToUpper(),
                MemberCardNo = XMLHelper.GetString(order.CardId),
                FullName = XMLHelper.GetString(order.ContactName),
                Address = XMLHelper.GetString(order.ContactAddress.Address1),
                Address2 = XMLHelper.GetString(order.ContactAddress.Address2),
                HouseApartmentNo = XMLHelper.GetString(order.ContactAddress.HouseNo),
                City = XMLHelper.GetString(order.ContactAddress.City),
                County = XMLHelper.GetString(order.ContactAddress.StateProvinceRegion),
                PostCode = XMLHelper.GetString(order.ContactAddress.PostCode),
                CountryRegionCode = XMLHelper.GetString(order.ContactAddress.Country),
                PhoneNo = XMLHelper.GetString(order.ContactAddress.PhoneNumber),
                MobilePhoneNo = XMLHelper.GetString(order.ContactAddress.CellPhoneNumber),
                DaytimePhoneNo = XMLHelper.GetString(order.DayPhoneNumber),
                Email = XMLHelper.GetString(order.Email),
                ShipToFullName = XMLHelper.GetString(order.ShipToName),
                ShipToAddress = XMLHelper.GetString(order.ShipToAddress.Address1),
                ShipToAddress2 = XMLHelper.GetString(order.ShipToAddress.Address2),
                ShipToHouseApartmentNo = XMLHelper.GetString(order.ShipToAddress.HouseNo),
                ShipToCity = XMLHelper.GetString(order.ShipToAddress.City),
                ShipToCounty = XMLHelper.GetString(order.ShipToAddress.StateProvinceRegion),
                ShipToPostCode = XMLHelper.GetString(order.ShipToAddress.PostCode),
                ShipToCountryRegionCode = XMLHelper.GetString(order.ShipToAddress.Country),
                ShipToPhoneNo = XMLHelper.GetString(order.ShipToAddress.PhoneNumber),
                ShipToEmail = XMLHelper.GetString(order.ShipToEmail),
                ClickAndCollectOrder = (order.OrderType == OrderType.ClickAndCollect),
                AnonymousOrder = string.IsNullOrEmpty(order.CardId),
                ShippingAgentServiceCode = XMLHelper.GetString(order.ShippingAgentServiceCode),
                ShippingAgentCode = XMLHelper.GetString(order.ShippingAgentCode),
                SourcingLocation = string.Empty,
                ReceiptNo = string.Empty
            });

            if (NavVersion > new Version("13.4"))
                header[0].CustomerNo = XMLHelper.GetString(order.CustomerId);

            root.CustomerOrderHeader = header.ToArray();

            List<NavWS.CustomerOrderLine2> orderLines = new List<NavWS.CustomerOrderLine2>();
            foreach (OrderLine line in order.OrderLines)
            {
                orderLines.Add(new NavWS.CustomerOrderLine2()
                {
                    DocumentId = order.Id,
                    LineNo = LineNumberToNav(line.LineNumber),
                    LineType = Convert.ToInt32(line.LineType).ToString(),
                    Number = line.ItemId,
                    VariantCode = XMLHelper.GetString(line.VariantId),
                    UnitofMeasureCode = XMLHelper.GetString(line.UomId),
                    OrderReference = XMLHelper.GetString(line.OrderId),
                    Quantity = line.Quantity,
                    DiscountAmount = line.DiscountAmount,
                    DiscountPercent = line.DiscountPercent,
                    Amount = line.Amount,
                    NetAmount = line.NetAmount,
                    VatAmount = line.TaxAmount,
                    Price = line.Price,
                    NetPrice = line.NetPrice,
                    SourcingLocation = string.Empty
                });
            }
            root.CustomerOrderLine = orderLines.ToArray();

            List<NavWS.CustomerOrderDiscountLine> discLines = new List<NavWS.CustomerOrderDiscountLine>();
            if (order.OrderDiscountLines != null)
            {
                foreach (OrderDiscountLine line in order.OrderDiscountLines)
                {
                    discLines.Add(new NavWS.CustomerOrderDiscountLine()
                    {
                        DocumentId = order.Id,
                        LineNo = LineNumberToNav(line.LineNumber),
                        EntryNo = Convert.ToInt32(line.No),
                        DiscountType = (int)line.DiscountType,
                        OfferNo = XMLHelper.GetString(line.OfferNumber),
                        PeriodicDiscType = (int)line.PeriodicDiscType,
                        PeriodicDiscGroup = XMLHelper.GetString(line.PeriodicDiscGroup),
                        Description = XMLHelper.GetString(line.Description),
                        DiscountPercent = line.DiscountPercent,
                        DiscountAmount = line.DiscountAmount
                    });
                }
            }
            root.CustomerOrderDiscountLine = discLines.ToArray();

            List<NavWS.CustomerOrderPayment> payLines = new List<NavWS.CustomerOrderPayment>();
            if (order.OrderPayments != null)
            {
                foreach (OrderPayment line in order.OrderPayments)
                {
                    payLines.Add(new NavWS.CustomerOrderPayment()
                    {
                        DocumentId = order.Id,
                        LineNo = LineNumberToNav(line.LineNumber),
                        PreApprovedAmount = line.Amount,
                        TenderType = line.TenderType,
                        CardType = XMLHelper.GetString(line.CardType),
                        CurrencyCode = XMLHelper.GetString(line.CurrencyCode),
                        CurrencyFactor = line.CurrencyFactor,
                        AuthorisationCode = XMLHelper.GetString(line.AuthorizationCode),
                        PreApprovedValidDate = line.PreApprovedValidDate,
                        CardorCustomernumber = XMLHelper.GetString(order.CardId)
                    });
                }
            }
            root.CustomerOrderPayment = payLines.ToArray();
            return root;
        }

        public NavWS.RootMobileTransaction MapFromRetailTransactionToRoot(OneList list)
        {
            NavWS.RootMobileTransaction root = new NavWS.RootMobileTransaction();

            //MobileTrans
            List<NavWS.MobileTransaction> trans = new List<NavWS.MobileTransaction>();
            trans.Add(new NavWS.MobileTransaction()
            {
                Id = list.Id,
                StoreId = list.StoreId.ToUpper(),
                TransactionType = 2,
                EntryStatus = (int)EntryStatus.Normal,
                MemberCardNo = XMLHelper.GetString(list.CardId),

                // fill out null fields
                CustomerId = string.Empty,
                BusinessTAXCode = string.Empty,
                CurrencyCode = string.Empty,
                CustDiscGroup = string.Empty,
                DiningTblDescription = string.Empty,
                MemberPriceGroupCode = string.Empty,
                PriceGroupCode = string.Empty,
                ReceiptNo = string.Empty,
                RefundedFromPOSTermNo = string.Empty,
                RefundedFromStoreNo = string.Empty,
                RefundedReceiptNo = string.Empty,
                SalesType = string.Empty,
                StaffId = string.Empty,
                TerminalId = string.Empty,
                TransDate = DateTime.Now,
                PointsUsedInBasket = list.PointAmount
            });
            root.MobileTransaction = trans.ToArray();

            //MobileTransLines
            int lineno = 1;
            List<NavWS.MobileTransactionLine> transLines = new List<NavWS.MobileTransactionLine>();
            foreach (OneListItem line in list.Items)
            {
                NavWS.MobileTransactionLine tline = new NavWS.MobileTransactionLine()
                {
                    Id = root.MobileTransaction[0].Id,
                    LineNo = LineNumberToNav(lineno++),
                    EntryStatus = (int)EntryStatus.Normal,
                    LineType = (int)LineType.Item,
                    Number = line.ItemId,
                    CurrencyFactor = 1,
                    VariantCode = line.VariantId,
                    UomId = line.UnitOfMeasureId,
                    Quantity = line.Quantity,
                    DiscountAmount = line.DiscountAmount,
                    DiscountPercent = line.DiscountPercent,
                    NetAmount = line.NetAmount,
                    TAXAmount = line.TaxAmount,
                    Price = line.Price,
                    NetPrice = line.NetPrice,
                    StoreId = list.StoreId.ToUpper(),

                    Barcode = string.Empty,
                    CardOrCustNo = string.Empty,
                    CouponCode = string.Empty,
                    CurrencyCode = string.Empty,
                    EFTAuthCode = string.Empty,
                    EFTCardName = string.Empty,
                    EFTCardNumber = string.Empty,
                    EFTTransactionNo = string.Empty,
                    EFTMessage = string.Empty,
                    ItemDescription = string.Empty,
                    LineKitchenStatusCode = string.Empty,
                    OrigTransPos = string.Empty,
                    OrigTransStore = string.Empty,
                    PriceGroupCode = string.Empty,
                    RestMenuTypeCode = string.Empty,
                    SalesType = string.Empty,
                    StaffId = string.Empty,
                    TAXBusinessCode = string.Empty,
                    TAXProductCode = string.Empty,
                    TenderDescription = string.Empty,
                    TerminalId = string.Empty,
                    UomDescription = string.Empty,
                    VariantDescription = string.Empty,
                    TransDate = DateTime.Now
                };
                if (NavVersion > new Version("14.2"))
                    tline.RetailImageID = string.Empty;
                transLines.Add(tline);
            }

            //Coupons
            if (list.PublishedOffers != null)
            {
                foreach (OneListPublishedOffer line in list.PublishedOffers?.Where(x => x.Type == OfferDiscountType.Coupon))
                {
                    NavWS.MobileTransactionLine tline = new NavWS.MobileTransactionLine()
                    {
                        Id = root.MobileTransaction[0].Id,
                        LineNo = LineNumberToNav(lineno++),
                        EntryStatus = (int)EntryStatus.Normal,
                        LineType = (int)LineType.Coupon,
                        Number = line.Id,
                        CurrencyFactor = 1,
                        VariantCode = string.Empty,
                        UomId = string.Empty,
                        Quantity = 0,
                        DiscountAmount = 0,
                        DiscountPercent = 0,
                        NetAmount = 0,
                        TAXAmount = 0,
                        Price = 0,
                        NetPrice = 0,
                        StoreId = list.StoreId.ToUpper(),

                        Barcode = line.Id,
                        CardOrCustNo = string.Empty,
                        CouponCode = string.Empty,
                        CurrencyCode = string.Empty,
                        EFTAuthCode = string.Empty,
                        EFTCardName = string.Empty,
                        EFTCardNumber = string.Empty,
                        EFTTransactionNo = string.Empty,
                        EFTMessage = string.Empty,
                        ItemDescription = string.Empty,
                        LineKitchenStatusCode = string.Empty,
                        OrigTransPos = string.Empty,
                        OrigTransStore = string.Empty,
                        PriceGroupCode = string.Empty,
                        RestMenuTypeCode = string.Empty,
                        SalesType = string.Empty,
                        StaffId = string.Empty,
                        TAXBusinessCode = string.Empty,
                        TAXProductCode = string.Empty,
                        TenderDescription = string.Empty,
                        TerminalId = string.Empty,
                        UomDescription = string.Empty,
                        VariantDescription = string.Empty,
                        TransDate = DateTime.Now
                    };
                    if (NavVersion > new Version("14.2"))
                        tline.RetailImageID = string.Empty;
                    transLines.Add(tline);
                }
            }

            root.MobileTransactionLine = transLines.ToArray();
            root.MobileReceiptInfo = new List<NavWS.MobileReceiptInfo>().ToArray();
            root.MobileTransactionSubLine = new List<NavWS.MobileTransactionSubLine>().ToArray();
            List<NavWS.MobileTransDiscountLine> discLines = new List<NavWS.MobileTransDiscountLine>();
            return root;
        }

        public NavWS.RootMobileTransaction MapFromCustItemToRoot(string storeId, string cardId, List<ItemCustomerPrice> items)
        {
            NavWS.RootMobileTransaction root = new NavWS.RootMobileTransaction();

            //MobileTrans
            string id = Guid.NewGuid().ToString();
            List<NavWS.MobileTransaction> trans = new List<NavWS.MobileTransaction>();
            trans.Add(new NavWS.MobileTransaction()
            {
                Id = id,
                StoreId = storeId,
                TransactionType = 0,
                EntryStatus = (int)EntryStatus.Normal,
                MemberCardNo = cardId,

                // fill out null fields
                BusinessTAXCode = string.Empty,
                CustomerId = string.Empty,
                CurrencyCode = string.Empty,
                CustDiscGroup = string.Empty,
                DiningTblDescription = string.Empty,
                MemberPriceGroupCode = string.Empty,
                PriceGroupCode = string.Empty,
                ReceiptNo = string.Empty,
                RefundedFromPOSTermNo = string.Empty,
                RefundedFromStoreNo = string.Empty,
                RefundedReceiptNo = string.Empty,
                SalesType = string.Empty,
                StaffId = string.Empty,
                TerminalId = string.Empty,
                GenBusPostingGroup = string.Empty,
                VATBusPostingGroup = string.Empty,
                TransDate = DateTime.Now
            });
            root.MobileTransaction = trans.ToArray();

            //MobileTransLines
            int lineno = 1;
            List<NavWS.MobileTransactionLine> transLines = new List<NavWS.MobileTransactionLine>();
            foreach (ItemCustomerPrice line in items)
            {
                transLines.Add(new NavWS.MobileTransactionLine()
                {
                    Id = id,
                    LineNo = LineNumberToNav(lineno++),
                    EntryStatus = (int)EntryStatus.Normal,
                    LineType = (int)LineType.Item,
                    Number = line.Id,
                    CurrencyFactor = 1,
                    VariantCode = line.VariantId,
                    Quantity = line.Quantity,
                    StoreId = storeId,

                    Barcode = string.Empty,
                    CardOrCustNo = string.Empty,
                    CouponCode = string.Empty,
                    CurrencyCode = string.Empty,
                    EFTAuthCode = string.Empty,
                    EFTCardName = string.Empty,
                    EFTCardNumber = string.Empty,
                    EFTTransactionNo = string.Empty,
                    EFTMessage = string.Empty,
                    ItemDescription = string.Empty,
                    LineKitchenStatusCode = string.Empty,
                    OrigTransPos = string.Empty,
                    OrigTransStore = string.Empty,
                    PriceGroupCode = string.Empty,
                    RestMenuTypeCode = string.Empty,
                    SalesType = string.Empty,
                    StaffId = string.Empty,
                    TAXBusinessCode = string.Empty,
                    TAXProductCode = string.Empty,
                    TenderDescription = string.Empty,
                    TerminalId = string.Empty,
                    UomDescription = string.Empty,
                    VariantDescription = string.Empty,
                    GenBusPostingGroup = string.Empty,
                    GenProdPostingGroup = string.Empty,
                    VatBusPostingGroup = string.Empty,
                    VatProdPostingGroup = string.Empty,
                    UomId = string.Empty,
                    TransDate = DateTime.Now,
                    RetailImageID = string.Empty
                });
            }

            root.MobileTransactionLine = transLines.ToArray();
            root.MobileReceiptInfo = new List<NavWS.MobileReceiptInfo>().ToArray();
            root.MobileTransactionSubLine = new List<NavWS.MobileTransactionSubLine>().ToArray();
            List<NavWS.MobileTransDiscountLine> discLines = new List<NavWS.MobileTransDiscountLine>();
            return root;
        }

        public OrderAvailabilityResponse MapRootToOrderavailabilty(NavWS.RootCOQtyAvailabilityExtIn rootin, NavWS.RootCOQtyAvailabilityExtOut root)
        {
            OrderAvailabilityResponse resp = new OrderAvailabilityResponse();

            if (root.WSInventoryBuffer == null)
                return resp;

            foreach (NavWS.WSInventoryBuffer line in root.WSInventoryBuffer)
            {
                resp.Lines.Add(new OrderLineAvailabilityResponse()
                {
                    ItemId = line.ItemNo,
                    VariantId = line.VariantCode,
                    UnitOfMeasureId = line.BaseUnitOfMeasure,
                    LocationCode = line.LocationCode,
                    Quantity = line.ActualInventory,
                    LeadTimeDays = line.LeadTimeDays
                });
            }

            // check for missing items as nav does not return 0 items
            foreach (NavWS.CustomerOrderLine1 item in rootin.CustomerOrderLine)
            {
                OrderLineAvailabilityResponse it = resp.Lines.Find(i => i.ItemId.Equals(item.Number) && i.VariantId.Equals(item.VariantCode));
                if (it == null)
                {
                    resp.Lines.Add(new OrderLineAvailabilityResponse()
                    {
                        ItemId = item.Number,
                        VariantId = item.VariantCode,
                        UnitOfMeasureId = item.UnitofMeasureCode,
                        Quantity = 0
                    });
                }
            }
            return resp;
        }

        public OrderAvailabilityResponse MapRootToOrderavailabilty2(NavWS.RootCOQtyAvailabilityInV2 rootin, NavWS.RootCOQtyAvailabilityExtOut root)
        {
            OrderAvailabilityResponse resp = new OrderAvailabilityResponse();

            if (root.WSInventoryBuffer == null)
                return resp;

            foreach (NavWS.WSInventoryBuffer line in root.WSInventoryBuffer)
            {
                resp.Lines.Add(new OrderLineAvailabilityResponse()
                {
                    ItemId = line.ItemNo,
                    VariantId = line.VariantCode,
                    UnitOfMeasureId = line.BaseUnitOfMeasure,
                    LocationCode = line.LocationCode,
                    Quantity = line.ActualInventory,
                    LeadTimeDays = line.LeadTimeDays
                });
            }

            // check for missing items as nav does not return 0 items
            foreach (NavWS.CustomerOrderLine item in rootin.CustomerOrderLine)
            {
                OrderLineAvailabilityResponse it = resp.Lines.Find(i => i.ItemId.Equals(item.Number) && i.VariantId.Equals(item.VariantCode));
                if (it == null)
                {
                    resp.Lines.Add(new OrderLineAvailabilityResponse()
                    {
                        ItemId = item.Number,
                        VariantId = item.VariantCode,
                        UnitOfMeasureId = item.UnitofMeasureCode,
                        Quantity = 0
                    });
                }
            }
            return resp;
        }

        public NavWS.RootCOQtyAvailabilityExtIn MapOneListToInvReq(OneList list)
        {
            List<NavWS.CustomerOrderHeader1> header = new List<NavWS.CustomerOrderHeader1>();
            header.Add(new NavWS.CustomerOrderHeader1()
            {
                DocumentID = string.Empty,
                StoreNo = list.StoreId,
                MemberCardNo = list.CardId ?? string.Empty,
                SourceType = 1,
                SourcingOrder = string.Empty
            });

            List<NavWS.CustomerOrderLine1> lines = new List<NavWS.CustomerOrderLine1>();
            int linenr = 1;
            foreach (OneListItem item in list.Items)
            {
                lines.Add(new NavWS.CustomerOrderLine1()
                {
                    DocumentID = string.Empty,
                    LineNo = LineNumberToNav(linenr++),
                    Number = item.ItemId,
                    VariantCode = item.VariantId,
                    UnitofMeasureCode = item.UnitOfMeasureId,
                    Quantity = item.Quantity,
                    LineType = ((int)LineType.Item).ToString()
                });
            }

            NavWS.RootCOQtyAvailabilityExtIn rootin = new NavWS.RootCOQtyAvailabilityExtIn();
            rootin.CustomerOrderHeader = header.ToArray();
            rootin.CustomerOrderLine = lines.ToArray();
            return rootin;
        }

        public NavWS.RootCOQtyAvailabilityInV2 MapOneListToInvReq2(OneList list)
        {
            List<NavWS.CustomerOrderHeader> header = new List<NavWS.CustomerOrderHeader>();
            header.Add(new NavWS.CustomerOrderHeader()
            {
                DocumentID = string.Empty,
                CreatedAtStore = list.StoreId,
                MemberCardNo = list.CardId ?? string.Empty
            });

            List<NavWS.CustomerOrderLine> lines = new List<NavWS.CustomerOrderLine>();
            int linenr = 1;
            foreach (OneListItem item in list.Items)
            {
                lines.Add(new NavWS.CustomerOrderLine()
                {
                    DocumentID = string.Empty,
                    LineNo = LineNumberToNav(linenr++),
                    Number = item.ItemId,
                    VariantCode = item.VariantId,
                    UnitofMeasureCode = item.UnitOfMeasureId,
                    Quantity = item.Quantity,
                    LineType = ((int)LineType.Item).ToString(),
                    SourcingOrderType = string.Empty
                });
            }

            NavWS.RootCOQtyAvailabilityInV2 rootin = new NavWS.RootCOQtyAvailabilityInV2();
            rootin.CustomerOrderHeader = header.ToArray();
            rootin.CustomerOrderLine = lines.ToArray();
            return rootin;
        }
    }
}
