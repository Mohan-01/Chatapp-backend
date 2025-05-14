using Microsoft.Extensions.DependencyInjection;
using ChatApp.UserService.Core.Interfaces;
using Microsoft.Extensions.Hosting;

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
                var consumer = scope.ServiceProvider.GetRequiredService<IUserDeletedConsumer>();
                consumer.StartConsuming();
            }

            return Task.CompletedTask;
        }
    }
}
