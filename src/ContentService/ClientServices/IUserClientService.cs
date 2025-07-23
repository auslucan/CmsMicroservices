using ContentService.DTOs;

namespace ContentService.ClientServices
{
    public interface IUserClientService
    {
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task UpdateUserLastContentAsync(Guid userId, UserUpdateDto dto);
    }
}
