using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Activity.Activities
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class ActivityRequest : IDisposable
    {
        public ActivityRequest()
        {
            ReservationNo = string.Empty;
            Location = string.Empty;
            ProductNo = string.Empty;
            ContactNo = string.Empty;
            ContactAccount = string.Empty;
            ContactName = string.Empty;
            OptionalResource = string.Empty;
            OptionalComment = string.Empty;
            GroupNo = string.Empty;
            SetGroupReservation = string.Empty;
            PromoCode = string.Empty;
            Email = string.Empty;
            Token = string.Empty;
            GuestType = string.Empty;

            ActivityTime = DateTime.MinValue.ToUniversalTime();
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
        public string ReservationNo { get; set; }
        [DataMember(IsRequired = true)]
        public string Location { get; set; }
        [DataMember(IsRequired = true)]
        public string ProductNo { get; set; }
        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public DateTime ActivityTime { get; set; }
        [DataMember(IsRequired = true)]
        public string ContactNo { get; set; }
        [DataMember]
        public string ContactAccount { get; set; }
        [DataMember]
        public string ContactName { get; set; }
        [DataMember]
        public string OptionalResource { get; set; }
        [DataMember]
        public string OptionalComment { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public decimal NoOfPeople { get; set; }
        [DataMember]
        public bool Paid { get; set; }
        [DataMember]
        public string PromoCode { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Token { get; set; }
        [DataMember]
        public string GroupNo { get; set; }
        [DataMember]
        public string SetGroupReservation { get; set; }
        [DataMember]
        public decimal UnitPrice { get; set; }
        [DataMember]
        public bool SetGroupHeaderStatus { get; set; }
        [DataMember]
        public string GuestType { get; set; }
    }
}
