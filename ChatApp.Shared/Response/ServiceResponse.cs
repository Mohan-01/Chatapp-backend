namespace Shared.Response
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; } // Indicates if the operation was successful
        public string Message { get; set; } = string.Empty; // Descriptive message for the operation
        public T? Data { get; set; } // Optional data payload for the response
        public string? Token { get; set; }
    }
}