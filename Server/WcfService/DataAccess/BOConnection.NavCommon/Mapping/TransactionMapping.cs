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
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Pos.Items;
using LSRetail.Omni.Domain.DataModel.Pos.Items.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Pos.Payments;
using LSRetail.Omni.Domain.DataModel.Pos.TenderTypes;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions.Discounts;

namespace LSOmni.DataAccess.BOConnection.NavCommon.Mapping
{
    public class TransactionMapping : BaseMapping
    {
        private Version NavVersion;

        public TransactionMapping(Version navVersion)
        {
            NavVersion = navVersion;
        }

        public RetailTransaction MapFromRootToRetailTransaction(NavWS.RootMobileTransaction root)
        {
            NavWS.MobileTransaction header = root.MobileTransaction.FirstOrDefault();
            UnknownCurrency transactionCurrency = new UnknownCurrency(header.CurrencyCode);

            RetailTransaction transaction = new RetailTransaction()
            {
                Id = header.Id,
                Voided = (EntryStatus)header.EntryStatus == EntryStatus.Voided,
                ReceiptNumber = header.ReceiptNo,
                TransactionNumber = header.TransactionNo.ToString(),
                BeginDateTime = header.TransDate,
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

            //now loop thru the discount lines
            List<DiscountLine> discounts = new List<DiscountLine>();
            if (root.MobileTransDiscountLine != null)
            {
                foreach (NavWS.MobileTransDiscountLine mobileTransDisc in root.MobileTransDiscountLine)
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
                    discount.LineNumber = LineNumberFromNav(mobileTransDisc.LineNo);
                    discounts.Add(discount);
                }
            }

            transaction.IncomeExpenseLines = new List<IncomeExpenseLine>();

            //now loop thru the lines
            transaction.SaleLines = new List<SaleLine>();
            if (root.MobileTransactionLine != null)
            {
                foreach (NavWS.MobileTransactionLine mobileTransLine in root.MobileTransactionLine)
                {
                    MobileTransLine(mobileTransLine, ref transaction, discounts, root.MobileReceiptInfo);
                }
            }
            return transaction;
        }

        public SalesEntry MapFromRootToRetailTransaction(NavWS.RootGetTransaction root)
        {
            NavWS.TransactionHeader header = root.TransactionHeader.FirstOrDefault();
            UnknownCurrency transactionCurrency = new UnknownCurrency(header.TransCurrency);

            SalesEntry transaction = new SalesEntry()
            {
                Id = header.ReceiptNo,
                DocumentRegTime = header.Date,
                CardId = header.MemberCardNo,
                StoreId = header.StoreNo,
                TerminalId = header.POSTerminalNo,
                TotalAmount = header.GrossAmount,
                TotalNetAmount = header.NetAmount,
                TotalDiscount = header.DiscountAmount,
                LineItemCount = (int)header.NoofItemLines,
                IdType = DocumentIdType.Receipt
            };

            //now loop thru the discount lines
            transaction.DiscountLines = new List<SalesEntryDiscountLine>();
            if (root.TransDiscountEntry != null)
            {
                foreach (NavWS.TransDiscountEntry mobileTransDisc in root.TransDiscountEntry)
                {
                    transaction.DiscountLines.Add(new SalesEntryDiscountLine()
                    {
                        OfferNumber = mobileTransDisc.OfferNo,
                        LineNumber = LineNumberFromNav(mobileTransDisc.LineNo),
                        DiscountAmount = mobileTransDisc.DiscountAmount,
                        PeriodicDiscType = (PeriodicDiscType)(Convert.ToInt32(mobileTransDisc.OfferType) + 1)
                    });
                }
            }

            //now loop thru the lines
            transaction.Lines = new List<SalesEntryLine>();
            if (root.TransSalesEntry != null)
            {
                foreach (NavWS.TransSalesEntry mobileTransLine in root.TransSalesEntry)
                {
                    transaction.Lines.Add(new SalesEntryLine()
                    {
                        ItemId = mobileTransLine.ItemNo,
                        UomId = mobileTransLine.UnitofMeasure,
                        VariantId = mobileTransLine.VariantCode,
                        Amount = mobileTransLine.NetAmount + mobileTransLine.VATAmount,
                        NetAmount = mobileTransLine.NetAmount,
                        TaxAmount = mobileTransLine.VATAmount,
                        DiscountAmount = mobileTransLine.DiscountAmount,
                        Quantity = mobileTransLine.Quantity,
                        Price = mobileTransLine.Price,
                        NetPrice = mobileTransLine.NetPrice,
                        LineNumber = LineNumberFromNav(mobileTransLine.LineNo)
                    });
                }
            }
            return transaction;
        }

