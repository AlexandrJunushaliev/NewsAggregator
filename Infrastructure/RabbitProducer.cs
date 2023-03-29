using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Infrastructure;

public abstract class RabbitProducer : RabbitMqClientBase
{
    private readonly ILogger<RabbitProducer> _logger;
    protected abstract string RoutingKeyName { get; }
    protected abstract string AppId { get; }

    protected RabbitProducer(
        ConnectionFactory connectionFactory,
        ILogger<RabbitMqClientBase> logger,
        ILogger<RabbitProducer> producerBaseLogger) :
        base(connectionFactory, logger) => _logger = producerBaseLogger;

    public virtual void Publish<T>(T message)
    {
        try
        {
            var sw = new Stopwatch();
            var serialized = JsonSerializer.Serialize(message, _options);
            var properties = Channel!.CreateBasicProperties();
            properties.AppId = AppId;
            properties.ContentType = "application/json";
            properties.DeliveryMode = 1; // Doesn't persist to disk
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            Channel.BasicPublish(exchange: Exchange, routingKey: RoutingKeyName,
                body: Encoding.UTF8.GetBytes(serialized),
                basicProperties: properties);
            _logger.LogTrace(
                $"Rabbit: Message with props:{JsonSerializer.Serialize(properties)}was sent to exchange {Exchange} with routing key {RoutingKeyName} in {sw.Elapsed}: {serialized}");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,
                $"Error while publishing to exchange {Exchange} with routing key {RoutingKeyName}");
        }
    }

    private static JsonSerializerOptions _options = new()
        { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
}