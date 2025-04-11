﻿using System;
using System.Collections.Generic;
using System.Linq;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.BLL.Loyalty
{
    public class OfferBLL : BaseLoyBLL
    {
        public OfferBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
        }

        public OfferBLL(BOConfiguration config) : base(config, 0)
        {
        }

        public virtual List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string schemeCode, Statistics stat)
        {
            return BOAppConnection.DiscountsGet(storeId, itemIds, schemeCode, stat);
        }

        public virtual List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId, Statistics stat)
        {
            // this new published offers method came with LS Nav 9.00.03
            // before that it didn't make sense to show the coupons since they couldn't be used in MPOS transaction anyway!
            // old style didn't have offerId 
            return BOLoyConnection.PublishedOffersGet(cardId, itemId, storeId, stat);
        }
    }
}
 