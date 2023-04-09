using NewsProcessor.Domain;
using NewsProcessor.Index;
using NLog.Config;
using NLog.Extensions.Logging;
using RabbitMQ.Client;
using Snowball;

namespace NewsProcessor;

class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder.Services);
        builder.Services.AddControllers();

        var app = builder.Build();
        app.MapControllers();
        await app.RunAsync();
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new SearchIndexEntryIdJsonConverter());
        });
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddNLog(new XmlLoggingConfiguration("NLog.config"));
        });
        services.AddHostedService<NewsProcessorService>();
        services.AddSingleton<ConnectionFactory>(_ => new ConnectionFactory()
        {
            HostName = "localhost"
        });
        services.AddSingleton<ReverseSearchIndex>(sp =>
        {
            var reverseSearchIndex = new ReverseSearchIndex(sp.GetService<ILogger<ReverseSearchIndex>>()!,
                sp.GetService<ILogger<SearchIndexBase<Dictionary<string, SearchIndexSortedSet>>>>()!);
            reverseSearchIndex.LoadSnapshot();
            return reverseSearchIndex;
        });
        services.AddSingleton<SearchIndex>(sp =>
        {
            var searchIndex = new SearchIndex(sp.GetService<ILogger<SearchIndex>>()!,
                sp.GetService<ILogger<SearchIndexBase<Dictionary<SearchIndexEntryId, HashSet<string>>>>>()!);
            searchIndex.LoadSnapshot();
            return searchIndex;
        });
        services.AddSingleton<RussianStemmer>(_ => new RussianStemmer());
    }
}