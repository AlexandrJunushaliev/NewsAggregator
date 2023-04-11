using Microsoft.AspNetCore.Mvc;
using NewsAggregatorApi.Domain;
using NewsAggregatorApi.Infrastructure.EntityFramework;
using NewsAggregatorApi.Models;
using Utils;

namespace NewsAggregatorApi.Controllers;

[Controller]
[Route("news")]
public class NewsController
{
    private readonly NewsAggregatorContext _context;
    private readonly SearchIndexClient _searchIndexClient;
    private readonly ILogger<NewsController> _logger;

    public NewsController(NewsAggregatorContext context, SearchIndexClient searchIndexClient,
        ILogger<NewsController> logger)
    {
        _context = context;
        _searchIndexClient = searchIndexClient;
        _logger = logger;
    }

    [HttpGet]
    [Route("")]
    public async Task<ApiNews[]?> GetNews(string[]? keywords, int take = 30, int skip = 0, bool reverseOrder = false,
        string? leftBorder = null, string? rightBorder = null)
    {
        (DateTime leftDt, DateTime rightDt) = (default, default);
        if (!DateTimeExtensions.TryParseAssumeUniversal(leftBorder, out leftDt) ||
            !DateTimeExtensions.TryParseAssumeUniversal(rightBorder, out rightDt))
        {
            _logger.LogWarning($"Unable to parse one of the dates {leftBorder} {rightBorder}");
        }

        if (keywords is null || keywords.Length == 0)
        {
            var queue = _context.Articles.Select(x => new ApiNews
            {
                NewsText = x.Text, RegistrationDate = x.RegistrationDate, SourceName = x.SourceName,
                SourceSite = x.SourceSite, Title = x.Title, Header = x.Header, Id = x.Id, Status = x.Status, NewsUrl = x.ArticleUrl
            });
            queue = reverseOrder
                ? queue.OrderBy(x => x.RegistrationDate)
                : queue.OrderByDescending(x => x.RegistrationDate);
            if (leftDt != default && rightDt != default)
            {
                // мерзкий хак из-за того, что npgsql на данный момент не умеет делать dateTime.Date
                rightDt = rightDt.AddDays(1);
                queue = queue.Where(x =>
                    x.RegistrationDate >= leftDt && x.RegistrationDate < rightDt);
            }
                
            var news = queue.Skip(skip).Take(take).ToArray();

            var idsToKeywords = await _searchIndexClient.GetKeywords(news);
            if (idsToKeywords is null)
                return news;
            foreach (var entry in news)
            {
                if (idsToKeywords.ContainsKey(entry.Id))
                {
                    entry.Keywords = idsToKeywords[entry.Id];
                }
            }

            return news;
        }

        var foundByKeywords =
            await _searchIndexClient.SearchByKeywords(keywords, take, skip, reverseOrder, leftDt, rightDt);
        if (foundByKeywords == null)
            return null;
        return _context.Articles.Select(x => new ApiNews
        {
            NewsText = x.Text, RegistrationDate = x.RegistrationDate, SourceName = x.SourceName,
            SourceSite = x.SourceSite, Title = x.Title, Header = x.Header, Id = x.Id
        }).Where(x => foundByKeywords.Contains(x.Id)).ToArray();
    }
}