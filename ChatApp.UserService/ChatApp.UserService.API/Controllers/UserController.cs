using ChatApp.UserService.Core.Interfaces;
using ChatApp.UserService.Core.RequestDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatApp.UserService.Core.ResponseDTOs;
using Shared.Models.User;

namespace ChatApp.UserService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("check")]
        public IActionResult Index()
        {
            return Ok("usercontroller");
        }

        #region Get User

        // GET: api/User/me
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserByUsername()
        {
            string? username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("Username not found in token.");
            }

            ServiceResponse<UserDto> response = await _userService.GetByUsernameAsync(username);
            
            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        // GET: api/User/email/{email}       [HttpGet("email/{email}")]
        //public async Task<IActionResult> GetUserByEmail(UserByEmailRequest dto)
        //{
        //    ServiceResponse<UserDto> response = await _userService.GetByEmailAsync(dto.email);

        //    if (!response.Success)
        //    {
        //        return NotFound("User not found.");
        //    }

        //    return Ok(response);
        //}

        #endregion

        // PUT: api/User/
        [Authorize]
        [HttpPut()]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest updateUserRequest)
        {
            if (updateUserRequest == null)
            {
                return BadRequest("User data is invalid.");
            }

            string? username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("Username not found in token.");
            }

            ServiceResponse<UserDto> response = await _userService.UpdateUserAsync(username, updateUserRequest);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] SearchUsersRequest dto)
        {
            var response = await _userService.SearchUsersAsync(dto);
            return Ok(response);
        }

        //[Authorize(Policy = "InternalOnly")]
        [AllowAnonymous]
        [HttpGet("batch")]
        public async Task<IActionResult> GetUsersBatch([FromQuery] BatchUserRequest dto)
        {
            ServiceResponse<List<UserDto>> response = await _userService.GetUsersBatchAsync(dto);

            return Ok(response);
        }
    }
}
