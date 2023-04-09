using Infrastructure;
using NewsProcessor.Domain;
using NewsProcessor.Index;
using RabbitMQ.Client;

namespace NewsProcessor;

class NewsProcessorService : RabbitConsumer, IHostedService
{
    public NewsProcessorService(IConfiguration configuration, ILogger<Processor.Processor> processorLogger, ILogger<RabbitConsumer> baseLogger,
        ILogger<RabbitMqClientBase> clientLogger, ConnectionFactory factory, ReverseSearchIndex reverseIndex, SearchIndex searchIndex) : base(factory,
        baseLogger,
        clientLogger)
    {
        SetConsume<NewsMessageEntry[]>((news) =>
        {
            var processor = new Processor.Processor(processorLogger, configuration.GetSection("keyWords").Get<string[]>()!);
            var (reverseProcessed, forwardProcessed ) = processor.Process(news);
            Task.WhenAny(reverseIndex.AddToIndex(reverseProcessed), searchIndex.AddToIndex(forwardProcessed)).GetAwaiter().GetResult();
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