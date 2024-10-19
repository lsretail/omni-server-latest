using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Activity.Activities
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class AdditionalCharge : IDisposable
    {
        public AdditionalCharge()
        {
            ActivityNo = string.Empty;
            ItemNo = string.Empty;
            Description = string.Empty;
            Optional = string.Empty;
            OptionalComment = string.Empty;
            UnitOfMeasure = string.Empty;
            VariantCode = string.Empty;
            InvoiceReference = string.Empty;
            PackageCode = string.Empty;
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
        public string ReservationNo { set; get; }
        [DataMember]
        public int MemberNo { set; get; }
        [DataMember]
        public int LineNo { set; get; }
        [DataMember]
        public int ParentLine { set; get; }
        [DataMember(IsRequired = true)]
        public string ItemNo { set; get; }
        [DataMember]
        public string Description { set; get; }
        [DataMember]
        public decimal Quantity { set; get; }
        [DataMember]
        public decimal Price { set; get; }
        [DataMember]
        public decimal DiscountPercentage { set; get; }
        [DataMember]
        public decimal TotalAmount { set; get; }
        [DataMember]
        public bool IsAllowance { set; get; }
        [DataMember]
        public string Optional { set; get; }
        [DataMember]
        public string OptionalComment { set; get; }
        [DataMember]
        public string UnitOfMeasure { set; get; }
        [DataMember]
        public string VariantCode { set; get; }
        [DataMember]
        public string InvoiceReference { set; get; }
        [DataMember]
        public string PackageCode { set; get; }
        [DataMember]
        public int PackageLine { set; get; }
        [DataMember]
        public int ParentSequence { set; get; }
        [DataMember]
        public int ReservationLineNo { set; get; }
        [DataMember]
        public ProductChargeType ProductType { set; get; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public enum ProductChargeType
    {
        [EnumMember]
        Item = 0,
        [EnumMember]
        Deal = 1,
        [EnumMember]
        InfoCode = 2
    }
}
