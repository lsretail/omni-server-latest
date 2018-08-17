using System.Globalization;

namespace LSRetail.Omni.Domain.DataModel.Pos.Utils
{
    public static class Utils
    {
        public static string Right(this string source, int count)
        {
            if (count >= source.Length)
            {
                return source;
            }
            return source.Substring(source.Length - count, count);
        }

        public static string Left(this string source, int count)
        {
            if (count >= source.Length)
            {
                return source;
            }
            return source.Substring(0, count);
        }

    }
}
