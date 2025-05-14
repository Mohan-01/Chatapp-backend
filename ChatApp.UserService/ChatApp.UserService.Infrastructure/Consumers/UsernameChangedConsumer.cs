using Microsoft.Extensions.DependencyInjection;
using ChatApp.UserService.Core.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Configurations;
using Shared.Constants;
using Shared.EventContracts;
using System.Text;
using System.Text.Json;

namespace ChatApp.UserService.Infrastructure.Consumers
{
    public class UsernameChangedConsumer : IUsernameChangedConsumer
    {
        private readonly IRabbitMQConnection _rabbitConnection;
        private readonly ILogger<IUsernameChangedConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public UsernameChangedConsumer(IRabbitMQConnection rabbitConnection, ILogger<IUsernameChangedConsumer> logger, IServiceProvider serviceProvider)
        {
            _rabbitConnection = rabbitConnection;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public void StartConsuming()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        _logger.LogInformation("Attempting to start UsernameChangedConsumer...");
                        using var channel = _rabbitConnection.GetConnection().CreateModel();

                        if (channel == null || !channel.IsOpen)
                        {
                            _logger.LogError("RabbitMQ channel is not open. Attempting reconnection...");
                            _rabbitConnection.Reconnect();
                        }

                        // Declare queues safely
                        _rabbitConnection.DeclareQueue(QueueNames.UsernameChangedQueue, channel, withDeadLetter: false);

                        _logger.LogInformation("UsernameChangedConsumer started listening on {QueueName}", QueueNames.UsernameChangedQueue);

                        var consumer = new AsyncEventingBasicConsumer(channel);
                        consumer.Received += async (model, ea) =>
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var _profileService = scope.ServiceProvider.GetRequiredService<IUserEventsService>();

                            try
                            {
                                var body = ea.Body.ToArray();
                                var message = Encoding.UTF8.GetString(body);
                                var @event = JsonSerializer.Deserialize<UsernameChangedEvent>(message);

                                if (@event == null)
                                {
                                    throw new Exception("Invalid UsernameChanged event received. Event is null.");
                                }

                                await _profileService.ChangeUsernameAsync(@event);
                                channel.BasicAck(ea.DeliveryTag, false);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error processing UsernameChanged event. Sending to DLQ.");
                                channel.BasicNack(ea.DeliveryTag, false, false);
                            }
                        };

                        channel.BasicConsume(QueueNames.UsernameChangedQueue, false, consumer);

                        _logger.LogInformation("Consumer is now actively listening on {QueueName}", QueueNames.UsernameChangedQueue);

                        // Keep the consumer running
                        await Task.Delay(Timeout.Infinite);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error starting UsernameChangedConsumer. Retrying in 5 seconds...");
                        await Task.Delay(5000); // Retry after 5 seconds
                    }
                }
            });
        }
    }
}
