using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Pos.Currencies;
using LSRetail.Omni.Domain.DataModel.Pos.Items;
using LSRetail.Omni.Domain.DataModel.Pos.Payments.Exception;
using LSRetail.Omni.Domain.DataModel.Pos.TenderTypes;

namespace LSRetail.Omni.Domain.DataModel.Pos.Payments
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public enum AuthorizationStatus
    {
        [EnumMember]
        Approved = 0,
        [EnumMember]
        Declined = 1,
        [EnumMember]
        Cancelled = 2,
        [EnumMember]
        Failure = 3,
        [EnumMember]
        UserRejected = 4,
        [EnumMember]
        Voided = 5,
        [EnumMember]
        Unknown = 6,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public enum PaymentTransactionType
    {
        [EnumMember]
        Purchase = 0,
        [EnumMember]
        Reversal = 1,
        [EnumMember]
        Refund = 2
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public enum VerificationMethod
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Signature = 1,
        [EnumMember]
        OnlinePIN = 2,
        [EnumMember]
        OfflinePIN = 3,
        [EnumMember]
        OnlinePINAndSignature = 4,
        [EnumMember]
        OfflinePINAndSignature = 5,
        [EnumMember]
        Unknown = 6
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class Payment
    {
        [DataMember]
        public TenderType TenderType { get; set; }
        [DataMember]
        public Money Amount { get; set; }
        [DataMember]
        public DateTime DateTime { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string TerminalId { get; set; }
        [DataMember]
        public string StaffId { get; set; }
        [DataMember]
        public CurrencyExchRate CurrencyExchRate { get; set; }
        [DataMember]
        public string AuthenticationCode { get; set; }
        [DataMember]
        public string CardNumber { get; set; }
        [DataMember]
        public string CardType { get; set; }
        [DataMember]
        public string EFTMessage { get; set; }
        [DataMember]
        public string EFTTransactionId { get; set; }
        [DataMember]
        public string TransactionId { get; set; }
        [DataMember]
        public VerificationMethod VerificationMethod { get; set; }
        [DataMember]
        public AuthorizationStatus AuthorizationStatus { get; set; }
        [DataMember]
        public PaymentTransactionType TransactionType { get; set; }
        [DataMember]
        public List<ReceiptInfo> ReceiptInfo { get; set; }

        public Payment()
        {
            TenderType = null;
            Amount = null;
            CurrencyExchRate = null;
            StoreId = string.Empty;
			TerminalId = string.Empty;
			StaffId = string.Empty;
            AuthenticationCode = string.Empty;
            CardNumber = string.Empty;
            CardType = string.Empty;
            EFTMessage = string.Empty;
            EFTTransactionId = string.Empty;
            VerificationMethod = VerificationMethod.None;
            AuthorizationStatus = AuthorizationStatus.Approved;
            TransactionType = PaymentTransactionType.Purchase;
            ReceiptInfo = new List<ReceiptInfo>();
        }

        public void ValidatePayment(Money balanceAmount)
        {
            ValidateOvertendering(balanceAmount);
            ValidateUndertendering(balanceAmount);
        }

        public bool IsCurrencyPayment()
        {
            return CurrencyExchRate != null;
        }

        private void ValidateOvertendering(Money balanceAmount)
        {
            // Check if the payment amount exceeds the balance amount we need to check if overtendering is allowed
            Money tmpBalanceAmount = new Money(balanceAmount.Value, balanceAmount.Currency);
            tmpBalanceAmount.Value = balanceAmount.Value < 0 ? balanceAmount.Value * -1 : balanceAmount.Value;

            if (Math.Abs(Amount.Value) > tmpBalanceAmount.Value)
            {
                if (TenderType.AllowOverTendering == false)
                {
                    throw new OvertenderingNotAllowedException(TenderType);
                }
                else
                {
                    // Check if we are within allowed overtendering bounds
                    decimal overTenderingAmount = Amount.Value - tmpBalanceAmount.Value;
                    if (TenderType.MaximumOverTenderAmount == 0)
                    {
                        // Do nothing. The zero is used to note unlimited overtendering allowance.
                    }
                    else
                    {
                        if (overTenderingAmount > TenderType.MaximumOverTenderAmount)
                            throw new OvertenderingExceedsAllowedLimitException(TenderType.MaximumOverTenderAmount);
                    }
                }
            }
        }

        private void ValidateUndertendering(Money balanceAmount)
        {
            Money tmpBalanceAmount = new Money(balanceAmount.Value, balanceAmount.Currency);
            tmpBalanceAmount.Value = balanceAmount.Value < 0 ? balanceAmount.Value * -1 : balanceAmount.Value;
            // If the payment amount is less than the balance amount we need to check if undertendering is allowed
            if (Math.Abs(Amount.Value) < tmpBalanceAmount.Value)
            {
                if (TenderType.AllowUnderTendering == false)
                    throw new UndertenderingNotAllowedException();
            }
        }
    }
}
