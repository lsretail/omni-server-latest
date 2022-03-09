using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2020")]
    public class OrderHosp : Entity, IDisposable
    { 
        public OrderHosp(string id) : base(id)
        {
            StoreId = string.Empty;
            CardId = string.Empty;
            Name = string.Empty;
            BillToName = string.Empty;
            Email = string.Empty;
            Directions = string.Empty;
            ReceiptNo = string.Empty;
            RestaurantNo = string.Empty;

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
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string BillToName { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public Address Address { get; set; }
        [DataMember]
        public string Directions { get; set; }
        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public string RestaurantNo { get; set; }
        [DataMember]
        public DateTime OrderDate { get; set; }
        [DataMember]
        public DateTime PickupTime { get; set; }
        [DataMember]
        public HospDeliveryType DeliveryType { get; set; }
        [DataMember]
        public string SalesType { get; set; }

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
        [DataMember]
        public List<OrderDiscountLine> OrderDiscountLines { get; set; }

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

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2020")]
    public enum HospDeliveryType
    {
        [EnumMember]
        NoChoice = 0,
        [EnumMember]
        Home = 1,
        [EnumMember]
        Work = 2,
        [EnumMember]
        Other = 3,
        [EnumMember]
        Takeout = 4
    }
}

