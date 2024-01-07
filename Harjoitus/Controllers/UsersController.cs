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
    public class UsersController : ControllerBase
    {
        
        private readonly IUserService _userService;
        private readonly IUserAuthenticationservice _authService;


        public UsersController(IUserService service, IUserAuthenticationservice authService)
        {
            _userService = service;
            _authService = authService;
        }

        // GET: api/Users
        /// <summary>
        /// Search users
        /// </summary>
        /// <returns>List of users</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            return Ok(await _userService.GetUsersAsync());
        }

        // GET: api/Users/5
        /// <summary>
        /// Search specific user
        /// </summary>
        /// <returns>user</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(long id) 
        {
            UserDTO dto = await _userService.GetUserAsync(id);

            if (dto == null)  
            {
                return NotFound();
            }
            return Ok(dto);

        }

        // PUT: api/Users/5
        /// <summary>
        /// Edit a user
        /// </summary>
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUser(long id, User user)
        {

            if (id != user.Id)
            {
                return BadRequest();
            }

            if (await _userService.UpdateUserAsync(user))
            {
                return NoContent();
            }
            return NotFound();
        }

        // POST: api/Users
        /// <summary>
        /// Register user
        /// </summary>
        /// <returns>User created</returns>
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserDTO>> PostUser(User user)
        {
            UserDTO? newUser = await _userService.NewUserAsync(user);

            if (newUser == null)
            {
                return Problem("Username not available. Choose different username");
            }

            return CreatedAtAction(nameof(GetUser), new { id = newUser.UserName}, newUser);
    
        }

        // DELETE: api/Users/5 
        /// <summary>
        /// Delete user
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(long id)
        {
            if (await _authService.isMyAccount(this.User.FindFirst(ClaimTypes.Name).Value, id))
            {
                await _userService.DeleteUserAsync(id);
            
                return NoContent();
            }

            return NotFound();
     
        }

    }
}
