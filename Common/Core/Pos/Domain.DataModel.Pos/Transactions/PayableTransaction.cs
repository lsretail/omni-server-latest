using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Pos.Payments;
using LSRetail.Omni.Domain.DataModel.Pos.TenderTypes;
using LSRetail.Omni.Domain.DataModel.Pos.TenderTypes.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions.Exceptions;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public enum TransactionStatus
    {
        [EnumMember]
        Normal = 0,
        [EnumMember]
        UnderTendered = 1,
        [EnumMember]
        CanBePosted = 2,
        [EnumMember]
        OverTendered = 3
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public abstract class PayableTransaction : Entity, IAggregateRoot
    {
        [DataMember]
        public Terminal Terminal { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? BeginDateTime { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? EndDateTime { get; set; }
        [DataMember]
        public string TransactionNumber { get; set; }

        [DataMember]
        public bool CouldHaveLostPaymentData { get; set; } = false;       //if transaction was recovered, a payment may have been dropped
        [DataMember]
        public string ReceiptNumber { get; set; }
        [DataMember]
        public Customer Customer { get; set; }

        [DataMember]
        public Money PaymentAmount { get; set; }
        [DataMember]
        public Money BalanceAmount { get; set; }

        [DataMember]
        public List<PaymentLine> TenderLines { get; set; }

        [DataMember]
        public bool IsDirty { get; set; }
        [DataMember]
        public bool Voided { get; set; }
        [IgnoreDataMember]
        public int NumberOfTenderLines => TenderLines.Count;

        [DataMember]
        public Money GrossAmount { get; set; }
        [DataMember]
        public Money NetAmount { get; set; }

        // For serialization only
        internal PayableTransaction()
        {
            TenderLines = new List<PaymentLine>();
            PaymentAmount = new Money();
            BalanceAmount = new Money();
            GrossAmount = new Money();
            NetAmount = new Money();
        }

        public PayableTransaction(Terminal terminal)
        {
            ReceiptNumber = string.Empty;
            PaymentAmount = new Money(0M, terminal.Store.Currency);
            BalanceAmount = new Money(0M, terminal.Store.Currency);
            Customer = null;
            TenderLines = new List<PaymentLine>();
            Voided = false;
            TransactionNumber = string.Empty;
            Terminal = terminal;
            BeginDateTime = DateTime.Now;
        }

        public void AddTenderLine(Payment payment)
        {
            if (payment.TenderType.Function == TenderTypeFunction.Cards)
            {
                //we only validate approved and non void PED transactions
                if (payment.AuthorizationStatus == AuthorizationStatus.Approved && payment.TransactionType != PaymentTransactionType.Reversal)
                {
                    payment.ValidatePayment(BalanceAmount);
                }
            }
            else
            {
                payment.ValidatePayment(BalanceAmount);
            }
			payment.StoreId = Terminal.Store.Id;
            PaymentLine tenderLine = new PaymentLine(NumberOfTenderLines + 1, payment);
            TenderLines.Add(tenderLine);

            Calculate(false);
        }

        public void AddCurrencyTenderLine(Payment payment)
        {
            decimal balanceAmountInRelevantCurrencyValue = payment.Amount.RoundToUnit(BalanceAmount.Value / payment.CurrencyExchRate.CurrencyFactor, payment.Amount.Currency.RoundOffSales, payment.Amount.Currency.SaleRoundingMethod);
            Money balanceAmountInRelevantCurrency = new Money(balanceAmountInRelevantCurrencyValue, payment.Amount.Currency);

            //Validate the payment with the balance amount in the same currency as the payment
            payment.ValidatePayment(balanceAmountInRelevantCurrency);

            PaymentLine tenderLine = new PaymentLine(NumberOfTenderLines + 1, payment);
            TenderLines.Add(tenderLine);

            Calculate(false);
        }

        public void AddPointTenderLine(Payment payment)
        {
            decimal balanceAmountInRelevantCurrencyValue = payment.Amount.RoundToUnit(BalanceAmount.Value / payment.CurrencyExchRate.CurrencyFactor, payment.Amount.Currency.RoundOffSales, payment.Amount.Currency.SaleRoundingMethod);
            Money balanceAmountInRelevantCurrency = new Money(balanceAmountInRelevantCurrencyValue, payment.Amount.Currency);

            //Validate the payment with the balance amount in the same currency as the payment
            payment.ValidatePayment(balanceAmountInRelevantCurrency);

            PaymentLine tenderLine = new PaymentLine(NumberOfTenderLines + 1, payment);
            TenderLines.Add(tenderLine);

            Calculate(false);
        }

        public void AddVoidedTenderLine(Payment payment)
        {
            // Don't need to validate that the payment can be added.
            // Tenderlines can't be un-voided. TODO Verify that they can't be unvoided.
            PaymentLine tenderLine = new PaymentLine(NumberOfTenderLines + 1, payment);
            tenderLine.Voided = true;
            TenderLines.Add(tenderLine);

            Calculate(false);
        }

        public void VoidTenderLine(int lineNumber)
        {
            PaymentLine line = TenderLines.Find(delegate (PaymentLine s) { return s.LineNumber == lineNumber; });
            line.Voided = true;
            Calculate(false);
        }

        private bool ContainsUnvoidedTenderLines()
        {
            foreach (PaymentLine line in TenderLines)
            {
                if (line.Voided == false)
                {
                    if (line.Payment.TenderType.Function == TenderTypeFunction.Cards)
                    {
                        //We don't have to void tender lines that are declined/rejected or void lines
                        if (line.Payment.AuthorizationStatus == AuthorizationStatus.Approved &&
                           (line.Payment.TransactionType == PaymentTransactionType.Purchase || line.Payment.TransactionType == PaymentTransactionType.Refund))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void AddCustomer(Customer customer, bool allowCalculation = true)
        {
            if (Customer != null && Customer.Id == customer.Id)
            {
                this.Customer = customer;
            }
            else
            {
                this.Customer = customer;

                if (allowCalculation)
                {
                    Calculate(true);
                }
            }

            IsDirty = true;
        }

        public void ClearCustomer()
        {
            Customer = null;
            Calculate(true);
        }

        public void VoidTransaction()
        {
            // The transaction cannot be voided if it contains any unvoided payment lines
            if (ContainsUnvoidedTenderLines() == true)
            {
                throw new VoidingDueToTenderlinesExceptions();
            }
            Voided = true;
        }

        public TransactionStatus GetStatus(bool isRefund)
        {
            // Transactions that are voided can be posted
            if (Voided)
            {
                return TransactionStatus.CanBePosted;
            }

            //Omni-3447 on Hold
            PaymentLine lastTenderLine = TenderLines?.LastOrDefault(x => !x.Voided);
            if (lastTenderLine != null)
            {
                decimal amountToPay = GrossAmount.RoundToUnit(lastTenderLine.Payment.TenderType.RoundOff, lastTenderLine.Payment.TenderType.RoundOffType);
                if (amountToPay == PaymentAmount.Value)
                {
                    return TransactionStatus.CanBePosted;
                }
            }

            if (isRefund)
            {
                if (BalanceAmount.Value < 0)
                {
                    return TransactionStatus.UnderTendered;
                }
                else if (BalanceAmount.Value > 0)
                {
                    return TransactionStatus.OverTendered;
                }
            }
            else
            {
                if (BalanceAmount.Value > 0)
                {
                    return TransactionStatus.UnderTendered;
                }
                else if (BalanceAmount.Value < 0)
                {
                    return TransactionStatus.OverTendered;
                }
            }

            return TransactionStatus.CanBePosted;
        }

        // Returns an instance of the Payment class that represents the change back
        public Payment GetChangeBackPayment()
        {
            // Check the properties of the last tendertype added to the transaction so see which tender type the 
            // change back should be

            Payment lastPayment = ((PaymentLine)TenderLines[NumberOfTenderLines - 1]).Payment;
            TenderType lastTenderType = lastPayment.TenderType;
            TenderType changeBackTenderType = lastTenderType.ChangeTenderType;

            if (changeBackTenderType.ForeignCurrency && lastPayment.CurrencyExchRate != null && lastPayment.CurrencyExchRate.CurrencyFactor != 1)
            {
                Payment returnPayment = new Payment();

                Money money = new Money(BalanceAmount.Value / lastPayment.CurrencyExchRate.CurrencyFactor, lastPayment.Amount.Currency);
                decimal changeBackAmount = money.RoundToUnit(money.Value, money.Currency.RoundOffSales, money.Currency.SaleRoundingMethod);

                if (lastTenderType.AboveMinimumChangeTenderType is UnknownTenderType == false)
                {
                    // An above minimum change tender type is defined.
                    // Let's check if the change back amount excceds the minimum amout and then
                    // change the cash back tender type if so
                    if (changeBackAmount > lastTenderType.MinimumChangeAmount)
                    {
                        changeBackTenderType = lastTenderType.AboveMinimumChangeTenderType;

                        if (changeBackTenderType.ForeignCurrency == false)
                        {
                            returnPayment.Amount = new Money(BalanceAmount.Value, Terminal.Store.Currency);
                            returnPayment.TenderType = changeBackTenderType;

                            return returnPayment;
                        }
                    }
                }

                // Construct the Payment to return
                returnPayment.Amount.Currency = lastPayment.Amount.Currency;
                returnPayment.CurrencyExchRate = lastPayment.CurrencyExchRate;
                returnPayment.Amount = new Money(changeBackAmount, lastPayment.Amount.Currency);
                returnPayment.TenderType = changeBackTenderType;

                return returnPayment;
            }
            else
            {
                //Omni-3447 on Hold
                decimal amountToPay = GrossAmount.RoundToUnit(lastPayment.TenderType.RoundOff, lastPayment.TenderType.RoundOffType);

                Payment returnPayment = new Payment();
                //decimal changeBackAmount = balanceAmount.Value;     //Omni-3447 on Hold
                decimal changeBackAmount = amountToPay - PaymentAmount.Value;

                if (lastTenderType.AboveMinimumChangeTenderType is UnknownTenderType == false)
                {
                    // An above minimum change tender type is defined.
                    // Let's check if the change back amount excceds the minimum amout and then
                    // change the cash back tender type if so
                    if (changeBackAmount > lastTenderType.MinimumChangeAmount)
                        changeBackTenderType = lastTenderType.AboveMinimumChangeTenderType;
                }

                // Construct the Payment to return
                returnPayment.Amount = new Money(changeBackAmount, Terminal.Store.Currency);
                returnPayment.TenderType = changeBackTenderType;

                return returnPayment;
            }
        }

        public virtual void Calculate(bool calculateSaleLines = true)
        {
            PaymentAmount.Value = 0M;
            BalanceAmount.Value = 0M;

            foreach (PaymentLine tenderLine in TenderLines)
            {
                if (tenderLine.Voided == false)
                {
                    if (tenderLine.Payment.CurrencyExchRate != null && tenderLine.Payment.CurrencyExchRate.CurrencyFactor != 1)
                    {
                        Money localMoney = new Money(tenderLine.Payment.Amount.Value * tenderLine.Payment.CurrencyExchRate.CurrencyFactor, Terminal.Store.Currency);
                        decimal localAmount = localMoney.RoundToUnit(localMoney.Value, localMoney.Currency.RoundOffSales, localMoney.Currency.SaleRoundingMethod);

                        PaymentAmount.Value += localAmount;
                    }
                    else if (tenderLine.Payment.TenderType.Function == TenderTypeFunction.Cards)
                    {
                        if (tenderLine.Payment.AuthorizationStatus == AuthorizationStatus.Approved && (tenderLine.Payment.TransactionType == PaymentTransactionType.Purchase))
                        {
                            PaymentAmount.Value += tenderLine.Payment.Amount.Value;
                        }
                    }
                    else
                    {
                        PaymentAmount.Value += tenderLine.Payment.Amount.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if payment can be added. Does it exceed the transactions over-/undertendering limitations?
        /// </summary>
        /// <param name="payment">Payment.</param>
        public void CheckIfPaymentCanBeAdded(Payment payment)
        {
            Money tmpBalanceAmount = new Money(BalanceAmount.Value, BalanceAmount.Currency);
            tmpBalanceAmount.Value = BalanceAmount.Value < 0 ? BalanceAmount.Value * -1 : BalanceAmount.Value;
            payment.ValidatePayment(tmpBalanceAmount);
        }
    }
}
