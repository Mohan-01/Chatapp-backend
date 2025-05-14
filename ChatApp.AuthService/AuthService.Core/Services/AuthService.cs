using System.Security.Claims;
using AuthService.Core.DTOs;
using AuthService.Core.Interfaces;
using AuthService.Core.Models;
using AuthService.Core.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Shared.Constants;
using Shared.Enums.User;
using Shared.EventContracts;

namespace AuthService.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenHandler _tokenHandler;
        private readonly ILogger<IAuthService> _logger;
        private readonly IAuthRepository _authRepository;
        private readonly IEventPublisher _publisher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public readonly IEmailNotificationService _emailNotificationService;

        public AuthService(
            IAuthRepository authRepository,
            ILogger<IAuthService> logger,
            IEventPublisher publisher,
            ITokenHandler tokenHandler,
            IEmailNotificationService emailNotificationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _authRepository = authRepository;
            _tokenHandler = tokenHandler;
            _logger = logger;
            _publisher = publisher;
            _httpContextAccessor = httpContextAccessor;
            _emailNotificationService = emailNotificationService;
        }

        public async Task<AuthServiceResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto)
        {
            _logger.LogInformation("Registering new user: {Username}", dto.Username);

            if (await _authRepository.GetByUsernameAsync(dto.Username) != null)
            {
                _logger.LogWarning("Username {Username} already exists", dto.Username);
                throw new InvalidOperationException("Username already exists");
            }

            AuthUser user = new AuthUser
            {
                Username = dto.Username,
                Email = dto.Email,
                Roles = [UserRole.Member],
                PasswordHash = PasswordHasher.Hash(dto.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _authRepository.CreateUserAsync(user);
            _publisher.Publish(QueueNames.UserRegisteredQueue, new UserRegisteredEvent
            {
                UserId = user.UserId?.ToString() ?? "",
                Username = user.Username,
                Email = user.Email,
                Roles = [user.Roles.ToString()],
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }, false);

            _logger.LogInformation("User {Username} registered successfully", user.Username);

            var emailSent = await _emailNotificationService.SendRegistrationConfirmationEmailAsync(user.Email, user.Username);
            if (emailSent)
                _logger.LogInformation("Welcome email sent successfully to {Email}", user.Email);
            else
                _logger.LogError("Failed to send welcome email to {Email}", user.Email);

            var responseData = new AuthResponseDto
            {
                // Token = _tokenHandler.GenerateLoginToken(user.Username, user.Roles),
                Token = _tokenHandler.GenerateJwtToken(user),
                Username = user.Username,
                Email = user.Email,
                Roles = [.. user.Roles.Select(role => role.ToString())]
            };

            return new AuthServiceResponseDto<AuthResponseDto> { Data = responseData, Message = "User Registered Successfully", Success = true };
        }
        public async Task<AuthServiceResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            _logger.LogInformation("User {Username} attempting login", dto.Username);
            _logger.LogInformation("With password {password}", dto.Password);

            AuthUser? user = await _authRepository.GetByUsernameAsync(dto.Username);
            if (user == null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid login attempt for {Username}", dto.Username);
                throw new AuthException("Invalid username or password");
            }

            var responseData = new AuthResponseDto
            {
                Token = _tokenHandler.GenerateJwtToken(user),
                Username = user.Username,
                Email = user.Email,
                Roles = [.. user.Roles.Select(role => role.ToString())]
            };

            return new AuthServiceResponseDto<AuthResponseDto> { Data = responseData, Message = "Logged in successfully", Success = true };
        }
        public async Task<AuthServiceResponseDto<string>> ForgotUsernameAsync(ForgotUsernameRequestDto dto)
        {
            var user = await _authRepository.GetByEmailAsync(dto.Email);
            if (user == null) return new AuthServiceResponseDto<string> { Message = "Email not found", Success = false };

            return await _emailNotificationService.SendForgotUsernameEmailAsync(user.Email, user.Username)
                ? new AuthServiceResponseDto<string> { Message = "Username sent to your mail.", Success = true }
                : new AuthServiceResponseDto<string> { Message = "Failed to send email.", Success = false };
        }
        public async Task<AuthServiceResponseDto<string>> ForgotPasswordAsync(ForgotPasswordRequestDto dto)
        {
            AuthUser? user = await _authRepository.GetByEmailAsync(dto.Email);
            if (user == null) return new AuthServiceResponseDto<string> { Message = "User not found", Success = false };

            var resetToken = _tokenHandler.GenerateResetToken(user);
            var request = _httpContextAccessor.HttpContext?.Request;
            var origin = request != null ? $"{request.Scheme}://{request.Host.Value}": "http://localhost:4200";
            var resetLink = $"{origin}/reset-password?reset-token={resetToken}";

            return await _emailNotificationService.SendPasswordResetEmailAsync(dto.Email, user.Username, resetLink)
                ? new AuthServiceResponseDto<string> { Data = resetToken, Message = "Check mail for the password reset link", Success = true }
                : new AuthServiceResponseDto<string> { Message = "Failed to send email", Success = false };
        }
        public async Task<AuthServiceResponseDto<string>> ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            var claimsPrincipal = _tokenHandler.ValidateToken(dto.ResetToken);
            if (claimsPrincipal == null)
            {
                throw new AuthException("Invalid or expired token");
            }

            var username = claimsPrincipal.FindFirst("username")?.Value;
            if (string.IsNullOrEmpty(username))
            {
                throw new AuthException("Invalid token: Username missing");
            }

            var user = await _authRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                throw new AuthException("User not found");
            }
            var email = user.Email;

            bool isUpdated = await _authRepository.UpdatePasswordAsync(username, PasswordHasher.Hash(dto.NewPassword));

            if (!isUpdated)
            {
                return new AuthServiceResponseDto<string> { Message = "Username not found", Success = false};
            }

            _logger.LogInformation("User {Username} has successfully reset their password", username);

            // ✅ Send password change confirmation email
            var emailSent = await _emailNotificationService.SendPasswordChangedEmailAsync(email, username);

            if (emailSent)
                _logger.LogInformation("Password change confirmation email sent to {Email}", email);
            else
                _logger.LogError("Failed to send password change confirmation email to {Email}", email);

            return new AuthServiceResponseDto<string> { Message = "Password reset successfully", Success = true };
        }
        public async Task<AuthServiceResponseDto<string>> ChangeUsernameAsync(string username, string newUsername)
        {
            _logger.LogInformation("Change username requested: {CurrentUsername} -> {NewUsername}", username, newUsername);
            if (await _authRepository.GetByUsernameAsync(newUsername) != null)
            {
                _logger.LogWarning("Username {NewUsername} is already taken", newUsername);
                throw new InvalidOperationException($"Username {newUsername} is already taken");
            }

            if (await _authRepository.UpdateUsernameAsync(username, newUsername))
            {
                UsernameChangedEvent usernameChangedEvent = new()
                {
                    CurrentUsername = username,
                    NewUsername = newUsername
                };

                _publisher.Publish(QueueNames.UsernameChangedQueue, usernameChangedEvent, false);
                _logger.LogInformation("Username changed successfully: {CurrentUsername} -> {NewUsername}", username, newUsername);

                // ✅ Send username change confirmation email
                AuthUser? user = await _authRepository.GetByUsernameAsync(newUsername);
                if (user != null) {
                    _logger.LogInformation("User found: {Username}", user.Username);
                }
                else
                {
                    _logger.LogWarning("User not found for username: {Username}", newUsername);
                    throw new InvalidOperationException($"User not found for username: {newUsername}");
                }

                bool emailSent = await _emailNotificationService.SendUsernameChangedEmailAsync(user.Email, newUsername);

                if (emailSent)
                    _logger.LogInformation("Username change confirmation email sent to {Email}", user.Email);
                else
                    _logger.LogError("Failed to send username change confirmation email to {Email}", user.Email);


                return new AuthServiceResponseDto<string> { Data = user.Username, Message = "Username changed successfully", Success = true };
            }

            _logger.LogWarning("Failed to change username: {CurrentUsername}", username);
            throw new InvalidOperationException($"Username change failed. Try again later");
        }
        public async Task<AuthServiceResponseDto<string>> UpdateEmailAsync(string username, string newEmail)
        {
            _logger.LogInformation("Update email requested: {Username} -> {NewEmail}", username, newEmail);

            if (await _authRepository.GetByEmailAsync(newEmail) != null)
            {
                _logger.LogWarning("Email {NewEmail} is already taken", newEmail);
                return new AuthServiceResponseDto<string> { Message="Email already in use", Success = false};
            }

            EmailChangedEvent emailChangedEvent = new()
            {
                Username = username,
                NewEmail = newEmail
            };

            string? email = _authRepository.GetByUsernameAsync(username).Result?.Email;

            if (await _authRepository.UpdateEmailAsync(emailChangedEvent))
            {
                _publisher.Publish(QueueNames.EmailChangedQueue, emailChangedEvent, false);

                if(email == null)
                {
                    _logger.LogWarning("Email not found for username: {Username}", username);
                    return new AuthServiceResponseDto<string> { Message = "Email not found", Success = false };
                }

                bool emailSent = await _emailNotificationService.SendEmailChangeConfirmationAsync(
                    email,
                    "Email Address Changed"
                );

                if (!emailSent)
                {
                    _logger.LogError("Failed to send email change confirmation to {NewEmail}", newEmail);
                    return new AuthServiceResponseDto<string> { Data = newEmail, Message = "Email updated but failed to send confirmation", Success = true };
                }

                _logger.LogInformation("Email updated successfully: {Username} -> {NewEmail}", username, newEmail);

                return new AuthServiceResponseDto<string> { Data = newEmail, Message = "Email updated successfully", Success = true };
            }

            _logger.LogWarning("Failed to update email for username: {Username}", username);
            return new AuthServiceResponseDto<string> { Message = "Email update failed", Success = false};
        }
        public async Task<AuthServiceResponseDto<string>> ChangePasswordAsync(string username, string newPassword)
        {
            _logger.LogInformation("Password change requested for username: {Username}", username);

            AuthUser? user = await _authRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User not found for username: {Username}", username);
                return new AuthServiceResponseDto<string> { Message = "User not found", Success = false };
            }

            string newPasswordHash = PasswordHasher.Hash(newPassword);
            bool updated = await _authRepository.UpdatePasswordAsync(username, newPasswordHash);
            if (updated)
            {
                bool emailSent = await _emailNotificationService.SendPasswordChangedEmailAsync(
                    user.Email,
                    "Password Changed"
                );

                if (!emailSent)
                {
                    _logger.LogError("Failed to send password change confirmation to {Email}", user.Email);
                    return new AuthServiceResponseDto<string>
                    {
                        Data = username,
                        Message = "Password updated but failed to send confirmation",
                        Success = true
                    };
                }

                _logger.LogInformation("Password updated successfully for username: {Username}", username);
                return new AuthServiceResponseDto<string>
                {
                    Data = username,
                    Message = "Password updated successfully",
                    Success = true
                };
            }

            _logger.LogWarning("Failed to update password for username: {Username}", username);
            return new AuthServiceResponseDto<string> { Message = "Password update failed", Success = false };
        }
        public async Task<AuthServiceResponseDto<string>> DeleteUserAsync(string username)
        {
            _logger.LogInformation("Delete user requested for username: {Username}", username);

            AuthUser? user = await _authRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("Username {Username} not found during delete request", username);
                return new AuthServiceResponseDto<string> { Message = "Username not found", Success = false};
            }

            UserDeletedEvent userDeletedEvent = new()
            {
                Username = user.Username
            };

            bool isUpdated = await _authRepository.UpdateIsActiveStatusAsync(userDeletedEvent);
            if (isUpdated)
            {
                _publisher.Publish(QueueNames.UserDeletedQueue, userDeletedEvent, false);
                _logger.LogInformation("User {Username} marked as inactive successfully", userDeletedEvent.Username);

                // ✅ Send account deactivation email
                bool emailSent = await _emailNotificationService.SendAccountDeactivationEmailAsync(user.Email);
                if (emailSent)
                    _logger.LogInformation("Account deactivation email sent to {Email}", user.Email);
                else
                    _logger.LogError("Failed to send account deactivation email to {Email}", user.Email);

                return new AuthServiceResponseDto<string> { Message = "User deleted successfully", Success = true };
            }

            _logger.LogWarning("Failed to delete user: {Username}", userDeletedEvent.Username);
            return new AuthServiceResponseDto<string> { Message = "User deletion failed", Success = false};
        }

        public AuthServiceResponseDto<ValidateTokenResponseDto> ValidateToken(string token)
        {
            _logger.LogInformation("Attempting to validate token");

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token validation failed: token was empty or null");
                return new AuthServiceResponseDto<ValidateTokenResponseDto>
                {
                    Success = false,
                    Message = "Token is required"
                };
            }

            try
            {
                ClaimsPrincipal? principal = _tokenHandler.ValidateToken(token);
                if (principal == null)
                {
                    _logger.LogWarning("Token validation failed: invalid token structure");
                    return new AuthServiceResponseDto<ValidateTokenResponseDto>
                    {
                        Success = false,
                        Message = "Invalid token"
                    };
                }

                string? username = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                string? email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                List<string>? roles = [.. principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)];

                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("Token validation failed: username not found in token");
                    return new AuthServiceResponseDto<ValidateTokenResponseDto>
                    {
                        Success = false,
                        Message = "Invalid token payload"
                    };
                }

                _logger.LogInformation("Token validation successful for user {Username}", username);

                if(email == null)
                {
                    _logger.LogWarning("Token validation failed: email not found in token");
                    return new AuthServiceResponseDto<ValidateTokenResponseDto>
                    {
                        Success = false,
                        Message = "Invalid token payload"
                    };
                }

                ValidateTokenResponseDto responseData = new()
                {
                    Username = username,
                    Email = email,
                    Roles = roles
                };

                return new AuthServiceResponseDto<ValidateTokenResponseDto>
                {
                    Data = responseData,
                    Message = "Token is valid",
                    Success = true
                };
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "Token validation failed: token expired");
                return new AuthServiceResponseDto<ValidateTokenResponseDto>
                {
                    Success = false,
                    Message = "Token has expired"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed: unexpected error");
                return new AuthServiceResponseDto<ValidateTokenResponseDto>
                {
                    Success = false,
                    Message = "An error occurred during token validation"
                };
            }
        }

    }
}
