using System;
using System.Collections.Generic;
using System.Linq;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Printer;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Retail.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Pos.Items;
using LSRetail.Omni.Domain.DataModel.Pos.Items.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Pos.Payments;
using LSRetail.Omni.Domain.DataModel.Pos.Replication;
using LSRetail.Omni.Domain.DataModel.Pos.TenderTypes;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions.Discounts;

namespace LSOmni.DataAccess.BOConnection.PreCommon.Mapping
{
    public class TransactionMapping : BaseMapping
    {
        public TransactionMapping(Version lscVersion, bool json)
        {
            LSCVersion = lscVersion;
            IsJson = json;
        }

        public RetailTransaction MapFromRootToRetailTransaction(LSCentral.RootMobileTransaction root)
        {
            LSCentral.MobileTransaction header = root.MobileTransaction.FirstOrDefault();
            UnknownCurrency transactionCurrency = new UnknownCurrency(header.CurrencyCode);

            RetailTransaction transaction = new RetailTransaction()
            {
                Id = header.Id,
                Voided = (EntryStatus)header.EntryStatus == EntryStatus.Voided,
                ReceiptNumber = header.ReceiptNo,
                TransactionNumber = header.TransactionNo.ToString(),
                BeginDateTime = ConvertTo.SafeJsonDate(header.TransDate, IsJson),
                IncomeExpenseAmount = new Money(header.IncomeExpAmount, transactionCurrency),
                PointBalance = header.PointBalance,
                PointsUsedInOrder = header.PointsUsedInBasket,
                PointsRewarded = header.IssuedPoints,
                PointAmount = header.BasketInPoints,
                PointCashAmountNeeded = header.AmountRemaining,
                RefundedReceiptNo = header.RefundedReceiptNo,
                RefundedFromStoreNo = header.RefundedFromStoreNo,
                RefundedFromTerminalNo = header.RefundedFromPOSTermNo,
                RefundedFromTransNo = header.RefundedFromTransNo.ToString()
            };

            transaction.Id = transaction.Id.Replace("{", "").Replace("}", ""); //strip out the curly brackets

            transaction.Terminal = new UnknownTerminal(header.TerminalId);
            transaction.Terminal.Store = new UnknownStore(header.StoreId);
            transaction.Terminal.Staff = new UnknownStaff(header.StaffId);

            if (string.IsNullOrEmpty(header.CustomerId) == false)
            {
                transaction.Customer = new Customer(header.CustomerId);
                transaction.Customer.TaxGroup = header.CustDiscGroup;
            }

            if (string.IsNullOrEmpty(header.MemberCardNo) == false)
            {
                transaction.LoyaltyContact = new LSRetail.Omni.Domain.DataModel.Loyalty.Members.MemberContact(string.Empty);
                transaction.LoyaltyContact.Cards.Add(new Card(header.MemberCardNo));
            }

            if (header.ManualTotalDiscAmount > 0)
            {
                transaction.ManualDiscount = new Discount(transactionCurrency, header.ManualTotalDiscAmount, 0, DiscountEntryType.Amount);
            }
            else if (header.ManualTotalDiscPercent > 0)
            {
                transaction.ManualDiscount = new Discount(transactionCurrency, 0, header.ManualTotalDiscPercent, DiscountEntryType.Percentage);
            }
            else
            {
                transaction.ManualDiscount = new Discount(transactionCurrency, 0, 0, DiscountEntryType.Amount);
            }

            transaction.IncomeExpenseAmount = new Money(header.IncomeExpAmount, transactionCurrency);
            transaction.GrossAmount = new Money(header.GrossAmount, transactionCurrency);
            transaction.NetAmount = new Money(header.NetAmount, transactionCurrency);
            transaction.PaymentAmount = new Money(header.Payment, transactionCurrency);
            transaction.TotalDiscount = new Money(header.LineDiscount, transactionCurrency);
            transaction.HeadDiscount = new Money(header.TotalDiscount, transactionCurrency);
            transaction.TaxAmount = new Money(transaction.GrossAmount.Value - transaction.NetAmount.Value, transactionCurrency);

            //now loop through the discount lines
            List<DiscountLine> discounts = new List<DiscountLine>();
            if (root.MobileTransDiscountLine != null)
            {
                foreach (LSCentral.MobileTransDiscountLine mobileTransDisc in root.MobileTransDiscountLine)
                {
                    //DiscountType: Periodic Disc.,Customer,InfoCode,Total,Line,Promotion,Deal,Total Discount,Tender Type,Item Point,Line Discount,Member Point,Coupon
                    DiscountLine discount = new DiscountLine();
                    discount.Type = (DiscountType)mobileTransDisc.DiscountType;
                    discount.PeriodicType = (PeriodicDiscType)mobileTransDisc.PeriodicDiscType;
                    discount.Percentage = mobileTransDisc.DiscountPercent;
                    discount.Amount = new Money(mobileTransDisc.DiscountAmount, transactionCurrency);
                    discount.Description = mobileTransDisc.Description;
                    discount.No = mobileTransDisc.Id;
                    discount.OfferNo = mobileTransDisc.OfferNo;
                    discount.LineNumber = mobileTransDisc.LineNo;
                    discounts.Add(discount);
                }
            }

            transaction.IncomeExpenseLines = new List<IncomeExpenseLine>();

            //now loop through the lines
            transaction.SaleLines = new List<SaleLine>();
            if (root.MobileTransactionLine != null)
            {
                foreach (LSCentral.MobileTransactionLine mobileTransLine in root.MobileTransactionLine)
                {
                    MobileTransLine(mobileTransLine, ref transaction, discounts, root.MobileReceiptInfo);
                }
            }
            return transaction;
        }

        public OrderHosp MapFromRootToOrderHosp(LSCentral.RootMobileTransaction root)
        {
            LSCentral.MobileTransaction header = root.MobileTransaction.FirstOrDefault();
            UnknownCurrency transactionCurrency = new UnknownCurrency(header.CurrencyCode);

            OrderHosp order = new OrderHosp()
            {
                Id = header.Id,
                ReceiptNo = header.ReceiptNo,
                DocumentRegTime = ConvertTo.SafeJsonDate(header.TransDate, IsJson),
                TotalAmount = header.GrossAmount,
                TotalNetAmount = header.NetAmount,
                TotalDiscount = header.LineDiscount,
                Currency = header.CurrencyCode,
                CardId = header.MemberCardNo,
                StoreId = header.StoreId,
                SalesType = header.SalesType
            };

            //now loop through the lines
            order.OrderLines = new List<OrderHospLine>();
            order.OrderDiscountLines = new List<OrderDiscountLine>();

            if (root.MobileTransactionLine != null)
            {
                foreach (LSCentral.MobileTransactionLine line in root.MobileTransactionLine)
                {
                    LineType lineType = (LineType)line.LineType;
                    if (lineType == LineType.PerDiscount || lineType == LineType.Coupon)
                        continue;

                    OrderHospLine oline = new OrderHospLine()
                    {
                        Id = line.Id,
                        LineType = lineType,
                        LineNumber = line.LineNo,
                        ItemId = line.Number,
                        ItemDescription = line.ItemDescription,
                        VariantId = line.VariantCode,
                        VariantDescription = line.VariantDescription,
                        UomId = line.UomId,
                        NetPrice = line.NetPrice,
                        Quantity = line.Quantity,
                        DiscountAmount = line.DiscountAmount,
                        DiscountPercent = line.DiscountPercent,
                        NetAmount = line.NetAmount,
                        TaxAmount = line.TAXAmount,
                        Amount = line.NetAmount + line.TAXAmount,
                    };

                    if (line.ManualPrice > 0)
                    {
                        oline.Price = line.ManualPrice;
                        oline.PriceModified = true;
                    }
                    else
                    {
                        oline.Price = line.Price;
                        oline.PriceModified = false;
                    }

                    if (lineType == LineType.Item || lineType == LineType.IncomeExpense)
                    {
                        if (line.DealItem > 0)
                        {
                            //its a deal, get the deal lines from the mobileTransLine 
                            oline.IsADeal = true;
                            oline.SubLines.AddRange(DealSubLine(root.MobileTransactionSubLine, oline.LineNumber));
                        }

                        //get the text modifiers from the mobileTransLine 
                        List<OrderHospSubLine> tmlist = TextModifierSubLine(root.MobileTransactionSubLine, oline.LineNumber);
                        if (tmlist != null && tmlist.Count > 0)
                        {
                            oline.SubLines.AddRange(tmlist);
                        }

                        //get the item modifiers from the mobileTransLine 
                        List<OrderHospSubLine> modiferLineList = ModifierSubLine(root.MobileTransactionSubLine, oline.LineNumber);
                        if (modiferLineList != null && modiferLineList.Count > 0)
                        {
                            oline.SubLines.AddRange(modiferLineList);
                        }

                        oline.LineNumber = oline.LineNumber; // MPOS expects to get 1 not 10000
                        order.OrderLines.Add(oline);
                    }
                }
            }

            //now loop through the discount lines
            if (root.MobileTransDiscountLine != null)
            {
                foreach (LSCentral.MobileTransDiscountLine mobileTransDisc in root.MobileTransDiscountLine)
                {
                    //DiscountType: Periodic Disc.,Customer,InfoCode,Total,Line,Promotion,Deal,Total Discount,Tender Type,Item Point,Line Discount,Member Point,Coupon
                    OrderDiscountLine discount = new OrderDiscountLine()
                    {
                        DiscountType = (DiscountType)mobileTransDisc.DiscountType,
                        PeriodicDiscType = (PeriodicDiscType)mobileTransDisc.PeriodicDiscType,
                        DiscountPercent = mobileTransDisc.DiscountPercent,
                        DiscountAmount = mobileTransDisc.DiscountAmount,
                        Description = mobileTransDisc.Description,
                        No = mobileTransDisc.Id,
                        OfferNumber = mobileTransDisc.OfferNo,
                        LineNumber = mobileTransDisc.LineNo
                    };

                    if (discount.DiscountType == DiscountType.Coupon)
                    {
                        LSCentral.MobileTransactionLine tline = root.MobileTransactionLine.ToList().Find(l => l.LineType == 6);
                        if (tline != null)
                            discount.OfferNumber = tline.CouponCode;
                    }

                    order.OrderDiscountLines.Add(discount);
                }
            }
            return order;
        }

