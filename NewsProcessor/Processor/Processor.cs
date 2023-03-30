using System.Diagnostics;
using System.Text.Json;
using HtmlAgilityPack;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsProcessor.Domain;
using Version = Lucene.Net.Util.Version;

namespace NewsProcessor.Processor;

public class Processor
{
    private readonly ILogger<Processor> _logger;
    private readonly (string keyword, string[] splitted )[] _keywords;

    public Processor(ILogger<Processor> logger, string[] keywords)
    {
        _logger = logger;
        _keywords = keywords.Select(x => (keyword: x, splitted: x.Split(SplitCharsetArray,
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Select(Stemmer.GetStemmed).ToArray()))
            .ToArray();
    }

    public Dictionary<string, NewsMessageEntry[]> Process(NewsMessageEntry[] newsMessageEntries)
    {
        _logger.LogInformation(
            $"Starting process of {newsMessageEntries.Length} entries. Keywords are as following {JsonSerializer.Serialize(_keywords)}");
        var sw = new Stopwatch();
        sw.Start();
        var result = newsMessageEntries.SelectMany(ProcessOneEntry).GroupBy(x => x.Key)
            .Select(x => (x.Key, x.Select(y => y.Value).ToArray()))
            .ToDictionary(x => x.Key, y => y.Item2);
        _logger.LogInformation($"Processed in {sw.Elapsed}");
        return result;
    }

    private IEnumerable<KeyValuePair<string, NewsMessageEntry>>? ProcessOneEntry(NewsMessageEntry entry)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(entry.Text);
        var text = doc.DocumentNode.InnerText;
        if (text is null)
        {
            _logger.LogCritical(
                $"Unable to parse retrieve innerText from news id {entry.Id}. Perhaps html in Text field is incorrect");
            return null;
        }

        return FindKeywordsInText(entry, text);
    }

    private IEnumerable<KeyValuePair<string, NewsMessageEntry>> FindKeywordsInText(NewsMessageEntry entry, string text)
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
                yield return new KeyValuePair<string, NewsMessageEntry>(keyword.keyword, entry);
            }
        }
    }

    private static HashSet<char> SplitCharset = new HashSet<char>()
        { ' ', ',', '.', ':', ';', '-', '_', '|', '#', '!', '@', '<', '>' };

    private static char[] SplitCharsetArray = SplitCharset.ToArray();
}