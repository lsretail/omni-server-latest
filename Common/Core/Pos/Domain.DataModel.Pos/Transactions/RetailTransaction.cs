using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Retail.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Pos.Items;
using LSRetail.Omni.Domain.DataModel.Pos.Items.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions.Calculators.Price;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions.Calculators.Tax;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions.Discounts;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions.Exceptions;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class RetailTransaction : PayableTransaction
    {
        [DataMember]
        public SourceType SourceType { get; set; }
        [DataMember]
        public MemberContact LoyaltyContact { get; set; }

        [IgnoreDataMember]
        public PriceCalculator PriceCalculator { get; set; }
        [IgnoreDataMember]
        public TaxCalculator TaxCalculator { get; set; }
        [IgnoreDataMember]
        public TransactionCalculator TransactionCalculator { get; set; }

        [DataMember]
        public Money IncomeExpenseAmount { get; set; }

        [DataMember]
        public Money TaxAmount { get; set; }
        [DataMember]
        public Discount ManualDiscount { get; set; }
        [DataMember]
        public Money TotalDiscount { get; set; }
        [DataMember]
        public Money HeadDiscount { get; set; }

        [DataMember]
        public bool Posted { get; set; }
        [DataMember]
        public bool OfflinePosted { get; set; }

        [DataMember]
        public int NumberOfItems { get; set; }
        [IgnoreDataMember]
        public int NumberOfTransactionLines { get { return SaleLines.Count + IncomeExpenseLines.Count; } }
        [DataMember]
        public bool RemoteCalculationNeeded { get; set; }
        [DataMember]
        public bool ReturnNextSaleLine { get; set; }	// Next sale line added should be a return sale line (negative)
        [DataMember]
        public List<SaleLine> SaleLines { get; set; }
        [DataMember]
        public List<IncomeExpenseLine> IncomeExpenseLines { get; set; }

        //return by receipt fields
        [DataMember]
        public string RefundedReceiptNo { get; set; } = "";
        [DataMember]
        public string RefundedFromStoreNo { get; set; } = "";
        [DataMember]
        public string RefundedFromTerminalNo { get; set; } = "";
        [DataMember]
        public string RefundedFromTransNo { get; set; } = "";

        [IgnoreDataMember]
        public int NumberOfSaleLines { get { return SaleLines.Count; } }

        [IgnoreDataMember]
        public bool IsRefundByReceiptTransaction
        {
            get { return !string.IsNullOrEmpty(RefundedReceiptNo); }
        }

        public RetailTransaction() : base()
        {
            TaxAmount = new Money();
            IncomeExpenseAmount = new Money();
            HeadDiscount = new Money();
            TotalDiscount = new Money();

            SaleLines = new List<SaleLine>();
            IncomeExpenseLines = new List<IncomeExpenseLine>();
        }

        public RetailTransaction(RetailTransaction originalTransaction, string id, string transactionNumber, string receiptNumber, DateTime transactionDate, Discount manualDiscount, string loyaltyCardNumber, string customerId, List<SaleLine> saleLines, List<IncomeExpenseLine> incomeExpenseLines, List<PaymentLine> tenderLines, bool isDirty,
            decimal netAmount, decimal grossAmount, decimal taxAmount, decimal incomeExpenseAmount, decimal lineDiscount, decimal totalDiscount)
            : base(originalTransaction.Terminal)
        {
            this.PriceCalculator = originalTransaction.PriceCalculator;
            this.TaxCalculator = originalTransaction.TaxCalculator;

            this.NetAmount = new Money(0M, this.Terminal.Store.Currency);
            this.GrossAmount = new Money(0M, this.Terminal.Store.Currency);

            this.HeadDiscount = new Money(0M, this.Terminal.Store.Currency);
            this.TotalDiscount = new Money(0M, this.Terminal.Store.Currency);

            this.TaxAmount = new Money(0M, this.Terminal.Store.Currency);

            this.IncomeExpenseAmount = new Money(0M, this.Terminal.Store.Currency);

            #region Base Transaction

            Id = id;
            TransactionNumber = transactionNumber;
            ReceiptNumber = receiptNumber;
            BeginDateTime = transactionDate;
            this.TotalDiscount = new Money(totalDiscount, this.Terminal.Store.Currency);
            this.ManualDiscount = manualDiscount;
            IsDirty = isDirty;

            #endregion

            #region Loy contact and customer

            if (originalTransaction.LoyaltyContact != null)
            {
                this.LoyaltyContact = originalTransaction.LoyaltyContact;
            }

            if (originalTransaction.Customer != null)
            {
                this.Customer = originalTransaction.Customer;
            }

            if (string.IsNullOrEmpty(loyaltyCardNumber) == false)
            {
                if (this.LoyaltyContact == null || this.LoyaltyContact.Card.Id != loyaltyCardNumber)
                {
                    this.LoyaltyContact = new UnknownMemberContact(loyaltyCardNumber);
                }
            }
            else if (string.IsNullOrEmpty(customerId) == false)
            {
                if (this.Customer == null || this.Customer.Id != customerId)
                {
                    this.Customer = new UnknownCustomer(customerId);
                }
            }

            #endregion

            #region Sale lines

            this.SaleLines = saleLines;
            this.IncomeExpenseLines = incomeExpenseLines;

            #endregion

            #region Tender lines

            this.TenderLines = tenderLines;

            #endregion

            Calculate();

            this.TaxAmount.Value = taxAmount;
            this.IncomeExpenseAmount.Value = incomeExpenseAmount;
        }

        // For database restoring only
        public RetailTransaction(Discount manualDiscount, Customer customer, Money netAmount, Money grossAmount, Money incomeExpenseAmount, Money balanceAmount, Money headDiscount, Money totalDiscount, Money paymentAmount, Money taxAmount, int numberOfItems)
            : base()
        {
            this.SaleLines = new List<SaleLine>();
            this.IncomeExpenseLines = new List<IncomeExpenseLine>();
            this.ManualDiscount = manualDiscount;
            this.Customer = customer;
            this.NetAmount = netAmount;
            this.GrossAmount = grossAmount;
            this.IncomeExpenseAmount = incomeExpenseAmount;
            this.NumberOfItems = numberOfItems;
            this.BalanceAmount = balanceAmount;
            this.HeadDiscount = headDiscount;
            this.TotalDiscount = totalDiscount;
            this.PaymentAmount = paymentAmount;
            this.TaxAmount = taxAmount;
        }

        public RetailTransaction(RetailTransaction transToCopy)
            : this(transToCopy.Terminal, transToCopy.PriceCalculator, transToCopy.TaxCalculator)
        {
            if (transToCopy.LoyaltyContact != null)
            {
                this.LoyaltyContact = transToCopy.LoyaltyContact;
            }

            if (transToCopy.Customer != null)
            {
                this.Customer = transToCopy.Customer;
            }
        }

        public RetailTransaction(Terminal terminal, PriceCalculator priceCalculator, TaxCalculator taxCalculator)
            : base(terminal)
        {
            this.PriceCalculator = priceCalculator;
            this.TaxCalculator = taxCalculator;

            this.NetAmount = new Money(0M, this.Terminal.Store.Currency);
            this.GrossAmount = new Money(0M, this.Terminal.Store.Currency);
            this.TaxAmount = new Money(0M, this.Terminal.Store.Currency);

            this.ManualDiscount = new Discount(terminal.Store.Currency, 0, 0, DiscountEntryType.Amount);
            this.TotalDiscount = new Money(0M, this.Terminal.Store.Currency);
            this.HeadDiscount = new Money(0M, this.Terminal.Store.Currency);

            this.IncomeExpenseAmount = new Money(0M, this.Terminal.Store.Currency);

            this.SaleLines = new List<SaleLine>();
            this.IncomeExpenseLines = new List<IncomeExpenseLine>();
        }

        public void AddSaleLine(RetailItem item, decimal quantity, int originalTransactionLineNo = 0, bool isFromRecommendation = false)
        {
            // Throw dedicated exceptions for e.g. item blocked, zero price not allowed, must key in price, etc...

            // Check if item was found
            if (item is UnknownRetailItem)
                throw new ItemNotFoundException(item);

            // Check if item is blocked
            if (item.Blocked == true)
                throw new ItemBlockedException(item);

            // TODO
            // Do other necessary checks prior to adding the item to the transaction

            if (quantity < 0)
            {
                quantity = Math.Abs(quantity);
                ReturnNextSaleLine = true;
            }

            SaleLine line = new SaleLine(this.NumberOfTransactionLines + 1, item, quantity, this.Terminal.Store.Currency, PriceCalculator, this.ReturnNextSaleLine);
            line.OriginalTransactionLineNo = originalTransactionLineNo;
            line.FromRecommendation = isFromRecommendation;

            this.SaleLines.Add(line);

            this.ReturnNextSaleLine = false;

            Calculate();

            // Check if there is already a total discount on the transaction, that we then need to apply
            // to this new item as well.
            CheckIfManualDiscountApplies();

            IsDirty = true;
        }

        public SaleLine GetSaleLine(int lineNr)
        {
            return SaleLines[lineNr - 1];
        }

        public void AddIncomeExpenseLine(string accountNumber, decimal amount, string tenderLineReference)
        {
            IncomeExpenseLine line = new IncomeExpenseLine(this.NumberOfTransactionLines + 1, accountNumber, amount, tenderLineReference, this.Terminal.Store.Currency);
            this.IncomeExpenseLines.Add(line);

            Calculate();

            IsDirty = true;
        }

        public BaseLine GetTransactionLine(int lineNr)
        {
            BaseLine baseLine;

            baseLine = SaleLines.Find(line => line.LineNumber == lineNr);

            if (baseLine != null)
                return baseLine;
            else
                return IncomeExpenseLines.Find(line => line.LineNumber == lineNr);
        }

        public void IncrementQty(int lineNumber)
        {
            SaleLine line = SaleLines.Find(delegate (SaleLine s) { return s.LineNumber == lineNumber; });
            line.IncrementQty();

            Calculate();
            CheckIfManualDiscountApplies();

            IsDirty = true;
        }

        public void DecrementQty(int lineNumber)
        {
            SaleLine line = SaleLines.Find(delegate (SaleLine s) { return s.LineNumber == lineNumber; });
            if (line.Quantity > 1)
                line.DecrementQty();

            Calculate();
            CheckIfManualDiscountApplies();

            IsDirty = true;
        }

        public void SetQty(int lineNumber, decimal qty)
        {
            SaleLine line = SaleLines.Find(delegate (SaleLine s) { return s.LineNumber == lineNumber; });
            line.SetQty(qty);

            Calculate();
            CheckIfManualDiscountApplies();

            IsDirty = true;
        }

        public void SetUOM(int lineNumber, UnitOfMeasure uom)
        {
            SaleLine line = SaleLines.Find(delegate (SaleLine s) { return s.LineNumber == lineNumber; });
            line.SetUOM(uom);

            Calculate();
            CheckIfManualDiscountApplies();

            IsDirty = true;
        }

        public void PriceOverride(int lineNumber, decimal price)
        {
            SaleLine line = SaleLines.Find(delegate (SaleLine s) { return s.LineNumber == lineNumber; });
            line.PriceOverride(price);

            Calculate();
            CheckIfManualDiscountApplies();

            IsDirty = true;
        }

        public void VoidLine(int lineNumber)
        {
            SaleLine line = SaleLines.Find(delegate (SaleLine s) { return s.LineNumber == lineNumber; });

            if (line.Voided == true)
                line.Unvoid();
            else
                line.Void();

            Calculate();
            CheckIfManualDiscountApplies();
            IsDirty = true;
        }

        public void SetManualDiscountPercentage(int lineNumber, decimal percentage)
        {
            if (percentage > 100)
                throw new LineDiscountPercentageTooHighException(percentage);

            SaleLine line = SaleLines.Find(delegate (SaleLine s) { return s.LineNumber == lineNumber; });
            line.SetManualDiscount(0, percentage, DiscountEntryType.Percentage);

            Calculate();
            CheckIfManualDiscountApplies();

            IsDirty = true;
        }

        public void SetManualDiscountAmount(int lineNumber, decimal amount)
        {
            SaleLine line = SaleLines.Find(delegate (SaleLine s) { return s.LineNumber == lineNumber; });

            // The line discount can never be higher than the gross amount minus the sum of all automatic discounts
            // originating from the external calculation engine (mix and match, etc...) minus the total discount currently on the line
            if (amount > line.GrossAmount.Value - line.LineDiscount.Amount.Value)
                throw new LineDiscountHigherThanSaleLineAmountException();

            line.SetManualDiscount(amount, 0, DiscountEntryType.Amount);

            Calculate();
            CheckIfManualDiscountApplies();

            IsDirty = true;
        }

        public void SetManualDiscountPercentage(decimal percentage, bool calculateTransaction = true)
        {
            if (percentage > 100)
                throw new TotalDiscountPercentageTooHighException(percentage);
            if ((GrossAmount.Value == 0) && (this.SaleLines.Count == 0))
                throw new TotalDiscountZeroGrossAmountException();

            if (GrossAmount.Value > 0)
                this.ManualDiscount = new Discount(this.Terminal.Store.Currency, 0, percentage, DiscountEntryType.Percentage);
            else
                // There is no gross amount to discount, therefore we null the discount
                this.ManualDiscount = new Discount(this.Terminal.Store.Currency, 0, 0, DiscountEntryType.Percentage);

            if (calculateTransaction == true)
                Calculate();

            IsDirty = true;
        }

        public void SetManualDiscountAmount(decimal amount, bool calculateTransaction, bool allowTotalDiscountAmountHigherThanBalance = true)
        {
            decimal amountNotAllowedToSetDiscount = 0;
            decimal allowedTotalDiscountAmount = this.BalanceAmount.Value + this.ManualDiscount.Amount.Value;

            foreach (SaleLine saleLine in SaleLines)
            {
                if (saleLine.Item.DiscountAllowed == false && saleLine.Voided == false)
                {
                    amountNotAllowedToSetDiscount += saleLine.Item.Price.Value;
                }
            }

            allowedTotalDiscountAmount -= amountNotAllowedToSetDiscount;
            if (allowTotalDiscountAmountHigherThanBalance == false && amount > allowedTotalDiscountAmount && amount != 0)
            {
                throw new TotalDiscountHighterThanTransactionBalanceException();
            }

            decimal totalDiscountPercentage;
            if (GrossAmount.Value > 0)
            {
                // We have the amount, let's figure out the percentage of the gross amount and 
                // apply this percentage to each line.
                ManualDiscount = new Discount(null, amount, 0, DiscountEntryType.Amount);
                totalDiscountPercentage = amount / (GrossAmount.Value - amountNotAllowedToSetDiscount);
            }
            else
            {
                // There is no gross amount to discount, therefore we null the discount
                ManualDiscount = new Discount(null, 0, 0, DiscountEntryType.Amount);
                totalDiscountPercentage = 0;
            }

            if (calculateTransaction)
            {
                Calculate();
            }
            IsDirty = true;
        }

        public void AddLoyaltyContact(MemberContact loyaltyContact, bool allowCalculation = true)
        {
            // If it is the same contact, no need to mark as Dirty
            if (LoyaltyContact != null && LoyaltyContact.Id == loyaltyContact.Id)
            {
                this.LoyaltyContact = loyaltyContact;
            }
            else
            {
                this.LoyaltyContact = loyaltyContact;
                if (allowCalculation)
                {
                    Calculate();
                    IsDirty = true;
                }
            }
        }

        public void RemoveLoyaltyContact()
        {
            this.LoyaltyContact = null;
            Calculate();
            IsDirty = true;
        }

        public bool IsEmpty()
        {
            if ((NumberOfSaleLines == 0) && (Customer == null) && (LoyaltyContact == null))
                return true;
            else
                return false;
        }

        public void CanTransactionBeSuspendedRemotely()
        {
            // If the transaction contains no sale lines then there is nothing to suspend
            if (this.SaleLines.Count < 1)
                throw new SuspendWithNoSaleLinesException();
        }

        public void ImportExternalValues()
        {
            Calculate();
            IsDirty = true;
        }

        public override void Calculate(bool calculateSaleLines = true)
        {
            base.Calculate();
            this.NumberOfItems = 0;

            foreach (SaleLine saleLine in SaleLines)
            {
                if (saleLine.Voided == false)
                {
                    NumberOfItems += Convert.ToInt32(saleLine.Quantity);
                }
            }

            if (calculateSaleLines)
            {
                /*this.NetAmount.Value = 0M;
                this.GrossAmount.Value = 0M;
                this.TaxAmount.Value = 0;
                this.IncomeExpenseAmount.Value = 0M;
                this.HeadDiscount.Value = 0;
                this.TotalDiscount.Value = 0;

                foreach (SaleLine saleLine in this.SaleLines)
                {
                    if (saleLine.Voided == false)
                    {
                        saleLine.Calculate(TaxCalculator, this);

                        if (saleLine.IsReturnLine)
                        {
                            GrossAmount.Value -= saleLine.GrossAmount.Value;
                            NetAmount.Value -= saleLine.NetAmount.Value;
                            TotalDiscount.Value -= saleLine.LineDiscount.Amount.Value;
                            TaxAmount.Value -= saleLine.TaxAmount.Value;
                        }
                        else
                        {
                            GrossAmount.Value += saleLine.GrossAmount.Value;
                            NetAmount.Value += saleLine.NetAmount.Value;
                            TotalDiscount.Value += saleLine.LineDiscount.Amount.Value;
                            TaxAmount.Value += saleLine.TaxAmount.Value;
                        }
                    }
                }

                foreach (IncomeExpenseLine incomeExpenseLine in this.IncomeExpenseLines)
                {
                    if (incomeExpenseLine.Voided == false)
                    {
                        IncomeExpenseAmount.Value += incomeExpenseLine.Amount.Value;
                        GrossAmount.Value += incomeExpenseLine.Amount.Value;
                        NetAmount.Value += incomeExpenseLine.Amount.Value;
                    }
                }

                HeadDiscount.Value += ManualDiscount.Amount.Value;
                TotalDiscount.Value += ManualDiscount.Amount.Value;*/

                TransactionCalculator.TransactionCalc(this);
            }

            BalanceAmount.Value = GrossAmount.Value - PaymentAmount.Value;

            // We round the balance to currency decimals since we had case where if you had something that costs 900 and gave 150 as total discount
            // then the calculation engine discturbutes the discount on the items in then calculates to % and then back to amount, that would cause
            // rounding issue of 0.000000003, but of course this should not be like that but until we get better calculation mechanishm then we are
            // a bit stuck with that.
            BalanceAmount.Value = BalanceAmount.RoundToUnit(BalanceAmount.Value, Terminal.Store.Currency.RoundOffSales, Terminal.Store.Currency.SaleRoundingMethod);
        }

        protected void CheckIfManualDiscountApplies()
        {
            if ((ManualDiscount.EntryType == DiscountEntryType.Amount) && (ManualDiscount.Amount.Value > 0))
                SetManualDiscountAmount(ManualDiscount.Amount.Value, true);
            else if ((ManualDiscount.EntryType == DiscountEntryType.Percentage) && (ManualDiscount.Percentage > 0))
                SetManualDiscountPercentage(ManualDiscount.Percentage, true);
        }
    }
}
