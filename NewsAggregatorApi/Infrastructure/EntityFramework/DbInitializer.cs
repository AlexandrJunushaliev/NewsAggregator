namespace NewsAggregatorApi.Infrastructure.EntityFramework;

public static class DbInitializer
{
    public static void Initialize(NewsAggregatorContext context)
    {
        context.Database.EnsureCreated();
    }
}