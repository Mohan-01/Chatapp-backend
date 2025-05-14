using AuthService.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace AuthService.API.Middlewares.CurrentlyNotUsing
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>();

            if (IsSwaggerRequest(context.Request.Path))
            {
                await _next(context);
                return;
            }

            if (allowAnonymous != null)
            {
                await _next(context);
                return;
            }

            var accessToken = context.Request.Cookies["access_token"];

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Access token is missing");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Access token is missing");
                return;
            }

            try
            {
                var tokenHandler = context.RequestServices.GetRequiredService<ITokenHandler>();
                var claimsPrincipal = tokenHandler.ValidateToken(accessToken);

                if (claimsPrincipal == null)
                {
                    _logger.LogWarning("Invalid access token");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid access token");
                    return;
                }

                // Extract username and token version
                var username = claimsPrincipal.Identity?.Name;
                var tokenVersionClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "tokenVersion")?.Value;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(tokenVersionClaim))
                {
                    _logger.LogWarning("Token is missing username or token version");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Token is missing required claims");
                    return;
                }

                var authRepository = context.RequestServices.GetRequiredService<IAuthRepository>();
                int? tokenVersionInDb = await authRepository.GetTokenVersionWithUsername(username);

                if (tokenVersionInDb == null)
                {
                    _logger.LogWarning("User not found for username: {Username}", username);
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("User not found");
                    return;
                }

                if (tokenVersionInDb != int.Parse(tokenVersionClaim))
                {
                    _logger.LogWarning("Token version mismatch for user: {Username}", username);
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Token version mismatch");
                    return;
                }

                Log.Information("User {Username} has token version {TokenVersion}", username, tokenVersionClaim);

                context.User = claimsPrincipal;
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error validating access token: {Message}", ex.Message);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Error validating access token");
            }
        }

        private static bool IsSwaggerRequest(PathString path)
        {
            return path.StartsWithSegments("/swagger") || path.StartsWithSegments("/swagger/index.html");
        }
    }
}