        public SalesEntry MapFromRootToRetailTransaction(LSCentral.RootGetTransaction root)
        {
            LSCentral.TransactionHeader header = root.TransactionHeader.FirstOrDefault();

            SalesEntry transaction = new SalesEntry()
            {
                Id = header.ReceiptNo,
                DocumentRegTime = ConvertTo.NavJoinDateAndTime(header.Date, header.Time),
                CardId = header.MemberCardNo,
                StoreId = header.StoreNo,
                TerminalId = header.POSTerminalNo,
                CustomerOrderNo = header.CustomerOrderNo,
                ClickAndCollectOrder = header.CustomerOrder,
                TotalAmount = header.GrossAmount * -1,
                TotalNetAmount = header.NetAmount * -1,
                TotalDiscount = header.DiscountAmount * -1,
                LineItemCount = (int)header.NoofItems,
                StoreCurrency = header.TransCurrency,
                IdType = DocumentIdType.Receipt,
                Status = SalesEntryStatus.Complete,
                Posted = true
            };

            //now loop through the discount lines
            transaction.DiscountLines = new List<SalesEntryDiscountLine>();
            if (root.TransDiscountEntry != null)
            {
                foreach (LSCentral.TransDiscountEntry mobileTransDisc in root.TransDiscountEntry)
                {
                    transaction.DiscountLines.Add(new SalesEntryDiscountLine()
                    {
                        OfferNumber = mobileTransDisc.OfferNo,
                        LineNumber = mobileTransDisc.LineNo,
                        DiscountAmount = mobileTransDisc.DiscountAmount,
                        PeriodicDiscType = (PeriodicDiscType)(Convert.ToInt32(mobileTransDisc.OfferType) + 1)
                    });
                }
            }

            //now loop through the lines
            transaction.Lines = new List<SalesEntryLine>();
            if (root.TransSalesEntry != null)
            {
                foreach (LSCentral.TransSalesEntry mobileTransLine in root.TransSalesEntry)
                {
                    transaction.Lines.Add(new SalesEntryLine()
                    {
                        ItemId = mobileTransLine.ItemNo,
                        UomId = mobileTransLine.UnitofMeasure,
                        VariantId = mobileTransLine.VariantCode,
                        Amount = (mobileTransLine.NetAmount + mobileTransLine.VATAmount) * -1,
                        NetAmount = mobileTransLine.NetAmount * -1,
                        TaxAmount = mobileTransLine.VATAmount * -1,
                        DiscountAmount = mobileTransLine.DiscountAmount,
                        Quantity = mobileTransLine.Quantity * -1,
                        Price = mobileTransLine.Price,
                        NetPrice = mobileTransLine.NetPrice,
                        LineNumber = mobileTransLine.LineNo,
                        StoreId = mobileTransLine.StoreNo
                    });
                }
            }
            return transaction;
        }

        public LSCentral.RootMobileTransaction MapFromRetailTransactionToRoot(RetailTransaction transaction, bool addDiscountLines)
        {
            LSCentral.RootMobileTransaction root = new LSCentral.RootMobileTransaction();

            //MobileTrans
            List<LSCentral.MobileTransaction> trans = new List<LSCentral.MobileTransaction>();
            trans.Add(MobileTrans(transaction));
            root.MobileTransaction = trans.ToArray();

            //MobileTransLines
            List<LSCentral.MobileTransactionLine> transLines = new List<LSCentral.MobileTransactionLine>();
            List<LSCentral.MobileReceiptInfo> receiptLines = new List<LSCentral.MobileReceiptInfo>();

            int maxTransLine = 1;

            foreach (SaleLine saleLine in transaction.SaleLines)
            {
                if (maxTransLine < saleLine.LineNumber)
                    maxTransLine = saleLine.LineNumber;

                transLines.Add(MobileTransLine(transaction.Id, saleLine, transaction.Terminal.Store.Currency, transaction.Terminal.Store.Id.ToUpper(), transaction.RefundedFromStoreNo, transaction.RefundedFromTerminalNo, transaction.RefundedFromTransNo));
            }

            if (addDiscountLines)
            {
                List<LSCentral.MobileTransDiscountLine> discLines = new List<LSCentral.MobileTransDiscountLine>();
                foreach (SaleLine posLine in transaction.SaleLines)
                {
                    if (posLine.Discounts.Count == 0)
                        continue;

                    int discNo = 1;
                    foreach (DiscountLine disc in posLine.Discounts)
                    {
                        discLines.Add(new LSCentral.MobileTransDiscountLine()
                        {
                            Id = transaction.Id,
                            LineNo = XMLHelper.LineNumberToNav(posLine.LineNumber),
                            No = discNo++,
                            DiscountType = (int)disc.Type,
                            OfferNo = XMLHelper.GetString(disc.OfferNo),
                            Description = XMLHelper.GetString(disc.Description),
                            PeriodicDiscType = (int)disc.PeriodicType,
                            DiscountAmount = disc.Amount.Value,
                            DiscountPercent = disc.Percentage,
                            PeriodicDiscGroup = string.Empty
                        });
                    }
                }
                root.MobileTransDiscountLine = discLines.ToArray();
            }

            if (transaction.LoyaltyContact != null)
            {
                if (transaction.LoyaltyContact.PublishedOffers != null)
                {
                    foreach (PublishedOffer offer in transaction.LoyaltyContact.PublishedOffers.Where(x => x.Selected))
                    {
                        maxTransLine++;
                        transLines.Add(MobileTransLine(transaction.Id, offer, maxTransLine, transaction.Terminal.Store.Id.ToUpper()));
                    }
                }
            }

            int tenderLineNumber = maxTransLine + 1;
            foreach (PaymentLine tenderLine in transaction.TenderLines)
            {
                transLines.Add(TransPaymentLine(transaction.Id, tenderLine, transaction.Terminal.Store.Currency.Id, tenderLineNumber));
                foreach (ReceiptInfo receiptInfo in tenderLine.Payment.ReceiptInfo)
                {
                    receiptLines.Add(ReceiptInfoLine(transaction.Id, receiptInfo, tenderLineNumber, transaction.TransactionNumber));
                }
                tenderLineNumber++;
            }

            root.MobileReceiptInfo = receiptLines.ToArray();
            root.MobileTransactionLine = transLines.ToArray();
            return root;
        }

        public LSCentral.RootHospTransaction MapFromOrderToRoot(OrderHosp order)
        {
            LSCentral.RootHospTransaction root = new LSCentral.RootHospTransaction();

            //MobileTrans
            List<LSCentral.HospTransaction> trans = new List<LSCentral.HospTransaction>();
            trans.Add(new LSCentral.HospTransaction()
            {
                Id = order.Id,
                StoreId = order.StoreId,
                TransactionType = 2,
                EntryStatus = (int)EntryStatus.Normal,
                TransDate = DateTime.Now,
                SaleIsReturnSale = false,
                MemberCardNo = XMLHelper.GetString(order.CardId),
                CurrencyFactor = 1,
                SalesType = order.SalesType,

                TerminalId = string.Empty,
                StaffId = string.Empty,
                ReceiptNo = string.Empty,
                CurrencyCode = string.Empty,
                BusinessTAXCode = string.Empty,
                PriceGroupCode = string.Empty,
                CustomerId = string.Empty,
                CustDiscGroup = string.Empty,
                MemberPriceGroupCode = string.Empty,
                DiningTblDescription = string.Empty,
                RefundedFromStoreNo = string.Empty,
                RefundedFromPOSTermNo = string.Empty,
                RefundedReceiptNo = string.Empty,
                GenBusPostingGroup = string.Empty,
                VATBusPostingGroup = string.Empty
            });
            root.HospTransaction = trans.ToArray();

            /*
            if (LSCVersion >= new Version("21.1"))
            {
                List<LSCentral.FABOrder> faborder = new List<LSCentral.FABOrder>();
                LSCentral.FABOrder fab = new LSCentral.FABOrder()
                {
                    ClientAddress = XMLHelper.GetString(order.Address?.Address1),
                    ClientPhoneNo = XMLHelper.GetString(order.Address?.PhoneNumber),
                    ClientName = XMLHelper.GetString(order.Name),
                    ClientEmail = XMLHelper.GetString(order.Email),
                    ExternalID = XMLHelper.GetString(order.ExternalId),
                    PickupDate = ConvertTo.NavGetDate(order.OrderDate, true),
                    PickupTime = ConvertTo.NavGetTime(order.OrderDate, true),
                    PickupDateTime = order.PickupTime,
                    SalesType = order.SalesType,
                    StoreNo = order.RestaurantNo,

                    ContactCommentIcon = string.Empty,
                    CreatedOnPOSTermnial = string.Empty,
                    QueueCounter = string.Empty,
                    KitchenStatus = string.Empty,
                    OrderNo = string.Empty,
                    POSTerminalNo = string.Empty,
                    StaffID = string.Empty,
                    OrderStatus = string.Empty
                };
                faborder.Add(fab);
                root.FABOrder = faborder.ToArray();
            }
            */

            List<LSCentral.WebDeliveryOrder> delivery = new List<LSCentral.WebDeliveryOrder>();
            LSCentral.WebDeliveryOrder devord = new LSCentral.WebDeliveryOrder()
            {
                StreetName = XMLHelper.GetString(order.Address?.Address1),
                StreetNo = XMLHelper.GetString(order.Address?.HouseNo),
                Address2 = XMLHelper.GetString(order.Address?.Address2),
                PostCode = XMLHelper.GetString(order.Address?.PostCode),
                City = XMLHelper.GetString(order.Address?.City),
                PhoneNo = XMLHelper.GetString(order.Address?.PhoneNumber),
                Name = XMLHelper.GetString(order.Name),
                BillToName = XMLHelper.GetString(order.BillToName),
                OrderTypeOption = (int)order.DeliveryType,
                OrderDate = ConvertTo.NavGetDate(order.OrderDate, true),
                ContactPickupTime = order.PickupTime,
                RestaurantNo = order.RestaurantNo,
                Email = XMLHelper.GetString(order.Email),
                Directions = XMLHelper.GetString(order.Directions),
                SalesType = XMLHelper.GetString(order.SalesType),
                AddressType = "0",
                TotalAmount = order.TotalAmount,
                Comment = XMLHelper.GetString(order.Comment),

                TenderType = string.Empty,
                CompanyNo = string.Empty,
                GridCode = string.Empty,
                OrderNo = string.Empty,
                PreOrder = string.Empty
            };

            if (LSCVersion >= new Version("18.2"))
            {
                devord.PreOrderPrintDateTime = ConvertTo.NavGetDate(DateTime.MinValue, true);
            }

            delivery.Add(devord);
            root.WebDeliveryOrder = delivery.ToArray();

            //MobileTransLines
            List<LSCentral.HospTransactionLine> transLines = new List<LSCentral.HospTransactionLine>();
            List<LSCentral.HospTransactionSubLine> subLines = new List<LSCentral.HospTransactionSubLine>();
            List<LSCentral.HospReceiptInfo> receiptLines = new List<LSCentral.HospReceiptInfo>();
            List<LSCentral.HospTransDiscountLine> discLines = new List<LSCentral.HospTransDiscountLine>();

            // find highest lineNo count in sub line, if user has set some
            int LineCounter = 1;
            foreach (OrderHospLine l in order.OrderLines)
            {
                if (LineCounter < l.LineNumber)
                    LineCounter = l.LineNumber;

                if (l.SubLines != null)
                {
                    foreach (OrderHospSubLine s in l.SubLines)
                    {
                        if (LineCounter < s.LineNumber)
                            LineCounter = s.LineNumber;
                    }
                }
            }

            int discNo = 1;
            foreach (OrderHospLine posLine in order.OrderLines)
            {
                if (posLine.LineNumber == 0)
                    posLine.LineNumber = ++LineCounter;

                transLines.Add(MobileTransLine(order.Id, posLine, order.StoreId));
                subLines.AddRange(MobileTransSubLine(order.Id, posLine, ref LineCounter));
            }

            if (order.OrderDiscountLines != null)
            {
                foreach (OrderDiscountLine dline in order.OrderDiscountLines)
                {
                    discLines.Add(new LSCentral.HospTransDiscountLine()
                    {
                        Id = XMLHelper.GetString(dline.Id),
                        LineNo = XMLHelper.LineNumberToNav(dline.LineNumber),
                        No = discNo++,
                        DiscountType = (int)dline.DiscountType,
                        OfferNo = XMLHelper.GetString(dline.OfferNumber),
                        Description = XMLHelper.GetString(dline.Description),
                        PeriodicDiscType = (int)dline.PeriodicDiscType,
                        DiscountAmount = dline.DiscountAmount,
                        DiscountPercent = dline.DiscountPercent,
                        PeriodicDiscGroup = string.Empty
                    });
                }
            }

            if (order.OrderPayments != null)
            {
                foreach (OrderPayment tenderLine in order.OrderPayments)
                {
                    transLines.Add(TransPaymentLine(order.Id, tenderLine, order.StoreId, ++LineCounter));
                }
            }

            root.HospTransDiscountLine = discLines.ToArray();
            root.HospReceiptInfo = receiptLines.ToArray();
            root.HospTransactionLine = transLines.ToArray();
            root.HospTransactionSubLine = subLines.ToArray();
            return root;
        }

