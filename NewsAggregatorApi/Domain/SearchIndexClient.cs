using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using NewsAggregatorApi.Controllers;
using NewsAggregatorApi.Models;
using NewsProcessor.Domain;
using Utils;

namespace NewsAggregatorApi.Domain;

public class SearchIndexClient
{
    private readonly ILogger<SearchIndexClient> _logger;
    private readonly Uri _searchKeywordsUri;
    private readonly Uri _searchIndexUri;
    private readonly Uri _searchUri;
    private readonly Uri _countUri;
    private readonly Uri _getAvailableKeywordsUri;
    private readonly Uri _searchWithKeywordsByKeywordsUri;

    public SearchIndexClient(IConfiguration configuration, ILogger<SearchIndexClient> logger)
    {
        _logger = logger;
        var searchIndexUri = configuration.GetSection("searchIndexUri").Get<string>()!;
        _searchIndexUri = new Uri(searchIndexUri);
        _searchUri = new Uri(searchIndexUri + "/search");
        _searchKeywordsUri = new Uri(searchIndexUri + "/search/keywords");
        _countUri = new Uri(searchIndexUri + "/search/count");
        _getAvailableKeywordsUri = new Uri(searchIndexUri + "/search/availableKeywords");
        _searchWithKeywordsByKeywordsUri = new Uri(searchIndexUri + "/search/searchWithKeywords");
    }

    public async Task<Dictionary<string, string[]>?> GetKeywords(IEnumerable<ApiNews> articles)
    {
        var ids = articles.Select(x => new SearchIndexEntryId(x.Id, x.RegistrationDate));
        var uri = new UriBuilder(_searchKeywordsUri)
            { Query = $"ids={string.Join(',', ids.Select(SearchIndexEntryIdJsonConverter.GetIdAsJsonString))}" };
        var response =
            await HttpCall.Get<Dictionary<SearchIndexEntryId, string[]>>(uri.Uri,
                jsonSerializerOptions: Options);
        if (!response.HasResponse)
        {
            _logger.LogCritical($"Unable to get response from {response.RequestUri}");
            return default;
        }

        return response.Response
            .ToDictionary(x => x.Key.Id, x => x.Value);
    }

    public async Task<Dictionary<string, string[]>?> SearchByKeywordsWithKeywords(string[] keywords, int take = 100,
        int skip = 0,
        bool fromOlder = false,
        DateTime left = default, DateTime right = default)
    {
        var uri = new UriBuilder(_searchWithKeywordsByKeywordsUri)
        {
            Query =
                $"keywords={string.Join(',', keywords)}&take={take}&skip={skip}&fromOlder={fromOlder}{(left == default ? string.Empty : $"&left={left:dd.MM.yyyy}")}{(right == default ? string.Empty : $"&right={right:dd.MM.yyyy}")}"
        };
        var response =
            await HttpCall.Get<Dictionary<SearchIndexEntryId, string[]>>(uri.Uri,
                jsonSerializerOptions: Options);
        if (!response.HasResponse)
        {
            _logger.LogCritical($"Unable to get response from {response.RequestUri}");
            return default;
        }

        return response.Response
            .ToDictionary(x => x.Key.Id, x => x.Value);
    }

    public async Task<HashSet<string>?> SearchByKeywords(string[] keywords, int take = 100, int skip = 0,
        bool fromOlder = false,
        DateTime left = default, DateTime right = default)
    {
        var uri = new UriBuilder(_searchUri)
        {
            Query =
                $"keywords={string.Join(',', keywords)}&take={take}&skip={skip}&fromOlder={fromOlder}{(left == default ? string.Empty : $"&left={left:dd.MM.yyyy}")}{(right == default ? string.Empty : $"&right={right:dd.MM.yyyy}")}"
        };
        var response = await HttpCall.Get<SearchIndexEntryId[]>(uri.Uri, jsonSerializerOptions: Options);
        if (!response.HasResponse)
        {
            _logger.LogCritical($"Unable to get response from {response.RequestUri}");
            return default;
        }

        return response.Response.Select(x => x.Id).ToHashSet();
    }

    public async Task<string[]?> GetAvailableKeywords()
    {
        var response = await HttpCall.Get<string[]>(_getAvailableKeywordsUri);
        if (!response.HasResponse)
        {
            _logger.LogCritical($"Unable to get response from {response.RequestUri}");
            return default;
        }

        return response.Response;
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new SearchIndexEntryIdJsonConverter() }, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };
}