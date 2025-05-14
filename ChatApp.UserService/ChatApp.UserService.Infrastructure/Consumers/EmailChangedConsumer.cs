using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using ChatApp.UserService.Core.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Configurations;
using Shared.Constants;
using Shared.EventContracts;

namespace ChatApp.UserService.Infrastructure.Consumers
{
    public class EmailChangedConsumer : IEmailChangedConsumer
    {
        private readonly IRabbitMQConnection _rabbitConnection;
        private readonly ILogger<IEmailChangedConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public EmailChangedConsumer(IRabbitMQConnection rabbitConnection, ILogger<IEmailChangedConsumer> logger, IServiceProvider serviceProvider)
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
                        _logger.LogInformation("Attempting to start EmailChangedConsumer...");
                        using var channel = _rabbitConnection.GetConnection().CreateModel();

                        if (channel == null || !channel.IsOpen)
                        {
                            _logger.LogError("RabbitMQ channel is not open. Attempting reconnection...");
                            _rabbitConnection.Reconnect();
                        }

                        // Declare queues safely
                        _rabbitConnection.DeclareQueue(QueueNames.EmailChangedQueue, channel, withDeadLetter: false);

                        _logger.LogInformation("EmailChangedConsumer started listening on {QueueName}", QueueNames.EmailChangedQueue);

                        var consumer = new AsyncEventingBasicConsumer(channel);
                        consumer.Received += async (model, ea) =>
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var _profileService = scope.ServiceProvider.GetRequiredService<IUserEventsService>();

                            try
                            {
                                var body = ea.Body.ToArray();
                                var message = Encoding.UTF8.GetString(body);
                                var @event = JsonSerializer.Deserialize<EmailChangedEvent>(message);

                                if (@event == null)
                                {
                                    throw new Exception("Invalid EmailChanged event received. Event is null.");
                                }

                                await _profileService.ChangeEmailAsync(@event);
                                channel.BasicAck(ea.DeliveryTag, false);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error processing EmailChanged event. Sending to DLQ.");
                                channel.BasicNack(ea.DeliveryTag, false, false);
                            }
                        };

                        channel.BasicConsume(QueueNames.EmailChangedQueue, false, consumer);

                        _logger.LogInformation("Consumer is now actively listening on {QueueName}", QueueNames.EmailChangedQueue);

                        // Keep the consumer running
                        await Task.Delay(Timeout.Infinite);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error starting EmailChangedConsumer. Retrying in 5 seconds...");
                        await Task.Delay(5000); // Retry after 5 seconds
                    }
                }
            });
        }
    }
}
