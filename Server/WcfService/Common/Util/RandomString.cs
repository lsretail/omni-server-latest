using System;
using System.Linq;

namespace LSOmni.Common.Util
{
    public static class RandomString
    {
        public static string GetString(int lengthOfString)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, lengthOfString).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
