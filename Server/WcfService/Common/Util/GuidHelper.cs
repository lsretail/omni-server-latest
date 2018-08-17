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

        public static Guid NewGuid()
        {
            return Guid.NewGuid();
        }

        public static string NewGuidString()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }

        public static string GuidWithDash(string theGuid)
        {
            //theGuid may not have a dash in it, so retur a string with dash
            Guid aG = new Guid(theGuid);
            return aG.ToString();
        }
    }
}
