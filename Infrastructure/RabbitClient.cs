using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.Extensions.Logging;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ILogger = NLog.ILogger;

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
        catch (Exception _)
        {
            _logger.LogCritical("Error occured while connected to RabbitMq");
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

public class RabbitClient
{
    private readonly string _queueName;
    private readonly ConnectionFactory _factory;
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public RabbitClient(string hostname, string queueName)
    {
        _queueName = queueName;
        _factory = new ConnectionFactory() { HostName = hostname };

        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }


    public void SendMessageJson<T>(T message)
    {
        var sw = new Stopwatch();
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();
        var serialized = JsonSerializer.Serialize(message, _options);
        channel.BasicPublish(string.Empty, _queueName, body: Encoding.UTF8.GetBytes(serialized));
        Logger.Trace($"Rabbit: Message was sent to queue {_queueName} in {sw.Elapsed}: {serialized}");
    }

    public void SubscribeToQueueWithAsync<T>(Func<T, Task> func)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            var stringMessage = Encoding.UTF8.GetString(ea.Body.Span);
            var message = JsonSerializer.Deserialize<T>(stringMessage);
            if (message is null)
            {
                Logger.Warn($"Rabbit: Empty or invalid message was returned from {_queueName}.{stringMessage}");
            }
            else
                await func(message);
        };
        channel.BasicConsume(_queueName, autoAck: false, consumer);
    }

    public void SubscribeToQueue<T>(Action<T> func)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (_, ea) =>
        {
            var stringMessage = Encoding.UTF8.GetString(ea.Body.Span);
            var message = JsonSerializer.Deserialize<T>(stringMessage);
            if (message is null)
            {
                Logger.Warn($"Rabbit: Empty or invalid message was returned from {_queueName}.{stringMessage}");
            }
            else
            {
                try
                {
                    func(message);
                }
                finally
                {
                    _factory.CreateConnection().CreateModel().BasicAck(ea.DeliveryTag, false);
                }
            }
        };
        channel.BasicConsume(_queueName, autoAck: false, consumer);
    }

    private static JsonSerializerOptions _options = new()
        { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
}