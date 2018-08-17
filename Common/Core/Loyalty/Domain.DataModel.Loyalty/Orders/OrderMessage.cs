using System;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Orders
{
    //discount offer
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderMessage : Entity, IDisposable
    {
        public OrderMessage(string id) : base(id)
        {
            OrderId = 0;
            Details = string.Empty;
            Description = string.Empty;
            OrderMessageStatus = OrderMessageStatus.New;
            DateCreated = DateTime.Now;
            DateLastModified = DateTime.Now;
        }

        public OrderMessage()
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
        public Int64 OrderId { get; set; }
        [DataMember]
        public OrderMessageStatus OrderMessageStatus { get; set; }
        [DataMember]
        public string Details { get; set; }
        [DataMember]
        public DateTime DateCreated { get; set; }
        [DataMember]
        public DateTime DateLastModified { get; set; }
        [DataMember]
        public string Description { get; set; }

        public override string ToString()
        {
            return string.Format(@"OrderId: {0} Id: {1} OrderMessageStatus: {2}  DateCreated: {3} Details: {4}  Description: {5} ",
                OrderId, Id, OrderMessageStatus.ToString(), DateCreated, Details, Description);
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderMessageSearchRequest : IDisposable
    {
        public OrderMessageSearchRequest() 
        {
            MaxOrders = 0;
            MessageStatusFilter = OrderMessageStatusFilterType.None;
            DateFrom = null;
            DateTo = null;
            Description = null;
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
        public OrderMessageStatusFilterType MessageStatusFilter { get; set; }
        [DataMember]
        public DateTime? DateFrom { get; set; }
        [DataMember]
        public DateTime? DateTo { get; set; }
        [DataMember]
        public string Description { get; set; }

        public override string ToString()
        {
            return string.Format(@"MaxOrders: {0} MessageStatusFilter: {1} Description: {2} DateFrom: {3} DateTo: {4} ",
                MaxOrders, MessageStatusFilter.ToString(), Description, DateFrom, DateTo);
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum OrderMessageStatus
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
    public enum OrderMessageStatusFilterType
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
    public enum OrderMessagePayStatus
    {
        [EnumMember]
        Unchanged = 0,
        [EnumMember]
        Changed = 1,
        [EnumMember]
        Cancelled = 2,
    }
}

