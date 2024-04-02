using System;

namespace LSOmni.Common.Util
{
    public static class Validation
    {
        public static bool IsInt(string val)
        {
            int result;
            if (string.IsNullOrWhiteSpace(val))
                return false;
            return int.TryParse(val, out result);
        }

        public static bool IsDouble(string val)
        {
            double result;
            if (string.IsNullOrWhiteSpace(val))
                return false;
            return Double.TryParse(val, out result);
        }

        public static bool IsValidGuid(string theGuid)
        {
            try { Guid aG = new Guid(theGuid); }
            catch { return false; }

            return true;
        }

        public static bool IsValidXml(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return false;

            if (!(xml.StartsWith("<") && xml.EndsWith(">")))
                return false;

            return true;
        }

        public static bool IsValidPassword(string password)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(password) || password.Length < 3 || password.Contains(" "))
                isValid = false;

            return isValid;
        }

        public static bool IsValidUserName(string userName)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(userName) || userName.Length < 3)
                isValid = false;

            return isValid;
        }

        public static bool IsValidEmail(string email)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(email) || email.Length < 5 || !email.Contains(".") || !email.Contains("@"))
                isValid = false;

            return isValid;
        }

        public static bool ValidateKennitala(string kt)
        {
            //http://www.visindavefur.is/svar.php?id=183
            if (string.IsNullOrWhiteSpace(kt))
                return false;

            kt = kt.Trim();
            // remove all '-' from string
            kt = kt.Replace("-", "");
            //the 10th digit is "9" if born in the 20th century
            if (kt.Length != 10)
                return false;

            int iSum = (int.Parse(kt[0].ToString()) * 3) +
                    (int.Parse(kt[1].ToString()) * 2) +
                    (int.Parse(kt[2].ToString()) * 7) +
                    (int.Parse(kt[3].ToString()) * 6) +
                    (int.Parse(kt[4].ToString()) * 5) +
                    (int.Parse(kt[5].ToString()) * 4) +
                    (int.Parse(kt[6].ToString()) * 3) +
                    (int.Parse(kt[7].ToString()) * 2);

            int checkDigit = 11 - (iSum % 11); //vartalan
            //if checkDigit happens to be 11, the set it to zero
            if (iSum % 11 == 0)
                checkDigit = 0;

            if (checkDigit != int.Parse(kt[8].ToString()))
                return false;

            return true;
        }

        public static bool IsMinDate(DateTime date)
        {
            if (date == null || date == DateTime.MinValue)
                return true;
            if ((date.Year == 1754 || date.Year == 1753 || date.Year == 1900) && date.Month == 1 && date.Day == 1)
                return true;
            return false;
        }
    }
}
