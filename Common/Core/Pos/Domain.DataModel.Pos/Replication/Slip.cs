using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class Slip : IDisposable
    {
        public Slip(string receiptId)
        {
            ReceiptId = receiptId; //this is the unique slip ID, nice to have 
            StoreId = "";
            TerminalId = "";
            TransId = "";
            SlipPayments = new List<SlipPayment>();
        }

        public Slip()
            : this("")
        {
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
                if (SlipPayments != null)
                    SlipPayments.Clear();
            }
        }

        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string TerminalId { get; set; }
        [DataMember]
        public string TransId { get; set; }
        [DataMember]
        public List<SlipPayment> SlipPayments { get; set; }
        [DataMember]
        public string ReceiptId { get; set; }

        public override string ToString()
        {
            string s =
                string.Format("  StoreNo: {0}  TerminalNo: {1}  TransactionNo {2}  ReceiptId {3}",
                              StoreId, TerminalId, TransId, ReceiptId);
            return s;
        }
    }
}

