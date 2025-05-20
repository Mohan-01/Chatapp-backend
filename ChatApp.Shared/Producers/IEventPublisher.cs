namespace Shared.Producers
{
    public interface IEventPublisher
    {
        void Publish<T>(string queueName, T @event, bool withDeadLetter = false);
    }
}
