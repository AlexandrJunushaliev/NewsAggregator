using Microsoft.AspNetCore.Mvc;
using NewsAggregatorApi.Infrastructure.EntityFramework;
using NewsAggregatorApi.Models;

namespace NewsAggregatorApi.Controllers;

[Controller]
public class NewsController
{
    private readonly NewsAggregatorContext _context;
    public NewsController(NewsAggregatorContext context)
    {
        _context = context;
    }
    public ApiNews[]? GetNews()
    {
        var a = _context.Articles.Select(x => new ApiNews()
        {
            Keywords = null, NewsText = x.Text, RegistrationDate = x.RegistrationDate, SourceName = x.SourceName,
            SourceSite = x.SourceSite, Title = x.Title
        });
        return null;
    }
}