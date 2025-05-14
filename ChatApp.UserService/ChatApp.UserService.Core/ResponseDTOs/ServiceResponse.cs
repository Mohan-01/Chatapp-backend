namespace ChatApp.UserService.Core.ResponseDTOs
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; } // Indicates if the operation was successful
        public string Message { get; set; } = string.Empty; // Descriptive message for the operation
        public T? Data { get; set; } // Optional data payload for the response

        public string? Token { get; set; }

        // Constructor for success without data
        public ServiceResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        // Constructor for success with data
        public ServiceResponse(bool success, string message, T? data)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public ServiceResponse(bool success, string message, T? data, string token)
        {
            Success = success;
            Message = message;
            Data = data;
            Token = token;
        }

        // Default constructor
        public ServiceResponse() { }
    }
}
