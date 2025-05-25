using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Shared.Constants;

namespace Shared.Configurations
{
    public class RabbitMQConnection : IRabbitMQConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private readonly ILogger<IRabbitMQConnection> _logger;
        private readonly object _lock = new();

        public RabbitMQConnection(IConnectionFactory connectionFactory, ILogger<IRabbitMQConnection> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IConnection GetConnection()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                lock (_lock)
                {
                    if (_connection == null || !_connection.IsOpen)
                    {
                        _connection = _connectionFactory.CreateConnection();
                        _logger.LogInformation("🔌 RabbitMQ connection established.");
                    }
                }
            }

            return _connection;
        }

        public IConnection Reconnect()
        {
            Dispose();
            _logger.LogInformation("🔄 Reconnecting to RabbitMQ...");
            return GetConnection();
        }

        public void DeclareQueue(string queueName, string exchangeName, string routingKey, IModel channel, bool withDeadLetter = false)
        {
            var arguments = new Dictionary<string, object>();

            if (withDeadLetter)
            {
                var dlqName = $"{queueName}.dlq";
                const string dlxName = "dead_letter_exchange";

                // Declare DLX and DLQ
                channel.ExchangeDeclare(dlxName, ExchangeType.Topic, durable: true);
                channel.QueueDeclare(dlqName, durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind(dlqName, dlxName, routingKey: routingKey);

                // Bind DLX to main queue
                arguments["x-dead-letter-exchange"] = dlxName;
                arguments["x-dead-letter-routing-key"] = routingKey;
            }

            // Declare topic exchange
            channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true);

            // Declare the main queue with optional DLQ arguments
            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: arguments.Count > 0 ? arguments : null);

            // Bind queue to exchange using routing key
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);


            _logger.LogInformation("📥 Queue declared: {QueueName} bound to exchange {Exchange} with routing key {RoutingKey} (DLQ: {WithDLQ})",
        queueName, exchangeName, routingKey, withDeadLetter);
        }

        public void Dispose()
        {
            try
            {
                if (_connection?.IsOpen == true)
                {
                    _connection.Close();
                    _logger.LogInformation("🔌 RabbitMQ connection closed.");
                }

                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while disposing RabbitMQ connection.");
            }
        }
    }
}
