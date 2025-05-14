using System.Security.Claims;
using AuthService.Core.Models;

namespace AuthService.Core.Interfaces
{
    public interface ITokenHandler
    {
        //string GenerateJwtToken(string username);
        string GenerateJwtToken(AuthUser user);
        string GenerateResetToken(AuthUser user);
        ClaimsPrincipal? ValidateToken(string token);
        ClaimsPrincipal? ValidateTokenWithVersionCheck(string token, int currentTokenVersion);
    }
}
