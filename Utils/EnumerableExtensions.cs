namespace Utils;

public static class EnumerableExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) => source is null || !source.Any();
}