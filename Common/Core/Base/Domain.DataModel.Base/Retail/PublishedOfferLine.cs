using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class PublishedOfferLine : Entity, IDisposable
    {
        public PublishedOfferLine(string id) : base(id)
        {
            
        }

        public PublishedOfferLine() : this(string.Empty)
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
        public OfferDiscountType DiscountType { get; set; }
        [DataMember]
        public string DiscountId { get; set; }
        [DataMember]
        public int LineNo { get; set; }
        [DataMember]
        public OfferDiscountLineType LineType { get; set; }
        [DataMember]
        public string OfferId { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public OfferLineVariantType VariantType { get; set; }
        [DataMember]
        public string Variant { get; set; }
        [DataMember]
        public bool Exclude { get; set; }
        [DataMember]
        public string UnitOfMeasure { get; set; }
    }
}
