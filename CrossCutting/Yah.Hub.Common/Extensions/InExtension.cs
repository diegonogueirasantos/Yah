namespace Yah.Hub.Common.Extensions
{
    public static class InExtension
    {
        public static bool In<T>(this T? value, params T?[] values)
           where T : struct
        {
            return values.Contains(value.Value);
        }

        public static bool In<T>(this T? value, params T[] values)
            where T : struct
        {
            return values.Contains(value.Value);
        }

        public static bool In<T>(this T value, params T[] values)
            where T : struct
        {
            return values.Contains(value);
        }
    }
}