        public LSCentral.RootMobileTransaction MapFromOrderToRoot(OneList request)
        {
            LSCentral.RootMobileTransaction root = new LSCentral.RootMobileTransaction();

            //MobileTrans
            List<LSCentral.MobileTransaction> trans = new List<LSCentral.MobileTransaction>();
            trans.Add(new LSCentral.MobileTransaction()
            {
                Id = request.Id,
                StoreId = request.StoreId,
                TransactionType = 2,
                EntryStatus = (int)EntryStatus.Normal,
                TransDate = DateTime.Now,
                SaleIsReturnSale = false,
                MemberCardNo = XMLHelper.GetString(request.CardId),
                CurrencyFactor = 1,
                SalesType = XMLHelper.GetString(request.SalesType),

                TerminalId = string.Empty,
                StaffId = string.Empty,
                ReceiptNo = string.Empty,
                CurrencyCode = string.Empty,
                BusinessTAXCode = string.Empty,
                PriceGroupCode = string.Empty,
                CustomerId = string.Empty,
                CustDiscGroup = string.Empty,
                MemberPriceGroupCode = string.Empty,
                DiningTblDescription = string.Empty,
                RefundedFromStoreNo = string.Empty,
                RefundedFromPOSTermNo = string.Empty,
                RefundedReceiptNo = string.Empty,
                VATBusPostingGroup = string.Empty,
                GenBusPostingGroup = string.Empty
            });
            root.MobileTransaction = trans.ToArray();

            //MobileTransLines
            List<LSCentral.MobileTransactionLine> transLines = new List<LSCentral.MobileTransactionLine>();
            List<LSCentral.MobileTransactionSubLine> subLines = new List<LSCentral.MobileTransactionSubLine>();
            List<LSCentral.MobileReceiptInfo> receiptLines = new List<LSCentral.MobileReceiptInfo>();
            List<LSCentral.MobileTransDiscountLine> discLines = new List<LSCentral.MobileTransDiscountLine>();

            foreach (OneListItem line in request.Items)
            {
                transLines.Add(MobileTransLine(request.Id, line, request.StoreId));
                subLines.AddRange(MobileTransSubLine(request.Id, line));
            }

            if (request.PublishedOffers != null)
            {
                foreach (OneListPublishedOffer offer in request.PublishedOffers)
                {
                    transLines.Add(MobileTransLine(request.Id, offer, offer.LineNumber, request.StoreId));
                }
            }

            root.MobileTransDiscountLine = discLines.ToArray();
            root.MobileReceiptInfo = receiptLines.ToArray();
            root.MobileTransactionLine = transLines.ToArray();
            root.MobileTransactionSubLine = subLines.ToArray();
            return root;
        }

        public LSCentral.RootMobileTransaction MapFromOrderToRoot(Order order)
        {
            LSCentral.RootMobileTransaction root = new LSCentral.RootMobileTransaction();

            if (string.IsNullOrEmpty(order.Id))
                order.Id = GuidHelper.NewGuidString();

            //MobileTrans
            List<LSCentral.MobileTransaction> trans = new List<LSCentral.MobileTransaction>();
            trans.Add(new LSCentral.MobileTransaction()
            {
                Id = order.Id,
                StoreId = order.StoreId,
                TransactionType = 2,
                EntryStatus = (int)EntryStatus.Normal,
                TransDate = DateTime.Now,
                SaleIsReturnSale = false,
                MemberCardNo = XMLHelper.GetString(order.CardId),
                CurrencyFactor = 1,

                TerminalId = string.Empty,
                StaffId = string.Empty,
                ReceiptNo = string.Empty,
                CurrencyCode = string.Empty,
                BusinessTAXCode = string.Empty,
                PriceGroupCode = string.Empty,
                CustomerId = string.Empty,
                CustDiscGroup = string.Empty,
                MemberPriceGroupCode = string.Empty,
                DiningTblDescription = string.Empty,
                SalesType = string.Empty,
                RefundedFromStoreNo = string.Empty,
                RefundedFromPOSTermNo = string.Empty,
                RefundedReceiptNo = string.Empty
            });
            root.MobileTransaction = trans.ToArray();

            //MobileTransLines
            List<LSCentral.MobileTransactionLine> transLines = new List<LSCentral.MobileTransactionLine>();
            List<LSCentral.MobileReceiptInfo> receiptLines = new List<LSCentral.MobileReceiptInfo>();

            int lineno = 1;
            foreach (OrderLine line in order.OrderLines)
            {
                line.LineNumber = lineno++;
                transLines.Add(MobileTransLine(order.Id, line, order.StoreId));
            }

            root.MobileReceiptInfo = receiptLines.ToArray();
            root.MobileTransactionLine = transLines.ToArray();
            return root;
        }

        public List<SalesEntry> MapFromRootToSalesEntries(LSCentral.RootGetMemberSalesHistory root)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            if (root.MemberSalesEntry == null)
                return list;

            foreach (LSCentral.MemberSalesEntry trans in root.MemberSalesEntry)
            {
                list.Add(new SalesEntry()
                {
                    Id = trans.DocumentNo,
                    TerminalId = trans.POSTerminalNo,
                    DocumentRegTime = ConvertTo.SafeJsonDate(trans.Date, IsJson),
                    TotalAmount = trans.GrossAmount,
                    LineItemCount = (int)trans.Quantity,
                    StoreId = trans.StoreNo,
                    StoreName = trans.StoreName,
                    CardId = trans.MemberCardNo,
                    IdType = DocumentIdType.Receipt
                });
            }
            return list;
        }

        public RetailTransaction MapFromRootToLoyTransaction(LSCentral.RootGetTransaction root)
        {
            LSCentral.TransactionHeader header = root.TransactionHeader.FirstOrDefault();
            UnknownCurrency transactionCurrency = new UnknownCurrency(header.TransCurrency);

            RetailTransaction transaction = new RetailTransaction()
            {
                Id = header.TransactionNo.ToString(),
                GrossAmount = new Money(header.GrossAmount, transactionCurrency),
                TotalDiscount = new Money(header.DiscountAmount, transactionCurrency),
                NetAmount = new Money(header.NetAmount, transactionCurrency),
                ReceiptNumber = header.ReceiptNo,
                Terminal = new Terminal(header.POSTerminalNo),
                RefundedReceiptNo = header.RefundReceiptNo
            };

            if (root.TransSalesEntry != null)
            {
                foreach (LSCentral.TransSalesEntry entry in root.TransSalesEntry)
                {
                    transaction.SaleLines.Add(new SaleLine()
                    {
                        Item = new RetailItem(entry.ItemNo),
                        Quantity = entry.Quantity,
                        ReturnQuantity = entry.RefundQty,
                        NetAmount = new Money(entry.NetAmount, transactionCurrency),
                        TaxAmount = new Money(entry.VATAmount, transactionCurrency),
                        UnitPrice = new Money(entry.Price, transactionCurrency),
                    });
                }
            }

            if (root.TransPaymentEntry != null)
            {
                int lineno = 1;
                foreach (LSCentral.TransPaymentEntry pay in root.TransPaymentEntry)
                {
                    Payment payment = new Payment()
                    {
                        Amount = new Money(pay.AmountTendered, transactionCurrency)
                    };
                    transaction.TenderLines.Add(new PaymentLine(lineno++, payment));
                }
            }
            return transaction;
        }

