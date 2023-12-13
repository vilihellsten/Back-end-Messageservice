﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Harjoitus.Models;
using Harjoitus.Services;

namespace Harjoitus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        
        private readonly IUserService _userService;

        //TODO REMOVE _context from controller
        public UsersController(IUserService service)
        {
            _userService = service;
        }

        // GET: api/Users
        [HttpGet]
        // [Authorize]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            return Ok(await _userService.GetUsersAsync());
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(long id) // puhui jotain tähän liittyen, tarkista, 4.12 1.26.40 ish
        {
            UserDTO dto = await _userService.GetUserAsync(id);

            if (dto == null)  //1.28.00 4.12
            {
                return NotFound();
            }
            return Ok(dto);

        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserDTO>> PostUser(User user)
        {
            UserDTO? newUser = await _userService.NewUserAsync(user);

            if (newUser == null)// 1.29.40 4,12
            {
                return Problem("Username not available. Choose different username");
            }

            return CreatedAtAction(nameof(GetUser), new { id = newUser.UserName}, newUser);
    
        }

        // DELETE: api/Users/5 etsitään messagea jolla on tämä id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {

            if (await _userService.DeleteUserAsync(id))
            {
                return NoContent();
            }
            return NotFound();
     
        }

    }
}
