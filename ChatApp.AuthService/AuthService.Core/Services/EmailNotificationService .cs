using AuthService.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Core.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<IEmailNotificationService> _logger;

        public EmailNotificationService(IEmailService emailService, ILogger<IEmailNotificationService> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> SendRegistrationConfirmationEmailAsync(string email, string username)
        {
            var subject = "Welcome to Our Service!";
            var body = $"<p>Hello {username},</p><p>Welcome to our service! We're excited to have you on board.</p>";
            return await SendEmail(email, subject, body);
        }

        public async Task<bool> SendUsernameChangedEmailAsync(string email, string username)
        {
            var subject = "Your Username Has Been Changed";
            var body = $"<p>Hello,</p><p>Your username has been changed to: <strong>{username}</strong></p>";
            return await SendEmail(email, subject, body);
        }

        public async Task<bool> SendForgotUsernameEmailAsync(string email, string username)
        {
            var subject = "Your Username Reminder";
            var body = $"<p>Hello,</p><p>Your username is: <strong>{username}</strong></p>";
            return await SendEmail(email, subject, body);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string username, string resetLink)
        {
            var subject = "Password Reset Request";
            var body = $"<p>Hello {username},</p><p>Click <a href='{resetLink}'>here</a> to reset your password.</p>";
            return await SendEmail(email, subject, body);
        }

        public async Task<bool> SendPasswordChangedEmailAsync(string email, string username)
        {
            var subject = "Your Password Has Been Changed";
            var body = $"<p>Hello {username},</p><p>Your password has been successfully changed.</p>";
            return await SendEmail(email, subject, body);
        }

        public async Task<bool> SendEmailChangeConfirmationAsync(string email, string username)
        {
            var subject = "Email Address Changed";
            var body = $"<p>Hello {username},</p><p>Your email has been successfully updated to {email}.</p>";
            return await SendEmail(email, subject, body);
        }

        public async Task<bool> SendAccountDeactivationEmailAsync(string email)
        {
            var subject = "Your Account Has Been Deactivated";
            var body = $"<p>Hello,</p><p>Your account has been deactivated. If you believe this is an error, please contact support.</p>";
            return await SendEmail(email, subject, body);
        }

        private async Task<bool> SendEmail(string email, string subject, string body)
        {
            var sent = await _emailService.SendEmailAsync(email, subject, body);
            if (!sent)
            {
                _logger.LogError("Failed to send email to {Email}", email);
                return false;
            }
            _logger.LogInformation("Email sent successfully to {Email}", email);
            return true;
        }
    }

}
