using System;
using System.Collections.Generic;
using System.Linq;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Checkout;

namespace LSOmni.DataAccess.BOConnection.PreCommon.Mapping
{
    public class OrderMapping : BaseMapping
    {
        public OrderMapping(Version lscVersion, bool json)
        {
            LSCVersion = lscVersion;
            IsJson = json;
        }

        public Order MapFromRootTransactionToOrder(LSCentral.RootMobileTransaction root)
        {
            LSCentral.MobileTransaction header = root.MobileTransaction.FirstOrDefault();

            Order order = new Order()
            {
                Id = header.Id,
                ReceiptNo = header.ReceiptNo,
                TransId = header.TransactionNo.ToString(),
                TransTerminal = header.TerminalId,
                StoreId = header.StoreId,
                CardId = header.MemberCardNo,
                CustomerId = header.CustomerId,
                Currency = header.CurrencyCode,
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
                foreach (LSCentral.MobileTransactionLine mobileTransLine in root.MobileTransactionLine)
                {
                    LineType lineType = (LineType)mobileTransLine.LineType;
                    if (lineType == LineType.PerDiscount || lineType == LineType.Coupon)
                        continue;

                    OrderLine line = new OrderLine()
                    {
                        LineNumber = mobileTransLine.LineNo,
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
                        LineType = lineType,
                        ItemImageId = mobileTransLine.RetailImageID
                    };
                    order.OrderLines.Add(line);
                }
            }

            //now loop through the discount lines
            order.OrderDiscountLines = new List<OrderDiscountLine>();
            if (root.MobileTransDiscountLine != null)
            {
                foreach (LSCentral.MobileTransDiscountLine mobileTransDisc in root.MobileTransDiscountLine)
                {
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
                        LineNumber = mobileTransDisc.LineNo
                    };

                    if (discount.DiscountType == DiscountType.Coupon)
                    {
                        LSCentral.MobileTransactionLine tline = root.MobileTransactionLine.ToList().Find(l => l.LineType == 6);
                        if (tline != null)
                            discount.OfferNumber = tline.CouponCode;
                    }

                    foreach (OrderLine ol in order.OrderLines)
                    {
                        if (mobileTransDisc.LineNo >= ol.LineNumber && mobileTransDisc.LineNo < ol.LineNumber + 10000)
                        {
                            ol.DiscountLineNumbers.Add(mobileTransDisc.LineNo);
                            break;
                        }
                    }    
                    order.OrderDiscountLines.Add(discount);
                }
            }
            return order;
        }

        public List<ItemCustomerPrice> MapFromRootTransactionToItemCustPrice(LSCentral.RootMobileTransaction root)
        {
            List<ItemCustomerPrice> items = new List<ItemCustomerPrice>();

            LSCentral.MobileTransaction header = root.MobileTransaction.FirstOrDefault();
            if (root.MobileTransactionLine == null)
                return items;

            foreach (LSCentral.MobileTransactionLine mobileTransLine in root.MobileTransactionLine)
            {
                items.Add(new ItemCustomerPrice()
                {
                    LineNo = mobileTransLine.LineNo,
                    Id = mobileTransLine.Number,
                    Quantity = mobileTransLine.Quantity,
                    DiscountPercent = mobileTransLine.DiscountPercent,
                    Price = mobileTransLine.Price,
                    VariantId = mobileTransLine.VariantCode
                });
            }
            return items;
        }

