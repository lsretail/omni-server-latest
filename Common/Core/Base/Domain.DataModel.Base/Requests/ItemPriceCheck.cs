using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Requests
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ItemPriceCheckRequest : Entity, IDisposable
    {
        public ItemPriceCheckRequest()
        {
            StoreId = string.Empty;
            TerminalId = string.Empty;
            StaffId = string.Empty;
            CurrencyCode = string.Empty;
            CurrencyFactor = 1;
            CustomerId = string.Empty;
            CustDiscGroup = string.Empty;
            MemberCardNo = string.Empty;
            MemberPriceGroupCode = string.Empty;

            ItemPriceCheckLineRequests = new List<ItemPriceCheckLineRequest>();
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
                if (ItemPriceCheckLineRequests != null)
                    ItemPriceCheckLineRequests.Clear();
            }
        }

        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string TerminalId { get; set; }
        [DataMember]
        public string StaffId { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public int CurrencyFactor { get; set; }
        [DataMember]
        public string CustomerId { get; set; }
        [DataMember]
        public string CustDiscGroup { get; set; }
        [DataMember]
        public string MemberCardNo { get; set; }
        [DataMember]
        public string MemberPriceGroupCode { get; set; }
        [DataMember]
        public List<ItemPriceCheckLineRequest> ItemPriceCheckLineRequests { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0} TerminalId: {1} StoreId: {2}  CustomerId: {3} MemberCardNo: {4}", Id, TerminalId, StoreId, CustomerId, MemberCardNo);
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ItemPriceCheckResponse : Entity, IDisposable
    {
        public ItemPriceCheckResponse()
        {
            StoreId = string.Empty;
            TerminalId = string.Empty;
            StaffId = string.Empty;
            CurrencyCode = string.Empty;
            CurrencyFactor = 1;
            CustomerId = string.Empty;
            CustDiscGroup = string.Empty;
            MemberCardNo = string.Empty;
            MemberPriceGroupCode = string.Empty;
            ItemPriceCheckLineResponses = new List<ItemPriceCheckLineResponse>();
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
                if (ItemPriceCheckLineResponses != null)
                    ItemPriceCheckLineResponses.Clear();
            }
        }

        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string TerminalId { get; set; }
        [DataMember]
        public string StaffId { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public int CurrencyFactor { get; set; }
        [DataMember]
        public string CustomerId { get; set; }
        [DataMember]
        public string CustDiscGroup { get; set; }
        [DataMember]
        public string MemberCardNo { get; set; }
        [DataMember]
        public string MemberPriceGroupCode { get; set; }
        [DataMember]
        public List<ItemPriceCheckLineResponse> ItemPriceCheckLineResponses { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0} TerminalId: {1} StoreId: {2} CurrencyCode: {3} ", Id, TerminalId, StoreId, CurrencyCode);
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ItemPriceCheckLineRequest : IDisposable
    {
        public ItemPriceCheckLineRequest()
        {
            ItemId = string.Empty;
            BarcodeId = string.Empty;
            CurrencyCode = string.Empty;
            CurrencyFactor = 1;
            VariantId = string.Empty;
            Uom = string.Empty;
            Quantity = 1.0M;
            LineNumber = 0;
            LineType = 99;
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
        public string ItemId { get; set; }
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
        public decimal Quantity { get; set; }
        [DataMember]
        public int LineNumber { get; set; }
        [DataMember]
        public int LineType { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ItemPriceCheckLineResponse : IDisposable
    {
        public ItemPriceCheckLineResponse()
        {
            ItemId = string.Empty;
            BarcodeId = string.Empty;
            CurrencyCode = string.Empty;
            CurrencyFactor = 1;
            VariantId = string.Empty;
            Uom = string.Empty;
            Quantity = 1.0M;
            LineNumber = 0;

            NetPrice = 0.0M;
            Price = 0.0M;
            NetAmount = 0.0M;
            LineType = 99;  // Unknown
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
        public string ItemId { get; set; }
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
        public decimal Quantity { get; set; }
        [DataMember]
        public int LineNumber { get; set; }

        [DataMember]
        public decimal NetPrice { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public decimal NetAmount { get; set; }
        [DataMember]
        public int LineType { get; set; }
    }
}