        public ItemPriceCheckResponse MapFromRootToPriceCheck(LSCentral.RootMobileTransaction root)
        {
            LSCentral.MobileTransaction header = root.MobileTransaction.FirstOrDefault();

            ItemPriceCheckResponse rs = new ItemPriceCheckResponse()
            {
                Id = header.Id,
                StoreId = header.StoreId,
                TerminalId = header.TerminalId,
                StaffId = header.StaffId,
                CurrencyCode = header.CurrencyCode,
                CurrencyFactor = Convert.ToInt32(header.CurrencyFactor),
                CustomerId = header.CustomerId,
                CustDiscGroup = header.CustDiscGroup,
                MemberCardNo = header.MemberCardNo,
                MemberPriceGroupCode = header.MemberPriceGroupCode
            };

            if (root.MobileTransactionLine != null)
            {
                foreach (LSCentral.MobileTransactionLine line in root.MobileTransactionLine)
                {
                    rs.ItemPriceCheckLineResponses.Add(new ItemPriceCheckLineResponse()
                    {
                        LineNumber = line.LineNo,
                        LineType = line.LineType,
                        ItemId = line.Number,
                        BarcodeId = line.Barcode,
                        CurrencyCode = line.CurrencyCode,
                        CurrencyFactor = Convert.ToInt32(line.CurrencyFactor),
                        VariantId = line.VariantCode,
                        Uom = line.UomId,
                        NetPrice = line.NetPrice,
                        Price = line.Price,
                        Quantity = line.Quantity,
                        NetAmount = line.NetAmount
                    });
                }
            }
            return rs;
        }

        public LSCentral.RootMobileTransaction GetShiftRoot(ShiftRequest shiftRequest, string tendType)
        {
            TransactionType transType = TransactionType.Sales;
            if (shiftRequest.ShiftAction == ShiftAction.FloatEntry)
                transType = TransactionType.FloatEntry;
            else if (shiftRequest.ShiftAction == ShiftAction.TenderRemoval)
                transType = TransactionType.RemoveTender;

            LSCentral.RootMobileTransaction root = new LSCentral.RootMobileTransaction();
            List<LSCentral.MobileTransaction> header = new List<LSCentral.MobileTransaction>();
            header.Add(new LSCentral.MobileTransaction()
            {
                TerminalId = shiftRequest.TerminalNo,
                StaffId = shiftRequest.StaffId,
                StoreId = shiftRequest.StoreNo,
                TransactionType = (int)transType,

                ReceiptNo = string.Empty,
                CurrencyCode = string.Empty,
                TransDate = DateTime.Now,
                VATBusPostingGroup = string.Empty,
                CustDiscGroup = string.Empty,
                CustomerId = string.Empty,
                BusinessTAXCode = string.Empty,
                GenBusPostingGroup = string.Empty,
                MemberPriceGroupCode = string.Empty,
                DiningTblDescription = string.Empty,
                MemberCardNo = string.Empty,
                PriceGroupCode = string.Empty,
                RefundedReceiptNo = string.Empty,
                RefundedFromPOSTermNo = string.Empty,
                RefundedFromStoreNo = string.Empty,
                SalesType = string.Empty
            });
            root.MobileTransaction = header.ToArray();

            List<LSCentral.MobileTransactionLine> payline = new List<LSCentral.MobileTransactionLine>();
            payline.Add(new LSCentral.MobileTransactionLine()
            {
                LineType = 1,
                Number = tendType,
                NetAmount = shiftRequest.Amount,
                CurrencyFactor = 1,
                StaffId = shiftRequest.StaffId,
                StoreId = shiftRequest.StoreNo,
                TerminalId = shiftRequest.TerminalNo,

                EFTAuthCode = string.Empty,
                EFTCardName = string.Empty,
                EFTCardNumber = string.Empty,
                EFTMessage = string.Empty,
                EFTTransactionNo = string.Empty,
                Barcode = string.Empty,
                GenBusPostingGroup = string.Empty,
                GenProdPostingGroup = string.Empty,
                TAXBusinessCode = string.Empty,
                TAXProductCode = string.Empty,
                TenderDescription = string.Empty,
                CouponCode = string.Empty,
                CurrencyCode = string.Empty,
                RestMenuTypeCode = string.Empty,
                RetailImageID = string.Empty,
                CardOrCustNo = string.Empty,
                LineKitchenStatusCode = string.Empty,
                ItemDescription = string.Empty,
                PriceGroupCode = string.Empty,
                VariantCode = string.Empty,
                VariantDescription = string.Empty,
                UomDescription = string.Empty,
                UomId = string.Empty,
                SalesType = string.Empty,
                OrigTransPos = string.Empty,
                OrigTransStore = string.Empty
            });
            root.MobileTransactionLine = payline.ToArray();
            return root;
        }

        public List<EodReportRepsonse> EODReportResponse(LSCentral.RootMobilePosZReport root)
        {
            if (root.PosPrintBuffer == null)
                return new List<EodReportRepsonse>();

            List<EodReportRepsonse> list = new List<EodReportRepsonse>();
            foreach (LSCentral.PosPrintBuffer line in root.PosPrintBuffer)
            {
                list.Add(new EodReportRepsonse()
                {
                    TransactionNo = line.TransactionNo,
                    BufferIndex = line.BufferIndex,
                    StationNo = line.StationNo,
                    PageNo = line.PageNo,
                    PrintedLineNo = line.PrintedLineNo,
                    LineType = Convert.ToInt32(line.LineType),
                    HostId = line.HostID,
                    ProfileId = line.ProfileID,
                    TransactionType = line.TransactionType,
                    Text = line.Text,
                    Width = line.Width,
                    Height = line.Height,
                    BCType = line.BCType,
                    BCPos = line.BCPos,
                    SetBackPrinting = line.SetBackPrinting
                });
            }
            return list;
        }

        public List<SuspendedTransaction> RootToTransSuspendList(LSCentral.RootGetPosTransSuspList root)
        {
            List<SuspendedTransaction> tranactionList = new List<SuspendedTransaction>();
            if (root.POSTransaction == null)
                return tranactionList;

            foreach (LSCentral.POSTransaction trs in root.POSTransaction)
            {
                //Only want transaction of type  SALE (=2).  skip all others
                if (trs.TransactionType != "2")
                    continue;

                //Only want transaction with entry status of  Suspended (=1).  skip all others
                if (trs.EntryStatus != "1")
                    continue;

                tranactionList.Add(new SuspendedTransaction()
                {
                    StoreId = trs.StoreNo,
                    Terminal = trs.POSTerminalNo,
                    Staff = trs.StaffID,
                    ReceiptNumber = trs.ReceiptNo,
                    Id = trs.ReceiptNo,
                    Date = ConvertTo.SafeJsonDate(ConvertTo.NavJoinDateAndTime(trs.TransDate, trs.TransTime), IsJson),
                    CurrencyCode = trs.TransCurrencyCode,
                    SearchKey = trs.SearchKey,
                    CustomerId = trs.CustomerNo
                });
            }
            return tranactionList;
        }

        #region Private

        private LSCentral.MobileTransaction MobileTrans(RetailTransaction transaction)
        {
            DateTime transDate = DateTime.Now;
            //if transdate sent from mobile device is within 30 days then use that one 
            if (transaction.BeginDateTime.HasValue && transaction.BeginDateTime > DateTime.Now.AddDays(-30) && transaction.BeginDateTime < DateTime.Now.AddDays(30))
                transDate = transaction.BeginDateTime.Value; //in xml 2013-04-24T17:02:20.197Z

            string transNumber = (string.IsNullOrWhiteSpace(transaction.TransactionNumber) ? "0" : transaction.TransactionNumber);
            string receiptNumber = transaction.ReceiptNumber ?? string.Empty;

            string cardId = string.Empty;
            if (transaction.LoyaltyContact != null)
            {
                cardId = transaction.LoyaltyContact.Cards.FirstOrDefault().Id;
            }

            decimal manualTotalDiscPercent = 0m;
            decimal manualTotalDisAmount = 0m;

            if (transaction.ManualDiscount != null && transaction.ManualDiscount.Amount.Value != 0)
            {
                manualTotalDisAmount = transaction.ManualDiscount.Amount.Value;
                manualTotalDiscPercent = 0.0M;
            }
            else if (transaction.ManualDiscount != null && transaction.ManualDiscount.Percentage > 0)
            {
                manualTotalDisAmount = 0.0M;
                manualTotalDiscPercent = transaction.ManualDiscount.Percentage;
            }

            return new LSCentral.MobileTransaction()
            {
                Id = transaction.Id,
                StoreId = transaction.Terminal.Store.Id.ToUpper(),
                TerminalId = transaction.Terminal.Id.ToUpper(),
                StaffId = transaction.Terminal.Staff.Id.ToUpper(),
                TransactionType = 2,
                EntryStatus = (transaction.Voided) ? (int)EntryStatus.Voided : (int)EntryStatus.Normal,
                ReceiptNo = receiptNumber,
                TransactionNo = ConvertTo.SafeInt(transNumber),
                TransDate = transDate,
                CurrencyCode = transaction.Terminal.Store.Currency.Id,
                CurrencyFactor = 1,
                BusinessTAXCode = transaction.Terminal.Store.TaxGroupId ?? string.Empty,
                CustomerId = (transaction.Customer == null) ? string.Empty : transaction.Customer.Id,
                CustDiscGroup = (transaction.Customer == null) ? string.Empty : transaction.Customer.TaxGroup,
                MemberCardNo = cardId,
                ManualTotalDiscAmount = manualTotalDisAmount,
                ManualTotalDiscPercent = manualTotalDiscPercent,
                NetAmount = transaction.NetAmount.Value,
                GrossAmount = transaction.GrossAmount.Value,
                LineDiscount = transaction.TotalDiscount.Value,
                Payment = transaction.PaymentAmount.Value,
                RefundedFromStoreNo = transaction.RefundedFromStoreNo ?? string.Empty,
                RefundedFromPOSTermNo = transaction.RefundedFromTerminalNo ?? string.Empty,
                RefundedFromTransNo = ConvertTo.SafeInt(transaction.RefundedFromTransNo),
                RefundedReceiptNo = transaction.RefundedReceiptNo ?? string.Empty,
                SaleIsReturnSale = transaction.IsRefundByReceiptTransaction,

                PriceGroupCode = string.Empty,
                MemberPriceGroupCode = string.Empty,
                DiningTblDescription = string.Empty,
                SalesType = string.Empty
            };
        }

