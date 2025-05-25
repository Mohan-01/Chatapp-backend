namespace Shared.Producers
{
    public interface IEventPublisher
    {
        void Publish<T>(string exchangeName, string routingKey, T @event);
    }
}
