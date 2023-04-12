namespace Utils;

public static class EnumerableExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) => source is null || !source.Any();
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source) => source.SelectMany(x => x);
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? (IEnumerable<T>)Array.Empty<T>();
}