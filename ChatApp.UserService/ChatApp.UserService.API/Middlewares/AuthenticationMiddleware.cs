using ChatApp.UserService.Core.RequestDTOs;
using ChatApp.UserService.Core.ResponseDTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ChatApp.UserService.API.Middlewares.NotUsingCurrently
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationMiddleware> _logger;
        private readonly HttpClient _httpClient;

        public AuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
            _httpClient = new HttpClient(); // Later replace with IHttpClientFactory
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                Endpoint? endpoint = context.GetEndpoint();
                bool allowAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;
                _logger.LogInformation("AllowAnonymous detected: {AllowAnonymous}", allowAnonymous);

                if (allowAnonymous)
                {
                    await _next(context);
                    return;
                }

                string? accessToken = context.Request.Cookies["access_token"];

                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogWarning("Access token missing from cookies for path {Path}", context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: Token missing.");
                    return;
                }

                if (IsSwaggerRequest(context.Request.Path))
                {
                    _logger.LogInformation("Swagger request detected, skipping authentication.");
                    await _next(context);
                    return;
                }

                if (!IsApiRequest(context.Request.Path))
                {
                    _logger.LogInformation("Non-API request detected, skipping authentication.");
                    await _next(context);
                    return;
                }

                bool isValid = await ValidateTokenWithAuthServiceAsync(context, accessToken);

                if (!isValid)
                {
                    _logger.LogWarning("Access token validation failed via AuthService for path {Path}", context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: Invalid or expired token.");
                    return;
                }

                _logger.LogInformation("Access token validated successfully via AuthService for path {Path}", context.Request.Path);

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during authentication middleware for path {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Internal Server Error.");
            }
        }

        private async Task<bool> ValidateTokenWithAuthServiceAsync(HttpContext context, string token)
        {
            try
            {
                string? validateEndpoint = _configuration["AuthService:ValidateTokenUrl"];
                if (string.IsNullOrEmpty(validateEndpoint))
                {
                    validateEndpoint = "http://localhost:5001/api/Auth/validate-token"; // fallback
                }

                ValidateTokenRequest requestBody = new()
                {
                    Token = token
                };

                string json = JsonSerializer.Serialize(requestBody);
                StringContent content = new(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Calling AuthService to validate token...");

                HttpResponseMessage response = await _httpClient.PostAsync(validateEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("AuthService token validation success (HTTP {StatusCode})", response.StatusCode);

                    string responseContent = await response.Content.ReadAsStringAsync();
                    AuthServiceResponseDto<AuthResponseDto>? authResponse = JsonSerializer.Deserialize<AuthServiceResponseDto<AuthResponseDto>>(responseContent, JsonSerializerDefaults.CamelCase);

                    if (authResponse == null)
                    {
                        _logger.LogWarning("AuthService response is null or invalid.");
                        return false;
                    }

                    if (authResponse.Data != null)
                    {
                        List<Claim> claims = [
                            new(ClaimTypes.Name, authResponse.Data.Username),
                            new("username", authResponse.Data.Username),
                            new(ClaimTypes.Email, authResponse.Data.Email)
                        ];

                        foreach (string role in authResponse.Data.Roles)
                        {
                            claims.Add(new(ClaimTypes.Role, role));
                        }

                        ClaimsIdentity identity = new(claims, "AuthService");
                        ClaimsPrincipal principal = new(identity);

                        context.User = principal; // ✅ Set the user into HttpContext

                        return true;
                    }

                    _logger.LogWarning("AuthService responded without user data.");
                    return false;
                }
                else
                {
                    _logger.LogWarning("AuthService token validation failed (HTTP {StatusCode})", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calling AuthService for token validation.");
                return false;
            }
        }

        private static bool IsSwaggerRequest(string path)
        {
            return path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsApiRequest(string path)
        {
            return path.StartsWith("/api", StringComparison.OrdinalIgnoreCase);
        }
    }

    public static class JsonSerializerDefaults
    {
        public static readonly JsonSerializerOptions CamelCase = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
}
