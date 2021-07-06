using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Orders
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderAvailabilityRequest : Entity, IDisposable
    {
        public OrderAvailabilityRequest(string id) : base(id)
        {
            StoreId = string.Empty;
            CardId = string.Empty;
            OrderLineAvailabilityRequests = new List<OrderLineAvailability>();
        }

        public OrderAvailabilityRequest() : this(string.Empty)
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
                if (OrderLineAvailabilityRequests != null)
                    OrderLineAvailabilityRequests.Clear();
            }
        }

        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string CardId { get; set; }
        [DataMember]
        public List<OrderLineAvailability> OrderLineAvailabilityRequests { get; set; }
        
        public override string ToString()
        {
            string req = string.Empty;
            try
            {
                foreach (OrderLineAvailability line in OrderLineAvailabilityRequests)
                    req += "[" + line.ToString() + "] ";
            }
            catch { }

            string s = string.Format("Id: {0} StoreId: {1} CardId: {2} OrderLineAvailabilityRequests: {4}",
                Id, StoreId, CardId, req);
            return s;
        }
    }
}
