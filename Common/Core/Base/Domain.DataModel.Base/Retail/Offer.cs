using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class Offer : Entity, IDisposable
    {
        public Offer(string id) : base(id)
        {
            PrimaryText = string.Empty;
            SecondaryText = string.Empty;
            Details = new List<OfferDetails>();
            ValidationText = string.Empty; //text about when the expiration date is etc in text 
            Images = new List<ImageView>();
            ExpirationDate = null;
            RV = 0;
            Type = OfferDiscountType.Promotion;
            Code = OfferDiscountType.Unknown;

#if WCFSERVER
            ImgBytes = null;
#endif 
        }

        public Offer() : this(string.Empty)
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
                Images.Clear();
            }
        }

        [DataMember]
        public string PrimaryText { get; set; }
        [DataMember]
        public string SecondaryText { get; set; }
        [DataMember]
        public List<OfferDetails> Details { get; set; }
        [DataMember]
        public string ValidationText { get; set; }
        [DataMember]
        public List<ImageView> Images { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? ExpirationDate { get; set; }
        [DataMember]
        public long RV { get; set; }
        [DataMember]
        public OfferDiscountType Type { get; set; }
        [DataMember]
        public OfferDiscountType Code { get; set; }

#if WCFSERVER
        //not all data goes to wcf clients
        public byte[] ImgBytes { get; set; }
#endif  
    }
}

