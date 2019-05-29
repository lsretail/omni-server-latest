using System.Collections.Generic;

using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.BLL.Loyalty
{
    public class OfferBLL : BaseLoyBLL
    {
        public OfferBLL(int timeoutInSeconds)
            : this("", timeoutInSeconds)
        {
        }

        public OfferBLL(string securityToken, int timeoutInSeconds)
            : base(securityToken, timeoutInSeconds)
        {
        }

        public virtual List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string loyaltySchemeCode)
        {
            return BOAppConnection.DiscountsGet(storeId, itemIds, loyaltySchemeCode);
        }

        public virtual List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId)
        {
            if (string.IsNullOrWhiteSpace(cardId) == false)
                ValidateCard(cardId);

            // this new published offers method came with LS Nav 9.00.03
            // before that it didn't make sense to show the coupons since they couldn't be used in mpos transaciton anyway!
            // old styld didn't have offerId 
            return BOLoyConnection.PublishedOffersGet(cardId, itemId, storeId);
        }
    }
}

 