using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Orders
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderStatusResponse : IDisposable
    {
        public OrderStatusResponse()
        {
            WebOrderStatus = string.Empty;
            WebOrderShippingStatus = string.Empty;
            WebOrderPaymentStatus = string.Empty;
            DocumentType = string.Empty; // Added in NAV 7.1
            DocumentNo = string.Empty; // Added in NAV 7.1
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

        /// <summary>
        ///  Web Order Status
        /// </summary>
        /// <remarks>
        ///   10 : Pending
        ///   20 : Processing
        ///   30 : Complete
        ///   40 : Canceled
        /// </remarks>
        [DataMember]
        public string WebOrderStatus { get; set; }
        /// <summary>
        ///  Web Order Shipping Status
        /// </summary>
        /// <remarks>
        ///   10 : ShippigNotRequired
        ///   20 : NotYetShipped
        ///   25 : PartiallyShipped
        ///   30 : Shipped
        ///   40 : Delivered
        /// </remarks>
        [DataMember]
        public string WebOrderShippingStatus { get; set; }
        [DataMember]
        public string WebOrderPaymentStatus { get; set; }
        [DataMember]
        public string DocumentType { get; set; }
        [DataMember]
        public string DocumentNo { get; set; }

        public override string ToString()
        {
            string s = string.Format("WebOrderStatus: {0} WebOrderShippingStatus: {1} WebOrderPaymentStatus: {2} DocumentType: {3} DocumentNo: {4} ",
                WebOrderStatus, WebOrderShippingStatus, WebOrderPaymentStatus, DocumentType, DocumentNo);
            return s;
        }
    }
}
 
