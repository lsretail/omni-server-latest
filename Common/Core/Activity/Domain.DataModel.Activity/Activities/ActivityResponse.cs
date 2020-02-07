using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Activities
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class ActivityResponse : Entity, IDisposable
    {
        public ActivityResponse()
        {
            ErrorString = string.Empty;
            Currency = string.Empty;
            BookingRef = string.Empty;
            ReservationNo = string.Empty;
            ItemNo = string.Empty;
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
        public string ErrorString { get; set; }
        [DataMember]
        public decimal UnitPrice { get; set; }
        [DataMember]
        public decimal LineDiscount { get; set; }
        [DataMember]
        public decimal TotalAmount { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public string Currency { get; set; }
        [DataMember]
        public string BookingRef { get; set; }
        [DataMember]
        public string ReservationNo { get; set; }
        [DataMember]
        public string ItemNo { get; set; }
    }
}
