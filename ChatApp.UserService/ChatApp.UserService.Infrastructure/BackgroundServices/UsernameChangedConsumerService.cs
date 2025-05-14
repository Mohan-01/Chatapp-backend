using Microsoft.Extensions.DependencyInjection;
using ChatApp.UserService.Core.Interfaces;
using Microsoft.Extensions.Hosting;

namespace ChatApp.UserService.Infrastructure.BackgroundServices
{
    public class UsernameChangedConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider; // ✅ Use IServiceProvider

        public UsernameChangedConsumerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("UsernameChangedConsumerService is running.");
            using (var scope = _serviceProvider.CreateScope())
            {
                var _consumer = scope.ServiceProvider.GetRequiredService<IUsernameChangedConsumer>();
                _consumer.StartConsuming();
            }
            return Task.CompletedTask;
        }
    }
}