        public NavWS.RootMobileTransaction MapFromRetailTransactionToRoot(RetailTransaction transaction)
        {
            NavWS.RootMobileTransaction root = new NavWS.RootMobileTransaction();

            //MobileTrans
            List<NavWS.MobileTransaction> trans = new List<NavWS.MobileTransaction>();
            trans.Add(MobileTrans(transaction));
            root.MobileTransaction = trans.ToArray();

            //MobileTransLines
            List<NavWS.MobileTransactionLine> transLines = new List<NavWS.MobileTransactionLine>();
            List<NavWS.MobileReceiptInfo> receiptLines = new List<NavWS.MobileReceiptInfo>();

            int maxTransLine = 1;

            foreach (SaleLine saleLine in transaction.SaleLines)
            {
                if (maxTransLine < saleLine.LineNumber)
                    maxTransLine = saleLine.LineNumber;

                transLines.Add(MobileTransLine(transaction.Id, saleLine, transaction.Terminal.Store.Currency, transaction.Terminal.Store.Id.ToUpper(), transaction.RefundedFromStoreNo, transaction.RefundedFromTerminalNo, transaction.RefundedFromTransNo));
            }

            if (transaction.LoyaltyContact != null)
            {
                foreach (PublishedOffer offer in transaction.LoyaltyContact.PublishedOffers?.Where(x => x.Selected))
                {
                    maxTransLine++;
                    transLines.Add(MobileTransLine(transaction.Id, offer, maxTransLine, transaction.Terminal.Store.Id.ToUpper()));
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

        public NavWS.RootMobileTransaction MapFromOrderToRoot(Order transaction)
        {
            NavWS.RootMobileTransaction root = new NavWS.RootMobileTransaction();

            if (string.IsNullOrEmpty(transaction.Id))
                transaction.Id = Guid.NewGuid().ToString();

            //MobileTrans
            List<NavWS.MobileTransaction> trans = new List<NavWS.MobileTransaction>();
            trans.Add(new NavWS.MobileTransaction()
            {
                Id = transaction.Id,
                StoreId = transaction.StoreId,
                TransactionType = 2,
                EntryStatus = (int)EntryStatus.Normal,
                TransDate = DateTime.Now,
                SaleIsReturnSale = false,
                MemberCardNo = transaction.CardId,
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
            List<NavWS.MobileTransactionLine> transLines = new List<NavWS.MobileTransactionLine>();
            List<NavWS.MobileReceiptInfo> receiptLines = new List<NavWS.MobileReceiptInfo>();

            int lineno = 1;
            foreach (OrderLine saleLine in transaction.OrderLines)
            {
                saleLine.LineNumber = lineno++;
                transLines.Add(MobileTransLine(transaction.Id, saleLine, transaction.StoreId));
            }

            root.MobileReceiptInfo = receiptLines.ToArray();
            root.MobileTransactionLine = transLines.ToArray();
            return root;
        }

        public List<SalesEntry> MapFromRootToSalesEntries(NavWS.RootGetMemberSalesHistory root)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            if (root.MemberSalesEntry == null)
                return null;

            foreach (NavWS.MemberSalesEntry trans in root.MemberSalesEntry)
            {
                list.Add(new SalesEntry()
                {
                    Id = trans.DocumentNo,
                    TerminalId = trans.POSTerminalNo,
                    DocumentRegTime = trans.Date,
                    TotalAmount = trans.GrossAmount,
                    LineItemCount = (int)trans.Quantity,
                    StoreName = trans.StoreName,
                    CardId = trans.MemberCardNo
                });
            }
            return list;
        }

        public RetailTransaction MapFromRootToLoyTransaction(NavWS.RootGetTransaction root)
        {
            NavWS.TransactionHeader header = root.TransactionHeader.FirstOrDefault();
            UnknownCurrency transactionCurrency = new UnknownCurrency(header.TransCurrency);

            RetailTransaction transaction = new RetailTransaction()
            {
                Id = header.TransactionNo.ToString(),
                GrossAmount = new Money(header.GrossAmount, transactionCurrency),
                TotalDiscount = new Money(header.DiscountAmount, transactionCurrency),
                NetAmount = new Money(header.NetAmount, transactionCurrency),
                ReceiptNumber = header.ReceiptNo,
                Terminal = new Terminal(header.POSTerminalNo),
            };

            foreach (NavWS.TransSalesEntry entry in root.TransSalesEntry)
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

            if (root.TransPaymentEntry != null && root.TransPaymentEntry.Length > 0)
            {
                int lineno = 1;
                foreach (NavWS.TransPaymentEntry pay in root.TransPaymentEntry)
                {
                    Payment payment = new Payment()
                    {
                        Amount = new Money(pay.AmountTendered, transactionCurrency),
                    };
                    transaction.TenderLines.Add(new PaymentLine(lineno++, payment));
                }
            }
            return transaction;
        }

        public ItemPriceCheckResponse MapFromRootToPriceCheck(NavWS.RootMobileTransaction root)
        {
            NavWS.MobileTransaction header = root.MobileTransaction.FirstOrDefault();

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
                foreach (NavWS.MobileTransactionLine line in root.MobileTransactionLine)
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

        #region Private

        private NavWS.MobileTransaction MobileTrans(RetailTransaction transaction)
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

            if (transaction.ManualDiscount?.Amount.Value > 0)
            {
                manualTotalDisAmount = transaction.ManualDiscount.Amount.Value;
                manualTotalDiscPercent = 0.0M;
            }
            else if (transaction.ManualDiscount?.Percentage > 0)
            {
                manualTotalDisAmount = 0.0M;
                manualTotalDiscPercent = transaction.ManualDiscount.Percentage;
            }

            return new NavWS.MobileTransaction()
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
                PriceGroupCode = string.Empty,
                CustomerId = (transaction.Customer == null) ? string.Empty : transaction.Customer.Id,
                CustDiscGroup = (transaction.Customer == null) ? string.Empty : transaction.Customer.TaxGroup,
                MemberCardNo = cardId,
                MemberPriceGroupCode = string.Empty,
                ManualTotalDiscAmount = manualTotalDisAmount,
                ManualTotalDiscPercent = manualTotalDiscPercent,
                DiningTblDescription = string.Empty,
                SalesType = string.Empty,
                RefundedFromStoreNo = transaction.RefundedFromStoreNo ?? string.Empty,
                RefundedFromPOSTermNo = transaction.RefundedFromTerminalNo ?? string.Empty,
                RefundedFromTransNo = ConvertTo.SafeInt(transaction.RefundedFromTransNo),
                RefundedReceiptNo = transaction.RefundedReceiptNo ?? string.Empty,
                SaleIsReturnSale = transaction.IsRefundByReceiptTransaction
            };
        }

        private NavWS.MobileTransactionLine MobileTransLine(string id, BaseLine line, Currency currency, string storeId, string orgStore, string orgTerminal, string orgTransNo)
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

                lineOrgNumber = LineNumberToNav(saleLine.OriginalTransactionLineNo);
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

                if (saleLine.ManualDiscount.Amount.Value > 0)
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

            NavWS.MobileTransactionLine tline = new NavWS.MobileTransactionLine()
            {
                Id = id,
                LineNo = LineNumberToNav(line.LineNumber),
                EntryStatus = (int)entryStatus,
                LineType = (int)lineType,
                Number = itemId,
                Barcode = barcode ?? string.Empty,
                CurrencyCode = currency.Id,
                CurrencyFactor = 1,
                VariantCode = variantId ?? string.Empty,
                UomId = uomId ?? string.Empty,
                NetPrice = netPrice,
                Price = price,
                Quantity = quantity,
                DiscountAmount = discountAmount,
                DiscountPercent = discountPercent,
                NetAmount = netAmount,
                TAXAmount = taxAmount,
                TAXProductCode = taxProductCode,
                TAXBusinessCode = string.Empty,
                ManualPrice = manualPrice,
                CardOrCustNo = string.Empty,
                ManualDiscountAmount = manualDiscountAmount,
                ManualDiscountPercent = manualDiscountPercent,
                EFTCardNumber = string.Empty,
                EFTCardName = string.Empty,
                EFTAuthCode = string.Empty,
                EFTMessage = string.Empty,
                ExternalId = externalId,
                RecommendedItem = fromRecommend,

                StaffId = string.Empty,
                StoreId = storeId,
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
                OrigTransLineNo = lineOrgNumber,
                OrigTransNo = ConvertTo.SafeInt(orgTransNo),
                OrigTransPos = orgTerminal ?? string.Empty,
                OrigTransStore = orgStore ?? string.Empty,
            };
            if (NavVersion > new Version("14.2"))
                tline.RetailImageID = string.Empty;
            return tline;
        }

        private NavWS.MobileTransactionLine MobileTransLine(string id, OrderLine line, string storeId)
        {
            NavWS.MobileTransactionLine tline = new NavWS.MobileTransactionLine()
            {
                Id = id,
                LineNo = LineNumberToNav(line.LineNumber),
                EntryStatus = (int)EntryStatus.Normal,
                LineType = (int)LineType.Item,
                Number = line.ItemId,
                StoreId = storeId,
                CurrencyCode = string.Empty,
                CurrencyFactor = 1,
                VariantCode = line.VariantId,
                UomId = line.UomId,
                NetPrice = line.NetPrice,
                Price = line.Price,
                Quantity = line.Quantity,
                DiscountAmount = line.DiscountAmount,
                DiscountPercent = line.DiscountPercent,
                NetAmount = line.NetAmount,
                TAXAmount = line.TaxAmount,
                RecommendedItem = false,

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
            };
            if (NavVersion > new Version("14.2"))
                tline.RetailImageID = string.Empty;
            return tline;
        }

        private NavWS.MobileTransactionLine MobileTransLine(string id, PublishedOffer offer, int lineNumber, string storeId)
        {
            NavWS.MobileTransactionLine tline = new NavWS.MobileTransactionLine()
            {
                Id = id,
                LineNo = lineNumber,
                EntryStatus = (int)EntryStatus.Normal,
                LineType = (int)LineType.Coupon,
                Number = string.Empty,
                Barcode = offer.OfferId,
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
                StoreId = storeId,
                PriceGroupCode = string.Empty,
                TerminalId = string.Empty,
                EFTTransactionNo = string.Empty,
                SalesType = string.Empty,
                CouponCode = offer.OfferId,
                ItemDescription = string.Empty,
                LineKitchenStatusCode = string.Empty,
                RestMenuTypeCode = string.Empty,
                TenderDescription = string.Empty,
                UomDescription = string.Empty,
                VariantDescription = string.Empty,
                OrigTransPos = string.Empty,
                OrigTransStore = string.Empty
            };
            if (NavVersion > new Version("14.2"))
                tline.RetailImageID = string.Empty;
            return tline;
        }

        private NavWS.MobileTransactionLine TransPaymentLine(string id, PaymentLine paymentLine, string transactionCurrencyCode, int lineNumber)
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

            NavWS.MobileTransactionLine tline = new NavWS.MobileTransactionLine()
            {
                Id = id,
                LineNo = LineNumberToNav(lineNumber),
                EntryStatus = entryStatus,
                LineType = (int)LineType.Payment,
                Number = paymentLine.Payment.TenderType.Id,
                CurrencyCode = currencyCode,
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
                OrigTransStore = string.Empty
            };
            if (NavVersion > new Version("14.2"))
                tline.RetailImageID = string.Empty;
            return tline;
        }

        private void MobileTransLine(NavWS.MobileTransactionLine mobileTransLine, ref RetailTransaction transaction, List<DiscountLine> discounts, NavWS.MobileReceiptInfo[] receiptInfos)
        {
            LineType lineType = (LineType)mobileTransLine.LineType;
            EntryStatus entryStatus = (EntryStatus)mobileTransLine.EntryStatus;
            int lineNumber = LineNumberFromNav(mobileTransLine.LineNo);
            Currency transLineCurrency = new UnknownCurrency(mobileTransLine.CurrencyCode);

            decimal quantity = mobileTransLine.Quantity;
            //when qty is negative then set the IsReturnLine to true
            bool isReturnLine = (quantity < 0M ? true : false);
            //and set the qty to abs value for MPOS
            quantity = Math.Abs(quantity);

            //getting perdiscount totaldiscount - ignore them
            if (lineType == LineType.Item)
            {
                SaleLine saleLine = new SaleLine(lineNumber);   //mpos expects to get 1 not 10000

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
                if (NavVersion > new Version("14.2"))
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
                //getting tenderlines
                PaymentLine paymentLine = new PaymentLine(mobileTransLine.LineNo);

                paymentLine.Id = mobileTransLine.LineNo.ToString();
                paymentLine.Payment.StoreId = transaction.Terminal?.Store?.Id;
                paymentLine.Payment.TerminalId = transaction.Terminal?.Id;

                paymentLine.Payment.StoreId = mobileTransLine.StoreId;
                paymentLine.Payment.TerminalId = mobileTransLine.TerminalId;
                paymentLine.Payment.StaffId = mobileTransLine.StaffId;
                paymentLine.Payment.TenderType = new TenderType(mobileTransLine.Number);

                paymentLine.Payment.Amount = new Money(mobileTransLine.NetAmount, transLineCurrency); //request sends in netamount

                paymentLine.Voided = entryStatus == EntryStatus.Voided;
                paymentLine.Payment.CardNumber = mobileTransLine.EFTCardNumber;
                paymentLine.Payment.AuthenticationCode = mobileTransLine.EFTAuthCode;
                paymentLine.Payment.CardType = mobileTransLine.EFTCardName;
                paymentLine.Payment.EFTMessage = mobileTransLine.EFTMessage;
                paymentLine.Payment.EFTTransactionId = mobileTransLine.EFTTransactionNo;
                paymentLine.Payment.VerificationMethod = (VerificationMethod)mobileTransLine.EFTVerificationMethod;

                paymentLine.Payment.AuthorizationStatus = (AuthorizationStatus)mobileTransLine.EFTAuthStatus;
                paymentLine.Payment.TransactionType = (PaymentTransactionType)mobileTransLine.EFTTransType;

                paymentLine.Payment.DateTime = mobileTransLine.EFTDateTime;

                paymentLine.Payment.DateTime = new DateTime(1970, 1, 1, 1, 0, 0);

                //we voided these when sent to NAV
                if (paymentLine.Payment.AuthorizationStatus != AuthorizationStatus.Approved && paymentLine.Payment.AuthorizationStatus != AuthorizationStatus.Unknown)
                {
                    paymentLine.Voided = false;
                }
                if (paymentLine.Payment.TransactionType != PaymentTransactionType.Purchase)
                {
                    paymentLine.Voided = false;
                }

                //get the mobile receipt info for the tenderline
                paymentLine.Payment.ReceiptInfo = ReceiptInfoLine(receiptInfos, mobileTransLine.LineNo);
                transaction.TenderLines.Add(paymentLine);
            }
        }

        private NavWS.MobileReceiptInfo ReceiptInfoLine(string id, ReceiptInfo receiptInfo, int lineNumber, string transactionNo)
        {
            NavWS.MobileReceiptInfo info = new NavWS.MobileReceiptInfo()
            {
                Id = id,
                Line_No = LineNumberToNav(lineNumber),
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
                if (NavVersion > new Version("14.2"))
                {
                    List<string> enc = new List<string>();
                    enc.Add(ConvertTo.Base64Encode(receiptInfo.ValueAsText));
                    info.LargeValue = enc.ToArray();
                    info.Value = string.Empty;
                }
                else
                {
                    info.Value = receiptInfo.ValueAsText.Substring(0, 99);
                }
            }
            else
            {
                info.Value = receiptInfo.ValueAsText;
            }
            return info;
        }

        private List<ReceiptInfo> ReceiptInfoLine(NavWS.MobileReceiptInfo[] mobileReceiptInfos, int lineNo)
        {
            //get MobileTransactionSubLine for this parent
            List<ReceiptInfo> receiptInfoList = new List<ReceiptInfo>();

            if (mobileReceiptInfos == null)
                return receiptInfoList;

            foreach (NavWS.MobileReceiptInfo receiptInfo in mobileReceiptInfos.Where(x => x.Line_No == lineNo))
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

        #endregion
    }
}
