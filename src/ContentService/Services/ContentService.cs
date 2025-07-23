using ContentService.ClientServices;
using ContentService.Data;
using ContentService.DTOs;
using ContentService.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ContentService.Services
{
    public class ContentService : IContentService
    {
        private readonly AppDbContext _context;
        private readonly IUserClientService _userClientService;

        public ContentService(AppDbContext context, IUserClientService userClientService)
        {
            _context = context;
            _userClientService = userClientService;
        }


        public async Task<IEnumerable<Content>> GetAllContentsAsync()
        {
            return await _context.Contents.AsNoTracking().ToListAsync();
        }

        public async Task<Content?> GetContentByIdAsync(Guid id)
        {
            return await _context.Contents.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Content> CreateContentAsync(Content content)
        {
            content.Id = Guid.NewGuid();
            content.CreatedAt = DateTime.UtcNow;
            _context.Contents.Add(content);
            await _context.SaveChangesAsync();
            return content;
        }

        public async Task<bool> UpdateContentAsync(Guid id, Content updatedContent)
        {
            var existing = await _context.Contents.FindAsync(id);
            if (existing == null) {
                var errorMessage = $"Content with id {id} not found.";
                Log.Error(errorMessage);
                return false; }

            existing.Title = updatedContent.Title;
            existing.Body = updatedContent.Body;
            existing.CreatedByUserId = updatedContent.CreatedByUserId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteContentAsync(Guid id)
        {
            var existing = await _context.Contents.FindAsync(id);
            if (existing == null)
            {
                var errorMessage = $"Content with id {id} not found.";
                Log.Error(errorMessage);
                return false;
            }

            _context.Contents.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }




        public async Task UpdateContentAndUserAsync(Guid contentId, ContentUpdateDto contentDto, Guid userId)
        {
            try
            {

                var content = await _context.Contents.FindAsync(contentId);
                if (content == null)
                {
                    var errorMessage = $"Content with id {contentId} not found.";
                    Log.Error(errorMessage);
                    throw new Exception(errorMessage);
                }

                content.Title = contentDto.Title;
                content.Body = contentDto.Body;

                await _context.SaveChangesAsync();
                Log.Information("Content Updated: {ContentId}", contentId);

                await _userClientService.UpdateUserLastContentAsync(userId, new UserUpdateDto
                {
                    LastContentUpdated = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UpdateContentAndUserAsync failed");
                throw;
            }
        }
    }
}
