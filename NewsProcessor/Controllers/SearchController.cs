using Microsoft.AspNetCore.Mvc;
using NewsProcessor.Domain;
using NewsProcessor.Index;

namespace NewsProcessor.Controllers;

[Controller]
[Route("search")]
public class SearchController
{
    private readonly ReverseSearchIndex _reverseSearchIndex;
    private readonly SearchIndex _searchIndex;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ReverseSearchIndex reverseSearchIndex, SearchIndex searchIndex,
        ILogger<SearchController> logger)
    {
        _reverseSearchIndex = reverseSearchIndex;
        _searchIndex = searchIndex;
        _logger = logger;
    }

    [HttpGet]
    [Route("")]
    public HashSet<SearchIndexEntryId> Get(string[] keywords, int take = 100, int skip = 0)
        => _reverseSearchIndex.Find(keywords, take, skip);

    [HttpGet]
    [Route("keywords")]
    public Dictionary<SearchIndexEntryId, HashSet<string>> GetKeyWords(IEnumerable<string> news)
    {
        return _searchIndex.FindKeyWords(news.Select(x =>
        {
            if (!SearchIndexEntryId.TryParse(x, out var id))
            {
                _logger.LogWarning($"Unable to parse id {x} as id");
            }

            return id;
        }));
    }
}