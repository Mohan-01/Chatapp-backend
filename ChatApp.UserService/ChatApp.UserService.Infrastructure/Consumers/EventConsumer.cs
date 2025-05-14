//using System.Text;
//using System.Text.Json;
//using RabbitMQ.Client;
//using RabbitMQ.Client.Events;
//using Shared.Configurations;

//namespace UserService.Consumers.Dummy
//{
//    public class EventConsumer : IDisposable
//    {
//        private readonly IRabbitMQConnection _rabbitConnection;
//        private readonly ILogger<EventConsumer> _logger;

//        public EventConsumer(IRabbitMQConnection rabbitConnection, ILogger<EventConsumer> logger)
//        {
//            _rabbitConnection = rabbitConnection ?? throw new ArgumentNullException(nameof(rabbitConnection));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//        public void StartConsuming<T>(string queueName, Func<T, Task> handleEvent)
//        {
//            if (string.IsNullOrWhiteSpace(queueName))
//                throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));

//            _rabbitConnection.DeclareQueue(queueName, withDeadLetter: true);
//            _rabbitConnection.DeclareQueue($"{queueName}.dlq");

//            var consumer = new AsyncEventingBasicConsumer(_rabbitConnection.Channel);
//            consumer.Received += async (model, ea) =>
//            {
//                try
//                {
//                    var body = ea.Body.ToArray();
//                    var message = Encoding.UTF8.GetString(body);

//                    _logger.LogInformation("📥 Received message on queue '{QueueName}': {Message}", queueName, message);

//                    var @event = JsonSerializer.Deserialize<T>(message);

//                    if (@event != null)
//                    {
//                        await handleEvent(@event);

//                        // Acknowledge message after successful processing
//                        _rabbitConnection.Channel.BasicAck(ea.DeliveryTag, multiple: false);
//                        _logger.LogInformation("✅ Message processed successfully from queue '{QueueName}'.", queueName);
//                    }
//                    else
//                    {
//                        throw new Exception("Deserialization returned null");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "❌ Error processing message from queue '{QueueName}'. Moving to DLQ.", queueName);

//                    // Move message to Dead-Letter Queue (DLQ)
//                    _rabbitConnection.Channel.BasicReject(ea.DeliveryTag, requeue: false);
//                }
//            };

//            _rabbitConnection.Channel.BasicConsume(
//                queue: queueName,
//                autoAck: false, // Ensure manual acknowledgment
//                consumer: consumer
//            );

//            _logger.LogInformation("🎧 Listening on queue '{QueueName}'", queueName);
//        }

//        public void Dispose()
//        {
//            _rabbitConnection?.Dispose();
//        }
//    }
//}
