using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Baskets
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OneListItemSubLine : Entity, IDisposable
    {
        public OneListItemSubLine(string id) : base(id)
        {
            ItemId = string.Empty;
            VariantId = string.Empty;
            Uom = string.Empty;
            Quantity = 1.0M;

            Description = string.Empty;
            VariantDescription = string.Empty;

            ModifierGroupCode = string.Empty;
            ModifierSubCode = string.Empty;
        }

        public OneListItemSubLine() : this(string.Empty)
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
        public int LineNumber { get; set; }
        /// <summary>
        /// Use Modifier for Modifier and Recipe item, Deal for Deal, Deal Lines and Text for Text Modifiers
        /// </summary>
        [DataMember]
        public SubLineType Type { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string Uom { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string VariantDescription { get; set; }

        /// <summary>
        /// Deal Line LineNo
        /// </summary>
        [DataMember]
        public int DealLineId { get; set; }
        /// <summary>
        /// Deal Line Modifier Item LineNo, set if Deal Line is a Deal Modifier
        /// </summary>
        [DataMember]
        public int DealModLineId { get; set; }
        /// <summary>
        /// Modifier Code, used when adding Modifier to an Item
        /// </summary>
        [DataMember]
        public string ModifierGroupCode { get; set; }
        /// <summary>
        /// Modifier SubCode, used when adding Modifier to an Item
        /// </summary>
        [DataMember]
        public string ModifierSubCode { get; set; }
        /// <summary>
        /// Parent LineNo, used if Modifier is set to an Item in SubLines
        /// </summary>
        [DataMember]
        public int ParentSubLineId { get; set; }

        public string OneListId { get; set; }
        public string OneListItemId { get; set; }

    }

    public enum SubLineType
    {
        Modifier = 0,
        Deal = 1,
        Text = 2
    }
}
