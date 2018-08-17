using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class PrintTenderRequest : IDisposable
    {
        public PrintTenderRequest()
        {
            Id = "";
            StoreId = "";
            TerminalId = "";
            StaffId = "";
            TenderLine = new TenderLine();
            PrinterName = ""; //only used when PrintMethod is printer
            Email = "";  //only used when PrintMethod is email
            PrintTenderStatus = PrintTenderStatus.Unknown;
            TransactionType = TenderTransactionType.Purchase;
            PrintPaymentReceiptCode = PrintPaymentReceiptCode.MerchantAndCustomerCopy;
            PrintAsCopy = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        [DataMember]
        public string PrinterName { get; set; }

        [DataMember]
        public PrintTenderStatus PrintTenderStatus { get; set; }

        [DataMember]
        public TenderTransactionType TransactionType { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string StoreId { get; set; }

        [DataMember]
        public string TerminalId { get; set; }

        [DataMember]
        public string StaffId { get; set; }

        [DataMember]
        public TenderLine TenderLine { get; set; }

        [DataMember]
        public PrintMethod PrintMethod { get; set; }

        [DataMember]
        public bool PrintAsCopy { get; set; }

        [DataMember]
        public PrintPaymentReceiptCode PrintPaymentReceiptCode { get; set; }

        public override string ToString()
        {
            return string.Format(" Id: {0} StoreId: {1} TerminalId: {2} PrintMethod: {3}  Email: {4} PrintPaymentReceiptCode: {5} PrintTenderStatus: {6} PrintAsCopy: {7}  TenderLine: [{8}]",
            Id, StoreId, TerminalId, PrintMethod.ToString(), Email, PrintPaymentReceiptCode.ToString(), PrintAsCopy, PrintTenderStatus.ToString(),
            TenderLine.ToString());
        }
    }

    public enum PrintTenderStatus
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        Approved = 1,
        [EnumMember]
        Declined = 2,
        [EnumMember]
        Cancel = 3,
        [EnumMember]
        BankAxeptReserveLosning = 6
    }

    public enum PrintMethod
    {
        [EnumMember]
        Email = 0,
        //[EnumMember]
        //Buffer = 1,
        [EnumMember]
        Printer = 2,
        [EnumMember]
        Unknown = 3,
    }

    public enum PrintPaymentReceiptCode
    {
        [EnumMember]
        MerchantCopy = 0,    //Print only merchant copy
        [EnumMember]
        CustomerCopy = 1,    //Print only customer copy
        [EnumMember]
        MerchantAndCustomerCopy = 2,  //Print both merchant and customer copies
        [EnumMember]
        NoPaymentReceipt = 3,  //Do not print any payment receipts
    }

    public enum TenderTransactionType
    {
        [EnumMember]
        Purchase = 0,
        [EnumMember]
        Reversal = 1,
        [EnumMember]
        Refund = 2
    }
}