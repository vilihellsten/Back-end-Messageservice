using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Harjoitus.Models;
using Harjoitus.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Harjoitus.Middleware;

namespace Harjoitus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        // TODO: Remove context from controller, kontroller tuntee vain servicen functiont
        private readonly IMessageService _messageService;
        private readonly IUserAuthenticationservice _authService;


        public MessagesController(IMessageService service, IUserAuthenticationservice authService)
        {
            _messageService = service;
            _authService = authService;
        }

        // GET: api/Messages, alapuolella XML dokkari kommentteja, tosin en ole varma pitäisikö saada kaikkia viestejä jne, 4.12 vil // <param name="id"></param> jos olisi parametreja käytettäisiin tätä summaryn ja returns välissä
        /// <summary>
        /// Get all Messages database
        /// </summary>
        /// <returns>json with an array of MessageDTOs</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessages()
        {
            return Ok(await _messageService.GetMessagesAsync());
        }

        //GET : api/messages/search/{searchtext}
        /// <summary>
        /// Search for a public messages
        /// </summary>
        /// <returns>messages</returns>
        [HttpGet("search/{searchtext}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> SearchMessages(string searchtext) 
        {
            return Ok(await _messageService.SearchMessagesAsync(searchtext));
        }

        // GET: api/Messages/sent/username
        /// <summary>
        /// Messages sent by user
        /// </summary>
        /// <returns>messages by user</returns>
        [HttpGet("sent/{username}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMySentMessages(string username)
        {
            if (this.User.FindFirst(ClaimTypes.Name).Value == username)
            {

                IEnumerable<MessageDTO?> list = await _messageService.GetSentMessagesAsync(username);
                if (list == null)
                {
                    return BadRequest();
                }
                return Ok(list);
            }
            return BadRequest();
        }

        // GET: api/Messages/received/username
        /// <summary>
        /// User received messages
        /// </summary>
        /// <returns>messages to user</returns>
        [HttpGet("received/{username}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMyReceivedMessages(string username)
        {
            if (this.User.FindFirst(ClaimTypes.Name).Value == username)
            {

                IEnumerable<MessageDTO?> list = await _messageService.GetReceivedMessagesAsync(username);
                if (list == null)
                {
                    return BadRequest();
                }
                return Ok(list);
            }
            return BadRequest();
        }

        // GET: api/Messages/5
        /// <summary>
        /// Get a single message from database
        /// </summary>
        /// <param name="id">id of message</param>
        /// <returns>json with one messageDTO</returns>
        /// <response code="200">Returns a message</response>
        /// <response code="404">If message doesn't exist</response>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<MessageDTO>> GetMessage(long id)
        {

            MessageDTO message = await _messageService.GetMessageAsync(id);

            if (message == null)
            {
                return NotFound();
            }
            return message;

        }

        // PUT: api/Messages/5
        /// <summary>
        /// Edit a single message
        /// </summary>
        /// <param name="id">id of message</param>
        /// <param name="message">Updated message info</param>
        /// <response code="204">Success</response>
        /// <response code="400">If requrest is faulty</response>
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutMessage(long id, MessageDTO message)
        {

            if (this.User.FindFirst(ClaimTypes.Name).Value == message.Sender)
            {
                if (await _authService.isMyMessage(this.User.FindFirst(ClaimTypes.Name).Value, id))
                {

                    if (id != message.Id)
                    {
                        return BadRequest();
                    }

                    bool result = await _messageService.UpdateMessageAsync(message);

                    if (!result)
                    {
                        return NotFound(message);
                    }
                    return NoContent();
                }
                return BadRequest();
            }
            return BadRequest();

        }


        // POST: api/Messages
        /// <summary>
        /// Post a message
        /// </summary>
        /// <returns>message created</returns>
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<MessageDTO>> PostMessage(MessageDTO message)
        {
            if (this.User.FindFirst(ClaimTypes.Name).Value == message.Sender)
            {

                MessageDTO newMessage = await _messageService.NewMessageAsync(message); 

                if (newMessage == null)
                {
                    return Problem("Recipient not found");
                }

               
                return CreatedAtAction(nameof(GetMessage), new { id = newMessage.Id }, newMessage);
            }
            return BadRequest();
        }

        // DELETE: api/Messages/5 etsitään messagea jolla on tämä id
        /// <summary>
        /// Deletes user message
        /// </summary>
        [HttpDelete("{id}")] 
        [Authorize]
        public async Task<IActionResult> DeleteMessage(long id)
        {

            if(await _authService.isMyMessage(this.User.FindFirst(ClaimTypes.Name).Value, id)) // tarkistetaan että käyttäjällä on oikeus poistaa oma viesti, ei muita
            {

                if (await _messageService.DeleteMessageAsync(id))
                {
                    return Ok();
                }
                return NotFound();
            }
            return BadRequest();

        }

    }
}