        private LSCentral.MobileTransactionLine MobileTransLine(string id, BaseLine line, Currency currency, string storeId, string orgStore, string orgTerminal, string orgTransNo)
        {
            decimal quantity = 0m;
            int externalId = 0;

            string itemId = string.Empty;
            string barcode = string.Empty;
            string variantId = string.Empty;
            string uomId = string.Empty;

            int lineOrgNumber = 0;

            decimal netAmount = 0m;
            decimal netPrice = 0m;
            decimal price = 0m;
            decimal taxAmount = 0m;
            decimal manualPrice = 0m;

            string taxProductCode = string.Empty;

            decimal discountAmount = 0m;
            decimal discountPercent = 0m;

            decimal manualDiscountAmount = 0m;
            decimal manualDiscountPercent = 0m;

            bool fromRecommend = false;

            LineType lineType = LineType.Unknown;
            EntryStatus entryStatus = EntryStatus.Normal;

            if (line is SaleLine)
            {
                lineType = LineType.Item;

                SaleLine saleLine = (SaleLine)line;

                itemId = saleLine.Item.Id;
                netAmount = saleLine.NetAmount.Value;
                netPrice = saleLine.UnitPrice.Value;
                price = saleLine.UnitPriceWithTax.Value;
                taxAmount = saleLine.TaxAmount.Value;
                fromRecommend = saleLine.FromRecommendation;
                discountAmount = saleLine.LineDiscount.Amount.Value;
                discountPercent = saleLine.LineDiscount.Percentage;

                lineOrgNumber = XMLHelper.LineNumberToNav(saleLine.OriginalTransactionLineNo);
                entryStatus = (saleLine.Voided) ? EntryStatus.Voided : EntryStatus.Normal;
                quantity = (saleLine.IsReturnLine) ? -1 * Math.Abs(saleLine.Quantity) : Math.Abs(saleLine.Quantity);

                if (saleLine.PriceOverridden)
                {
                    manualPrice = saleLine.UnitPriceWithTax.Value;
                }

                if (saleLine.Item is RetailItem retailItem)
                {
                    barcode = retailItem.Barcode?.Id;
                    variantId = retailItem.SelectedVariant?.Id;
                    uomId = retailItem.UnitOfMeasure?.Id;
                    taxProductCode = retailItem.TaxGroupId;
                }

                if (saleLine.ManualDiscount.Amount.Value != 0)
                {
                    manualDiscountAmount = saleLine.ManualDiscount.Amount.Value;
                    manualDiscountPercent = 0.0M;
                }
                else if (saleLine.ManualDiscount.Percentage > 0)
                {
                    manualDiscountAmount = 0.0M;
                    manualDiscountPercent = saleLine.ManualDiscount.Percentage;
                }
            }
            else if (line is IncomeExpenseLine)
            {
                lineType = LineType.IncomeExpense;

                IncomeExpenseLine incomeExpenseLine = (IncomeExpenseLine)line;
                itemId = incomeExpenseLine.AccountNumber;
                netAmount = incomeExpenseLine.Amount.Value;
                entryStatus = (incomeExpenseLine.Voided) ? EntryStatus.Voided : EntryStatus.Normal;
                externalId = 0;
            }

            return new LSCentral.MobileTransactionLine()
            {
                Id = id,
                LineNo = XMLHelper.LineNumberToNav(line.LineNumber),
                EntryStatus = (int)entryStatus,
                LineType = (int)lineType,
                Number = itemId,
                Barcode = barcode ?? string.Empty,
                CurrencyCode = currency.Id,
                CurrencyFactor = 1,
                VariantCode = XMLHelper.GetString(variantId),
                UomId = XMLHelper.GetString(uomId),
                NetPrice = netPrice,
                Price = price,
                Quantity = quantity,
                DiscountAmount = discountAmount,
                DiscountPercent = discountPercent,
                NetAmount = netAmount,
                TAXAmount = taxAmount,
                TAXProductCode = XMLHelper.GetString(taxProductCode),
                ManualPrice = manualPrice,
                ManualDiscountAmount = manualDiscountAmount,
                ManualDiscountPercent = manualDiscountPercent,
                ExternalId = externalId,
                StoreId = storeId,
                RecommendedItem = fromRecommend,
                OrigTransLineNo = lineOrgNumber,
                OrigTransNo = ConvertTo.SafeInt(orgTransNo),
                OrigTransPos = orgTerminal ?? string.Empty,
                OrigTransStore = orgStore ?? string.Empty,

                CardOrCustNo = string.Empty,
                TAXBusinessCode = string.Empty,
                EFTCardNumber = string.Empty,
                EFTCardName = string.Empty,
                EFTAuthCode = string.Empty,
                EFTMessage = string.Empty,
                StaffId = string.Empty,
                PriceGroupCode = string.Empty,
                CouponCode = string.Empty,
                TerminalId = string.Empty,
                EFTTransactionNo = string.Empty,
                SalesType = string.Empty,
                ItemDescription = string.Empty,
                LineKitchenStatusCode = string.Empty,
                RestMenuTypeCode = string.Empty,
                TenderDescription = string.Empty,
                UomDescription = string.Empty,
                VariantDescription = string.Empty,
                RetailImageID = string.Empty
            };
        }

        private LSCentral.MobileTransactionLine MobileTransLine(string id, OrderLine line, string storeId)
        {
            return new LSCentral.MobileTransactionLine()
            {
                Id = id,
                LineNo = XMLHelper.LineNumberToNav(line.LineNumber),
                EntryStatus = (int)EntryStatus.Normal,
                LineType = (int)LineType.Item,
                Number = line.ItemId,
                StoreId = storeId,
                CurrencyFactor = 1,
                VariantCode = XMLHelper.GetString(line.VariantId),
                UomId = XMLHelper.GetString(line.UomId),
                NetPrice = line.NetPrice,
                Price = line.Price,
                Quantity = line.Quantity,
                DiscountAmount = line.DiscountAmount,
                DiscountPercent = line.DiscountPercent,
                NetAmount = line.NetAmount,
                TAXAmount = line.TaxAmount,
                RecommendedItem = false,

                CurrencyCode = string.Empty,
                Barcode = string.Empty,
                TAXProductCode = string.Empty,
                TAXBusinessCode = string.Empty,
                CardOrCustNo = string.Empty,
                EFTCardNumber = string.Empty,
                EFTCardName = string.Empty,
                EFTAuthCode = string.Empty,
                EFTMessage = string.Empty,
                StaffId = string.Empty,
                PriceGroupCode = string.Empty,
                CouponCode = string.Empty,
                TerminalId = string.Empty,
                EFTTransactionNo = string.Empty,
                SalesType = string.Empty,
                ItemDescription = string.Empty,
                LineKitchenStatusCode = string.Empty,
                RestMenuTypeCode = string.Empty,
                TenderDescription = string.Empty,
                UomDescription = string.Empty,
                VariantDescription = string.Empty,
                OrigTransPos = string.Empty,
                OrigTransStore = string.Empty,
                RetailImageID = string.Empty
            };
        }

        private LSCentral.MobileTransactionLine MobileTransLine(string id, OneListItem line, string storeId)
        {
            return new LSCentral.MobileTransactionLine()
            {
                Id = id,
                LineNo = line.LineNumber,
                EntryStatus = (int)EntryStatus.Normal,
                LineType = (int)LineType.Item,
                Number = line.ItemId,
                StoreId = storeId,
                CurrencyFactor = 1,
                VariantCode = XMLHelper.GetString(line.VariantId),
                UomId = XMLHelper.GetString(line.UnitOfMeasureId),
                NetPrice = line.NetPrice,
                Price = (line.PriceModified) ? 0 : line.Price,
                Quantity = line.Quantity,
                DiscountAmount = line.DiscountAmount,
                DiscountPercent = line.DiscountPercent,
                NetAmount = line.NetAmount,
                TAXAmount = line.TaxAmount,
                ManualPrice = (line.PriceModified) ? line.Price : 0,
                RecommendedItem = false,
                DealItem = (line.IsADeal ? 1 : 0),  //0=Item line, 1=Deal line

                CurrencyCode = string.Empty,
                Barcode = string.Empty,
                TAXProductCode = string.Empty,
                TAXBusinessCode = string.Empty,
                CardOrCustNo = string.Empty,
                EFTCardNumber = string.Empty,
                EFTCardName = string.Empty,
                EFTAuthCode = string.Empty,
                EFTMessage = string.Empty,
                StaffId = string.Empty,
                PriceGroupCode = string.Empty,
                CouponCode = string.Empty,
                TerminalId = string.Empty,
                EFTTransactionNo = string.Empty,
                SalesType = string.Empty,
                ItemDescription = string.Empty,
                LineKitchenStatusCode = string.Empty,
                RestMenuTypeCode = string.Empty,
                TenderDescription = string.Empty,
                UomDescription = string.Empty,
                VariantDescription = string.Empty,
                OrigTransPos = string.Empty,
                OrigTransStore = string.Empty,
                GenBusPostingGroup = string.Empty,
                GenProdPostingGroup = string.Empty,
                VatBusPostingGroup = string.Empty,
                VatProdPostingGroup = string.Empty,
                RetailImageID = string.Empty
            };
        }

