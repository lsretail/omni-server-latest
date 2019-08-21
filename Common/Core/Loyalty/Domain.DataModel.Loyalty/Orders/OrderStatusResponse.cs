using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Orders
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderStatusResponse : IDisposable
    {
        public OrderStatusResponse()
        {
            DocumentType = string.Empty;
            DocumentNo = string.Empty;
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
        ///  Customer Order Status
        /// </summary>
        /// <remarks>
        ///   10 : Pending
        ///   20 : Processing
        ///   30 : Complete
        ///   40 : Canceled
        /// </remarks>
        [DataMember]
        public int CustomerOrderStatus { get; set; }
        /// <summary>
        ///  Customer Order Shipping Status
        /// </summary>
        /// <remarks>
        ///   10 : ShippigNotRequired
        ///   20 : NotYetShipped
        ///   25 : PartiallyShipped
        ///   30 : Shipped
        ///   40 : Delivered
        /// </remarks>
        [DataMember]
        public int CustomerOrderShippingStatus { get; set; }
        /// <summary>
        /// Customer Order Payment Status
        /// </summary>
        /// <remarks>
        ///   10 : PreApproved
        ///   20 : Approved
        ///   25 : Posted
        /// </remarks>
        [DataMember]
        public int CustomerOrderPaymentStatus { get; set; }
        [DataMember]
        public string DocumentType { get; set; }
        [DataMember]
        public string DocumentNo { get; set; }

        public override string ToString()
        {
            return string.Format("COStatus:{0} COShippingStatus:{1} COPaymentStatus:{2} DocumentType:{3} DocumentNo:{4}",
                CustomerOrderStatus, CustomerOrderShippingStatus, CustomerOrderPaymentStatus, DocumentType, DocumentNo);
        }
    }
}
 
