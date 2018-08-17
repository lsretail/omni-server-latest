using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Pos.Payments;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]

    public class TenderLine : IDisposable
    {
        public TenderLine(string storeId, string terminalId, string transactionId, string id)
        {
            Id = id; //LineNumber
            TransactionId = transactionId;
            TerminalId = terminalId;
            StoreId = storeId;
            StaffId = "";
            TenderTypeId = "0"; // MobileStoreTenderType  TenderType.Cash;
            CurrencyCode = string.Empty;
            CurrencyFactor = string.Empty;
            Amount = 0.0M;
            EntryStatus = EntryStatus.Normal;
            TenderDescription = "";
            EFTCardNumber = "";
            EFTCardName = "";
            EFTCardType = "";
            EFTAuthCode = "";
            EFTMessage = "";
            EFTTransactionId = ""; //EFTTransactionNo
            ReceiptInfo = new List<ReceiptInfo>();
            EFTVerificationMethod = EFTVerificationMethod.None;
            EFTAuthStatus = EFTAuthorizationStatus.Unknown;
            EFTTransType = EFTTransactionType.Purchase;
            EFTDateTime = DateTime.Now;

            ExternalId = "";
            _sAmount = "";
        }

        public TenderLine()
            : this("", "", "", "")
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
                if (ReceiptInfo != null)
                    ReceiptInfo.Clear();
            }
        }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string TerminalId { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string StaffId { get; set; }
        [DataMember]
        public string TransactionId { get; set; }
        [DataMember]
        public string TenderDescription { get; set; }
        [DataMember]
        public string TenderTypeId { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public string CurrencyFactor { get; set; }
        [DataMember]
        public string ExternalId { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public EntryStatus EntryStatus { get; set; }
        [DataMember]
        public string EFTCardNumber { get; set; }
        [DataMember]
        public string EFTCardName { get; set; }
        [DataMember]
        public string EFTCardType { get; set; }
        [DataMember]
        public string EFTAuthCode { get; set; }
        [DataMember]
        public string EFTMessage { get; set; }
        [DataMember]
        public string EFTTransactionId { get; set; }
        [DataMember]
        public EFTAuthorizationStatus EFTAuthStatus { get; set; }
        [DataMember]
        public EFTTransactionType EFTTransType { get; set; }
        [DataMember]
        public DateTime EFTDateTime { get; set; }

        [DataMember]
        public string _sAmount { get; set; }

        [DataMember]
        public EFTVerificationMethod EFTVerificationMethod { get; set; }
        [DataMember]
        public List<ReceiptInfo> ReceiptInfo { get; set; }

        public override string ToString()
        {
            string rf = "";
            try
            {
                foreach (ReceiptInfo line in ReceiptInfo)
                    rf += "KeyValue[" + line.Key + " : " + line.Value + "  Type:" + line.Type + "  IsLargeValue:" + line.IsLargeValue.ToString() + "] ";
            }
            catch { }

            string s = "";
            try
            {
                s = string.Format("Id: {0} TerminalId: {1} StoreId: {2} TransactionId: {3} TenderDescription: {4} CurrencyCode: {5} TenderTypeId: {6} Amount: {7}  EntryStatus: {8}  EFTCardNumber: {9} EFTCardName: {10} EFTAuthCode: {11} EFTMessage: {12} EFTVerificationMethod: {13} EFTTransactionId: {14} StaffId: {15}  EFTAuthStatus: {16}  EFTTransType: {17} EFTDateTime: {18} ReceiptInfo: [{19}]",
                     Id, TerminalId, StoreId, TransactionId, TenderDescription, CurrencyCode, TenderTypeId,
                     Amount, EntryStatus.ToString(), EFTCardNumber, EFTCardName, EFTAuthCode, EFTMessage, EFTVerificationMethod.ToString(), EFTTransactionId, EFTAuthStatus.ToString(), EFTTransType.ToString(), EFTDateTime.ToString(), rf);
            }
            catch { }
            return s;
        }
    }

    public enum EFTVerificationMethod
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
        Unknown = 6,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    [Flags]
    public enum EFTAuthorizationStatus
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
        Unknown = 99
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    [Flags]
    public enum EFTTransactionType
    {
        [EnumMember]
        Purchase = 0,
        [EnumMember]
        Reversal = 1,
        [EnumMember]
        Refund = 2
    }
}


