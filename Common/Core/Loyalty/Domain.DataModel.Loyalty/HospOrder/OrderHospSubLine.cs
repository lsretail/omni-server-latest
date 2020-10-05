using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderHospSubLine : IDisposable
    {
        //DealLine is very similar to DealLine
        public OrderHospSubLine()
        {
            LineNumber = 0;
            ItemId = string.Empty;
            VariantId = string.Empty;
            Uom = string.Empty;
            NetPrice = 0M;
            Price = 0M;
            Quantity = 1.0M;
            DiscountAmount = 0.0M;
            DiscountPercent = 0.0M;
            Amount = 0M;
            NetAmount = 0M;
            TAXAmount = 0M;
            ManualDiscountPercent = 0.0M;
            ManualDiscountAmount = 0.0M;

            Description = string.Empty;
            VariantDescription = string.Empty;

            DealCode = string.Empty;
            ModifierGroupCode = string.Empty;
            ModifierSubCode = string.Empty;
            PriceReductionOnExclusion = false;
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
        public SubLineType Type { get; set; }

        [DataMember]
        public int LineNumber { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string Uom { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string VariantDescription { get; set; }

        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public decimal DiscountAmount { get; set; }
        [DataMember]
        public decimal DiscountPercent { get; set; }
        [DataMember]
        public decimal ManualDiscountPercent { get; set; }
        [DataMember]
        public decimal ManualDiscountAmount { get; set; }
        [DataMember]
        public decimal NetPrice { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public decimal NetAmount { get; set; }
        [DataMember]
        public decimal TAXAmount { get; set; }

        [DataMember]
        public string ModifierGroupCode { get; set; }
        [DataMember]
        public string ModifierSubCode { get; set; }

        [DataMember]
        public int DealLineId { get; set; }
        [DataMember]
        public int DealModifierLineId { get; set; }
        [DataMember]
        public string DealCode { get; set; }
        [DataMember]
        public int ParentSubLineId { get; set; }
        [DataMember]
        public bool PriceReductionOnExclusion { get; set; }

        public override string ToString()
        {
            return $"LineNo:{LineNumber}  ItemId:{ItemId}  VariantId:{VariantId}  Uom:{Uom}  " +
                   $"Quantity:{Quantity}  DiscAmount:{DiscountAmount}  DiscPercent:{DiscountPercent}  " +
                   $"ManDiscPercent:{ManualDiscountPercent}  ManDiscAmount:{ManualDiscountAmount}  " +
                   $"PriceReductionOnExclusion:{PriceReductionOnExclusion}  " +
                   $"NetPrice:{NetPrice}  Price:{Price}  NetAmount:{NetAmount}  TAXAmount:{TAXAmount}  " +
                   $"ModifierGroupCode:{ModifierGroupCode}  ModifierSubCode: {ModifierSubCode}  " +
                   $"DealLineCode:{DealLineId}  DealModifierLineCode:{DealModifierLineId}  DealId: {DealCode}";
        }
    }
}