        private LSCentral.HospTransactionLine MobileTransLine(string id, OrderHospLine line, string storeId)
        {
            return new LSCentral.HospTransactionLine()
            {
                Id = id,
                LineNo = XMLHelper.LineNumberToNav(line.LineNumber),
                EntryStatus = (int)EntryStatus.Normal,
                LineType = (int)line.LineType,
                Number = (line.LineType == LineType.Item ? line.ItemId : string.Empty),
                Barcode = (line.LineType == LineType.Coupon ? line.ItemId : string.Empty),
                StoreId = storeId,
                CurrencyFactor = 1,
                VariantCode = XMLHelper.GetString(line.VariantId),
                UomId = XMLHelper.GetString(line.UomId),
                NetPrice = line.NetPrice,
                Price = (line.PriceModified) ? 0 : line.Price,
                Quantity = line.Quantity,
                DiscountAmount = line.DiscountAmount,
                DiscountPercent = line.DiscountPercent,
                NetAmount = line.NetAmount,
                TAXAmount = line.TaxAmount,
                ManualPrice = (line.PriceModified) ? line.Price : 0,
                RecommendedItem = false,
                DealItem = (line.IsADeal ? 1 : 0),  //0=Item line, 1=Deal line

                StaffId = string.Empty,
                CurrencyCode = string.Empty,
                TAXProductCode = string.Empty,
                TAXBusinessCode = string.Empty,
                CardOrCustNo = string.Empty,
                EFTCardNumber = string.Empty,
                EFTCardName = string.Empty,
                EFTAuthCode = string.Empty,
                EFTMessage = string.Empty,
                PriceGroupCode = string.Empty,
                CouponCode = string.Empty,
                TerminalId = string.Empty,
                EFTTransactionNo = string.Empty,
                SalesType = string.Empty,
                ItemDescription = string.Empty,
                LineKitchenStatusCode = string.Empty,
                RestMenuTypeCode = string.Empty,
                TenderDescription = string.Empty,
                UomDescription = string.Empty,
                VariantDescription = string.Empty,
                GenBusPostingGroup = string.Empty,
                GenProdPostingGroup = string.Empty,
                VatBusPostingGroup = string.Empty,
                VatProdPostingGroup = string.Empty,
                OrigTransPos = string.Empty,
                OrigTransStore = string.Empty,
                RetailImageID = string.Empty
            };
        }

        private LSCentral.MobileTransactionLine MobileTransLine(string id, PublishedOffer offer, int lineNumber, string storeId)
        {
            return new LSCentral.MobileTransactionLine()
            {
                Id = id,
                LineNo = lineNumber,
                EntryStatus = (int)EntryStatus.Normal,
                LineType = (int)LineType.Coupon,
                CouponCode = XMLHelper.GetString(offer.OfferId),
                Barcode = XMLHelper.GetString(offer.OfferId),
                StoreId = storeId,

                Number = string.Empty,
                CurrencyCode = string.Empty,
                VariantCode = string.Empty,
                UomId = string.Empty,
                TAXProductCode = string.Empty,
                TAXBusinessCode = string.Empty,
                CardOrCustNo = string.Empty,
                EFTCardNumber = string.Empty,
                EFTCardName = string.Empty,
                EFTAuthCode = string.Empty,
                EFTMessage = string.Empty,
                StaffId = string.Empty,
                PriceGroupCode = string.Empty,
                TerminalId = string.Empty,
                EFTTransactionNo = string.Empty,
                SalesType = string.Empty,
                ItemDescription = string.Empty,
                LineKitchenStatusCode = string.Empty,
                RestMenuTypeCode = string.Empty,
                TenderDescription = string.Empty,
                UomDescription = string.Empty,
                VariantDescription = string.Empty,
                OrigTransPos = string.Empty,
                OrigTransStore = string.Empty,
                RetailImageID = string.Empty
            };
        }

        private LSCentral.MobileTransactionLine MobileTransLine(string id, OneListPublishedOffer offer, int lineNumber, string storeId)
        {
            return new LSCentral.MobileTransactionLine()
            {
                Id = id,
                LineNo = XMLHelper.LineNumberToNav(lineNumber),
                EntryStatus = (int)EntryStatus.Normal,
                LineType = (int)LineType.Coupon,
                Barcode = XMLHelper.GetString(offer.Id),
                CouponCode = XMLHelper.GetString(offer.Id),
                StoreId = storeId,

                StaffId = string.Empty,
                Number = string.Empty,
                CurrencyCode = string.Empty,
                VariantCode = string.Empty,
                UomId = string.Empty,
                TAXProductCode = string.Empty,
                TAXBusinessCode = string.Empty,
                CardOrCustNo = string.Empty,
                EFTCardNumber = string.Empty,
                EFTCardName = string.Empty,
                EFTAuthCode = string.Empty,
                EFTMessage = string.Empty,
                PriceGroupCode = string.Empty,
                TerminalId = string.Empty,
                EFTTransactionNo = string.Empty,
                SalesType = string.Empty,
                ItemDescription = string.Empty,
                LineKitchenStatusCode = string.Empty,
                RestMenuTypeCode = string.Empty,
                TenderDescription = string.Empty,
                UomDescription = string.Empty,
                VariantDescription = string.Empty,
                OrigTransPos = string.Empty,
                OrigTransStore = string.Empty,
                GenBusPostingGroup = string.Empty,
                GenProdPostingGroup = string.Empty,
                VatBusPostingGroup = string.Empty,
                VatProdPostingGroup = string.Empty,
                RetailImageID = string.Empty
            };
        }

        private LSCentral.MobileTransactionSubLine CreateTransactionSubLine(string id, OneListItemSubLine rq, int parLineNo, string parentItemNo)
        {
            return new LSCentral.MobileTransactionSubLine()
            {
                Id = id,
                LineNo = rq.LineNumber,
                ParentLineNo = XMLHelper.LineNumberToNav((rq.ParentSubLineId > 0) ? rq.ParentSubLineId : parLineNo),
                EntryStatus = (int)EntryStatus.Normal,
                ParentLineIsSubline = (rq.ParentSubLineId > 0) ? 1 : 0,
                LineType = (int)rq.Type,
                Number = XMLHelper.GetString(rq.ItemId),
                VariantCode = XMLHelper.GetString(rq.VariantId),
                UomId = XMLHelper.GetString(rq.Uom),
                Quantity = rq.Quantity,
                Description = XMLHelper.GetString(rq.Description),
                VariantDescription = XMLHelper.GetString(rq.VariantDescription),
                UomDescription = XMLHelper.GetString(rq.Uom),
                ModifierGroupCode = XMLHelper.GetString(rq.ModifierGroupCode),
                ModifierSubCode = XMLHelper.GetString(rq.ModifierSubCode),
                DealLine = string.IsNullOrEmpty(parentItemNo) ? 0 : XMLHelper.LineNumberToNav(rq.DealLineId),
                DealModLine = string.IsNullOrEmpty(parentItemNo) ? 0 : XMLHelper.LineNumberToNav(rq.DealModLineId),
                DealId = string.IsNullOrEmpty(parentItemNo) ? "0" : parentItemNo,

                StaffId = string.Empty,
                Barcode = string.Empty,
                TAXBusinessCode = string.Empty,
                TAXProductCode = string.Empty,
                TransDate = DateTime.Now,
                GenBusPostingGroup = string.Empty,
                GenProdPostingGroup = string.Empty,
                VatBusPostingGroup = string.Empty,
                VatProdPostingGroup = string.Empty,
                LineKitchenStatusCode = string.Empty
            };
        }

        private LSCentral.HospTransactionSubLine CreateTransactionSubLine(string id, OrderHospSubLine rq, ref int lineNo, int parLineNo, SubLineType subLineType)
        {
            if (subLineType == SubLineType.Deal)
            {
                rq.ModifierGroupCode = string.Empty;
                rq.ModifierSubCode = string.Empty;
            }
            else if (subLineType == SubLineType.Modifier)
            {
                rq.DealCode = "0";
                rq.DealLineId = 0;
                rq.DealModifierLineId = 0;
            }
            else if (subLineType == SubLineType.Text)
            {
                rq.ItemId = string.Empty;
                rq.DealCode = "0";
                rq.DealLineId = 0;
                rq.DealModifierLineId = 0;
            }

            if (rq.LineNumber == 0)
                rq.LineNumber = ++lineNo;

            return new LSCentral.HospTransactionSubLine()
            {
                Id = id,
                LineNo = XMLHelper.LineNumberToNav(rq.LineNumber),
                ParentLineNo = XMLHelper.LineNumberToNav((rq.ParentSubLineId > 0) ? rq.ParentSubLineId : parLineNo),
                EntryStatus = (int)EntryStatus.Normal,
                ParentLineIsSubline = (rq.ParentSubLineId > 0) ? 1 : 0,
                LineType = (int)subLineType,
                Number = XMLHelper.GetString(rq.ItemId),
                VariantCode = XMLHelper.GetString(rq.VariantId),
                UomId = XMLHelper.GetString(rq.Uom),
                Quantity = rq.Quantity,
                NetPrice = rq.NetPrice,
                Price = rq.Price,
                DiscountAmount = rq.DiscountAmount,
                DiscountPercent = rq.DiscountPercent,
                NetAmount = rq.NetAmount,
                TAXAmount = rq.TAXAmount,
                ManualDiscountAmount = rq.ManualDiscountAmount,
                ManualDiscountPercent = rq.ManualDiscountPercent,
                Description = XMLHelper.GetString(rq.Description),
                VariantDescription = XMLHelper.GetString(rq.VariantDescription),
                UomDescription = XMLHelper.GetString(rq.Uom),
                ModifierGroupCode = XMLHelper.GetString(rq.ModifierGroupCode),
                ModifierSubCode = XMLHelper.GetString(rq.ModifierSubCode),
                DealLine = rq.DealLineId,
                DealModLine = rq.DealModifierLineId,
                DealId = XMLHelper.GetString(rq.DealCode),
                PriceReductionOnExclusion = (rq.PriceReductionOnExclusion ? 1 : 0),

                StaffId = string.Empty,
                Barcode = string.Empty,
                TAXBusinessCode = string.Empty,
                TAXProductCode = string.Empty,
                TransDate = DateTime.Now,
                GenBusPostingGroup = string.Empty,
                GenProdPostingGroup = string.Empty,
                VatBusPostingGroup = string.Empty,
                VatProdPostingGroup = string.Empty,
                LineKitchenStatusCode = string.Empty
            };
        }

        private List<LSCentral.HospTransactionSubLine> MobileTransSubLine(string id, OrderHospLine posline, ref int subLineCounter)
        {
            if (posline.SubLines == null)
                return new List<LSCentral.HospTransactionSubLine>();

            List<LSCentral.HospTransactionSubLine> subLines = new List<LSCentral.HospTransactionSubLine>();
            foreach (OrderHospSubLine dLine in posline.SubLines)
            {
                subLines.Add(CreateTransactionSubLine(id, dLine, ref subLineCounter, posline.LineNumber, dLine.Type));
            }
            return subLines;
        }

