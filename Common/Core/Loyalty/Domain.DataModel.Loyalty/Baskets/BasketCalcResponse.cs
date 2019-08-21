using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Baskets
{
    #region BasketCalcResponse
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class BasketCalcResponse : Entity, IDisposable
    {
        public BasketCalcResponse(string id) : base(id)
        {
            StoreId = string.Empty;
            TerminalId = string.Empty;
            StaffId = string.Empty;
            EntryStatus = EntryStatus.Normal;
            ReceiptNo = string.Empty;
            TransactionNo = string.Empty;
            TransDate = new DateTime(1970, 1, 1);//min json date
            CurrencyCode = string.Empty;
            CurrencyFactor = 1;
            BusinessTAXCode = string.Empty;
            PriceGroupCode = string.Empty;
            CustomerId = string.Empty;
            CustDiscGroup = string.Empty;
            MemberCardNo = string.Empty; //contactId ?
            MemberPriceGroupCode = string.Empty;

            ManualTotalDiscPercent = 0M;
            ManualTotalDiscAmount = 0M;
            BasketLineCalcResponses = new List<BasketLineCalcResponse>();
            ShippingPrice = 0M;
            TotalAmount = 0M;
            TotalNetAmount = 0M;
            TotalTaxAmount = 0M;
            TotalDiscAmount = 0M;
        }

        public BasketCalcResponse() : this(string.Empty)
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
                if (BasketLineCalcResponses != null)
                    BasketLineCalcResponses.Clear();
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
        public EntryStatus EntryStatus { get; set; }
        [DataMember]
        public string TransactionNo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TransDate { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public int CurrencyFactor { get; set; }
        [DataMember]
        public string BusinessTAXCode { get; set; }
        [DataMember]
        public string PriceGroupCode { get; set; }
        [DataMember]
        public string CustomerId { get; set; }
        [DataMember]
        public string CustDiscGroup { get; set; }
        [DataMember]
        public string MemberCardNo { get; set; }
        [DataMember]
        public string MemberPriceGroupCode { get; set; }
        [DataMember]
        public decimal ManualTotalDiscPercent { get; set; }  //decimal got truncated
        [DataMember]
        public decimal ManualTotalDiscAmount { get; set; }  //decimal got truncated
        [DataMember]
        public List<BasketLineCalcResponse> BasketLineCalcResponses { get; set; }
        [DataMember]
        public decimal ShippingPrice { get; set; }
        [DataMember]
        public decimal TotalAmount { get; set; }
        [DataMember]
        public decimal TotalTaxAmount { get; set; }  
        [DataMember]
        public decimal TotalNetAmount { get; set; }
        [DataMember]
        public decimal TotalDiscAmount { get; set; }  
        
        public override string ToString()
        {
            string s = string.Format("Id: {0} TerminalId: {1} StoreId: {2} MemberCardNo: {3} CustomerId: {4} CurrencyCode: {5} ManualTotalDiscAmount: {6} ManualTotalDiscPercent: {7} BusinessTAXCode: {8}  PriceGroupCode: {9}",
                Id, TerminalId, StoreId, MemberCardNo, CustomerId, CurrencyCode, ManualTotalDiscAmount, ManualTotalDiscPercent, BusinessTAXCode, PriceGroupCode);
            return s;
        }
    }
    #endregion BasketCalcResponse

    #region BasketLineCalcResponse
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class BasketLineCalcResponse : IDisposable
    {
        public BasketLineCalcResponse()
        {
            LineNumber = 0;
            ItemId = string.Empty;
            ItemDescription = string.Empty;
            BarcodeId = string.Empty;
            EntryStatus = EntryStatus.Normal;
            LineType = LineType.Unknown;
            CurrencyCode = string.Empty;
            CurrencyFactor = 1;
            VariantId = string.Empty;
            Uom = string.Empty;
            Quantity = 0M;
            DiscountAmount = 0M;
            DiscountPercent = 0M;
            TAXProductCode = string.Empty;
            TAXBusinessCode = string.Empty;
            ManualPrice = 0M;
            CardOrCustNo = string.Empty;
            ManualDiscountPercent = 0M;
            ManualDiscountAmount = 0M;

            NetPrice = 0M;
            Price = 0M;
            NetAmount = 0M;
            TAXAmount = 0M;
            BasketLineDiscResponses = new List<BasketLineDiscResponse>();

            CouponCode = string.Empty;
            ValidInTransaction = 0;
            CouponFunction = 0;  //0=Use, 1=Issue
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
                if (BasketLineDiscResponses != null)
                    BasketLineDiscResponses.Clear();
            }
        }

        [DataMember]
        public int LineNumber { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string ItemDescription { get; set; }
        [DataMember]
        public string BarcodeId { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public int CurrencyFactor { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string Uom { get; set; }
        [DataMember]
        public decimal Quantity { get; set; } // decimal
        [DataMember]
        public decimal DiscountAmount { get; set; } // decimal
        [DataMember]
        public decimal DiscountPercent { get; set; } // decimal
        [DataMember]
        public string TAXProductCode { get; set; }
        [DataMember]
        public string TAXBusinessCode { get; set; }
        [DataMember]
        public decimal ManualPrice { get; set; } // decimal
        [DataMember]
        public string CardOrCustNo { get; set; }
        [DataMember]
        public EntryStatus EntryStatus { get; set; }

        [DataMember]
        public decimal ManualDiscountPercent { get; set; } // decimal got truncated
        [DataMember]
        public decimal ManualDiscountAmount { get; set; } // decimal got truncated

        [DataMember]
        public decimal NetPrice { get; set; } // decimal got truncated
        [DataMember]
        public decimal Price { get; set; } // decimal got truncated
        [DataMember]
        public decimal NetAmount { get; set; } // decimal got truncated
        [DataMember]
        public decimal TAXAmount { get; set; } // decimal got truncated
        [DataMember]
        public List<BasketLineDiscResponse> BasketLineDiscResponses { get; set; }
        [DataMember]
        public LineType LineType { get; set; }

        [DataMember]
        public string CouponCode { get; set; } 

        [DataMember]
        public int ValidInTransaction { get; set; }  

        [DataMember]
        public int CouponFunction { get; set; }  
    }
    #endregion BasketLineCalcResponse

    #region BasketLineDiscResponse
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class BasketLineDiscResponse : IDisposable
    {
        public BasketLineDiscResponse()
        {
            LineNumber = 0;
            No = string.Empty;
            DiscountType = DiscountType.Unknown;
            PeriodicDiscType = PeriodicDiscType.Unknown;
            PeriodicDiscGroup = string.Empty;
            Description = string.Empty;
            DiscountAmount = 0M;
            DiscountPercent = 0M;
            Quantity = 1;
            OfferNumber = string.Empty;
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
        public string No { get; set; }
        [DataMember]
        public DiscountType DiscountType { get; set; }
        [DataMember]
        public PeriodicDiscType PeriodicDiscType { get; set; }
        [DataMember]
        public string PeriodicDiscGroup { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string OfferNumber { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }  // decimal got truncated
        [DataMember]
        public decimal DiscountAmount { get; set; }  // decimal got truncated
        [DataMember]
        public decimal DiscountPercent { get; set; }  // decimal got truncated

    }
    #endregion BasketLineDiscResponse
}
 
