namespace Shared.DTOs
{
    public class AuthServiceResponseDto<T>
    {
        public T? Data { get; set; }
        required public string Message { get; set; } = null!;
        required public bool Success { get; set; }
    }
}
