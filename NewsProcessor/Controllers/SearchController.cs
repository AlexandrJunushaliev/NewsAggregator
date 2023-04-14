using Microsoft.AspNetCore.Mvc;
using NewsProcessor.Domain;
using NewsProcessor.Index;
using Utils;

namespace NewsProcessor.Controllers;

[Controller]
[Route("search")]
public class SearchController
{
    private readonly ReverseSearchIndex _reverseSearchIndex;
    private readonly SearchIndex _searchIndex;
    private readonly ILogger<SearchController> _logger;
    private readonly IConfiguration _configuration;

    public SearchController(ReverseSearchIndex reverseSearchIndex, SearchIndex searchIndex,
        ILogger<SearchController> logger, IConfiguration configuration)
    {
        _reverseSearchIndex = reverseSearchIndex;
        _searchIndex = searchIndex;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    [Route("")]
    public IEnumerable<SearchIndexEntryId> Get(string keywords, int take = 100, int skip = 0, bool fromOlder = false,
        string? left = null, string? right = null)
    {
        var keywordsParsed = keywords.Split(',');

        var (leftDt, rightDt) = (DateTime.MinValue, DateTime.MinValue);
        if (left != null && DateTimeExtensions.TryParseAssumeUniversal(left, out leftDt))
        {
            _logger.LogWarning($"Unable to parse date {leftDt}");
        }

        if (right != null && DateTimeExtensions.TryParseAssumeUniversal(right, out rightDt))
        {
            _logger.LogWarning($"Unable to parse date {rightDt}");
        }

        return _reverseSearchIndex.Find(keywordsParsed, take, skip, fromOlder, leftDt == default ? null : leftDt,
            rightDt == default ? null : rightDt.AddDays(1));
    }

    [HttpGet]
    [Route("keywords")]
    public Dictionary<SearchIndexEntryId, HashSet<string>> GetKeyWords(string ids)
    {
        var split = ids.Split(",");
        var idsParsed = split.Select(x =>
        {
            if (!SearchIndexEntryId.TryParse(x, out var id))
            {
                _logger.LogWarning($"Unable to parse id {x} as id");
            }

            return id;
        });
        return GetKeyWordsForSearchIndexEntryIds(idsParsed);
    }

    private Dictionary<SearchIndexEntryId, HashSet<string>> GetKeyWordsForSearchIndexEntryIds(
        IEnumerable<SearchIndexEntryId> ids) =>
        _searchIndex.FindKeyWords(ids);

    [HttpGet]
    [Route("searchWithKeywords")]
    public Dictionary<SearchIndexEntryId, HashSet<string>> SearchWithKeywords(string keywords,
        int take = 100, int skip = 0, bool fromOlder = false,
        string? left = null, string? right = null)
    {
        return GetKeyWordsForSearchIndexEntryIds(Get(keywords, take, skip, fromOlder, left, right));
    }

    [HttpGet]
    [Route("count")]
    public int Count(string[] keywords, string? left = null, string? right = null)
    {
        var (leftDt, rightDt) = (DateTime.MinValue, DateTime.MinValue);
        if (left != null && DateTimeExtensions.TryParseAssumeUniversal(left, out leftDt))
        {
            _logger.LogWarning($"Unable to parse date {leftDt}");
        }

        if (right != null && DateTimeExtensions.TryParseAssumeUniversal(right, out rightDt))
        {
            _logger.LogWarning($"Unable to parse date {rightDt}");
        }

        return _reverseSearchIndex.Count(keywords, leftDt == default ? null : leftDt,
            rightDt == default ? null : rightDt.AddDays(1));
    }

    [HttpGet]
    [Route("availableKeywords")]
    public IEnumerable<string> GetAvailableKeywords() => _configuration.GetSection("keyWords").Get<string[]>()!;
}