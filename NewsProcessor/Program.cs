// See https://aka.ms/new-console-template for more information

using Infrastructure;
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
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddNLog(new XmlLoggingConfiguration("NLog.config"));
        });
        services.AddHostedService<Not>();
        services.AddSingleton<ConnectionFactory>(sp => new ConnectionFactory()
        {
            HostName = "localhost"
        });
        services.AddSingleton<SearchIndex>(services =>
        {
            var index = new SearchIndex(services.GetService<ILogger<SearchIndex>>()!);
            index.LoadSnapshot();
            return index;
        });
    }
    
    class Not : RabbitConsumer, IHostedService
    {
        public Not(IConfiguration configuration, ILogger<Processor> processorLogger, ILogger<RabbitConsumer> baseLogger,
            ILogger<RabbitMqClientBase> clientLogger, ConnectionFactory factory, SearchIndex index) : base(factory,
            baseLogger,
            clientLogger)
        {
            SetConsume<NewsMessageEntry[]>((news) =>
            {
                var processor = new Processor(processorLogger, configuration.GetSection("keyWords").Get<string[]>()!);
                index.AddToIndex(processor.Process(news)).GetAwaiter().GetResult();
            });
        }


        protected override string GetExchange() => "news";
        protected override string GetQueueAndExchangeRoutingKey() => "news";

        protected override string QueueName => "localhost.news";

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }
    }
}