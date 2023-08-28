using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Client
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class Promotion : Entity, IDisposable
    {
        public Promotion(string id) : base(id)
        {
            PriceDescription = string.Empty;
            ItemNo = string.Empty;
            ClubMembersOnly = string.Empty;
            IsPriceOrDiscount = string.Empty;
            DaySetting = string.Empty;
            Location = string.Empty;
            ProductName = string.Empty;
            PriceCurrency = string.Empty;
            DateFrom = DateTime.MinValue.ToUniversalTime();
            DateTo = DateTime.MinValue.ToUniversalTime();
            TimeFrom = DateTime.MinValue.ToUniversalTime();
            TimeTo = DateTime.MinValue.ToUniversalTime();
        }

        public Promotion() : this(string.Empty)
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
        public string ItemNo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateFrom { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateTo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TimeFrom { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TimeTo { get; set; }
        [DataMember]
        public string ClubMembersOnly { get; set; }
        [DataMember]
        public string IsPriceOrDiscount { get; set; }
        [DataMember]
        public decimal PriceOrDiscountValue { get; set; }
        [DataMember]
        public string PriceDescription { get; set; }
        [DataMember]
        public string DaySetting { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public string ProductName { get; set; }
        [DataMember]
        public string PriceCurrency { get; set; }
    }
}
