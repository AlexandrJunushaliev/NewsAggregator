using NewsProcessor.Domain;
using NewsProcessor.Index;
using NLog.Config;
using NLog.Extensions.Logging;
using RabbitMQ.Client;

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
        services.AddSingleton<SearchIndex>(sp =>
        {
            var index = new SearchIndex(sp.GetService<ILogger<SearchIndex>>()!);
            index.LoadSnapshot();
            return index;
        });
    }
    
    
}