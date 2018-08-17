using System;
using System.Collections.Generic;
using System.Linq;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Printer;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Retail.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Pos.Items;
using LSRetail.Omni.Domain.DataModel.Pos.Items.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Pos.Payments;
using LSRetail.Omni.Domain.DataModel.Pos.TenderTypes;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions.Discounts;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Mapping
{
    public class TransactionMapping1 : BaseMapping
    {
        public RetailTransaction MapFromRootToRetailTransaction(NavWS.Root9 root)
        {
            NavWS.MobileTransaction1 header = root.MobileTransaction.FirstOrDefault();
            UnknownCurrency transactionCurrency = new UnknownCurrency(header.CurrencyCode);

            RetailTransaction transaction = new RetailTransaction()
            {
                Id = header.Id,
                Voided = (EntryStatus)header.EntryStatus == EntryStatus.Voided,
                ReceiptNumber = header.ReceiptNo,
                TransactionNumber = header.TransactionNo.ToString(),
                BeginDateTime = header.TransDate,
                ManualDiscount = (header.ManualTotalDiscAmount > 0) ?
                    new Discount(transactionCurrency, header.ManualTotalDiscAmount, 0, DiscountEntryType.Amount) :
                    new Discount(transactionCurrency, 0, header.ManualTotalDiscPercent, DiscountEntryType.Percentage),
                IncomeExpenseAmount = new Money(header.IncomeExpAmount, transactionCurrency),
            };

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
                transaction.LoyaltyContact.Card = new Card(header.MemberCardNo);
            }

            decimal manualDiscPercent = header.ManualTotalDiscPercent;
            decimal manualDiscAmount = header.ManualTotalDiscAmount;

            if (manualDiscAmount > 0)
            {
                transaction.ManualDiscount = new Discount(transactionCurrency, manualDiscAmount, 0, DiscountEntryType.Amount);
            }
            else if (manualDiscPercent > 0)
            {
                transaction.ManualDiscount = new Discount(transactionCurrency, 0, manualDiscPercent, DiscountEntryType.Percentage);
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
            transaction.RefundedReceiptNo = header.RefundedReceiptNo;
            transaction.RefundedFromStoreNo = header.RefundedFromStoreNo;
            transaction.RefundedFromTerminalNo = header.RefundedFromPOSTermNo;
            transaction.RefundedFromTransNo = header.RefundedFromTransNo.ToString();

            //now loop thru the discount lines
            List<DiscountLine> discounts = new List<DiscountLine>();
            if (root.MobileTransDiscountLine != null)
            {
                foreach (NavWS.MobileTransDiscountLine1 mobileTransDisc in root.MobileTransDiscountLine)
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
            foreach (NavWS.MobileTransactionLine1 mobileTransLine in root.MobileTransactionLine)
            {
                MobileTransLine(mobileTransLine, ref transaction, discounts, root.MobileReceiptInfo);
            }
            return transaction;
        }

        public NavWS.Root9 MapFromRetailTransactionToRoot(RetailTransaction transaction)
        {
            NavWS.Root9 root = new NavWS.Root9();

            //MobileTrans
            List<NavWS.MobileTransaction1> trans = new List<NavWS.MobileTransaction1>();
            trans.Add(MobileTrans(transaction));
            root.MobileTransaction = trans.ToArray();

            //MobileTransLines
            List<NavWS.MobileTransactionLine1> transLines = new List<NavWS.MobileTransactionLine1>();
            List<NavWS.MobileReceiptInfo1> receiptLines = new List<NavWS.MobileReceiptInfo1>();

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

        #region Private

        private NavWS.MobileTransaction1 MobileTrans(RetailTransaction transaction)
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
                cardId = transaction.LoyaltyContact.Card.Id;
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

            return new NavWS.MobileTransaction1()
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
                RefundedReceiptNo = transaction.RefundedReceiptNo ?? string.Empty
            };
        }

        private NavWS.MobileTransactionLine1 MobileTransLine(string id, BaseLine line, Currency currency, string storeId, string orgStore, string orgTerminal, string orgTransNo)
        {
            decimal quantity = 0m;
            int externalId = 0;
            string eftTransactionNo = string.Empty;

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
                eftTransactionNo = incomeExpenseLine.TenderLineReference;
                entryStatus = (incomeExpenseLine.Voided) ? EntryStatus.Voided : EntryStatus.Normal;
                externalId = 0;
            }

            return new NavWS.MobileTransactionLine1()
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
        }

        private NavWS.MobileTransactionLine1 MobileTransLine(string id, PublishedOffer offer, int lineNumber, string storeId)
        {
            return new NavWS.MobileTransactionLine1()
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
                CouponCode = string.Empty,
                ItemDescription = string.Empty,
                LineKitchenStatusCode = string.Empty,
                RestMenuTypeCode = string.Empty,
                TenderDescription = string.Empty,
                UomDescription = string.Empty,
                VariantDescription = string.Empty,
                OrigTransPos = string.Empty,
                OrigTransStore = string.Empty
            };
        }

        private NavWS.MobileTransactionLine1 TransPaymentLine(string id, PaymentLine paymentLine, string transactionCurrencyCode, int lineNumber)
        {
            string currencyCode = string.Empty;
            decimal currencyFactor = 0;

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
                eftAuthCode = paymentLine.Payment.AuthenticationCode;
                eftCardNumber = paymentLine.Payment.CardNumber;
                eftCardName = paymentLine.Payment.CardType;
                eftMessage = paymentLine.Payment.EFTMessage;
                eftTransactionId = paymentLine.Payment.EFTTransactionId;
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

            return new NavWS.MobileTransactionLine1()
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
        }

        private void MobileTransLine(NavWS.MobileTransactionLine1 mobileTransLine, ref RetailTransaction transaction, List<DiscountLine> discounts, NavWS.MobileReceiptInfo1[] receiptInfos)
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

        private NavWS.MobileReceiptInfo1 ReceiptInfoLine(string id, ReceiptInfo receiptInfo, int lineNumber, string transactionNo)
        {
            return new NavWS.MobileReceiptInfo1()
            {
                Id = id,
                Line_No = LineNumberToNav(lineNumber),
                Key = receiptInfo.Key,
                Type = receiptInfo.Type,
                Transaction_No = string.IsNullOrEmpty(transactionNo) ? 0 : ConvertTo.SafeInt(transactionNo),
                Value = string.Empty,
            };
        }

        private List<ReceiptInfo> ReceiptInfoLine(NavWS.MobileReceiptInfo1[] mobileReceiptInfos, int lineNo)
        {
            //get MobileTransactionSubLine for this parent
            List<ReceiptInfo> receiptInfoList = new List<ReceiptInfo>();

            if (mobileReceiptInfos == null)
                return receiptInfoList;

            foreach (NavWS.MobileReceiptInfo1 receiptInfo in mobileReceiptInfos.Where(x => x.Line_No == lineNo))
            {
                ReceiptInfo ri = new ReceiptInfo();
                ri.IsLargeValue = false;
                ri.Key = receiptInfo.Key;
                ri.Value = new List<PrintLine>();
                ri.Value.Add(new PrintLine()
                {
                    Text = receiptInfo.Value
                });
                ri.Type = receiptInfo.Type;
                receiptInfoList.Add(ri);
            }
            return receiptInfoList;
        }

        #endregion
    }
}
