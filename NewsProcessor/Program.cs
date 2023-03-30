// See https://aka.ms/new-console-template for more information

using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewsProcessor.Domain;
using NewsProcessor.Processor;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using RabbitMQ.Client;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

class Program
{
    public static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
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
            })
            .RunConsoleAsync();
    }

    class Not : RabbitConsumer, IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Processor> _processorLogger;
        private readonly RabbitConsumer _rabbitConsumer;

        public Not(IConfiguration configuration, ILogger<Processor> processorLogger, ILogger<RabbitConsumer> baseLogger,
            ILogger<RabbitMqClientBase> clientLogger, ConnectionFactory factory) : base(factory, baseLogger,
            clientLogger)
        {
            _configuration = configuration;
            _processorLogger = processorLogger;
            SetConsume<NewsMessageEntry[]>((news) =>
            {
                var processor = new Processor(_processorLogger, _configuration.GetSection("keyWords").Get<string[]>()!);
                processor.Process(news);
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