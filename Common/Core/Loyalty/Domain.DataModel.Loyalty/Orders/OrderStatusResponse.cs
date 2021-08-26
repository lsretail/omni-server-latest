using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Orders
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderStatusResponse : IDisposable
    {
        public OrderStatusResponse()
        {
            DocumentNo = string.Empty;
            OrderStatus = string.Empty;
            ExtCode = string.Empty;
            Lines = new List<OrderLineStatus>();
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
        public string DocumentNo { get; set; }
        /// <summary>
        ///  Customer Order Status
        /// </summary>
        [DataMember]
        public string OrderStatus { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string ExtCode { get; set; }
        [DataMember]
        public List<OrderLineStatus> Lines { get; set; }

        public override string ToString()
        {
            return string.Format("COStatus:{0} DocumentNo:{1}", OrderStatus, DocumentNo);
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderLineStatus : IDisposable
    {
        public OrderLineStatus()
        {
            LineStatus = string.Empty;
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
        ///  Customer Order Line Status
        /// </summary>
        [DataMember]
        public string LineStatus { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string ExtCode { get; set; }
        [DataMember]
        public int LineNumber { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string UnitOfMeasureId { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public bool AllowCancel { get; set; }
        [DataMember]
        public bool AllowModify { get; set; }
    }
}

