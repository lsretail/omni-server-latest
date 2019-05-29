using System;
using System.Collections.Generic;
using System.Linq;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Mapping
{
    public class OrderMapping : BaseMapping
    {
        public Order MapFromRootTransactionToOrder(NavWS.RootMobileTransaction root)
        {
            NavWS.MobileTransaction header = root.MobileTransaction.FirstOrDefault();
            UnknownCurrency transactionCurrency = new UnknownCurrency(header.CurrencyCode);

            Order order = new Order()
            {
                Id = header.Id,
                ReceiptNo = header.ReceiptNo,
                TransId = header.TransactionNo.ToString(),
                TransTerminal = header.TerminalId,
                StoreId = header.StoreId,
                CardId = header.MemberCardNo,
                TotalAmount = header.GrossAmount,
                TotalNetAmount = header.NetAmount,
                TotalDiscount = header.LineDiscount,
                PointBalance = header.PointBalance,
                PointsRewarded = header.IssuedPoints,
                PointCashAmountNeeded = header.AmountRemaining,
                PointAmount = header.BasketInPoints,
                PointsUsedInOrder = header.PointsUsedInBasket
            };

            //now loop thru the discount lines
            order.OrderDiscountLines = new List<OrderDiscountLine>();
            if (root.MobileTransDiscountLine != null)
            {
                foreach (NavWS.MobileTransDiscountLine mobileTransDisc in root.MobileTransDiscountLine)
                {
                    OrderDiscountLine discount = new OrderDiscountLine()
                    {
                        //DiscountType: Periodic Disc.,Customer,InfoCode,Total,Line,Promotion,Deal,Total Discount,Tender Type,Item Point,Line Discount,Member Point,Coupon
                        DiscountType = (DiscountType)mobileTransDisc.DiscountType,
                        PeriodicDiscType = (PeriodicDiscType)mobileTransDisc.PeriodicDiscType,
                        DiscountPercent = mobileTransDisc.DiscountPercent,
                        DiscountAmount = mobileTransDisc.DiscountAmount,
                        Description = mobileTransDisc.Description,
                        No = mobileTransDisc.No.ToString(),
                        OfferNumber = mobileTransDisc.OfferNo,
                        LineNumber = LineNumberFromNav(mobileTransDisc.LineNo)
                    };
                    order.OrderDiscountLines.Add(discount);
                }
            }

            //now loop thru the lines
            order.OrderLines = new List<OrderLine>();
            if (root.MobileTransactionLine != null)
            {
                foreach (NavWS.MobileTransactionLine mobileTransLine in root.MobileTransactionLine)
                {
                    LineType lineType = (LineType)mobileTransLine.LineType;
                    if (lineType == LineType.PerDiscount || lineType == LineType.Coupon)
                        continue;

                    OrderLine line = new OrderLine()
                    {
                        LineNumber = LineNumberFromNav(mobileTransLine.LineNo),
                        ItemId = mobileTransLine.Number,
                        Quantity = mobileTransLine.Quantity,
                        QuantityToInvoice = mobileTransLine.Quantity,
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
                    order.OrderLines.Add(line);
                }
            }
            return order;
        }

        public NavWS.RootCustomerOrderCreateV2 MapFromOrderV2ToRoot(Order order, Version navVersion)
        {
            NavWS.RootCustomerOrderCreateV2 root = new NavWS.RootCustomerOrderCreateV2();

            List<NavWS.CustomerOrderHeaderV2> header = new List<NavWS.CustomerOrderHeaderV2>();
            header.Add(new NavWS.CustomerOrderHeaderV2()
            {
                DocumentID = string.Empty,
                ExternalID = order.Id.ToUpper(),
                StoreNo = (order.ClickAndCollectOrder && string.IsNullOrEmpty(order.CollectLocation) == false) ? order.CollectLocation.ToUpper() : order.StoreId.ToUpper(),
                MemberCardNo = GetString(order.CardId),
                FullName = GetString(order.ContactName),
                SourceType = Convert.ToInt32(order.SourceType),
                Address = GetString(order.ContactAddress.Address1),
                Address2 = GetString(order.ContactAddress.Address2),
                HouseApartmentNo = GetString(order.ContactAddress.HouseNo),
                City = GetString(order.ContactAddress.City),
                County = GetString(order.ContactAddress.StateProvinceRegion),
                PostCode = GetString(order.ContactAddress.PostCode),
                CountryRegionCode = GetString(order.ContactAddress.Country),
                PhoneNo = GetString(order.PhoneNumber),
                MobilePhoneNo = GetString(order.MobileNumber),
                DaytimePhoneNo = GetString(order.DayPhoneNumber),
                Email = GetString(order.Email),
                ShipToFullName = GetString(order.ShipToName),
                ShipToAddress = GetString(order.ShipToAddress.Address1),
                ShipToAddress2 = GetString(order.ShipToAddress.Address2),
                ShipToHouseApartmentNo = GetString(order.ShipToAddress.HouseNo),
                ShipToCity = GetString(order.ShipToAddress.City),
                ShipToCounty = GetString(order.ShipToAddress.StateProvinceRegion),
                ShipToPostCode = GetString(order.ShipToAddress.PostCode),
                ShipToCountryRegionCode = GetString(order.ShipToAddress.Country),
                ShipToPhoneNo = GetString(order.ShipToPhoneNumber),
                ShipToEmail = GetString(order.ShipToEmail),
                ClickAndCollectOrder = order.ClickAndCollectOrder,
                ShippingAgentCode = GetString(order.ShippingAgentCode),
                ShippingAgentServiceCode = GetString(order.ShippingAgentServiceCode),
                CustomerNo = string.Empty,
                CreatedAtStore = string.Empty,
                TerritoryCode = string.Empty,
                SourcingLocation = string.Empty,
                ReceiptNo = string.Empty
            });

            if (navVersion > new Version("13.4"))
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
                    VariantCode = GetString(line.VariantId),
                    UnitofMeasureCode = GetString(line.UomId),
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
                    OrderReference = string.Empty,
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
                        OfferNo = GetString(line.OfferNumber),
                        PeriodicDiscType = (int)line.PeriodicDiscType,
                        PeriodicDiscGroup = GetString(line.PeriodicDiscGroup),
                        Description = GetString(line.Description),
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
                        PreApprovedAmount = line.PreApprovedAmount,
                        TenderType = line.TenderType,
                        CardType = GetString(line.CardType),
                        CurrencyCode = GetString(line.CurrencyCode),
                        CurrencyFactor = line.CurrencyFactor,
                        AuthorisationCode = GetString(line.AuthorisationCode),
                        PreApprovedValidDate = line.PreApprovedValidDate,
                        CardorCustomernumber = GetString(line.CardNumber),
                        IncomeExpenseAccountNo = string.Empty,
                        StoreNo = string.Empty
                    });
                }
            }
            root.CustomerOrderPaymentV2 = payLines.ToArray();
            return root;
        }

        public NavWS.RootCustomerOrder MapFromOrderToRoot(Order order, Version navVersion)
        {
            NavWS.RootCustomerOrder root = new NavWS.RootCustomerOrder();

            List<NavWS.CustomerOrderHeader> header = new List<NavWS.CustomerOrderHeader>();
            header.Add(new NavWS.CustomerOrderHeader()
            {
                DocumentId = order.Id,
                StoreNo = (order.ClickAndCollectOrder && string.IsNullOrEmpty(order.CollectLocation) == false) ? order.CollectLocation.ToUpper() : order.StoreId.ToUpper(),
                MemberCardNo = GetString(order.CardId),
                FullName = GetString(order.ContactName),
                SourceType = Convert.ToInt32(order.SourceType),
                Address = GetString(order.ContactAddress.Address1),
                Address2 = GetString(order.ContactAddress.Address2),
                HouseApartmentNo = GetString(order.ContactAddress.HouseNo),
                City = GetString(order.ContactAddress.City),
                County = GetString(order.ContactAddress.StateProvinceRegion),
                PostCode = GetString(order.ContactAddress.PostCode),
                CountryRegionCode = GetString(order.ContactAddress.Country),
                PhoneNo = GetString(order.PhoneNumber),
                MobilePhoneNo = GetString(order.MobileNumber),
                DaytimePhoneNo = GetString(order.DayPhoneNumber),
                Email = GetString(order.Email),
                ShipToFullName = GetString(order.ShipToName),
                ShipToAddress = GetString(order.ShipToAddress.Address1),
                ShipToAddress2 = GetString(order.ShipToAddress.Address2),
                ShipToHouseApartmentNo = GetString(order.ShipToAddress.HouseNo),
                ShipToCity = GetString(order.ShipToAddress.City),
                ShipToCounty = GetString(order.ShipToAddress.StateProvinceRegion),
                ShipToPostCode = GetString(order.ShipToAddress.PostCode),
                ShipToCountryRegionCode = GetString(order.ShipToAddress.Country),
                ShipToPhoneNo = GetString(order.ShipToPhoneNumber),
                ShipToEmail = GetString(order.ShipToEmail),
                ClickAndCollectOrder = order.ClickAndCollectOrder,
                AnonymousOrder = order.AnonymousOrder,
                ShippingAgentCode = GetString(order.ShippingAgentCode),
                ShippingAgentServiceCode = GetString(order.ShippingAgentServiceCode),
                SourcingLocation = string.Empty,
                ReceiptNo = string.Empty
            });

            if (navVersion > new Version("13.4"))
                header[0].CustomerNo = string.Empty;

            root.CustomerOrderHeader = header.ToArray();

            List<NavWS.CustomerOrderLine> orderLines = new List<NavWS.CustomerOrderLine>();
            foreach (OrderLine line in order.OrderLines)
            {
                orderLines.Add(new NavWS.CustomerOrderLine()
                {
                    DocumentId = order.Id,
                    LineNo = LineNumberToNav(line.LineNumber),
                    LineType = Convert.ToInt32(line.LineType).ToString(),
                    Number = line.ItemId,
                    VariantCode = GetString(line.VariantId),
                    UnitofMeasureCode = GetString(line.UomId),
                    Quantity = line.Quantity,
                    DiscountAmount = line.DiscountAmount,
                    DiscountPercent = line.DiscountPercent,
                    Amount = line.Amount,
                    NetAmount = line.NetAmount,
                    VatAmount = line.TaxAmount,
                    Price = line.Price,
                    NetPrice = line.NetPrice,
                    OrderReference = string.Empty,
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
                        OfferNo = GetString(line.OfferNumber),
                        PeriodicDiscType = (int)line.PeriodicDiscType,
                        PeriodicDiscGroup = GetString(line.PeriodicDiscGroup),
                        Description = GetString(line.Description),
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
                        PreApprovedAmount = line.PreApprovedAmount,
                        TenderType = line.TenderType,
                        CardType = GetString(line.CardType),
                        CurrencyCode = GetString(line.CurrencyCode),
                        CurrencyFactor = line.CurrencyFactor,
                        AuthorisationCode = GetString(line.AuthorisationCode),
                        PreApprovedValidDate = line.PreApprovedValidDate,
                        CardorCustomernumber = GetString(line.CardNumber)
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
                CustomerId = GetString(list.CustomerId),
                MemberCardNo = GetString(list.CardId),

                // fill out null fields
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
                transLines.Add(new NavWS.MobileTransactionLine()
                {
                    Id = root.MobileTransaction[0].Id,
                    LineNo = LineNumberToNav(lineno++),
                    EntryStatus = (int)EntryStatus.Normal,
                    LineType = (int)LineType.Item,
                    Number = line.Item.Id,
                    CurrencyFactor = 1,
                    VariantCode = (line.VariantReg == null) ? string.Empty : line.VariantReg.Id,
                    UomId = (line.UnitOfMeasure == null) ? string.Empty : line.UnitOfMeasure.Id,
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
                });
            }

            //Coupons
            if (list.PublishedOffers != null)
            {
                foreach (OneListPublishedOffer line in list.PublishedOffers?.Where(x => x.Type == OfferDiscountType.Coupon))
                {
                    transLines.Add(new NavWS.MobileTransactionLine()
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
                    });
                }
            }

            root.MobileTransactionLine = transLines.ToArray();
            root.MobileReceiptInfo = new List<NavWS.MobileReceiptInfo>().ToArray();
            root.MobileTransactionSubLine = new List<NavWS.MobileTransactionSubLine>().ToArray();
            List<NavWS.MobileTransDiscountLine> discLines = new List<NavWS.MobileTransDiscountLine>();
            return root;
        }

        public OrderAvailabilityResponse MapRootToOrderavailabilty(NavWS.RootGetInventoryMultipleIn rootin, NavWS.RootGetInventoryMultipleOut root)
        {
            OrderAvailabilityResponse resp = new OrderAvailabilityResponse();

            if (root.InventoryBufferOut == null)
                return resp;

            foreach (NavWS.InventoryBufferOut line in root.InventoryBufferOut)
            {
                resp.Lines.Add(new OrderLineAvailabilityResponse()
                {
                    ItemId = line.Number,
                    VariantId = line.Variant,
                    LocationCode = line.Location,
                    Quantity = line.Inventory
                });
            }

            // check for missing items as nav does not return 0 items
            foreach (NavWS.InventoryBufferIn item in rootin.InventoryBufferIn)
            {
                OrderLineAvailabilityResponse it = resp.Lines.Find(i => i.ItemId.Equals(item.Number) && i.VariantId.Equals(item.Variant));
                if (it == null)
                {
                    resp.Lines.Add(new OrderLineAvailabilityResponse()
                    {
                        ItemId = item.Number,
                        VariantId = item.Variant
                    });
                }
            }
            return resp;
        }

        public NavWS.RootGetInventoryMultipleIn MapListToInvReq(List<InventoryRequest> items)
        {
            List<NavWS.InventoryBufferIn> lines = new List<NavWS.InventoryBufferIn>();
            foreach (InventoryRequest item in items)
            {
                lines.Add(new NavWS.InventoryBufferIn()
                {
                    Number = item.ItemId,
                    Variant = item.VariantId ?? string.Empty
                });
            }

            NavWS.RootGetInventoryMultipleIn rootin = new NavWS.RootGetInventoryMultipleIn();
            rootin.InventoryBufferIn = lines.ToArray();
            return rootin;
        }

        public NavWS.RootGetInventoryMultipleIn MapOneListToInvReq(OneList list)
        {
            List<NavWS.InventoryBufferIn> lines = new List<NavWS.InventoryBufferIn>();
            foreach (OneListItem item in list.Items)
            {
                lines.Add(new NavWS.InventoryBufferIn()
                {
                    Number = item.Item?.Id,
                    Variant = item.VariantReg?.Id ?? string.Empty
                });
            }

            NavWS.RootGetInventoryMultipleIn rootin = new NavWS.RootGetInventoryMultipleIn();
            rootin.InventoryBufferIn = lines.ToArray();
            return rootin;
        }
    }
}
