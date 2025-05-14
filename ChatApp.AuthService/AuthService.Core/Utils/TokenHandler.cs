using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Core.Interfaces;
using AuthService.Core.Models;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace AuthService.Core.Utils
{
    public class TokenHandler: ITokenHandler
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _tokenExpirationDays;

        public TokenHandler(string secretKey, string issuer, string audience, int tokenExpirationDays)
        {
            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
            _tokenExpirationDays = tokenExpirationDays;
        }

        public string GenerateResetToken(AuthUser user)
        {
            return GenerateJwtToken(user); // Short expiration
        }
        
        public string GenerateJwtToken(AuthUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.Name, user.Username),
                new("username", user.Username),
                new("email", user.Email),
                new("tokenVersion", user.TokenVersion.ToString()) // Add TokenVersion claim

            };

            var roles = user.Roles;

            if (roles != null && roles.Count > 0)
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.ToString())));

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_tokenExpirationDays),
                signingCredentials: credentials
            );

            //return EncryptToken(new JwtSecurityTokenHandler().WriteToken(token));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    // Validate the signing key
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),

                    // Validate the issuer
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,

                    // Validate the audience
                    ValidateAudience = true,
                    ValidAudience = _audience,

                    // Validate the token lifetime (expiration)
                    ValidateLifetime = true,

                    // Allow a small clock skew to account for server time variations
                    ClockSkew = TimeSpan.Zero
                };

                // Validate and decode the token to get the ClaimsPrincipal
                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch (SecurityTokenExpiredException ex)
            {
                // Handle the expiration case (Optional: Log or handle expired tokens)
                Log.Error("Token expired: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                // Catch all other exceptions (log, rethrow, or handle as needed)
                Log.Error("Token validation failed: {Message}", ex.Message);
                return null;
            }
        }

        public ClaimsPrincipal? ValidateTokenWithVersionCheck(string token, int currentTokenVersion)
        {
            try
            {
                // First, validate the token using the general validation method
                var principal = ValidateToken(token);

                if (principal != null)
                {
                    // Extract TokenVersion from token claims
                    var tokenVersionClaim = principal.Claims.FirstOrDefault(c => c.Type == "tokenVersion")?.Value;

                    if (tokenVersionClaim == null || !int.TryParse(tokenVersionClaim, out var tokenVersionFromToken))
                    {
                        Log.Error("Token version is missing or invalid for user: {Username}", principal.Identity?.Name);
                        return null;
                    }

                    // Check if the token version matches the current version in the database
                    if (tokenVersionFromToken != currentTokenVersion)
                    {
                        // Token version mismatch means the token is no longer valid
                        Log.Error("Token version mismatch for user: {Username}", principal.Identity?.Name);
                        return null;
                    }

                    return principal; // Token is valid and version matches
                }

                return null;
            }
            catch (Exception ex)
            {
                // Catch all other exceptions (log, rethrow, or handle as needed)
                Log.Error("Token validation with version check failed: {Message}", ex.Message);
                return null;
            }
        }
    }
}
