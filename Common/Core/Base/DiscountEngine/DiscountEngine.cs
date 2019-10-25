using System;
using System.Collections.Generic;
using System.Linq;

using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.DiscountEngine.Interfaces;
using LSRetail.Omni.DiscountEngine.Repositories;

namespace LSRetail.Omni.DiscountEngine
{
    public class DiscountEngine
    {
        private IDiscountRepository discRepo;
        private IPriceRepository priceRepo;

        public DiscountEngine(IDiscountRepository discountRepository, IPriceRepository priceRepository)
        {
            discRepo = discountRepository;
            priceRepo = priceRepository;
        }

        public DiscountEngine(NavRepository repository)
        {
            discRepo = repository;
            priceRepo = repository;
        }

        public List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string loyaltySchemeCode)
        {
            //Returns all anon discounts, plus loyalty scheme discounts if scheme code is provided
            List<ProactiveDiscount> discounts = new List<ProactiveDiscount>();
            foreach (string id in itemIds)
            {
                discounts.AddRange(discRepo.DiscountsGetByStoreAndItem(storeId, id));
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
                if (OfferIsValid(disc.Id))
                {
                    if (disc.Type == ProactiveDiscountType.MixMatch)
                    {
                        disc.ItemIds = discRepo.GetItemIdsByMixAndMatchOffer(storeId, disc.Id);
                        disc.ItemIds.Remove(disc.ItemId);
                    }
                    else if (disc.Type == ProactiveDiscountType.DiscOffer)
                    {
                        disc.Price = priceRepo.PriceGetByItem(storeId, disc.ItemId, disc.VariantId);
                        disc.PriceWithDiscount = disc.Price * (1 - disc.Percentage / 100);
                    }
                    list.Add(disc);
                }
            }
            return list;
        }

        public bool OfferIsValid(string offerId)
        {
            DiscountValidation discval = discRepo.GetDiscountValidationByOfferId(offerId);

            bool withinBounds = false;
            DateTime start = new DateTime();
            DateTime end = new DateTime();
            bool afterMidnight = false;

            if (discval != null)
            {
                switch (DateTime.Now.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        {
                            withinBounds = discval.MondayWithinBounds;
                            start = discval.MondayStart;
                            end = discval.MondayEnd;
                            afterMidnight = discval.MondayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Tuesday:
                        {
                            withinBounds = discval.TuesdayWithinBounds;
                            start = discval.TuesdayStart;
                            end = discval.TuesdayEnd;
                            afterMidnight = discval.TuesdayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Wednesday:
                        {
                            withinBounds = discval.WednesdayWithinBounds;
                            start = discval.WednesdayStart;
                            end = discval.WednesdayEnd;
                            afterMidnight = discval.WednesdayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Thursday:
                        {
                            withinBounds = discval.ThursdayWithinBounds;
                            start = discval.ThursdayStart;
                            end = discval.ThursdayEnd;
                            afterMidnight = discval.ThursdayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Friday:
                        {
                            withinBounds = discval.FridayWithinBounds;
                            start = discval.FridayStart;
                            end = discval.FridayEnd;
                            afterMidnight = discval.FridayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Saturday:
                        {
                            withinBounds = discval.SaturdayWithinBounds;
                            start = discval.SaturdayStart;
                            end = discval.SaturdayEnd;
                            afterMidnight = discval.SaturdayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Sunday:
                        {
                            withinBounds = discval.SundayWithinBounds;
                            start = discval.SundayStart;
                            end = discval.SundayEnd;
                            afterMidnight = discval.SundayEndAfterMidnight;
                            break;
                        }
                }
            }

            if (start == DateTime.Parse("1753-01-01 00:00:00.000") && end == DateTime.Parse("1753-01-01 00:00:00.000"))
            {
                withinBounds = discval.TimeWithinBounds;
                start = discval.StartTime;
                end = discval.EndTime;
                afterMidnight = discval.EndAfterMidnight;
            }

            if (start == DateTime.Parse("1753-01-01 00:00:00.000") && end == DateTime.Parse("1753-01-01 00:00:00.000"))
                return withinBounds;
            if (DateTime.Now >= start && DateTime.Now <= end)
                return withinBounds;
            if (afterMidnight && (DateTime.Now >= start || DateTime.Now <= end))
                return withinBounds;

            return false;
        }
    }
}
