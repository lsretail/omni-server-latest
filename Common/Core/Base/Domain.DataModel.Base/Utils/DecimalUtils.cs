using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSRetail.Omni.Domain.DataModel.Base.Utils
{
    public class DecimalUtils
    {
        public enum DecimalFormat
        {
            Unknown = 0,
            DecimalComma = 1,
            DecimalPoint = 2,
        }

        public static CultureInfo GetCultureInfo(DecimalFormat decimalFormat)
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;

            if (decimalFormat == DecimalFormat.DecimalComma)
            {
                cultureInfo = new CultureInfo("is-IS");
            }
            else if (decimalFormat == DecimalFormat.DecimalPoint)
            {
                cultureInfo = new CultureInfo("en-US");
            }

            return cultureInfo;
        }

        public static string GetDecimalSeperator(DecimalFormat decimalFormat)
        {
            string decimalSeperator = CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;

            if (decimalFormat == DecimalFormat.DecimalComma)
            {
                decimalSeperator = ",";
            }
            else if (decimalFormat == DecimalFormat.DecimalPoint)
            {
                decimalSeperator = ".";
            }

            return decimalSeperator;
        }

        public static decimal ParseDecimal(string s)
        {
            decimal qty = 0m;

            try
            {
                qty = Decimal.Parse(s, NumberStyles.AllowDecimalPoint);

            }
            catch (System.Exception)
            {
                try
                {
                    qty = Decimal.Parse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                }
                catch (System.Exception)
                {
                }
            }

            return qty;
        }

        public static bool IsNumeric(string s)
        {
            try
            {
                Decimal.Parse(s, NumberStyles.AllowDecimalPoint);
            }
            catch (System.Exception)
            {
                try
                {
                    Decimal.Parse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                }
                catch (System.Exception)
                {
                    return false;
                }
                return true;
            }
            return true;
        }
    }
}
