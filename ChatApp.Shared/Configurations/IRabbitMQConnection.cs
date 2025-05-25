using RabbitMQ.Client;

namespace Shared.Configurations
{
    public interface IRabbitMQConnection : IDisposable
    {
        IConnection GetConnection();
        IConnection Reconnect();
        void DeclareQueue(string queueName, string exchangeName, string routingKey, IModel channel, bool withDeadLetter = false);
        //void DeclareQueue(string queueName, bool withDeadLetter = false);
    }
}
