using System.Text;
using System.Text.Json;
using Shared.Producers;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Configurations;

namespace ChatApp.AuthService.Infrastructure.Producers.NotUsing
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
        public void Publish<T>(string exchangeName, string routingKey, T @event)
        {
            try
            {
                using var channel = _rabbitConnection.GetConnection().CreateModel();

                // Ensure exchange exists (topic type)
                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true, autoDelete: false);

                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;  // Make message persistent

                channel.BasicPublish(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body
                );

                _logger.LogInformation("Event published to queue {RoutingKey}: {Message}", routingKey, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event to queue {RoutingKey}", routingKey);
            }
        }

    }
}
