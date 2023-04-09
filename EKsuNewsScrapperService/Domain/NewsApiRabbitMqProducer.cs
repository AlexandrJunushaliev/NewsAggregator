using Infrastructure;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EKsuNewsScrapperService.Domain;

public class NewsApiRabbitMqProducer:RabbitProducer
{
    public NewsApiRabbitMqProducer(ConnectionFactory connectionFactory, ILogger<RabbitMqClientBase> logger, ILogger<RabbitProducer> producerBaseLogger) : base(connectionFactory, logger, producerBaseLogger)
    {
    }

    protected override string GetExchange() => "newsForApi";
    protected override string GetQueueAndExchangeRoutingKey() => "newsForApi";
    protected override string RoutingKeyName => "newsForApi";
    protected override string AppId => "EKsuNewsScrapperService";
}