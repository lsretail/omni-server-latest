using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Activity.Activities
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class AvailabilityResponse : IDisposable
    {
        public AvailabilityResponse()
        {
            ItemNo = string.Empty;
            WeekDay = string.Empty;
            TimeCaption = string.Empty;
            Location = string.Empty;
            OptionalResourceNo = string.Empty;
            OptionalResourceName = string.Empty;
            PriceCurrency = string.Empty;
            Comment = string.Empty;
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
        public string ItemNo { set; get; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime AvailDate { set; get; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime AvailTime { set; get; }
        [DataMember]
        public string WeekDay { set; get; }
        [DataMember]
        public int Availability { set; get; }
        [DataMember]
        public string TimeCaption { set; get; }
        [DataMember]
        public string Location { set; get; }
        [DataMember]
        public decimal Price { set; get; }
        [DataMember]
        public string OptionalResourceNo { set; get; }
        [DataMember]
        public string OptionalResourceName { set; get; }
        [DataMember]
        public string PriceCurrency { set; get; }
        [DataMember]
        public string Comment { set; get; }
    }
}
