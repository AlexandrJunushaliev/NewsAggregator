using System.Diagnostics;
using System.Text.Json;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using NewsProcessor.Domain;
using Utils;

namespace NewsProcessor.Processor;

public class Processor
{
    private readonly ILogger<Processor> _logger;
    private readonly (string keyword, string[] splitted )[] _keywords;

    public Processor(ILogger<Processor> logger, string[] keywords)
    {
        _logger = logger;
        _keywords = keywords.Select(x => (keyword: x, splitted: x.Split(SplitCharsetArray,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Select(Stemmer.GetStemmed)
                .ToArray()))
            .ToArray();
    }

    public Dictionary<string, HashSet<SearchIndexEntry>> Process(NewsMessageEntry[] newsMessageEntries)
    {
        var now = DateTime.UtcNow;
        _logger.LogInformation(
            $"Starting process of {newsMessageEntries.Length} entries. Keywords are as following {JsonSerializer.Serialize(_keywords)}");
        var sw = new Stopwatch();
        sw.Start();
        var result = newsMessageEntries
            .Select(x=>ProcessOneEntry(x, now))
            .Where(x => x is not null)!
            .Flatten()
            .GroupBy(x => x.Key)
            .Select(x => (x.Key, x.Select(y => y.Value).ToHashSet()))
            .ToDictionary(x => x.Key, y => y.Item2);
        _logger.LogInformation($"Processed in {sw.Elapsed}");
        return result;
    }

    private IEnumerable<KeyValuePair<string, SearchIndexEntry>>? ProcessOneEntry(NewsMessageEntry entry, DateTime dateTime)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(entry.Text);
        var text = doc.DocumentNode.InnerText;
        if (text is null)
        {
            _logger.LogCritical(
                $"Unable to parse retrieve innerText from news id {entry.Id}. Perhaps html in Text field is incorrect. Text was {entry.Text}");
            return null;
        }

        return FindKeywordsInText(entry, text, dateTime);
    }

    private IEnumerable<KeyValuePair<string, SearchIndexEntry>> FindKeywordsInText(NewsMessageEntry entry, string text, DateTime dateTime)
    {
        var splitted = text.Split(SplitCharsetArray,
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Select(Stemmer.GetStemmed);
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
                yield return new KeyValuePair<string, SearchIndexEntry>(keyword.keyword, new SearchIndexEntry(int.Parse(entry.Id), dateTime));
            }
        }
    }

    private static HashSet<char> SplitCharset = new()
        { ' ', ',', '.', ':', ';', '-', '_', '|', '#', '!', '@', '<', '>' };

    private static char[] SplitCharsetArray = SplitCharset.ToArray();
}