using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllUsersAsync());

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _service.GetUserByIdAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            var createdUser = await _service.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] User updatedUser)
        {
            var result = await _service.UpdateUserAsync(id, updatedUser);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteUserAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        [HttpPatch("{id}/lastcontentupdated")]
        public async Task<IActionResult> UpdateLastContentUpdated(Guid id, [FromBody] UpdateUserDto dto)
        {
            var user = await _service.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            user.LastContentUpdated = dto.LastContentUpdated;

            var success = await _service.UpdateUserAsync(id, user);

            if (!success) return StatusCode(500, "Failed to update user");

            return NoContent();
        }
    }
}
