using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class PublishedOffer : Entity, IDisposable
    {
        public PublishedOffer(string id) : base(id)
        {
            OfferId = string.Empty;   //the offer Id in Nav, used in transactions, coupons,item points.  discount_type
            Description = string.Empty;   //primary text
            Details = string.Empty;       //secondary text
            ValidationText = string.Empty;  //text about when the expiration date is etc in text 
            Images = new List<ImageView>();
            OfferDetails = new List<OfferDetails>();
            OfferLines = new List<PublishedOfferLine>();
            ExpirationDate = null;
            Selected = false;
            Code = OfferDiscountType.Unknown;   //discount type
            Type = OfferType.Unknown;  //Offer_Category
#if WCFSERVER
            ImgBytes = null;
#endif 
        }

        public PublishedOffer() : this(string.Empty)
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
                if (Images != null)
                    Images.Clear();
                if (OfferDetails != null)
                    OfferDetails.Clear();
                if (OfferLines != null)
                    OfferLines.Clear();
            }
        }

        [DataMember]
        public string OfferId { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Details { get; set; }
        [DataMember]
        public string ValidationText { get; set; }
        [DataMember]
        public bool Selected { get; set; }
        [DataMember]
        public List<ImageView> Images { get; set; }
        [DataMember]
        public List<PublishedOfferLine> OfferLines { get; set; }
        [DataMember]
        public DateTime? ExpirationDate { get; set; }
        [DataMember]
        public OfferDiscountType Code { get; set; }
        [DataMember]
        public OfferType Type { get; set; }
        [DataMember]
        public List<OfferDetails> OfferDetails { get; set; }
#if WCFSERVER
        //not all data goes to wcf clients
        public byte[] ImgBytes { get; set; }
#endif  
    }
}
