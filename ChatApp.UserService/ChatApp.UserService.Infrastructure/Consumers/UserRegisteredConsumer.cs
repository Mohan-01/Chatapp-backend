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
    public class UserRegisteredConsumer: IUserRegisteredConsumer
    {
        private readonly IRabbitMQConnection _rabbitConnection;
        private readonly ILogger<IUserRegisteredConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public UserRegisteredConsumer(IRabbitMQConnection rabbitConnection, ILogger<IUserRegisteredConsumer> logger, IServiceProvider serviceProvider)
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
                        _logger.LogInformation("Attempting to start UserRegisteredConsumer...");
                        using var channel = _rabbitConnection.GetConnection().CreateModel();

                        if (channel == null || !channel.IsOpen)
                        {
                            _logger.LogError("RabbitMQ channel is not open. Attempting reconnection...");
                            _rabbitConnection.Reconnect();
                        }

                        // Declare queues safely
                        _rabbitConnection.DeclareQueue(QueueNames.UserRegisteredQueue, channel, withDeadLetter: false);

                        _logger.LogInformation("UserRegisteredConsumer started listening on {QueueName}", QueueNames.UserRegisteredQueue);

                        var consumer = new AsyncEventingBasicConsumer(channel);
                        consumer.Received += async (model, ea) =>
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var _profileService = scope.ServiceProvider.GetRequiredService<IUserEventsService>();

                            try
                            {
                                var body = ea.Body.ToArray();
                                var message = Encoding.UTF8.GetString(body);
                                var @event = JsonSerializer.Deserialize<UserRegisteredEvent>(message);

                                if (@event == null)
                                {
                                    throw new Exception("Invalid UserRegistered event received. Event is null.");
                                }

                                await _profileService.CreateUserProfileAsync(@event);
                                channel.BasicAck(ea.DeliveryTag, false);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error processing UserRegistered event. Sending to DLQ.");
                                channel.BasicNack(ea.DeliveryTag, false, false);
                            }
                        };

                        channel.BasicConsume(QueueNames.UserRegisteredQueue, false, consumer);

                        _logger.LogInformation("Consumer is now actively listening on {QueueName}", QueueNames.UserRegisteredQueue);

                        // Keep the consumer running
                        await Task.Delay(Timeout.Infinite);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error starting UserRegisteredConsumer. Retrying in 5 seconds...");
                        await Task.Delay(5000); // Retry after 5 seconds
                    }
                }
            });
        }

    }
}
