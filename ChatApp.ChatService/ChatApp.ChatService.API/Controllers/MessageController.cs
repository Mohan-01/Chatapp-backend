using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChatApp.ChatService.Core.Interfaces;
using ChatApp.ChatService.Core.RequestResponseModels.Message;
using ChatApp.ChatService.Core.DTOs.Message;
using ChatApp.ChatService.Core.RequestResponseModels.Chat;
using ChatApp.ChatService.Core.Mappings;

namespace ChatApp.ChatService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        #region GET
        // GET: api/Message/{messageId}
        [HttpGet("{messageId}")]
        public async Task<IActionResult> GetMessageById(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return BadRequest("Message ID cannot be null or empty.");
            }

            var message = await _messageService.GetByIdAsync(messageId);
            return message != null ? Ok(MappingToDtos.MapMessageToDto(message)) : NotFound("Message not found.");
        }

        // GET: api/Message/chat/{chatId}
        //[Authorize(Policy = "InternalOnly")]
        [HttpGet("chat/{chatId}")]
        public async Task<IActionResult> GetMessagesByChatId(string chatId)
        {
            try
            {
                var serviceResponse = await _messageService.GetMessagesByChatIdAsync(chatId);

                if (!serviceResponse.Success)
                {
                    return BadRequest(serviceResponse);
                }

                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                // Optional: log ex here using a logger
                return StatusCode(500, ex);
            }
        }


        // GET: api/Message/user/{userId}
        //[Authorize(Policy = "InternalOnly")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetMessagesByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            var messages = await _messageService.GetMessagesByUserIdAsync(userId);

            return Ok(MappingToDtos.MapListOfMessagesToDto(messages));
        }

        // GET: api/Message/unread/{userId}
        [HttpGet("unread/{userId}")]
        public async Task<IActionResult> GetUnreadMessagesByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            var unreadMessages = await _messageService.GetUnreadMessagesByUserIdAsync(userId);
            return Ok(MappingToDtos.MapListOfMessagesToDto(unreadMessages));
        }
        #endregion

        // POST: api/Message/send
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto messageRequestDto)
        {
            if (messageRequestDto == null)
            {
                return BadRequest("Message cannot be null.");
            }

            // Save the message and update the chat
            ServiceResponse<MessageDto> response = await _messageService.SendMessageAsync(messageRequestDto);

            if(!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // PUT: api/Message/{messageId}/read
        [HttpPut("{messageId}/read")]
        public async Task<IActionResult> MarkMessageAsRead(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return BadRequest("Message ID cannot be null or empty.");
            }

            await _messageService.MarkMessageAsReadAsync(messageId);
            return NoContent();
        }

        // PUT: api/Message/{chatId}/readall/{userId}
        [HttpPut("{chatId}/readall/{userId}")]
        public async Task<IActionResult> MarkChatMessagesAsRead(string chatId, string userId)
        {
            if (string.IsNullOrEmpty(chatId) || string.IsNullOrEmpty(userId))
            {
                return BadRequest("Chat ID and User ID cannot be null or empty.");
            }

            await _messageService.MarkChatMessagesAsReadAsync(chatId, userId);
            return NoContent();
        }

        // // PUT: api/Message
        //[HttpPut]
        //public async Task<IActionResult> UpdateMessage([FromBody] MessageDto message)
        //{
            
        //}

        // DELETE: api/Message/{messageId}
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return BadRequest("Message ID cannot be null or empty.");
            }

            await _messageService.DeleteMessageAsync(messageId);
            return NoContent();
        }


    }
}
