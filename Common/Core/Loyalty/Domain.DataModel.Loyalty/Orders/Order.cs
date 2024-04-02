using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Orders
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class Order : Entity, IDisposable
    {
        public enum OrderTenderType
        {
            Cash = 0,
            Card = 1,
            Coupons = 2,
            Member = 3,
            GiftCard = 4,
            Customer = 5
        }

        public Order(string id) : base(id)
        {
            StoreId = string.Empty;
            Currency = string.Empty;
            CardId = string.Empty;
            CustomerId = string.Empty;
            ReceiptNo = string.Empty;
            CollectLocation = string.Empty;
            ContactId = string.Empty;
            ContactName = string.Empty;
            Email = string.Empty;
            DayPhoneNumber = string.Empty;
            ShipToName = string.Empty;
            ShipToEmail = string.Empty;
            ShippingAgentCode = string.Empty;
            ShippingAgentServiceCode = string.Empty;
            TransId = string.Empty;
            TransTerminal = string.Empty;

            OrderLines = new List<OrderLine>();
            OrderDiscountLines = new List<OrderDiscountLine>();
            OrderPayments = new List<OrderPayment>();
            ContactAddress = new Address();
            ShipToAddress = new Address();
        }

        public Order() : this(string.Empty)
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
                if (OrderDiscountLines != null)
                    OrderDiscountLines.Clear();
                if (OrderPayments != null)
                    OrderPayments.Clear();
            }
        }

        [DataMember(IsRequired = true)]
        public string StoreId { get; set; }
        /// <summary>
        /// Member Contact Card Id
        /// </summary>
        [DataMember(IsRequired = true)]
        public string CardId { get; set; }
        [DataMember]
        public string CustomerId { get; set; }
        /// <summary>
        /// Transaction Receipt Number
        /// </summary>
        [DataMember]
        public string ReceiptNo { get; set; }
        [DataMember]
        public string Currency { get; set; }

        /// <summary>
        /// Ship Order, can also be used with Click And Collect order to be shipped from CAC Store
        /// </summary>
        [DataMember]
        public bool ShipOrder { get; set; }

        [DataMember]
        public decimal TotalNetAmount { get; set; }
        [DataMember]
        public decimal TotalAmount { get; set; }
        [DataMember]
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// Type of Order
        /// ClickAndCollect order are processed in the store at POS Terminal.  ScanPayGo are for In-store shopping apps.
        /// </summary>
        [DataMember(IsRequired = true)]
        public OrderType OrderType { get; set; }
        /// <summary>
        /// Store to collect all Click And Collect Lines from, if left empty, store location and Click And Collect option in lines will be used
        /// </summary>
        [DataMember]
        public string CollectLocation { get; set; }
        [DataMember]
        public List<OrderLine> OrderLines { get; set; }
        [DataMember]
        public List<OrderDiscountLine> OrderDiscountLines { get; set; }
        [DataMember]
        public List<OrderPayment> OrderPayments { get; set; }

        /// <summary>
        /// Member Contact Id, only used in Older NAV Web services
        /// </summary>
        [DataMember]
        public string ContactId { get; set; }
        [DataMember]
        public string ContactName { get; set; }
        [DataMember]
        public Address ContactAddress { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string DayPhoneNumber { get; set; }

        // shipping info
        [DataMember]
        public string ShipToName { get; set; }
        [DataMember]
        public Address ShipToAddress { get; set; }
        [DataMember]
        public string ShipToEmail { get; set; }

        [DataMember]
        public string ShippingAgentCode { get; set; }
        [DataMember]
        public string ShippingAgentServiceCode { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime RequestedDeliveryDate { get; set; }

        /// <summary>
        /// Current Member Contact Point balance 
        /// </summary>
        [DataMember]
        public decimal PointBalance { get; set; }
        /// <summary>
        /// Points used as payment in order
        /// </summary>
        [DataMember]
        public decimal PointsUsedInOrder { get; set; }
        /// <summary>
        /// Number of points needed to pay full amount
        /// </summary>
        [DataMember]
        public decimal PointAmount { get; set; }
        /// <summary>
        /// Cash Needed with points, if Point balance is not enough to pay order in full
        /// </summary>
        [DataMember]
        public decimal PointCashAmountNeeded { get; set; }
        /// <summary>
        /// Points Rewarded for this Order
        /// </summary>
        [DataMember]
        public decimal PointsRewarded { get; set; }

        // local properties to find transactions
        public string TransId { get; set; }
        public string TransTerminal { get; set; }

        public override string ToString()
        {
            string req = string.Empty, osldreq = string.Empty;
            try
            {
                foreach (OrderLine line in OrderLines)
                    req += "[" + line.ToString() + "] ";
                foreach (OrderDiscountLine line in OrderDiscountLines)
                    osldreq += "[" + line.ToString() + "] ";
            }
            catch
            {
            }

            return string.Format("Id:{0} StoreId:{1} CardId:{2} ContactId:{3} OrderLineCreateRequests:{4} OrderDiscountLineCreateRequests:{5}",
                Id, StoreId, CardId, ContactId, req, osldreq);
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum OrderType
    {
        /// <summary>
        /// Sales Order to be shipped
        /// </summary>
        [EnumMember]
        Sale,
        /// <summary>
        /// Click and Collect Order to be collected at store
        /// </summary>
        [EnumMember]
        ClickAndCollect,
        /// <summary>
        /// Order Created with App to be processed at checkout in store
        /// </summary>
        [EnumMember]
        ScanPayGo,
        /// <summary>
        /// Order created with App that will be retrieved at the POS 
        /// </summary>
        [EnumMember]
        ScanPayGoSuspend
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum OrderEditType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        General = 1,
        [EnumMember]
        Header = 2
    }
}

