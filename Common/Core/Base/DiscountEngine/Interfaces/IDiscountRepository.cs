using System.Collections.Generic;
using LSRetail.Omni.DiscountEngine.DataModels;

namespace LSRetail.Omni.DiscountEngine.Interfaces
{
    public interface IDiscountRepository
    {
        /// <summary>
        /// Retrieves all discounts for a given pair of store id and item id from local database (Disc. Offer, Multibuy, Mix&Match)
        /// </summary>
        /// <param name="storeId">Store id</param>
        /// <param name="itemId">Item id</param>
        /// <returns></returns>
        List<ProactiveDiscount> DiscountsGetByStoreAndItem(string storeId, string itemId);
        /// <summary>
        /// Retrieves all information on validation period for a given offer id from local database
        /// </summary>
        /// <param name="offerId">Offer id</param>
        /// <returns></returns>
        DiscountValidation GetDiscountValidationByOfferId(string offerId);
        /// <summary>
        /// Retrieves all item ids included in a Mix&Match offer from local database
        /// </summary>
        /// <param name="offerId"></param>
        /// <returns></returns>
        List<string> GetItemIdsByMixAndMatchOffer(string storeId, string offerId);
    }
}
