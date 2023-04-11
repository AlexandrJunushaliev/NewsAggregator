using System.Diagnostics;
using System.Text.Json;
using HtmlAgilityPack;
using NewsProcessor.Domain;
using Utils;

namespace NewsProcessor.Processor;

public class Processor
{
    private readonly ILogger<Processor> _logger;
    private readonly (string keyword, string[] splitted )[] _keywords;

    public Processor(ILogger<Processor> logger, string[] keywords, Func<string, string> stem)
    {
        _logger = logger;
        _keywords = keywords.Select(x => (keyword: x, splitted: x.Split(SplitCharsetArray,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Select(stem)
                .ToArray()))
            .ToArray();
        _stem = stem;
    }

    public (Dictionary<string, HashSet<SearchIndexEntry>>, Dictionary<SearchIndexEntry, HashSet<string>>) Process(
        NewsMessageEntry[] newsMessageEntries)
    {
        var now = DateTime.UtcNow;
        _logger.LogInformation(
            $"Starting process of {newsMessageEntries.Length} entries. Keywords are as following {JsonSerializer.Serialize(_keywords)}");
        var sw = new Stopwatch();
        sw.Start();
        var processed = newsMessageEntries
            .Select(x => ProcessOneEntry(x, now)).ToArray();
        var reverseNews = processed
            .Select(x => x.Item1)
            .Where(x => x is not null)!
            .Flatten()
            .GroupBy(x => x.Key)
            .Select(x => (x.Key, x.Select(y => y.Value).ToHashSet()))
            .ToDictionary(x => x.Key, y => y.Item2);

        var news = processed
            .Select(x => x.Item2)
            .Where(x => x is not null)!
            .Flatten()
            .GroupBy(x => x.Key)
            .Select(x => (x.Key, x.Select(y => y.Value).ToHashSet()))
            .ToDictionary(x => x.Key, y => y.Item2);
        GC.Collect(3, GCCollectionMode.Forced);
        _logger.LogInformation($"Processed in {sw.Elapsed}");
        return (reverseNews, news);
    }

    private (IEnumerable<KeyValuePair<string, SearchIndexEntry>>?, IEnumerable<KeyValuePair<SearchIndexEntry, string>>?)
        ProcessOneEntry(NewsMessageEntry entry,
            DateTime dateTime)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(entry.Text);
        var text = doc.DocumentNode.InnerText;
        if (text is null)
        {
            _logger.LogCritical(
                $"Unable to parse retrieve innerText from news id {entry.Id}. Perhaps html in Text field is incorrect. Text was {entry.Text}");
            if (entry.Header is null && entry.Title is null)
                return default;
        }

        var foundedKeyWords = FindKeywordsInText(entry, text+$" {entry.Title}"+ $"{ entry.Header}", dateTime).ToArray();
        return (foundedKeyWords,
            foundedKeyWords.Select(x => new KeyValuePair<SearchIndexEntry, string>(x.Value, x.Key)));
    }

    private IEnumerable<KeyValuePair<string, SearchIndexEntry>> FindKeywordsInText(NewsMessageEntry entry, string text,
        DateTime dateTime)
    {
        var splitted = text.Split(SplitCharsetArray,
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Select(_stem)
            .ToArray();
        foreach (var keyword in _keywords)
        {
            if (keyword.splitted.Length is 0 or > 3)
            {
                _logger.LogCritical(
                    $"Keyword sequence {keyword.keyword} will not be searched in text because of it's length. Only 1,2,3-long sequences are allowed");
                continue;
            }

            if (NGram.FindAsNGrams(splitted, keyword.splitted, keyword.splitted.Length))
            {
                yield return new KeyValuePair<string, SearchIndexEntry>(keyword.keyword,
                    new SearchIndexEntry(entry.Id,
                        /*DateTime.TryParse(entry.UpdDate, out var updDate)
                            ? updDate
                            : */DateTimeExtensions.TryParseAssumeUniversal(entry.RegDate, out var regDate)
                            ? regDate
                            : dateTime));
            }
        }
    }

    private static readonly HashSet<char> SplitCharset = new()
        { ' ', ',', '.', ':', ';', '-', '_', '|', '#', '!', '@', '<', '>', ')', '(' };

    private readonly Func<string, string> _stem;

    private static readonly char[] SplitCharsetArray = SplitCharset.ToArray();
}