using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Orders
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderLineAvailability : IDisposable
    {
        public OrderLineAvailability()
        {
            OrderId = string.Empty;
            LineNumber = 1;
            ItemId = string.Empty;
            VariantId = string.Empty;
            UomId = string.Empty;
            Quantity = 1.0M;
            LineType = LineType.Item; //never change this unless you know what you are doing !
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
        public string OrderId { get; set; }
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
        public LineType LineType { get; set; }

        public override string ToString()
        {
            string s = string.Format("LineNumber: {0} ItemId: {1} VariantId: {2} UomId: {3} Quantity: {4} LineType: {5}",
                LineNumber, ItemId, VariantId, UomId, Quantity, LineType.ToString());
            return s;
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderAvailabilityResponse : IDisposable
    {
        public OrderAvailabilityResponse()
        {
            PreferredSourcingLocation = string.Empty;
            Lines = new List<OrderLineAvailabilityResponse>();
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
        public string PreferredSourcingLocation { get; set; }
        [DataMember]
        public List<OrderLineAvailabilityResponse> Lines { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderLineAvailabilityResponse : IDisposable
    {
        public OrderLineAvailabilityResponse()
        {
            ItemId = string.Empty;
            VariantId = string.Empty;
            UnitOfMeasureId = string.Empty;
            LocationCode = string.Empty;
            Quantity = 1.0M;
            LeadTimeDays = 0;
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
        public string LocationCode { get; set; }
        [DataMember]
        public int LeadTimeDays { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string UnitOfMeasureId { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
    }
}

