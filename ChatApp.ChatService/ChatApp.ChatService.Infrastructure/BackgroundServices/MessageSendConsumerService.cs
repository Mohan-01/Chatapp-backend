using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ChatApp.ChatService.Core.Interfaces;

namespace ChatApp.ChatService.Infrastructure.BackgroundServices
{
    public class MessageSendConsumerService: BackgroundService
    {
        private readonly IServiceProvider _serviceProvider; // ✅ Use IServiceProvider

        public MessageSendConsumerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("MessageSendConsumerService is running.");
            using (var scope = _serviceProvider.CreateScope())
            {
                var consumer = scope.ServiceProvider.GetRequiredService<IConsumer>();
                consumer.StartConsuming();
                await Task.CompletedTask;
            }
        }
    }
}
