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

namespace LSOmni.DataAccess.BOConnection.PreCommon.Mapping25
{
    public class OrderMapping25 : BaseMapping25
    {
        public OrderMapping25(Version lscVersion, bool json)
        {
            LSCVersion = lscVersion;
            IsJson = json;
        }

        public Order MapFromRootTransactionToOrder(LSCentral25.RootMobileTransaction root)
        {
            LSCentral25.MobileTransaction header = root.MobileTransaction.FirstOrDefault();

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
                foreach (LSCentral25.MobileTransactionLine mobileTransLine in root.MobileTransactionLine)
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
                foreach (LSCentral25.MobileTransDiscountLine mobileTransDisc in root.MobileTransDiscountLine)
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
                        LSCentral25.MobileTransactionLine tline = root.MobileTransactionLine.ToList().Find(l => l.LineType == 6);
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

        public List<ItemCustomerPrice> MapFromRootTransactionToItemCustPrice(LSCentral25.RootMobileTransaction root)
        {
            List<ItemCustomerPrice> items = new List<ItemCustomerPrice>();

            LSCentral25.MobileTransaction header = root.MobileTransaction.FirstOrDefault();
            if (root.MobileTransactionLine == null)
                return items;

            foreach (LSCentral25.MobileTransactionLine mobileTransLine in root.MobileTransactionLine)
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

        public SalesEntry MapFromRootToSalesEntry(LSCentral25.RootCustomerOrderGetV3 root)
        {
            LSCentral25.CustomerOrderGetCOHeaderV3 header = root.CustomerOrderGetCOHeaderV3.FirstOrDefault();

            SalesEntry order = new SalesEntry()
            {
                Id = header.DocumentID,
                CreateAtStoreId = header.CreatedatStore,
                CustomerOrderNo = header.DocumentID,
                ExternalId = header.ExternalID,
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
                    County = header.County,
                    StateProvinceRegion = header.TerritoryCode,
                    PhoneNumber = header.PhoneNo,
                    CellPhoneNumber = header.MobilePhoneNo
                },

                ShipToName = header.ShiptoName,
                ShipToEmail = header.ShiptoEmail,
                RequestedDeliveryDate = header.RequestedDeliveryDate,
                ShipToAddress = new Address()
                {
                    Address1 = header.ShiptoAddress,
                    Address2 = header.ShiptoAddress2,
                    HouseNo = header.ShiptoHouseApartmentNo,
                    City = header.ShiptoCity,
                    Country = header.ShiptoCountryRegionCode,
                    PostCode = header.ShiptoPostCode,
                    County = header.ShiptoCounty,
                    PhoneNumber = header.ShiptoPhoneNo
                }
            };

            //now loop through the discount lines
            order.DiscountLines = new List<SalesEntryDiscountLine>();
            if (root.CustomerOrderGetCODiscountLineV3 != null)
            {
                foreach (LSCentral25.CustomerOrderGetCODiscountLineV3 line in root.CustomerOrderGetCODiscountLineV3)
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
                foreach (LSCentral25.CustomerOrderGetCOLineV3 oline in root.CustomerOrderGetCOLineV3)
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

                    SalesEntryLine eline = new SalesEntryLine()
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
                    };
                    order.Lines.Add(eline);
                }
            }
            order.ShippingStatus = (order.ClickAndCollectOrder) ? ShippingStatus.ShippigNotRequired : ShippingStatus.NotYetShipped;

            if (root.CustomerOrderGetCOLineDataEntryV3 != null)
            {
                foreach (LSCentral25.CustomerOrderGetCOLineDataEntryV3 entry in root.CustomerOrderGetCOLineDataEntryV3)
                {
                    SalesEntryLine rec = order.Lines.Find(l => l.LineNumber == entry.LineNo);
                    if (rec != null)
                        rec.ExtraInformation = $"Code:{entry.DataEntryCode} Type:{entry.DataEntryType} Pin:{entry.PIN}";
                }
            }

