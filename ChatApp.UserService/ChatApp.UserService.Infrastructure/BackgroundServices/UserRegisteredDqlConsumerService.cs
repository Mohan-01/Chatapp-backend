using ChatApp.UserService.Core.Interfaces;
using ChatApp.UserService.Infrastructure.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.UserService.Infrastructure.BackgroundServices
{
    public class UserRegisteredDqlConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider; // ✅ Use IServiceProvider

        public UserRegisteredDqlConsumerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("UserRegisteredDqlConsumerService is running.");
            using (var scope = _serviceProvider.CreateScope())
            {
                var consumer = scope.ServiceProvider.GetRequiredService<UserRegisteredDlqConsumer>();
                consumer.StartConsuming();
                await Task.CompletedTask;
            }
        }
    }
}
