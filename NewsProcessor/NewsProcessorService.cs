using Infrastructure;
using NewsProcessor.Domain;
using NewsProcessor.Index;
using RabbitMQ.Client;
using Snowball;

namespace NewsProcessor;

class NewsProcessorService : RabbitConsumer, IHostedService
{
    public NewsProcessorService(IConfiguration configuration, ILogger<Processor.Processor> processorLogger, ILogger<RabbitConsumer> baseLogger,
        ILogger<RabbitMqClientBase> clientLogger, ConnectionFactory factory, ReverseSearchIndex reverseIndex, SearchIndex searchIndex, RussianStemmer russianStemmer) : base(factory,
        baseLogger,
        clientLogger)
    {
        SetConsume<NewsMessageEntry[]>(news =>
        {
            var processor = new Processor.Processor(processorLogger, configuration.GetSection("keyWords").Get<string[]>()!, russianStemmer.Stem);
            var (reverseProcessed, forwardProcessed ) = processor.Process(news);
            Task.WhenAny(reverseIndex.AddToIndex(reverseProcessed, configuration.GetSection("keyWordsToDelete").Get<HashSet<string>>()), searchIndex.AddToIndex(forwardProcessed, configuration.GetSection("keyWordsToDelete").Get<HashSet<string>>())).GetAwaiter().GetResult();
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