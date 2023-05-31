using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Infrastructure;

public abstract class RabbitMqClientBase : IDisposable
{
    protected const string VirtualHost = "localhost";
    protected readonly string Exchange;
    protected readonly string Queue;
    protected readonly string QueueAndExchangeRoutingKey;
    protected abstract string GetExchange();
    protected abstract string GetQueueAndExchangeRoutingKey();

    protected IModel? Channel { get; private set; }
    private IConnection? _connection;
    private readonly ConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqClientBase> _logger;

    protected RabbitMqClientBase(
        ConnectionFactory connectionFactory,
        ILogger<RabbitMqClientBase> logger)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Exchange = $"{VirtualHost}.{GetExchange()}";
        // ReSharper disable once VirtualMemberCallInConstructor
        Queue = $"{VirtualHost}.{GetQueueAndExchangeRoutingKey()}";
        // ReSharper disable once VirtualMemberCallInConstructor
        QueueAndExchangeRoutingKey = $"{GetQueueAndExchangeRoutingKey()}";
        _connectionFactory = connectionFactory;
        _logger = logger;
        ConnectToRabbitMq();
    }

    private void ConnectToRabbitMq()
    {
        try
        {
            if (_connection == null || _connection.IsOpen == false)
            {
                _connection = _connectionFactory.CreateConnection();
            }

            if (Channel == null || Channel.IsOpen == false)
            {
                Channel = _connection.CreateModel();
                Channel.ExchangeDeclare(exchange: Exchange, type: "direct", durable: true, autoDelete: false);
                Channel.QueueDeclare(queue: Queue, durable: false, exclusive: false, autoDelete: false);
                Channel.QueueBind(queue: Queue, exchange: Exchange,
                    routingKey: QueueAndExchangeRoutingKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error occured while connected to RabbitMq");
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            Channel?.Close();
            Channel?.Dispose();
            Channel = null;

            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Cannot dispose RabbitMQ channel or connection");
        }
    }
}
