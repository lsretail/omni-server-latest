using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Orders
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderQueue : Entity, IDisposable
    {
        public OrderQueue(string id) : base(id)
        {
            QueueId = 0;
            OrderQueueStatus = OrderQueueStatus.New;
            OrderQueueType = OrderQueueType.QSR;
            OrderXml = string.Empty;
            DateCreated = DateTime.Now;
            DateLastModified = DateTime.Now;
            Description = string.Empty;
            SearchKey = string.Empty;
            ContactId = string.Empty;
            DeviceId = string.Empty;
            StoreId = string.Empty;
            TerminalId = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            StatusChange = string.Empty;
        }

        public OrderQueue()
            : this(string.Empty)
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
            }
        }

        [DataMember]
        public Int64 QueueId { get; set; }
        [DataMember]
        public OrderQueueStatus OrderQueueStatus { get; set; }
        [DataMember]
        public OrderQueueType OrderQueueType { get; set; }
        [DataMember]
        public string OrderXml { get; set; }
        [DataMember]
        public DateTime DateCreated { get; set; }
        [DataMember]
        public DateTime DateLastModified { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string SearchKey { get; set; }
        [DataMember]
        public string ContactId { get; set; }
        [DataMember]
        public string DeviceId { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string TerminalId { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string PhoneNumber { get; set; }
        [DataMember]
        public string StatusChange { get; set; }

        public override string ToString()
        {
            return string.Format(@"QueueId: {0} Id: {1} OrderXml: {2} Description: {3} SearchKey: {4} ContactId: {5} 
            DeviceId: {6} StoreId: {7} TerminalId: {8} Email: {9}  PhoneNumber: {10}",
                QueueId, Id, OrderXml, Description, SearchKey, ContactId, DeviceId, StoreId, TerminalId, Email, PhoneNumber);
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum OrderQueueStatus
    {
        [EnumMember]
        New = 0,
        [EnumMember]
        InProcess = 1,
        [EnumMember]
        Failed = 2,
        [EnumMember]
        Processed = 3,

    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum OrderQueueType
    {
        [EnumMember]
        QSR = 0,
        [EnumMember]
        ClickCollect = 1,

    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum OrderQueueStatusFilterType
    {
        /// <summary>
        /// No filtering, simply returns last N, ignores search string
        /// </summary>
        [EnumMember]
        None = 99,
        [EnumMember]
        New = 0,
        [EnumMember]
        InProcess = 1,
        [EnumMember]
        Failed = 2,
        [EnumMember]
        Processed = 3,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderSearchRequest : IDisposable
    {
        public OrderSearchRequest()
        {
            MaxOrders = 0;
            OrderStatusFilter = OrderQueueStatusFilterType.None;
            OrderType = OrderQueueType.QSR;
            DateFrom = null;
            DateTo = null;
            StoreId = null;
            ContactId = null;
            SearchKey = null;
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
        public int MaxOrders { get; set; }
        [DataMember]
        public OrderQueueStatusFilterType OrderStatusFilter { get; set; }
        [DataMember]
        public OrderQueueType OrderType { get; set; }
        [DataMember]
        public DateTime? DateFrom { get; set; }
        [DataMember]
        public DateTime? DateTo { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string ContactId { get; set; }
        [DataMember]
        public string SearchKey { get; set; }

        public override string ToString()
        {
            return string.Format(@"MaxOrders: {0} OrderStatusFilter: {1} OrderType: {2} DateFrom: {3} DateTo: {4} StoreId: {5} ContactId: {6} SearchKey: {7}",
                MaxOrders, OrderStatusFilter.ToString(), OrderType.ToString(), DateFrom, DateTo, StoreId, ContactId, SearchKey);
        }
    }
}
