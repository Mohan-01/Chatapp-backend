using System.Text;
using System.Text.Json;
using AuthService.Core.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Configurations;

namespace AuthService.Infrastructure.Producers
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IRabbitMQConnection _rabbitConnection;
        private readonly ILogger<IEventPublisher> _logger;

        public EventPublisher(IRabbitMQConnection rabbitConnection, ILogger<IEventPublisher> logger)
        {
            _rabbitConnection = rabbitConnection;
            _logger = logger; 

        }

        public void Publish<T>(string queueName, T @event, bool withDeadLetter = false)
        {
            try
            {
                using var channel = _rabbitConnection.GetConnection().CreateModel();
                // Ensure queue exists before publishing
                _rabbitConnection.DeclareQueue(queueName, channel, withDeadLetter);

                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                //var properties = _rabbitConnection.Channel.CreateBasicProperties();
                //properties.Persistent = true;

                channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: null,
                    body: body
                );

                _logger.LogInformation("Event published to queue {QueueName}: {Message}", queueName, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event to queue {QueueName}", queueName);
            }
        }
    }
}
