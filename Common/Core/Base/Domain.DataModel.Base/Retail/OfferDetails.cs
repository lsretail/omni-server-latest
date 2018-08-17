using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class OfferDetails : IDisposable
    {
        public OfferDetails(string offerId)
        {
            OfferId = offerId;
            LineNumber = string.Empty;
            Description = string.Empty;
            Image = new ImageView();
        }

        public OfferDetails()
            : this("0")
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
                if (Image != null)
                    Image.Dispose();
            }
        }

        [DataMember]
        public string OfferId { get; set; }
        [DataMember]
        public string LineNumber { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public ImageView Image { get; set; }
    }
}
 
