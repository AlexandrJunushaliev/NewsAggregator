using System.Globalization;

namespace Utils;

public static class DateTimeExtensions
{
    public static bool TryParseAssumeUniversal(string? str, out DateTime dt)
    {
        return DateTime.TryParse(str,
            new DateTimeFormatInfo { FullDateTimePattern = "dd.MM.yyyy HH:mm:ss", ShortDatePattern = "dd.MM.yyyy" },
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt);
    }

    public static DateTime ParseAssumeUniversal(string str) =>
        DateTime.Parse(str,
            new DateTimeFormatInfo { FullDateTimePattern = "dd.MM.yyyy HH:mm:ss", ShortDatePattern = "dd.MM.yyyy" },
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}