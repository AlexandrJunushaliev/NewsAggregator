using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NewsProcessor.Domain;

public class SearchIndexEntryIdJsonConverter : JsonConverter<SearchIndexEntryId>
{
    public override void WriteAsPropertyName(Utf8JsonWriter writer, SearchIndexEntryId value,
        JsonSerializerOptions options)
    {
        writer.WritePropertyName(GetIdAsJsonString(value));
    }

    public override SearchIndexEntryId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return ReadIdFromJsonString(str);
    }
    public override SearchIndexEntryId Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return ReadIdFromJsonString(str);
    }

    public override void Write(Utf8JsonWriter writer, SearchIndexEntryId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(GetIdAsJsonString(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetIdAsJsonString(SearchIndexEntryId value) => $"{value.DateTime:yyyyMMdd}_{value.Id}";

    private SearchIndexEntryId ReadIdFromJsonString(string? str)
    {
        if (str is null)
            return default;
        var split = str.Split("_");
        if (split.Length is < 2 or 0)
            throw GetArgumentException(str);
        if (!DateTime.TryParseExact(split[0], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
            throw GetArgumentException(str);
        if (!int.TryParse(split[1], out var id))
            throw GetArgumentException(str);
        return new SearchIndexEntryId(id, dt);
    }

    private static ArgumentException GetArgumentException(string str) =>
        new($"Wrong format of {nameof(SearchIndexEntryId)} {str}");
}