using ChatApp.UserService.Core.Interfaces;
using ChatApp.UserService.Core.ResponseDTOs;
using Microsoft.Extensions.Logging;
using Shared.EventContracts;

namespace ChatApp.UserService.Core.Services
{
    public class UserEventsService : IUserEventsService
    {

        private readonly IUserRepository _userRepository;
        private readonly ILogger<IUserEventsService> _logger;

        public UserEventsService(IUserRepository userRepository, ILogger<IUserEventsService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ServiceResponse<string>> CreateUserProfileAsync(UserRegisteredEvent profile)
        {
            try
            {
                _logger.LogInformation("Creating user profile for {Username}", profile.Username);
                try
                {
                    var result = await _userRepository.CreateUserProfileAsync(profile);
                    if (!result)
                    {
                        _logger.LogWarning("User profile creation failed for {Username}", profile.Username);
                        return new ServiceResponse<string>(false, "User profile creation failed", null);
                    }
                    _logger.LogInformation("User profile created successfully for {Username}", profile.Username);
                    return new ServiceResponse<string>(true, "User profile created successfully", null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating user profile for {Username}", profile.Username);
                    return new ServiceResponse<string>(false, $"User profile creation failed: {ex.Message}", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user profile for {Username}", profile.Username);
                throw new InvalidOperationException($"Error creating user profile for {profile.Username}: {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponse<string>> ChangeUsernameAsync(UsernameChangedEvent usernameChangedEvent)
        {
            try
            {
                _logger.LogInformation("Updating username from {CurrentUsername} to {NewUsername}", usernameChangedEvent.CurrentUsername, usernameChangedEvent.NewUsername);
                bool response = await _userRepository.ChangeUsernameAsync(usernameChangedEvent);

                if(response)
                {
                    _logger.LogInformation("Username updated from {CurrentUsername} to {NewUsername}", usernameChangedEvent.CurrentUsername, usernameChangedEvent.NewUsername);
                    return new ServiceResponse<string>(true, "Username updated successfully", usernameChangedEvent.NewUsername);
                } else
                {
                    _logger.LogWarning("Username update failed from {CurrentUsername} to {NewUsername}", usernameChangedEvent.CurrentUsername, usernameChangedEvent.NewUsername);
                    return new ServiceResponse<string>(false, "Username update failed", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating username from {CurrentUsername} to {NewUsername}", usernameChangedEvent.CurrentUsername, usernameChangedEvent.NewUsername);
                return new ServiceResponse<string>(false, $"Error updating username: {ex.Message}", null);
            }
        }

        public async Task<ServiceResponse<string>> ChangeEmailAsync(EmailChangedEvent emailChangedEvent)
        {
            try
            {
                _logger.LogInformation("Updating email for {Username} to {NewEmail}", emailChangedEvent.Username, emailChangedEvent.NewEmail);
                bool response = await _userRepository.UpdateEmailAsync(emailChangedEvent);

                if (response)
                {
                    _logger.LogInformation("Email updated for {Username}", emailChangedEvent.Username);
                    return new ServiceResponse<string>(true, "Email updated successfully", emailChangedEvent.NewEmail);
                }
                else
                {
                    _logger.LogWarning("Email update failed for {Username}", emailChangedEvent.Username);
                    return new ServiceResponse<string>(false, "Email update failed", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email for {Username}", emailChangedEvent.Username);
                return new ServiceResponse<string>(false, $"Error updating email: {ex.Message}", null);
            }
        }

        public async Task<ServiceResponse<string>> DeactivateUserAsync(UserDeletedEvent userDeletedEvent)
        {
            try
            {
                _logger.LogInformation("Deactivating user {Username}", userDeletedEvent.Username);
                bool response = await _userRepository.DeactivateUserAsync(userDeletedEvent);

                if (response)
                {
                    _logger.LogInformation("User {Username} deactivated successfully", userDeletedEvent.Username);
                    return new ServiceResponse<string>(true, "User deactivated successfully", null);
                }
                else
                {
                    _logger.LogWarning("User deactivation failed for {Username}", userDeletedEvent.Username);
                    return new ServiceResponse<string>(false, "User deactivation failed", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {Username}", userDeletedEvent.Username);
                return new ServiceResponse<string>(false, "Error deactivating user: {ex.Message}", null);
            }
        }

    }
}
