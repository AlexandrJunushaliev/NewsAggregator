﻿using Microsoft.AspNetCore.Mvc;
using NewsAggregatorApi.Domain;
using NewsAggregatorApi.Infrastructure.EntityFramework;
using NewsAggregatorApi.Models;
using SelectPdf;
using Utils;

namespace NewsAggregatorApi.Controllers;

[Controller]
[Route("news")]
public class NewsController : Controller
{
    private readonly NewsAggregatorContext _context;
    private readonly SearchIndexClient _searchIndexClient;
    private readonly ILogger<NewsController> _logger;
    private readonly IHttpContextFactory _httpContextFactory;

    public NewsController(NewsAggregatorContext context, SearchIndexClient searchIndexClient,
        ILogger<NewsController> logger, IHttpContextFactory httpContextFactory)
    {
        _context = context;
        _searchIndexClient = searchIndexClient;
        _logger = logger;
        _httpContextFactory = httpContextFactory;
    }

    [HttpGet]
    [Route("")]
    public async Task<IEnumerable<ApiNews>?> GetNews(string[]? keywords, int take = 30, int skip = 0,
        bool reverseOrder = false,
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
                SourceSite = x.SourceSite, Title = x.Title, Header = x.Header, Id = x.Id, Status = x.Status,
                NewsUrl = x.ArticleUrl, Images = x.Images, Videos = x.VideoUrls, MainPicture = x.Picture
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
                if (idsToKeywords.TryGetValue(entry.Id, out var keyword))
                {
                    entry.Keywords = keyword;
                }
            }

            return news;
        }

        var foundByKeywords =
            await _searchIndexClient.SearchByKeywordsWithKeywords(keywords, take, skip, reverseOrder, leftDt, rightDt);
        if (foundByKeywords == null)
            return null;
        var apiNews = _context.Articles.Select(x => new ApiNews
        {
            NewsText = x.Text, RegistrationDate = x.RegistrationDate, SourceName = x.SourceName,
            SourceSite = x.SourceSite, Title = x.Title, Header = x.Header, Id = x.Id
        }).Where(x => foundByKeywords.Keys.Contains(x.Id)).ToArray();
        foreach (var entry in apiNews)
        {
            entry.Keywords = foundByKeywords[entry.Id];
        }

        return !reverseOrder
            ? apiNews.OrderByDescending(x => x.RegistrationDate)
            : apiNews.OrderBy(x => x.RegistrationDate);
    }

    [HttpGet]
    [Route("getKeywords")]
    public async Task<string[]?> GetAvailableKeywords()
    {
        return await _searchIndexClient.GetAvailableKeywords();
    }

    [HttpGet]
    [Route("getPdf")]
    public IActionResult? GetPdf(string id)
    {
        var article = _context.Find<Article>(id);
        if (article == null)
            return null;
        var converter = new HtmlToPdf();
        var doc = converter.ConvertUrl(article.ArticleUrl);
        return File(doc.Save(),"application/pdf") ;
    }
}