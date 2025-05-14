using Microsoft.Extensions.DependencyInjection;
using ChatApp.UserService.Core.Interfaces;
using Microsoft.Extensions.Hosting;

namespace ChatApp.UserService.Infrastructure.BackgroundServices
{
    public class EmailChangedConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider; // ✅ Use IServiceProvider

        public EmailChangedConsumerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("EmailChangedConsumerService is running.");
            using (var scope = _serviceProvider.CreateScope())
            {
                var consumer = scope.ServiceProvider.GetRequiredService<IEmailChangedConsumer>();
                consumer.StartConsuming();
                await Task.CompletedTask;
            }
        }
    }
}
