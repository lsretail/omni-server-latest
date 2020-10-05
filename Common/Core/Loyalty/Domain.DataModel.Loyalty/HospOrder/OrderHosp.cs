using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderHosp : Entity, IDisposable
    { 
        public OrderHosp(string id) : base(id)
        {
            OrderLines = new List<OrderHospLine>();
            OrderPayments = new List<OrderPayment>();
        }

        public OrderHosp() : this(string.Empty)
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
                if (OrderLines != null)
                    OrderLines.Clear();
                if (OrderPayments != null)
                    OrderPayments.Clear();
            }
        }

        /// <summary>
        /// Order Number
        /// </summary>
        [DataMember]
        public string DocumentId { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DocumentRegTime { get; set; }
        [DataMember(IsRequired = true)]
        public string StoreId { get; set; }
        /// <summary>
        /// Member Contact Card Id
        /// </summary>
        [DataMember]
        public string CardId { get; set; }
        /// <summary>
        /// Transaction Receipt Number
        /// </summary>
        [DataMember]
        public string ReceiptNo { get; set; }

        [DataMember]
        public decimal TotalNetAmount { get; set; }
        [DataMember]
        public decimal TotalAmount { get; set; }
        [DataMember]
        public decimal TotalDiscount { get; set; }
        [DataMember]
        public int LineItemCount { get; set; }

        [DataMember]
        public List<OrderHospLine> OrderLines { get; set; }
        [DataMember]
        public List<OrderPayment> OrderPayments { get; set; }

        public override string ToString()
        {
            string req = string.Empty, osldreq = string.Empty;
            try
            {
                foreach (OrderHospLine line in OrderLines)
                    req += "[" + line.ToString() + "] ";
            }
            catch
            {
            }

            return string.Format("Id:{0} StoreId:{1} CardId:{2} OrderLineCreateRequests:{3} OrderDiscountLineCreateRequests:{4}",
                Id, StoreId, CardId, req, osldreq);
        }
    }
}

