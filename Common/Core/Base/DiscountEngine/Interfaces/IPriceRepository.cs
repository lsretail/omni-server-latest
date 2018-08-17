using System;
using System.Collections.Generic;
using System.Text;

namespace LSRetail.Omni.DiscountEngine.Interfaces
{
    public interface IPriceRepository
    {
        /// <summary>
        /// Gets the price for an item
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        decimal PriceGetByItem(string storeId, string itemId, string variantId);
    }
}
