using ContentService.DTOs;
using ContentService.Models;
using ContentService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Controllers
{
    [ApiController]
    [Route("contents")]
    public class ContentsController : ControllerBase
    {
        private readonly IContentService _service;

        public ContentsController(IContentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllContentsAsync());

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var content = await _service.GetContentByIdAsync(id);
            return content == null ? NotFound() : Ok(content);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Content content)
        {
            var created = await _service.CreateContentAsync(content);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Content updated)
        {
            var result = await _service.UpdateContentAsync(id, updated);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteContentAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        [HttpPatch("{contentId}/update-and-notify-user/{userId}")]
        public async Task<IActionResult> UpdateContentAndNotifyUser(Guid contentId, Guid userId, [FromBody] ContentUpdateDto contentDto)
        {
            try
            {
                await _service.UpdateContentAndUserAsync(contentId, contentDto, userId);
                return Ok(new { Message = "Content and user updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating content and user.", Details = ex.Message });
            }
        }
    }
}
