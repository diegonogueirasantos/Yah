namespace Yah.Hub.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public const string DateTimeIsoFormat = "yyyy-MM-ddTHH:mm:sszzz";

        public static string ToISODate(this DateTimeOffset date)
        {
            return date.ToString(DateTimeIsoFormat);
        }

        public static string ToISODate(this DateTimeOffset? date)
        {
            if (date.HasValue)
            {
                return date.Value.ToISODate();
            }
            else
            {
                return default;
            }
        }

        public static string ToISODate(this DateTime date)
        {
            return date.ToString(DateTimeIsoFormat);
        }

        public static string ToISODate(this DateTime? date)
        {
            if (date.HasValue)
            {
                return date.Value.ToISODate();
            }
            else
            {
                return default;
            }
        }

        public static DateTimeOffset AddWorkingDays(this DateTimeOffset date, int daysToAdd)
        {
            DayOfWeek dayOfWeek = date.DayOfWeek;

            int totalWorkingDays = 0;

            for (int day = 0; day <= daysToAdd; day++)
            {
                dayOfWeek = date.AddDays(day).DayOfWeek;

                if (dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday)
                    totalWorkingDays += 1;
            }

            return date.AddDays(totalWorkingDays);
        }

        public static short WorkingDays(this DateTimeOffset StartDate, short daysToAdd)
        {
            return Convert.ToInt16(Math.Round((StartDate.AddWorkingDays(daysToAdd) - StartDate).TotalDays, 0));
        }
    }
}
