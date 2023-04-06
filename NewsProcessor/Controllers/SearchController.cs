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

    public HashSet<SearchIndexEntryId> Get(string[] keywords, int take, int skip)
        => _searchIndex.Find(keywords, take, skip);

}