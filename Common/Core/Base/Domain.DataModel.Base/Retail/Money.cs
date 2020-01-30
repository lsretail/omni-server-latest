using System;
using System.Globalization;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Retail.Exceptions;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    /// <summary>
    /// This is an immutable Value class.
    /// </summary>
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public sealed class Money
    {
        [DataMember]
        public decimal Value { get; set; }
        [DataMember]
        public Currency Currency { get; set; }

        public string ValueFormattedForReceipt
        {
            get { return this.RoundForReceipt(); }
        }

        public Money()
        {
            Value = 0;
            Currency = null;
        }

        public Money(Money money)
        {
            Value = money.Value;
            Currency = money.Currency;
        }

        public Money(decimal amount, string currencyCode)
        {
            Value = amount;
            Currency = new Currency(currencyCode);
        }

        public Money(decimal amount, Currency currency)
        {
            Value = amount;
            Currency = currency;
        }

        public override string ToString()
        {
            return RoundForReceipt();
        }

        public static Money operator +(Money leftHandSide, Money rightHandSide)
        {
            if (leftHandSide.Currency.Id != rightHandSide.Currency.Id)
                throw new DifferentCurrenciesException();

            Money result = new Money(leftHandSide.Value + rightHandSide.Value, leftHandSide.Currency);
            return result;
        }

        public static Money operator -(Money leftHandSide, Money rightHandSide)
        {
            if (leftHandSide.Currency.Id != rightHandSide.Currency.Id)
                throw new DifferentCurrenciesException();

            Money result = new Money(leftHandSide.Value - rightHandSide.Value, leftHandSide.Currency);
            return result;
        }

        /// <summary>
        /// Allocate the specified numberOfParts, i.e. split into a list of 
        /// Money entities with equal value parts.
        /// </summary>
        /// <returns>The allocate.</returns>
        /// <param name="numberOfParts">Number of parts.</param>
        public Money[] Allocate(int numberOfParts)
        {
            Money part = new Money(Value / numberOfParts, Currency);
            Money[] results = new Money[numberOfParts];


            for (int i = 0; i < numberOfParts; i++)
                results[i] = part;
            return results;

            //Money lowResult = new Money(Value / numberOfParts, Currency);
            //Money highResult = new Money(lowResult.Value + 1, Currency);
            //Money[] results = new Money[numberOfParts];
            //int reminder = (int)Value % numberOfParts;
            //for (int i = 0; i < reminder; i++)
            //    results[i] = highResult;
            //for (int i = reminder; i < numberOfParts; i++)
            //    results[i] = lowResult;
            //return results;

        }

        //TODO:  * operator, / operator, Rounding for display, rounding for calculation, Allocating a sum to x parts



        //--------------------------- Rounding code ----------------------

        /// <summary>
        /// Rounds for receipt.
        /// </summary>
        /// <returns>String formatted to display the Money value.</returns>
        private string RoundForReceipt()
        {
            string priceDecimalPlaces = "3:3";   //TODO:  Read this value from the functionality profile
            string outputString = "";
            try
            {
                if (priceDecimalPlaces != "")
                {
                    DecimalSetting Decimals = new DecimalSetting(priceDecimalPlaces);

                    for (int i = Decimals.Max; i >= Decimals.Min; i--)
                    {
                        outputString = Round(Value, i);
                        if (outputString.Length > 0 && (outputString.EndsWith("0") == false))
                            return outputString;
                    }
                }
            }
            catch (Exception)
            {
            }
            // if all fails, normal rounding will be used.
            return RoundForDisplay(false);
        }

        public string RoundForDisplay(bool showCurrencySymbol)
        {
            decimal roundedValue = RoundToUnit(Value, Currency.RoundOffSales, Currency.SaleRoundingMethod);
            string numberFormat = NumberFormat(Currency.RoundOffSales);
            string roundedValueString = roundedValue.ToString(numberFormat);

            if (showCurrencySymbol)
            {
                // TODO This should be a property somewhere else ... right-to-left language
                if (Currency.Id == "AED")  //Everything is reversed
                {
                    if (Currency.Prefix.Length > 0)
                        roundedValueString = roundedValueString + " " + Currency.Prefix;

                    if (Currency.Postfix.Length > 0)
                        roundedValueString = Currency.Postfix + " " + roundedValueString;
                }
                else //Normal
                {
                    if (Currency.Prefix.Length > 0)
                        roundedValueString = Currency.Prefix + " " + roundedValueString;

                    if (Currency.Postfix.Length > 0)
                        roundedValueString = roundedValueString + " " + Currency.Postfix;
                }
            }

            return roundedValueString;
        }

        public string RoundForDisplay(bool showCurrencySymbol, DecimalUtils.DecimalFormat decimalFormat)
        {
            CultureInfo cultureInfo = DecimalUtils.GetCultureInfo(decimalFormat);

            decimal roundedValue = RoundToUnit(Value, Currency.RoundOffSales, Currency.SaleRoundingMethod);
            string numberFormat = NumberFormat(Currency.RoundOffSales);
            string roundedValueString = roundedValue.ToString(numberFormat, cultureInfo);

            if (showCurrencySymbol)
            {
                // TODO This should be a property somewhere else ... right-to-left language
                if (Currency.Id == "AED")  //Everything is reversed
                {
                    if (Currency.Prefix.Length > 0)
                        roundedValueString = roundedValueString + " " + Currency.Prefix;

                    if (Currency.Postfix.Length > 0)
                        roundedValueString = Currency.Postfix + " " + roundedValueString;
                }
                else //Normal
                {
                    if (Currency.Prefix.Length > 0)
                        roundedValueString = Currency.Prefix + " " + roundedValueString;

                    if (Currency.Postfix.Length > 0)
                        roundedValueString = roundedValueString + " " + Currency.Postfix;
                }
            }

            return roundedValueString;
        }

        public string RoundForDisplayAbs(bool showCurrencySymbol)
        {
            decimal roundedValue = Math.Abs(RoundToUnit(Value, Currency.RoundOffSales, Currency.SaleRoundingMethod));
            string numberFormat = NumberFormat(Currency.RoundOffSales);
            string roundedValueString = roundedValue.ToString(numberFormat);

            if (showCurrencySymbol)
            {
                // TODO This should be a property somewhere else ... right-to-left language
                if (Currency.Id == "AED")  //Everything is reversed
                {
                    if (Currency.Prefix.Length > 0)
                        roundedValueString = roundedValueString + " " + Currency.Prefix;

                    if (Currency.Postfix.Length > 0)
                        roundedValueString = Currency.Postfix + " " + roundedValueString;
                }
                else //Normal
                {
                    if (Currency.Prefix.Length > 0)
                        roundedValueString = Currency.Prefix + " " + roundedValueString;

                    if (Currency.Postfix.Length > 0)
                        roundedValueString = roundedValueString + " " + Currency.Postfix;
                }
            }

            return roundedValueString;
        }




        /// <summary>
        /// Round for receipt format. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="numberOfDecimals"></param>
        /// <returns></returns>
        private string Round(decimal value, int numberOfDecimals)
        {
            decimal unit = 1.0M / (decimal)Math.Pow(10, numberOfDecimals);
            decimal roundedValue = RoundToUnit(value, unit, CurrencyRoundingMethod.RoundNearest);
            string numberFormat = NumberFormat(unit);
            return roundedValue.ToString(numberFormat);
        }

        public decimal RoundToUnit(decimal unit, CurrencyRoundingMethod roundMethod)
        {
            return RoundToUnit(Value, unit, roundMethod);
        }

        /// <summary>
        /// Rounds values to nearest currency unit, i.e 16,45 kr. rounded up if the smallest coin is 10 kr will give 20 kr.
        /// or if the smallest coin is 24 aurar(0,25 kr.) then if rounded up it will give 16,50 kr.
        /// </summary>
        /// <param name="value">The currency value or value to be rounded.</param>
        /// <param name="unit">The smallest unit to be rounded to.</param>
        /// <param name="roundMethod">The method of rounding, Nearest,up and down</param>
        /// <returns>Returns a value rounded to the nearest unit.</returns>
        public decimal RoundToUnit(decimal value, decimal unit, CurrencyRoundingMethod roundMethod)
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
                        case CurrencyRoundingMethod.RoundNearest:
                            {
                                return Math.Round(Math.Round((value / unit), 0, MidpointRounding.AwayFromZero) * unit, GetNumberOfDecimals(unit), MidpointRounding.AwayFromZero);
                            }

                        case CurrencyRoundingMethod.RoundDown:
                            if (value > 0M)
                                return Math.Round(Math.Round((value / unit) - 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                            else
                                return Math.Round(Math.Round((value / unit) + 0.5M, 0) * unit, GetNumberOfDecimals(unit));

                        case CurrencyRoundingMethod.RoundUp:
                            if (value > 0M)
                                return Math.Round(Math.Round((value / unit) + 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                            else
                                return Math.Round(Math.Round((value / unit) - 0.5M, 0) * unit, GetNumberOfDecimals(unit));
                    }
                }
                else
                {
                    return Math.Round(value, GetNumberOfDecimals(unit));        //used to remove trailing zeros
                }
            }
            return value;
        }

        /// <summary>
        /// Rounds values to nearest a number of decimals using the currency rounding method.
        /// </summary>
        /// <param name="value">The currency value or value to be rounded.</param>
        /// <param name="numberOfDecimals">Number of decimals.</param>
        /// <param name="roundMethod">The method of rounding, Nearest,up and down</param>
        /// <returns>Returns a value rounded for currency.</returns>
        public static decimal Round(decimal value, int numberOfDecimals, CurrencyRoundingMethod roundMethod)
        {
            switch (roundMethod)
            {
                case CurrencyRoundingMethod.RoundNearest:
                    {
                        return Math.Round(value, numberOfDecimals, MidpointRounding.AwayFromZero);
                    }

                case CurrencyRoundingMethod.RoundDown:
                    if (value > 0M)
                        return Math.Round(value - 0.5M, numberOfDecimals);
                    else
                        return Math.Round(value + 0.5M, numberOfDecimals);

                case CurrencyRoundingMethod.RoundUp:
                    if (value > 0M)
                        return Math.Round(value + 0.5M, numberOfDecimals);
                    else
                        return Math.Round(value - 0.5M, numberOfDecimals);
            }

            return value;
        }

        private int GetNumberOfDecimals(decimal round)
        {
            int counter = 0;
            while (round % 1 != 0)
            {
                round = round * 10;
                counter++;
            }
            return counter;
        }

        /// <summary>
        /// Returns a string to give the correct number number of decimals depending on the currency unit. 
        /// 1 will give return N0, 0,1 returns N1, 0,01 returns N2 etc.
        /// </summary>
        /// <param name="currencyUnit">The smallest currency unit</param>
        /// <returns>Returns the format string N0,N1, etc for the ToString function</returns>
        private string NumberFormat(decimal currencyUnit)
        {
            string result = "N";
            if (Math.Round(currencyUnit) > 0)
            {
                result += "0";
            }
            else
            {
                for (int i = 1; i < 9; i++)
                {
                    decimal factor = (decimal)Math.Pow(10, i);
                    decimal multipl = currencyUnit * factor;
                    if (multipl == Math.Round(multipl))
                    {
                        result += i.ToString();
                        break;
                    }
                }
            }
            return result;
        }
    }


    internal class DecimalSetting
    {
        public int Min { get; set; }
        public int Max { get; set; }

        /// <summary>
        /// Default constructor sets Min and Max to 0
        /// </summary>
        public DecimalSetting()
        {
            Min = 0;
            Max = 0;
        }

        /// <summary>
        /// Constructor that takes min and max values and sets them
        /// </summary>
        /// <param name="Min">Minimum decimal places</param>
        /// <param name="Max">Maximum decimal places</param>
        public DecimalSetting(int Min, int Max)
        {
            this.Max = Max;
            this.Min = Min;
        }

        /// <summary>
        /// Constructor that takes a string of decimal places. The string must be at least 3 characters and contain the letter : i.e. 2:3 or 0:2
        /// </summary>
        /// <param name="DecimalPlaces">The decimal places as a string (0:2, 2:2 and etc)</param>
        public DecimalSetting(string DecimalPlaces)
        {
            // The minimum length of this is x:x = 3 letters, and it must contain the letter ':'
            if (DecimalPlaces.Length >= 3 && DecimalPlaces.Contains(":"))
            {
                string[] minAndMax = DecimalPlaces.Split(':');
                if (minAndMax.Length >= 2)
                {
                    Min = Convert.ToInt16(minAndMax[0]);
                    Max = Convert.ToInt16(minAndMax[1]);
                }
            }
        }
    }
}
