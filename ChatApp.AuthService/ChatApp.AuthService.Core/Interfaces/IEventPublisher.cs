namespace ChatApp.AuthService.Core.Interfaces.NotUsing
{
    public interface IEventPublisher
    {
        void Publish<T>(string exchangeName, string routingKey, T @event);
    }
}
