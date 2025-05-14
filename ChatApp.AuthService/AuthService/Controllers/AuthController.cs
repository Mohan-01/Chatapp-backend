using System.Security.Claims;
using AuthService.Core.Constants;
using AuthService.Core.DTOs;
using AuthService.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums.User;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _env;

        public AuthController(IAuthService authService, IWebHostEnvironment env)
        {
            _authService = authService;
            _env = env;
        }

        [AllowAnonymous]
        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var roles = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Select(role => new
                {
                    Role = role.ToString(),
                    Description = RoleDescriptions.GetDescription(role)
                });

            return Ok(roles);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto) {
            if (dto == null)
                return BadRequest("Invalid registration data");
            if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                return BadRequest("Username, email, and password are required");
            try
            {

                AuthServiceResponseDto<AuthResponseDto> authResponse = await _authService.RegisterAsync(dto);
                if (authResponse == null || authResponse.Data == null || authResponse.Data.Token == null)
                    return BadRequest("Registration failed");

                // Set the authentication cookies
                SetAuthCookies(authResponse.Data.Token);

                authResponse.Data.Token = null;
                
                return Ok(authResponse);
            } catch(Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto) {

            AuthServiceResponseDto<AuthResponseDto> authResponse = await _authService.LoginAsync(dto);

            if (authResponse == null || authResponse.Data == null || authResponse.Data.Token == null)
                return Unauthorized("Invalid credentials");

            // Set the authentication cookies
            SetAuthCookies(authResponse.Data.Token);

            // to make null of authrespose.data.token
            authResponse.Data.Token = null;

            return Ok(authResponse);
        }

        [HttpGet("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            if(string.IsNullOrEmpty(User.Identity?.Name))
            {
                return Unauthorized();
            }

            ClearAuthCookies();
            
            return Ok(new AuthServiceResponseDto<string>{ Message = "User Logged out successfully", Success = true});
        }

        [AllowAnonymous]
        [HttpPost("forgot-username")]
        public async Task<IActionResult> ForgotUsername(ForgotUsernameRequestDto dto) {
            AuthServiceResponseDto<string> response = await _authService.ForgotUsernameAsync(dto);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto) {
            if (dto == null)
                return BadRequest("Invalid request data");

            if (string.IsNullOrEmpty(dto.Email))
                return BadRequest("Email is required");

            AuthServiceResponseDto<string> response = await _authService.ForgotPasswordAsync(dto);

            if (string.IsNullOrEmpty(response.Data))
                return BadRequest("Failed to send password reset email");
            if (response.Data == "User not found")
                return NotFound("User not found");

            ClearAuthCookies();

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto) {
            if (dto == null)
                return BadRequest("Invalid request data");

            if (string.IsNullOrEmpty(dto.NewPassword) || string.IsNullOrEmpty(dto.ResetToken))
                return BadRequest("New password and reset token are required");

            AuthServiceResponseDto<string> response = await _authService.ResetPasswordAsync(dto);
            
            if (!response.Success)
                return BadRequest(response);

            ClearAuthCookies();

            return Ok(response);
        }

        [Authorize]
        [HttpGet("authenticate-user")]
        public IActionResult AuthenticateUser() => Ok();
        
        [Authorize]
        [HttpPut("change-username")]
        public async Task<IActionResult> ChangeUsername(ChangeUsernameRequestDto dto)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(username))
                return Unauthorized("Username claim is missing");

            try
            {
                return Ok(await _authService.ChangeUsernameAsync(username, dto.NewUsername));
            }
            catch (InvalidOperationException ex)
            {
                // Example: Username is already taken
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                // General error fallback
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize]
        [HttpPut("update-email")]
        public async Task<IActionResult> ChangeEmail(UpdateEmailRequestDto dto)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(username))
                return Unauthorized("Username claim is missing");

            return Ok(await _authService.UpdateEmailAsync(username, dto.NewEmail));
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto dto)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(username))
                return Unauthorized("Username claim is missing");
            AuthServiceResponseDto<string> response = await _authService.ChangePasswordAsync(username, dto.NewPassword);

            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [Authorize]
        [HttpDelete("delete-user")]
        public async Task<IActionResult> DeleteUser()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(username))
                return Unauthorized("Username claim is missing");

            AuthServiceResponseDto<string > response = await _authService.DeleteUserAsync(username);
            
            if (!response.Success)
                return BadRequest(response);

            // Clear the authentication cookies
            ClearAuthCookies();

            return Ok(response);
        }

        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] ValidateTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Token))
                return BadRequest(new { Message = "Token is required" });

            var result = _authService.ValidateToken(request.Token);

            if (!result.Success)
                return Unauthorized(new { Message = "Invalid or expired token" });

            return Ok(result);
        }

        private void SetAuthCookies(string accessToken)
        {
            bool isProduction = _env.IsProduction();

            var accessTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("access_token", accessToken, accessTokenCookieOptions);
        }

        private void ClearAuthCookies()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = _env.IsProduction(),
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            };
            Response.Cookies.Append("access_token", "", cookieOptions);
        }
    }
}
