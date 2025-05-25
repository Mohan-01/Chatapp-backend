using ChatApp.UserService.Core.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Shared.Configurations;
using Shared.Constants;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using Shared.EventContracts;

public class UserRegisteredConsumer : IConsumer
{
    private readonly IRabbitMQConnection _rabbitConnection;
    private readonly ILogger<IConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private const int MaxRetryAttempts = 3;

    public UserRegisteredConsumer(IRabbitMQConnection rabbitConnection, ILogger<IConsumer> logger, IServiceProvider serviceProvider)
    {
        _rabbitConnection = rabbitConnection;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public void StartConsuming()
    {
        Task.Run(async () =>
        {
            IModel? channel = null;

            while (true)
            {
                try
                {
                    _logger.LogInformation("🔄 Starting UserRegisteredConsumer...");
                    channel?.Dispose();
                    channel = _rabbitConnection.GetConnection().CreateModel();

                    _rabbitConnection.DeclareQueue(QueueNames.UserRegisteredQueue, Exchanges.UserEventsExchange, RoutingKeys.UserRegistered, channel, withDeadLetter: true);
                    _logger.LogInformation("✅ Queue declared: {QueueName}", QueueNames.UserRegisteredQueue);

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.Received += async (model, ea) =>
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var profileService = scope.ServiceProvider.GetRequiredService<IUserEventsService>();

                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        _logger.LogInformation("📩 Received message: {Message}", message);

                        try
                        {
                            var @event = JsonSerializer.Deserialize<UserRegisteredEvent>(message);
                            if (@event == null)
                                throw new InvalidDataException("❌ Deserialized event is null.");

                            await profileService.CreateUserProfileAsync(@event);
                            channel.BasicAck(ea.DeliveryTag, false);
                            _logger.LogInformation("✅ User profile created. Message acknowledged.");
                        }
                        catch (InvalidDataException ex)
                        {
                            _logger.LogError(ex, "❌ Invalid data. Routing to DLQ.");
                            channel.BasicNack(ea.DeliveryTag, false, false);
                        }
                        catch (Exception ex)
                        {
                            int retryCount = GetRetryCount(ea.BasicProperties);

                            if (retryCount >= MaxRetryAttempts)
                            {
                                _logger.LogWarning("🚫 Retry limit reached ({RetryCount}). Sending to DLQ.", retryCount);
                                channel.BasicNack(ea.DeliveryTag, false, false);
                            }
                            else
                            {
                                var delayMs = (int)Math.Pow(2, retryCount) * 1000;
                                _logger.LogWarning("⏱️ Transient error. Retrying in {DelayMs}ms (Attempt {RetryCount})", delayMs, retryCount + 1);

                                PublishToRetryQueue(channel, ea.Body.ToArray(), ea.BasicProperties, delayMs);
                                channel.BasicAck(ea.DeliveryTag, false);
                            }

                            _logger.LogError(ex, "⚠️ Error processing message.");
                        }
                    };

                    channel.BasicConsume(queue: QueueNames.UserRegisteredQueue, autoAck: false, consumer: consumer);
                    _logger.LogInformation("Consumer is now actively listening on {QueueName}", QueueNames.UserRegisteredQueue);

                    await Task.Delay(Timeout.Infinite);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "💥 Consumer startup failed. Retrying...");
                    channel?.Dispose();
                    channel = null;
                    await Task.Delay(5000);
                }
            }
        });
    }

    private int GetRetryCount(IBasicProperties props)
    {
        if (props.Headers != null && props.Headers.TryGetValue("x-death", out var xDeathObj))
        {
            var xDeath = xDeathObj as List<object>;
            if (xDeath != null && xDeath.Count > 0)
            {
                var deathDict = xDeath[0] as Dictionary<string, object>;
                if (deathDict != null && deathDict.TryGetValue("count", out var countObj))
                {
                    return Convert.ToInt32(countObj);
                }
            }
        }

        return 0;
    }

    private void PublishToRetryQueue(IModel channel, byte[] messageBody, IBasicProperties originalProps, int delayMs)
    {
        var retryQueue = $"{QueueNames.UserRegisteredQueue}.retry.{delayMs}";

        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "" },
            { "x-dead-letter-routing-key", QueueNames.UserRegisteredQueue },
            { "x-message-ttl", delayMs }
        };

        channel.QueueDeclare(retryQueue, durable: true, exclusive: false, autoDelete: true, arguments: args);

        var props = channel.CreateBasicProperties();
        props.Persistent = true;

        // Copy headers
        if (originalProps.Headers != null)
            props.Headers = new Dictionary<string, object>(originalProps.Headers);

        channel.BasicPublish(
            exchange: "",
            routingKey: retryQueue,
            basicProperties: props,
            body: messageBody
        );

        _logger.LogInformation("📤 Message published to retry queue: {RetryQueue} with {DelayMs}ms delay", retryQueue, delayMs);
    }
}
