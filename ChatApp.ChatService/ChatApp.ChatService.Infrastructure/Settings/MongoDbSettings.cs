namespace ChatApp.ChatService.Infrastructure.Settings
{
    public class MongoDbSettings
    {
        required public string ConnectionString { get; set; } = null!;
        required public string DatabaseName { get; set; } = null!;
    }

}
