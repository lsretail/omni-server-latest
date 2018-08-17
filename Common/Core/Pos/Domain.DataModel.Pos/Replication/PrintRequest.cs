using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class PrinterRequest : IDisposable
    {
        public PrinterRequest()
        {
            StoreNo = "";
            TerminalNo = "";
            TransactionNo = "";
            PrinterName = "";
            PrintPaymentReceiptCode = PrintPaymentReceiptCode.NoPaymentReceipt;
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
        public string StoreNo { get; set; }

        [DataMember]
        public string TerminalNo { get; set; }

        [DataMember]
        public string TransactionNo { get; set; }

        [DataMember]
        public string PrinterName { get; set; }

        [DataMember]
        public PrintPaymentReceiptCode PrintPaymentReceiptCode { get; set; }

        public override string ToString()
        {
            return string.Format("  StoreNo: {0}  TerminalNo: {1}  TransactionNo {2} PrinterName {3}  PrintPaymentReceiptCode {4}",
                              StoreNo, TerminalNo, TransactionNo, PrinterName, PrintPaymentReceiptCode.ToString());
        }
    }
}