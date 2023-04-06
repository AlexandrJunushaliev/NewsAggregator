using Infrastructure;
using NewsProcessor.Domain;
using NewsProcessor.Index;
using RabbitMQ.Client;

namespace NewsProcessor;

class NewsProcessorService : RabbitConsumer, IHostedService
{
    public NewsProcessorService(IConfiguration configuration, ILogger<Processor.Processor> processorLogger, ILogger<RabbitConsumer> baseLogger,
        ILogger<RabbitMqClientBase> clientLogger, ConnectionFactory factory, SearchIndex index) : base(factory,
        baseLogger,
        clientLogger)
    {
        SetConsume<NewsMessageEntry[]>((news) =>
        {
            var processor = new Processor.Processor(processorLogger, configuration.GetSection("keyWords").Get<string[]>()!);
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