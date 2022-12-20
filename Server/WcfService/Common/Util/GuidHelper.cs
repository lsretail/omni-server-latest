using System;

namespace LSOmni.Common.Util
{
    public static class GuidHelper
    {
        public static bool IsValidGuid(string theGuid)
        {
            if (string.IsNullOrWhiteSpace(theGuid))
                return false;

            try { Guid aG = new Guid(theGuid); }
            catch { return false; }

            return true;
        }

        public static string NewGuidString()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }

        public static string NewGuidWithoutDashes(int maxlen)
        {
            string s = NewGuidString().Replace("-", "");
            if (maxlen > 0)
                s = s.Substring(0, maxlen);
            return s;
        }
    }
}
