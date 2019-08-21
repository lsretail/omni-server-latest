using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class TransactionGetInfoRequest : IDisposable
    {
        public TransactionGetInfoRequest()
        {
            StoreId = "";
            TerminalId = "";
            TransactionId = "";
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
        public string TransactionId { get; set; }

        public override string ToString()
        {
            string s = string.Format("StoreId: {0} TerminalId: {1} TransactionId: {2}", StoreId, TerminalId, TransactionId);
            return s;
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class TransactionGetInfoResponse : Entity, IDisposable
    {
        public TransactionGetInfoResponse(string id) : base(id)
        {
            StoreId = "";
            TerminalId = "";
            StaffId = "";
            ReceiptNo = "";
            TransactionNo = "";
            TransDate = new DateTime(1970, 1, 1);//min json date
            CurrencyCode = "";
            Lines = new List<TransactionGetInfoResponseLine>();
        }

        public TransactionGetInfoResponse() : this(string.Empty)
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
                if (Lines != null)
                    Lines.Clear();
            }
        }

        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string TerminalId { get; set; }
        [DataMember]
        public string StaffId { get; set; }
        [DataMember]
        public string ReceiptNo { get; set; }
        [DataMember]
        public string TransactionNo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TransDate { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public List<TransactionGetInfoResponseLine> Lines { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class TransactionGetInfoResponseLine : IDisposable
    {
        public TransactionGetInfoResponseLine()
        {
            LineNumber = 0;
            LineType = LineType.Unknown;
            ItemId = "";
            CurrencyCode = "";
            VariantId = "";
            Uom = "";
            Quantity = "";
            DiscountAmount = "";
            NetAmount = "";
            TAXAmount = "";
            ItemDescription = "";
            VariantDescription = "";
            UomDescription = "";
            TenderDescription = "";
            TransDate = new DateTime(1970, 1, 1);//min json date
            EFTCardNumber = "";
            EFTCardName = "";
            EFTAuthCode = "";
            EFTMessage = "";
            EFTVerificationMethod = EFTVerificationMethod.Unknown;
            EFTTransactionId = "";
            EFTAuthStatus = EFTAuthorizationStatus.Unknown;
            EFTTransType = EFTTransactionType.Purchase;
            EFTDateTime = DateTime.Now;
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
        public int LineNumber { get; set; }
        [DataMember]
        public LineType LineType { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string Uom { get; set; }
        [DataMember]
        public string Quantity { get; set; }
        [DataMember]
        public string DiscountAmount { get; set; }
        [DataMember]
        public string NetAmount { get; set; }
        [DataMember]
        public string TAXAmount { get; set; }
        [DataMember]
        public string ItemDescription { get; set; }
        [DataMember]
        public string VariantDescription { get; set; }
        [DataMember]
        public string UomDescription { get; set; }
        [DataMember]
        public string TenderDescription { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TransDate { get; set; }
        [DataMember]
        public string EFTCardNumber { get; set; }
        [DataMember]
        public string EFTCardName { get; set; }
        [DataMember]
        public string EFTAuthCode { get; set; }
        [DataMember]
        public string EFTMessage { get; set; }
        [DataMember]
        public EFTVerificationMethod EFTVerificationMethod { get; set; }
        [DataMember]
        public string EFTTransactionId { get; set; }
        [DataMember]
        public EFTAuthorizationStatus EFTAuthStatus { get; set; }
        [DataMember]
        public EFTTransactionType EFTTransType { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime EFTDateTime { get; set; }
    }
}