        private List<LSCentral.MobileTransactionSubLine> MobileTransSubLine(string id, OneListItem posline)
        {
            if (posline.OnelistSubLines == null)
                return new List<LSCentral.MobileTransactionSubLine>();

            List<LSCentral.MobileTransactionSubLine> subLines = new List<LSCentral.MobileTransactionSubLine>();
            foreach (OneListItemSubLine subLine in posline.OnelistSubLines)
            {
                switch (subLine.Type)
                {
                    case SubLineType.Modifier:
                        subLines.Add(CreateTransactionSubLine(id, subLine, posline.LineNumber, string.Empty));
                        break;

                    case SubLineType.Deal:
                        subLines.Add(CreateTransactionSubLine(id, subLine, posline.LineNumber, posline.ItemId));
                        break;
                }
            }
            return subLines;
        }


        private LSCentral.MobileTransactionLine TransPaymentLine(string id, PaymentLine paymentLine, string transactionCurrencyCode, int lineNumber)
        {
            string currencyCode;
            decimal currencyFactor;

            if (paymentLine.Payment.CurrencyExchRate != null)
            {
                currencyCode = paymentLine.Payment.Amount.Currency.Id;
                currencyFactor = paymentLine.Payment.CurrencyExchRate.CurrencyFactor;
            }
            else
            {
                //The tender line is in the store currency
                currencyCode = transactionCurrencyCode;
                currencyFactor = 1;
            }

            int entryStatus = paymentLine.Voided ? (int)EntryStatus.Voided : (int)EntryStatus.Normal; //0=normal, 1=voided
            AuthorizationStatus eftAuthStatus = AuthorizationStatus.Approved;
            PaymentTransactionType paymentTransactionType = PaymentTransactionType.Purchase;

            string eftAuthCode = string.Empty;
            string eftCardNumber = string.Empty;
            string eftCardName = string.Empty;
            string eftMessage = string.Empty;
            string eftTransactionId = string.Empty;
            VerificationMethod verificationMethod = VerificationMethod.None;

            if (paymentLine.Payment.TenderType.Function == TenderTypeFunction.Cards)
            {
                eftAuthCode = XMLHelper.GetString(paymentLine.Payment.AuthenticationCode);
                eftCardNumber = XMLHelper.GetString(paymentLine.Payment.CardNumber);
                eftCardName = XMLHelper.GetString(paymentLine.Payment.CardType);
                eftMessage = XMLHelper.GetString(paymentLine.Payment.EFTMessage);
                eftTransactionId = XMLHelper.GetString(paymentLine.Payment.EFTTransactionId);
                eftAuthStatus = paymentLine.Payment.AuthorizationStatus;
                paymentTransactionType = paymentLine.Payment.TransactionType;
                verificationMethod = paymentLine.Payment.VerificationMethod;
            }

            if (eftAuthStatus != AuthorizationStatus.Approved && eftAuthStatus != AuthorizationStatus.Unknown)
            {
                entryStatus = 1; //voided
            }
            if (paymentTransactionType != PaymentTransactionType.Purchase && paymentTransactionType != PaymentTransactionType.Refund)
            {
                entryStatus = 1; //voided
            }

            return new LSCentral.MobileTransactionLine()
            {
                Id = id,
                LineNo = XMLHelper.LineNumberToNav(lineNumber),
                EntryStatus = entryStatus,
                LineType = (int)LineType.Payment,
                Number = paymentLine.Payment.TenderType.Id,
                CurrencyCode = XMLHelper.GetString(currencyCode),
                CurrencyFactor = currencyFactor,
                NetAmount = paymentLine.Payment.Amount.Value,
                StoreId = paymentLine.Payment.StoreId,
                TerminalId = paymentLine.Payment.TerminalId,
                StaffId = paymentLine.Payment.StaffId,
                EFTCardNumber = eftCardNumber,
                EFTCardName = eftCardName,
                EFTAuthCode = eftAuthCode,
                EFTMessage = eftMessage,
                EFTVerificationMethod = (int)verificationMethod,
                EFTTransactionNo = eftTransactionId ?? string.Empty,
                EFTAuthStatus = (int)eftAuthStatus,
                EFTTransType = (int)paymentTransactionType,

                VariantCode = string.Empty,
                VariantDescription = string.Empty,
                Barcode = string.Empty,
                UomDescription = string.Empty,
                UomId = string.Empty,
                TAXBusinessCode = string.Empty,
                TAXProductCode = string.Empty,
                CardOrCustNo = string.Empty,
                CouponCode = string.Empty,
                ItemDescription = string.Empty,
                LineKitchenStatusCode = string.Empty,
                PriceGroupCode = string.Empty,
                RestMenuTypeCode = string.Empty,
                SalesType = string.Empty,
                TenderDescription = string.Empty,
                OrigTransPos = string.Empty,
                OrigTransStore = string.Empty,
                RetailImageID = string.Empty
            };
        }

        private LSCentral.HospTransactionLine TransPaymentLine(string id, OrderPayment paymentLine, string storeId, int lineNumber)
        {
            return new LSCentral.HospTransactionLine()
            {
                Id = id,
                LineNo = XMLHelper.LineNumberToNav(lineNumber),
                EntryStatus = (paymentLine.PaymentType == PaymentType.Refund) ? (int)EntryStatus.Voided : (int)EntryStatus.Normal, //0=normal, 1=voided,
                LineType = (int)LineType.Payment,
                Number = paymentLine.TenderType,
                CurrencyCode = XMLHelper.GetString(paymentLine.CurrencyCode),
                CurrencyFactor = paymentLine.CurrencyFactor,
                NetAmount = paymentLine.Amount,
                StoreId = storeId,
                EFTCardNumber = XMLHelper.GetString(paymentLine.CardNumber),
                EFTAuthCode = XMLHelper.GetString(paymentLine.AuthorizationCode),
                EFTTransactionNo = XMLHelper.GetString(paymentLine.ExternalReference),

                EFTMessage = string.Empty,
                EFTCardName = string.Empty,
                TerminalId = string.Empty,
                StaffId = string.Empty,
                VariantCode = string.Empty,
                VariantDescription = string.Empty,
                Barcode = string.Empty,
                UomDescription = string.Empty,
                UomId = string.Empty,
                TAXBusinessCode = string.Empty,
                TAXProductCode = string.Empty,
                CardOrCustNo = string.Empty,
                CouponCode = string.Empty,
                ItemDescription = string.Empty,
                LineKitchenStatusCode = string.Empty,
                PriceGroupCode = string.Empty,
                RestMenuTypeCode = string.Empty,
                SalesType = string.Empty,
                TenderDescription = string.Empty,
                OrigTransPos = string.Empty,
                OrigTransStore = string.Empty,
                RetailImageID = string.Empty
            };
        }

        private void MobileTransLine(LSCentral.MobileTransactionLine mobileTransLine, ref RetailTransaction transaction, List<DiscountLine> discounts, LSCentral.MobileReceiptInfo[] receiptInfos)
        {
            LineType lineType = (LineType)mobileTransLine.LineType;
            EntryStatus entryStatus = (EntryStatus)mobileTransLine.EntryStatus;
            int lineNumber = mobileTransLine.LineNo;
            Currency transLineCurrency = new UnknownCurrency(mobileTransLine.CurrencyCode);

            decimal quantity = mobileTransLine.Quantity;
            //when qty is negative then set the IsReturnLine to true
            bool isReturnLine = (quantity < 0M ? true : false);
            //and set the qty to abs value for MPOS
            quantity = Math.Abs(quantity);

            //getting periodic discount total discount - ignore them
            if (lineType == LineType.Item)
            {
                SaleLine saleLine = new SaleLine(lineNumber);   // MPOS expects to get 1 not 10000

                saleLine.Quantity = quantity;
                saleLine.IsReturnLine = isReturnLine;
                saleLine.Voided = entryStatus == EntryStatus.Voided;

                if (mobileTransLine.ManualPrice != 0)
                {
                    saleLine.PriceOverride(mobileTransLine.ManualPrice);
                }

                if (mobileTransLine.ManualDiscountPercent > 0)
                    saleLine.SetManualDiscount(0, mobileTransLine.ManualDiscountPercent, DiscountEntryType.Percentage);
                else if (mobileTransLine.ManualDiscountAmount > 0)
                    saleLine.SetManualDiscount(mobileTransLine.ManualDiscountAmount, 0, DiscountEntryType.Amount);

                saleLine.LineDiscount = new Discount(transLineCurrency, mobileTransLine.DiscountAmount, mobileTransLine.DiscountPercent, DiscountEntryType.Both);
                List<DiscountLine> saleLineDiscounts = discounts.Where(x => x.LineNumber == lineNumber).ToList();
                if (saleLineDiscounts?.Count > 0)
                {
                    foreach (DiscountLine saleLineDiscount in saleLineDiscounts)
                    {
                        saleLine.Discounts.Add(saleLineDiscount);
                    }
                }

                UnknownRetailItem item = new UnknownRetailItem(mobileTransLine.Number);
                item.SelectedVariant = new UnknownVariantRegistration(mobileTransLine.VariantCode);
                item.UnitOfMeasure = new UnknownUnitOfMeasure(mobileTransLine.UomId, mobileTransLine.Number);
                item.Images.Add(new ItemImage(mobileTransLine.RetailImageID));

                saleLine.Item = item;

                saleLine.UnitPrice = new Money(mobileTransLine.NetPrice, transLineCurrency);
                saleLine.UnitPriceWithTax = new Money(mobileTransLine.Price, transLineCurrency);

                saleLine.NetAmount = new Money(mobileTransLine.NetAmount, transLineCurrency);
                saleLine.TaxAmount = new Money(mobileTransLine.TAXAmount, transLineCurrency);
                saleLine.GrossAmount = new Money(mobileTransLine.NetAmount + mobileTransLine.TAXAmount, transLineCurrency);
                saleLine.ContainsExternalPrices = true;
                saleLine.FromRecommendation = mobileTransLine.RecommendedItem;
                saleLine.OriginalTransactionLineNo = mobileTransLine.OrigTransLineNo;

                transaction.SaleLines.Add(saleLine);
            }
            else if (lineType == LineType.IncomeExpense)
            {
                IncomeExpenseLine incomeExpenseLine = new IncomeExpenseLine(lineNumber, mobileTransLine.Number, mobileTransLine.NetAmount, mobileTransLine.EFTTransactionNo, transLineCurrency);
                incomeExpenseLine.Voided = entryStatus == EntryStatus.Voided;

                transaction.IncomeExpenseLines.Add(incomeExpenseLine);
            }
            else if (lineType == LineType.Payment)
            {
                //getting tender lines
                PaymentLine paymentLine = new PaymentLine(mobileTransLine.LineNo);

                paymentLine.Id = mobileTransLine.LineNo.ToString();
                paymentLine.Payment.StoreId = transaction.Terminal?.Store?.Id;
                paymentLine.Payment.TerminalId = transaction.Terminal?.Id;

                paymentLine.Payment.StoreId = mobileTransLine.StoreId;
                paymentLine.Payment.TerminalId = mobileTransLine.TerminalId;
                paymentLine.Payment.StaffId = mobileTransLine.StaffId;
                paymentLine.Payment.TenderType = new TenderType(mobileTransLine.Number);

                paymentLine.Payment.Amount = new Money(mobileTransLine.NetAmount, transLineCurrency); //request sends in net amount

                paymentLine.Voided = entryStatus == EntryStatus.Voided;
                paymentLine.Payment.CardNumber = mobileTransLine.EFTCardNumber;
                paymentLine.Payment.AuthenticationCode = mobileTransLine.EFTAuthCode;
                paymentLine.Payment.CardType = mobileTransLine.EFTCardName;
                paymentLine.Payment.EFTMessage = mobileTransLine.EFTMessage;
                paymentLine.Payment.EFTTransactionId = mobileTransLine.EFTTransactionNo;
                paymentLine.Payment.VerificationMethod = (VerificationMethod)mobileTransLine.EFTVerificationMethod;

                paymentLine.Payment.AuthorizationStatus = (AuthorizationStatus)mobileTransLine.EFTAuthStatus;
                paymentLine.Payment.TransactionType = (PaymentTransactionType)mobileTransLine.EFTTransType;

                paymentLine.Payment.DateTime = ConvertTo.SafeJsonDate(mobileTransLine.EFTDateTime, IsJson);

                //we voided these when sent to NAV
                if (paymentLine.Payment.AuthorizationStatus != AuthorizationStatus.Approved && paymentLine.Payment.AuthorizationStatus != AuthorizationStatus.Unknown)
                {
                    paymentLine.Voided = false;
                }
                if (paymentLine.Payment.TransactionType != PaymentTransactionType.Purchase)
                {
                    paymentLine.Voided = false;
                }

                //get the mobile receipt info for the tender line
                paymentLine.Payment.ReceiptInfo = ReceiptInfoLine(receiptInfos, mobileTransLine.LineNo);
                transaction.TenderLines.Add(paymentLine);
            }
        }

