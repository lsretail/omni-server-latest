using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2021")]
    public class HospAvailabilityRequest : IDisposable
    {
        public HospAvailabilityRequest()
        {
            ItemId = string.Empty;
            UnitOfMeasure = string.Empty;
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
        public string ItemId { get; set; }
        [DataMember]
        public string UnitOfMeasure { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2021")]
    public class HospAvailabilityResponse : IDisposable
    {
        public HospAvailabilityResponse()
        {
            StoreId = string.Empty;
            Number = string.Empty;
            UnitOfMeasure = string.Empty;
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
        public string StoreId { get; set; }
        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public bool IsDeal { get; set; }
        [DataMember]
        public string UnitOfMeasure { get; set; }
        /// <summary>
        /// Quantity Available in Kitchen 
        /// </summary>
        [DataMember]
        public decimal Quantity { get; set; }
    }
}



