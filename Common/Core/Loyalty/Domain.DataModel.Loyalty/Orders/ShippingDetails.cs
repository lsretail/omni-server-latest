using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Orders
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ShippingDetails : IDisposable
    {
        public ShippingDetails()
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
        public string Courier { get; set; }
        [DataMember]
        public string DeliveryType { get; set; }
        [DataMember]
        public string DeliveryComment { get; set; }
        [DataMember]
        public string TrackingNumber { get; set; }
        [DataMember]
        public string Packaging { get; set; }
        [DataMember]
        public string TransactionId { get; set; }
        [DataMember]
        public string GeneralInformation { get; set; }
    }
}