            //now loop through the discount lines
            order.Payments = new List<SalesEntryPayment>();
            if (root.CustomerOrderGetCOPaymentV3 != null)
            {
                foreach (LSCentral25.CustomerOrderGetCOPaymentV3 line in root.CustomerOrderGetCOPaymentV3)
                {
                    SalesEntryPayment pay = new SalesEntryPayment()
                    {
                        LineNumber = line.LineNo,
                        Amount = (line.FinalizedAmount > 0) ? line.FinalizedAmount : line.PreApprovedAmount,
                        AmountLCY = (line.FinalizedAmountLCY > 0) ? line.FinalizedAmountLCY : line.PreApprovedAmountLCY,
                        CurrencyCode = line.CurrencyCode,
                        TenderType = line.TenderType,
                        CardType = line.CardType
                    };
                    order.Payments.Add(pay);
                }
            }
            return order;
        }

        public List<SalesEntry> MapFromRootToSalesEntryHistory(LSCentral25.RootCOFilteredListV2 root)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            if (root.CustomerOrderHeaderV2 != null)
            {
                foreach (LSCentral25.CustomerOrderHeaderV2 header in root.CustomerOrderHeaderV2)
                {
                    list.Add(new SalesEntry()
                    {
                        Id = header.DocumentID,
                        ExternalId = header.ExternalID,
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
                    });
                }
            }
            return list;
        }

        public LSCentral25.RootCustomerOrderCreateV5 MapFromOrderV5ToRoot(Order order, string loyCur)
        {
            LSCentral25.RootCustomerOrderCreateV5 root = new LSCentral25.RootCustomerOrderCreateV5();

            if (order.OrderType == OrderType.Sale)
                order.ShipOrder = true;

            List<LSCentral25.CustomerOrderCreateCOHeaderV5> header = new List<LSCentral25.CustomerOrderCreateCOHeaderV5>();
            LSCentral25.CustomerOrderCreateCOHeaderV5 head = new LSCentral25.CustomerOrderCreateCOHeaderV5()
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
                County = XMLHelper.GetString(order.ContactAddress.County),
                PostCode = XMLHelper.GetString(order.ContactAddress.PostCode),
                CountryRegionCode = XMLHelper.GetString(order.ContactAddress.Country),
                TerritoryCode = XMLHelper.GetString(order.ContactAddress.StateProvinceRegion),
                PhoneNo = XMLHelper.GetString(order.ContactAddress.PhoneNumber),
                MobilePhoneNo = XMLHelper.GetString(order.ContactAddress.CellPhoneNumber),
                DaytimePhoneNo = XMLHelper.GetString(order.DayPhoneNumber),
                Email = XMLHelper.GetString(order.Email),
                ShipToName = XMLHelper.GetString(order.ShipToName),
                ShipToAddress = XMLHelper.GetString(order.ShipToAddress.Address1),
                ShipToAddress2 = XMLHelper.GetString(order.ShipToAddress.Address2),
                ShipToHouseApartmentNo = XMLHelper.GetString(order.ShipToAddress.HouseNo),
                ShipToCity = XMLHelper.GetString(order.ShipToAddress.City),
                ShipToCounty = XMLHelper.GetString(order.ShipToAddress.County),
                ShipToPostCode = XMLHelper.GetString(order.ShipToAddress.PostCode),
                ShipToCountryRegionCode = XMLHelper.GetString(order.ShipToAddress.Country),
                ShipToPhoneNo = XMLHelper.GetString(order.ShipToAddress.PhoneNumber),
                ShipToEmail = XMLHelper.GetString(order.ShipToEmail),
                ShippingAgentCode = XMLHelper.GetString(order.ShippingAgentCode),
                ShippingAgentServiceCode = XMLHelper.GetString(order.ShippingAgentServiceCode),
                ShipOrder = order.ShipOrder,
                CreatedAtStore = order.StoreId,
                RequestedDeliveryDate = order.RequestedDeliveryDate,
                ScanPaygo = (order.OrderType == OrderType.ScanPayGo)
            };

            bool useHeaderCAC = false;
            string storeId = order.StoreId.ToUpper();
            if (order.OrderType == OrderType.ClickAndCollect && string.IsNullOrEmpty(order.CollectLocation) == false)
            {
                storeId = order.CollectLocation.ToUpper();
                useHeaderCAC = true;
            }

            header.Add(head);
            root.CustomerOrderCreateCOHeaderV5 = header.ToArray();

            int lineNo = order.OrderLines.Max(l => l.LineNumber);
            List<LSCentral25.CustomerOrderCreateCOLineV5> orderLines = new List<LSCentral25.CustomerOrderCreateCOLineV5>();
            foreach (OrderLine line in order.OrderLines)
            {
                if (line.LineNumber == 0)
                    line.LineNumber = ++lineNo;

                orderLines.Add(new LSCentral25.CustomerOrderCreateCOLineV5()
                {
                    DocumentID = string.Empty,
                    ExternalID = (string.IsNullOrEmpty(line.Id)) ? string.Empty : line.Id.ToUpper(),
                    LineNo = XMLHelper.LineNumberToNav(line.LineNumber),
                    LineType = Convert.ToInt32(line.LineType).ToString(),
                    Number = line.ItemId,
                    VariantCode = XMLHelper.GetString(line.VariantId),
                    UnitofMeasureCode = XMLHelper.GetString(line.UomId),
                    OrderReference = XMLHelper.GetString(line.OrderId),
                    StoreNo = string.IsNullOrEmpty(line.StoreId) ? storeId : line.StoreId.ToUpper(),
                    ClickAndCollect = (useHeaderCAC) ? (order.OrderType == OrderType.ClickAndCollect) : line.ClickAndCollectLine,
                    Quantity = line.Quantity,
                    SourcingLocation = XMLHelper.GetString(line.SourcingLocation),
                    InventoryTransfer = line.InventoryTransfer,
                    VendorSourcing = line.VendorSourcing,
                    DiscountAmount = line.DiscountAmount,
                    DiscountPercent = line.DiscountPercent,
                    Amount = line.Amount,
                    NetAmount = line.NetAmount,
                    VatAmount = line.TaxAmount,
                    Price = line.Price,
                    NetPrice = line.NetPrice,
                    ValidateTaxParameter = line.ValidateTax
                });
            }
            root.CustomerOrderCreateCOLineV5 = orderLines.ToArray();

            List<LSCentral25.CustomerOrderCreateCODiscountLineV5> discLines = new List<LSCentral25.CustomerOrderCreateCODiscountLineV5>();
            if (order.OrderDiscountLines != null)
            {
                foreach (OrderDiscountLine line in order.OrderDiscountLines)
                {
                    discLines.Add(new LSCentral25.CustomerOrderCreateCODiscountLineV5()
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

            List<LSCentral25.CustomerOrderCreateCOPaymentV5> payLines = new List<LSCentral25.CustomerOrderCreateCOPaymentV5>();
            if (order.OrderPayments != null)
            {
                foreach (OrderPayment line in order.OrderPayments)
                {
                    string curcode = XMLHelper.GetString(line.CurrencyCode);
                    bool loypayment = (string.IsNullOrEmpty(curcode)) ? false : curcode.Equals(loyCur, StringComparison.InvariantCultureIgnoreCase);

                    payLines.Add(new LSCentral25.CustomerOrderCreateCOPaymentV5()
                    {
                        DocumentID = string.Empty,
                        LineNo = XMLHelper.LineNumberToNav(line.LineNumber),
                        PreApprovedAmount = line.Amount,
                        PreApprovedAmountLCY = line.CurrencyFactor * line.Amount,
                        Type = ((int)line.PaymentType).ToString(),
                        TenderType = line.TenderType,
                        CardType = XMLHelper.GetString(line.CardType),
                        EFTCardType = XMLHelper.GetString(line.EFTCardType),
                        StoreNo = (order.OrderType == OrderType.ClickAndCollect && string.IsNullOrEmpty(order.CollectLocation) == false) ? order.CollectLocation.ToUpper() : order.StoreId.ToUpper(),
                        CurrencyCode = curcode,
                        CurrencyFactor = line.CurrencyFactor,
                        AuthorizationCode = XMLHelper.GetString(line.AuthorizationCode),
                        AuthorizationExpired = line.AuthorizationExpired,
                        TokenNo = XMLHelper.GetString(line.TokenNumber),
                        ExternalReference = XMLHelper.GetString(line.ExternalReference),
                        PreApprovedValidDate = line.PreApprovedValidDate,
                        CardorCustomernumber = XMLHelper.GetString(line.CardNumber),
                        LoyaltyPointpayment = loypayment,
                        DepositPayment = line.DepositPayment
                    });
                }
            }
            root.CustomerOrderCreateCOPaymentV5 = payLines.ToArray();
            return root;
        }

        public LSCentral25.RootCustomerOrderEdit MapFromOrderEditToRoot(Order order, string orderId, OrderEditType editType, string loyCur)
        {
            LSCentral25.RootCustomerOrderEdit root = new LSCentral25.RootCustomerOrderEdit();

            if (order.OrderType == OrderType.Sale)
                order.ShipOrder = true;

            List<LSCentral25.COEditHeader> header = new List<LSCentral25.COEditHeader>();
            LSCentral25.COEditHeader head = new LSCentral25.COEditHeader()
            {
                DocumentID = orderId,
                ExternalID = order.Id.ToUpper(),
                MemberCardNo = XMLHelper.GetString(order.CardId),
                CustomerNo = XMLHelper.GetString(order.CustomerId),
                Name = XMLHelper.GetString(order.ContactName),
                SourceType = "1", //NAV POS = 0, Omni = 1
                Address = XMLHelper.GetString(order.ContactAddress.Address1),
                Address2 = XMLHelper.GetString(order.ContactAddress.Address2),
                HouseApartmentNo = XMLHelper.GetString(order.ContactAddress.HouseNo),
                City = XMLHelper.GetString(order.ContactAddress.City),
                County = XMLHelper.GetString(order.ContactAddress.County),
                PostCode = XMLHelper.GetString(order.ContactAddress.PostCode),
                CountryRegionCode = XMLHelper.GetString(order.ContactAddress.Country),
                TerritoryCode = XMLHelper.GetString(order.ContactAddress.StateProvinceRegion),
                PhoneNo = XMLHelper.GetString(order.ContactAddress.PhoneNumber),
                MobilePhoneNo = XMLHelper.GetString(order.ContactAddress.CellPhoneNumber),
                DaytimePhoneNo = XMLHelper.GetString(order.DayPhoneNumber),
                Email = XMLHelper.GetString(order.Email),
                ShipToName = XMLHelper.GetString(order.ShipToName),
                ShipToAddress = XMLHelper.GetString(order.ShipToAddress.Address1),
                ShipToAddress2 = XMLHelper.GetString(order.ShipToAddress.Address2),
                ShipToHouseApartmentNo = XMLHelper.GetString(order.ShipToAddress.HouseNo),
                ShipToCity = XMLHelper.GetString(order.ShipToAddress.City),
                ShipToCounty = XMLHelper.GetString(order.ShipToAddress.County),
                ShipToPostCode = XMLHelper.GetString(order.ShipToAddress.PostCode),
                ShipToCountryRegionCode = XMLHelper.GetString(order.ShipToAddress.Country),
                ShipToPhoneNo = XMLHelper.GetString(order.ShipToAddress.PhoneNumber),
                ShipToEmail = XMLHelper.GetString(order.ShipToEmail),
                ShipOrder = order.ShipOrder,
                ShippingAgentCode = XMLHelper.GetString(order.ShippingAgentCode),
                ShippingAgentServiceCode = XMLHelper.GetString(order.ShippingAgentServiceCode),
                CreatedAtStore = order.StoreId,
                RequestedDeliveryDate = order.RequestedDeliveryDate,
                ScanPaygo = (order.OrderType == OrderType.ScanPayGo)
            };

            header.Add(head);
            string storeId = order.StoreId.ToUpper();
            if (order.OrderType == OrderType.ClickAndCollect && string.IsNullOrEmpty(order.CollectLocation) == false)
            {
                storeId = order.CollectLocation.ToUpper();
            }
            root.COEditHeader = header.ToArray();

            if (editType == OrderEditType.Header)
                return root;

            int lineNo = order.OrderLines.Max(l => l.LineNumber);
            List<LSCentral25.COEditLine> orderLines = new List<LSCentral25.COEditLine>();
            foreach (OrderLine line in order.OrderLines)
            {
                if (line.LineNumber == 0)
                    line.LineNumber = ++lineNo;

                orderLines.Add(new LSCentral25.COEditLine()
                {
                    DocumentID = orderId,
                    ExternalID = (string.IsNullOrEmpty(line.Id)) ? string.Empty : line.Id.ToUpper(),
                    StoreNo = string.IsNullOrEmpty(line.StoreId) ? storeId : line.StoreId.ToUpper(),
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
                    ValidateTaxParameter = line.ValidateTax,
                    Price = line.Price,
                    NetPrice = line.NetPrice
                });
            }
            root.COEditLine = orderLines.ToArray();

            List<LSCentral25.COEditDiscountLine> discLines = new List<LSCentral25.COEditDiscountLine>();
            if (order.OrderDiscountLines != null)
            {
                foreach (OrderDiscountLine line in order.OrderDiscountLines)
                {
                    discLines.Add(new LSCentral25.COEditDiscountLine()
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
            root.COEditDiscountLine = discLines.ToArray();

            List<LSCentral25.COEditPayment> payLines = new List<LSCentral25.COEditPayment>();
            if (order.OrderPayments != null)
            {
                foreach (OrderPayment line in order.OrderPayments)
                {
                    string curCode = XMLHelper.GetString(line.CurrencyCode);
                    bool loyPayment = (string.IsNullOrEmpty(curCode)) ? false : curCode.Equals(loyCur, StringComparison.InvariantCultureIgnoreCase);

                    payLines.Add(new LSCentral25.COEditPayment()
                    {
                        DocumentID = string.Empty,
                        LineNo = XMLHelper.LineNumberToNav(line.LineNumber),
                        PreApprovedAmount = line.Amount,
                        PreApprovedAmountLCY = line.CurrencyFactor * line.Amount,
                        Type = ((int)line.PaymentType).ToString(),
                        TenderType = line.TenderType,
                        CardType = XMLHelper.GetString(line.CardType),
                        StoreNo = (order.OrderType == OrderType.ClickAndCollect && string.IsNullOrEmpty(order.CollectLocation) == false) ? order.CollectLocation.ToUpper() : order.StoreId.ToUpper(),
                        CurrencyCode = curCode,
                        CurrencyFactor = line.CurrencyFactor,
                        AuthorizationCode = XMLHelper.GetString(line.AuthorizationCode),
                        AuthorizationExpired = line.AuthorizationExpired,
                        TokenNo = XMLHelper.GetString(line.TokenNumber),
                        ExternalReference = XMLHelper.GetString(line.ExternalReference),
                        PreApprovedValidDate = line.PreApprovedValidDate,
                        CardorCustomernumber = XMLHelper.GetString(line.CardNumber),
                        LoyaltyPointpayment = loyPayment,
                        DepositPayment = line.DepositPayment,
                        EFTCardType = XMLHelper.GetString(line.EFTCardType)
                    });
                }
            }
            root.COEditPayment = payLines.ToArray();
            return root;
        }

        public LSCentral25.RootCOUpdatePayment MapFromOrderPaymentToRoot(string orderId, string storeId, OrderPayment payment, string loyCur)
        {
            LSCentral25.RootCOUpdatePayment root = new LSCentral25.RootCOUpdatePayment();
            if (payment == null)
                return root;

            string curCode = XMLHelper.GetString(payment.CurrencyCode);
            bool loyPayment = (string.IsNullOrEmpty(curCode)) ? false : curCode.Equals(loyCur, StringComparison.InvariantCultureIgnoreCase);

            List<LSCentral25.CustomerOrderPayment> payLines = new List<LSCentral25.CustomerOrderPayment>();
            LSCentral25.CustomerOrderPayment pay = new LSCentral25.CustomerOrderPayment()
            {
                DocumentID = orderId,
                StoreNo = storeId,
                LineNo = XMLHelper.LineNumberToNav(payment.LineNumber),
                Type = ((int)payment.PaymentType).ToString(),
                TenderType = payment.TenderType,
                CardType = XMLHelper.GetString(payment.CardType),
                CurrencyCode = curCode,
                CurrencyFactor = payment.CurrencyFactor,
                AuthorisationCode = XMLHelper.GetString(payment.AuthorizationCode),
                AuthorizationExpired = payment.AuthorizationExpired,
                TokenNo = XMLHelper.GetString(payment.TokenNumber),
                ExternalReference = XMLHelper.GetString(payment.ExternalReference),
                PreApprovedValidDate = payment.PreApprovedValidDate,
                CardorCustomernumber = XMLHelper.GetString(payment.CardNumber),
                LoyaltyPointpayment = loyPayment,
                DepositPayment = payment.DepositPayment
            };

            if (payment.PaymentType == PaymentType.PreAuthorization)
            {
                pay.PreApprovedAmount = payment.Amount;
                pay.PreApprovedAmountLCY = payment.CurrencyFactor * payment.Amount;
            }
            else
            {
                pay.FinalisedAmount = payment.Amount;
                pay.FinalisedAmountLCY = payment.CurrencyFactor * payment.Amount;
            }
            payLines.Add(pay);
            root.CustomerOrderPayment = payLines.ToArray();
            return root;
        }

        public LSCentral25.RootMobileTransaction MapFromRetailTransactionToRoot(OneList list)
        {
            LSCentral25.RootMobileTransaction root = new LSCentral25.RootMobileTransaction();

            if (list.CurrencyFactor == 0)
                list.CurrencyFactor = 1;

            //MobileTrans
            List<LSCentral25.MobileTransaction> trans = new List<LSCentral25.MobileTransaction>();
            LSCentral25.MobileTransaction head = new LSCentral25.MobileTransaction()
            {
                Id = list.Id,
                StoreId = list.StoreId.ToUpper(),
                TransactionType = 2,
                EntryStatus = (int)EntryStatus.Normal,
                MemberCardNo = XMLHelper.GetString(list.CardId),
                TransDate = DateTime.Now,
                PointsUsedInBasket = list.PointAmount,
                CurrencyFactor = list.CurrencyFactor,
                SourceType = "1" //NAV POS = 0, Omni = 1
            };

            trans.Add(head);
            root.MobileTransaction = trans.ToArray();

            //MobileTransLines
            List<LSCentral25.MobileTransactionLine> transLines = new List<LSCentral25.MobileTransactionLine>();
            foreach (OneListItem line in list.Items)
            {
                transLines.Add(new LSCentral25.MobileTransactionLine()
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
                    TransDate = DateTime.Now
                });
            }

            //Coupons
            if (list.PublishedOffers != null)
            {
                foreach (OneListPublishedOffer line in list.PublishedOffers?.Where(x => x.Type == OfferDiscountType.Coupon))
                {
                    transLines.Add(new LSCentral25.MobileTransactionLine()
                    {
                        Id = root.MobileTransaction[0].Id,
                        LineNo = line.LineNumber,
                        EntryStatus = (int)EntryStatus.Normal,
                        LineType = (int)LineType.Coupon,
                        Number = line.Id,
                        CurrencyFactor = 1,
                        StoreId = list.StoreId.ToUpper(),
                        TransDate = DateTime.Now,
                        Barcode = line.Id
                    });
                }
            }

            root.MobileTransactionLine = transLines.ToArray();
            root.MobileReceiptInfo = new List<LSCentral25.MobileReceiptInfo>().ToArray();
            root.MobileTransactionSubLine = new List<LSCentral25.MobileTransactionSubLine>().ToArray();
            List<LSCentral25.MobileTransDiscountLine> discLines = new List<LSCentral25.MobileTransDiscountLine>();
            return root;
        }

        public LSCentral25.RootMobileTransaction MapFromCustItemToRoot(string storeId, string cardId, List<ItemCustomerPrice> items)
        {
            LSCentral25.RootMobileTransaction root = new LSCentral25.RootMobileTransaction();

            //MobileTrans
            string id = Guid.NewGuid().ToString();
            List<LSCentral25.MobileTransaction> trans = new List<LSCentral25.MobileTransaction>();
            trans.Add(new LSCentral25.MobileTransaction()
            {
                Id = id,
                StoreId = storeId,
                TransactionType = 0,
                EntryStatus = (int)EntryStatus.Normal,
                MemberCardNo = cardId,
                TransDate = DateTime.Now,
                SourceType = "1" //NAV POS = 0, Omni = 1
            });
            root.MobileTransaction = trans.ToArray();

            //MobileTransLines
            int lineno = 1;
            List<LSCentral25.MobileTransactionLine> transLines = new List<LSCentral25.MobileTransactionLine>();
            foreach (ItemCustomerPrice line in items)
            {
                transLines.Add(new LSCentral25.MobileTransactionLine()
                {
                    Id = id,
                    LineNo = XMLHelper.LineNumberToNav(lineno++),
                    EntryStatus = (int)EntryStatus.Normal,
                    LineType = (int)LineType.Item,
                    Number = line.Id,
                    CurrencyFactor = 1,
                    VariantCode = line.VariantId,
                    Quantity = line.Quantity,
                    TransDate = DateTime.Now,
                    StoreId = storeId
                });
            }

            root.MobileTransactionLine = transLines.ToArray();
            root.MobileReceiptInfo = new List<LSCentral25.MobileReceiptInfo>().ToArray();
            root.MobileTransactionSubLine = new List<LSCentral25.MobileTransactionSubLine>().ToArray();
            root.MobileTransDiscountLine = new List<LSCentral25.MobileTransDiscountLine>().ToArray();
            return root;
        }

        public OrderAvailabilityResponse MapRootToOrderavailabilty2(LSCentral25.RootCOQtyAvailabilityInV2 rootin, LSCentral25.RootCOQtyAvailabilityExtOut root)
        {
            OrderAvailabilityResponse resp = new OrderAvailabilityResponse();

            if (root.WSInventoryBuffer == null)
                return resp;

            foreach (LSCentral25.WSInventoryBuffer line in root.WSInventoryBuffer)
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
            foreach (LSCentral25.CustomerOrderLine item in rootin.CustomerOrderLine)
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

        public LSCentral25.RootCOQtyAvailabilityInV2 MapOneListToInvReq2(OneList list, bool shipOrder)
        {
            List<LSCentral25.CustomerOrderHeader> header = new List<LSCentral25.CustomerOrderHeader>();
            header.Add(new LSCentral25.CustomerOrderHeader()
            {
                DocumentID = string.Empty,
                CreatedAtStore = list.StoreId,
                MemberCardNo = XMLHelper.GetString(list.CardId),
                ShipOrder = shipOrder
            });

            List<LSCentral25.CustomerOrderLine> lines = new List<LSCentral25.CustomerOrderLine>();
            int linenr = 1;
            foreach (OneListItem item in list.Items)
            {
                lines.Add(new LSCentral25.CustomerOrderLine()
                {
                    DocumentID = string.Empty,
                    LineNo = XMLHelper.LineNumberToNav(linenr++),
                    Number = item.ItemId,
                    VariantCode = item.VariantId,
                    UnitofMeasureCode = item.UnitOfMeasureId,
                    Quantity = item.Quantity,
                    LineType = ((int)LineType.Item).ToString()
                });
            }

            LSCentral25.RootCOQtyAvailabilityInV2 rootin = new LSCentral25.RootCOQtyAvailabilityInV2();
            rootin.CustomerOrderHeader = header.ToArray();
            rootin.CustomerOrderLine = lines.ToArray();
            return rootin;
        }

        public OrderCheck RootToOrderCheck(LSCentral25.RootSPGOrderCheck root)
        {
            OrderCheck order = new OrderCheck();
            if (root.SPGOrderCheckCOHeader == null || root.SPGOrderCheckCOHeader.Count() == 0)
                return order;

            order.StatusDate = root.SPGOrderCheckCOHeader[0].StatusDateTime;
            order.Status = root.SPGOrderCheckCOHeader[0].Status;

            if (root.SPGOrderCheckCOLine != null)
            {
                order.Lines = new List<OrderCheckLines>();
                foreach (LSCentral25.SPGOrderCheckCOLine line in root.SPGOrderCheckCOLine)
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
                foreach (LSCentral25.SPGOrderCheckCOPayment pay in root.SPGOrderCheckCOPayment)
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

        public LSCentral25.RootCustomerOrderCancel OrderCancelToRoot(string orderId, string storeId, string userId, List<OrderCancelLine> lines)
        {
            LSCentral25.RootCustomerOrderCancel root = new LSCentral25.RootCustomerOrderCancel();
            List<LSCentral25.CustomerOrderStatusLog> log = new List<LSCentral25.CustomerOrderStatusLog>()
            {
                new LSCentral25.CustomerOrderStatusLog()
                {
                    StoreNo = XMLHelper.GetString(storeId),
                    UserID = XMLHelper.GetString(userId),
                    ReceiptNo = orderId
                }
            };
            root.CustomerOrderStatusLog = log.ToArray();

            if (lines != null && lines.Count > 0)
            {
                List<LSCentral25.CustomerOrderCancelCOLine> clines = new List<LSCentral25.CustomerOrderCancelCOLine>();
                foreach (OrderCancelLine line in lines)
                {
                    clines.Add(new LSCentral25.CustomerOrderCancelCOLine()
                    {
                        DocumentID = orderId,
                        LineNo = XMLHelper.LineNumberToNav(line.LineNo),
                        Quantity = line.Quantity
                    });
                }
                root.CustomerOrderCancelCOLine = clines.ToArray();
            }
            return root;
        }
    }
}
