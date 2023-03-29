using Infrastructure;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EKsuNewsScrapperService.Domain;

public class NewsRabbitMqProducer:RabbitProducer
{
    public NewsRabbitMqProducer(ConnectionFactory connectionFactory, ILogger<RabbitMqClientBase> logger, ILogger<RabbitProducer> producerBaseLogger) : base(connectionFactory, logger, producerBaseLogger)
    {
    }

    protected override string GetExchange() => "news";
    protected override string GetQueueAndExchangeRoutingKey() => "news";
    protected override string RoutingKeyName => "news";
    protected override string AppId => "EKsuNewsScrapperService";
}