using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Activity.Activities
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class AdditionalCharge : IDisposable
    {
        public AdditionalCharge()
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

        [DataMember(IsRequired = true)]
        public string ActivityNo { set; get; }
        [DataMember]
        public int LineNo { set; get; }
        [DataMember(IsRequired = true)]
        public string ItemNo { set; get; }
        [DataMember]
        public decimal Quantity { set; get; }
        [DataMember]
        public decimal Price { set; get; }
        [DataMember]
        public decimal DiscountPercentage { set; get; }
        [DataMember]
        public decimal TotalAmount { set; get; }
        [DataMember]
        public string Optional { set; get; }
        [DataMember]
        public string UnitOfMeasure { set; get; }
        [DataMember]
        public string InvoiceReference { set; get; }
        [DataMember]
        public ProductChargeType ProductType { set; get; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public enum ProductChargeType
    {
        [EnumMember]
        Item = 0,
        [EnumMember]
        Deal = 1
    }
}