        public SalesEntry MapFromRootToSalesEntry(LSCentral.RootCustomerOrderGetV3 root)
        {
            LSCentral.CustomerOrderGetCOHeaderV3 header = root.CustomerOrderGetCOHeaderV3.FirstOrDefault();

            SalesEntry order = new SalesEntry()
            {
                Id = header.DocumentID,
                CreateAtStoreId = header.CreatedatStore,
                CustomerOrderNo = header.DocumentID,
                ExternalId = header.ExternalID,
                ClickAndCollectOrder = header.ClickandCollectOrder,
                AnonymousOrder = string.IsNullOrEmpty(header.MemberCardNo),
                DocumentRegTime = ConvertTo.SafeJsonDate(header.Created, IsJson),
                Status = SalesEntryStatus.Created,
                IdType = DocumentIdType.Order,
                Posted = false,
                TotalAmount = header.TotalAmount,
                TotalDiscount = header.TotalDiscount,
                TotalNetAmount = header.OrderNetAmount,
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
                RequestedDeliveryDate = header.RequestedDeliveryDate,
                ShippingStatus = (header.ClickandCollectOrder) ? ShippingStatus.ShippigNotRequired : ShippingStatus.NotYetShipped,
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
                foreach (LSCentral.CustomerOrderGetCODiscountLineV3 line in root.CustomerOrderGetCODiscountLineV3)
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
                        LineNumber = line.LineNo
                    };
                    order.DiscountLines.Add(discount);
                }
            }

            //now loop through the lines
            bool missingtotals = order.TotalAmount == 0;

            order.Lines = new List<SalesEntryLine>();
            if (root.CustomerOrderGetCOLineV3!= null)
            {
                foreach (LSCentral.CustomerOrderGetCOLineV3 oline in root.CustomerOrderGetCOLineV3)
                {
                    LineType lineType = (LineType)Convert.ToInt32(oline.LineType);
                    if (lineType == LineType.PerDiscount || lineType == LineType.Coupon)
                        continue;

                    if (string.IsNullOrEmpty(order.StoreId))
                        order.StoreId = oline.StoreNo;

                    if (oline.ClickAndCollectLine && order.ClickAndCollectOrder == false)
                        order.ClickAndCollectOrder = true;

                    if (missingtotals)
                    {
                        order.TotalAmount += oline.Amount;
                        order.TotalDiscount += oline.DiscountAmount;
                        order.TotalNetAmount += oline.NetAmount;
                    }

                    order.Lines.Add(new SalesEntryLine()
                    {
                        LineNumber = oline.LineNo,
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
                        ClickAndCollectLine = oline.ClickAndCollectLine,
                        ItemImageId = oline.RetailImageID
                    });
                }
            }

            //now loop through the discount lines
            order.Payments = new List<SalesEntryPayment>();
            if (root.CustomerOrderGetCOPaymentV3 != null)
            {
                foreach (LSCentral.CustomerOrderGetCOPaymentV3 line in root.CustomerOrderGetCOPaymentV3)
                {
                    SalesEntryPayment pay = new SalesEntryPayment()
                    {
                        LineNumber = line.LineNo,
                        Amount = (line.FinalizedAmount > 0) ? line.FinalizedAmount : line.PreApprovedAmount,
                        CurrencyCode = line.CurrencyCode,
                        TenderType = line.TenderType,
                        CardType = line.CardType
                    };
                    order.Payments.Add(pay);
                }
            }
            return order;
        }

        public List<SalesEntry> MapFromRootToSalesEntryHistory(LSCentral.RootCOFilteredListV2 root)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            if (root.CustomerOrderHeaderV2 != null)
            {
                foreach (LSCentral.CustomerOrderHeaderV2 header in root.CustomerOrderHeaderV2)
                {
                    SalesEntry entry = new SalesEntry()
                    {
                        Id = header.DocumentID,
                        StoreId = header.CreatedatStore,
                        CardId = header.MemberCardNo,
                        ContactEmail = header.Email,
                        ContactDayTimePhoneNo = header.PhoneNo,
                        StoreName = header.CreatedatStore,
                        AnonymousOrder = string.IsNullOrEmpty(header.MemberCardNo),
                        DocumentRegTime = ConvertTo.SafeJsonDate(header.Created, IsJson),
                        ShipToName = header.FullName,
                        TotalAmount = header.TotalAmount,
                        LineItemCount = (int)header.TotalQuantity,
                        RequestedDeliveryDate = header.RequestedDeliveryDate
                    };

                    if (LSCVersion >= new Version("20.2"))
                        entry.ExternalId = header.ExternalID;
                    
                    list.Add(entry);
                }
            }
            return list;
        }

        public LSCentral.RootCustomerOrderCreateV5 MapFromOrderV5ToRoot(Order order)
        {
            LSCentral.RootCustomerOrderCreateV5 root = new LSCentral.RootCustomerOrderCreateV5();

            List<LSCentral.CustomerOrderCreateCOHeaderV5> header = new List<LSCentral.CustomerOrderCreateCOHeaderV5>();
            LSCentral.CustomerOrderCreateCOHeaderV5 head = new LSCentral.CustomerOrderCreateCOHeaderV5()
            {
                DocumentID = string.Empty,
                ExternalID = order.Id.ToUpper(),
                MemberCardNo = XMLHelper.GetString(order.CardId),
                CustomerNo = XMLHelper.GetString(order.CustomerId),
                Name = XMLHelper.GetString(order.ContactName),
                SourceType = "1", //NAV POS = 0, Omni = 1
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
                RequestedDeliveryDate = order.RequestedDeliveryDate,
                ScanPaygo = (order.OrderType == OrderType.ScanPayGo),
                TerritoryCode = string.Empty
            };

            if (LSCVersion >= new Version("19.0"))
            {
                head.ShippingAgentCode = XMLHelper.GetString(order.ShippingAgentCode);
                head.ShippingAgentServiceCode = XMLHelper.GetString(order.ShippingAgentServiceCode);
            }

            header.Add(head);
            bool useHeaderCAC = false;
            string storeId = order.StoreId.ToUpper();
            if (order.OrderType == OrderType.ClickAndCollect && string.IsNullOrEmpty(order.CollectLocation) == false)
            {
                storeId =  order.CollectLocation.ToUpper();
                useHeaderCAC = true;
            }
            root.CustomerOrderCreateCOHeaderV5 = header.ToArray();

            int lineNo = order.OrderLines.Max(l => l.LineNumber);
            List<LSCentral.CustomerOrderCreateCOLineV5> orderLines = new List<LSCentral.CustomerOrderCreateCOLineV5>();
            foreach (OrderLine line in order.OrderLines)
            {
                if (line.LineNumber == 0)
                    line.LineNumber = ++lineNo;

                LSCentral.CustomerOrderCreateCOLineV5 ln = new LSCentral.CustomerOrderCreateCOLineV5()
                {
                    DocumentID = string.Empty,
                    ExternalID = (string.IsNullOrEmpty(line.Id)) ? string.Empty : line.Id.ToUpper(),
                    LineNo = XMLHelper.LineNumberToNav(line.LineNumber),
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
                    TaxGroupCode = string.Empty,
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
                };

                if (LSCVersion >= new Version("18.2"))
                    ln.ValidateTaxParameter = line.ValidateTax;

                orderLines.Add(ln);
            }
            root.CustomerOrderCreateCOLineV5 = orderLines.ToArray();

            List<LSCentral.CustomerOrderCreateCODiscountLineV5> discLines = new List<LSCentral.CustomerOrderCreateCODiscountLineV5>();
            if (order.OrderDiscountLines != null)
            {
                foreach (OrderDiscountLine line in order.OrderDiscountLines)
                {
                    discLines.Add(new LSCentral.CustomerOrderCreateCODiscountLineV5()
                    {
                        DocumentID = string.Empty,
                        LineNo = XMLHelper.LineNumberToNav(line.LineNumber),
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

            List<LSCentral.CustomerOrderCreateCOPaymentV5> payLines = new List<LSCentral.CustomerOrderCreateCOPaymentV5>();
            if (order.OrderPayments != null)
            {
                foreach (OrderPayment line in order.OrderPayments)
                {
                    string curcode = XMLHelper.GetString(line.CurrencyCode);
                    bool loypayment = (string.IsNullOrEmpty(curcode)) ? false : curcode.Equals("LOY", StringComparison.InvariantCultureIgnoreCase);

                    LSCentral.CustomerOrderCreateCOPaymentV5 pay = new LSCentral.CustomerOrderCreateCOPaymentV5()
                    {
                        DocumentID = string.Empty,
                        LineNo = XMLHelper.LineNumberToNav(line.LineNumber),
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
                        DepositPayment = line.DepositPayment,
                        IncomeExpenseAccountNo = string.Empty,
                        StoreNo = (order.OrderType == OrderType.ClickAndCollect && string.IsNullOrEmpty(order.CollectLocation) == false) ? order.CollectLocation.ToUpper() : order.StoreId.ToUpper(),
                        PosTransReceiptNo = string.Empty,
                        TaxGroupCode = string.Empty
                    };

                    if (LSCVersion >= new Version("19.5"))
                    {
                        pay.EFTCardType = XMLHelper.GetString(line.EFTCardType);
                    }

                    payLines.Add(pay);
                }
            }
            root.CustomerOrderCreateCOPaymentV5 = payLines.ToArray();
            return root;
        }

        public LSCentral.RootMobileTransaction MapFromRetailTransactionToRoot(OneList list)
        {
            LSCentral.RootMobileTransaction root = new LSCentral.RootMobileTransaction();

            //MobileTrans
            List<LSCentral.MobileTransaction> trans = new List<LSCentral.MobileTransaction>();
            LSCentral.MobileTransaction head = new LSCentral.MobileTransaction()
            {
                Id = list.Id,
                StoreId = list.StoreId.ToUpper(),
                TransactionType = 2,
                EntryStatus = (int)EntryStatus.Normal,
                MemberCardNo = XMLHelper.GetString(list.CardId),
                TransDate = DateTime.Now,
                PointsUsedInBasket = list.PointAmount,

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
                TerminalId = string.Empty
            };

            if (LSCVersion >= new Version("19.0"))
            {
                head.ShipToCountryRegionCode = XMLHelper.GetString(list.ShipToCountryCode);
            }

            trans.Add(head);
            root.MobileTransaction = trans.ToArray();

            //MobileTransLines
            List<LSCentral.MobileTransactionLine> transLines = new List<LSCentral.MobileTransactionLine>();
            foreach (OneListItem line in list.Items)
            {
                transLines.Add(new LSCentral.MobileTransactionLine()
                {
                    Id = root.MobileTransaction[0].Id,
                    LineNo = line.LineNumber,
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
                    TransDate = DateTime.Now,

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
                    RetailImageID = string.Empty
                });
            }

            //Coupons
            if (list.PublishedOffers != null)
            {
                foreach (OneListPublishedOffer line in list.PublishedOffers?.Where(x => x.Type == OfferDiscountType.Coupon))
                {
                    transLines.Add(new LSCentral.MobileTransactionLine()
                    {
                        Id = root.MobileTransaction[0].Id,
                        LineNo = line.LineNumber,
                        EntryStatus = (int)EntryStatus.Normal,
                        LineType = (int)LineType.Coupon,
                        Number = line.Id,
                        CurrencyFactor = 1,
                        StoreId = list.StoreId.ToUpper(),
                        TransDate = DateTime.Now,

                        Barcode = line.Id,
                        VariantCode = string.Empty,
                        UomId = string.Empty,
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
                        RetailImageID = string.Empty
                    });
                }
            }

            root.MobileTransactionLine = transLines.ToArray();
            root.MobileReceiptInfo = new List<LSCentral.MobileReceiptInfo>().ToArray();
            root.MobileTransactionSubLine = new List<LSCentral.MobileTransactionSubLine>().ToArray();
            List<LSCentral.MobileTransDiscountLine> discLines = new List<LSCentral.MobileTransDiscountLine>();
            return root;
        }

        public LSCentral.RootMobileTransaction MapFromCustItemToRoot(string storeId, string cardId, List<ItemCustomerPrice> items)
        {
            LSCentral.RootMobileTransaction root = new LSCentral.RootMobileTransaction();

            //MobileTrans
            string id = Guid.NewGuid().ToString();
            List<LSCentral.MobileTransaction> trans = new List<LSCentral.MobileTransaction>();
            trans.Add(new LSCentral.MobileTransaction()
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
            List<LSCentral.MobileTransactionLine> transLines = new List<LSCentral.MobileTransactionLine>();
            foreach (ItemCustomerPrice line in items)
            {
                transLines.Add(new LSCentral.MobileTransactionLine()
                {
                    Id = id,
                    LineNo = XMLHelper.LineNumberToNav(lineno++),
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
            root.MobileReceiptInfo = new List<LSCentral.MobileReceiptInfo>().ToArray();
            root.MobileTransactionSubLine = new List<LSCentral.MobileTransactionSubLine>().ToArray();
            List<LSCentral.MobileTransDiscountLine> discLines = new List<LSCentral.MobileTransDiscountLine>();
            return root;
        }

        public OrderAvailabilityResponse MapRootToOrderavailabilty2(LSCentral.RootCOQtyAvailabilityInV2 rootin, LSCentral.RootCOQtyAvailabilityExtOut root)
        {
            OrderAvailabilityResponse resp = new OrderAvailabilityResponse();

            if (root.WSInventoryBuffer == null)
                return resp;

            foreach (LSCentral.WSInventoryBuffer line in root.WSInventoryBuffer)
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
            foreach (LSCentral.CustomerOrderLine item in rootin.CustomerOrderLine)
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

        public LSCentral.RootCOQtyAvailabilityInV2 MapOneListToInvReq2(OneList list, bool shipOrder)
        {
            List<LSCentral.CustomerOrderHeader> header = new List<LSCentral.CustomerOrderHeader>();
            header.Add(new LSCentral.CustomerOrderHeader()
            {
                DocumentID = string.Empty,
                CreatedAtStore = list.StoreId,
                MemberCardNo = list.CardId ?? string.Empty,
                ShipOrder = shipOrder
            });

            List<LSCentral.CustomerOrderLine> lines = new List<LSCentral.CustomerOrderLine>();
            int linenr = 1;
            foreach (OneListItem item in list.Items)
            {
                lines.Add(new LSCentral.CustomerOrderLine()
                {
                    DocumentID = string.Empty,
                    LineNo = XMLHelper.LineNumberToNav(linenr++),
                    Number = item.ItemId,
                    VariantCode = item.VariantId,
                    UnitofMeasureCode = item.UnitOfMeasureId,
                    Quantity = item.Quantity,
                    LineType = ((int)LineType.Item).ToString(),
                    SourcingOrderType = string.Empty
                });
            }

            LSCentral.RootCOQtyAvailabilityInV2 rootin = new LSCentral.RootCOQtyAvailabilityInV2();
            rootin.CustomerOrderHeader = header.ToArray();
            rootin.CustomerOrderLine = lines.ToArray();
            return rootin;
        }

        public OrderCheck RootToOrderCheck(LSCentral.RootSPGOrderCheck root)
        {
            OrderCheck order = new OrderCheck();
            if (root.SPGOrderCheckCOHeader == null || root.SPGOrderCheckCOHeader.Count() == 0)
                return order;

            order.StatusDate = root.SPGOrderCheckCOHeader[0].StatusDateTime;
            order.Status = root.SPGOrderCheckCOHeader[0].Status;

            if (root.SPGOrderCheckCOLine != null)
            {
                order.Lines = new List<OrderCheckLines>();
                foreach (LSCentral.SPGOrderCheckCOLine line in root.SPGOrderCheckCOLine)
                {
                    order.Lines.Add(new OrderCheckLines()
                    {
                        DocumentID = line.DocumentID,
                        ItemId = line.Number,
                        ItemDescription = line.ItemDescription,
                        VariantCode = line.VariantCode,
                        VariantDescription = line.VariantDescription,
                        UnitofMeasureCode = line.UnitofMeasureCode,
                        UOMDescription = line.UoMDescription,
                        Amount = line.Amount,
                        LineNo = line.LineNo,
                        Quantity = line.Quantity,
                        AlwaysCheck = line.AlwaysCheck
                    });
                }
            }

            if (root.SPGOrderCheckCOPayment != null)
            {
                order.Payments = new List<OrderCheckPayment>();
                foreach (LSCentral.SPGOrderCheckCOPayment pay in root.SPGOrderCheckCOPayment)
                {
                    order.Payments.Add(new OrderCheckPayment()
                    {
                        CardType = pay.CardType,
                        AutorizationCode = pay.AutorizationCode,
                        ExternalRef = pay.ExternalRef,
                        PaymentAmount = pay.PaymentAmount
                    });
                }
            }
            return order;
        }
    }
}
