namespace AuthService.Core.Interfaces
{
    public interface IEmailNotificationService
    {
        Task<bool> SendRegistrationConfirmationEmailAsync(string email, string username);
        Task<bool> SendForgotUsernameEmailAsync(string email, string username);
        Task<bool> SendUsernameChangedEmailAsync(string email, string username);
        Task<bool> SendPasswordResetEmailAsync(string email, string username, string resetLink);
        Task<bool> SendPasswordChangedEmailAsync(string email, string username);
        Task<bool> SendEmailChangeConfirmationAsync(string email, string username);
        Task<bool> SendAccountDeactivationEmailAsync(string email);
    }
}
