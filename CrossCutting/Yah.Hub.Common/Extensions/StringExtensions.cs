using System.Globalization;
using System.Text;

namespace Yah.Hub.Common.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveAccents(this string value)
        {
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = value.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }

        public static string Truncate(this string value, int length, out string cutOff)
        {
            var pos = Math.Max(0, value.Length > length ? length : value.Length);
            cutOff = value.Substring(pos);
            return string.IsNullOrEmpty(value) ? value : value.Substring(0, pos);
        }

        public static string OnlyNumbers(this string value, string defaultValue = "")
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            var output = value.Where(Char.IsDigit).Aggregate(string.Empty, (a, c) => a = string.Concat(a, c));

            return string.IsNullOrEmpty(output) ? defaultValue : output;
        }

        public static string OnlyLetters(this string value, string defaultValue = "")
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            var output = value.Where(Char.IsLetter).Aggregate(string.Empty, (a, c) => a = string.Concat(a, c));

            return string.IsNullOrEmpty(output) ? defaultValue : output;
        }

        public static string Truncate(this string value, int length)
            => value.Truncate(length, out string cutOff);

        public static TEnum ToEnum<TEnum>(this string value) where TEnum : struct
        {
            Enum.TryParse<TEnum>(value, true, out TEnum parsedEnum);
            return parsedEnum;
        }

        public static TEnum ToEnum<TEnum>(this string value, TEnum defaultValue)
            where TEnum : struct
        {
            if (Enum.TryParse<TEnum>(value, true, out TEnum parsedEnum))
                return parsedEnum;

            return defaultValue;
        }

        public static string ExtractVendorId(this string value, string separator)
        {
            return value.Split(separator).First(); 
        }

        public static string ExtractTenantId(this string value, string separator)
        {
            return value.Substring(value.IndexOf(separator) + 1, value.LastIndexOf(separator) - 2);
        }

        public static string ExtractAccountId(this string value, string separator)
        {
            return value.Split("-").Last();
        }
    }
}
