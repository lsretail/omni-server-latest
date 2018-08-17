using System;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.Common.Util
{
    public static class CurrencyGetHelper
    {
        /// <summary>
        /// Rounds values to nearest currency unit, i.e 16,45 kr. rounded up if the smallest coin is 10 kr will give 20 kr.
        /// or if the smallest coin is 24 aurar(0,25 kr.) then if rounded up it will give 16,50 kr.
        /// </summary>
        /// <param name="value">The currency value or value to be rounded.</param>
        /// <param name="unit">The smallest unit to be rounded to.  ex: 0.01</param>
        /// <param name="roundMethod">The method of rounding, Nearest,up and down</param>
        /// <returns>Returns a value rounded to the nearest unit.</returns>
        public static decimal RoundToUnit(decimal value, decimal unit, RoundingMethod roundMethod)
        {
            try
            {
                if (unit != 0)
                {
                    decimal decimalValue = value / unit;
                    Int64 intValue = (Int64)(value / unit);
                    decimal difference = Math.Abs(decimalValue) - Math.Abs(intValue);

                    // is rounding required?
                    if (difference > 0)
                    {
                        switch (roundMethod)
                        {
                            case RoundingMethod.RoundNearest: { return Math.Round(Math.Round((value / unit), 0, MidpointRounding.AwayFromZero) * unit, GetNumberOfDecimals(unit), MidpointRounding.AwayFromZero); }
                            case RoundingMethod.RoundDown:
                                {
                                    if (value > 0M)
                                        return Math.Round(Math.Round((value / unit) - 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                                    else
                                        return Math.Round(Math.Round((value / unit) + 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                                }
                            case RoundingMethod.RoundUp:
                                {
                                    if (value > 0M)
                                        return Math.Round(Math.Round((value / unit) + 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                                    else
                                        return Math.Round(Math.Round((value / unit) - 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                                }
                        }
                    }
                }
                return value;
            }
            catch (OverflowException x)
            {
                throw x;
            }
        }

        public static decimal RoundToUnit(decimal value, decimal unit, CurrencyRoundingMethod roundMethod)
        {
            try
            {
                if (unit != 0)
                {
                    decimal decimalValue = value / unit;
                    Int64 intValue = (Int64)(value / unit);
                    decimal difference = Math.Abs(decimalValue) - Math.Abs(intValue);

                    // is rounding required?
                    if (difference > 0)
                    {
                        switch (roundMethod)
                        {
                            case CurrencyRoundingMethod.RoundNearest: { return Math.Round(Math.Round((value / unit), 0, MidpointRounding.AwayFromZero) * unit, GetNumberOfDecimals(unit), MidpointRounding.AwayFromZero); }
                            case CurrencyRoundingMethod.RoundDown:
                            {
                                if (value > 0M)
                                    return Math.Round(Math.Round((value / unit) - 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                                else
                                    return Math.Round(Math.Round((value / unit) + 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                            }
                            case CurrencyRoundingMethod.RoundUp:
                            {
                                if (value > 0M)
                                    return Math.Round(Math.Round((value / unit) + 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                                else
                                    return Math.Round(Math.Round((value / unit) - 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                            }
                        }
                    }
                }
                return value;
            }
            catch (OverflowException x)
            {
                throw x;
            }
        }

        public static int GetNumberOfDecimals(decimal round)
        {
            int counter = 0;
            while (round < 1 && round > 0)
            {
                round = round * 10;
                counter++;
            }
            return counter;
        }

        public enum RoundingMethod
        {
            //same as CurrencyRoundigMethod for nav,
            RoundNearest = 0,
            RoundDown = 1,
            RoundUp = 2
        }
    }
}
 