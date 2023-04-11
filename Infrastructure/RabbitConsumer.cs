using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure;

public abstract class RabbitConsumer : RabbitMqClientBase
{
    private readonly ILogger<RabbitConsumer> _logger;
    protected abstract string QueueName { get; }

    public RabbitConsumer(
        ConnectionFactory connectionFactory,
        ILogger<RabbitConsumer> consumerLogger,
        ILogger<RabbitMqClientBase> logger) :
        base(connectionFactory, logger)
    {
        _logger = consumerLogger;
    }

    public void SetConsume<T>(Action<T> func)
    {
        var consumer = new EventingBasicConsumer(Channel);
        consumer.Received += (_, ea) =>
        {
            var stringMessage = Encoding.UTF8.GetString(ea.Body.Span);
            var message = JsonSerializer.Deserialize<T>(stringMessage);
            if (message is null)
            {
                _logger.LogWarning($"Rabbit: Empty or invalid message was returned from {QueueName}.{stringMessage}");
            }
            else
            {
                try
                {
                    func(message);
                }
                catch(Exception e)
                {
                    _logger.LogCritical($"Processing of message failed: {e}. Will try to requeue");
                    Channel!.BasicReject(ea.DeliveryTag, true);
                }
                finally
                {
                    Channel!.BasicAck(ea.DeliveryTag, false);
                }
            }
        };
        Channel.BasicConsume(QueueName, autoAck: false, consumer);
    }
}