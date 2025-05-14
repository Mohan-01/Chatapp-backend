namespace AuthService.Infrastructure.Configurations
{
    class RabbitMqConfig
    {
        public string Host { get; set; } = "localhost";
        public string User { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public int Port { get; set; } = 5672;
        public bool EnableAutoRecovery { get; set; } = true;
        public int NetworkRecoveryInterval { get; set; } = 5;
    }
}
