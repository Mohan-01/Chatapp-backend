using Microsoft.Extensions.DependencyInjection;
using ChatApp.UserService.Core.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Constants;
using System.Text;
using System.Text.Json;
using Shared.Configurations;

namespace ChatApp.UserService.Infrastructure.Consumers
{
    public class UserRegisteredDlqConsumer : IConsumer
    {
        private readonly IRabbitMQConnection _rabbitConnection;
        private readonly ILogger<IConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public UserRegisteredDlqConsumer(IRabbitMQConnection rabbitConnection, ILogger<IConsumer> logger, IServiceProvider serviceProvider)
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
                        _logger.LogInformation("📦 Attempting to start UserRegisteredDlqConsumer...");
                        channel?.Dispose();
                        channel = _rabbitConnection.GetConnection().CreateModel();

                        if (channel == null || !channel.IsOpen)
                        {
                            _logger.LogWarning("⚠️ DLQ channel not open. Attempting reconnection...");
                            _rabbitConnection.Reconnect();
                            continue;
                        }

                        // Declare the DLQ queue
                        _rabbitConnection.DeclareQueue(QueueNames.UserRegisteredDlqQueue, Exchanges.UserEventsExchange, RoutingKeys.UserRegistered, channel);

                        _logger.LogInformation("✅ DLQ declared. Listening on {QueueName}", QueueNames.UserRegisteredDlqQueue);

                        var consumer = new AsyncEventingBasicConsumer(channel);
                        consumer.Received += async (model, ea) =>
                        {
                            try
                            {
                                var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                                _logger.LogWarning("☠️ Message routed to DLQ: {Message}", message);

                                // Optionally deserialize and log details
                                var @event = JsonSerializer.Deserialize<object>(message);
                                _logger.LogWarning("📄 DLQ Payload: {@Event}", @event);

                                // Simulate persistence or alerting logic here (optional)
                                await Task.CompletedTask;

                                channel.BasicAck(ea.DeliveryTag, false);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "❌ Error handling DLQ message. Skipping.");
                                channel.BasicAck(ea.DeliveryTag, false); // Prevent message reprocessing
                            }
                        };

                        channel.BasicConsume(queue: QueueNames.UserRegisteredDlqQueue, autoAck: false, consumer: consumer);

                        _logger.LogInformation("Consumer is now actively listening on {QueueName}", QueueNames.UserRegisteredDlqQueue);

                        await Task.Delay(Timeout.Infinite);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "💥 DLQ Consumer startup failure. Retrying in 5 seconds...");
                        channel?.Dispose();
                        channel = null;
                        await Task.Delay(5000);
                    }
                }
            });
        }
    }
}
