using Microsoft.Extensions.DependencyInjection;
using ChatApp.UserService.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using ChatApp.UserService.Infrastructure.Consumers;

namespace ChatApp.UserService.Infrastructure.BackgroundServices
{
    public class UserDeletedConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider; // ✅ Use IServiceProvider

        public UserDeletedConsumerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var consumer = scope.ServiceProvider.GetRequiredService<UserDeletedConsumer>();
                consumer.StartConsuming();
            }

            return Task.CompletedTask;
        }
    }
}
