using ChatApp.ChatService.Core.DTOs.Chat;
using ChatApp.ChatService.Core.Enums.Chat;
using ChatApp.ChatService.Core.Interfaces;
using ChatApp.ChatService.Core.RequestResponseModels.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace ChatApp.ChatService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("check")]
        public IActionResult Get()
        {
            return Ok("Chat Controller");
        }

        // POST: api/Chat/create/{withUsername}
        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.WithUsername))
            {
                return BadRequest("Invalid input.");
            }

            if (User.Identity?.Name == null)
            {
                return Unauthorized();
            }

            var username1 = User.Identity.Name;
            var username2 = request.WithUsername;

            ServiceResponse<PrivateChatDto> response = await _chatService.CreateOneToOneChatAsync(username1, username2);

            return Ok(response);
        }

        // GET: api/chat/{chatId}
        [HttpGet("id/{chatId}")]
        public async Task<IActionResult> GetChatById(string chatId)
        {
            // Validate the chatId format
            if (!ObjectId.TryParse(chatId, out var objectId))
            {
                return BadRequest("Invalid chatId format.");
            } else
            {
                Console.WriteLine("\nChat Id is valid\n");
            }

            // Call the service to get the chat by its ObjectId
            ServiceResponse<PrivateChatDto> response = await _chatService.GetChatByIdAsync(objectId.ToString());

            // If chat is not found, return NotFound
            if (!response.Success)
            {
                return NotFound("Chat not found.");
            }

            // Return the chat if found
            // It returns PrivateChatDto
            return Ok(response);
        }

        // GET: api/Chat/{username1}/{username2}
        [HttpGet("{username2}")]
        public async Task<IActionResult> GetChatByUsernames(string username2)
        {
            if (User.Identity?.Name == null)
            {
                return Unauthorized();
            }

            var username1 = User.Identity.Name;

            if (string.IsNullOrEmpty(username1) || string.IsNullOrEmpty(username2))
            {
                return BadRequest("Invalid Username.");
            }

            ServiceResponse<PrivateChatDto> response = await _chatService.GetChatByUsernamesAsync(username1, username2);

            if (!response.Success)
            {
                return NotFound("Chat not found");
            }

            return Ok(response);
        }

        // GET: api/Chat/
        [HttpGet]
        public async Task<IActionResult> GetChatsByUsername()
        {
            if (User.Identity?.Name == null)
            {
                return Unauthorized();
            }

            var username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Invalid Username");
            }

            ServiceResponse<List<PrivateChatDto>> response = await _chatService.GetChatsByUsernameAsync(username, ChatStatus.Active);

            if (!response.Success)
            {
                return NotFound("Chats not found");
            }

            return Ok(response);
        }
        
        // PUT: api/Chat/{chatId}/update-status
        [HttpPut("{chatId}/update-status")]
        public async Task<IActionResult> UpdateChatStatus(string chatId, [FromBody] UpdateChatStatusRequest request)
        {
            if (string.IsNullOrEmpty(chatId) || string.IsNullOrEmpty(request.ChatStatus))
            {
                return BadRequest("Invalid input.");
            }

            // Try to parse the ChatType enum from the request
            if (!Enum.TryParse<ChatStatus>(request.ChatStatus, true, out var chatStatus))
            {
                return BadRequest("Invalid ChatType chatStatus.");
            }

            // Update the chat status
            await _chatService.UpdateChatStatusAsync(chatId, chatStatus);

            return NoContent();  // Return 204 No Content on successful update
        }
        /*
        // currently no need i think
        // PUT: api/Chat/{chatId}/update-messages
        [HttpPut("{chatId}/update-messages")]
        public async Task<IActionResult> UpdateChatMessages(string chatId, [FromBody] Message messages)
        {
            if (string.IsNullOrEmpty(chatId) || messages == null)
            {
                return BadRequest("Invalid input or empty message list.");
            }

            // Update the chat messages
            await _chatService.UpdateChatMessagesAsync(ObjectId.Parse(chatId), messages);

            return NoContent();  // Return 204 No Content on successful update
        }
        

        // PUT: api/Chat/{chatId}/archive
        [HttpPut("{chatId}/archive")]
        public async Task<IActionResult> ArchiveChat(string chatId)
        {
            if (string.IsNullOrEmpty(chatId))
            {
                return BadRequest("Invalid chat ID.");
            }

            await _chatService.ArchiveChatAsync(ObjectId.Parse(chatId));

            return NoContent();
        }

        */
    }
}
