using RabbitMQ.Client;

namespace Shared.Configurations
{
    public interface IRabbitMQConnection : IDisposable
    {
        IConnection GetConnection();
        IConnection Reconnect();
        void DeclareQueue(string queueName, IModel Channel, bool withDeadLetter = false);
        //void DeclareQueue(string queueName, bool withDeadLetter = false);
    }
}
