using ChatApp.ChatService.Core.Interfaces;
using ChatApp.ChatService.Core.RequestResponseModels.Message;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Configurations;
using Shared.Constants;
using Shared.EventContracts;
using System.Text;
using System.Text.Json;

namespace ChatApp.ChatService.Infrastructure.Consumers
{
    public class MessageSentConsumer: IConsumer
    {
        private readonly IRabbitMQConnection _rabbitConnection;
        private readonly ILogger<IConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MessageSentConsumer(IRabbitMQConnection rabbitConnection, ILogger<IConsumer> logger, IServiceProvider serviceProvider)
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
                        _logger.LogInformation("Attempting to start MessageSentConsumer...");
                        using var channel = _rabbitConnection.GetConnection().CreateModel();

                        if (channel == null || !channel.IsOpen)
                        {
                            _logger.LogError("RabbitMQ channel is not open. Attempting reconnection...");
                            _rabbitConnection.Reconnect();
                        }

                        // Declare queues safely
                        _rabbitConnection.DeclareQueue(QueueNames.SendMessageQueue, Exchanges.ChatMessageExchange, RoutingKeys.ChatMessageSent, channel, withDeadLetter: true);

                        _logger.LogInformation("MessageSentConsumer started listening on {QueueName}", QueueNames.EmailChangedQueue);

                        var consumer = new AsyncEventingBasicConsumer(channel);
                        consumer.Received += async (model, ea) =>
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var _messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();

                            try
                            {
                                var body = ea.Body.ToArray();
                                var message = Encoding.UTF8.GetString(body);
                                var @event = JsonSerializer.Deserialize<SendMessageDto>(message);

                                if (@event == null)
                                {
                                    throw new Exception("Invalid EmailChanged event received. Event is null.");
                                }

                                await _messageService.SendMessageAsync(@event);
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
