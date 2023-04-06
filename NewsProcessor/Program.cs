// See https://aka.ms/new-console-template for more information

using Infrastructure;
using NewsProcessor;
using NewsProcessor.Domain;
using NewsProcessor.Index;
using NewsProcessor.Processor;
using NLog.Config;
using NLog.Extensions.Logging;
using RabbitMQ.Client;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

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
        services.AddSingleton<ConnectionFactory>(sp => new ConnectionFactory()
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