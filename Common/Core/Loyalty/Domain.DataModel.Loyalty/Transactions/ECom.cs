using System;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Transactions
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class EComRequestPayment : IDisposable
    {
        public EComRequestPayment()
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
        public Guid Id { get; set; }
        [DataMember]
        public string OrderId { get; set; }
        [DataMember]
        public OrderMessagePayStatus Status { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public string Token { get; set; }
    }
}
 
