using ContentService.DTOs;
using ContentService.Models;

namespace ContentService.Services
{
    public interface IContentService
    {
        Task<IEnumerable<Content>> GetAllContentsAsync();
        Task<Content?> GetContentByIdAsync(Guid id);
        Task<Content> CreateContentAsync(Content content);
        Task<bool> UpdateContentAsync(Guid id, Content updatedContent);
        Task<bool> DeleteContentAsync(Guid id);
        Task UpdateContentAndUserAsync(Guid contentId, ContentUpdateDto contentDto, Guid userId);
    }
}
