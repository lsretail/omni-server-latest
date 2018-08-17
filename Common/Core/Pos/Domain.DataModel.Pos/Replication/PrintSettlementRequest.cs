using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Pos.Payments;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class PrintSettlementRequest : IDisposable
    {
        public PrintSettlementRequest()
        {
            Id = "";
            StoreId = "";
            TerminalId = "";
            StaffId = "";

            PrinterName = ""; //only used when PrintMethod is printer
            Email = "";  //only used when PrintMethod is email
            PrintMethod = PrintMethod.Printer;
            ReceiptInfo = new List<ReceiptInfo>();
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
                if (ReceiptInfo != null)
                    ReceiptInfo.Clear();
            }
        }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string TerminalId { get; set; }
        [DataMember]
        public string StaffId { get; set; }

        [DataMember]
        public string PrinterName { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public PrintMethod PrintMethod { get; set; }
        [DataMember]
        public List<ReceiptInfo> ReceiptInfo { get; set; }
 
        public override string ToString()
        {
            string rf = "";
            try
            {
                foreach (ReceiptInfo line in ReceiptInfo)
                    rf += "[" + line.Key + " : " + line.Value + "] ";
            }
            catch { }

            return string.Format(" Id: {0} StoreId: {1} TerminalId: {2} PrintMethod: {3}  Email: {4}   ReceiptInfo: [{5}]",
            Id, StoreId, TerminalId, PrintMethod.ToString(), Email, rf);
        }
    }
}