        private LSCentral.MobileReceiptInfo ReceiptInfoLine(string id, ReceiptInfo receiptInfo, int lineNumber, string transactionNo)
        {
            LSCentral.MobileReceiptInfo info = new LSCentral.MobileReceiptInfo()
            {
                Id = id,
                Line_No = XMLHelper.LineNumberToNav(lineNumber),
                Key = receiptInfo.Key,
                Type = receiptInfo.Type,
                Transaction_No = string.IsNullOrEmpty(transactionNo) ? 0 : ConvertTo.SafeInt(transactionNo)
            };

            if (receiptInfo.ValueAsText == null)
            {
                info.Value = string.Empty;
                return info;
            }

            if (receiptInfo.ValueAsText.Length > 100)
            {
                List<string> enc = new List<string>();
                enc.Add(ConvertTo.Base64Encode(receiptInfo.ValueAsText));
                info.LargeValue = enc.ToArray();
                info.Value = string.Empty;
            }
            else
            {
                info.Value = receiptInfo.ValueAsText;
            }
            return info;
        }

        private List<ReceiptInfo> ReceiptInfoLine(LSCentral.MobileReceiptInfo[] mobileReceiptInfos, int lineNo)
        {
            List<ReceiptInfo> receiptInfoList = new List<ReceiptInfo>();
            if (mobileReceiptInfos == null)
                return receiptInfoList;

            //get MobileTransactionSubLine for this parent
            foreach (LSCentral.MobileReceiptInfo receiptInfo in mobileReceiptInfos.Where(x => x.Line_No == lineNo))
            {
                ReceiptInfo ri = new ReceiptInfo();
                ri.IsLargeValue = false;
                ri.Key = receiptInfo.Key;
                ri.Type = receiptInfo.Type;

                ri.Value = new List<PrintLine>();
                ri.Value.Add(new PrintLine()
                {
                    Text = receiptInfo.Value
                });
                receiptInfoList.Add(ri);
            }
            return receiptInfoList;
        }

        private List<OrderHospSubLine> ModifierSubLine(LSCentral.MobileTransactionSubLine[] mobileTransSubLines, int parentLine)
        {
            List<OrderHospSubLine> modLineList = new List<OrderHospSubLine>();
            if (mobileTransSubLines == null)
                return modLineList;

            //get MobileTransactionSubLine for this parent
            List<LSCentral.MobileTransactionSubLine> list = mobileTransSubLines.ToList();
            List<LSCentral.MobileTransactionSubLine> lines = list.FindAll(l => l.ParentLineNo == parentLine && l.LineType == 0);
            foreach (LSCentral.MobileTransactionSubLine line in lines)
            {
                modLineList.Add(new OrderHospSubLine()
                {
                    Type = (SubLineType)line.LineType,
                    LineNumber = line.LineNo,
                    ItemId = line.Number,
                    VariantId = line.VariantCode,
                    Uom = line.UomId,
                    NetPrice = line.NetPrice,
                    Price = line.Price,
                    Quantity = line.Quantity,
                    DiscountAmount = line.DiscountAmount,
                    DiscountPercent = line.DiscountPercent,
                    NetAmount = line.NetAmount,
                    TAXAmount = line.TAXAmount,
                    Amount = line.NetAmount + line.TAXAmount,
                    ManualDiscountAmount = line.ManualDiscountAmount,
                    ManualDiscountPercent = line.ManualDiscountPercent,
                    Description = line.Description,
                    VariantDescription = line.VariantDescription,
                    ModifierGroupCode = line.ModifierGroupCode,
                    ModifierSubCode = line.ModifierSubCode,
                    DealLineId = Convert.ToInt32(line.DealLine),
                    DealModifierLineId = Convert.ToInt32(line.DealModLine),
                    DealCode = line.DealId,
                    PriceReductionOnExclusion = line.PriceReductionOnExclusion != 0
                });
            }
            return modLineList;
        }

        private List<OrderHospSubLine> DealSubLine(LSCentral.MobileTransactionSubLine[] mobileTransSubLines, int parentLine)
        {
            List<OrderHospSubLine> modLineList = new List<OrderHospSubLine>();
            if (mobileTransSubLines == null)
                return modLineList;

            List<LSCentral.MobileTransactionSubLine> list = mobileTransSubLines.ToList();
            List<LSCentral.MobileTransactionSubLine> lines = list.FindAll(l => l.LineType == 1 && l.ParentLineNo == parentLine);
            foreach (LSCentral.MobileTransactionSubLine line in lines)
            {
                modLineList.Add(new OrderHospSubLine()
                {
                    Type = SubLineType.Deal,
                    LineNumber = line.LineNo,
                    DealLineId = line.DealLine,
                    DealCode = line.DealId,
                    DealModifierLineId = line.DealModLine,
                    ItemId = line.Number,
                    Uom = line.UomId,
                    Description = line.Description,
                    NetPrice = line.NetPrice,
                    Price = line.Price,
                    Quantity = line.Quantity,
                    DiscountAmount = line.DiscountAmount,
                    DiscountPercent = line.DiscountPercent,
                    NetAmount = line.NetAmount,
                    TAXAmount = line.TAXAmount,
                    Amount = line.NetAmount + line.TAXAmount
                });

                // check if there are modifiers for the sub lines
                List<LSCentral.MobileTransactionSubLine> slines = list.FindAll(l => l.LineType == 0 && l.ParentLineNo == line.LineNo && l.ParentLineIsSubline == 1);
                foreach (LSCentral.MobileTransactionSubLine sline in slines)
                {
                    modLineList.Add(new OrderHospSubLine()
                    {
                        Type = (SubLineType)sline.LineType,
                        LineNumber = sline.LineNo,
                        ParentSubLineId = sline.ParentLineNo,
                        ItemId = sline.Number,
                        VariantId = sline.VariantCode,
                        Uom = sline.UomId,
                        NetPrice = sline.NetPrice,
                        Price = sline.Price,
                        Quantity = sline.Quantity,
                        DiscountAmount = sline.DiscountAmount,
                        DiscountPercent = sline.DiscountPercent,
                        NetAmount = sline.NetAmount,
                        TAXAmount = sline.TAXAmount,
                        Amount = sline.NetAmount + sline.TAXAmount,
                        ManualDiscountAmount = sline.ManualDiscountAmount,
                        ManualDiscountPercent = sline.ManualDiscountPercent,
                        Description = sline.Description,
                        VariantDescription = sline.VariantDescription,
                        ModifierGroupCode = sline.ModifierGroupCode,
                        ModifierSubCode = sline.ModifierSubCode,
                        DealLineId = Convert.ToInt32(sline.DealLine),
                        DealModifierLineId = Convert.ToInt32(sline.DealModLine),
                        DealCode = sline.DealId,
                        PriceReductionOnExclusion = sline.PriceReductionOnExclusion != 0
                    });
                }
            }
            return modLineList;
        }

        private List<OrderHospSubLine> TextModifierSubLine(LSCentral.MobileTransactionSubLine[] mobileTransSubLines, int parentLine)
        {
            List<OrderHospSubLine> modLineList = new List<OrderHospSubLine>();
            if (mobileTransSubLines == null)
                return modLineList;

            List<LSCentral.MobileTransactionSubLine> list = mobileTransSubLines.ToList();
            List<LSCentral.MobileTransactionSubLine> lines = list.FindAll(l => l.LineType == 2 && l.ParentLineNo == parentLine);
            foreach (LSCentral.MobileTransactionSubLine line in lines)
            {
                modLineList.Add(new OrderHospSubLine()
                {
                    Type = SubLineType.Text,
                    LineNumber = line.LineNo,
                    Description = line.Description,
                    ModifierGroupCode = line.ModifierGroupCode,
                    ModifierSubCode = line.ModifierSubCode,
                    Quantity = line.Quantity,
                });
            }
            return modLineList;
        }

        #endregion
    }
}
