using Microsoft.EntityFrameworkCore;
using NewsAggregatorApi.Models;

namespace NewsAggregatorApi.Infrastructure.EntityFramework;

public class NewsAggregatorContext: DbContext
{
    public NewsAggregatorContext(DbContextOptions<NewsAggregatorContext> options) : base(options)
    {
    }

    public DbSet<Article> Articles { get; set; } = null!;
}