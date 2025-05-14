using AuthService.Core.Utils;

namespace AuthService.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AuthException ex)
            {
                _logger.LogWarning("Authentication error: {Message}", ex.Message);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Authentication error: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unhandled error: {Message}", ex.Message);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal Server Error");
            }
        }
    }

}
