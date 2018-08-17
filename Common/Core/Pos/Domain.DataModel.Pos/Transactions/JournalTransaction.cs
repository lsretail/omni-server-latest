using System;
using System.Collections.Generic;
using System.Text;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Pos.Payments;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions
{
    // Note: We have something called TransactionForStaticDisplay ... maybe we could reuse that, i.e. expand and reuse that?

    /// <summary>
    /// Journal transaction. Meant for display only, different from the transaction objects in use elsewhere in the MPOS.
    /// </summary>
    public class JournalTransaction
    {
        public JournalTransaction()
        {
            TransactionId = string.Empty;
            TransactionNumber = string.Empty;
            StoreId = string.Empty;
            TerminalId = string.Empty;
            StaffId = string.Empty;
            ReceiptNumber = string.Empty;
            TransactionDateTime = DateTime.MinValue;
            TenderLines = new List<JournalTransactionTenderLine>();
            SaleLines = new List<JournalTransactionSaleLine>();
        }

        public string TransactionId { get; set; }
        public string TransactionNumber { get; set; }
        public string StoreId { get; set; }
        public string TerminalId { get; set; }
        public string StaffId { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime? TransactionDateTime { get; set; }
        public List<JournalTransactionTenderLine> TenderLines { get; set; }
        public List<JournalTransactionSaleLine> SaleLines { get; set; }

        public override string ToString()
        {
            StringBuilder sBuilder = new StringBuilder();

            string transString = string.Format("[JournalTransaction: TransactionId={0}, TransactionNumber={1}, StoreId={2}, TerminalId={3}, StaffId={4}, ReceiptNumber={5}, TransactionDateTime={6}, TenderLines={7}, SaleLines={8}]",
                TransactionId, TransactionNumber, StoreId, TerminalId, StaffId, ReceiptNumber, TransactionDateTime, TenderLines, SaleLines);

            sBuilder.Append(transString + Environment.NewLine);

            foreach (JournalTransactionTenderLine tenderLine in TenderLines)
                sBuilder.Append(tenderLine.ToString() + Environment.NewLine);

            foreach (JournalTransactionSaleLine saleLine in SaleLines)
                sBuilder.Append(saleLine.ToString() + Environment.NewLine);

            return sBuilder.ToString();
        }
    }

    public class JournalTransactionTenderLine
    {
        public JournalTransactionTenderLine()
        {
            TenderType = JournalTransactionTenderType.Unknown;
            NetAmount = new Money(0M, new Currency());
            TAXAmount = new Money(0M, new Currency());
            TAXPercentage = 0;
            TenderDescription = string.Empty;
            TransDate = DateTime.MinValue;
            EFTCardNumber = string.Empty;
            EFTCardName = string.Empty;
            EFTAuthCode = string.Empty;
            EFTMessage = string.Empty;
            EFTVerificationMethod = VerificationMethod.Unknown;
            EFTTransactionId = string.Empty;
            Voided = false;
        }

        public JournalTransactionTenderType TenderType { get; set; }
        public Money NetAmount { get; set; }
        public Money TAXAmount { get; set; }
        public decimal TAXPercentage { get; set; } // HACK for the certification, as we are not getting the TAXAmount in the response from NAV
        public string TenderDescription { get; set; }
        public DateTime TransDate { get; set; }
        public string EFTCardNumber { get; set; }
        public string EFTCardName { get; set; }
        public string EFTAuthCode { get; set; }
        public string EFTMessage { get; set; }
        public VerificationMethod EFTVerificationMethod { get; set; }
        public string EFTTransactionId { get; set; }
        public bool Voided { get; set; }

        public override string ToString()
        {
            return string.Format("[JournalTransactionTenderLine: TenderType={0}, NetAmount={1}, TAXAmount={2}, TenderDescription={3}, TransDate={4}, EFTCardNumber={5}, EFTCardName={6}, EFTAuthCode={7}, EFTMessage={8}, EFTVerificationMethod={9}, EFTTransactionId={10}, Voided={11}]",
                TenderType, NetAmount, TAXAmount, TenderDescription, TransDate, EFTCardNumber, EFTCardName, EFTAuthCode, EFTMessage, EFTVerificationMethod, EFTTransactionId, Voided);
        }
    }

    public class JournalTransactionSaleLine
    {
        // NOTE: There is no guarantee that the item in the sale line is for sale in the MPOS when it is displaying the journal, if it ever was.
        // Therefore we can't have the Item object as a property here, since we can't get it from DB.

        public JournalTransactionSaleLine()
        {
            ItemId = string.Empty;
            VariantId = string.Empty;
            Uom = string.Empty;
            Quantity = 0;
            DiscountAmount = new Money(0M, new Currency());
            NetAmount = new Money(0M, new Currency());
            TAXAmount = new Money(0M, new Currency());
            ItemDescription = string.Empty;
            VariantDescription = string.Empty;
            UomDescription = string.Empty;
        }

        public string ItemId { get; set; }
        public string VariantId { get; set; }
        public string Uom { get; set; }
        public decimal Quantity { get; set; }
        public Money DiscountAmount { get; set; }
        public Money NetAmount { get; set; }
        public Money TAXAmount { get; set; }
        public string ItemDescription { get; set; }
        public string VariantDescription { get; set; }
        public string UomDescription { get; set; }

        public override string ToString()
        {
            return string.Format("[JournalTransactionSaleLine: ItemId={0}, VariantId={1}, Uom={2}, Quantity={3}, DiscountAmount={4}, NetAmount={5}, TAXAmount={6}, ItemDescription={7}, VariantDescription={8}, UomDescription={9}]",
                ItemId, VariantId, Uom, Quantity, DiscountAmount, NetAmount, TAXAmount, ItemDescription, VariantDescription, UomDescription);
        }
    }

    public enum JournalTransactionTenderType
    {
        Unknown = -1,
        Cash = 1,
        Check = 2,
        Cards = 3,
        CustomerAccount = 4,
        Currency = 6,
        Voucher = 7,
        GiftCard = 8,
        TenderRemoveFloat = 9,
        Coupons = 10,
        MemberCardPayment = 11,
        SpecialOrderDeposit = 15,
        CurrencyRemoveFloat = 19
    }
}
