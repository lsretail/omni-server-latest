using System;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail.SpecialCase
{
    public class UnknownPublishedOffer : PublishedOffer
    {
        public UnknownPublishedOffer(string id) : base(id)
        {
        }

        public UnknownPublishedOffer() : this(string.Empty)
        {
        }
    }
}
