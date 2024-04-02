using System;
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

        public virtual List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string loyaltySchemeCode, Statistics stat)
        {
            //Returns all anon discounts, plus loyalty scheme discounts if scheme code is provided
            List<ProactiveDiscount> discounts = new List<ProactiveDiscount>();
            foreach (string id in itemIds)
            {
                discounts.AddRange(BOAppConnection.DiscountsGetByStoreAndItem(storeId, id, stat));
            }

            if (string.IsNullOrEmpty(loyaltySchemeCode))
            {
                discounts = discounts.Where(disc => disc.LoyaltySchemeCode == string.Empty).ToList();
            }
            else
            {
                discounts = discounts.Where(disc => disc.LoyaltySchemeCode == string.Empty || disc.LoyaltySchemeCode == loyaltySchemeCode).ToList();
            }

            List<ProactiveDiscount> list = new List<ProactiveDiscount>();
            foreach (ProactiveDiscount disc in discounts)
            {
                if (OfferIsValid(disc.Id, stat))
                {
                    BOAppConnection.LoadDiscountDetails(disc, storeId, loyaltySchemeCode, stat);
                    list.Add(disc);
                }
            }
            return list;
        }

        private bool OfferIsValid(string offerId, Statistics stat)
        {
            DiscountValidation dValid = BOAppConnection.GetDiscountValidationByOfferId(offerId, stat);

            bool withinBounds = true;
            DateTime start = new DateTime();
            DateTime end = new DateTime();
            bool afterMidnight = false;

            if (dValid != null)
            {
                switch (DateTime.Now.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        {
                            withinBounds = dValid.MondayWithinBounds;
                            start = dValid.MondayStart;
                            end = dValid.MondayEnd;
                            afterMidnight = dValid.MondayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Tuesday:
                        {
                            withinBounds = dValid.TuesdayWithinBounds;
                            start = dValid.TuesdayStart;
                            end = dValid.TuesdayEnd;
                            afterMidnight = dValid.TuesdayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Wednesday:
                        {
                            withinBounds = dValid.WednesdayWithinBounds;
                            start = dValid.WednesdayStart;
                            end = dValid.WednesdayEnd;
                            afterMidnight = dValid.WednesdayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Thursday:
                        {
                            withinBounds = dValid.ThursdayWithinBounds;
                            start = dValid.ThursdayStart;
                            end = dValid.ThursdayEnd;
                            afterMidnight = dValid.ThursdayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Friday:
                        {
                            withinBounds = dValid.FridayWithinBounds;
                            start = dValid.FridayStart;
                            end = dValid.FridayEnd;
                            afterMidnight = dValid.FridayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Saturday:
                        {
                            withinBounds = dValid.SaturdayWithinBounds;
                            start = dValid.SaturdayStart;
                            end = dValid.SaturdayEnd;
                            afterMidnight = dValid.SaturdayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Sunday:
                        {
                            withinBounds = dValid.SundayWithinBounds;
                            start = dValid.SundayStart;
                            end = dValid.SundayEnd;
                            afterMidnight = dValid.SundayEndAfterMidnight;
                            break;
                        }
                }

                if (Validation.IsMinDate(start) && Validation.IsMinDate(end))
                {
                    withinBounds = dValid.TimeWithinBounds;
                    start = dValid.StartTime;
                    end = dValid.EndTime;
                    afterMidnight = dValid.EndAfterMidnight;
                }
            }

            if (Validation.IsMinDate(start) && Validation.IsMinDate(end))
                return withinBounds;
            if (DateTime.Now >= start && DateTime.Now <= end)
                return withinBounds;
            if (afterMidnight && (DateTime.Now >= start || DateTime.Now <= end))
                return withinBounds;

            return false;
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
 