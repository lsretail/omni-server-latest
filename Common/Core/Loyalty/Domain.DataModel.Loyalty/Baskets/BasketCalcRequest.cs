using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Baskets
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class BasketCalcRequest : Entity, IDisposable
    {
        public BasketCalcRequest(string id) : base(id)
        {
            ContactId = "";
            CardId = "";
            BasketCalcLineRequests = new List<BasketCalcLineRequest>();
            ShippingAddress = null; // new Address();
            StoreId = "";
            CalcType = BasketCalcType.None; //Pre, Final, Collect - only in North America, eCommerce.  None used by non ecommerce
        }

        public BasketCalcRequest() : this(string.Empty)
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
                if (BasketCalcLineRequests != null)
                    BasketCalcLineRequests.Clear();
                if (ShippingAddress != null)
                    ShippingAddress.Dispose();
            }
        }

        [DataMember]
        public string ContactId { get; set; }
        [DataMember]
        public string CardId { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public BasketCalcType CalcType { get; set; }
        [DataMember]
        public List<BasketCalcLineRequest> BasketCalcLineRequests { get; set; }
        [DataMember]
        public Address ShippingAddress { get; set; }

        public override string ToString()
        {
            string req = "";
            try
            {
                foreach (BasketCalcLineRequest line in BasketCalcLineRequests)
                    req += "[" + line.ToString() + "] ";
            }
            catch { }

            string s = string.Format("Id: {0} ContactId: {1} CardId: {2} BasketCalcLineRequests: {3} ShippingAddress: {4} StoreId: {5} CalcType: {6}",
                Id, ContactId, CardId, req, (ShippingAddress != null ? ShippingAddress.ToString() : ""), StoreId, CalcType);
            return s;
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class BasketCalcLineRequest : IDisposable
    {
        public BasketCalcLineRequest()
        {
            LineNumber = 0;
            ItemId = "";
            VariantId = "";
            UomId = "";
            Quantity = 1.0M;
            ExternalId = "0";
            CouponCode = "";
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
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string UomId { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public string ExternalId { get; set; }
        [DataMember]
        public string CouponCode { get; set; }
        public override string ToString()
        {
            string s = string.Format("LineNumber: {0} ItemId: {1} VariantId: {2} UomId: {3} Quantity: {4} CouponCode: {5}",
                LineNumber, ItemId, VariantId, UomId, Quantity, CouponCode);
            return s;
        }
    }
}
