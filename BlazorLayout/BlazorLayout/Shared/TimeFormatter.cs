using System.Globalization;

namespace BlazorLayout.Shared
{
    public static class TimeFormatter
    {
        private static readonly TimeZoneInfo germanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        public static string GetDayName(this DateOnly date)
        {
            return date.ToString("dddd", CultureInfo.CurrentUICulture);
        }
        public static string FormatTimeSpanToHhMm(this TimeSpan timeSpan)
        {
            return timeSpan.ToString(@"hh\:mm");
        }

        public static DateTimeOffset ConvertToGermanLocalTime(this DateTimeOffset dateTime)
        {
            var offset = germanTimeZone.GetUtcOffset(dateTime);
            return new DateTimeOffset(dateTime.DateTime, offset);
        }
    }
}
