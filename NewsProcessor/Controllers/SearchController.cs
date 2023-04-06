using Microsoft.AspNetCore.Mvc;
using NewsProcessor.Domain;
using NewsProcessor.Index;

namespace NewsProcessor.Controllers;
[Controller]
[Route("search")]
public class SearchController
{
    private readonly SearchIndex _searchIndex;

    public SearchController(SearchIndex searchIndex)
    {
        _searchIndex = searchIndex;
    }
    public (DateTime lastProcessedTime, HashSet<SearchIndexEntryId> entries) Get(string[] keywords, int take, int skip)
    {
        return _searchIndex.Find(keywords, take, skip);
    }
}