using AuthService.Core.DTOs;

namespace AuthService.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthServiceResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto);
        Task<AuthServiceResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto dto);
        Task<AuthServiceResponseDto<string>> ForgotUsernameAsync(ForgotUsernameRequestDto dto);
        Task<AuthServiceResponseDto<string>> ForgotPasswordAsync(ForgotPasswordRequestDto dto);
        Task<AuthServiceResponseDto<string>> ResetPasswordAsync(ResetPasswordRequestDto dto);
        Task<AuthServiceResponseDto<string>> ChangeUsernameAsync(string username, string newUsername);
        Task<AuthServiceResponseDto<string>> UpdateEmailAsync(string username, string newEmail);
        Task<AuthServiceResponseDto<string>> ChangePasswordAsync(string username, string newPassword);
        Task<AuthServiceResponseDto<string>> DeleteUserAsync(string username);

        AuthServiceResponseDto<ValidateTokenResponseDto> ValidateToken(string token);
    }
}