using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class TransactionsGetLatestRequest : IDisposable
    {
        public TransactionsGetLatestRequest()
        {
            StoreId = "";
            TerminalId = "";
            TransType = TransType.Mobile;
            NumberOfTransactionsToGet = 0;
            ReceiptNumber = "";
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
        public string StoreId { get; set; }
        [DataMember]
        public string TerminalId { get; set; }
        [DataMember]
        public TransType TransType { get; set; }
        [DataMember]
        public int NumberOfTransactionsToGet { get; set; }
        [DataMember]
        public string ReceiptNumber { get; set; }

        public override string ToString()
        {
            return string.Format("TerminalId: {0} StoreId: {1} TransType: {2} NumberOfTransactionsToGet: {3} ReceiptNumber: {4}",
                TerminalId, StoreId, TransType.ToString(), NumberOfTransactionsToGet.ToString(), ReceiptNumber);
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class TransactionsGetLatestResponse : IDisposable
    {
        public TransactionsGetLatestResponse()
        {
            LatestTransactions = new List<TransactionGetLatestResponseLine>();
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
                if (LatestTransactions != null)
                    LatestTransactions.Clear();
            }
        }

        [DataMember]
        public List<TransactionGetLatestResponseLine> LatestTransactions { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class TransactionGetLatestResponseLine : Entity, IDisposable
    {
        public TransactionGetLatestResponseLine(string id) : base(id)
        {
            StoreId = "";
            TerminalId = "";
            TransactionNo = "";
            ReceiptNo = "";
            TransDate = new DateTime(1970, 1, 1);//min json date
            MemberCardNo = "";
            TransAmount = "";
            CurrencyCode = "";
        }

        public TransactionGetLatestResponseLine() : this(string.Empty)
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
            }
        }

        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string TerminalId { get; set; }
        [DataMember]
        public string TransactionNo { get; set; }
        [DataMember]
        public string ReceiptNo { get; set; }
        [DataMember]
        public DateTime TransDate { get; set; }
        [DataMember]
        public string MemberCardNo { get; set; }
        [DataMember]
        public string TransAmount { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }

        public override string ToString()
        {
            string s = string.Format("Id: {0} StoreId: {1} TerminalId: {2} TransactionNo: {3} ReceiptNo: {4} TransDate: {5} MemberCardNo: {6} TransAmount: {7} CurrencyCode: {8}",
                Id, StoreId, TerminalId, TransactionNo, ReceiptNo, TransDate.ToString(), MemberCardNo, TransAmount, CurrencyCode);
            return s;
        }
    }

    public enum TransType
    {
        All = 0,
        Stationary = 1,
        Mobile = 2
    }
}


