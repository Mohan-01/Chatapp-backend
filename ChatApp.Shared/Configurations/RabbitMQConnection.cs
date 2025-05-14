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
            _logger = logger;
            _connectionFactory = connectionFactory;
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
                    }
                }
            }
            return _connection;
        }

        public IConnection Reconnect()
        {
            Dispose();
            _logger.LogInformation("Attempting to reconnect to RabbitMQ...");
            return GetConnection();
        }

        
        private void DeclareExchange(string exchangeName, IModel channel, string exchangeType)
        {
            try
            {
                channel.ExchangeDeclare(exchangeName, exchangeType, durable: true);
                _logger.LogInformation("Exchange declared: {ExchangeName}", exchangeName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error declaring exchange {ExchangeName}", exchangeName);
            }
        }

        public void DeclareQueue(string queueName, IModel channel, bool withDeadLetter = false)
        {
            var arguments = new Dictionary<string, object>();

            if (withDeadLetter)
            {
                var dlqName = $"{queueName}.dlq";
                channel.ExchangeDeclare("dead_letter_exchange", ExchangeType.Direct, durable: true);
                channel.QueueDeclare(dlqName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(dlqName, "dead_letter_exchange", routingKey: dlqName);

                arguments["x-dead-letter-exchange"] = "dead_letter_exchange";
                arguments["x-dead-letter-routing-key"] = dlqName;
            }

            try
            {
                //Channel.QueueDelete(queueName);
                //_logger.LogInformation("Deleted existing queue {QueueName} before redeclaration.", queueName);
            }
            catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex) when (ex.ShutdownReason.ReplyCode == 404)
            {
                _logger.LogWarning("Queue {QueueName} does not exist. Skipping deletion.", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting queue {QueueName}", queueName);
                throw;
            }

            //Channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _logger.LogInformation("Queue declared: {QueueName} (DLQ: {WithDeadLetter})", queueName, withDeadLetter);
        }
       
        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        /*
        public void Dispose()
        {
            try
            {
                if (Channel?.IsOpen == true)
                {
                    Channel.Close();
                    _logger.LogInformation("RabbitMQ channel closed.");
                }

                if (_connection?.IsOpen == true)
                {
                    _connection.Close();
                    _logger.LogInformation("RabbitMQ connection closed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while closing RabbitMQ connection or channel.");
            }
        }
        */
    }
}
