using System.Text.Json.Serialization;

namespace ChatApp.ChatService.Core.RequestResponseModels.Chat
{
    public class ServiceResponse<T>
    {
        required public bool Success { get; set; }
        required public string Message { get; set; } = null!;
        public T? Data { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Token { get; set; } 
    }

}